// <copyright file="ServiceCollectionExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Extensions;

using Hexalith.KeyValueStorages.DaprComponents.Actors;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Dapr actor key-value storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a typed Dapr actor key-value storage implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Optional custom actor type name. If not provided, the name is derived from the TValue type name.</param>
    /// <param name="keyToActorId">Optional function to convert the key to an actor ID. If not provided, a default conversion is used.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprActorKeyValueStorage<TKey, TState>(
        this IServiceCollection services,
        string name,
        Func<TKey, string>? keyToActorId = null)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
    {
        ArgumentNullException.ThrowIfNull(services);
        keyToActorId ??= DaprActorKeyValueStorage<TKey, TState>.KeyToRfc1123;

        // Register the actor with the Dapr runtime
        return services.AddDaprKeyValueStoreActor<TState>(name)

            // Register the key-value storage implementation
            .AddSingleton<IKeyValueStore<TKey, TState>>(
                _ => new DaprActorKeyValueStorage<TKey, TState>(name, keyToActorId));
    }

    /// <summary>
    /// Registers the key-value store actor with the Dapr runtime.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">Optional custom actor type name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprKeyValueStoreActor<TState>(this IServiceCollection services, string name)
        where TState : StateBase
    {
        services.AddActors(options =>
            options.Actors.RegisterActor<KeyValueStoreActor<TState>>(name));
        return services;
    }
}