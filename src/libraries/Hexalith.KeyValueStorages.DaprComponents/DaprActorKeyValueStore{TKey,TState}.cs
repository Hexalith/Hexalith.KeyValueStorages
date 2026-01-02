// <copyright file="DaprActorKeyValueStore{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Dapr.Actors.Client;

using Hexalith.Commons.StringEncoders;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents.Actors;

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
        : this(settings, database, container, entity, KeyToRfc1123, timeProvider)
    {
    }

    private string ActorType => Database + "." + Container;

    /// <summary>
    /// Converts the key to an RFC 1123 string representation for use as an actor ID.
    /// </summary>
    /// <param name="key">The key to convert.</param>
    /// <returns>The RFC 1123 string representation of the key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key is null or its string representation is null or empty.</exception>
    public static string KeyToRfc1123(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        string? keyString = key.ToString();
        if (string.IsNullOrWhiteSpace(keyString))
        {
            throw new ArgumentNullException(nameof(key), "key.ToString() cannot be null or empty");
        }

        return key.ToString()!.ToRFC1123(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc/>
    public override async Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken) => await GetActor(key).AddAsync(value, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<string> AddOrUpdateAsync(TKey key, TState value, CancellationToken cancellationToken) => await GetActor(key).AddAsync(value, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public override async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken) => (await GetActor(key).TryGetAsync(cancellationToken).ConfigureAwait(false)) != null;

    /// <inheritdoc/>
    public override Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken) => throw new NotImplementedException();

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