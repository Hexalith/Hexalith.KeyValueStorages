# Hexalith Key/Value Storages Abstractions

## Overview

The Hexalith.KeyValueStorages.Abstractions package provides the core interfaces and abstractions for implementing key/value storage solutions in the Hexalith ecosystem. It defines a common contract that different storage implementations must follow, ensuring consistency and interoperability across various storage providers.

## Purpose

This abstraction layer serves several key purposes:

- Defines a standardized interface for key/value storage operations
- Enables storage provider independence through abstraction
- Facilitates easy switching between different storage implementations
- Provides a foundation for building storage-agnostic applications

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
- A compatible storage implementation package

### Installation

Add the package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages.Abstractions
```

### Basic Usage

```csharp
// Inject the storage interface
IKeyValueStorage storage = // ... storage implementation

// Store a value
await storage.SetAsync("myKey", "myValue");

// Retrieve a value
string value = await storage.GetAsync<string>("myKey");

// Check if key exists
bool exists = await storage.ExistsAsync("myKey");

// Remove a value
await storage.RemoveAsync("myKey");
```

## Learn More

- [Hexalith Key/Value Stores](https://github.com/Hexalith/Hexalith.KeyValueStores) - Main repository
- [Available Storage Implementations](https://github.com/Hexalith/Hexalith.KeyValueStores/tree/main/src) - List of supported storage providers
- [Examples](https://github.com/Hexalith/Hexalith.KeyValueStores/tree/main/examples) - Sample implementations
