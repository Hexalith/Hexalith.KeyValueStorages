// <copyright file="ServiceCollectionExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Extensions;

using System.Diagnostics.CodeAnalysis;

using Hexalith.KeyValueStorages.DaprComponents.Actors;
using Hexalith.KeyValueStorages.Helpers;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Dapr actor key-value storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Gets the actor type name for the specified state type, database, and container.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container. If not provided, a default value is used.</param>
    /// <returns>The actor type name.</returns>
    public static string ActorType<TState>([NotNull] string database, string? container)
            where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        return database + "." + (container ?? StateHelper.GetStateName<TState>());
    }

    /// <summary>
    /// Registers a typed Dapr actor key-value storage implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container. If not provided, a default value is used.</param>
    /// <param name="keyToActorId">Optional function to convert the key to an actor ID. If not provided, a default conversion is used.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprActorKeyValueStorage<TKey, TState>(
        this IServiceCollection services,
        string database,
        string? container = null,
        Func<TKey, string>? keyToActorId = null)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentNullException.ThrowIfNull(services);
        keyToActorId ??= DaprActorKeyValueStorage<TKey, TState>.KeyToRfc1123;

        // Register the actor with the Dapr runtime
        return services
            .AddDaprKeyValueStoreActor<TState>(database, container)

            // Register the key-value storage implementation
            .AddSingleton<IKeyValueStore<TKey, TState>>(
                sp => new DaprActorKeyValueStorage<TKey, TState>(
                    database,
                    container,
                    keyToActorId,
                    sp.GetRequiredService<TimeProvider>()));
    }

    /// <summary>
    /// Registers the key-value store actor with the Dapr runtime.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container. If not provided, a default value is used.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprKeyValueStoreActor<TState>(
        this IServiceCollection services,
        string database,
        string? container = null)
        where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        services.AddActors(options =>
            options.Actors.RegisterActor<KeyValueStoreActor<TState>>(ActorType<TState>(database, container)));
        return services;
    }
}