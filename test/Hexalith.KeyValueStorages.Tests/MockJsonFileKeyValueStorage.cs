// <copyright file="MockJsonFileKeyValueStorage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages.Files;

/// <summary>
/// Mock implementation of JsonFileKeyValueStorage for testing.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class MockJsonFileKeyValueStorage<TKey, TValue> :
    FileKeyValueStorage<
        TKey,
        TValue,
        string,
        KeyToStringSerializer<TKey>,
        MockJsonFileSerializer<TValue, string>>
    where TKey : notnull, IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockJsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    public MockJsonFileKeyValueStorage(string rootPath)
        : base(rootPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockJsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    public MockJsonFileKeyValueStorage()
        : base("mock-key-value-store")
    {
    }

    /// <summary>
    /// Generates a new Etag.
    /// </summary>
    /// <returns>The generated Etag.</returns>
    protected override string GenerateEtag() => UniqueIdHelper.GenerateUniqueStringId();

    /// <summary>
    /// Writes the value to the file with the specified Etag.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="etag">The etag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected new async Task WriteValueToFileAsync(string filePath, TValue value, string etag, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        var serializer = new MockJsonFileSerializer<TValue, string>();
        await serializer.SerializeAsync(stream, value, etag, cancellationToken).ConfigureAwait(false);
    }
}