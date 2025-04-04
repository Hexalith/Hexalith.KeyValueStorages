// <copyright file="IKeySerializer{TKey}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Interface for serializing keys.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public interface IKeySerializer<in TKey>
{
    /// <summary>
    /// Serializes the specified key.
    /// </summary>
    /// <param name="key">The key to serialize.</param>
    /// <returns>A string representation of the key.</returns>
    string Serialize(TKey key);

    /// <summary>
    /// Serializes the specified key asynchronously.
    /// </summary>
    /// <param name="key">The key to serialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string representation of the key.</returns>
    Task<string> SerializeAsync(TKey key, CancellationToken cancellationToken) => Task.FromResult(Serialize(key));
}