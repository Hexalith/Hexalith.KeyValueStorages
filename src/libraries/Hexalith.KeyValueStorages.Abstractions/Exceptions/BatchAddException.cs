// <copyright file="BatchAddException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.KeyValueStorages.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Exception thrown when a batch add operation fails.
/// </summary>
/// <typeparam name="TKey">The type of the keys that caused the failure.</typeparam>
/// <remarks>
/// <para>
/// This exception provides detailed information about batch failures, including
/// which keys caused the failure and the reason for the failure.
/// </para>
/// <para>
/// The exception message includes up to 5 failed keys for readability.
/// Access <see cref="FailedKeys"/> for the complete list.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     await store.AddRangeAsync(items, cancellationToken);
/// }
/// catch (BatchAddException&lt;string&gt; ex)
/// {
///     Console.WriteLine($"Batch failed: {ex.Reason}");
///     Console.WriteLine($"Failed keys: {string.Join(", ", ex.FailedKeys)}");
/// }
/// </code>
/// </example>
public class BatchAddException<TKey> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    public BatchAddException()
        : base("Batch add operation failed.")
    {
        FailedKeys = [];
        Reason = BatchAddFailureReason.PartialFailure;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BatchAddException(string message)
        : base(message)
    {
        FailedKeys = [];
        Reason = BatchAddFailureReason.PartialFailure;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BatchAddException(string message, Exception innerException)
        : base(message, innerException)
    {
        FailedKeys = [];
        Reason = BatchAddFailureReason.PartialFailure;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="failedKeys">The keys that caused the batch operation to fail.</param>
    /// <param name="reason">The reason for the batch failure.</param>
    public BatchAddException(IEnumerable<TKey> failedKeys, BatchAddFailureReason reason)
        : base(BuildMessage(failedKeys, reason))
    {
        ArgumentNullException.ThrowIfNull(failedKeys);
        FailedKeys = failedKeys.ToList().AsReadOnly();
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="failedKeys">The keys that caused the batch operation to fail.</param>
    /// <param name="reason">The reason for the batch failure.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BatchAddException(
        IEnumerable<TKey> failedKeys,
        BatchAddFailureReason reason,
        Exception innerException)
        : base(BuildMessage(failedKeys, reason), innerException)
    {
        ArgumentNullException.ThrowIfNull(failedKeys);
        FailedKeys = failedKeys.ToList().AsReadOnly();
        Reason = reason;
    }

    /// <summary>
    /// Gets the keys that caused the batch operation to fail.
    /// </summary>
    /// <value>
    /// A read-only list of keys. Empty if the failure is not key-specific.
    /// </value>
    public IReadOnlyList<TKey> FailedKeys { get; }

    /// <summary>
    /// Gets the reason for the batch failure.
    /// </summary>
    /// <value>
    /// The <see cref="BatchAddFailureReason"/> indicating why the batch failed.
    /// </value>
    public BatchAddFailureReason Reason { get; }

    private static string BuildMessage(IEnumerable<TKey> failedKeys, BatchAddFailureReason reason)
    {
        List<TKey> keyList = [.. failedKeys];
        string displayKeys = string.Join(", ", keyList.Take(5).Select(k => k?.ToString() ?? "null"));
        string suffix = keyList.Count > 5 ? $" and {keyList.Count - 5} more" : string.Empty;

        return reason switch
        {
            BatchAddFailureReason.DuplicateKeysInBatch =>
                $"Batch contains duplicate keys: {displayKeys}{suffix}",
            BatchAddFailureReason.DuplicateKeysInStore =>
                $"Keys already exist in store: {displayKeys}{suffix}",
            BatchAddFailureReason.TransactionAborted =>
                $"Transaction was aborted. Affected keys: {displayKeys}{suffix}",
            BatchAddFailureReason.PartialFailure =>
                $"Partial failure during batch operation. Failed keys: {displayKeys}{suffix}",
            _ => $"Batch add failed for keys: {displayKeys}{suffix}",
        };
    }
}
