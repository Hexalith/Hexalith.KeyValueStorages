// <copyright file="IKeyValueStore{TKey,TValue,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Defines a generic asynchronous interface for a key-value storage system.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the store. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of the values in the store.</typeparam>
/// <typeparam name="TState">The type of the state associated with the values.</typeparam>
public interface IKeyValueStore<TKey, TValue, TState> : IKeyValueStore<TKey, TValue, Dictionary<string, string>, TState>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
    where TState : State<TValue>;