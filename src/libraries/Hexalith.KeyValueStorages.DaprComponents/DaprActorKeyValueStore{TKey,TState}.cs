// <copyright file="DaprActorKeyValueStore{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapr.Actors.Client;

using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents.Actors;
using Hexalith.KeyValueStorages.Exceptions;

using Microsoft.Extensions.Options;

/// <summary>
/// Dapr actor-based key-value storage implementation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public class DaprActorKeyValueStore<TKey, TState>
    : KeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    private readonly Func<TKey, string> _keyToActorId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DaprActorKeyValueStore{TKey, TState}"/> class.
    /// </summary>
    /// <param name="settings">The settings for the key-value store.</param>
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
    /// <param name="keyToActorId">The function to convert the key to an actor ID.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    public DaprActorKeyValueStore(
        IOptions<KeyValueStoreSettings> settings,
        string? database,
        string? container,
        string? entity,
        Func<TKey, string> keyToActorId,
        TimeProvider? timeProvider = null)
        : base(settings, database, container, entity, timeProvider)
    {
        ArgumentNullException.ThrowIfNull(keyToActorId);
        _keyToActorId = keyToActorId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaprActorKeyValueStore{TKey, TState}"/> class.
    /// </summary>
    /// <param name="settings">The settings for the key-value store.</param>
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    public DaprActorKeyValueStore(
        IOptions<KeyValueStoreSettings> settings,
        string? database,
        string? container,
        string? entity,
        TimeProvider? timeProvider = null)
        : this(settings, database, container, entity, StateBase.KeyToRfc1123, timeProvider)
    {
    }

    private string ActorType => Database + "." + Container;

    /// <inheritdoc/>
    public override async Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken) => await GetActor(key).AddAsync(value, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor<TState> actor = GetActor(key);
        TState? existing = await actor.TryGetAsync(cancellationToken).ConfigureAwait(false);
        return existing is null
            ? await actor.AddAsync(value, cancellationToken).ConfigureAwait(false)
            : await actor.SetAsync(value, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Best effort roolback")]
    public override async Task<IReadOnlyList<string>> AddRangeAsync(
        IEnumerable<(TKey Key, TState Value)> items,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        cancellationToken.ThrowIfCancellationRequested();

        List<(TKey Key, TState Value)> itemList = [.. items];

        // Handle empty batch
        if (itemList.Count == 0)
        {
            return [];
        }

        // Phase 1: Pre-validate - check for existing keys in store
        List<TKey> existingKeys = [];
        foreach ((TKey key, _) in itemList)
        {
            IKeyValueStoreActor<TState> actor = GetActor(key);
            if (await actor.TryGetAsync(cancellationToken).ConfigureAwait(false) is not null)
            {
                existingKeys.Add(key);
            }
        }

        if (existingKeys.Count > 0)
        {
            throw new BatchAddException<TKey>(
                existingKeys,
                BatchAddFailureReason.DuplicateKeysInStore);
        }

        // Phase 2: Add all items sequentially (Dapr actors don't support cross-actor transactions)
        List<(TKey Key, string Etag)> addedItems = [];

        try
        {
            foreach ((TKey key, TState value) in itemList)
            {
                ArgumentNullException.ThrowIfNull(value);

                IKeyValueStoreActor<TState> actor = GetActor(key);
                string etag = await actor.AddAsync(value, cancellationToken).ConfigureAwait(false);
                addedItems.Add((key, etag));
            }

            return [.. addedItems.Select(i => i.Etag)];
        }
        catch (Exception)
        {
            // Compensating rollback: remove any items that were added
            foreach ((TKey key, _) in addedItems)
            {
                try
                {
                    IKeyValueStoreActor<TState> actor = GetActor(key);
                    _ = await actor.RemoveAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Best effort rollback
                }
            }

            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken) => (await GetActor(key).TryGetAsync(cancellationToken).ConfigureAwait(false)) is not null;

    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken) => (await GetActor(key).TryGetAsync(cancellationToken).ConfigureAwait(false)) is not null;

    /// <inheritdoc/>
    public override async Task<TState> GetAsync(TKey key, CancellationToken cancellationToken) => await GetActor(key).GetAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken)
        => string.IsNullOrWhiteSpace(etag)
            ? await GetActor(key).RemoveAsync(cancellationToken).ConfigureAwait(false)
            : await GetActor(key).RemoveAsync(etag, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken) => await GetActor(key).SetAsync(value, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken)
        => await GetActor(key).TryGetAsync(cancellationToken).ConfigureAwait(false);

    private IKeyValueStoreActor<TState> GetActor(TKey key)
        => ActorProxy.Create<IKeyValueStoreActor<TState>>(new(_keyToActorId(key)), ActorType);
}