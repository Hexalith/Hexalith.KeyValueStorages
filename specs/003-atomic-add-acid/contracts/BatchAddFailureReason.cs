// <copyright file="BatchAddFailureReason.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// CONTRACT DEFINITION - New file
// Target: src/libraries/Hexalith.KeyValueStorages.Abstractions/Exceptions/BatchAddFailureReason.cs

namespace Hexalith.KeyValueStorages.Exceptions;

/// <summary>
/// Specifies the reason a batch add operation failed.
/// </summary>
/// <remarks>
/// This enumeration is used with <see cref="BatchAddException{TKey}"/> to provide
/// detailed failure information for batch operations.
/// </remarks>
public enum BatchAddFailureReason
{
    /// <summary>
    /// The batch contained duplicate keys within itself.
    /// This is detected during pre-validation before any storage operations.
    /// </summary>
    DuplicateKeysInBatch = 0,

    /// <summary>
    /// One or more keys in the batch already exist in the store.
    /// This is detected during pre-validation against the existing store contents.
    /// </summary>
    DuplicateKeysInStore = 1,

    /// <summary>
    /// The underlying transaction was aborted.
    /// This typically occurs for ACID-compliant providers (Redis, Dapr) when
    /// the transaction cannot be committed due to conflicts or timeouts.
    /// </summary>
    TransactionAborted = 2,

    /// <summary>
    /// A partial failure occurred during the batch operation.
    /// For non-transactional providers, this indicates that some items may have
    /// been written before the failure, but compensating rollback was performed.
    /// The store should be in a consistent state (no items from this batch).
    /// </summary>
    PartialFailure = 3
}
