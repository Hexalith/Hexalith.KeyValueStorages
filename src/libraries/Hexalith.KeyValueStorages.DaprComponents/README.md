# Hexalith.KeyValueStorages.DaprComponents

## Overview

Hexalith.KeyValueStorages.DaprComponents provides a distributed key-value storage implementation using [Dapr Actors](https://docs.dapr.io/developing-applications/building-blocks/actors/). It leverages Dapr's actor model to ensure single-threaded access per key, providing strong consistency guarantees in distributed environments.

## Features

- **Distributed Storage**: State replicated across Dapr sidecars
- **Single-Threaded Access**: Actor model ensures one operation per key at a time
- **Automatic Concurrency**: No manual locking required
- **Scalable**: Actors distributed across cluster nodes
- **Persistent State**: Backed by configurable Dapr state stores
- **ETag Support**: Optimistic concurrency control

## Prerequisites

- .NET 8.0 or later
- [Dapr runtime](https://docs.dapr.io/getting-started/) installed and configured
- Dapr state store component configured

## Installation

```bash
dotnet add package Hexalith.KeyValueStorages.DaprComponents
```

## Quick Start

### 1. Define Your State

```csharp
using Hexalith.KeyValueStorages.Abstractions;

public record CartState : StateBase
{
    public static string Name => "Cart";

    public string UserId { get; init; } = string.Empty;
    public List<CartItem> Items { get; init; } = [];
    public decimal Total { get; init; }
}

public record CartItem
{
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
```

### 2. Configure ASP.NET Core Application

```csharp
using Hexalith.KeyValueStorages.DaprComponents;

var builder = WebApplication.CreateBuilder(args);

// Add Dapr actor services
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<KeyValueStoreActor<CartState>>();
});

// Register the Dapr key-value store
builder.Services.AddSingleton<IKeyValueStore<string, CartState>>(sp =>
{
    return new DaprActorKeyValueStore<string, CartState>(
        keyToActorId: key => $"cart-{key}");
});

var app = builder.Build();

// Configure Dapr actors endpoint
app.UseRouting();
app.MapActorsHandlers();

app.Run();
```

### 3. Use the Store

```csharp
public class CartService
{
    private readonly IKeyValueStore<string, CartState> _store;

    public CartService(IKeyValueStore<string, CartState> store)
    {
        _store = store;
    }

    public async Task<CartState> GetOrCreateCartAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var cart = await _store.TryGetAsync(userId, cancellationToken);

        if (cart == null)
        {
            cart = new CartState { UserId = userId };
            await _store.AddAsync(userId, cart, cancellationToken);
        }

        return cart;
    }

    public async Task AddItemAsync(
        string userId,
        CartItem item,
        CancellationToken cancellationToken)
    {
        var cart = await _store.GetAsync(userId, cancellationToken);

        var updated = cart with
        {
            Items = [.. cart.Items, item],
            Total = cart.Total + (item.Price * item.Quantity)
        };

        await _store.SetAsync(userId, updated, cancellationToken);
    }
}
```

## How It Works

### Actor Per Key

Each key maps to a dedicated actor instance:

```
Key "user-123" → Actor "cart-user-123"
Key "user-456" → Actor "cart-user-456"
```

### Single-Threaded Execution

Dapr actors process one request at a time per actor, eliminating race conditions:

```
Request 1 (user-123) ─┬─→ Actor "cart-user-123" → Sequential execution
Request 2 (user-123) ─┘

Request 3 (user-456) ───→ Actor "cart-user-456" → Parallel with above
```

### State Persistence

Actor state is persisted to the configured Dapr state store (Redis, CosmosDB, etc.):

```yaml
# components/statestore.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: localhost:6379
```

## Configuration

### Actor Registration

```csharp
builder.Services.AddActors(options =>
{
    // Register the actor type
    options.Actors.RegisterActor<KeyValueStoreActor<CartState>>();

    // Configure actor options
    options.ActorIdleTimeout = TimeSpan.FromMinutes(10);
    options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(30);
    options.DrainRebalancedActors = true;
});
```

### Custom Actor ID Generation

```csharp
// Simple prefix
var store = new DaprActorKeyValueStore<string, CartState>(
    keyToActorId: key => $"cart-{key}");

// Composite key
var store = new DaprActorKeyValueStore<(string TenantId, string UserId), CartState>(
    keyToActorId: key => $"{key.TenantId}-{key.UserId}");

// Hashed for long keys
var store = new DaprActorKeyValueStore<string, CartState>(
    keyToActorId: key => Convert.ToBase64String(
        SHA256.HashData(Encoding.UTF8.GetBytes(key))));
```

## DaprActorKeyValueStore API

### Constructor

```csharp
public DaprActorKeyValueStore(
    Func<TKey, string> keyToActorId,
    string? actorType = null,
    KeyValueStorageSettings? settings = null)
```

| Parameter | Description |
|-----------|-------------|
| `keyToActorId` | Function to convert key to actor ID |
| `actorType` | Actor type name (defaults to `KeyValueStoreActor_{StateName}`) |
| `settings` | Optional settings for database/container |

### Methods

Same interface as other implementations:

| Method | Description |
|--------|-------------|
| `AddAsync` | Add new value via actor |
| `AddOrUpdateAsync` | Add or update via actor |
| `SetAsync` | Update existing value |
| `GetAsync` | Get value from actor state |
| `TryGetAsync` | Try to get value |
| `ContainsKeyAsync` | Check if key exists |
| `RemoveAsync` | Remove value from actor |

## Advanced Usage

### Multiple State Types

```csharp
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<KeyValueStoreActor<CartState>>();
    options.Actors.RegisterActor<KeyValueStoreActor<SessionState>>();
    options.Actors.RegisterActor<KeyValueStoreActor<PreferenceState>>();
});

builder.Services.AddSingleton<IKeyValueStore<string, CartState>>(sp =>
    new DaprActorKeyValueStore<string, CartState>(key => $"cart-{key}"));

builder.Services.AddSingleton<IKeyValueStore<string, SessionState>>(sp =>
    new DaprActorKeyValueStore<string, SessionState>(key => $"session-{key}"));
```

### With Dependency Injection Provider

```csharp
builder.Services.AddSingleton<IKeyValueProvider, DaprActorKeyValueProvider>();

// Usage
public class MyService
{
    private readonly IKeyValueStore<string, CartState> _cartStore;
    private readonly IKeyValueStore<string, SessionState> _sessionStore;

    public MyService(IKeyValueProvider provider)
    {
        _cartStore = provider.Create<string, CartState>("app", "carts", "cart");
        _sessionStore = provider.Create<string, SessionState>("app", "sessions", "session");
    }
}
```

### Custom Actor Implementation

```csharp
public class CustomKeyValueActor : KeyValueStoreActor<CartState>
{
    private readonly ILogger<CustomKeyValueActor> _logger;

    public CustomKeyValueActor(
        ActorHost host,
        ILogger<CustomKeyValueActor> logger)
        : base(host)
    {
        _logger = logger;
    }

    public override async Task<string> AddAsync(CartState value)
    {
        _logger.LogInformation("Adding cart for {ActorId}", Id);
        return await base.AddAsync(value);
    }
}
```

## Error Handling

```csharp
try
{
    await store.SetAsync(userId, cart, cancellationToken);
}
catch (ConcurrencyException<string> ex)
{
    // Another request modified the cart
    _logger.LogWarning("Concurrency conflict on cart {Key}", ex.Key);

    // Reload and retry
    var current = await store.GetAsync(userId, cancellationToken);
    // Merge changes and retry...
}
catch (ActorInvokeException ex)
{
    // Dapr actor invocation failed
    _logger.LogError(ex, "Actor invocation failed");
    throw;
}
```

## Best Practices

1. **Keep Actor State Small**: Large state increases serialization overhead
2. **Use Meaningful Actor IDs**: Include context for debugging
3. **Handle Actor Timeouts**: Configure appropriate timeouts for your workload
4. **Monitor Actor Placement**: Use Dapr dashboard for actor distribution
5. **Plan for State Migration**: Consider versioning for state schema changes

## Performance Considerations

- **Actor Activation**: First request to an actor incurs activation cost
- **State Serialization**: JSON serialization for each state access
- **Network Latency**: Actor calls go through Dapr sidecar
- **Idle Timeout**: Configure based on access patterns

## Dapr Configuration

### State Store Component

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
    - name: redisHost
      value: redis:6379
    - name: actorStateStore
      value: "true"
```

### Actor Runtime Configuration

```yaml
apiVersion: dapr.io/v1alpha1
kind: Configuration
metadata:
  name: daprConfig
spec:
  features:
    - name: Actor.TypeMetadata
      enabled: true
  actor:
    drainOngoingCallTimeout: 30s
    drainRebalancedActors: true
    idleTimeout: 1h
    reentrancy:
      enabled: false
```

## Related Packages

- [Hexalith.KeyValueStorages.Abstractions](../Hexalith.KeyValueStorages.Abstractions/README.md) - Core interfaces
- [Hexalith.KeyValueStorages](../Hexalith.KeyValueStorages/README.md) - In-memory implementation
- [Hexalith.KeyValueStorages.Files](../Hexalith.KeyValueStorages.Files/README.md) - File-based storage

## License

This project is licensed under the MIT License.