// <copyright file="KeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;

using Hexalith.Commons.Configurations;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents an abstract key-value store.
/// </summary>
public abstract class KeyValueProvider : IKeyValueProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueProvider"/> class.
    /// </summary>
    /// <param name="settings">The settings for the key-value store.</param>
    /// <param name="database">The name of the database. If not provided, the setting value is used.</param>
    /// <param name="container">The name of the container. If not provided, the setting value is used.</param>
    /// <param name="entity">The name of the entity.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    protected KeyValueProvider(
        IOptions<KeyValueStoreSettings> settings,
        string? database,
        string? container,
        string? entity,
        TimeProvider? timeProvider)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;

        if (string.IsNullOrWhiteSpace(database))
        {
            SettingsException.ThrowIfUndefined<KeyValueStoreSettings>(settings.Value.DefaultDatabase);
            Database = settings.Value.DefaultDatabase;
        }
        else
        {
            Database = database;
        }

        if (string.IsNullOrWhiteSpace(container))
        {
            SettingsException.ThrowIfUndefined<KeyValueStoreSettings>(settings.Value.DefaultContainer);
            Container = settings.Value.DefaultContainer;
        }
        else
        {
            Container = container;
        }

        Entity = entity;
        TimeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Gets the container name.
    /// </summary>
    public string Container { get; init; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; init; }

    /// <summary>
    /// Gets the entity name.
    /// </summary>
    public string? Entity { get; init; }

    /// <summary>
    /// Gets the settings for the key-value store.
    /// </summary>
    protected IOptions<KeyValueStoreSettings> Settings { get; }

    /// <summary>
    /// Gets the time provider, using the service provider if not already set.
    /// </summary>
    protected TimeProvider TimeProvider { get; }

    /// <inheritdoc/>
    public abstract IKeyValueStore<TKey, TState> Create<TKey, TState>(string? database = null, string? container = null, string? entity = null)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase;
}