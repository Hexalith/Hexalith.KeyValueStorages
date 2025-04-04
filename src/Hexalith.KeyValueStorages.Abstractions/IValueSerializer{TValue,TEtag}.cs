// <copyright file="IValueSerializer{TValue,TEtag}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Interface for serializing values.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the ETAG.</typeparam>
public interface IValueSerializer<TValue, TEtag>
{
    /// <summary>
    /// Gets the data type of the value.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Deserializes the specified string to a value.
    /// </summary>
    /// <param name="value">The string representation of the value.</param>
    /// <returns>The deserialized value.</returns>
    (TValue Value, TEtag Etag) Deserialize(string value);

    /// <summary>
    /// Deserializes the specified string to a value asynchronously.
    /// </summary>
    /// <param name="value">The string representation of the value.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value.</returns>
    Task<(TValue Value, TEtag Etag)> DeserializeAsync(string value, CancellationToken cancellationToken) => Task.FromResult(Deserialize(value));

    /// <summary>
    /// Deserializes the specified stream to a value asynchronously.
    /// </summary>
    /// <param name="stream">The stream containing the value.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value.</returns>
    Task<(TValue Value, TEtag Etag)> DeserializeAsync(Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// Serializes the specified value.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="etag">The ETAG.</param>
    /// <returns>A string representation of the value.</returns>
    string Serialize(TValue value, TEtag etag);

    /// <summary>
    /// Serializes the specified value asynchronously.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="etag">The ETAG.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string representation of the value.</returns>
    Task<string> SerializeAsync(TValue value, TEtag etag, CancellationToken cancellationToken) => Task.FromResult(Serialize(value, etag));

    /// <summary>
    /// Serializes the specified stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream containing the value.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="etag">The ETAG.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string representation of the value.</returns>
    Task SerializeAsync(Stream stream, TValue value, TEtag etag, CancellationToken cancellationToken);
}