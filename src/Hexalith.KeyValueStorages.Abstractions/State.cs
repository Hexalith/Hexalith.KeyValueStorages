// <copyright file="State.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System.Runtime.Serialization;

/// <summary>
/// Represents a stored value with its etag.
/// </summary>
/// <param name="Value">The stored value.</param>
/// <param name="Metadata">The metadata associated with the value.</param>
/// <param name="TimeToLive">The time to live for the value.</param>
/// <param name="Etag">The etag associated with the value.</param>
[DataContract]
public abstract record class State(
    string Value,
    Dictionary<string, string>? Metadata,
    TimeSpan? TimeToLive,
    string? Etag)
    : State<string>(Value, Metadata, TimeToLive, Etag);