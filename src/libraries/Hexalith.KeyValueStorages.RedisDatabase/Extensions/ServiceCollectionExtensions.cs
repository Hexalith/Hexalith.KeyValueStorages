// <copyright file="ServiceCollectionExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.RedisDatabase.Extensions;

using System;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Redis key-value storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Redis connection multiplexer as a singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisConnection(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));
    }

    /// <summary>
    /// Registers the Redis connection multiplexer as a singleton using settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisConnection(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            IOptions<RedisKeyValueStoreSettings> settings = sp.GetRequiredService<IOptions<RedisKeyValueStoreSettings>>();
            return string.IsNullOrWhiteSpace(settings.Value.ConnectionString)
                ? throw new InvalidOperationException("Redis connection string is not configured.")
                : ConnectionMultiplexer.Connect(settings.Value.ConnectionString);
        });
    }

    /// <summary>
    /// Registers a typed Redis key-value storage implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the service.</param>
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity. If not provided, the state object data contract name is used or the type name.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for serialization.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisKeyValueStorage(
        this IServiceCollection services,
        string name,
        string? database = null,
        string? container = null,
        string? entity = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return services
            .AddKeyedSingleton<IKeyValueProvider>(
                name,
                (sp, _) => new RedisKeyValueProvider(
                    sp.GetRequiredService<IConnectionMultiplexer>(),
                    sp.GetRequiredService<IOptions<KeyValueStoreSettings>>(),
                    database,
                    container,
                    entity,
                    jsonSerializerOptions,
                    sp.GetService<TimeProvider>()));
    }
}