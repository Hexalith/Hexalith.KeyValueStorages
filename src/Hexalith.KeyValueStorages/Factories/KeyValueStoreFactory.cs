// <copyright file="KeyValueStoreFactory.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Factories;

using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Factory class for creating instances of <see cref="IKeyValueStore{TKey, TState}"/>.
/// </summary>
public class KeyValueStoreFactory : IKeyValueStoreFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueStoreFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for managing dependencies.</param>
    public KeyValueStoreFactory([NotNull] IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IKeyValueStore<TKey, TState> Create<TKey, TState>(string name = "data")
        where TKey : notnull, IEquatable<TKey>
        where TState : StateBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _serviceProvider.GetRequiredKeyedService<IKeyValueStore<TKey, TState>>(name);
    }
}