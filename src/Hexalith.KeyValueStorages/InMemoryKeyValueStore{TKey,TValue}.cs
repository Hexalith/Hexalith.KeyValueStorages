// <copyright file="InMemoryKeyValueStore{TKey,TValue}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Represents an in-memory key-value store with automatic ETag generation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class InMemoryKeyValueStore<TKey, TValue> : InMemoryKeyValueStore<TKey, TValue, long>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Generates the initial ETag value.
    /// </summary>
    /// <returns>The initial ETag value.</returns>
    protected override long GenerateInitialEtag() => 1L;

    /// <summary>
    /// Generates the next ETag value based on the previous ETag.
    /// </summary>
    /// <param name="previousEtag">The previous ETag value.</param>
    /// <returns>The next ETag value.</returns>
    protected override long GenerateNextEtag(long previousEtag) => previousEtag + 1;
}