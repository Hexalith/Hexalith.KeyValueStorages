// <copyright file="JsonFileKeyValueStore{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.Text.Json;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents a key-value storage that uses JSON files for persistence.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonFileKeyValueStore{TKey, TValue}"/> class.
/// </remarks>
/// <param name="settings">The settings for the file key-value store.</param>
/// <param name="database">The name of the database. If not provided, the setting value is used.</param>
/// <param name="container">The name of the container. If not provided, the setting value is used.</param>
/// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
/// <param name="options">The JSON serializer options.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>
public class JsonFileKeyValueStore<TKey, TState>(
    IOptions<KeyValueStoreSettings> settings,
    string? database = null,
    string? container = null,
    string? entity = null,
    JsonSerializerOptions? options = null,
    TimeProvider? timeProvider = null) :
    FileKeyValueStorage<TKey, TState>(
        settings,
        database,
        container,
        entity,
        timeProvider)
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStore{TKey, TState}"/> class.
    /// </summary>
    public JsonFileKeyValueStore()
        : this(
            Options.Create(new KeyValueStoreSettings()),
            null,
            null,
            null,
            null,
            null)
    {
    }

    /// <inheritdoc/>
    protected override string KeyToFileName(TKey key) => $"{key.ToString() ?? throw new ArgumentNullException(nameof(key))}.json";

    /// <inheritdoc/>
    protected override async Task<TState> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        => await JsonSerializer.DeserializeAsync<TState>(stream, options, cancellationToken)
.ConfigureAwait(false) ?? throw new JsonException("Deserialization returned a null value");

    /// <inheritdoc/>
    protected override async Task WriteToStreamAsync(Stream stream, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(stream);
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}