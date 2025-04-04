# Hexalith Key/Value Storages

## Overview

Hexalith Key/Value Storages is a flexible and extensible key-value storage library for .NET applications. It provides both in-memory and file-based storage implementations with support for optimistic concurrency control through ETags.

## Purpose

The library offers a robust foundation for implementing key-value storage solutions with the following features:

- Generic type support for both keys and values
- Optimistic concurrency control using ETags
- Multiple storage implementations:
  - In-memory storage for fast, temporary data
  - File-based storage with JSON serialization
- Extensible architecture through interfaces
- Thread-safe operations
- Asynchronous API support

## Getting Started

### Prerequisites

- .NET 8.0 or later
- For file-based storage: Write permissions to the storage directory

### Installation

Add the NuGet package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages
```

### Basic Usage

1. In-Memory Storage:

```csharp
// Create an in-memory store
var store = new InMemoryKeyValueStore<string, MyData>();

// Add a value
long etag = await store.AddAsync("key1", new MyData(), CancellationToken.None);

// Retrieve a value
var result = await store.GetAsync("key1", CancellationToken.None);
MyData value = result.Value;
long currentEtag = result.ETag;

// Update a value
long newEtag = await store.SetAsync("key1", updatedData, currentEtag, CancellationToken.None);
```

2. JSON File Storage:

```csharp
// Create a JSON file store
var store = new JsonFileKeyValueStorage<string, MyData>("data-directory");

// Operations are similar to in-memory store
string etag = await store.AddAsync("key1", new MyData(), CancellationToken.None);
```

## Architecture

### Core Interfaces

- `IKeyValueStore<TKey, TValue, TEtag>`: The main interface defining key-value store operations
- `IKeySerializer<TKey>`: Interface for key serialization
- `IValueSerializer<TValue, TEtag>`: Interface for value serialization with ETag support

### Implementations

1. In-Memory Storage:
   - `InMemoryKeyValueStore<TKey, TValue>`: Simple in-memory implementation with long-based ETags
   - `InMemoryKeyValueStore<TKey, TValue, TEtag>`: Base class supporting custom ETag types

2. [File Storage](https://hexalith.github.io/Hexalith.KeyValueStores).

## Features

### Optimistic Concurrency

The library implements optimistic concurrency control using ETags:
- Each value has an associated ETag
- Updates require the current ETag
- Concurrent modifications are detected and prevented

### Thread Safety

All implementations are thread-safe:
- In-memory store uses internal locking
- File-based store uses file system locks

### Extensibility

Create custom implementations by:
1. Implementing `IKeyValueStore<TKey, TValue, TEtag>`
2. Extending base classes for specific storage types
3. Creating custom serializers implementing `IKeySerializer<TKey>` or `IValueSerializer<TValue, TEtag>`

## Learn More

- [Hexalith Key/Value Stores](https://github.com/Hexalith/Hexalith.KeyValueStores)
- [API Documentation](https://hexalith.github.io/Hexalith.KeyValueStores)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.