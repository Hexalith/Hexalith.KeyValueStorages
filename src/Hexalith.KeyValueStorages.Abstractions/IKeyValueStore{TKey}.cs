// <copyright file="IKeyValueStore{TKey}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Defines a generic asynchronous interface for a key-value storage system.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the store. Must be non-nullable.</typeparam>
public interface IKeyValueStore<TKey> : IKeyValueStore<TKey, string, State>
    where TKey : notnull, IEquatable<TKey>;