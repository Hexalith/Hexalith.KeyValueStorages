// <copyright file="DaprActorExample.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Example;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents;
using Hexalith.KeyValueStorages.DaprComponents.Extensions;
using Hexalith.KeyValueStorages.Exceptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Example of using the Dapr actor-based key-value store.
/// </summary>
public static class DaprActorExample
{
    /// <summary>
    /// Runs the example.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task RunAsync(string[] args)
    {
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
        var keyValueStore = app.Services.GetRequiredService<IKeyValueStore<string, Person, string>>();

        // Use the key-value store
        await UseKeyValueStoreAsync(keyValueStore);

        // Stop the application
        await app.StopAsync();
    }

    private static async Task UseKeyValueStoreAsync(IKeyValueStore<string, Person, string> keyValueStore)
    {
        // Create a person
        var person = new Person
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            BirthDate = new DateTime(1980, 1, 1)
        };

        Console.WriteLine("Adding a person to the key-value store...");

        // Add the person to the key-value store
        string etag = await keyValueStore.AddAsync(person.Id, person, CancellationToken.None);

        Console.WriteLine($"Person added with etag: {etag}");

        // Get the person from the key-value store
        StoreResult<Person, string> result = await keyValueStore.GetAsync(person.Id, CancellationToken.None);

        Console.WriteLine($"Retrieved person: {result.Value.FirstName} {result.Value.LastName}, Etag: {result.Etag}");

        // Update the person
        person.Email = "john.doe.updated@example.com";

        // Set the updated person in the key-value store
        string newEtag = await keyValueStore.SetAsync(person.Id, person, result.Etag, CancellationToken.None);

        Console.WriteLine($"Person updated with new etag: {newEtag}");

        // Try to update with the old etag (should fail)
        try
        {
            await keyValueStore.SetAsync(person.Id, person, etag, CancellationToken.None);
            Console.WriteLine("This should not happen - update with old etag should fail");
        }
        catch (ConcurrencyException ex)
        {
            Console.WriteLine($"Expected concurrency exception: {ex.Message}");
        }

        // Remove the person
        bool removed = await keyValueStore.RemoveAsync(person.Id, newEtag, CancellationToken.None);

        Console.WriteLine($"Person removed: {removed}");

        // Try to get the removed person
        StoreResult<Person, string>? tryGetResult = await keyValueStore.TryGetValueAsync(person.Id, CancellationToken.None);

        Console.WriteLine($"Try get result: {(tryGetResult == null ? "Not found" : "Found")}");
    }
}

/// <summary>
/// Represents a person.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the birth date.
    /// </summary>
    public DateTime BirthDate { get; set; }
}

/// <summary>
/// Serializer for <see cref="Person"/> objects.
/// </summary>
public class PersonSerializer : IValueSerializer<Person, string>
{
    /// <inheritdoc/>
    public string DataType => "application/json";

    /// <inheritdoc/>
    public (Person Value, string Etag) Deserialize(string value)
    {
        var document = JsonDocument.Parse(value);
        var root = document.RootElement;
        
        string etag = root.GetProperty("etag").GetString() ?? throw new InvalidOperationException("Etag is missing");
        Person? person = JsonSerializer.Deserialize<Person>(root.GetProperty("data").GetRawText()) 
            ?? throw new InvalidOperationException("Failed to deserialize person");
        
        return (person, etag);
    }

    /// <inheritdoc/>
    public async Task<(Person Value, string Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        
        string etag = root.GetProperty("etag").GetString() ?? throw new InvalidOperationException("Etag is missing");
        Person? person = JsonSerializer.Deserialize<Person>(root.GetProperty("data").GetRawText()) 
            ?? throw new InvalidOperationException("Failed to deserialize person");
        
        return (person, etag);
    }

    /// <inheritdoc/>
    public string Serialize(Person value, string etag)
    {
        var wrapper = new { data = value, etag };
        return JsonSerializer.Serialize(wrapper);
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream, Person value, string etag, CancellationToken cancellationToken)
    {
        var wrapper = new { data = value, etag };
        await JsonSerializer.SerializeAsync(stream, wrapper, cancellationToken: cancellationToken);
    }
}