// <copyright file="InMemoryKeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Hexalith.KeyValueStorages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// An in-memory implementation of the IKeyValueStore interface.
/// </summary>
/// <typeparam name="TKey">The type of the key, must be non-null.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the etag, must be non-null.</typeparam>
public abstract class InMemoryKeyValueStore<TKey, TValue, TEtag> : IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
{
    private readonly Lock _lock = new();
    private readonly Dictionary<TKey, (TValue Value, TEtag Etag)> _store = [];

    /// <inheritdoc/>
    public Task<TEtag> AddAsync(TKey key, TValue value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            if (_store.ContainsKey(key))
            {
                throw new InvalidOperationException($"Key {key} already exists.");
            }

            TEtag etag = GenerateInitialEtag();
            _store[key] = (value, etag);
            return Task.FromResult(etag);
        }
    }

    /// <inheritdoc/>
    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            return Task.FromResult(_store.ContainsKey(key));
        }
    }

    /// <inheritdoc/>
    public Task<StoreResult<TValue, TEtag>> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            if (_store.TryGetValue(key, out (TValue Value, TEtag Etag) pair))
            {
                return Task.FromResult(new StoreResult<TValue, TEtag>(pair.Value, pair.Etag));
            }

            throw new KeyNotFoundException($"Key {key} not found.");
        }
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            return Task.FromResult(_store.Remove(key));
        }
    }

    /// <inheritdoc/>
    public Task<TEtag> SetAsync(TKey key, TValue value, TEtag etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            if (!_store.TryGetValue(key, out (TValue Value, TEtag Etag) current))
            {
                throw new KeyNotFoundException($"Key {key} not found.");
            }

            if (!EqualityComparer<TEtag>.Default.Equals(current.Etag, etag))
            {
                throw new InvalidOperationException($"Etag mismatch for key {key}.");
            }

            TEtag newEtag = GenerateNextEtag(etag);
            _store[key] = (value, newEtag);
            return Task.FromResult(newEtag);
        }
    }

    /// <inheritdoc/>
    public Task<StoreResult<TValue, TEtag>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (_lock.EnterScope())
        {
            if (_store.TryGetValue(key, out (TValue Value, TEtag Etag) pair))
            {
                return Task.FromResult<StoreResult<TValue, TEtag>?>(new StoreResult<TValue, TEtag>(pair.Value, pair.Etag));
            }

            return Task.FromResult<StoreResult<TValue, TEtag>?>(null);
        }
    }

    /// <summary>
    /// Generates the initial etag for a new key-value pair.
    /// </summary>
    /// <returns>The initial etag.</returns>
    protected abstract TEtag GenerateInitialEtag();

    /// <summary>
    /// Generates the next etag based on the previous etag.
    /// </summary>
    /// <param name="previousEtag">The previous etag.</param>
    /// <returns>The next etag.</returns>
    protected abstract TEtag GenerateNextEtag(TEtag previousEtag);
}