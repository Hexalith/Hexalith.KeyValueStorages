// <copyright file="JsonFileKeyValueStorage{TKey,TState}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.Text.Json;

/// <summary>
/// Represents a key-value storage that uses JSON files for persistence.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public class JsonFileKeyValueStorage<TKey, TState> :
    FileKeyValueStorage<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <param name="subPath">The sub-path for storing files.</param>
    /// <param name="keyToFileName">The function to convert the key to a file name.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    public JsonFileKeyValueStorage(
        string rootPath,
        JsonSerializerOptions? options,
        string subPath,
        Func<TKey, string> keyToFileName,
        TimeProvider timeProvider)
        : base(
            rootPath,
            (s, c) => ReadFromStreamAsync(s, options, c),
            (s, v, c) => WriteToStreamAsync(s, v, options, c),
            (key) => (subPath, keyToFileName(key)),
            timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentNullException.ThrowIfNull(timeProvider);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileKeyValueStorage{TKey, TValue}"/> class.
    /// </summary>
    public JsonFileKeyValueStorage()
        : this(
              "/store",
              null,
              typeof(TState).Name,
              (key) => $"{key.ToString() ?? throw new ArgumentNullException(nameof(key))}.json",
              TimeProvider.System)
    {
    }

    /// <summary>
    /// Reads the state from the provided stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the state.</returns>
    protected static async Task<TState> ReadFromStreamAsync(
        Stream stream,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken) => await JsonSerializer.DeserializeAsync<TState>(stream, options, cancellationToken)
            ?? throw new JsonException("Deserialization returned a null value");

    /// <summary>
    /// Writes the state to the provided stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="value">The state value to write.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    protected static async Task WriteToStreamAsync(Stream stream, TState value, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}