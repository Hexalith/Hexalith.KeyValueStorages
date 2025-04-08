// <copyright file="IKeyValueFactory.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;

/// <summary>
/// Factory interface for creating instances of <see cref="IKeyValueStore{TKey, TState}"/>.
/// </summary>
public interface IKeyValueFactory
{
    /// <summary>
    /// The default name for the key-value store.
    /// </summary>
    const string Default = "default";

    /// <summary>
    /// Creates a new instance of <see cref="IKeyValueStore{TKey, TState}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="name">The name of the key-value store.</param>
    /// <returns>A new instance of <see cref="IKeyValueStore{TKey, TState}"/>.</returns>
    IKeyValueStore<TKey, TState> Create<TKey, TState>(string name = Default)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase;

    /// <summary>
    /// Creates a new instance of <see cref="IKeyValueStore{TKey, TState}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <returns>A new instance of <see cref="IKeyValueStore{TKey, TState}"/>.</returns>
    IKeyValueStore<TKey, TState> Create<TKey, TState>(string database, string container)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase;
}