# Hexalith Key/Value Storages Packages

This directory contains the source code for the Hexalith Key/Value Storages packages. Each package provides specific functionality for implementing key-value storage solutions in .NET applications.

## Available Packages

### [Hexalith.KeyValueStorages](./Hexalith.KeyValueStorages/README.md)

The main package that provides both in-memory and file-based key-value storage implementations with support for optimistic concurrency control through Etags.

**Key Features:**

- Generic type support for both keys and values
- Optimistic concurrency control using Etags
- Thread-safe operations
- Asynchronous API support
- Multiple storage implementations

### [Hexalith.KeyValueStorages.Abstractions](./Hexalith.KeyValueStorages.Abstractions/README.md)

Defines the core interfaces and abstractions for implementing key/value storage solutions in the Hexalith ecosystem.

**Key Features:**

- Standardized interface for key/value storage operations
- Storage provider independence through abstraction
- Foundation for building storage-agnostic applications

### [Hexalith.KeyValueStorages.Files](./Hexalith.KeyValueStorages.Files/README.md)

Provides file-based implementations of the key-value storage interfaces with JSON serialization support.

**Key Features:**

- Persistent storage for key-value pairs using the file system
- Optimistic concurrency control to prevent data corruption
- Flexible serialization mechanism for various data types
- Thread-safe file operations for concurrent access

## Getting Started

To use these packages, add the desired NuGet package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages
```

Or for a specific implementation:

```bash
dotnet add package Hexalith.KeyValueStorages.Files
```

Refer to each package's README for detailed usage instructions and examples.
