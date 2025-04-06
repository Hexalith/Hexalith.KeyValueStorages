// <copyright file="ServiceCollectionExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents.Extensions;

using Dapr.Actors.Runtime;

using Hexalith.KeyValueStorages.DaprComponents.Actors;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Dapr actor key-value storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the key-value store actor with the Dapr runtime.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="actorTypeName">Optional custom actor type name. If not provided, defaults to "KeyValueStoreActor".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprKeyValueStoreActor(this IServiceCollection services, string? actorTypeName = null)
    {
        // Register the actor with the Dapr runtime
        services.AddActors(options =>
        {
            // Register the KeyValueStoreActor with the runtime
            options.Actors.RegisterActor<KeyValueStoreActor>(actorTypeName ?? "KeyValueStoreActor");
            
            // Configure actor settings if needed
            options.ActorIdleTimeout = TimeSpan.FromMinutes(60);
            options.ActorScanInterval = TimeSpan.FromSeconds(30);
            options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(60);
            options.DrainRebalancedActors = true;
        });
        
        return services;
    }

    /// <summary>
    /// Registers a typed Dapr actor key-value storage implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TKeySerializer">The type of the key serializer.</typeparam>
    /// <typeparam name="TValueSerializer">The type of the value serializer.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="actorTypeName">Optional custom actor type name. If not provided, defaults to "KeyValueStoreActor".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDaprActorKeyValueStorage<TKey, TValue, TKeySerializer, TValueSerializer>(
        this IServiceCollection services,
        string? actorTypeName = null)
        where TKey : notnull
        where TKeySerializer : IKeySerializer<TKey>, new()
        where TValueSerializer : IValueSerializer<TValue, string>, new()
    {
        // Register the actor with the Dapr runtime
        services.AddDaprKeyValueStoreActor(actorTypeName);
        
        // Register the key-value storage implementation
        services.AddSingleton<IKeyValueStore<TKey, TValue, string>>(
            sp => new DaprActorKeyValueStorage<TKey, TValue, TKeySerializer, TValueSerializer>(actorTypeName));
        
        return services;
    }
}