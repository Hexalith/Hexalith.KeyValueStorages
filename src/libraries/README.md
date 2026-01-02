# Hexalith Key/Value Storages Libraries

This directory contains the source code for the Hexalith Key/Value Storages NuGet packages.

## Package Overview

| Package | Description | Use When |
|---------|-------------|----------|
| [Abstractions](./Hexalith.KeyValueStorages.Abstractions/README.md) | Core interfaces | Building storage-agnostic code |
| [KeyValueStorages](./Hexalith.KeyValueStorages/README.md) | In-memory implementation | Fast, temporary storage |
| [Files](./Hexalith.KeyValueStorages.Files/README.md) | File-based JSON storage | Simple persistent storage |
| [DaprComponents](./Hexalith.KeyValueStorages.DaprComponents/README.md) | Dapr actor storage | Distributed applications |

## Installation

```bash
# Core abstractions (usually included as dependency)
dotnet add package Hexalith.KeyValueStorages.Abstractions

# In-memory implementation
dotnet add package Hexalith.KeyValueStorages

# File-based JSON storage
dotnet add package Hexalith.KeyValueStorages.Files

# Dapr actor-based storage
dotnet add package Hexalith.KeyValueStorages.DaprComponents
```

## Quick Comparison

| Feature | In-Memory | Files | Dapr Actors |
|---------|-----------|-------|-------------|
| Persistence | No | Yes | Yes |
| Distributed | No | No | Yes |
| TTL Support | Yes | Yes | Configurable |
| Performance | Fastest | Medium | Network-dependent |
| Setup | None | Path config | Dapr runtime |

## Common Interface

All implementations share the same interface:

```csharp
public interface IKeyValueStore<TKey, TState>
{
    Task<string> AddAsync(TKey key, TState value, CancellationToken ct);
    Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken ct);
    Task<string> SetAsync(TKey key, TState value, CancellationToken ct);
    Task<TState> GetAsync(TKey key, CancellationToken ct);
    Task<TState?> TryGetAsync(TKey key, CancellationToken ct);
    Task<bool> ContainsKeyAsync(TKey key, CancellationToken ct);
    Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken ct);
}
```

## Choosing the Right Implementation

### Use In-Memory When:
- Data can be lost on restart
- Maximum performance is required
- Testing or development scenarios
- Caching with TTL

### Use File Storage When:
- Simple persistence is needed
- Single-server deployment
- Human-readable storage is helpful
- No distributed requirements

### Use Dapr Actors When:
- Distributed/multi-instance deployment
- Strong consistency is required
- High availability is needed
- Already using Dapr ecosystem

## Related Documentation

- [Best Practices](../../docs/BEST_PRACTICES.md) - Design patterns and advanced usage
- [Troubleshooting](../../docs/TROUBLESHOOTING.md) - Common issues and solutions
- [Examples](../examples/README.md) - Sample applications
