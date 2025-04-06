// <copyright file="CountryState.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Example;

using System.Runtime.Serialization;

/// <summary>
/// Represents a country with its code, name, and currency.
/// </summary>
/// <param name="Value"> The country.</param>
/// <param name="TimeToLive"> The time to live for the value.</param>
/// <param name="Etag"> The etag associated with the value.</param>
[DataContract]
public record CountryState(
    Country Value,
    TimeSpan? TimeToLive,
    string? Etag) : State<Country, string>(Value, null, TimeToLive, Etag)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountryState"/> class.
    /// </summary>
    /// <param name="value"> The country.</param>
    public CountryState(Country value)
        : this(value, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryState"/> class.
    /// </summary>
    /// <param name="value"> The country.</param>
    /// <param name="etag"> The etag associated with the value.</param>
    public CountryState(Country value, string etag)
        : this(value, null, etag)
    {
    }
}