// <copyright file="KeyValueStoreActor{TValue,TState,TMetadata}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Actors;

using System.Threading;
using System.Threading.Tasks;

using Dapr.Actors.Runtime;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages;

/// <summary>
/// Represents an actor that provides key-value store functionality.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="KeyValueStoreActor{TValue, TState, TMetadata}"/> class.
/// </remarks>
/// <param name="host">The actor host.</param>
public class KeyValueStoreActor<TValue, TState, TMetadata>(ActorHost host) : Actor(host), IKeyValueStoreActor<TValue, TState, TMetadata>
    where TState : State<TValue, TMetadata>
{
    private const string _stateName = "State";

    /// <inheritdoc/>
    public async Task<TState?> GetAsync(CancellationToken cancellationToken)
    {
        ConditionalValue<TState> result = await StateManager.TryGetStateAsync<TState>(_stateName, cancellationToken);
        if (result.HasValue)
        {
            return result.Value;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task SetAsync(TState? value, CancellationToken cancellationToken)
    {
        if (value == null)
        {
            await StateManager.RemoveStateAsync(_stateName, cancellationToken).ConfigureAwait(false);
            return;
        }

        TState newValue = value with
        {
            Value = value.Value,
            Etag = value.Etag ?? UniqueIdHelper.GenerateUniqueStringId(),  // Generate a new Etag if not provided
            TimeToLive = value.TimeToLive,
        };

        _ = await StateManager.AddOrUpdateStateAsync(
            _stateName,
            value,
            (key, oldValue) =>
            {
                if (value.Etag != null && value.Etag != oldValue.Etag)
                {
                    throw new ConcurrencyException(
                        $"Etag mismatch for key '{key}'. Expected: {oldValue.Etag}, Actual: {value.Etag}");
                }

                return newValue;
            },
            value.TimeToLive,
            cancellationToken)
            .ConfigureAwait(false);
    }
}