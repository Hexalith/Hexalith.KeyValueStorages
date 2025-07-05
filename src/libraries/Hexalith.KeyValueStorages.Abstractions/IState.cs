// <copyright file="IState.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

/// <summary>
/// Represents a stored value with its etag.
/// </summary>
public interface IState
{
    /// <summary>
    /// Gets the name of the state.
    /// </summary>
    static abstract IState Name { get; }

    /// <summary>
    /// Gets the etag of the state.
    /// </summary>
    string? Etag { get; }

    /// <summary>
    /// Gets the time to live of the state.
    /// </summary>
    TimeSpan? TimeToLive { get; }
}