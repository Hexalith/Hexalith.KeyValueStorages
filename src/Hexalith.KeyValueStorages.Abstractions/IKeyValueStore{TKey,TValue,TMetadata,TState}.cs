// <copyright file="IKeyValueStore{TKey,TValue,TMetadata,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Defines a generic asynchronous interface for a key-value storage system.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the store. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of the values in the store.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata associated with the values.</typeparam>
/// <typeparam name="TState">The type of the state associated with the values.</typeparam>
public interface IKeyValueStore<TKey, TValue, TMetadata, TState>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
    where TMetadata : notnull
    where TState : State<TValue, TMetadata>
{
    /// <summary>
    /// Asynchronously adds a new key/value pair.
    /// </summary>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns the Etag.</returns>
    Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously determines whether the store contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result is true if the store contains an element with the specified key; otherwise, false.</returns>
    Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the store.</exception>
    Task<TState> GetAsync(TKey key, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously removes the element with the specified key from the store.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="etag">The Etag associated with the value. If null, the element will be removed regardless of its Etag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result is true if the element is successfully found and removed; otherwise, false.</returns>
    Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously adds a new key/value pair or updates the value if the key already exists.
    /// </summary>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns the Etag.</returns>
    Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the value associated with the key, or null if the key is not found.</returns>
    Task<TState?> TryGetValueAsync(TKey key, CancellationToken cancellationToken);
}