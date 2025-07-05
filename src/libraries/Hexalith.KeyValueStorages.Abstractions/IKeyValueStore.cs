// <copyright file="IKeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Defines a generic asynchronous interface for a key-value storage system.
/// </summary>
public interface IKeyValueStore
{
    /// <summary>
    /// Gets the container name.
    /// </summary>
    string Container { get; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    string Database { get; }

    /// <summary>
    /// Gets the entity name.
    /// </summary>
    string? Entity { get; }
}