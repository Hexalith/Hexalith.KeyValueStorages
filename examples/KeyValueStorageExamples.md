# Key-Value Storage Examples Plan

## Overview
This document outlines the plan for implementing examples that demonstrate the usage of memory and file-based key-value stores in the Hexalith.KeyValueStorages library.

## Project Setup
1. Update the example project to include a reference to the Files project:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages\Hexalith.KeyValueStorages.csproj" />
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages.Files\Hexalith.KeyValueStorages.Files.csproj" />
   </ItemGroup>
   ```

## Program Structure
The Program.cs file will be structured with the following sections:
- Main method with a simple menu to run either memory or file examples
- Memory store example section
- File store example section
- Helper methods for displaying results

## Memory Store Example
This section will demonstrate:
- Creating an in-memory key-value store with string keys and values
- Adding key-value pairs
- Retrieving values by key
- Updating existing values
- Removing key-value pairs
- Handling concurrency with Etags

```csharp
// Example code for memory store
var memoryStore = new InMemoryKeyValueStore<string, string>();
await memoryStore.AddAsync("key1", "value1", CancellationToken.None);
var result = await memoryStore.GetAsync("key1", CancellationToken.None);
// etc.
```

## File Store Example
This section will demonstrate:
- Creating a JSON file-based key-value store with string keys and values
- Adding key-value pairs (which creates files)
- Retrieving values by key (reading from files)
- Updating existing values (updating files)
- Removing key-value pairs (deleting files)
- Handling concurrency with Etags

```csharp
// Example code for file store
var fileStore = new JsonFileKeyValueStorage<string, string>("./key-value-store");
await fileStore.AddAsync("key1", "value1", CancellationToken.None);
var result = await fileStore.GetAsync("key1", CancellationToken.None);
// etc.
```

## Implementation Details
- We'll use `InMemoryKeyValueStore<string, string>` for the memory example
- We'll use `JsonFileKeyValueStorage<string, string>` for the file example
- We'll create a temporary directory for the file store examples
- We'll implement proper error handling and display results clearly
- We'll add comments to explain each operation

## Code Structure
```csharp
// Program.cs
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Files;

// Main method with menu
// Memory store example methods
// File store example methods
// Helper methods
```

## Next Steps
1. Switch to Code mode to implement the solution
2. Update the project references
3. Implement the Program.cs file with the examples
4. Test the examples