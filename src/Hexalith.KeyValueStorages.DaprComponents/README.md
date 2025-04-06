# Hexalith.KeyValueStorages.DaprComponents

This library provides a Dapr actor-based implementation of the key-value storage interface. It uses Dapr actors to ensure concurrency control and data consistency.

## Features

- Thread-safe key-value storage using Dapr actors
- Automatic concurrency control with etags
- Serialization support for keys and values
- Easy integration with ASP.NET Core and Dapr applications

## Getting Started

### Prerequisites

- .NET 9.0 or later
- Dapr runtime installed and configured

### Installation

Add the package to your project:

```bash
dotnet add package Hexalith.KeyValueStorages.DaprComponents
```

### Usage

#### 1. Register the actor and key-value storage in your ASP.NET Core application

```csharp
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register the Dapr actor key-value storage
builder.Services.AddDaprActorKeyValueStorage<string, MyData, KeyToStringSerializer<string>, MyDataSerializer>();

var app = builder.Build();

// Configure the Dapr actors middleware
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapActorsHandlers();
});

app.Run();
```

#### 2. Create a value serializer

```csharp
using System.Text.Json;
using Hexalith.KeyValueStorages;

public class MyDataSerializer : IValueSerializer<MyData, string>
{
    public string DataType => "application/json";

    public (MyData Value, string Etag) Deserialize(string value)
    {
        var document = JsonDocument.Parse(value);
        var root = document.RootElement;
        
        string etag = root.GetProperty("etag").GetString() ?? throw new InvalidOperationException("Etag is missing");
        MyData data = JsonSerializer.Deserialize<MyData>(root.GetProperty("data").GetRawText()) 
            ?? throw new InvalidOperationException("Failed to deserialize data");
        
        return (data, etag);
    }

    public async Task<(MyData Value, string Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        
        string etag = root.GetProperty("etag").GetString() ?? throw new InvalidOperationException("Etag is missing");
        MyData data = JsonSerializer.Deserialize<MyData>(root.GetProperty("data").GetRawText()) 
            ?? throw new InvalidOperationException("Failed to deserialize data");
        
        return (data, etag);
    }

    public string Serialize(MyData value, string etag)
    {
        var wrapper = new { data = value, etag };
        return JsonSerializer.Serialize(wrapper);
    }

    public async Task SerializeAsync(Stream stream, MyData value, string etag, CancellationToken cancellationToken)
    {
        var wrapper = new { data = value, etag };
        await JsonSerializer.SerializeAsync(stream, wrapper, cancellationToken: cancellationToken);
    }
}
```

#### 3. Use the key-value storage in your application

```csharp
public class MyService
{
    private readonly IKeyValueStore<string, MyData, string> _storage;

    public MyService(IKeyValueStore<string, MyData, string> storage)
    {
        _storage = storage;
    }

    public async Task SaveDataAsync(string key, MyData data, CancellationToken cancellationToken)
    {
        // Add new data
        string etag = await _storage.AddAsync(key, data, cancellationToken);
        
        // Update existing data
        data.UpdatedAt = DateTime.UtcNow;
        string newEtag = await _storage.SetAsync(key, data, etag, cancellationToken);
        
        // Get data
        StoreResult<MyData, string> result = await _storage.GetAsync(key, cancellationToken);
        Console.WriteLine($"Data: {result.Value}, Etag: {result.Etag}");
    }
}
```

## How It Works

The Dapr actor-based key-value storage uses Dapr actors to ensure that only one operation can be performed on a key at a time. This prevents race conditions and ensures data consistency.

1. Each key-value pair is stored in the actor's state
2. The actor handles concurrency control using etags
3. The actor ensures that only one operation can be performed on a key at a time

## Configuration

You can configure the actor settings when registering the key-value storage:

```csharp
builder.Services.AddDaprActorKeyValueStorage<string, MyData, KeyToStringSerializer<string>, MyDataSerializer>(
    actorTypeName: "CustomKeyValueStoreActor");
```

## Advanced Usage

### Custom Actor Type Name

You can specify a custom actor type name when registering the key-value storage:

```csharp
builder.Services.AddDaprActorKeyValueStorage<string, MyData, KeyToStringSerializer<string>, MyDataSerializer>(
    actorTypeName: "CustomKeyValueStoreActor");
```

### Custom Key Serializer

You can create a custom key serializer by implementing the `IKeySerializer<TKey>` interface:

```csharp
public class CustomKeySerializer<TKey> : IKeySerializer<TKey>
    where TKey : notnull
{
    public string Serialize(TKey key)
    {
        // Custom serialization logic
        return key.ToString() ?? throw new InvalidOperationException("Key cannot be null");
    }
}
```

### Custom Value Serializer

You can create a custom value serializer by implementing the `IValueSerializer<TValue, TEtag>` interface:

```csharp
public class CustomValueSerializer<TValue> : IValueSerializer<TValue, string>
{
    public string DataType => "application/custom";

    public (TValue Value, string Etag) Deserialize(string value)
    {
        // Custom deserialization logic
        // ...
    }

    public Task<(TValue Value, string Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken)
    {
        // Custom async deserialization logic
        // ...
    }

    public string Serialize(TValue value, string etag)
    {
        // Custom serialization logic
        // ...
    }

    public Task SerializeAsync(Stream stream, TValue value, string etag, CancellationToken cancellationToken)
    {
        // Custom async serialization logic
        // ...
    }
}