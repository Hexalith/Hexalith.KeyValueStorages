// <copyright file="RedisKeyValueStore{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.RedisDatabase;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Exceptions;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

/// <summary>
/// A Redis implementation of the IKeyValueStore interface.
/// </summary>
/// <typeparam name="TKey">The type of the key, must be non-null.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
/// <param name="settings">The settings for the key-value store.</param>
/// <param name="database">The name of the database. If not provided, the setting value is used.</param>
/// <param name="container">The name of the container. If not provided, the setting value is used.</param>
/// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
/// <param name="jsonSerializerOptions">The JSON serializer options for serialization.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>
public class RedisKeyValueStore<TKey, TState>(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<KeyValueStoreSettings> settings,
    string? database,
    string? container = null,
    string? entity = null,
    JsonSerializerOptions? jsonSerializerOptions = null,
    TimeProvider? timeProvider = null)
        : KeyValueStore<TKey, TState>(settings, database, container, entity, timeProvider ?? TimeProvider.System)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();

    /// <inheritdoc/>
    public override async Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();

        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        // Check if key already exists
        if (await db.KeyExistsAsync(redisKey).ConfigureAwait(false))
        {
            throw new DuplicateKeyException<TKey>(key);
        }

        string etag = value.Etag ?? UniqueIdHelper.GenerateUniqueStringId();
        TState stateWithEtag = value with { Etag = etag };
        string serializedValue = JsonSerializer.Serialize(stateWithEtag, _jsonSerializerOptions);

        TimeSpan? expiry = value.TimeToLive > TimeSpan.Zero ? value.TimeToLive : null;

        // Use SetAsync with When.NotExists to prevent race conditions
        bool success = await db.StringSetAsync(redisKey, serializedValue, expiry, when: When.NotExists).ConfigureAwait(false);

        if (!success)
        {
            throw new DuplicateKeyException<TKey>(key);
        }

        return etag;
    }

    /// <inheritdoc/>
    public override async Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();

        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        // Check etag if provided
        if (!string.IsNullOrWhiteSpace(value.Etag))
        {
            RedisValue existingValue = await db.StringGetAsync(redisKey).ConfigureAwait(false);
            if (existingValue.HasValue)
            {
                TState? currentState = JsonSerializer.Deserialize<TState>(existingValue!, _jsonSerializerOptions);
                if (currentState != null && !string.IsNullOrWhiteSpace(currentState.Etag) && value.Etag != currentState.Etag)
                {
                    throw new ConcurrencyException<TKey>(key, value.Etag, currentState.Etag);
                }
            }
        }

        string newEtag = UniqueIdHelper.GenerateUniqueStringId();
        TState stateWithEtag = value with { Etag = newEtag };
        string serializedValue = JsonSerializer.Serialize(stateWithEtag, _jsonSerializerOptions);

        TimeSpan? expiry = value.TimeToLive > TimeSpan.Zero ? value.TimeToLive : null;
        _ = await db.StringSetAsync(redisKey, serializedValue, expiry).ConfigureAwait(false);

        return newEtag;
    }

    /// <inheritdoc/>
    public override async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);
        return await db.KeyExistsAsync(redisKey).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken)
        => ContainsKeyAsync(key, cancellationToken);

    /// <inheritdoc/>
    public override async Task<TState> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        RedisValue value = await db.StringGetAsync(redisKey).ConfigureAwait(false);
        if (!value.HasValue)
        {
            throw new KeyNotFoundException($"Key {key} not found.");
        }

        return JsonSerializer.Deserialize<TState>(value!, _jsonSerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize value for key {key}.");
    }

    /// <inheritdoc/>
    public override async Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        // Check etag if provided
        if (!string.IsNullOrWhiteSpace(etag))
        {
            RedisValue existingValue = await db.StringGetAsync(redisKey).ConfigureAwait(false);
            if (!existingValue.HasValue)
            {
                return false;
            }

            TState? currentState = JsonSerializer.Deserialize<TState>(existingValue!, _jsonSerializerOptions);
            if (currentState != null && !string.IsNullOrWhiteSpace(currentState.Etag) && etag != currentState.Etag)
            {
                throw new ConcurrencyException<TKey>(key, etag, currentState.Etag);
            }
        }

        return await db.KeyDeleteAsync(redisKey).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();

        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        // Get existing value to check for existence and etag
        RedisValue existingValue = await db.StringGetAsync(redisKey).ConfigureAwait(false);
        if (!existingValue.HasValue)
        {
            throw new KeyNotFoundException($"Key {key} not found.");
        }

        TState? currentState = JsonSerializer.Deserialize<TState>(existingValue!, _jsonSerializerOptions);
        if (!string.IsNullOrWhiteSpace(value.Etag) && currentState != null && value.Etag != currentState.Etag)
        {
            throw new ConcurrencyException<TKey>(key, value.Etag, currentState.Etag);
        }

        string newEtag = UniqueIdHelper.GenerateUniqueStringId();
        TState stateWithEtag = value with { Etag = newEtag };
        string serializedValue = JsonSerializer.Serialize(stateWithEtag, _jsonSerializerOptions);

        TimeSpan? expiry = value.TimeToLive > TimeSpan.Zero ? value.TimeToLive : null;
        _ = await db.StringSetAsync(redisKey, serializedValue, expiry).ConfigureAwait(false);

        return newEtag;
    }

    /// <inheritdoc/>
    public override async Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IDatabase db = _connectionMultiplexer.GetDatabase();
        string redisKey = GetRedisKey(key);

        RedisValue value = await db.StringGetAsync(redisKey).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<TState>(value!, _jsonSerializerOptions);
    }

    /// <summary>
    /// Gets the Redis key for the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The Redis key string.</returns>
    private string GetRedisKey(TKey key)
        => $"{Database}:{Container}:{Entity}:{StateBase.KeyToRfc1123(key)}";
}
