// <copyright file="InMemoryKeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Hexalith.KeyValueStorages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Exceptions;

/// <summary>
/// An in-memory implementation of the IKeyValueStore interface.
/// </summary>
/// <typeparam name="TKey">The type of the key, must be non-null.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public class InMemoryKeyValueStore<TKey, TValue, TMetadata, TState>
    : IKeyValueStore<TKey, TValue, TMetadata, TState>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
    where TMetadata : notnull
    where TState : State<TValue, TMetadata>
{
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, TState> _store = [];
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<TKey, DateTimeOffset> _timeToLive = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryKeyValueStore{TKey, TValue, TMetadata, TState}"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    public InMemoryKeyValueStore(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);
            if (_store.ContainsKey(key))
            {
                // The key already exists and has not expired
                throw new DuplicateKeyException<TKey>(key);
            }

            string etag = string.IsNullOrWhiteSpace(value.Etag) ? UniqueIdHelper.GenerateUniqueStringId() : value.Etag;
            _store[key] = value with { Etag = etag };
            if (value.TimeToLive is not null && value.TimeToLive.Value > TimeSpan.Zero)
            {
                _timeToLive[key] = _timeProvider.GetUtcNow().Add(value.TimeToLive.Value);
            }

            return Task.FromResult(etag);
        }
    }

    /// <inheritdoc/>
    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);
            if (_store.ContainsKey(key))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public Task<TState> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);

            if (_store.TryGetValue(key, out TState? value))
            {
                return Task.FromResult(value);
            }

            throw new KeyNotFoundException($"Key {key} not found.");
        }
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);
            if (!_store.TryGetValue(key, out TState? current))
            {
                return Task.FromResult(false);
            }

            if (!string.IsNullOrWhiteSpace(etag) && etag != current.Etag)
            {
                throw new ConcurrencyException<TKey>(key, etag, current.Etag);
            }

            return Task.FromResult(_store.Remove(key));
        }
    }

    /// <inheritdoc/>
    public Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);
            if (!_store.TryGetValue(key, out TState? current))
            {
                throw new KeyNotFoundException($"Key {key} not found.");
            }

            if (!string.IsNullOrWhiteSpace(value.Etag) && value.Etag != current.Etag)
            {
                throw new ConcurrencyException<TKey>(key, value.Etag, current.Etag);
            }

            string newEtag = UniqueIdHelper.GenerateUniqueStringId();
            _store[key] = value with { Etag = newEtag };
            if (value.TimeToLive is not null && value.TimeToLive.Value > TimeSpan.Zero)
            {
                _timeToLive[key] = _timeProvider.GetUtcNow().Add(value.TimeToLive.Value);
            }
            else
            {
                _ = _timeToLive.Remove(key);
            }

            return Task.FromResult(newEtag);
        }
    }

    /// <inheritdoc/>
    public Task<TState?> TryGetValueAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            CheckTimeToLive(key);
            if (_store.TryGetValue(key, out TState? value))
            {
                return Task.FromResult<TState?>(value);
            }

            return Task.FromResult<TState?>(null);
        }
    }

    private void CheckTimeToLive(TKey key)
    {
        if (_timeToLive.TryGetValue(key, out DateTimeOffset expirationTime) && expirationTime < _timeProvider.GetUtcNow())
        {
            // The key exists and has expired
            _ = _store.Remove(key);
            _ = _timeToLive.Remove(key);
        }
    }
}