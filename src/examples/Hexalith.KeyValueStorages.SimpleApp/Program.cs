// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Hexalith.Commons.Configurations;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents.Extensions;
using Hexalith.KeyValueStorages.Files;
using Hexalith.KeyValueStorages.Helpers;
using Hexalith.KeyValueStorages.RedisDatabase;
using Hexalith.KeyValueStorages.RedisDatabase.Extensions;
using Hexalith.KeyValueStorages.SimpleApp;
using Hexalith.KeyValueStorages.SimpleApp.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure settings from configuration
builder.Services.ConfigureSettings<KeyValueStoreSettings>(builder.Configuration);

// Get storage type from configuration
KeyValueStoreSettings settings = builder.Configuration
    .GetSection(KeyValueStoreSettings.ConfigurationName())
    .Get<KeyValueStoreSettings>() ?? new KeyValueStoreSettings();

// KeyValueStore configuration based on settings
switch (settings.StorageType)
{
    case KeyValueStorageType.Memory:
        Console.WriteLine("Using Memory storage...");
        _ = builder.Services.AddMemoryKeyValueStore("sample");
        break;

    case KeyValueStorageType.Redis:
        Console.WriteLine("Using Redis storage...");
        _ = builder.Services.ConfigureSettings<RedisKeyValueStoreSettings>(builder.Configuration);
        _ = builder.Services.AddRedisConnection();
        _ = builder.Services.AddRedisKeyValueStorage("sample");
        break;

    case KeyValueStorageType.File:
        Console.WriteLine("Using File storage...");
        _ = builder.Services.AddJsonFileKeyValueStore("sample");
        break;

    case KeyValueStorageType.Dapr:
        Console.WriteLine("Using Dapr storage...");
        _ = builder.Services.AddDaprActorKeyValueStorage<string, CountryState>(nameof(Country), nameof(Country));
        _ = builder.Services.AddDaprKeyValueStoreActor<CountryState>(nameof(Country));
        break;

    default:
        throw new NotSupportedException($"Storage type {settings.StorageType} is not supported.");
}

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