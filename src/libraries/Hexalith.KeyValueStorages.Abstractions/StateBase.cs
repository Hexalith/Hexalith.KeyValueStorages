// <copyright file="StateBase.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;
using System.Globalization;
using System.Runtime.Serialization;

using Hexalith.Commons.StringEncoders;

/// <summary>
/// Represents a stored value with its etag.
/// </summary>
/// <param name="Etag">The etag associated with the value.</param>
/// <param name="TimeToLive">The time to live for the value.</param>
[DataContract]
public record StateBase(
    [property: DataMember(Order = 2)] string? Etag,
    [property: DataMember(Order = 1)] TimeSpan? TimeToLive)
{
    /// <summary>
    /// Converts the key to an RFC 1123 string representation for use as an actor ID.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="key">The key to convert.</param>
    /// <returns>The RFC 1123 string representation of the key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key is null or its string representation is null or empty.</exception>
    public static string KeyToRfc1123<TKey>(TKey key)
        where TKey : notnull, IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(key);
        string? keyString = key.ToString();
        return string.IsNullOrWhiteSpace(keyString)
            ? throw new ArgumentNullException(nameof(key), "key.ToString() cannot be null or empty")
            : key.ToString()!.ToRFC1123(CultureInfo.InvariantCulture);
    }
}