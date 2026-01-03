// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Example;

using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages.Files;
using Hexalith.KeyValueStorages.InMemory;
using Hexalith.KeyValueStorages.RedisDatabase;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

/// <summary>
/// Example program demonstrating the usage of key-value stores.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Sample")]
    public static async Task Main()
    {
        // Ask user to choose storage type
        Console.WriteLine("Select storage type:");
        Console.WriteLine("1. Memory (in-memory storage)");
        Console.WriteLine("2. File (JSON file storage)");
        Console.WriteLine("3. Redis (Redis database storage)");
        Console.Write("Enter choice (1-3): ");

        string? choice = Console.ReadLine();

        IKeyValueStore<string, CountryState> store = choice switch
        {
            "1" => CreateMemoryStore(),
            "3" => CreateRedisStore(),
            _ => CreateFileStore(),
        };

        try
        {
            // Add a new country to the store
            _ = await store.AddAsync(
                "FR",
                new(new("FR", "France", "EUR")),
                CancellationToken.None).ConfigureAwait(false);
            string cnEtag = await store.AddAsync(
                "CN",
                new(new("CN", "China", "CNY")),
                CancellationToken.None).ConfigureAwait(false);
            string usEtag = await store.AddAsync(
                "US",
                new(new("US", "United States", "XXX")),
                CancellationToken.None).ConfigureAwait(false);
            _ = await store.AddAsync(
                "VN",
                new(new("VN", "Vietnam", "VND")),
                CancellationToken.None).ConfigureAwait(false);

            // Update the country information
            _ = await store.SetAsync(
                "US",
                new(new("US", "United States of America", "USD"), null, usEtag),
                CancellationToken.None).ConfigureAwait(false);

            // Delete a country from the store
            _ = await store.RemoveAsync("CN", cnEtag, CancellationToken.None).ConfigureAwait(false);

            CountryState usa = await store.GetAsync("US", CancellationToken.None).ConfigureAwait(false);

            Console.WriteLine($"Country: {usa.Value.Name}, Currency: {usa.Value.Currency}, Etag: {usa.Etag}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in store examples: {ex.Message}");
        }
    }

    private static JsonFileKeyValueStore<string, CountryState> CreateFileStore()
    {
        JsonFileKeyValueStore<string, CountryState> store = new();
        Console.WriteLine("Using File storage...");
        Console.WriteLine("Files will be created in: " + store.GetDirectoryPath());
        return store;
    }

    private static InMemoryKeyValueStore<string, CountryState> CreateMemoryStore()
    {
        Console.WriteLine("Using Memory storage...");
        return new InMemoryKeyValueStore<string, CountryState>();
    }

    private static RedisKeyValueStore<string, CountryState> CreateRedisStore()
    {
        const string connectionString = "localhost:6379";
        Console.WriteLine("Using Redis storage...");
        Console.WriteLine("Connecting to: " + connectionString);
        IConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionString);
        IOptions<KeyValueStoreSettings> settings = Options.Create(new KeyValueStoreSettings());
        return new RedisKeyValueStore<string, CountryState>(connection, settings, "example");
    }
}