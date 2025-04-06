// <copyright file="DaprKeyValueStorage{TKey,TValue,TEtag,TKeySerializer,TValueSerializer}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.DaprComponents;

using System;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;

/// <summary>
/// File based key-value storage implementation.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public abstract class DaprKeyValueStorage<TKey, TValue>
    : IKeyValueStore<TKey, TValue, string>
    where TKey : notnull
{
    public Task<string> AddAsync(TKey key, TValue value, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<StoreResult<TValue, string>> GetAsync(TKey key, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> RemoveAsync(TKey key, string etag, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<string> SetAsync(TKey key, TValue value, string etag, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<StoreResult<TValue, string>?> TryGetValueAsync(TKey key, CancellationToken cancellationToken) => throw new NotImplementedException();
}