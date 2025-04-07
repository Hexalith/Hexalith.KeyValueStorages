// <copyright file="StoreType.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Factories;

using System;

/// <summary>
/// Represents a type of store with a specific key and value type.
/// </summary>
public record StoreType(
    string Name,
    Type KeyType,
    Type ValueType)
{
    /// <summary>
    /// Gets the name of the store type.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the type of the key.
    /// </summary>
    public Type KeyType { get; init; } = KeyType;

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public Type ValueType { get; init; } = ValueType;
}