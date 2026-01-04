# Data Model: Atomic Add with ACID Support

**Feature**: 003-atomic-add-acid
**Date**: 2026-01-04
**Status**: Complete

## Entity Overview

This feature introduces new types for batch operations while extending existing abstractions.

---

## New Entities

### 1. ProviderCapabilities

**Purpose**: Describes transaction and batch operation capabilities of a specific provider.

**Location**: `Hexalith.KeyValueStorages.Abstractions/ProviderCapabilities.cs`

```csharp
/// <summary>
/// Describes the capabilities of a key-value store provider.
/// </summary>
/// <param name="SupportsAcidTransactions">
/// True if the provider supports full ACID transactions for batch operations.
/// When true, batch operations use native provider transactions.
/// When false, batch operations use compensating transactions (best-effort atomicity).
/// </param>
/// <param name="SupportsBatchOperations">
/// True if the provider supports batch operations.
/// All providers in this library support batch operations.
/// </param>
/// <param name="MaxBatchSize">
/// Maximum number of items allowed in a single batch operation.
/// Null indicates no provider-imposed limit (library default of 1000 applies).
/// </param>
[DataContract]
public sealed record ProviderCapabilities(
    [property: DataMember(Order = 1)] bool SupportsAcidTransactions,
    [property: DataMember(Order = 2)] bool SupportsBatchOperations,
    [property: DataMember(Order = 3)] int? MaxBatchSize);
```

**Validation Rules**:
- `MaxBatchSize` must be null or greater than 0
- Immutable record - no state transitions

**Provider Values**:

| Provider | SupportsAcidTransactions | SupportsBatchOperations | MaxBatchSize |
|----------|--------------------------|-------------------------|--------------|
| InMemory | false | true | null |
| JsonFile | false | true | null |
| Redis | true | true | null |
| DaprActor | true (depends on underlying store) | true | null |

---

### 2. BatchAddException<TKey>

**Purpose**: Exception thrown when a batch add operation fails, containing details about which keys caused the failure.

**Location**: `Hexalith.KeyValueStorages.Abstractions/Exceptions/BatchAddException.cs`

```csharp
/// <summary>
/// Exception thrown when a batch add operation fails.
/// </summary>
/// <typeparam name="TKey">The type of the keys that caused the failure.</typeparam>
public class BatchAddException<TKey> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="failedKeys">The keys that caused the batch operation to fail.</param>
    /// <param name="reason">The reason for the batch failure.</param>
    public BatchAddException(
        IEnumerable<TKey> failedKeys,
        BatchAddFailureReason reason)
        : base(BuildMessage(failedKeys, reason))
    {
        FailedKeys = failedKeys.ToList().AsReadOnly();
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAddException{TKey}"/> class.
    /// </summary>
    /// <param name="failedKeys">The keys that caused the batch operation to fail.</param>
    /// <param name="reason">The reason for the batch failure.</param>
    /// <param name="innerException">The inner exception.</param>
    public BatchAddException(
        IEnumerable<TKey> failedKeys,
        BatchAddFailureReason reason,
        Exception innerException)
        : base(BuildMessage(failedKeys, reason), innerException)
    {
        FailedKeys = failedKeys.ToList().AsReadOnly();
        Reason = reason;
    }

    /// <summary>
    /// Gets the keys that caused the batch operation to fail.
    /// </summary>
    public IReadOnlyList<TKey> FailedKeys { get; }

    /// <summary>
    /// Gets the reason for the batch failure.
    /// </summary>
    public BatchAddFailureReason Reason { get; }

    private static string BuildMessage(IEnumerable<TKey> failedKeys, BatchAddFailureReason reason)
    {
        string keyList = string.Join(", ", failedKeys.Take(5));
        int count = failedKeys.Count();
        string suffix = count > 5 ? $" and {count - 5} more" : string.Empty;

        return reason switch
        {
            BatchAddFailureReason.DuplicateKeysInBatch =>
                $"Batch contains duplicate keys: {keyList}{suffix}",
            BatchAddFailureReason.DuplicateKeysInStore =>
                $"Keys already exist in store: {keyList}{suffix}",
            BatchAddFailureReason.TransactionAborted =>
                $"Transaction was aborted. Affected keys: {keyList}{suffix}",
            BatchAddFailureReason.PartialFailure =>
                $"Partial failure during batch operation. Failed keys: {keyList}{suffix}",
            _ => $"Batch add failed for keys: {keyList}{suffix}"
        };
    }
}
```

---

### 3. BatchAddFailureReason

**Purpose**: Enumeration of reasons a batch add operation can fail.

**Location**: `Hexalith.KeyValueStorages.Abstractions/Exceptions/BatchAddFailureReason.cs`

```csharp
/// <summary>
/// Specifies the reason a batch add operation failed.
/// </summary>
public enum BatchAddFailureReason
{
    /// <summary>
    /// The batch contained duplicate keys within itself.
    /// </summary>
    DuplicateKeysInBatch = 0,

    /// <summary>
    /// One or more keys in the batch already exist in the store.
    /// </summary>
    DuplicateKeysInStore = 1,

    /// <summary>
    /// The underlying transaction was aborted (for ACID-compliant providers).
    /// </summary>
    TransactionAborted = 2,

    /// <summary>
    /// A partial failure occurred during the batch operation.
    /// For non-transactional providers, this indicates compensating rollback was performed.
    /// </summary>
    PartialFailure = 3
}
```

