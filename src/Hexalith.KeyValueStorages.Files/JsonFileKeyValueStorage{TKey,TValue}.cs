// <copyright file="JsonFileKeyValueStorage{TKey,TValue}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;

using Hexalith.Commons.UniqueIds;

/// <summary>
/// Represents a key-value storage that uses JSON files for persistence.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class JsonFileKeyValueStorage<TKey, TValue> :
    JsonFileKeyValueStorage<
        TKey,
        TValue,
        string>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    public JsonFileKeyValueStorage(string rootPath)
        : base(rootPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    public JsonFileKeyValueStorage()
        : base("key-value-store")
    {
    }

    /// <summary>
    /// Generates a new ETag.
    /// </summary>
    /// <returns>The generated ETag.</returns>
    protected override string GenerateEtag() => UniqueIdHelper.GenerateUniqueStringId();
}