# Hexalith.KeyValueStorages Documentation

## Overview

The Hexalith.KeyValueStorages library provides flexible and extensible key-value storage implementations for .NET applications. It includes both in-memory and file-based storage options, with support for concurrency control through Etags.

## Core Concepts

### Key-Value Store

A key-value store is a data storage paradigm designed for storing, retrieving, and managing associative arrays, a data structure more commonly known as a dictionary or hash table. Values are stored and retrieved using a key that uniquely identifies the value.

### Etag (Entity Tag)

Etags are used for concurrency control. When a value is retrieved, its associated Etag is also returned. When updating a value, the client must provide the Etag of the value it's updating. If the Etag doesn't match the current Etag in the store, the update is rejected, preventing lost updates.

## Architecture

The library is built around the following core interfaces and classes:

### Interfaces

#### `IKeyValueStore<TKey, TValue, TEtag>`

The main interface for key-value stores, defining methods for adding, retrieving, updating, and removing key-value pairs.

```csharp
public interface IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
{
    Task<TEtag> AddAsync(TKey key, TValue value, CancellationToken cancellationToken);
    Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken);
    Task<StoreResult<TValue, TEtag>> GetAsync(TKey key, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(TKey key, TEtag etag, CancellationToken cancellationToken);
    Task<TEtag> SetAsync(TKey key, TValue value, TEtag etag, CancellationToken cancellationToken);
    Task<StoreResult<TValue, TEtag>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken);
}
```

#### `IKeySerializer<TKey>`

Interface for serializing keys to strings.

```csharp
public interface IKeySerializer<in TKey>
{
    string Serialize(TKey key);
    Task<string> SerializeAsync(TKey key, CancellationToken cancellationToken);
}
```

#### `IValueSerializer<TValue, TEtag>`

Interface for serializing values and their associated Etags.

```csharp
public interface IValueSerializer<TValue, TEtag>
{
    string DataType { get; }
    (TValue Value, TEtag Etag) Deserialize(string value);
    Task<(TValue Value, TEtag Etag)> DeserializeAsync(string value, CancellationToken cancellationToken);
    Task<(TValue Value, TEtag Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken);
    string Serialize(TValue value, TEtag etag);
    Task<string> SerializeAsync(TValue value, TEtag etag, CancellationToken cancellationToken);
    Task SerializeAsync(Stream stream, TValue value, TEtag etag, CancellationToken cancellationToken);
}
```

### Classes

#### `StoreResult<TValue, TEtag>`

Record class representing the result of a store operation, containing the value and its associated Etag.

```csharp
public record class StoreResult<TValue, TEtag>(TValue Value, TEtag Etag)
    where TEtag : notnull;
```

#### `ConcurrencyException`

Exception thrown when a concurrency conflict occurs during a storage operation.

```csharp
public class ConcurrencyException : Exception
{
    public ConcurrencyException();
    public ConcurrencyException(string? message);
    public ConcurrencyException(string? message, Exception? innerException);
}
```

## Implementations

### In-Memory Key-Value Store

The in-memory implementation stores key-value pairs in memory, making it suitable for caching and temporary storage.

#### `InMemoryKeyValueStore<TKey, TValue, TEtag>`

Base class for in-memory key-value stores with custom Etag types.

```csharp
public abstract class InMemoryKeyValueStore<TKey, TValue, TEtag> : IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
{
    // Implementation of IKeyValueStore<TKey, TValue, TEtag>
    
    protected abstract TEtag GenerateInitialEtag();
    protected abstract TEtag GenerateNextEtag(TEtag previousEtag);
}
```

#### `InMemoryKeyValueStore<TKey, TValue>`

Specialized implementation with `long` Etags.

```csharp
public class InMemoryKeyValueStore<TKey, TValue> : InMemoryKeyValueStore<TKey, TValue, long>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    protected override long GenerateInitialEtag() => 1L;
    protected override long GenerateNextEtag(long previousEtag) => previousEtag + 1;
}
```

### File-Based Key-Value Store

The file-based implementation stores each key-value pair in a separate file, providing persistence.

#### `FileKeyValueStorage<TKey, TValue, TEtag, TKeySerializer, TValueSerializer>`

Base class for file-based key-value stores.

```csharp
public abstract class FileKeyValueStorage<TKey, TValue, TEtag, TKeySerializer, TValueSerializer>
    : IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
    where TKeySerializer : IKeySerializer<TKey>, new()
    where TValueSerializer : IValueSerializer<TValue, TEtag>, new()
{
    // Implementation of IKeyValueStore<TKey, TValue, TEtag>
    
    protected abstract TEtag GenerateEtag();
    protected string GetFilePath(TKey key);
    protected Task<(TValue Value, TEtag Etag)> ReadValueFromFileAsync(string filePath, CancellationToken cancellationToken);
    protected virtual string SanitizeFileName(string fileName);
    protected Task WriteValueToFileAsync(string filePath, TValue value, TEtag etag, CancellationToken cancellationToken);
}
```

