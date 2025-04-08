// <copyright file="CountryState.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.SimpleApp;

using System.Runtime.Serialization;

/// <summary>
/// Represents a country with its associated details.
/// </summary>
/// <param name="Value"> The country.</param>
/// <param name="Etag"> The etag associated with the value.</param>
/// <param name="TimeToLive"> The time to live for the value.</param>
[DataContract(Name = "Countries")]
public record CountryState(
    Country Value,
    string? Etag = null,
    TimeSpan? TimeToLive = null)
    : State<Country>(Value, Etag, TimeToLive);