---

## Modified Entities

### 1. IKeyValueStore<TKey, TState> (Extended)

**Location**: `Hexalith.KeyValueStorages.Abstractions/IKeyValueStore{TKey,TState}.cs`

**New Members**:

```csharp
/// <summary>
/// Gets the capabilities of this provider.
/// </summary>
ProviderCapabilities Capabilities { get; }

/// <summary>
/// Asynchronously adds multiple key/value pairs in a single atomic operation.
/// </summary>
/// <param name="items">The key-value pairs to add.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>
/// A collection of ETags for each successfully added item, in the same order as the input.
/// </returns>
/// <exception cref="BatchAddException{TKey}">
/// Thrown when the batch contains duplicate keys (within batch or in store),
/// or when the transaction fails.
/// </exception>
/// <remarks>
/// This operation is atomic: either all items are added or none are.
/// For providers with native transaction support, full ACID guarantees apply.
/// For other providers, compensating transactions ensure atomicity.
/// </remarks>
Task<IReadOnlyList<string>> AddRangeAsync(
    IEnumerable<(TKey Key, TState Value)> items,
    CancellationToken cancellationToken);
```

---

### 2. KeyValueStore<TKey, TState> (Abstract Base)

**Location**: `Hexalith.KeyValueStorages/KeyValueStore{TKey,TState}.cs`

**New Abstract/Virtual Members**:

```csharp
/// <summary>
/// Gets the capabilities of this provider.
/// </summary>
public abstract ProviderCapabilities Capabilities { get; }

/// <summary>
/// Asynchronously adds multiple key/value pairs in a single atomic operation.
/// </summary>
/// <param name="items">The key-value pairs to add.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>
/// A collection of ETags for each successfully added item, in the same order as the input.
/// </returns>
public abstract Task<IReadOnlyList<string>> AddRangeAsync(
    IEnumerable<(TKey Key, TState Value)> items,
    CancellationToken cancellationToken);

/// <summary>
/// Validates the batch for duplicate keys within itself.
/// </summary>
/// <param name="items">The items to validate.</param>
/// <exception cref="BatchAddException{TKey}">
/// Thrown when duplicate keys are found within the batch.
/// </exception>
protected void ValidateBatchNoDuplicates(IEnumerable<(TKey Key, TState Value)> items)
{
    var keys = items.Select(i => i.Key).ToList();
    var duplicates = keys.GroupBy(k => k)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();

    if (duplicates.Count > 0)
    {
        throw new BatchAddException<TKey>(
            duplicates,
            BatchAddFailureReason.DuplicateKeysInBatch);
    }
}
```

---

## Existing Entities (Unchanged)

### StateBase

Used as-is. Each item in a batch uses `StateBase` with its own `Etag` and `TimeToLive`.

### DuplicateKeyException<TKey>

Retained for single-key operations. `BatchAddException<TKey>` used for batch operations.

### ConcurrencyException<TKey>

Retained for ETag mismatch scenarios. Not applicable to batch add (new keys only).

---

## Entity Relationships

```text
┌─────────────────────────────────┐
│  IKeyValueStore<TKey, TState>   │
│  ─────────────────────────────  │
│  + Capabilities                 │◄───────────────┐
│  + AddRangeAsync()              │                │
└─────────────────────────────────┘                │
              ▲                                    │
              │ implements                         │
              │                                    │
┌─────────────────────────────────┐    ┌─────────────────────────┐
│  KeyValueStore<TKey, TState>    │    │   ProviderCapabilities  │
│  (abstract base)                │───►│   ──────────────────    │
│  ─────────────────────────────  │    │   SupportsAcidTxn       │
│  + ValidateBatchNoDuplicates()  │    │   SupportsBatchOps      │
└─────────────────────────────────┘    │   MaxBatchSize          │
              ▲                        └─────────────────────────┘
              │ extends
    ┌─────────┴─────────┬──────────────┬─────────────────┐
    │                   │              │                 │
┌───────────┐    ┌───────────┐  ┌───────────┐    ┌───────────┐
│ InMemory  │    │  JsonFile │  │   Redis   │    │ DaprActor │
│ KVStore   │    │  KVStore  │  │  KVStore  │    │  KVStore  │
└───────────┘    └───────────┘  └───────────┘    └───────────┘
```

---

## Validation Rules Summary

| Entity | Rule | Enforcement |
|--------|------|-------------|
| Batch items | No duplicate keys within batch | Pre-validation, throws `BatchAddException` |
| Batch items | No keys exist in store | Pre-validation, throws `BatchAddException` |
| Batch items | All values non-null | ArgumentNullException in each provider |
| Batch size | ≤ MaxBatchSize (default 1000) | Configurable via settings |
| ETags | Generated per item on success | Provider implementation |
| TTL | Honored per item independently | Provider implementation |
