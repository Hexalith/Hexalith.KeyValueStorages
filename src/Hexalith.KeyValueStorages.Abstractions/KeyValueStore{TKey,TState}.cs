// <copyright file="KeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents an abstract key-value store.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
public abstract class KeyValueStore<TKey, TState> : IKeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueStore{TKey, TState}"/> class.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    protected KeyValueStore([NotNull] string database, string? container, TimeProvider? timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);

        // If container is null or empty, use DataContract attribute name or type name
        if (string.IsNullOrWhiteSpace(container))
        {
            Container = typeof(TState)
                .GetCustomAttributes(typeof(DataContractAttribute), true)
                .OfType<DataContractAttribute>()
                .FirstOrDefault()?.Name ?? typeof(TState).Name;
        }
        else
        {
            Container = container;
        }

        TimeProvider = timeProvider ?? TimeProvider.System;
        Database = database;
    }

    /// <summary>
    /// Gets the container name.
    /// </summary>
    public string Container { get; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the time provider, using the service provider if not already set.
    /// </summary>
    protected TimeProvider TimeProvider { get; }

    /// <inheritdoc/>
    public abstract Task<string> AddAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<TState> GetAsync(TKey key, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<bool> RemoveAsync(TKey key, string? etag, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<string> SetAsync(TKey key, TState value, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<TState?> TryGetAsync(TKey key, CancellationToken cancellationToken);
}