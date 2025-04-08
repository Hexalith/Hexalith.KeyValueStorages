// <copyright file="State{TValue}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System.Runtime.Serialization;

/// <summary>
/// Represents a stored value with its etag.
/// </summary>
/// <typeparam name="TValue">The type of the stored value.</typeparam>
/// <param name="Value">The stored value.</param>
/// <param name="Etag">The etag associated with the value.</param>
/// <param name="TimeToLive">The time to live for the value.</param>
[DataContract]
public record State<TValue>(
    [property: DataMember(Order = 3)] TValue Value,
    string? Etag,
    TimeSpan? TimeToLive)
    : StateBase(Etag, TimeToLive);