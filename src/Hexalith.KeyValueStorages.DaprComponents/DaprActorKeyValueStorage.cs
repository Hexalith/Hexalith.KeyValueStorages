// <copyright file="DaprActorKeyValueStorage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents;

using System;
using System.Threading;
using System.Threading.Tasks;

using Dapr.Actors;
using Dapr.Actors.Client;

using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.DaprComponents.Actors;

/// <summary>
/// Dapr actor-based key-value storage implementation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TKeySerializer">The type of the key serializer.</typeparam>
/// <typeparam name="TValueSerializer">The type of the value serializer.</typeparam>
public class DaprActorKeyValueStorage<TKey, TValue, TKeySerializer, TValueSerializer>
    : IKeyValueStore<TKey, TValue, string>
    where TKey : notnull
    where TKeySerializer : IKeySerializer<TKey>, new()
    where TValueSerializer : IValueSerializer<TValue, string>, new()
{
    private readonly string _actorType;
    private readonly TKeySerializer _keySerializer;
    private readonly TValueSerializer _valueSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DaprActorKeyValueStorage{TKey, TValue, TKeySerializer, TValueSerializer}"/> class.
    /// </summary>
    /// <param name="actorType">The actor type name. If not provided, defaults to "KeyValueStoreActor".</param>
    public DaprActorKeyValueStorage(string? actorType = null)
    {
        _actorType = actorType ?? "KeyValueStoreActor";
        _keySerializer = new TKeySerializer();
        _valueSerializer = new TValueSerializer();
    }

    /// <inheritdoc/>
    public async Task<string> AddAsync(TKey key, TValue value, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        string serializedValue = _valueSerializer.Serialize(value, Guid.NewGuid().ToString());
        
        string etag = await actor.AddAsync(serializedKey, serializedValue, cancellationToken).ConfigureAwait(false);
        return etag;
    }

    /// <inheritdoc/>
    public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        
        return await actor.ContainsKeyAsync(serializedKey, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<StoreResult<TValue, string>> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        
        try
        {
            (string serializedValue, string etag) = await actor.GetWithEtagAsync(serializedKey, cancellationToken).ConfigureAwait(false);
            (TValue value, _) = _valueSerializer.Deserialize(serializedValue);
            
            return new StoreResult<TValue, string>(value, etag);
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(TKey key, string etag, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        
        return await actor.RemoveAsync(serializedKey, etag, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> SetAsync(TKey key, TValue value, string etag, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        string serializedValue = _valueSerializer.Serialize(value, etag);
        
        string newEtag = await actor.SetAsync(serializedKey, serializedValue, etag, cancellationToken).ConfigureAwait(false);
        return newEtag;
    }

    /// <inheritdoc/>
    public async Task<StoreResult<TValue, string>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken)
    {
        IKeyValueStoreActor actor = GetActor();
        string serializedKey = _keySerializer.Serialize(key);
        
        var result = await actor.TryGetValueWithEtagAsync(serializedKey, cancellationToken).ConfigureAwait(false);
        if (result == null || result.Value.Value == null || result.Value.Etag == null)
        {
            return null;
        }
        
        (TValue value, _) = _valueSerializer.Deserialize(result.Value.Value);
        return new StoreResult<TValue, string>(value, result.Value.Etag);
    }

    private IKeyValueStoreActor GetActor()
    {
        // Create a unique ID for the actor based on the store name
        // This ensures that all operations for this store go to the same actor instance
        ActorId actorId = new(nameof(DaprActorKeyValueStorage<TKey, TValue, TKeySerializer, TValueSerializer>));
        
        // Create a proxy to the actor
        IKeyValueStoreActor actor = ActorProxy.Create<IKeyValueStoreActor>(actorId, _actorType);
        return actor;
    }
}