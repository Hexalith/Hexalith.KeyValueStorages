// <copyright file="Country.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Example;

using System.Runtime.Serialization;

/// <summary>
/// Represents a country with its code, name, and currency.
/// </summary>
/// <param name="Code"> The country code.</param>
/// <param name="Name"> The country name.</param>
/// /// <param name="Currency"> The currency used in the country.</param>
[DataContract]
internal record Country(
    [property: DataMember(Order = 1)] string Code,
    [property: DataMember(Order = 2)] string Name,
    [property: DataMember(Order = 3)] string Currency);