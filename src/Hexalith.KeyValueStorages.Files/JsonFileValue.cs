// <copyright file="JsonFileValue.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Files;

using System.Runtime.Serialization;

/// <summary>
/// Represents a JSON file value with an associated ETag.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the ETag.</typeparam>
[DataContract]
public record JsonFileValue<TValue, TEtag>(
    [property: DataMember(Order = 1)] TValue Value,
    [property: DataMember(Order = 2)] TEtag Etag)
    where TValue : notnull
    where TEtag : notnull;