// <copyright file="JsonFileKeyValueStorage{TKey,TValue,TEtag}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;

using Hexalith.KeyValueStorages;

/// <summary>
/// Represents a key-value storage that uses JSON files for persistence.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the Etag.</typeparam>
public class JsonFileKeyValueStorage<TKey, TValue, TEtag> :
    FileKeyValueStorage<
        TKey,
        TValue,
        TEtag,
        KeyToStringSerializer<TKey>,
        JsonFileSerializer<TValue, TEtag>>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
    where TEtag : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue, TEtag}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    public JsonFileKeyValueStorage(string rootPath)
        : base(rootPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue, TEtag}"/> class.
    /// </summary>
    public JsonFileKeyValueStorage()
        : base("key-value-store")
    {
    }

    /// <summary>
    /// Generates a new Etag.
    /// </summary>
    /// <returns>The generated Etag.</returns>
    protected override TEtag GenerateEtag() => throw new NotImplementedException();
}