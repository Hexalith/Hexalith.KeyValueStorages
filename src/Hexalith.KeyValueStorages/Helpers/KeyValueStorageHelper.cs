// <copyright file="KeyValueStorageHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

using Hexalith.KeyValueStorages.Factories;
using Hexalith.KeyValueStorages.InMemory;

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
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    public static IServiceCollection AddMemoryKeyValueStore<TKey, TState>(
        this IServiceCollection services,
        [NotNull] string database = "database",
        string? name = null)
        where TState : StateBase
        where TKey : notnull, IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        services
            .AddKeyedTransient<IKeyValueStore<TKey, TState>, InMemoryKeyValueStore<TKey, TState>>(
                name,
                (sp, _) =>
                {
                    // Get the container name from the DataContract attribute or use the type name
                    string container = typeof(TState)
                        .GetCustomAttributes(typeof(DataContractAttribute), true)
                        .OfType<DataContractAttribute>()
                        .FirstOrDefault()?.Name ?? nameof(DataContractAttribute);
                    var store = new InMemoryKeyValueStore<TKey, TState>(
                        database,
                        container,
                        sp.GetRequiredService<TimeProvider>());
                    return store;
                })
            .TryAddSingleton<IKeyValueStoreFactory, KeyValueStoreFactory>();
        return services;
    }
}