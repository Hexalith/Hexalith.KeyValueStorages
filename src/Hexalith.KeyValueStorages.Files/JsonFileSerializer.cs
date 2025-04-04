// <copyright file="JsonFileSerializer.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;

/// <summary>
/// Serializer for JSON file values.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the Etag.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonFileSerializer{TValue, TEtag}"/> class.
/// </remarks>
public class JsonFileSerializer<TValue, TEtag> : IValueSerializer<TValue, TEtag>
    where TValue : notnull
    where TEtag : notnull
{
    private readonly JsonSerializerOptions? _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileSerializer{TValue, TEtag}"/> class.
    /// </summary>
    /// <param name="options">The options to use for serialization.</param>
    public JsonFileSerializer(JsonSerializerOptions? options) => _options = options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileSerializer{TValue, TEtag}"/> class.
    /// </summary>
    public JsonFileSerializer()
    {
    }

    /// <inheritdoc/>
    public string DataType => "json";

    /// <inheritdoc/>
    public (TValue Value, TEtag Etag) Deserialize(string value)
    {
        JsonFileValue<TValue, TEtag>? result = JsonSerializer.Deserialize<JsonFileValue<TValue, TEtag>>(value, _options)
            ?? throw new JsonException("Deserialization return a null value : " + value);

        return (result.Value, result.Etag);
    }

    /// <inheritdoc/>
    public Task<(TValue Value, TEtag Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public string Serialize(TValue value, TEtag etag) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task SerializeAsync(Stream stream, TValue value, TEtag etag, CancellationToken cancellationToken) => throw new NotImplementedException();
}