#### `JsonFileKeyValueStorage<TKey, TValue, TEtag>`

JSON file implementation with custom Etag types.

```csharp
public class JsonFileKeyValueStorage<TKey, TValue, TEtag> :
    FileKeyValueStorage<
        TKey,
        TValue,
        TEtag,
        KeyToStringSerializer<TKey>,
        JsonFileSerializer<TValue, TEtag>>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
    where TEtag : notnull
{
    public JsonFileKeyValueStorage(string rootPath);
    public JsonFileKeyValueStorage();
    protected override TEtag GenerateEtag();
}
```

#### `JsonFileKeyValueStorage<TKey, TValue>`

JSON file implementation with string Etags.

```csharp
public class JsonFileKeyValueStorage<TKey, TValue> :
    JsonFileKeyValueStorage<
        TKey,
        TValue,
        string>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    public JsonFileKeyValueStorage(string rootPath);
    public JsonFileKeyValueStorage();
    protected override string GenerateEtag();
}
```

## Usage Examples

### Basic Usage

```csharp
// Create an in-memory key-value store
var memoryStore = new InMemoryKeyValueStore<string, string>();

// Add a key-value pair
var etag = await memoryStore.AddAsync("key1", "value1", CancellationToken.None);

// Get a value
var result = await memoryStore.GetAsync("key1", CancellationToken.None);
Console.WriteLine($"Value: {result.Value}, Etag: {result.Etag}");

// Update a value
var newEtag = await memoryStore.SetAsync("key1", "updated-value1", result.Etag, CancellationToken.None);

// Remove a key-value pair
var removed = await memoryStore.RemoveAsync("key1", newEtag, CancellationToken.None);
```

### File-Based Storage

```csharp
// Create a file-based key-value store
var fileStore = new JsonFileKeyValueStorage<string, string>("./key-value-store");

// Add a key-value pair
var etag = await fileStore.AddAsync("key1", "value1", CancellationToken.None);

// Get a value
var result = await fileStore.GetAsync("key1", CancellationToken.None);
Console.WriteLine($"Value: {result.Value}, Etag: {result.Etag}");

// Update a value
var newEtag = await fileStore.SetAsync("key1", "updated-value1", result.Etag, CancellationToken.None);

// Remove a key-value pair
var removed = await fileStore.RemoveAsync("key1", newEtag, CancellationToken.None);
```

### Handling Concurrency

```csharp
var store = new InMemoryKeyValueStore<string, string>();
var etag = await store.AddAsync("key1", "value1", CancellationToken.None);
var result = await store.GetAsync("key1", CancellationToken.None);

// Simulate a concurrent update
var concurrentEtag = await store.SetAsync("key1", "concurrent-value", result.Etag, CancellationToken.None);

try
{
    // This will fail because the Etag has changed
    await store.SetAsync("key1", "conflict-value", result.Etag, CancellationToken.None);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Concurrency error: {ex.Message}");
    
    // Get the latest value and Etag
    var latestResult = await store.GetAsync("key1", CancellationToken.None);
    
    // Update with the latest Etag
    var newEtag = await store.SetAsync("key1", "resolved-value", latestResult.Etag, CancellationToken.None);
}
```

### Custom Types

```csharp
// Custom value type
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Using custom types
var personStore = new InMemoryKeyValueStore<Guid, Person>();
var personId = Guid.NewGuid();
await personStore.AddAsync(personId, new Person { Name = "John", Age = 30 }, CancellationToken.None);
```

## Best Practices

1. **Always use the Etag for updates**: When updating a value, always use the Etag from the most recent retrieval to ensure you're not overwriting changes made by other clients.

2. **Handle concurrency exceptions**: Be prepared to handle concurrency exceptions and implement a strategy for resolving conflicts.

3. **Use appropriate store types**: Use in-memory stores for caching and temporary storage, and file-based stores for persistence.

4. **Dispose of resources**: If your implementation uses disposable resources, ensure they are properly disposed of.

5. **Consider performance implications**: File-based stores are slower than in-memory stores, so consider the performance requirements of your application.

## Extension Points

The library is designed to be extensible. You can create your own implementations of:

- `IKeyValueStore<TKey, TValue, TEtag>` for custom storage mechanisms
- `IKeySerializer<TKey>` for custom key serialization
- `IValueSerializer<TValue, TEtag>` for custom value serialization

## Conclusion

The Hexalith.KeyValueStorages library provides a flexible and extensible framework for key-value storage in .NET applications. With support for both in-memory and file-based storage, and built-in concurrency control, it's suitable for a wide range of use cases.