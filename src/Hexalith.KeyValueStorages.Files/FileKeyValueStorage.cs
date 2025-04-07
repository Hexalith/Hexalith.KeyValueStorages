// <copyright file="FileKeyValueStorage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.KeyValueStorages;
using Hexalith.KeyValueStorages.Exceptions;

/// <summary>
/// File based key-value storage implementation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public abstract class FileKeyValueStorage<TKey, TState>
    : KeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    private readonly Func<TKey, string> _keyToFileName;
    private readonly Func<Stream, CancellationToken, Task<TState>> _readFromStream;
    private readonly string _rootPath;
    private readonly Func<Stream, TState, CancellationToken, Task> _writeToStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileKeyValueStorage{TKey, TState}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="readFromStream">The function to read the value from the file.</param>
    /// <param name="writeToStream">The function to write the value to the file.</param>
    /// <param name="keyToFileName">The function to convert the key to a file name.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    protected FileKeyValueStorage(
        string rootPath,
        string database,
        string? container,
        Func<Stream, CancellationToken, Task<TState>> readFromStream,
        Func<Stream, TState, CancellationToken, Task> writeToStream,
        Func<TKey, string> keyToFileName,
        TimeProvider? timeProvider)
        : base(database, container, timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentNullException.ThrowIfNull(readFromStream);
        ArgumentNullException.ThrowIfNull(writeToStream);
        ArgumentNullException.ThrowIfNull(keyToFileName);
        _rootPath = rootPath;
        _readFromStream = readFromStream;
        _writeToStream = writeToStream;
        _keyToFileName = keyToFileName;
    }

    /// <inheritdoc/>
    public override async Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        TState? state = await ReadAsync(filePath, cancellationToken).ConfigureAwait(false);
        if (state is not null)
        {
            throw new DuplicateKeyException<TKey>(key);
        }

        string directory = GetDirectoryPath();
        if (!Directory.Exists(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        await using FileStream stream = new(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        string etag = string.IsNullOrWhiteSpace(value.Etag) ? UniqueIdHelper.GenerateUniqueStringId() : value.Etag;
        await _writeToStream(stream, value with { Etag = etag }, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

        return etag;
    }

    /// <inheritdoc/>
    public override async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
        => await ReadAsync(key, cancellationToken).ConfigureAwait(false) is not null;

    /// <inheritdoc/>
    public override async Task<TState> GetAsync(TKey key, CancellationToken cancellationToken)
        => await ReadAsync(key, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Key not found: {key}");

    /// <summary>
    /// Gets the directory path for the storage.
    /// </summary>
    /// <returns>The directory path.</returns>
    public string GetDirectoryPath() => Path.Combine(_rootPath, Database, Container);

    /// <summary>
    /// Gets the file path for the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The file path.</returns>
    public string GetFilePath(TKey key)
    {
        string fileName = _keyToFileName(key);
        return Path.Combine(GetDirectoryPath(), fileName);
    }

    /// <inheritdoc/>
    public override async Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetFilePath(key);

        TState? value = await ReadAsync(filePath, cancellationToken);

        if (value is null)
        {
            return false;
        }

        if (etag is not null && value.Etag is not null && value.Etag != etag)
        {
            throw new ConcurrencyException<TKey>(key, value.Etag, etag);
        }

        File.Delete(filePath);

        return true;
    }

    /// <inheritdoc/>
    public override async Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        TState current = await ReadAsync(filePath, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Key not found: {key}");

        if (value.Etag is not null && current.Etag is not null && current.Etag != value.Etag)
        {
            throw new ConcurrencyException<TKey>(key, value.Etag, current.Etag);
        }

        string newEtag = UniqueIdHelper.GenerateUniqueStringId();
        await WriteValueToFileAsync(filePath, value with { Etag = newEtag }, cancellationToken).ConfigureAwait(false);

        return newEtag;
    }

    /// <inheritdoc/>
    public override async Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken)
        => await ReadAsync(key, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Reads the value from the file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized value.</returns>
    protected async Task<TState> ReadValueFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await _readFromStream(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sanitizes the file name.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>The sanitized file name.</returns>
    protected virtual string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Writes the value to the file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task WriteValueToFileAsync(string filePath, TState value, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await _writeToStream(stream, value, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TState?> ReadAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        TState state = await ReadValueFromFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        if (state.TimeToLive is not null &&
            state.TimeToLive.Value > TimeSpan.Zero &&
            File.GetLastWriteTimeUtc(filePath).Add(state.TimeToLive.Value) < TimeProvider.GetUtcNow().UtcDateTime)
        {
            // The file time to live has expired, delete it
            File.Delete(filePath);
            return null;
        }

        return state;
    }

    private async Task<TState?> ReadAsync(TKey key, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);
        return await ReadAsync(filePath, cancellationToken);
    }
}