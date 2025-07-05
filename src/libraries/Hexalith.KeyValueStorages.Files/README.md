# Hexalith File Key/Value Storages

## Overview

Hexalith.KeyValueStorages.Files provides file-based implementations of the key-value storage interfaces defined in Hexalith.KeyValueStorages.Abstractions. It offers a robust solution for persisting key-value pairs to the file system using JSON serialization, with built-in support for optimistic concurrency control through Etags.

## Purpose

This library serves several key purposes:

- Provides persistent storage for key-value pairs using the file system
- Implements optimistic concurrency control to prevent data corruption
- Offers a flexible serialization mechanism for various data types
- Ensures thread-safe file operations for concurrent access
- Maintains compatibility with the core Hexalith.KeyValueStorages interfaces

## Features

### File-Based Persistence

- Stores each key-value pair in a separate file
- Automatically creates storage directories as needed
- Supports customizable root storage paths
- Handles file system operations safely

### JSON Serialization

- Serializes values to JSON format for human-readable storage
- Includes Etags in the serialized data for concurrency control
- Supports custom serialization options
- Handles complex object graphs through System.Text.Json

### Optimistic Concurrency

- Uses Etags to detect concurrent modifications
- Prevents data corruption from simultaneous updates
- Throws ConcurrencyException when Etags don't match
- Generates new Etags for each update

### Extensibility

- Abstract base classes for creating custom file storage implementations
- Pluggable serialization mechanisms
- Support for different Etag types

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Write permissions to the storage directory
- Hexalith.KeyValueStorages.Abstractions package

### Installation

Add the NuGet package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages.Files
```

### Basic Usage

```csharp
// Create a JSON file store with string keys and custom value type
var store = new JsonFileKeyValueStorage<string, MyData>("data-directory");

// Add a new value
string etag = await store.AddAsync("key1", new MyData { Name = "Test" }, CancellationToken.None);

// Retrieve a value
var result = await store.GetAsync("key1", CancellationToken.None);
MyData value = result.Value;
string currentEtag = result.Etag;

// Update a value with concurrency check
string newEtag = await store.SetAsync("key1", updatedData, currentEtag, CancellationToken.None);

// Check if a key exists
bool exists = await store.ContainsKeyAsync("key1", CancellationToken.None);

// Remove a value
bool removed = await store.RemoveAsync("key1", currentEtag, CancellationToken.None);
```

## Architecture

### Class Hierarchy

- `FileKeyValueStorage<TKey, TValue, TEtag, TKeySerializer, TValueSerializer>`: Abstract base class for file-based storage
  - `JsonFileKeyValueStorage<TKey, TValue, TEtag>`: Implementation using JSON serialization with custom Etag type
    - `JsonFileKeyValueStorage<TKey, TValue>`: Simplified implementation using string Etags

### Key Components

- `JsonFileValue<TValue, TEtag>`: Record type for storing value and Etag together
- `JsonFileSerializer<TValue, TEtag>`: Serializer for JSON file values
- `KeyToStringSerializer<TKey>`: Converts keys to file-safe string representations

## Examples

### Using with Simple Types

```csharp
// Create a store for string keys and values
var stringStore = new JsonFileKeyValueStorage<string, string>("string-store");

// Add a string value
string etag = await stringStore.AddAsync("greeting", "Hello, World!", CancellationToken.None);
```

### Using with Complex Types

```csharp
// Define a custom data type
public record UserProfile
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTimeOffset LastLogin { get; init; }
    public int LoginCount { get; init; }
}

// Create a store for user profiles
var userStore = new JsonFileKeyValueStorage<string, UserProfile>("user-profiles");

// Add a user profile
var profile = new UserProfile
{
    Username = "johndoe",
    Email = "john@example.com",
    LastLogin = DateTimeOffset.UtcNow,
    LoginCount = 1
};

string etag = await userStore.AddAsync("user123", profile, CancellationToken.None);
```

## Advanced Usage

### Custom Etag Generation

The `JsonFileKeyValueStorage<TKey, TValue>` class uses string Etags by default, generated using `UniqueIdHelper.GenerateUniqueStringId()`. You can create a custom implementation with different Etag types:

```csharp
public class CustomEtagStorage<TKey, TValue> :
    JsonFileKeyValueStorage<TKey, TValue, Guid>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    public CustomEtagStorage(string rootPath) : base(rootPath) { }

    protected override Guid GenerateEtag() => Guid.NewGuid();
}
```

### Custom Serialization Options

You can customize the JSON serialization options:

```csharp
public class CustomJsonStorage<TKey, TValue> :
    FileKeyValueStorage<TKey, TValue, string, KeyToStringSerializer<TKey>, CustomJsonSerializer<TValue, string>>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    public CustomJsonStorage(string rootPath) : base(rootPath) { }

    protected override string GenerateEtag() => UniqueIdHelper.GenerateUniqueStringId();
}

public class CustomJsonSerializer<TValue, TEtag> : JsonFileSerializer<TValue, TEtag>
    where TValue : notnull
    where TEtag : notnull
{
    public CustomJsonSerializer() : base(new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    })
    { }
}
```

## Learn More

- [Hexalith Key/Value Stores](https://github.com/Hexalith/Hexalith.KeyValueStorages) - Main repository
- [Hexalith.KeyValueStorages.Abstractions](https://github.com/Hexalith/Hexalith.KeyValueStorages/tree/main/src/Hexalith.KeyValueStorages.Abstractions) - Core interfaces
- [Examples](https://github.com/Hexalith/Hexalith.KeyValueStorages/tree/main/examples) - Sample implementations