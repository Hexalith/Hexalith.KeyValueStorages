# Key-Value Storage Examples

This directory contains examples demonstrating the usage of the Hexalith.KeyValueStorages library.

## Overview

The Hexalith.KeyValueStorages library provides implementations for:

1. **In-Memory Key-Value Store**: A fast, in-memory implementation suitable for caching and temporary storage.
2. **File-Based Key-Value Store**: A persistent storage implementation that saves data to files.

## Example Code

The `Program.cs` file in the `Hexalith.KeyValueStorages.Example` project demonstrates:

- Basic CRUD operations (Add, Get, Update, Remove)
- Concurrency control with Etags
- Error handling

## Running the Examples

To run the examples:

1. Make sure you have the .NET 9.0 SDK installed
2. Update the project references in the `Hexalith.KeyValueStorages.Example.csproj` file:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages\Hexalith.KeyValueStorages.csproj" />
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages.Files\Hexalith.KeyValueStorages.Files.csproj" />
   </ItemGroup>
   ```
3. Uncomment the file store examples in the `Program.cs` file
4. Run the project:
   ```
   cd Hexalith.KeyValueStorages.Example
   dotnet run
   ```

## Memory Store Example

```csharp
// Create an in-memory key-value store with string keys and values
var memoryStore = new InMemoryKeyValueStore<string, string>();

// Add key-value pairs
var etag1 = await memoryStore.AddAsync("key1", "value1", CancellationToken.None);
var etag2 = await memoryStore.AddAsync("key2", "value2", CancellationToken.None);

// Get values by key
var result1 = await memoryStore.GetAsync("key1", CancellationToken.None);
Console.WriteLine($"Retrieved key1: Value = {result1.Value}, Etag = {result1.Etag}");

// Check if a key exists
var exists1 = await memoryStore.ContainsKeyAsync("key1", CancellationToken.None);
var exists3 = await memoryStore.ContainsKeyAsync("key3", CancellationToken.None);

// Try to get a value (safe method that doesn't throw if key doesn't exist)
var tryResult1 = await memoryStore.TryGetValueAsync("key1", CancellationToken.None);
var tryResult3 = await memoryStore.TryGetValueAsync("key3", CancellationToken.None);

// Update a value
var newEtag1 = await memoryStore.SetAsync("key1", "updated-value1", result1.Etag, CancellationToken.None);
var updatedResult1 = await memoryStore.GetAsync("key1", CancellationToken.None);

// Demonstrate concurrency control
try {
    // Try to update with an old Etag
    await memoryStore.SetAsync("key1", "conflict-value", result1.Etag, CancellationToken.None);
} catch (InvalidOperationException ex) {
    Console.WriteLine($"Expected concurrency error: {ex.Message}");
}

// Remove a key-value pair
var removed = await memoryStore.RemoveAsync("key2", etag2, CancellationToken.None);
```

## File Store Example

```csharp
// Create a temporary directory for the file store
var tempDir = Path.Combine(Path.GetTempPath(), "KeyValueStoreExample", Guid.NewGuid().ToString());
Directory.CreateDirectory(tempDir);

// Create a file-based key-value store with string keys and values
var fileStore = new JsonFileKeyValueStorage<string, string>(tempDir);

// Add key-value pairs
var etag1 = await fileStore.AddAsync("file-key1", "file-value1", CancellationToken.None);
var etag2 = await fileStore.AddAsync("file-key2", "file-value2", CancellationToken.None);

// Get values by key
var result1 = await fileStore.GetAsync("file-key1", CancellationToken.None);
Console.WriteLine($"Retrieved file-key1: Value = {result1.Value}, Etag = {result1.Etag}");

// Check if a key exists
var exists1 = await fileStore.ContainsKeyAsync("file-key1", CancellationToken.None);
var exists3 = await fileStore.ContainsKeyAsync("file-key3", CancellationToken.None);

// Try to get a value (safe method that doesn't throw if key doesn't exist)
var tryResult1 = await fileStore.TryGetValueAsync("file-key1", CancellationToken.None);
var tryResult3 = await fileStore.TryGetValueAsync("file-key3", CancellationToken.None);

// Update a value
var newEtag1 = await fileStore.SetAsync("file-key1", "updated-file-value1", result1.Etag, CancellationToken.None);
var updatedResult1 = await fileStore.GetAsync("file-key1", CancellationToken.None);

// Demonstrate concurrency control
try {
    // Try to update with an old Etag
    await fileStore.SetAsync("file-key1", "conflict-value", result1.Etag, CancellationToken.None);
} catch (ConcurrencyException ex) {
    Console.WriteLine($"Expected concurrency error: {ex.Message}");
}

// Remove a key-value pair
var removed = await fileStore.RemoveAsync("file-key2", etag2, CancellationToken.None);

// Clean up the temporary directory
Directory.Delete(tempDir, true);
```

## Key Features

### Memory Store
- Fast in-memory storage
- Thread-safe operations
- Automatic Etag generation for concurrency control
- No persistence (data is lost when the application restarts)

### File Store
- Persistent storage using files
- Each key-value pair is stored in a separate file
- JSON serialization for values
- Automatic Etag generation for concurrency control
- File name sanitization for keys

## Advanced Usage

### Custom Key Types

Both store implementations support custom key types:

```csharp
// Using a custom key type (must be non-null and implement IEquatable<T>)
var customKeyStore = new InMemoryKeyValueStore<Guid, string>();
var key = Guid.NewGuid();
await customKeyStore.AddAsync(key, "value", CancellationToken.None);
```

### Custom Value Types

Both store implementations support custom value types:

```csharp
// Using a custom value type (must be non-null)
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

var personStore = new InMemoryKeyValueStore<string, Person>();
await personStore.AddAsync("person1", new Person { Name = "John", Age = 30 }, CancellationToken.None);
```

### Custom Etag Types

The base implementations support custom Etag types:

```csharp
// Using a custom Etag type (must be non-null)
public class CustomEtag : IEquatable<CustomEtag>
{
    public string Value { get; }
    
    public CustomEtag(string value)
    {
        Value = value;
    }
    
    public bool Equals(CustomEtag? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }
    
    public override bool Equals(object? obj) => Equals(obj as CustomEtag);
    public override int GetHashCode() => Value.GetHashCode();
}