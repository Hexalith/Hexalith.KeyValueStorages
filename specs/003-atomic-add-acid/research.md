# Research: Atomic Add with ACID Support

**Feature**: 003-atomic-add-acid
**Date**: 2026-01-04
**Status**: Complete

## Research Tasks

This document consolidates research findings for implementing atomic batch add operations across all key-value store providers.

---

## 1. Redis Transaction Mechanism

**Question**: How to implement atomic batch operations in Redis?

**Decision**: Use Redis MULTI/EXEC transactions via StackExchange.Redis `ITransaction` interface.

**Rationale**:
- Redis MULTI/EXEC provides atomicity - all commands execute together or none do
- StackExchange.Redis `ITransaction` maps directly to MULTI/EXEC semantics
- Existing codebase uses `IConnectionMultiplexer` which supports `CreateTransaction()`
- WATCH can be used for optimistic concurrency if needed, but not required for Add operations since we use `When.NotExists`

**Alternatives Considered**:
- **Lua scripts**: More powerful but adds complexity; MULTI/EXEC sufficient for batch add
- **Pipeline without transaction**: Faster but no atomicity guarantee
- **Individual operations with manual rollback**: Complex error handling, not truly atomic

**Implementation Pattern**:
```csharp
IDatabase db = _connectionMultiplexer.GetDatabase();
ITransaction transaction = db.CreateTransaction();

foreach (var (key, value) in items)
{
    // Queue operations
    _ = transaction.StringSetAsync(GetRedisKey(key), serializedValue, expiry, When.NotExists);
}

bool committed = await transaction.ExecuteAsync();
```

---

## 2. Dapr State Store Transactions

**Question**: How to leverage Dapr's transaction API for atomic batch operations?

**Decision**: Use Dapr State Management Transaction API (`ExecuteStateTransactionAsync`).

**Rationale**:
- Dapr provides built-in transaction support for state stores that support it
- Transaction API accepts a list of `StateTransactionRequest` operations
- Consistent with Dapr's design philosophy of abstracting infrastructure
- Current `DaprActorKeyValueStore` uses actor proxy; transactions require Dapr client directly

**Alternatives Considered**:
- **Actor-level transactions**: Dapr actors don't support multi-key transactions across actors
- **Sequential actor calls with compensation**: Complex, not truly ACID
- **Workflow-based saga pattern**: Overkill for batch add, adds latency

**Implementation Pattern**:
```csharp
var operations = items.Select(item => new StateTransactionRequest(
    key: GetDaprKey(item.Key),
    value: JsonSerializer.SerializeToUtf8Bytes(item.Value),
    operationType: StateOperationType.Upsert
)).ToList();

await _daprClient.ExecuteStateTransactionAsync(storeName, operations);
```

**Note**: Actor-based implementation may need refactoring to use `DaprClient` for transactions, or implement best-effort atomicity via compensating actions.

---

## 3. InMemory Atomicity Strategy

**Question**: How to ensure atomicity for in-memory batch operations?

