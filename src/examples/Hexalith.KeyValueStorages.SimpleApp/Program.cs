// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Hexalith.Commons.Configurations;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Files;
using Hexalith.KeyValueStorages.Helpers;
using Hexalith.KeyValueStorages.RedisDatabase;
using Hexalith.KeyValueStorages.RedisDatabase.Extensions;
using Hexalith.KeyValueStorages.SimpleApp.Components;

// Ask user to choose storage type
Console.WriteLine("Select storage type:");
Console.WriteLine("1. Memory (in-memory storage)");
Console.WriteLine("2. File (JSON file storage)");
Console.WriteLine("3. Redis (Redis database storage)");
Console.Write("Enter choice (1-3): ");

string? choice = Console.ReadLine();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// KeyValueStore configuration based on user choice
switch (choice)
{
    case "1":
        Console.WriteLine("Using Memory storage...");
        _ = builder.Services.AddMemoryKeyValueStore("sample");
        break;

    case "2":
        Console.WriteLine("Using File storage...");
        _ = builder.Services.AddJsonFileKeyValueStore("sample");
        break;

    case "3":
        Console.WriteLine("Using Redis storage...");
        _ = builder.Services.ConfigureSettings<RedisKeyValueStoreSettings>(builder.Configuration);
        _ = builder.Services.AddRedisConnection();
        _ = builder.Services.AddRedisKeyValueStorage<string, StateBase>("sample");
        break;

    default:
        Console.WriteLine("Invalid choice. Defaulting to File storage...");
        _ = builder.Services.AddJsonFileKeyValueStore("sample");
        break;
}

builder.Services.ConfigureSettings<KeyValueStoreSettings>(builder.Configuration); // Add configuration settings for the key-value store

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync().ConfigureAwait(false);