// <copyright file="KeyValueStore{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages.Helpers;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents an abstract key-value store.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="KeyValueStore{TKey, TState}"/> class.
/// </remarks>
/// <param name="settings">The settings for the key-value store.</param>
/// <param name="database">The name of the database.</param>
/// <param name="container">The name of the container.</param>
/// <param name="entity">The name of the entity.</param>
/// <param name="timeProvider">The time provider to use for managing expiration times.</param>
public abstract class KeyValueStore<TKey, TState>(
    IOptions<KeyValueStoreSettings> settings,
    string? database,
    string? container,
    string? entity,
    TimeProvider? timeProvider) : KeyValueStore(
        settings,
        database,
        container,
        GetEntity(entity),
        timeProvider), IKeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    /// <inheritdoc/>
    public abstract Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<TState> GetAsync(TKey key, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken);

    private static string GetEntity(string? entity)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            return StateHelper.GetStateName<TState>();
        }

        return entity;
    }
}