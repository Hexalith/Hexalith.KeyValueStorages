// <copyright file="JsonFileKeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Hexalith.KeyValueStorages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Factory class for creating instances of <see cref="IKeyValueStore{TKey, TState}"/>.
/// </summary>
public class JsonFileKeyValueProvider : IKeyValueProvider
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for managing dependencies.</param>
    public JsonFileKeyValueProvider([NotNull] IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IKeyValueStore<TKey, TState> Create<TKey, TState>(string database, string? container)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        JsonSerializerOptions? options = _serviceProvider.GetService<JsonSerializerOptions>();
        var store = new JsonFileKeyValueStore<TKey, TState>(
            _serviceProvider.GetRequiredService<IOptions<FileKeyValueStoreSettings>>(),
            database,
            container,
            options ?? JsonSerializerOptions.Default,
            _serviceProvider.GetRequiredService<TimeProvider>());
        return store;
    }
}