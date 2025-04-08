// <copyright file="KeyValueStoreFactory.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;

/// <summary>
/// Factory class for creating instances of key-value stores.
/// </summary>
public class KeyValueStoreFactory : IKeyValueStoreFactory
{
    /// <summary>
    /// Creates a key-value store with the specified name.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="name">The name of the key-value store. Defaults to <see cref="IKeyValueStoreFactory.Default"/>.</param>
    /// <returns>An instance of <see cref="IKeyValueStore{TKey, TState}"/>.</returns>
    public IKeyValueStore<TKey, TState> Create<TKey, TState>(string name = IKeyValueStoreFactory.Default)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase => throw new NotImplementedException();

    /// <summary>
    /// Creates a key-value store with the specified database and container.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <returns>An instance of <see cref="IKeyValueStore{TKey, TState}"/>.</returns>
    public IKeyValueStore<TKey, TState> Create<TKey, TState>(string database, string container)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase => throw new NotImplementedException();
}