// <copyright file="JsonFileKeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Hexalith.KeyValueStorages.Files;

using System.Text.Json;

using Hexalith.KeyValueStorages;

using Microsoft.Extensions.Options;

/// <summary>
/// Factory class for creating instances of <see cref="IKeyValueStore{TKey, TState}"/>.
/// </summary>
/// <param name="settings">The settings for the key-value store.</param>
/// <param name="database">The name of the database. If not provided, the setting value is used.</param>
/// <param name="container">The name of the container. If not provided, the setting value is used.</param>
/// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
/// <param name="options">The JSON serializer options.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>
public class JsonFileKeyValueProvider(
        IOptions<KeyValueStoreSettings> settings,
        string? database,
        string? container,
        string? entity,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider) : KeyValueProvider(settings, database, container, entity, timeProvider)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueProvider"/> class.
    /// </summary>
    public JsonFileKeyValueProvider()
        : this(
            Options.Create(new KeyValueStoreSettings()),
            null,
            null,
            null,
            null,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueProvider"/> class with the specified JSON serializer options.
    /// </summary>
    /// <param name="options">The JSON serializer options to use for the key-value store.</param>
    public JsonFileKeyValueProvider(JsonSerializerOptions options)
        : this(
            Options.Create(new KeyValueStoreSettings()),
            null,
            null,
            null,
            options,
            null)
    {
    }

    /// <inheritdoc/>
    public override IKeyValueStore<TKey, TState> Create<TKey, TState>(string? database = null, string? container = null, string? entity = null) => new JsonFileKeyValueStore<TKey, TState>(
            Settings,
            database,
            container,
            entity,
            options,
            TimeProvider);
}