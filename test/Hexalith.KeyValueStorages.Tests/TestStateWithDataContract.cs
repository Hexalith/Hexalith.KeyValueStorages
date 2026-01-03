// <copyright file="TestStateWithDataContract.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Tests;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Unit tests for the <see cref="Helpers.StateHelper"/> class.
/// </summary>
public partial class StateHelperTests
{
    /// <summary>
    /// Test state with a <see cref="DataContractAttribute"/> specifying a custom serialization name.
    /// </summary>
    /// <param name="Etag">The entity tag used for concurrency control.</param>
    /// <param name="TimeToLive">The optional time-to-live for the state.</param>
    [DataContract(Name = "CustomStateName")]
    internal sealed record TestStateWithDataContract(string? Etag, TimeSpan? TimeToLive) : StateBase(Etag, TimeToLive);
}