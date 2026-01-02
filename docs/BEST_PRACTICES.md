# Best Practices and Advanced Patterns

This guide covers best practices, design patterns, and advanced usage scenarios for Hexalith.KeyValueStorages.

## Table of Contents

- [State Design](#state-design)
- [Concurrency Patterns](#concurrency-patterns)
- [Performance Optimization](#performance-optimization)
- [Testing Strategies](#testing-strategies)
- [Error Handling](#error-handling)
- [Multi-Tenancy](#multi-tenancy)
- [Migration Strategies](#migration-strategies)

## State Design

### Keep States Small and Focused

Design your state classes to contain only the data that needs to be stored together:

```csharp
// Good: Focused state with related data
public record UserProfileState : StateBase
{
    public static string Name => "UserProfile";

    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
}

// Good: Separate state for different access patterns
public record UserPreferencesState : StateBase
{
    public static string Name => "UserPreferences";

    public string Theme { get; init; } = "light";
    public string Language { get; init; } = "en";
    public bool NotificationsEnabled { get; init; } = true;
}

// Avoid: Large monolithic state
public record UserEverythingState : StateBase  // Don't do this
{
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<Order> Orders { get; init; } = [];  // Too much data
    public List<Notification> Notifications { get; init; } = [];
    // ... many more fields
}
```

### Use Immutable Records

Leverage C# records for immutable state with easy updates:

```csharp
public record OrderState : StateBase
{
    public static string Name => "Order";

    public string OrderId { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public List<OrderItem> Items { get; init; } = [];
    public decimal Total { get; init; }
}

// Update using 'with' expression
var updated = order with
{
    Status = OrderStatus.Shipped,
    // Etag is preserved for concurrency check
};
```

### Implement Static Name Property

Always implement the `Name` property for proper identification:

```csharp
public record ProductState : StateBase
{
    // Used for entity naming in storage paths
    public static string Name => "Product";

    public string Sku { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}
```

## Concurrency Patterns

### Optimistic Retry Pattern

Handle concurrency conflicts with automatic retry:

```csharp
public class OptimisticUpdateService<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    private readonly IKeyValueStore<TKey, TState> _store;
    private readonly int _maxRetries;

    public OptimisticUpdateService(
        IKeyValueStore<TKey, TState> store,
        int maxRetries = 3)
    {
        _store = store;
        _maxRetries = maxRetries;
    }

    public async Task<TState> UpdateAsync(
        TKey key,
        Func<TState, TState> updateFunc,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                var current = await _store.GetAsync(key, cancellationToken);
                var updated = updateFunc(current);
                await _store.SetAsync(key, updated, cancellationToken);
                return updated;
            }
            catch (ConcurrencyException<TKey>)
            {
                if (attempt == _maxRetries - 1)
                    throw;

                // Exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt)));
            }
        }

        throw new InvalidOperationException("Should not reach here");
    }
}
```

### Read-Modify-Write Pattern

Safely update state with a single operation:

```csharp
public async Task IncrementCounterAsync(
    string key,
    CancellationToken cancellationToken)
{
    await _updateService.UpdateAsync(
        key,
        state => state with { Count = state.Count + 1 },
        cancellationToken);
}
```

### Compare-and-Swap Pattern

Only update if a condition is met:

```csharp
public async Task<bool> TryClaimResourceAsync(
    string resourceId,
    string claimerId,
    CancellationToken cancellationToken)
{
    try
    {
        var resource = await _store.GetAsync(resourceId, cancellationToken);

        if (resource.ClaimedBy != null)
            return false;

        var claimed = resource with { ClaimedBy = claimerId };
        await _store.SetAsync(resourceId, claimed, cancellationToken);
        return true;
    }
    catch (ConcurrencyException<string>)
    {
        // Another process claimed it first
        return false;
    }
}
```

## Performance Optimization

### Batch Operations

Process multiple items efficiently:

```csharp
public async Task<IReadOnlyList<TState>> GetManyAsync<TKey, TState>(
    IKeyValueStore<TKey, TState> store,
    IEnumerable<TKey> keys,
    CancellationToken cancellationToken)
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    var tasks = keys.Select(key =>
        store.TryGetAsync(key, cancellationToken));

    var results = await Task.WhenAll(tasks);
    return results.Where(r => r != null).ToList()!;
}
```

### Caching Layer

Add an in-memory cache for frequently accessed data:

```csharp
public class CachedKeyValueStore<TKey, TState> : IKeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    private readonly IKeyValueStore<TKey, TState> _innerStore;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    public CachedKeyValueStore(
        IKeyValueStore<TKey, TState> innerStore,
        IMemoryCache cache,
        TimeSpan cacheDuration)
    {
        _innerStore = innerStore;
        _cache = cache;
        _cacheDuration = cacheDuration;
    }

    public async Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken)
    {
        var cacheKey = $"{typeof(TState).Name}:{key}";

        if (_cache.TryGetValue(cacheKey, out TState? cached))
            return cached;

        var value = await _innerStore.TryGetAsync(key, cancellationToken);

        if (value != null)
        {
            _cache.Set(cacheKey, value, _cacheDuration);
        }

        return value;
    }

    public async Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        var etag = await _innerStore.SetAsync(key, value, cancellationToken);

        var cacheKey = $"{typeof(TState).Name}:{key}";
        _cache.Set(cacheKey, value with { Etag = etag }, _cacheDuration);

        return etag;
    }

    // ... implement other methods similarly
}
```

### Use TTL for Temporary Data

Set appropriate TTL values to prevent unbounded growth:

```csharp
// Session data with automatic expiration
var session = new SessionState
{
    UserId = userId,
    Token = token,
    TimeToLive = TimeSpan.FromHours(24)
};

// Cache entries with short TTL
var cacheEntry = new CacheState
{
    Data = computedResult,
    TimeToLive = TimeSpan.FromMinutes(5)
};
```

## Testing Strategies

### Use FakeTimeProvider for TTL Testing

```csharp
public class ExpirationTests
{
    [Fact]
    public async Task Expired_Items_Are_Not_Returned()
    {
        // Arrange
        var fakeTime = new FakeTimeProvider();
        var store = new InMemoryKeyValueStore<string, TestState>(fakeTime);

        var state = new TestState { TimeToLive = TimeSpan.FromMinutes(30) };
        await store.AddAsync("key", state, default);

        // Act - advance time past TTL
        fakeTime.Advance(TimeSpan.FromMinutes(31));

        // Assert
        var result = await store.TryGetAsync("key", default);
        result.Should().BeNull();
    }
}
```

### Create Test Fixtures

```csharp
public class KeyValueStoreFixture<TState> : IAsyncLifetime
    where TState : class, IState
{
    public IKeyValueStore<string, TState> Store { get; private set; } = null!;
    private string _tempPath = null!;

    public async Task InitializeAsync()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);

        var settings = new KeyValueStorageSettings { StorageRootPath = _tempPath };
        Store = new JsonFileKeyValueStore<string, TState>(settings, "test", "data");

        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.Delete(_tempPath, recursive: true);
        return Task.CompletedTask;
    }
}
```

### Mock the Store Interface

```csharp
[Fact]
public async Task Service_Returns_Correct_Data()
{
    // Arrange
    var mockStore = new Mock<IKeyValueStore<string, CustomerState>>();
    var expectedCustomer = new CustomerState { Name = "John Doe" };

    mockStore
        .Setup(s => s.GetAsync("customer-123", It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedCustomer);

    var service = new CustomerService(mockStore.Object);

    // Act
    var result = await service.GetCustomerAsync("customer-123", default);

    // Assert
    result.Should().BeEquivalentTo(expectedCustomer);
}
```

## Error Handling

### Define Domain-Specific Exceptions

```csharp
public class CustomerNotFoundException : Exception
{
    public string CustomerId { get; }

    public CustomerNotFoundException(string customerId)
        : base($"Customer not found: {customerId}")
    {
        CustomerId = customerId;
    }
}

public class CustomerService
{
    public async Task<CustomerState> GetCustomerAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _store.TryGetAsync(customerId, cancellationToken);

        if (customer == null)
            throw new CustomerNotFoundException(customerId);

        return customer;
    }
}
```

### Implement Resilient Operations

```csharp
public class ResilientStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    private readonly IKeyValueStore<TKey, TState> _store;
    private readonly ILogger _logger;

    public async Task<TState?> TryGetWithFallbackAsync(
        TKey key,
        Func<TState> fallbackFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _store.TryGetAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve {Key}, using fallback", key);
            return fallbackFactory();
        }
    }
}
```

## Multi-Tenancy

### Tenant Isolation with Database/Container

```csharp
public class TenantStoreFactory
{
    private readonly IKeyValueProvider _provider;

    public TenantStoreFactory(IKeyValueProvider provider)
    {
        _provider = provider;
    }

    public IKeyValueStore<string, TState> GetStoreForTenant<TState>(string tenantId)
        where TState : class, IState
    {
        return _provider.Create<string, TState>(
            database: tenantId,  // Tenant isolation at database level
            container: TState.Name,
            entity: TState.Name.ToLowerInvariant());
    }
}
```

### Tenant Context Middleware

```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant ID is required");
            return;
        }

        tenantContext.SetTenant(tenantId);
        await _next(context);
    }
}
```

## Migration Strategies

### Versioned State

```csharp
public record CustomerStateV2 : StateBase
{
    public static string Name => "Customer";

    public int Version { get; init; } = 2;
    public string FullName { get; init; } = string.Empty;  // Renamed from Name
    public string Email { get; init; } = string.Empty;
    public Address? Address { get; init; }  // New field
}
```

### Migration Service

```csharp
public class MigrationService
{
    public async Task MigrateCustomersAsync(CancellationToken cancellationToken)
    {
        var oldStore = _provider.Create<string, CustomerStateV1>("app", "customers", "customer");
        var newStore = _provider.Create<string, CustomerStateV2>("app", "customers_v2", "customer");

        // Iterate through all keys (implementation depends on your key discovery method)
        foreach (var key in await GetAllCustomerKeysAsync(cancellationToken))
        {
            var old = await oldStore.TryGetAsync(key, cancellationToken);
            if (old == null) continue;

            var migrated = new CustomerStateV2
            {
                Version = 2,
                FullName = old.Name,
                Email = old.Email,
                Address = null  // New field defaults
            };

            await newStore.AddAsync(key, migrated, cancellationToken);
        }
    }
}
```

## Summary

Following these best practices will help you:

1. **Design better states** - Small, focused, immutable
2. **Handle concurrency** - Retry patterns, compare-and-swap
3. **Optimize performance** - Caching, batching, TTL
4. **Write better tests** - Mocking, time control, fixtures
5. **Handle errors** - Domain exceptions, resilience
6. **Support multi-tenancy** - Isolation, context
7. **Plan for migration** - Versioning, migration services
