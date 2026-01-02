# Troubleshooting Guide

This guide helps you diagnose and resolve common issues with Hexalith.KeyValueStorages.

## Table of Contents

- [Common Exceptions](#common-exceptions)
- [In-Memory Storage Issues](#in-memory-storage-issues)
- [File Storage Issues](#file-storage-issues)
- [Dapr Storage Issues](#dapr-storage-issues)
- [Concurrency Issues](#concurrency-issues)
- [Performance Issues](#performance-issues)
- [Configuration Issues](#configuration-issues)

## Common Exceptions

### DuplicateKeyException

**Symptom**: `DuplicateKeyException<TKey>` thrown when calling `AddAsync`.

**Cause**: The key already exists in the store.

**Solution**:

```csharp
// Option 1: Use AddOrUpdateAsync for idempotent operations
await store.AddOrUpdateAsync(key, state, cancellationToken);

// Option 2: Check existence first
if (!await store.ContainsKeyAsync(key, cancellationToken))
{
    await store.AddAsync(key, state, cancellationToken);
}
else
{
    // Handle existing key
}

// Option 3: Handle the exception
try
{
    await store.AddAsync(key, state, cancellationToken);
}
catch (DuplicateKeyException<string> ex)
{
    _logger.LogWarning("Key {Key} already exists", ex.Key);
    // Retry with update or return existing
}
```

### KeyNotFoundException

**Symptom**: `KeyNotFoundException<TKey>` thrown when calling `GetAsync`.

**Cause**: The key does not exist in the store.

**Solution**:

```csharp
// Option 1: Use TryGetAsync for optional retrieval
var state = await store.TryGetAsync(key, cancellationToken);
if (state == null)
{
    // Handle missing key
}

// Option 2: Check existence first
if (await store.ContainsKeyAsync(key, cancellationToken))
{
    var state = await store.GetAsync(key, cancellationToken);
}

// Option 3: Handle the exception
try
{
    var state = await store.GetAsync(key, cancellationToken);
}
catch (KeyNotFoundException<string> ex)
{
    _logger.LogWarning("Key {Key} not found", ex.Key);
    return null;
}
```

### ConcurrencyException

**Symptom**: `ConcurrencyException<TKey>` thrown when calling `SetAsync` or `RemoveAsync`.

**Cause**: The ETag in the state does not match the current ETag in the store.

**Solution**:

```csharp
// Option 1: Reload and retry
async Task UpdateWithRetryAsync(string key, Func<MyState, MyState> updateFunc)
{
    const int maxRetries = 3;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var current = await store.GetAsync(key, cancellationToken);
            var updated = updateFunc(current);
            await store.SetAsync(key, updated, cancellationToken);
            return;
        }
        catch (ConcurrencyException<string>)
        {
            if (i == maxRetries - 1) throw;
            await Task.Delay(50 * (i + 1)); // Backoff
        }
    }
}

// Option 2: Force update (lose concurrent changes)
var state = await store.GetAsync(key, cancellationToken);
var updated = state with { Etag = null }; // Clear ETag to force
// Note: This may not work with all implementations
```

## In-Memory Storage Issues

### Data Lost After Restart

**Symptom**: All data disappears when the application restarts.

**Cause**: In-memory storage is not persistent.

**Solution**: Use file-based or Dapr storage for persistence:

```csharp
// Switch to file storage
var store = new JsonFileKeyValueStore<string, MyState>(
    settings,
    "myapp",
    "data");
```

### High Memory Usage

**Symptom**: Application memory grows over time.

**Cause**: TTL cleanup not running, or no TTL set.

**Solution**:

```csharp
// 1. Set TTL on temporary data
var state = new CacheState
{
    Data = computed,
    TimeToLive = TimeSpan.FromHours(1)
};

// 2. Run periodic cleanup
var timer = new Timer(
    _ => InMemoryKeyValueStore<string, MyState>.TimeToLiveCleanup(),
    null,
    TimeSpan.Zero,
    TimeSpan.FromMinutes(5));

// 3. Clear specific containers when appropriate
InMemoryKeyValueStore<string, MyState>.Clear("mydb", "mycontainer");
```

### Static Data Sharing Between Tests

**Symptom**: Tests fail when run together but pass individually.

**Cause**: In-memory storage uses static dictionaries shared across instances.

**Solution**:

```csharp
// Clear storage before each test
public class MyTests : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        InMemoryKeyValueStore<string, TestState>.Clear("test", "container");
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

## File Storage Issues

### Permission Denied

**Symptom**: `UnauthorizedAccessException` when reading or writing.

**Cause**: Application doesn't have file system permissions.

**Solution**:

```bash
# Linux/Mac: Grant permissions
chmod -R 755 /var/data/stores
chown -R appuser:appgroup /var/data/stores

# Docker: Mount volume with correct permissions
docker run -v /host/data:/app/data:rw myapp
```

### File Locked

**Symptom**: `IOException` indicating file is in use.

**Cause**: Another process or thread is accessing the file.

**Solution**:

```csharp
// The library handles locking internally, but if issues persist:

// 1. Check for multiple instances accessing same files
// 2. Ensure proper disposal of stores
// 3. Check for external processes (file sync, antivirus)

// For debugging, check which process has the lock:
// Windows: handle.exe -a "path\to\file.json"
// Linux: lsof /path/to/file.json
```

### Disk Full

**Symptom**: `IOException` when writing files.

**Cause**: Insufficient disk space.

**Solution**:

```csharp
// 1. Monitor disk space
var drives = DriveInfo.GetDrives();
foreach (var drive in drives.Where(d => d.IsReady))
{
    var freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
    if (freePercent < 10)
    {
        _logger.LogWarning("Drive {Name} has only {Free}% free space",
            drive.Name, freePercent);
    }
}

// 2. Implement cleanup of old files
// 3. Use TTL to automatically expire data
// 4. Consider archiving old data
```

### File Corruption

**Symptom**: `JsonException` when reading files.

**Cause**: File was not completely written (crash, power loss).

**Solution**:

```csharp
// 1. Handle corrupt files gracefully
try
{
    var state = await store.GetAsync(key, cancellationToken);
}
catch (JsonException ex)
{
    _logger.LogError(ex, "Corrupt file for key {Key}", key);

    // Remove corrupt file
    await store.RemoveAsync(key, null, cancellationToken);

    // Recreate from source if possible
}

// 2. Implement backup strategy
// 3. Consider using file system with journaling
```

## Dapr Storage Issues

### Actor Not Found

**Symptom**: `ActorNotFoundException` or similar.

**Cause**: Actor not registered or Dapr sidecar not running.

**Solution**:

```csharp
// 1. Verify actor registration
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<KeyValueStoreActor<MyState>>();
});

// 2. Check Dapr sidecar is running
// dapr list

// 3. Verify actors endpoint is mapped
app.MapActorsHandlers();
```

### State Store Not Configured

**Symptom**: State operations fail with component not found.

**Cause**: Dapr state store component not configured.

**Solution**:

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
    - name: actorStateStore
      value: "true"  # Required for actors
```

### Actor Timeout

**Symptom**: Operations take too long or timeout.

**Cause**: Actor processing is slow or stuck.

**Solution**:

```csharp
// 1. Configure appropriate timeouts
builder.Services.AddActors(options =>
{
    options.ActorIdleTimeout = TimeSpan.FromMinutes(5);
    options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(30);
});

// 2. Keep actor state small
// 3. Avoid long-running operations in actors
// 4. Use async operations properly
```

## Concurrency Issues

### Frequent Concurrency Conflicts

**Symptom**: Many `ConcurrencyException` occurrences.

**Cause**: High contention on the same keys.

**Solution**:

```csharp
// 1. Reduce contention by partitioning
// Instead of single counter, use sharded counters
var shardId = key.GetHashCode() % 10;
var shardKey = $"counter-{shardId}";

// 2. Use optimistic concurrency with retry
// 3. Consider using Dapr actors for high-contention keys
// 4. Redesign to reduce write frequency
```

### Stale Reads

**Symptom**: Reading outdated data.

**Cause**: Caching without invalidation, or reading replica.

**Solution**:

```csharp
// 1. Disable caching for critical reads
var fresh = await store.GetAsync(key, cancellationToken);
// fresh.Etag contains the current version

// 2. Implement cache invalidation on writes
// 3. Use consistent read options if available
```

## Performance Issues

### Slow Operations

**Symptom**: Read/write operations are slow.

**Diagnosis**:

```csharp
// Add timing to diagnose
var sw = Stopwatch.StartNew();
await store.GetAsync(key, cancellationToken);
_logger.LogDebug("GetAsync took {Elapsed}ms", sw.ElapsedMilliseconds);
```

**Solutions**:

| Issue | Solution |
|-------|----------|
| Slow disk I/O | Use SSD, optimize file paths |
| Large state objects | Reduce state size, split into multiple keys |
| Network latency (Dapr) | Co-locate services, use local sidecar |
| JSON serialization | Use source generators, optimize options |

### Memory Leaks

**Symptom**: Memory usage continuously increases.

**Diagnosis**:

```csharp
// Monitor memory
GC.Collect();
GC.WaitForPendingFinalizers();
var memoryBefore = GC.GetTotalMemory(true);

// Perform operations

var memoryAfter = GC.GetTotalMemory(true);
_logger.LogDebug("Memory delta: {Delta} bytes", memoryAfter - memoryBefore);
```

**Solutions**:

1. Ensure proper disposal of resources
2. Use TTL for temporary data
3. Run periodic cleanup
4. Profile with memory analysis tools

## Configuration Issues

### Settings Not Loading

**Symptom**: Default values used instead of configured values.

**Cause**: Configuration section name mismatch or binding issue.

**Solution**:

```csharp
// Verify configuration section exists
var section = configuration.GetSection("Hexalith:KeyValueStorages");
if (!section.Exists())
{
    _logger.LogWarning("Configuration section not found");
}

// Check binding
var settings = new KeyValueStorageSettings();
section.Bind(settings);
_logger.LogDebug("StorageRootPath: {Path}", settings.StorageRootPath);

// Ensure correct registration
services.Configure<KeyValueStorageSettings>(
    configuration.GetSection("Hexalith:KeyValueStorages"));
```

### Path Resolution Issues

**Symptom**: Files created in unexpected locations.

**Cause**: Relative paths resolved differently than expected.

**Solution**:

```csharp
// Use absolute paths
var settings = new KeyValueStorageSettings
{
    StorageRootPath = Path.GetFullPath("./data")
    // or
    // StorageRootPath = "/var/data/stores"
};

// Log the resolved path
_logger.LogInformation("Storage path: {Path}",
    Path.GetFullPath(settings.StorageRootPath));
```

## Diagnostic Logging

Enable detailed logging to diagnose issues:

```csharp
// In appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Hexalith.KeyValueStorages": "Debug"
    }
  }
}
```

```csharp
// Or configure in code
builder.Logging.AddFilter("Hexalith.KeyValueStorages", LogLevel.Debug);
```

## Getting Help

If you're still experiencing issues:

1. **Check the documentation**: Review package README files
2. **Search existing issues**: [GitHub Issues](https://github.com/Hexalith/Hexalith.KeyValueStorages/issues)
3. **Create a new issue**: Include:
   - Package version
   - .NET version
   - Minimal reproduction code
   - Full exception stack trace
   - Relevant configuration
4. **Join the community**: [Discord](https://discordapp.com/channels/1102166958918610994/1102166958918610997)
