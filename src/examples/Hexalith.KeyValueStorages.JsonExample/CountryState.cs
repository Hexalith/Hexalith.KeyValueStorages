// <copyright file="CountryState.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.JsonExample;

using System.Runtime.Serialization;

/// <summary>
/// Represents a country with its code, name, and currency.
/// </summary>
/// <param name="Value"> The country.</param>
/// <param name="TimeToLive"> The time to live for the value.</param>
/// <param name="Etag"> The etag associated with the value.</param>
[DataContract(Name = nameof(Country))]
internal record CountryState(
    Country Value,
    TimeSpan? TimeToLive = null,
    string? Etag = null) : State<Country>(Value, Etag, TimeToLive);