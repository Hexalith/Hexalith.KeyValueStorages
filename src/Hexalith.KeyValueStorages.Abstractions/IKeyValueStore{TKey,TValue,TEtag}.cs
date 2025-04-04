// <copyright file="IKeyValueStore{TKey,TValue,TEtag}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Defines a generic asynchronous interface for a key-value storage system.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the store. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of the values in the store.</typeparam>
/// <typeparam name="TEtag">The type of the ETag associated with the values. Must be non-nullable.</typeparam>
public interface IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
{
    /// <summary>
    /// Asynchronously adds a new key/value pair.
    /// </summary>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns the Etag.</returns>
    Task<TEtag> AddAsync(TKey key, TValue value, CancellationToken cancellationToken);

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
    Task<StoreResult<TValue, TEtag>> GetAsync(TKey key, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously removes the element with the specified key from the store.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result is true if the element is successfully found and removed; otherwise, false.</returns>
    Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously adds a new key/value pair or updates the value if the key already exists.
    /// </summary>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="etag">The ETag associated with the value. If null, a new ETag will be generated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Returns the Etag.</returns>
    Task<TEtag> SetAsync(TKey key, TValue value, TEtag etag, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the value associated with the key, or null if the key is not found.</returns>
    Task<StoreResult<TValue, TEtag>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken);
}