# Quickstart: Atomic Add with ACID Support

**Feature**: 003-atomic-add-acid
**Date**: 2026-01-04

## Overview

This guide demonstrates how to use the new `AddRangeAsync` method for atomic batch add operations across different key-value store providers.

---

## Basic Usage

### Adding Multiple Items Atomically

```csharp
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.InMemory;

// Create a store
IKeyValueStore<string, ProductState> store = new InMemoryKeyValueStore<string, ProductState>();

// Prepare batch items
var products = new[]
{
    ("SKU-001", new ProductState("Widget A", 29.99m, null, null)),
    ("SKU-002", new ProductState("Widget B", 39.99m, null, null)),
    ("SKU-003", new ProductState("Widget C", 49.99m, null, TimeSpan.FromHours(24)))
};

// Add all items atomically
IReadOnlyList<string> etags = await store.AddRangeAsync(products, cancellationToken);

// etags[0] = ETag for SKU-001
// etags[1] = ETag for SKU-002
// etags[2] = ETag for SKU-003
Console.WriteLine($"Added {etags.Count} products successfully");
```

---

## Handling Errors

### Duplicate Keys in Batch

```csharp
using Hexalith.KeyValueStorages.Exceptions;

var itemsWithDuplicates = new[]
{
    ("SKU-001", new ProductState("Widget A", 29.99m, null, null)),
    ("SKU-001", new ProductState("Widget A Copy", 29.99m, null, null)), // Duplicate!
};

try
{
    await store.AddRangeAsync(itemsWithDuplicates, cancellationToken);
}
catch (BatchAddException<string> ex) when (ex.Reason == BatchAddFailureReason.DuplicateKeysInBatch)
{
    Console.WriteLine($"Duplicate keys found in batch: {string.Join(", ", ex.FailedKeys)}");
    // Output: Duplicate keys found in batch: SKU-001
}
```

### Keys Already Exist in Store

```csharp
// First, add some items
await store.AddRangeAsync(new[] { ("SKU-001", new ProductState("Existing", 10m, null, null)) }, cancellationToken);

// Try to add items that include an existing key
var newItems = new[]
{
    ("SKU-001", new ProductState("Conflict", 20m, null, null)), // Already exists!
    ("SKU-004", new ProductState("New Item", 30m, null, null)),
};

try
{
    await store.AddRangeAsync(newItems, cancellationToken);
}
catch (BatchAddException<string> ex) when (ex.Reason == BatchAddFailureReason.DuplicateKeysInStore)
{
    Console.WriteLine($"Keys already exist: {string.Join(", ", ex.FailedKeys)}");
    // Output: Keys already exist: SKU-001
    // Note: SKU-004 was NOT added (atomic rollback)
}
```

---

## Checking Provider Capabilities

```csharp
// Query provider capabilities before batch operations
if (store.Capabilities.SupportsAcidTransactions)
{
    Console.WriteLine("Provider supports full ACID transactions");
    Console.WriteLine("Safe for concurrent batch operations with isolation guarantees");
}
else
{
    Console.WriteLine("Provider uses best-effort atomicity");
    Console.WriteLine("Atomicity guaranteed within process, but not across processes");
}

// Check batch size limits
int maxBatch = store.Capabilities.MaxBatchSize ?? 1000;
Console.WriteLine($"Maximum batch size: {maxBatch}");
```

---

## Provider-Specific Examples

### Redis (ACID Transactions)

```csharp
using Hexalith.KeyValueStorages.RedisDatabase;
using StackExchange.Redis;

// Configure Redis connection
IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
var settings = Options.Create(new KeyValueStoreSettings());

// Create Redis store
var store = new RedisKeyValueStore<string, OrderState>(
    redis,
    settings,
    database: "orders",
    container: "pending");

// Verify ACID support
Debug.Assert(store.Capabilities.SupportsAcidTransactions);

// Batch add with Redis MULTI/EXEC transaction
var orders = Enumerable.Range(1, 100)
    .Select(i => ($"ORD-{i:D5}", new OrderState(i, DateTime.UtcNow, null, null)))
    .ToArray();

IReadOnlyList<string> etags = await store.AddRangeAsync(orders, cancellationToken);
Console.WriteLine($"Added {etags.Count} orders atomically via Redis transaction");
```

### InMemory (Best-Effort Atomicity)

```csharp
using Hexalith.KeyValueStorages.InMemory;

var store = new InMemoryKeyValueStore<int, SessionState>(
    database: "sessions",
    container: "active",
    timeProvider: TimeProvider.System);

// Verify best-effort atomicity
Debug.Assert(!store.Capabilities.SupportsAcidTransactions);

// Batch add with lock-based atomicity
var sessions = new[]
{
    (1001, new SessionState("user-a", DateTime.UtcNow, null, TimeSpan.FromMinutes(30))),
    (1002, new SessionState("user-b", DateTime.UtcNow, null, TimeSpan.FromMinutes(30))),
    (1003, new SessionState("user-c", DateTime.UtcNow, null, TimeSpan.FromMinutes(30))),
};

IReadOnlyList<string> etags = await store.AddRangeAsync(sessions, cancellationToken);
```

