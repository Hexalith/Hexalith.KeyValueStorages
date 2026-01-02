# Hexalith Key/Value Storages

## Overview

Hexalith Key/Value Storages is a flexible and extensible key-value storage library for .NET applications. This package provides the core in-memory implementation with support for optimistic concurrency control through ETags and automatic value expiration via TTL.

## Features

- **In-Memory Storage**: Fast, thread-safe storage for temporary data
- **Optimistic Concurrency**: ETag-based conflict detection
- **Time-to-Live (TTL)**: Automatic expiration of stored values
- **Generic Type Support**: Works with any key and value types
- **Async/Await**: Fully asynchronous API
- **Testable**: Supports custom `TimeProvider` for deterministic testing

## Installation

```bash
dotnet add package Hexalith.KeyValueStorages
```

## Quick Start

### Define Your State

```csharp
using Hexalith.KeyValueStorages.Abstractions;

public record SessionState : StateBase
{
    public static string Name => "Session";

    public string UserId { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset LastActivity { get; init; }
}
```

### Create and Use a Store

```csharp
using Hexalith.KeyValueStorages;

// Create an in-memory store
var store = new InMemoryKeyValueStore<string, SessionState>();

// Add a new session
var session = new SessionState
{
    UserId = "user-123",
    Token = "abc123",
    LastActivity = DateTimeOffset.UtcNow,
    TimeToLive = TimeSpan.FromHours(1) // Session expires in 1 hour
};

string etag = await store.AddAsync("session-456", session, cancellationToken);
Console.WriteLine($"Created session with ETag: {etag}");

// Retrieve the session
SessionState? retrieved = await store.TryGetAsync("session-456", cancellationToken);
if (retrieved != null)
{
    Console.WriteLine($"User: {retrieved.UserId}");
}

// Update the session (extends activity time)
var updated = retrieved! with { LastActivity = DateTimeOffset.UtcNow };
string newEtag = await store.SetAsync("session-456", updated, cancellationToken);

// Remove the session
await store.RemoveAsync("session-456", newEtag, cancellationToken);
```

## InMemoryKeyValueStore API

### Constructor

```csharp
// Default constructor
var store = new InMemoryKeyValueStore<TKey, TState>();

// With custom TimeProvider (useful for testing)
var store = new InMemoryKeyValueStore<TKey, TState>(timeProvider);

// With full settings
var store = new InMemoryKeyValueStore<TKey, TState>(
    settings: new KeyValueStorageSettings(),
    database: "mydb",
    container: "mycontainer",
    timeProvider: TimeProvider.System);
```

### Methods

| Method | Description |
|--------|-------------|
| `AddAsync` | Add a new key-value pair. Throws `DuplicateKeyException` if key exists. |
| `AddOrUpdateAsync` | Add or update a key-value pair (idempotent). |
| `SetAsync` | Update an existing value. Validates ETag for concurrency. |
| `GetAsync` | Get a value. Throws `KeyNotFoundException` if not found. |
| `TryGetAsync` | Try to get a value. Returns `null` if not found. |
| `ContainsKeyAsync` | Check if a key exists. |
| `ExistsAsync` | Alias for `ContainsKeyAsync`. |
| `RemoveAsync` | Remove a value. Validates ETag for concurrency. |

### Static Methods

| Method | Description |
|--------|-------------|
| `Clear()` | Clear all entries for a specific database/container. |
| `TimeToLiveCleanup()` | Remove all expired entries. |

## Concurrency Control

The store uses optimistic concurrency with ETags:

```csharp
// Two concurrent updates
var state = await store.GetAsync("key", cancellationToken);

// First update succeeds
var update1 = state with { Value = "new value 1" };
await store.SetAsync("key", update1, cancellationToken);

// Second update fails - ETag has changed
var update2 = state with { Value = "new value 2" };
try
{
    await store.SetAsync("key", update2, cancellationToken);
}
catch (ConcurrencyException<string> ex)
{
    Console.WriteLine($"Conflict on key: {ex.Key}");
    // Retry with fresh data
}
```

## Time-to-Live (TTL)

Values can automatically expire:

```csharp
// Create a value with TTL
var cacheEntry = new CacheState
{
    Data = expensiveComputation,
    TimeToLive = TimeSpan.FromMinutes(5)
};
await store.AddAsync("cache-key", cacheEntry, cancellationToken);

// After 5 minutes, TryGetAsync returns null
// and the entry is automatically cleaned up
```

### Manual Cleanup

```csharp
// Remove all expired entries across all stores
InMemoryKeyValueStore<string, SessionState>.TimeToLiveCleanup();
```

## Testing with FakeTimeProvider

```csharp
using Microsoft.Extensions.Time.Testing;

[Fact]
public async Task ValueExpiresAfterTtl()
{
    var fakeTime = new FakeTimeProvider();
    var store = new InMemoryKeyValueStore<string, CacheState>(fakeTime);

    var entry = new CacheState
    {
        Data = "cached data",
        TimeToLive = TimeSpan.FromMinutes(10)
    };
    await store.AddAsync("key", entry, default);

    // Value exists before expiration
    var result = await store.TryGetAsync("key", default);
    Assert.NotNull(result);

    // Advance time past TTL
    fakeTime.Advance(TimeSpan.FromMinutes(11));

    // Value is now expired
    result = await store.TryGetAsync("key", default);
    Assert.Null(result);
}
```

## Dependency Injection

### Registration

```csharp
// Register as singleton
services.AddSingleton<IKeyValueStore<string, SessionState>,
    InMemoryKeyValueStore<string, SessionState>>();

// Or use the provider pattern
services.AddSingleton<IKeyValueProvider, InMemoryKeyValueProvider>();
```

### Usage

```csharp
public class SessionService
{
    private readonly IKeyValueStore<string, SessionState> _store;

    public SessionService(IKeyValueStore<string, SessionState> store)
    {
        _store = store;
    }

    public async Task<SessionState> CreateSessionAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new SessionState
        {
            UserId = userId,
            Token = GenerateToken(),
            LastActivity = DateTimeOffset.UtcNow,
            TimeToLive = TimeSpan.FromHours(24)
        };

        await _store.AddAsync(sessionId, session, cancellationToken);
        return session;
    }
}
```

## Thread Safety

The `InMemoryKeyValueStore` uses a static `Lock` object for thread safety across all instances. This ensures:

- Atomic read/write operations
- Safe concurrent access from multiple threads
- Consistent ETag validation

## Best Practices

1. **Use TTL for Cache Data**: Set appropriate TTL values to prevent memory growth
2. **Handle Concurrency Exceptions**: Implement retry logic for concurrent updates
3. **Use TryGetAsync**: Prefer `TryGetAsync` over `GetAsync` when the key might not exist
4. **Periodic Cleanup**: Call `TimeToLiveCleanup()` periodically in long-running applications
5. **Inject TimeProvider**: Use `FakeTimeProvider` in tests for deterministic behavior

## Related Packages

- [Hexalith.KeyValueStorages.Abstractions](../Hexalith.KeyValueStorages.Abstractions/README.md) - Core interfaces
- [Hexalith.KeyValueStorages.Files](../Hexalith.KeyValueStorages.Files/README.md) - File-based persistence
- [Hexalith.KeyValueStorages.DaprComponents](../Hexalith.KeyValueStorages.DaprComponents/README.md) - Distributed storage

## License

This project is licensed under the MIT License.