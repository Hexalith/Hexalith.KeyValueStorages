// <copyright file="InMemoryKeyValueProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Factories;

using System;
using System.Diagnostics.CodeAnalysis;

using Hexalith.KeyValueStorages.InMemory;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Factory class for creating instances of <see cref="IKeyValueStore{TKey, TState}"/>.
/// </summary>
public class InMemoryKeyValueProvider : IKeyValueProvider
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryKeyValueProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for managing dependencies.</param>
    public InMemoryKeyValueProvider([NotNull] IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IKeyValueStore<TKey, TState> Create<TKey, TState>(string database, string? container)
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        var store = new InMemoryKeyValueStore<TKey, TState>(
            database,
            container,
            _serviceProvider.GetRequiredService<TimeProvider>());
        return store;
    }
}