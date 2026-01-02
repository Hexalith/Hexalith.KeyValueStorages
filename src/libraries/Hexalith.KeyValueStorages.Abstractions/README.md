# Hexalith Key/Value Storages Abstractions

## Overview

The Hexalith.KeyValueStorages.Abstractions package provides the core interfaces and abstractions for implementing key/value storage solutions in the Hexalith ecosystem. It defines a common contract that different storage implementations must follow, ensuring consistency and interoperability across various storage providers.

## Purpose

This abstraction layer serves several key purposes:

- Defines a standardized interface for key/value storage operations
- Enables storage provider independence through abstraction
- Facilitates easy switching between different storage implementations
- Provides a foundation for building storage-agnostic applications

## Installation

```bash
dotnet add package Hexalith.KeyValueStorages.Abstractions
```

## Core Interfaces

### IKeyValueStore<TKey, TState>

The main interface for all key-value store operations:

```csharp
public interface IKeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    // Add a new value (throws DuplicateKeyException if key exists)
    Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken);

    // Add or update a value (idempotent)
    Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken);

    // Update an existing value (validates ETag for concurrency)
    Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken);

    // Get a value (throws KeyNotFoundException if not found)
    Task<TState> GetAsync(TKey key, CancellationToken cancellationToken);

    // Try to get a value (returns null if not found)
    Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken);

    // Check if a key exists
    Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken);

    // Remove a value (validates ETag for concurrency)
    Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken);
}
```

### IKeyValueProvider

Factory interface for creating store instances:

```csharp
public interface IKeyValueProvider
{
    IKeyValueStore<TKey, TState> Create<TKey, TState>(
        string database,
        string container,
        string entity)
        where TKey : notnull, IEquatable<TKey>
        where TState : class, IState;
}
```

### IState

Interface for state objects with metadata:

```csharp
public interface IState
{
    static abstract string Name { get; }    // State type identifier
    string? Etag { get; }                   // Concurrency control token
    TimeSpan? TimeToLive { get; }           // Optional expiration duration
}
```

## Base Classes

### StateBase

Base record for state objects with built-in serialization support:

```csharp
public record StateBase : IState
{
    public string? Etag { get; init; }
    public TimeSpan? TimeToLive { get; init; }
}
```

### State<TValue>

Generic wrapper for storing any value type:

```csharp
public record State<TValue> : StateBase
{
    public TValue Value { get; init; }
}
```

## Usage Examples

### Defining a Custom State

```csharp
using Hexalith.KeyValueStorages.Abstractions;

public record CustomerState : StateBase
{
    public static string Name => "Customer";

    public string CustomerId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
```

### Using the Store Interface

```csharp
public class CustomerService
{
    private readonly IKeyValueStore<string, CustomerState> _store;

    public CustomerService(IKeyValueStore<string, CustomerState> store)
    {
        _store = store;
    }

    public async Task<CustomerState> CreateCustomerAsync(
        string customerId,
        string email,
        CancellationToken cancellationToken)
    {
        var customer = new CustomerState
        {
            CustomerId = customerId,
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        string etag = await _store.AddAsync(customerId, customer, cancellationToken);
        return customer with { Etag = etag };
    }

    public async Task<CustomerState?> GetCustomerAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        return await _store.TryGetAsync(customerId, cancellationToken);
    }

    public async Task UpdateEmailAsync(
        string customerId,
        string newEmail,
        CancellationToken cancellationToken)
    {
        var customer = await _store.GetAsync(customerId, cancellationToken);
        var updated = customer with { Email = newEmail };
        await _store.SetAsync(customerId, updated, cancellationToken);
    }
}
```

### Using the Factory Pattern

```csharp
public class MultiTenantService
{
    private readonly IKeyValueProvider _provider;

    public MultiTenantService(IKeyValueProvider provider)
    {
        _provider = provider;
    }

    public IKeyValueStore<string, CustomerState> GetStoreForTenant(string tenantId)
    {
        return _provider.Create<string, CustomerState>(
            database: tenantId,
            container: "customers",
            entity: "customer");
    }
}
```

## Exception Types

| Exception | Description |
|-----------|-------------|
| `DuplicateKeyException<TKey>` | Thrown when adding a key that already exists |
| `KeyNotFoundException<TKey>` | Thrown when getting a key that doesn't exist |
| `ConcurrencyException<TKey>` | Thrown when ETag validation fails during update/delete |

## Available Implementations

| Package | Storage Type | Description |
|---------|--------------|-------------|
| [Hexalith.KeyValueStorages](../Hexalith.KeyValueStorages/README.md) | In-Memory | Fast, temporary storage |
| [Hexalith.KeyValueStorages.Files](../Hexalith.KeyValueStorages.Files/README.md) | File System | Persistent JSON storage |
| [Hexalith.KeyValueStorages.DaprComponents](../Hexalith.KeyValueStorages.DaprComponents/README.md) | Dapr Actors | Distributed storage |

## Learn More

- [Main Repository](https://github.com/Hexalith/Hexalith.KeyValueStorages)
- [Examples](https://github.com/Hexalith/Hexalith.KeyValueStorages/tree/main/src/examples)
