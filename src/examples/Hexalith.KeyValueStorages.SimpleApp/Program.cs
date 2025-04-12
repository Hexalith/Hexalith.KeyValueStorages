// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Hexalith.Commons.Configurations;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Files;
using Hexalith.KeyValueStorages.SimpleApp.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// KeyValueStore configuration
builder.Services.AddJsonFileKeyValueStore("sample"); // Add json file storage for key-value pairs
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

await app.RunAsync();