# Hexalith KeyValueStorages Simple Example

This document provides a simple example of how to use the Hexalith KeyValueStorages library, specifically the `JsonFileKeyValueStorage`, to create and manage key-value pairs stored in JSON files. The example demonstrates basic CRUD (Create, Read, Update, Delete) operations using a `Country` record.

## Data Model (`Country.cs`)

The example uses a simple `Country` record to store information:

```csharp
using System.Runtime.Serialization;

[DataContract]
public record Country(
    [property: DataMember(Order = 1)] string Code,
    [property: DataMember(Order = 2)] string Name,
    [property: DataMember(Order = 3)] string Currency);
```

## Example Usage (`Program.cs`)

The main program demonstrates the following operations:

1.  **Initialization**: Creates an instance of `JsonFileKeyValueStorage<string, Country>`, specifying the base path `/Tests/KeyValueStorages/Country` where the JSON files will be stored. Each key-value pair will be stored in a separate JSON file within this directory structure.
2.  **Add**: Adds several `Country` records to the store using the `AddAsync` method. The country code (e.g., "FR", "CN") is used as the key. The method returns an ETag (entity tag) which represents the version of the stored item.
3.  **Update**: Updates the information for the "US" country using the `SetAsync` method. The ETag obtained during the `AddAsync` operation is required for optimistic concurrency control, ensuring that we are updating the expected version of the item.
4.  **Remove**: Deletes the "CN" country from the store using the `RemoveAsync` method. Similar to `SetAsync`, the ETag is required for concurrency control.
5.  **Get**: Retrieves the updated "US" country information using the `GetAsync` method. The result includes the `Country` object and its current ETag.
6.  **Output**: Prints the retrieved country's name, currency, and ETag to the console.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Files;

public static class Program
{
    public static async Task Main()
    {
        var fileStore = new JsonFileKeyValueStorage<string, Country>("/Tests/KeyValueStorages/Country");

        // Add new countries to the store
        _ = await fileStore.AddAsync("FR", new("FR", "France", "EUR"), CancellationToken.None);
        string cnEtag = await fileStore.AddAsync("CN", new("CN", "China", "CNY"), CancellationToken.None);
        string usEtag = await fileStore.AddAsync("US", new("US", "United States", "XXX"), CancellationToken.None);
        _ = await fileStore.AddAsync("VN", new("VN", "Vietnam", "VND"), CancellationToken.None);

        // Update the country information for US
        _ = await fileStore.SetAsync("US", new("US", "United States of America", "USD"), usEtag, CancellationToken.None);
        Console.WriteLine("US country updated.");

        // Remove CN country from the store
        _ = await fileStore.RemoveAsync("CN", cnEtag, CancellationToken.None);
        Console.WriteLine("CN country removed.");

        // Retrieve the updated US country information
        StoreResult<Country, string> usa = await fileStore.GetAsync("US", CancellationToken.None);
        Console.WriteLine($"Retrieved Country: {usa.Value.Name}, Currency: {usa.Value.Currency}, Etag: {usa.Etag}");

    }
}
```

## Running the Example

To run this example:

1.  Ensure you have the .NET SDK installed.
2.  Navigate to the `examples/Hexalith.KeyValueStorages.Example` directory in your terminal.
3.  Run the command: `dotnet run`

## Expected Output

After running the program, you should see files FR.json, US.json and VN.json in the `/Tests/KeyValueStorages/Country` directory, and the US.json content will look like this:

```json
{
  "Value": {
    "Code": "US",
    "Name": "United States of America",
    "Currency": "USD"
  },
  "Etag": "zkL6yItLZk2bN4hbvejleQ"
}
```

*(Note: The actual ETag value will vary)*
