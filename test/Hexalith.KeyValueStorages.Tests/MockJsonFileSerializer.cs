// <copyright file="MockJsonFileSerializer.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System.Text.Json;

using Hexalith.KeyValueStorages.Files;

/// <summary>
/// Mock implementation of JsonFileSerializer for testing.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the ETag.</typeparam>
public class MockJsonFileSerializer<TValue, TEtag> : IValueSerializer<TValue, TEtag>
    where TValue : notnull
    where TEtag : notnull
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
    };

    /// <inheritdoc/>
    public string DataType => "json";

    /// <inheritdoc/>
    public (TValue Value, TEtag Etag) Deserialize(string value)
    {
        JsonFileValue<TValue, TEtag>? result = JsonSerializer.Deserialize<JsonFileValue<TValue, TEtag>>(value, _options)
            ?? throw new JsonException("Deserialization returned a null value: " + value);

        return (result.Value, result.Etag);
    }

    /// <inheritdoc/>
    public async Task<(TValue Value, TEtag Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(stream);
        string json = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return Deserialize(json);
    }

    /// <inheritdoc/>
    public string Serialize(TValue value, TEtag etag)
    {
        var fileValue = new JsonFileValue<TValue, TEtag>(value, etag);
        return JsonSerializer.Serialize(fileValue, _options);
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream, TValue value, TEtag etag, CancellationToken cancellationToken)
    {
        string json = Serialize(value, etag);
        await using StreamWriter writer = new(stream);
        await writer.WriteAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
    }
}