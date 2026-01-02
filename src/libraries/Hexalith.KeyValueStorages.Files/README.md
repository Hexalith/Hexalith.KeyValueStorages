# Hexalith File Key/Value Storages

## Overview

Hexalith.KeyValueStorages.Files provides file-based implementations of the key-value storage interfaces. It offers persistent storage for key-value pairs using JSON serialization, with built-in support for optimistic concurrency control through ETags and automatic TTL-based expiration.

## Features

- **Persistent Storage**: Each key-value pair stored in a separate JSON file
- **Optimistic Concurrency**: ETag-based conflict detection
- **Time-to-Live (TTL)**: Automatic expiration based on file modification time
- **Human-Readable**: JSON format allows easy inspection and debugging
- **Thread-Safe**: File locking for concurrent access
- **Configurable**: Customizable paths, serialization options, and directory structure

## Installation

```bash
dotnet add package Hexalith.KeyValueStorages.Files
```

## Quick Start

### Define Your State

```csharp
using Hexalith.KeyValueStorages.Abstractions;

public record DocumentState : StateBase
{
    public static string Name => "Document";

    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public DateTimeOffset ModifiedAt { get; init; }
}
```

### Create and Use a Store

```csharp
using Hexalith.KeyValueStorages.Files;

// Create a JSON file store
var settings = new KeyValueStorageSettings
{
    StorageRootPath = "./data"
};

var store = new JsonFileKeyValueStore<string, DocumentState>(
    settings,
    database: "myapp",
    container: "documents");

// Add a new document
var document = new DocumentState
{
    Title = "Getting Started",
    Content = "Welcome to the documentation...",
    Author = "John Doe",
    ModifiedAt = DateTimeOffset.UtcNow
};

string etag = await store.AddAsync("doc-001", document, cancellationToken);

// Retrieve the document
var retrieved = await store.GetAsync("doc-001", cancellationToken);
Console.WriteLine($"Title: {retrieved.Title}");

// Update with concurrency check
var updated = retrieved with
{
    Content = "Updated content...",
    ModifiedAt = DateTimeOffset.UtcNow
};
string newEtag = await store.SetAsync("doc-001", updated, cancellationToken);

// Remove the document
await store.RemoveAsync("doc-001", newEtag, cancellationToken);
```

## File Structure

Files are organized in a hierarchical directory structure:

```
{StorageRootPath}/
└── {database}/
    └── {container}/
        └── {entity}/
            ├── doc-001.json
            ├── doc-002.json
            └── doc-003.json
```

### JSON File Format

Each file contains the state data with ETag:

```json
{
  "Title": "Getting Started",
  "Content": "Welcome to the documentation...",
  "Author": "John Doe",
  "ModifiedAt": "2024-01-15T10:30:00+00:00",
  "Etag": "abc123xyz",
  "TimeToLive": "1.00:00:00"
}
```

## Configuration

### KeyValueStorageSettings

```csharp
var settings = new KeyValueStorageSettings
{
    // Root directory for all storage
    StorageRootPath = "/var/data/stores",

    // Default values when not specified
    DefaultDatabase = "default",
    DefaultContainer = "default"
};
```

### From Configuration File

```json
{
  "Hexalith": {
    "KeyValueStorages": {
      "StorageRootPath": "/var/data/stores",
      "DefaultDatabase": "myapp",
      "DefaultContainer": "main"
    }
  }
}
```

```csharp
services.Configure<KeyValueStorageSettings>(
    configuration.GetSection("Hexalith:KeyValueStorages"));
```

## JsonFileKeyValueStore API

### Constructor Options

```csharp
// Minimal - uses defaults
var store = new JsonFileKeyValueStore<string, DocumentState>();

// With settings
var store = new JsonFileKeyValueStore<string, DocumentState>(settings);

// Full control
var store = new JsonFileKeyValueStore<string, DocumentState>(
    settings: settings,
    database: "myapp",
    container: "documents",
    entity: "doc",
    timeProvider: TimeProvider.System);
```

### Methods

