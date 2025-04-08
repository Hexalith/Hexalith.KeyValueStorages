// <copyright file="Country.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.SimpleApp;

using System.Runtime.Serialization;

/// <summary>
/// Represents a country with its associated details.
/// </summary>
/// <param name="Code">The ISO 3166-1 alpha-2 country code.</param>
/// <param name="Name">The name of the country.</param>
/// <param name="Currency">The currency used in the country.</param>
/// <param name="PhonePrefix">The international phone prefix for the country.</param>
[DataContract]
public record Country(
   [property: DataMember(Order = 1)] string Code,
   [property: DataMember(Order = 2)] string Name,
   [property: DataMember(Order = 3)] string Currency,
   [property: DataMember(Order = 4)] int PhonePrefix);