// <copyright file="StateBase.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System.Runtime.Serialization;

/// <summary>
/// Represents a stored value with its etag.
/// </summary>
/// <param name="Etag">The etag associated with the value.</param>
/// <param name="TimeToLive">The time to live for the value.</param>
[DataContract]
public record StateBase(
    [property: DataMember(Order = 2)] string? Etag,
    [property: DataMember(Order = 1)] TimeSpan? TimeToLive);