**Decision**: Use `Lock` (C# 13+) with scope-based locking and pre-validation before any writes.

**Rationale**:
- Existing `InMemoryKeyValueStore` already uses `Lock` for thread safety
- Pre-validate all keys don't exist before writing any
- Single lock scope ensures no interleaving
- Simple rollback: just don't write if validation fails

**Alternatives Considered**:
- **ReaderWriterLockSlim**: More granular but unnecessary complexity for batch add
- **Concurrent collections with retry**: Doesn't guarantee atomicity
- **Two-phase commit simulation**: Overkill for single-process in-memory store

**Implementation Pattern**:
```csharp
using (_lock.EnterScope())
{
    // Phase 1: Validate all keys
    var duplicates = items.Where(i => _store.ContainsKey(GetKey(i.Key))).ToList();
    if (duplicates.Any())
        throw new BatchDuplicateKeyException(duplicates.Select(d => d.Key));

    // Phase 2: Write all (guaranteed atomic within lock)
    foreach (var (key, value) in items)
    {
        _store[GetKey(key)] = value with { Etag = GenerateEtag() };
    }
}
```

---

## 4. File-Based Atomicity Strategy

**Question**: How to ensure atomicity for file-based batch operations?

**Decision**: Write to temporary directory, then atomically move/rename on success.

**Rationale**:
- File systems don't support transactions, but directory rename is atomic on most filesystems
- Write all files to temp location with same structure
- On success, use atomic rename pattern
- On failure, delete temp directory (no cleanup needed on main store)

**Alternatives Considered**:
- **Write-ahead log (WAL)**: Adds complexity, overkill for simple use case
- **Individual files with compensation**: Complex rollback, window for inconsistency
- **SQLite for file storage**: Changes storage model entirely

**Implementation Pattern**:
```csharp
string tempDir = Path.Combine(GetDirectoryPath(), $".batch_{Guid.NewGuid()}");
Directory.CreateDirectory(tempDir);

try
{
    // Write all files to temp
    foreach (var (key, value) in items)
    {
        await WriteToTempAsync(tempDir, key, value);
    }

    // Move each file to final location atomically
    foreach (var file in Directory.GetFiles(tempDir))
    {
        File.Move(file, Path.Combine(GetDirectoryPath(), Path.GetFileName(file)));
    }
}
finally
{
    if (Directory.Exists(tempDir))
        Directory.Delete(tempDir, recursive: true);
}
```

---

## 5. Duplicate Key Detection Strategy

**Question**: How to efficiently detect duplicates within batch and against existing keys?

**Decision**: Two-phase validation - batch self-check first, then store check.

**Rationale**:
- Fail fast on batch self-duplicates (no I/O needed)
- Batch store check before any writes
- Return all duplicate keys in exception for caller context

**Implementation Pattern**:
```csharp
// Phase 1: Check for duplicates within batch
var batchKeys = items.Select(i => i.Key).ToList();
var duplicatesInBatch = batchKeys.GroupBy(k => k)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .ToList();

if (duplicatesInBatch.Any())
    throw new BatchDuplicateKeyException<TKey>(duplicatesInBatch, inBatch: true);

// Phase 2: Check against existing store
var existingKeys = new List<TKey>();
foreach (var key in batchKeys)
{
    if (await ContainsKeyAsync(key, cancellationToken))
        existingKeys.Add(key);
}

if (existingKeys.Any())
    throw new BatchDuplicateKeyException<TKey>(existingKeys, inBatch: false);
```

---

## 6. Return Type Design

**Question**: What should `AddRangeAsync` return?

**Decision**: Return `IReadOnlyList<string>` (list of ETags in same order as input).

**Rationale**:
- Maintains order correspondence with input for caller to track
- ETags are needed for subsequent operations (Set, Remove)
- Simple, lightweight return type
- Matches existing `AddAsync` pattern (returns single ETag)

**Alternatives Considered**:
- **BatchAddResult class**: More flexible but adds type overhead; not needed initially
- **Dictionary<TKey, string>**: Breaks order, adds memory overhead
- **void with out parameter**: Awkward async pattern

---

## 7. Provider Capabilities API

**Question**: How should providers expose their transaction capabilities?

**Decision**: Add `ProviderCapabilities` property to `IKeyValueStore` interface.

**Rationale**:
- Spec requirement FR-011: "System MUST provide a mechanism to query whether a provider supports true ACID transactions"
- Simple flags-based approach (SupportsTransactions, SupportsBatchOperations)
- Allows callers to make informed decisions about retry strategies

**Implementation Pattern**:
```csharp
public record ProviderCapabilities(
    bool SupportsAcidTransactions,
    bool SupportsBatchOperations,
    int? MaxBatchSize);

// On interface
ProviderCapabilities Capabilities { get; }
```

---

## 8. Exception Design

**Question**: What exception types are needed for batch operations?

**Decision**: Create `BatchAddException<TKey>` with detailed failure information.

**Rationale**:
- Must identify which keys caused failures (FR-003, FR-004)
- Must distinguish between "duplicates in batch" vs "duplicates in store"
- Existing `DuplicateKeyException<TKey>` handles single key; batch needs collection

**Implementation Pattern**:
```csharp
public class BatchAddException<TKey> : Exception
{
    public IReadOnlyList<TKey> FailedKeys { get; }
    public BatchAddFailureReason Reason { get; }
}

public enum BatchAddFailureReason
{
    DuplicateKeysInBatch,
    DuplicateKeysInStore,
    PartialFailure,
    TransactionAborted
}
```

---

## Summary of Decisions

| Area | Decision | Confidence |
|------|----------|------------|
| Redis transactions | MULTI/EXEC via ITransaction | High |
| Dapr transactions | ExecuteStateTransactionAsync | Medium (needs actor refactor assessment) |
| InMemory atomicity | Lock with pre-validation | High |
| File atomicity | Temp directory + atomic move | High |
| Duplicate detection | Two-phase validation | High |
| Return type | IReadOnlyList<string> (ETags) | High |
| Capabilities API | ProviderCapabilities record | High |
| Exception design | BatchAddException<TKey> | High |

---

## Open Questions Resolved

1. ✅ **Redis transaction pattern** - Use MULTI/EXEC
2. ✅ **Dapr transaction support** - Use state transaction API (may need client injection)
3. ✅ **InMemory lock strategy** - Existing Lock pattern sufficient
4. ✅ **File atomicity approach** - Temp directory with atomic moves
5. ✅ **Duplicate key handling** - Fail-fast with all duplicates listed
6. ✅ **Return type** - ETag list maintaining input order
7. ✅ **Capabilities query** - Property on interface
8. ✅ **Exception hierarchy** - New BatchAddException type