### File-Based (Compensating Transactions)

```csharp
using Hexalith.KeyValueStorages.Files;

var settings = Options.Create(new KeyValueStoreSettings
{
    StorageRootPath = "./data"
});

var store = new JsonFileKeyValueStore<string, ConfigState>(
    settings,
    database: "config",
    container: "tenants");

// File provider uses temp directory + atomic moves
var configs = new[]
{
    ("tenant-1", new ConfigState("Production", true, null, null)),
    ("tenant-2", new ConfigState("Staging", false, null, null)),
};

IReadOnlyList<string> etags = await store.AddRangeAsync(configs, cancellationToken);
```

---

## Performance Considerations

### Batch Size Recommendations

```csharp
// Recommended: Split large datasets into batches
const int BatchSize = 500; // Conservative for most providers

var allItems = GetLargeDataset(); // 10,000 items

foreach (var batch in allItems.Chunk(BatchSize))
{
    try
    {
        await store.AddRangeAsync(batch, cancellationToken);
    }
    catch (BatchAddException<string> ex)
    {
        // Handle batch-level failures
        // Caller decides whether to retry
        logger.LogError(ex, "Batch failed: {Reason}", ex.Reason);
    }
}
```

### Parallel Batches (Advanced)

```csharp
// For high-throughput scenarios with ACID providers
if (store.Capabilities.SupportsAcidTransactions)
{
    var batches = allItems.Chunk(500).ToList();

    await Parallel.ForEachAsync(batches, cancellationToken, async (batch, ct) =>
    {
        await store.AddRangeAsync(batch, ct);
    });
}
```

---

## Integration with Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddSingleton<IKeyValueProvider, InMemoryKeyValueProvider>();

// Or for Redis
services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));
services.AddSingleton<IKeyValueProvider, RedisKeyValueProvider>();

// Usage in services
public class ProductService(IKeyValueProvider provider)
{
    private readonly IKeyValueStore<string, ProductState> _store =
        provider.Create<string, ProductState>(database: "catalog", container: "products");

    public async Task ImportProductsAsync(IEnumerable<Product> products, CancellationToken ct)
    {
        var items = products
            .Select(p => (p.Sku, new ProductState(p.Name, p.Price, null, null)))
            .ToArray();

        await _store.AddRangeAsync(items, ct);
    }
}
```

---

## Common Patterns

### Idempotent Import with Pre-Check

```csharp
public async Task IdempotentImportAsync(
    IEnumerable<(string Key, ProductState Value)> items,
    CancellationToken ct)
{
    var itemList = items.ToList();

    // Filter out already-existing keys
    var newItems = new List<(string Key, ProductState Value)>();
    foreach (var item in itemList)
    {
        if (!await _store.ContainsKeyAsync(item.Key, ct))
        {
            newItems.Add(item);
        }
    }

    if (newItems.Count > 0)
    {
        await _store.AddRangeAsync(newItems, ct);
    }
}
```

### With Structured Logging

```csharp
public async Task TrackedBatchAddAsync(
    IEnumerable<(string Key, OrderState Value)> orders,
    CancellationToken ct)
{
    var correlationId = Guid.NewGuid().ToString("N");
    var orderList = orders.ToList();

    _logger.LogInformation(
        "Starting batch add. CorrelationId={CorrelationId}, ItemCount={Count}",
        correlationId,
        orderList.Count);

    try
    {
        var etags = await _store.AddRangeAsync(orderList, ct);

        _logger.LogInformation(
            "Batch add completed. CorrelationId={CorrelationId}, ETagCount={Count}",
            correlationId,
            etags.Count);
    }
    catch (BatchAddException<string> ex)
    {
        _logger.LogError(
            ex,
            "Batch add failed. CorrelationId={CorrelationId}, Reason={Reason}, FailedKeys={Keys}",
            correlationId,
            ex.Reason,
            string.Join(",", ex.FailedKeys.Take(10)));

        throw;
    }
}
```

---

## State Definition Example

```csharp
using System.Runtime.Serialization;
using Hexalith.KeyValueStorages;

/// <summary>
/// Represents a product state in the catalog.
/// </summary>
[DataContract]
public sealed record ProductState(
    [property: DataMember(Order = 3)] string Name,
    [property: DataMember(Order = 4)] decimal Price,
    [property: DataMember(Order = 2)] string? Etag,
    [property: DataMember(Order = 1)] TimeSpan? TimeToLive)
    : StateBase(Etag, TimeToLive);
```
