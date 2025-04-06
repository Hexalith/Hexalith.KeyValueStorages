# Hexalith.KeyValueStorages Examples

This directory contains example projects demonstrating how to use the Hexalith.KeyValueStorages library in different scenarios.

## Available Examples

### [Hexalith.KeyValueStorages.JsonExample](./Hexalith.KeyValueStorages.JsonExample/README.md)

This example demonstrates how to use the Hexalith.KeyValueStorages library with JSON files as a storage mechanism.

**Key Features:**
- Creating a JSON file-based key-value store
- Adding, updating, retrieving, and deleting items from the store
- Using ETags for optimistic concurrency control
- Implementing a state wrapper pattern for metadata storage

**Main Components:**
- `Country` record - A simple data model with code, name, and currency
- `CountryState` record - A wrapper extending `State<Country, string>` with metadata
- `JsonFileKeyValueStorage` - The main component providing file-based persistence

**Running the Example:**
```bash
cd Hexalith.KeyValueStorages.JsonExample
dotnet run
```

For more detailed information, see the [example's README.md](./Hexalith.KeyValueStorages.JsonExample/README.md).