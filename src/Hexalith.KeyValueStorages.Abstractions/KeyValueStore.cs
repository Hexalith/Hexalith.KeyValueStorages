// <copyright file="KeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;

/// <summary>
/// Represents an abstract key-value store.
/// </summary>
public abstract class KeyValueStore : IKeyValueStore
{
    /// <summary>
    /// Gets the container name.
    /// </summary>
    public string Container { get; init; } = string.Empty;

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; init; } = string.Empty;

    /// <summary>
    /// Gets the time provider, using the service provider if not already set.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }
}