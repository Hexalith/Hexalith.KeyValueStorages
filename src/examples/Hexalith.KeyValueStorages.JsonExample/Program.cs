// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.JsonExample;

using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages.Files;

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
        // Create JSON file key-value store with string keys and values
        JsonFileKeyValueStore<string, CountryState> jsonStore = new();

        try
        {
            Console.WriteLine("JSON File Key-Value Store Examples are created in : " + jsonStore.GetDirectoryPath());

            // Add a new country to the store
            _ = await jsonStore.AddAsync(
                "FR",
                new(new("FR", "France", "EUR")),
                CancellationToken.None).ConfigureAwait(false);
            string cnEtag = await jsonStore.AddAsync(
                "CN",
                new(new("CN", "China", "CNY")),
                CancellationToken.None).ConfigureAwait(false);
            string usEtag = await jsonStore.AddAsync(
                "US",
                new(new("US", "United States", "XXX")),
                CancellationToken.None).ConfigureAwait(false);
            _ = await jsonStore.AddAsync(
                "VN",
                new(new("VN", "Vietnam", "VND")),
                CancellationToken.None).ConfigureAwait(false);

            // Update the country information
            _ = await jsonStore.SetAsync(
                "US",
                new(new("US", "United States of America", "USD"), null, usEtag),
                CancellationToken.None).ConfigureAwait(false);

            // Delete a country from the store
            _ = await jsonStore.RemoveAsync("CN", cnEtag, CancellationToken.None).ConfigureAwait(false);

            CountryState usa = await jsonStore.GetAsync("US", CancellationToken.None).ConfigureAwait(false);

            Console.WriteLine($"Country: {usa.Value.Name}, Currency: {usa.Value.Currency}, Etag: {usa.Etag}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in json store examples: {ex.Message}");
        }
    }
}