| Method | Description |
|--------|-------------|
| `AddAsync` | Add a new key-value pair. Creates the file. |
| `AddOrUpdateAsync` | Add or update (idempotent). |
| `SetAsync` | Update existing. Validates ETag. |
| `GetAsync` | Get value. Throws if file not found. |
| `TryGetAsync` | Try to get. Returns `null` if not found. |
| `ContainsKeyAsync` | Check if file exists. |
| `ExistsAsync` | Alias for `ContainsKeyAsync`. |
| `RemoveAsync` | Delete the file. Validates ETag. |

## Time-to-Live (TTL)

TTL is calculated based on file modification time:

```csharp
// Create a document that expires in 7 days
var document = new DocumentState
{
    Title = "Temporary Document",
    Content = "This will expire...",
    TimeToLive = TimeSpan.FromDays(7)
};

await store.AddAsync("temp-doc", document, cancellationToken);

// After 7 days, TryGetAsync returns null
// The file is automatically cleaned up on access
```

## Dependency Injection

### Registration

```csharp
// Register settings
services.Configure<KeyValueStorageSettings>(
    configuration.GetSection("Hexalith:KeyValueStorages"));

// Register provider (creates stores on demand)
services.AddSingleton<IKeyValueProvider, JsonFileKeyValueProvider>();

// Or register specific store directly
services.AddSingleton<IKeyValueStore<string, DocumentState>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<KeyValueStorageSettings>>();
    return new JsonFileKeyValueStore<string, DocumentState>(
        settings.Value,
        "myapp",
        "documents");
});
```

### Using the Provider

```csharp
public class DocumentService
{
    private readonly IKeyValueStore<string, DocumentState> _store;

    public DocumentService(IKeyValueProvider provider)
    {
        _store = provider.Create<string, DocumentState>(
            "myapp",
            "documents",
            "document");
    }
}
```

## Custom Implementations

### Custom Key Serialization

Keys are converted to file-safe names. Implement custom logic:

```csharp
public class MyFileStore<TState> : JsonFileKeyValueStore<Guid, TState>
    where TState : class, IState
{
    public MyFileStore(KeyValueStorageSettings settings)
        : base(settings, "mydb", "mycontainer") { }

    protected override string KeyToFileName(Guid key)
        => key.ToString("N"); // No hyphens: "abc123def456..."
}
```

### Custom Serialization Options

```csharp
public class PrettyJsonFileStore<TKey, TState>
    : FileKeyValueStorage<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : class, IState
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected override async Task WriteToStreamAsync(
        Stream stream,
        TState value,
        CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync(stream, value, _options, cancellationToken);
    }

    protected override async Task<TState?> ReadFromStreamAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        return await JsonSerializer.DeserializeAsync<TState>(
            stream, _options, cancellationToken);
    }
}
```

## Error Handling

```csharp
try
{
    await store.AddAsync("doc-001", document, cancellationToken);
}
catch (DuplicateKeyException<string> ex)
{
    Console.WriteLine($"Document already exists: {ex.Key}");
}

try
{
    var doc = await store.GetAsync("doc-999", cancellationToken);
}
catch (KeyNotFoundException<string> ex)
{
    Console.WriteLine($"Document not found: {ex.Key}");
}

try
{
    await store.SetAsync("doc-001", updated, cancellationToken);
}
catch (ConcurrencyException<string> ex)
{
    Console.WriteLine($"Document was modified: {ex.Key}");
    // Reload and retry
}
```

## Best Practices

1. **Use Appropriate Paths**: Store data in application-specific directories
2. **Handle Disk Space**: Monitor available space for large datasets
3. **Backup Strategy**: Files can be backed up with standard tools
4. **File Permissions**: Ensure the application has read/write access
5. **Avoid Long Keys**: Keep key names reasonable for file system limits
6. **Use TTL for Temporary Data**: Prevent unbounded storage growth

## Performance Considerations

- **File I/O**: Each operation involves disk access
- **Large Files**: Consider chunking for very large values
- **Many Files**: Directory listing may slow with thousands of files
- **SSD Recommended**: Faster random access improves performance

## Related Packages

- [Hexalith.KeyValueStorages.Abstractions](../Hexalith.KeyValueStorages.Abstractions/README.md) - Core interfaces
- [Hexalith.KeyValueStorages](../Hexalith.KeyValueStorages/README.md) - In-memory implementation
- [Hexalith.KeyValueStorages.DaprComponents](../Hexalith.KeyValueStorages.DaprComponents/README.md) - Distributed storage

## License

This project is licensed under the MIT License.