// <copyright file="IKeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;

/// <summary>
/// Provides functionality to create key-value store instances.
/// </summary>
public interface IKeyValueProvider
{
    /// <summary>
    /// Creates a key-value store for the specified database and container.
    /// </summary>
    /// <typeparam name="TKey">The type of the key. Must implement <see cref="IEquatable{TKey}"/> and be non-nullable.</typeparam>
    /// <typeparam name="TState">The type of the state. Must inherit from <see cref="StateBase"/>.</typeparam>
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity.</param>
    /// <returns>An instance of <see cref="IKeyValueStore{TKey, TState}"/>.</returns>
    IKeyValueStore<TKey, TState> Create<TKey, TState>(string? database = null, string? container = null, string? entity = null)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase;
}