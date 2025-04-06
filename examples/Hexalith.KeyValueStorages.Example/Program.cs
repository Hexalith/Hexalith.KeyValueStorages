// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Example;

using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages.Files;

/// <summary>
/// Example program demonstrating the usage of key-value stores.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Main()
    {
        // Create an in-memory key-value store with string keys and values
        var memoryStore = new JsonFileKeyValueStorage<string, CountryState>();

        try
        {
            // Add a new country to the store
            _ = await memoryStore.AddAsync(
                "FR",
                new(new("FR", "France", "EUR")),
                CancellationToken.None);
            string cnEtag = await memoryStore.AddAsync(
                "CN",
                new(new("CN", "China", "CNY")),
                CancellationToken.None);
            string usEtag = await memoryStore.AddAsync(
                "US",
                new(new("US", "United States", "XXX")),
                CancellationToken.None);
            _ = await memoryStore.AddAsync(
                "VN",
                new(new("VN", "Vietnam", "VND")),
                CancellationToken.None);

            // Update the country information
            _ = await memoryStore.SetAsync(
                "US",
                new(new("US", "United States of America", "USD"), usEtag),
                CancellationToken.None);

            // Delete a country from the store
            _ = await memoryStore.RemoveAsync("CN", cnEtag, CancellationToken.None);

            CountryState usa = await memoryStore.GetAsync("US", CancellationToken.None);

            Console.WriteLine($"Country: {usa.Value.Name}, Currency: {usa.Value.Currency}, Etag: {usa.Etag}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in memory store examples: {ex.Message}");
        }
    }
}