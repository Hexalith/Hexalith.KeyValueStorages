// <copyright file="IKeyValueStoreActor{TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Actors;

using System.Threading.Tasks;

using Dapr.Actors;
using Dapr.Actors.Generators;

/// <summary>
/// Interface for a key-value store actor.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
[GenerateActorClient]
public interface IKeyValueStoreActor<TState> : IActor
{
    /// <summary>
    /// Adds the state asynchronously.
    /// </summary>
    /// <param name="value">The state value to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<string> AddAsync(TState value, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the state asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the state value.</returns>
    Task<TState> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Removes the state asynchronously using an etag.
    /// </summary>
    /// <param name="etag">The etag of the state to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the state was removed.</returns>
    Task<bool> RemoveAsync(string etag, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the state asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the state was removed.</returns>
    Task<bool> RemoveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sets the state asynchronously.
    /// </summary>
    /// <param name="value">The state value to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<string> SetAsync(TState value, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the state asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the state value if found; otherwise, null.</returns>
    Task<TState?> TryGetAsync(CancellationToken cancellationToken);
}