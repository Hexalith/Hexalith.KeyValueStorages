// <copyright file="RedisKeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.RedisDatabase;

using System;
using System.Text.Json;

using Hexalith.KeyValueStorages;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

/// <summary>
/// Factory class for creating instances of <see cref="IKeyValueStore{TKey, TState}"/> backed by Redis.
/// </summary>
/// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
/// <param name="settings">The settings for the key-value store.</param>
/// <param name="database">The name of the database. If not provided, the setting value is used.</param>
/// <param name="container">The name of the container. If not provided, the setting value is used.</param>
/// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
/// <param name="jsonSerializerOptions">The JSON serializer options for serialization.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>
public class RedisKeyValueProvider(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<KeyValueStoreSettings> settings,
    string? database,
    string? container,
    string? entity,
    JsonSerializerOptions? jsonSerializerOptions,
    TimeProvider? timeProvider) : KeyValueProvider(settings, database, container, entity, timeProvider)
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly JsonSerializerOptions? _jsonSerializerOptions = jsonSerializerOptions;

    /// <inheritdoc/>
    public override IKeyValueStore<TKey, TState> Create<TKey, TState>(string? database = null, string? container = null, string? entity = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        return new RedisKeyValueStore<TKey, TState>(
            _connectionMultiplexer,
            Settings,
            database,
            container,
            entity,
            _jsonSerializerOptions,
            TimeProvider);
    }
}
