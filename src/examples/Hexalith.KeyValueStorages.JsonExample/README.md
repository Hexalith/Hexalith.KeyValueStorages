# Hexalith.KeyValueStorages.JsonExample

This example demonstrates how to use the Hexalith.KeyValueStorages library with JSON files as a storage mechanism.

## Overview

The example shows how to:
- Create a JSON file-based key-value store
- Add items to the store
- Update existing items
- Delete items from the store
- Retrieve items from the store

## Key Components

### Country Record

A simple record representing a country with:
- Code (e.g., "US", "FR")
- Name (e.g., "United States", "France")
- Currency (e.g., "USD", "EUR")

### CountryState Record

A wrapper record that extends `State<Country, string>` and includes:
- The Country value
- Optional TimeToLive for the entry
- Optional Etag for concurrency control

### JsonFileKeyValueStorage

The main component from the Hexalith.KeyValueStorages.Files library that provides:
- File-based persistence using JSON serialization
- Support for ETags to handle concurrency
- Asynchronous operations for all CRUD functions

## Operations Demonstrated

The example demonstrates the following operations:

1. **Adding entries** to the store with `AddAsync`
   - Adding France, China, United States, and Vietnam to the store
   - Each addition returns an ETag that can be used for concurrency control

2. **Updating entries** with `SetAsync`
   - Updating the United States entry with corrected information
   - Using the previously obtained ETag to ensure consistency

3. **Removing entries** with `RemoveAsync`
   - Removing the China entry from the store
   - Using the ETag to ensure the correct version is deleted

4. **Retrieving entries** with `GetAsync`
   - Getting the United States entry and displaying its information

## Running the Example

To run this example:

1. Ensure you have .NET 9.0 SDK installed
2. Navigate to the example directory
3. Run the following command:

```bash
dotnet run
```

## Expected Output

When running the example, you should see output similar to:

```
Country: United States of America, Currency: USD, Etag: [some-etag-value]
```

## Dependencies

This example depends on the following Hexalith.KeyValueStorages packages:
- Hexalith.KeyValueStorages
- Hexalith.KeyValueStorages.Files
- Hexalith.KeyValueStorages.DaprComponents

## Key Concepts

### ETags

ETags (Entity Tags) are used for optimistic concurrency control. When you add or update an item, an ETag is returned. You must provide this ETag when updating or deleting the item to ensure you're working with the latest version.

### State Wrapper

The `CountryState` class wraps the `Country` object with additional metadata like TimeToLive and ETag. This pattern allows for storing metadata alongside the actual value.

### Error Handling

The example includes basic error handling with a try-catch block to capture and display any exceptions that might occur during the operations.