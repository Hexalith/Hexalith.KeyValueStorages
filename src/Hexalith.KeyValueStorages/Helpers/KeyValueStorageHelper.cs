// <copyright file="KeyValueStorageHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Helpers;

using System;

using Hexalith.KeyValueStorages.Factories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Provides helper methods for adding key-value store services to the service collection.
/// </summary>
public static class KeyValueStorageHelper
{
    /// <summary>
    /// Adds an in-memory key-value store to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static IServiceCollection AddMemoryKeyValueStore(
        this IServiceCollection services,
        string name)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services
            .AddKeyedTransient<IKeyValueProvider, InMemoryKeyValueProvider>(
                name,
                (sp, _) =>
                {
                    var store = new InMemoryKeyValueProvider(sp);
                    return store;
                })
            .TryAddSingleton<IKeyValueFactory, KeyValueStoreFactory>();
        return services;
    }
}