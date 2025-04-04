// <copyright file="DummyValue.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

/// <summary>
/// Represents a dummy value for testing.
/// </summary>
public record DummyValue
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public DateTimeOffset Started { get; set; }

    /// <summary>
    /// Gets or sets the number of retries.
    /// </summary>
    public long Retries { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation failed.
    /// </summary>
    public bool Failed { get; set; }
}