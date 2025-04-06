# Key-Value Storage Examples Plan

## Overview
This document outlines the plan for implementing examples that demonstrate the usage of memory and file-based key-value stores in the Hexalith.KeyValueStorages library.

## Project Setup
1. Update the example project to include references to the Files and DaprComponents projects:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages\Hexalith.KeyValueStorages.csproj" />
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages.Files\Hexalith.KeyValueStorages.Files.csproj" />
     <ProjectReference Include="..\..\src\Hexalith.KeyValueStorages.DaprComponents\Hexalith.KeyValueStorages.DaprComponents.csproj" />
   </ItemGroup>
   ```

## Program Structure
The Program.cs file will be structured with the following sections:
- Main method with a simple menu to run either memory, file, or Dapr actor examples
- Memory store example section
- File store example section
- Dapr actor store example section
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

## Dapr Actor Store Example
This section will demonstrate:
- Creating a Dapr actor-based key-value store with string keys and complex object values
- Adding key-value pairs (which are stored in actor state)
- Retrieving values by key
- Updating existing values
- Removing key-value pairs
- Handling concurrency with Etags

```csharp
// Example code for Dapr actor store
// Create a web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDaprActorKeyValueStorage<string, Person, KeyToStringSerializer<string>, PersonSerializer>();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapActorsHandlers();
});

// Start the application
await app.StartAsync();

// Get the key-value store from the service provider
var actorStore = app.Services.GetRequiredService<IKeyValueStore<string, Person, string>>();

// Use the key-value store
await actorStore.AddAsync("person1", new Person { Name = "John Doe" }, CancellationToken.None);
var result = await actorStore.GetAsync("person1", CancellationToken.None);
// etc.
```

## Implementation Details
- We'll use `InMemoryKeyValueStore<string, string>` for the memory example
- We'll use `JsonFileKeyValueStorage<string, string>` for the file example
- We'll use `DaprActorKeyValueStorage<string, Person, KeyToStringSerializer<string>, PersonSerializer>` for the Dapr actor example
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
using Hexalith.KeyValueStorages.DaprComponents;
using Hexalith.KeyValueStorages.DaprComponents.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

// Main method with menu
// Memory store example methods
// File store example methods
// Dapr actor store example methods
// Helper methods
```

## Next Steps
1. Switch to Code mode to implement the solution
2. Update the project references
3. Implement the Program.cs file with the examples
4. Implement the DaprActorExample.cs file
5. Test the examples

## Dapr Actor Implementation Details
The Dapr actor-based key-value store implementation:
- Uses Dapr actors to ensure thread safety and concurrency control
- Stores data in the actor's state store
- Uses etags for optimistic concurrency control
- Provides serialization for complex objects
- Requires a running Dapr sidecar

### Actor Implementation
The implementation consists of:
1. An actor interface (`IKeyValueStoreActor`) that defines the operations
2. An actor implementation (`KeyValueStoreActor`) that handles the storage operations
3. A client implementation (`DaprActorKeyValueStorage`) that communicates with the actor
4. Extension methods for easy registration in ASP.NET Core applications

### Benefits of Using Actors for Key-Value Storage
- Built-in concurrency control
- Distributed state management
- Automatic state persistence
- Scalability across multiple nodes