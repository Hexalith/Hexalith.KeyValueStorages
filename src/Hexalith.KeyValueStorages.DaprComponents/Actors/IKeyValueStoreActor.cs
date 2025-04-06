// <copyright file="IKeyValueStoreActor.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Actors;

using System.Threading.Tasks;

using Dapr.Actors;
using Dapr.Actors.Generators;

using Hexalith.KeyValueStorages;

/// <summary>
/// Interface for a key-value store actor.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
[GenerateActorClient]
public interface IKeyValueStoreActor<TValue, TMetadata, TState> : IActor
    where TState : State<TValue, TMetadata>
{
    /// <summary>
    /// Sets the state asynchronously.
    /// </summary>
    /// <param name="value">The state value to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAsync(TState? value, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the state asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the state value.</returns>
    Task<TState?> GetAsync(CancellationToken cancellationToken);
}