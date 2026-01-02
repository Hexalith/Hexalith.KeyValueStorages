// <copyright file="KeyValueStoreActor{TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Actors;

using System.Threading;
using System.Threading.Tasks;

using Dapr.Actors.Runtime;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Exceptions;

/// <summary>
/// Represents an actor that provides key-value store functionality.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="KeyValueStoreActor{TState}"/> class.
/// </remarks>
/// <param name="host">The actor host.</param>
public class KeyValueStoreActor<TState>(ActorHost host)
    : Actor(host),
    IKeyValueStoreActor<TState>
    where TState : StateBase
{
    private const string _stateName = "State";
    private TState? _state;
    private bool _stateLoaded;

    /// <inheritdoc/>
    public async Task<string> AddAsync(TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        TState newValue = value with
        {
            Etag = value.Etag ?? UniqueIdHelper.GenerateUniqueStringId(),  // Generate a new Etag if not provided
        };
        if (value.TimeToLive is not null)
        {
            await StateManager.AddStateAsync(
                _stateName,
                newValue,
                value.TimeToLive.Value,
                cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await StateManager.AddStateAsync(
                _stateName,
                newValue,
                cancellationToken)
                .ConfigureAwait(false);
        }

        _state = newValue;
        _stateLoaded = true;
        return newValue.Etag;
    }

    /// <inheritdoc/>
    public async Task<TState> GetAsync(CancellationToken cancellationToken) => await TryGetAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"State not found for identifier: {Id}");

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string etag, CancellationToken cancellationToken)
    {
        ConditionalValue<TState> state = await StateManager.TryGetStateAsync<TState>(_stateName, cancellationToken).ConfigureAwait(false);
        if (state.HasValue && etag != state.Value.Etag)
        {
            throw new ConcurrencyException<string>(_stateName, state.Value.Etag ?? string.Empty, etag);
        }

        _stateLoaded = false;
        _state = null;

        return await StateManager.TryRemoveStateAsync(_stateName, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(CancellationToken cancellationToken)
    {
        _stateLoaded = false;
        _state = null;
        return await StateManager.TryRemoveStateAsync(_stateName, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> SetAsync(TState value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        TState current = await GetAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(value.Etag) &&
            !string.IsNullOrWhiteSpace(current.Etag) &&
            current.Etag != value.Etag)
        {
            throw new ConcurrencyException<string>(_stateName, current.Etag ?? string.Empty, value.Etag);
        }

        TState newValue = value with
        {
            Etag = UniqueIdHelper.GenerateUniqueStringId(),  // Generate a new Etag
        };
        if (value.TimeToLive is not null)
        {
            await StateManager.SetStateAsync(
                _stateName,
                value,
                value.TimeToLive.Value,
                cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await StateManager.SetStateAsync(
                _stateName,
                value,
                cancellationToken)
                .ConfigureAwait(false);
        }

        _state = newValue;
        _stateLoaded = true;
        return newValue.Etag;
    }

    /// <inheritdoc/>
    public async Task<TState?> TryGetAsync(CancellationToken cancellationToken)
    {
        if (_stateLoaded)
        {
            return _state;
        }

        ConditionalValue<TState> result = await StateManager.TryGetStateAsync<TState>(_stateName, cancellationToken).ConfigureAwait(false);
        if (result.HasValue)
        {
            _state = result.Value;
            _stateLoaded = true;
            return result.Value;
        }

        _stateLoaded = false;
        _state = null;

        return null;
    }
}