# Hexalith Dapr Key/Value Storages

## Overview

Hexalith.KeyValueStorages.DaprComponents provides Dapr-based implementations of the key-value storage interfaces defined in Hexalith.KeyValueStorages.Abstractions. It offers a robust solution for persisting key-value pairs to the file system using JSON serialization, with built-in support for optimistic concurrency control through Etags.

## Purpose

This library serves several key purposes:

- Provides persistent storage for key-value pairs using the file system
- Implements optimistic concurrency control to prevent data corruption
- Offers a flexible serialization mechanism for various data types
- Ensures thread-safe file operations for concurrent access
- Maintains compatibility with the core Hexalith.KeyValueStorages interfaces

## Features

### Dapr-Based Persistence


### Optimistic Concurrency



## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Hexalith.KeyValueStorages.Abstractions package

### Installation

Add the NuGet package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages.DaprComponents
```

### Basic Usage

## Architecture

### Class Hierarchy

### Key Components

## Examples

### Using with Simple Types

### Using with Complex Types


## Advanced Usage

### Custom Etag Generation

### Custom Serialization Options

You can customize the JSON serialization options:


## Learn More

- [Hexalith Key/Value Stores](https://github.com/Hexalith/Hexalith.KeyValueStorages) - Main repository
- [Hexalith.KeyValueStorages.Abstractions](https://github.com/Hexalith/Hexalith.KeyValueStorages/tree/main/src/Hexalith.KeyValueStorages.Abstractions) - Core interfaces
- [Examples](https://github.com/Hexalith/Hexalith.KeyValueStorages/tree/main/examples) - Sample implementations