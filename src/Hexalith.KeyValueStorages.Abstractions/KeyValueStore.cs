// <copyright file="KeyValueStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages;

using System;
using System.Diagnostics.CodeAnalysis;
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
    private IServiceProvider? _services;
    private TimeProvider? _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueStore{TKey, TState}"/> class.
    /// </summary>
    protected KeyValueStore()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueStore{TKey, TState}"/> class.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="timeProvider">The time provider to use for managing expiration times.</param>
    protected KeyValueStore([NotNull] string database, [NotNull] string container, [NotNull] TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(container);
        _timeProvider = timeProvider;
        Container = container;
        Database = database;
    }

    /// <summary>
    /// Gets the container name.
    /// </summary>
    public string Container { get; internal set; } = nameof(Container);

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; internal set; } = nameof(Container);

    /// <summary>
    /// Gets or sets the service provider used by the key-value store.
    /// </summary>
    internal IServiceProvider Services
    {
        get => _services ?? throw new InvalidOperationException("KeyValueStore service provider is not initialized");
        set => _services = value;
    }

    /// <summary>
    /// Gets the time provider, using the service provider if not already set.
    /// </summary>
    protected TimeProvider TimeProvider
        => _timeProvider ??= _services is null
                ? TimeProvider.System
                : Services.GetService(typeof(TimeProvider)) as TimeProvider
                    ?? throw new InvalidOperationException("KeyValueStore time provider is missing in the service provider");

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