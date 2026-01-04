# Feature Specification: Atomic Add with ACID Support

**Feature Branch**: `003-atomic-add-acid`
**Created**: 2026-01-04
**Status**: Draft
**Input**: User description: "implement Add methods that can insert in one atomic operation. For providers that support transactions, make the operation ACID."

## Clarifications

### Session 2026-01-04

- Q: What is the acceptable latency target for batch operations handling up to 1000 items? → A: Under 5 seconds for 1000 items (balanced for most use cases)
- Q: What observability approach should be implemented for batch operations? → A: Structured logging with correlation IDs for each batch operation
- Q: Should the system automatically retry failed batch operations? → A: No automatic retries - caller handles retry logic
- Q: Should batch Update and Delete be in scope for this feature? → A: Out of scope - only batch Add (Update/Delete are future work)
- Q: What method signature pattern for the batch add operation? → A: `AddRangeAsync(IEnumerable<(TKey Key, TState Value)> items)` - tuple-based

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Bulk Data Import (Priority: P1)

As a developer integrating with a key-value store, I need to add multiple items in a single atomic operation so that either all items are persisted or none are, preventing partial data states.

**Why this priority**: This is the core functionality requested. Without atomic batch operations, developers must handle complex rollback scenarios manually, leading to data inconsistency risks and increased code complexity.

**Independent Test**: Can be fully tested by calling the batch add method with multiple items and verifying all items are persisted together. Delivers immediate value by enabling reliable bulk data operations.

**Acceptance Scenarios**:

1. **Given** a key-value store with no existing items, **When** I call the atomic add method with 5 new key-value pairs, **Then** all 5 items are persisted and accessible in the store.

2. **Given** a key-value store with existing data, **When** I call the atomic add method with 3 new items where one key already exists, **Then** no items are added and a duplicate key error is returned indicating which key failed.

3. **Given** a key-value store, **When** I call the atomic add method with an empty collection, **Then** the operation completes successfully with no changes to the store.

4. **Given** a key-value store, **When** I call the atomic add method with 100 items and the operation fails midway (e.g., connection loss), **Then** none of the items are persisted (atomicity guarantee).

---

### User Story 2 - ACID-Compliant Batch Operations (Priority: P2)

As a developer using a transaction-capable provider (Redis, Dapr State Store), I need batch add operations to be ACID-compliant so that I can rely on strong consistency guarantees for critical data operations.

**Why this priority**: ACID compliance is essential for production systems handling financial, inventory, or other critical data. This builds on P1 by adding transaction support for capable providers.

**Independent Test**: Can be tested by verifying that concurrent batch operations either complete fully or roll back completely, with no intermediate states visible to other operations.

**Acceptance Scenarios**:

1. **Given** a Redis-backed store and two concurrent batch add operations with overlapping keys, **When** both operations execute simultaneously, **Then** one operation succeeds completely and the other fails with a concurrency error (isolation).

2. **Given** a Dapr state store with transaction support, **When** I add 10 items atomically, **Then** the operation uses the provider's native transaction mechanism (Atomicity, Consistency).

3. **Given** an ACID-compliant batch add operation that succeeds, **When** the system restarts immediately after, **Then** all added items are still present (Durability).

---

### User Story 3 - Graceful Degradation for Non-Transactional Providers (Priority: P3)

As a developer using an in-memory or file-based provider without native transaction support, I need batch operations to provide best-effort atomicity so that I get consistent behavior across all providers.

**Why this priority**: Not all providers support transactions, but developers need a consistent API. This story ensures the batch API works across all providers with clear expectations about guarantees.

**Independent Test**: Can be tested by calling batch add on the in-memory provider and verifying the documented behavior (either all succeed or rollback via compensation).

**Acceptance Scenarios**:

1. **Given** an in-memory store without native transactions, **When** I call the atomic add method, **Then** the operation uses application-level rollback on failure to maintain atomicity.

2. **Given** a file-based store, **When** I add multiple items atomically and one write fails, **Then** previously written items in that batch are cleaned up (compensating transaction).

3. **Given** any provider, **When** I query the provider's capabilities, **Then** I can determine whether it supports true ACID transactions or best-effort atomicity.

---

### Edge Cases

