// <copyright file="FakeTimeProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;

/// <summary>
/// A fake time provider for testing time-dependent functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FakeTimeProvider"/> class.
/// </remarks>
/// <param name="initialTime">The initial time.</param>
internal class FakeTimeProvider(DateTimeOffset initialTime) : TimeProvider
{
    /// <summary>
    /// Advances the current time by the specified duration.
    /// </summary>
    /// <param name="duration">The duration to advance by.</param>
    public void AdvanceTime(TimeSpan duration) => initialTime = initialTime.Add(duration);

    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    /// <returns>The current UTC time.</returns>
    public override DateTimeOffset GetUtcNow() => initialTime;
}