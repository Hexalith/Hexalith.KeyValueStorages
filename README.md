# Hexalith.KeyValueStorages

A flexible, extensible key-value storage library for .NET applications. This library provides multiple storage backends with support for optimistic concurrency control through ETags, time-to-live (TTL) expiration, and thread-safe operations.

## Features

- **Multiple Storage Backends**: In-memory, file-based JSON, and Dapr actor-based storage
- **Optimistic Concurrency Control**: Built-in ETag support prevents data conflicts
- **Time-to-Live (TTL)**: Automatic expiration of stored values
- **Thread-Safe**: All implementations support concurrent access
- **Async/Await**: Fully asynchronous API
- **Generic Type Support**: Works with any key and value types
- **Extensible Architecture**: Easy to implement custom storage providers

## Build Status

[![License: MIT](https://img.shields.io/github/license/hexalith/hexalith.KeyValueStorages)](https://github.com/hexalith/hexalith/blob/main/LICENSE)
[![Discord](https://img.shields.io/discord/1063152441819942922?label=Discord&logo=discord&logoColor=white&color=d82679)](https://discordapp.com/channels/1102166958918610994/1102166958918610997)

[![Coverity Scan Build Status](https://scan.coverity.com/projects/31529/badge.svg)](https://scan.coverity.com/projects/hexalith-hexalith-KeyValueStorages)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/d48f6d9ab9fb4776b6b4711fc556d1c4)](https://app.codacy.com/gh/Hexalith/Hexalith.KeyValueStorages/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.KeyValueStorages&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.KeyValueStorages)

[![Build status](https://github.com/Hexalith/Hexalith.KeyValueStorages/actions/workflows/build-release.yml/badge.svg)](https://github.com/Hexalith/Hexalith.KeyValueStorages/actions)
[![NuGet](https://img.shields.io/nuget/v/Hexalith.KeyValueStorages.svg)](https://www.nuget.org/packages/Hexalith.KeyValueStorages)
[![Latest](https://img.shields.io/github/v/release/Hexalith/Hexalith.KeyValueStorages?include_prereleases&label=preview)](https://github.com/Hexalith/Hexalith.KeyValueStorages/pkgs/nuget/Hexalith.KeyValueStorages)

## Quick Start

### Installation

```bash
# Core package with in-memory storage
dotnet add package Hexalith.KeyValueStorages

# File-based JSON storage
dotnet add package Hexalith.KeyValueStorages.Files

# Dapr actor-based storage
dotnet add package Hexalith.KeyValueStorages.DaprComponents
```

### Basic Usage

```csharp
using Hexalith.KeyValueStorages;

// Define your state
public record ProductState : StateBase
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

// Create an in-memory store
var store = new InMemoryKeyValueStore<string, ProductState>();

// Add a new item
var product = new ProductState { Name = "Widget", Price = 19.99m };
string etag = await store.AddAsync("product-001", product, cancellationToken);

// Retrieve the item
ProductState? result = await store.TryGetAsync("product-001", cancellationToken);

// Update with optimistic concurrency
var updated = result! with { Price = 24.99m, Etag = etag };
string newEtag = await store.SetAsync("product-001", updated, cancellationToken);

// Remove the item
await store.RemoveAsync("product-001", newEtag, cancellationToken);
```

### File-Based Storage

```csharp
using Hexalith.KeyValueStorages.Files;

// Create a JSON file store
var settings = new KeyValueStorageSettings { StorageRootPath = "./data" };
var store = new JsonFileKeyValueStore<string, ProductState>(
    settings,
    database: "myapp",
    container: "products");

// Same API as in-memory store
string etag = await store.AddAsync("product-001", product, cancellationToken);
```

### Dependency Injection

```csharp
// In your Program.cs or Startup.cs
services.AddSingleton<IKeyValueProvider, JsonFileKeyValueProvider>();
services.Configure<KeyValueStorageSettings>(config.GetSection("Hexalith:KeyValueStorages"));

// In your service
public class ProductService
{
    private readonly IKeyValueStore<string, ProductState> _store;

    public ProductService(IKeyValueProvider provider)
    {
        _store = provider.Create<string, ProductState>("myapp", "products", "product");
    }
}
```

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| [Hexalith.KeyValueStorages.Abstractions](./src/libraries/Hexalith.KeyValueStorages.Abstractions/README.md) | Core interfaces and abstractions | [![NuGet](https://img.shields.io/nuget/v/Hexalith.KeyValueStorages.Abstractions.svg)](https://www.nuget.org/packages/Hexalith.KeyValueStorages.Abstractions) |
| [Hexalith.KeyValueStorages](./src/libraries/Hexalith.KeyValueStorages/README.md) | In-memory implementation | [![NuGet](https://img.shields.io/nuget/v/Hexalith.KeyValueStorages.svg)](https://www.nuget.org/packages/Hexalith.KeyValueStorages) |
| [Hexalith.KeyValueStorages.Files](./src/libraries/Hexalith.KeyValueStorages.Files/README.md) | File-based JSON storage | [![NuGet](https://img.shields.io/nuget/v/Hexalith.KeyValueStorages.Files.svg)](https://www.nuget.org/packages/Hexalith.KeyValueStorages.Files) |
| [Hexalith.KeyValueStorages.DaprComponents](./src/libraries/Hexalith.KeyValueStorages.DaprComponents/README.md) | Dapr actor-based storage | [![NuGet](https://img.shields.io/nuget/v/Hexalith.KeyValueStorages.DaprComponents.svg)](https://www.nuget.org/packages/Hexalith.KeyValueStorages.DaprComponents) |

## Repository Structure

- [src/libraries](./src/libraries/README.md) - Source code for NuGet packages
- [src/examples](./src/examples/README.md) - Example implementations
- [test](./test/README.md) - Unit tests
- [Hexalith.Builds](./Hexalith.Builds/README.md) - Shared build configurations

## Key Concepts

### Optimistic Concurrency with ETags

Every stored value has an associated ETag (entity tag) that changes on each update. When updating or removing a value, you must provide the current ETag to prevent conflicts:

```csharp
// Get current state with ETag
var state = await store.GetAsync("key", cancellationToken);

// Update requires the ETag
var updated = state with { Name = "New Name" };
string newEtag = await store.SetAsync("key", updated, cancellationToken);
// If another process updated the value, this throws ConcurrencyException
```

### Time-to-Live (TTL)

Values can automatically expire after a specified duration:

```csharp
var state = new ProductState
{
    Name = "Limited Offer",
    TimeToLive = TimeSpan.FromHours(24) // Expires in 24 hours
};
await store.AddAsync("offer-001", state, cancellationToken);
```

### State Base Class

The `StateBase` class provides common properties for stored values:

```csharp
public record StateBase
{
    public string? Etag { get; init; }         // Concurrency control
    public TimeSpan? TimeToLive { get; init; } // Optional expiration
}
```

## Configuration

Configure settings in `appsettings.json`:

```json
{
  "Hexalith": {
    "KeyValueStorages": {
      "StorageRootPath": "/var/data/stores",
      "DefaultDatabase": "myapp",
      "DefaultContainer": "default"
    }
  }
}
```

## Examples

- [JsonExample](./src/examples/Hexalith.KeyValueStorages.JsonExample/README.md) - Console application demonstrating basic CRUD operations
- [SimpleApp](./src/examples/Hexalith.KeyValueStorages.SimpleApp/README.md) - Blazor Server application with full UI

## Documentation

- [Best Practices](./docs/BEST_PRACTICES.md) - Design patterns, concurrency handling, and advanced usage
- [Troubleshooting](./docs/TROUBLESHOOTING.md) - Common issues, error handling, and debugging tips

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting a pull request.

## Prerequisites for Development

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- [PowerShell 7](https://github.com/PowerShell/PowerShell) or later
- [Git](https://git-scm.com/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