- What happens when the batch contains duplicate keys within the same request? The operation fails fast before attempting any writes, returning an error identifying the duplicate keys.
- How does the system handle partial network failures during distributed transactions? Full rollback occurs with appropriate error details including which items may have failed.
- What is the maximum batch size supported? Configurable via `ProviderCapabilities.MaxBatchSize` per provider (null = library default of 1000 items); exceeding the limit throws `ArgumentException` before any writes.
- How are ETags handled for batch operations? Each item in the batch receives its own ETag; the operation returns all ETags on success in the same order as input.
- What happens if TimeToLive differs across items in a batch? Each item's TTL is honored independently.
- What if the batch contains the same key multiple times? Validation fails before any write operations, returning an error with the duplicate key information.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a batch add method `AddRangeAsync(IEnumerable<(TKey Key, TState Value)> items)` that accepts a collection of key-value tuples and persists all items in a single logical operation.

- **FR-002**: System MUST guarantee that either all items in a batch are persisted or none are (atomicity).

- **FR-003**: System MUST throw a `BatchAddException<TKey>` with reason `DuplicateKeysInStore` containing all duplicate keys when any key in the batch already exists in the store.

- **FR-004**: System MUST validate that no duplicate keys exist within the batch itself before attempting any writes.

- **FR-005**: System MUST return a collection of ETags corresponding to each successfully added item, maintaining the same order as the input collection.

- **FR-006**: System MUST support batch add operations for all existing provider implementations (InMemory, Redis, JsonFile, DaprActor).

- **FR-007**: For providers with native transaction support (Redis, Dapr State), system MUST use the provider's transaction mechanism to ensure ACID compliance.

- **FR-008**: For providers without native transaction support (InMemory, JsonFile), system MUST implement compensating transactions (rollback previously written items on failure).

- **FR-009**: System MUST preserve existing ETag-based concurrency control semantics for individual items within a batch.

- **FR-010**: System MUST honor each item's individual TimeToLive setting within a batch operation.

- **FR-011**: System MUST provide a mechanism to query whether a provider supports true ACID transactions or best-effort atomicity.

- **FR-012**: System MUST emit structured log entries with a unique correlation ID for each batch operation, including operation start, completion status, item count, and failure details when applicable.

- **FR-013**: System MUST NOT automatically retry failed batch operations; retry logic is the caller's responsibility. Exceptions thrown must provide sufficient context for the caller to decide whether to retry.

### Key Entities

- **BatchAddRequest**: Represents a collection of key-value pairs to be added atomically. Contains validation logic for duplicate key detection within the batch.

- **BatchAddResult**: Contains the ETags for all successfully added items, or error information if the operation failed.

- **ProviderCapabilities**: Describes what transaction/atomicity guarantees a specific provider implementation offers (true ACID vs. best-effort).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can add up to 1000 items in a single batch operation with all-or-nothing semantics.

- **SC-002**: Batch operations on transaction-capable providers provide full ACID guarantees as verified by concurrent access tests.

- **SC-003**: When a batch operation fails, zero items from that batch are persisted in the store.

- **SC-004**: Batch add operations complete in under 5 seconds for 1000 items under normal conditions (linear scalability).

- **SC-005**: All existing single-item tests continue to pass after batch operations are added (backward compatibility).

- **SC-006**: Developers can programmatically determine a provider's transaction capabilities before executing batch operations.

## Out of Scope

- **Batch Update operations**: Atomic update of multiple existing items is future work.
- **Batch Delete operations**: Atomic deletion of multiple items is future work.
- **Batch mixed operations**: Combined add/update/delete in a single transaction is future work.
- **Cross-provider transactions**: Transactions spanning multiple provider instances are not supported.

## Assumptions

- Redis provider will use native Redis transactions for atomic batch operations.
- Dapr State Store provider will use Dapr's transaction API for atomic batch operations.
- InMemory provider will use lock-based synchronization with compensating rollback.
- File-based provider will write to temporary files first, then atomically move on success.
- The existing key-value store interface will be extended with new batch methods rather than creating a separate interface.
- Batch size limits are configurable per provider but have sensible defaults.
- Developers are responsible for splitting very large batches if they exceed provider limits.
