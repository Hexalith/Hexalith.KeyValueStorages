﻿// <copyright file="StateHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Helpers;

using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// Provides helper methods for state management.
/// </summary>
public static class StateHelper
{
    /// <summary>
    /// Gets the state name from the DataContract attribute or the type name.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>The name of the state.</returns>
    public static string GetStateName<TState>()
        where TState : StateBase =>
        typeof(TState).GetCustomAttributes(typeof(DataContractAttribute), true)
            .OfType<DataContractAttribute>()
            .Select(attr => attr.Name)
            .FirstOrDefault()
                ?? typeof(TState).Name;
}