// <copyright file="InMemoryKeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Hexalith.KeyValueStorages.InMemory;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Exceptions;

using Microsoft.Extensions.Options;

/// <summary>
/// An in-memory implementation of the IKeyValueStore interface.
/// </summary>
/// <typeparam name="TKey">The type of the key, must be non-null.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="settings">The settings for the key-value store.</param>
/// <param name="database">The name of the database. If not provided, the setting value is used.</param>
/// <param name="container">The name of the container. If not provided, the setting value is used.</param>
/// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>"
public class InMemoryKeyValueStore<TKey, TState>(
    IOptions<KeyValueStoreSettings> settings,
    string? database,
    string? container = null,
    string? entity = null,
    TimeProvider? timeProvider = null)
        : KeyValueStore<TKey, TState>(settings, database, container, entity, timeProvider ?? TimeProvider.System)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
{
    private static readonly Lock _lock = new();
    private static readonly Dictionary<InMemoryKey<TKey>, TState> _store = [];
    private static readonly Dictionary<InMemoryKey<TKey>, DateTimeOffset> _timeToLive = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryKeyValueStore{TKey, TState}"/> class.
    /// </summary>
    public InMemoryKeyValueStore()
        : this(
        Options.Create(new KeyValueStoreSettings()),
        null,
        null,
        null,
        null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryKeyValueStore{TKey, TState}"/> class.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times. If not provided, the system time provider is used.</param>
    public InMemoryKeyValueStore(string database, string container, TimeProvider? timeProvider)
        : this(
        Options.Create(new KeyValueStoreSettings()),
        database,
        container,
        null,
        timeProvider)
    {
    }

    /// <inheritdoc/>
    public override Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);
            if (_store.ContainsKey(storeKey))
            {
                // The key already exists and has not expired
                throw new DuplicateKeyException<TKey>(key);
            }

            string etag = string.IsNullOrWhiteSpace(value.Etag) ? UniqueIdHelper.GenerateUniqueStringId() : value.Etag;
            _store[storeKey] = value with { Etag = etag };
            if (value.TimeToLive is not null && value.TimeToLive.Value > TimeSpan.Zero)
            {
                _timeToLive[storeKey] = TimeProvider.GetUtcNow().Add(value.TimeToLive.Value);
            }

            return Task.FromResult(etag);
        }
    }

    /// <inheritdoc/>
    public override Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        InMemoryKey<TKey> storeKey = GetKey(key);
        if (_store.ContainsKey(storeKey))
        {
            // The key already exists and has not expired
            return SetAsync(key, value, cancellationToken);
        }

        // The key does not exist or has expired
        return AddAsync(key, value, cancellationToken);
    }

    /// <summary>
    /// Clears all entries in the in-memory key-value store for the current database and container.
    /// </summary>
    public void Clear()
    {
        using (_lock.EnterScope())
        {
            foreach (InMemoryKey<TKey>? key in _store.Keys.Where(p => p.Database == Database && p.Container == Container))
            {
                _ = _store.Remove(key);
                _ = _timeToLive.Remove(key);
            }
        }
    }

    /// <inheritdoc/>
    public override Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);
            if (_store.ContainsKey(storeKey))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Checks if a key exists in the in-memory key-value store.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the key exists; otherwise, false.</returns>
    public override Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            if (_store.ContainsKey(storeKey))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public override Task<TState> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);

            if (_store.TryGetValue(storeKey, out TState? value))
            {
                return Task.FromResult(value);
            }

            throw new KeyNotFoundException($"Key {key} not found.");
        }
    }

    /// <inheritdoc/>
    public override Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);
            if (!_store.TryGetValue(storeKey, out TState? current))
            {
                return Task.FromResult(false);
            }

            if (!string.IsNullOrWhiteSpace(etag) && etag != current.Etag)
            {
                throw new ConcurrencyException<TKey>(key, etag, current.Etag);
            }

            return Task.FromResult(_store.Remove(storeKey));
        }
    }

    /// <inheritdoc/>
    public override Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);
            if (!_store.TryGetValue(storeKey, out TState? current))
            {
                throw new KeyNotFoundException($"Key {key} not found.");
            }

            if (!string.IsNullOrWhiteSpace(value.Etag) && value.Etag != current.Etag)
            {
                throw new ConcurrencyException<TKey>(key, value.Etag, current.Etag);
            }

            string newEtag = UniqueIdHelper.GenerateUniqueStringId();
            _store[storeKey] = value with { Etag = newEtag };
            if (value.TimeToLive is not null && value.TimeToLive.Value > TimeSpan.Zero)
            {
                _timeToLive[storeKey] = TimeProvider.GetUtcNow().Add(value.TimeToLive.Value);
            }
            else
            {
                _ = _timeToLive.Remove(storeKey);
            }

            return Task.FromResult(newEtag);
        }
    }

    /// <summary>
    /// Cleans up expired items from the in-memory store based on their time-to-live values.
    /// </summary>
    public void TimeToLiveCleanup()
    {
        using (_lock.EnterScope())
        {
            DateTimeOffset now = TimeProvider.GetUtcNow();
            foreach (InMemoryKey<TKey>? key in _timeToLive
                .Where(p => p.Value < now)
                .Select(p => p.Key))
            {
                // The key exists and has expired
                _ = _store.Remove(key);
                _ = _timeToLive.Remove(key);
            }
        }
    }

    /// <inheritdoc/>
    public override async Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TState? result = null;
        using (_lock.EnterScope())
        {
            InMemoryKey<TKey> storeKey = GetKey(key);
            CheckTimeToLive(storeKey);
            if (_store.TryGetValue(storeKey, out TState? value))
            {
                result = value;
            }
        }

        return await Task.FromResult(result).ConfigureAwait(false);
    }

    private void CheckTimeToLive(InMemoryKey<TKey> storeKey)
    {
        if (_timeToLive.TryGetValue(storeKey, out DateTimeOffset expirationTime) && expirationTime < TimeProvider.GetUtcNow())
        {
            // The key exists and has expired
            _ = _store.Remove(storeKey);
            _ = _timeToLive.Remove(storeKey);
        }
    }

    private InMemoryKey<TKey> GetKey(TKey key) => new(Database, Container, key);
}