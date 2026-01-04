// <copyright file="IKeyValueStore-AddRangeAsync.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// CONTRACT DEFINITION - This file defines the interface contract for AddRangeAsync
// To be merged into: src/libraries/Hexalith.KeyValueStorages.Abstractions/IKeyValueStore{TKey,TState}.cs

namespace Hexalith.KeyValueStorages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension to IKeyValueStore for batch add operations.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the store. Must be non-nullable.</typeparam>
/// <typeparam name="TState">The type of the state associated with the values.</typeparam>
public partial interface IKeyValueStore<TKey, TState>
    where TKey : notnull, IEquatable<TKey>
    where TState : StateBase
{
    /// <summary>
    /// Gets the capabilities of this key-value store provider.
    /// </summary>
    /// <remarks>
    /// Use this property to determine whether the provider supports ACID transactions
    /// or uses compensating transactions for atomicity.
    /// </remarks>
    ProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Asynchronously adds multiple key/value pairs in a single atomic operation.
    /// </summary>
    /// <param name="items">
    /// The key-value pairs to add. Each item is a tuple containing the key and value.
    /// Values must include appropriate Etag and TimeToLive settings.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a read-only list
    /// of ETags for each successfully added item, in the same order as the input collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="items"/> is null or contains null values.
    /// </exception>
    /// <exception cref="Exceptions.BatchAddException{TKey}">
    /// Thrown when:
    /// <list type="bullet">
    /// <item>The batch contains duplicate keys within itself (DuplicateKeysInBatch)</item>
    /// <item>One or more keys already exist in the store (DuplicateKeysInStore)</item>
    /// <item>The underlying transaction is aborted (TransactionAborted)</item>
    /// <item>A partial failure occurs during the operation (PartialFailure)</item>
    /// </list>
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This operation is atomic: either all items are added successfully, or none are.
    /// The atomicity guarantee varies by provider:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>ACID-compliant providers</b> (Redis, Dapr with transaction support):
    /// Use native provider transactions for full ACID guarantees including isolation.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Best-effort providers</b> (InMemory, JsonFile):
    /// Use lock-based synchronization and compensating transactions.
    /// Atomicity is guaranteed within the process, but not across processes.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Performance: Batch operations should complete within 5 seconds for up to 1000 items
    /// under normal conditions.
    /// </para>
    /// <para>
    /// Each item's TimeToLive is honored independently.
    /// </para>
    /// <para>
    /// Empty collections complete successfully with no changes to the store.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var items = new[]
    /// {
    ///     ("key1", new MyState("value1", null, TimeSpan.FromHours(1))),
    ///     ("key2", new MyState("value2", null, null)),
    ///     ("key3", new MyState("value3", null, TimeSpan.FromMinutes(30)))
    /// };
    ///
    /// IReadOnlyList&lt;string&gt; etags = await store.AddRangeAsync(items, cancellationToken);
    ///
    /// // etags[0] corresponds to "key1"
    /// // etags[1] corresponds to "key2"
    /// // etags[2] corresponds to "key3"
    /// </code>
    /// </example>
    Task<IReadOnlyList<string>> AddRangeAsync(
        IEnumerable<(TKey Key, TState Value)> items,
        CancellationToken cancellationToken);
}
