// <copyright file="KeyValueStorageHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Helpers;

using System;

using Hexalith.KeyValueStorages.InMemory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static IServiceCollection AddMemoryKeyValueStore(
        this IServiceCollection services,
        string name,
        string? database = null,
        string? container = null,
        string? entity = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services.TryAddSingleton<TimeProvider>();
        return services
            .AddKeyedTransient<IKeyValueProvider, InMemoryKeyValueProvider>(
                name,
                (sp, _) =>
                {
                    var store = new InMemoryKeyValueProvider(
                        sp.GetRequiredService<IOptions<KeyValueStoreSettings>>(),
                        database,
                        container,
                        entity,
                        sp.GetRequiredService<TimeProvider>());
                    return store;
                });
    }
}