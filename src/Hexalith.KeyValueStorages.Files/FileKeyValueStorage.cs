// <copyright file="FileKeyValueStorage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;

/// <summary>
/// File based key-value storage implementation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the etag used for concurrency control.</typeparam>
/// <typeparam name="TKeySerializer">The serializer for the key.</typeparam>
/// <typeparam name="TValueSerializer">The serializer for the value.</typeparam>
public abstract class FileKeyValueStorage<TKey, TValue, TEtag, TKeySerializer, TValueSerializer>
    : IKeyValueStore<TKey, TValue, TEtag>
    where TEtag : notnull
    where TKey : notnull
    where TKeySerializer : IKeySerializer<TKey>, new()
    where TValueSerializer : IValueSerializer<TValue, TEtag>, new()
{
    private readonly TKeySerializer _keySerializer = new();
    private readonly string _rootPath;
    private readonly TValueSerializer _valueSerializer = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileKeyValueStorage{TKey, TValue, TEtag, TKeySerializer, TValueSerializer}"/> class.
    /// </summary>
    /// <param name="rootPath">The root path for storing files.</param>
    protected FileKeyValueStorage(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        _rootPath = rootPath;
        _ = Directory.CreateDirectory(_rootPath);
    }

    /// <inheritdoc/>
    public async Task<TEtag> AddAsync(TKey key, TValue value, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        if (File.Exists(filePath))
        {
            throw new InvalidOperationException($"Key already exists: {key}");
        }

        TEtag etag = GenerateEtag();
        await WriteValueToFileAsync(filePath, value, etag, cancellationToken).ConfigureAwait(false);

        return etag;
    }

    /// <inheritdoc/>
    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);
        return Task.FromResult(File.Exists(filePath));
    }

    /// <inheritdoc/>
    public async Task<StoreResult<TValue, TEtag>> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            throw new KeyNotFoundException($"Key not found: {key}");
        }

        (TValue value, TEtag etag) = await ReadValueFromFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        return new StoreResult<TValue, TEtag>(value, etag);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(TKey key, TEtag etag, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<TEtag> SetAsync(TKey key, TValue value, TEtag etag, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            throw new KeyNotFoundException($"Key not found: {key}");
        }

        StoreResult<TValue, TEtag> current = await GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (!current.Etag.Equals(etag))
        {
            throw new ConcurrencyException($"Etag mismatch for key {key}. Expected: {etag}, Current: {current.Etag}");
        }

        TEtag newEtag = GenerateEtag();
        await WriteValueToFileAsync(filePath, value, etag, cancellationToken).ConfigureAwait(false);

        return newEtag;
    }

    /// <inheritdoc/>
    public async Task<StoreResult<TValue, TEtag>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken)
    {
        string filePath = GetFilePath(key);

        if (!File.Exists(filePath))
        {
            return null;
        }

        (TValue value, TEtag etag) = await ReadValueFromFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        return new StoreResult<TValue, TEtag>(value, etag);
    }

    /// <summary>
    /// Generates the initial etag for a new key-value pair.
    /// </summary>
    /// <returns>The initial etag.</returns>
    protected abstract TEtag GenerateEtag();

    /// <summary>
    /// Gets the file path for the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The file path.</returns>
    protected string GetFilePath(TKey key)
    {
        string serializedKey = _keySerializer.Serialize(key);
        string fileExtension = _valueSerializer.DataType;
        return Path.Combine(_rootPath, $"{serializedKey}.{fileExtension}");
    }

    /// <summary>
    /// Reads the value from the file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized value.</returns>
    protected async Task<(TValue Value, TEtag Etag)> ReadValueFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await _valueSerializer.DeserializeAsync(stream, cancellationToken).ConfigureAwait(false);
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
    /// <param name="etag">The etag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task WriteValueToFileAsync(string filePath, TValue value, TEtag etag, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await _valueSerializer.SerializeAsync(stream, value, etag, cancellationToken).ConfigureAwait(false);
    }
}