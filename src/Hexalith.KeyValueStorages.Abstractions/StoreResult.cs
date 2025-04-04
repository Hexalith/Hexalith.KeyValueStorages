// <copyright file="StoreResult.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Represents the result of a store operation, containing the stored value and its associated Etag.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TEtag">The type of the Etag.</typeparam>
public record class StoreResult<TValue, TEtag>(TValue Value, TEtag Etag)
    where TEtag : notnull;