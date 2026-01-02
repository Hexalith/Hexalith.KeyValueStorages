// <copyright file="InMemoryKey.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.InMemory;
/// <summary>
/// Represents an in-memory key for a key-value storage.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <param name="Database">The name of the database.</param>
/// <param name="Container">The name of the container.</param>
/// <param name="Key">The key value.</param>
public record InMemoryKey<TKey>(
    string Database,
    string Container,
    TKey Key);