# Implementation Plan: Atomic Add with ACID Support

**Branch**: `003-atomic-add-acid` | **Date**: 2026-01-04 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-atomic-add-acid/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement batch add operations (`AddRangeAsync`) for key-value stores that guarantee atomicity - either all items are persisted or none are. For providers with native transaction support (Redis, Dapr State), leverage provider transactions for full ACID compliance. For providers without native transaction support (InMemory, JsonFile), implement compensating transactions with rollback on failure. The solution extends the existing `IKeyValueStore<TKey, TState>` interface following abstraction-first design principles.

## Technical Context

**Language/Version**: C# 14+ on .NET 10+
**Primary Dependencies**: StackExchange.Redis, Dapr.Actors, System.Text.Json
**Storage**: InMemory (Dictionary), Redis, JsonFile, DaprActors
**Testing**: XUnit with Shouldly assertions (per Hexalith standards)
**Target Platform**: Cross-platform (.NET 10)
**Project Type**: Multi-project library solution
**Performance Goals**: Batch add of 1000 items in under 5 seconds
**Constraints**: Thread-safe, ETag-based concurrency preserved, no automatic retries
**Scale/Scope**: 4 provider implementations, 1 abstraction package

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| **I. Abstraction-First Design** | ✅ PASS | New `AddRangeAsync` method will be defined in `IKeyValueStore<TKey, TState>` interface in Abstractions package before any implementation |
| **II. Concurrency Safety (NON-NEGOTIABLE)** | ✅ PASS | Batch operations will validate all ETags before any writes; duplicate keys within batch fail-fast; thread-safe implementations required |
| **III. Test-First Development** | ✅ PASS | Contract tests will be written first covering: batch add, atomicity rollback, duplicate key handling, ETag validation, TTL per item |
| **IV. Minimal API Surface** | ✅ PASS | Single new method `AddRangeAsync` with tuple-based signature matching existing patterns; `CancellationToken` as final parameter |
| **V. Configuration Over Code** | ✅ PASS | Batch size limits configurable per provider via existing `KeyValueStoreSettings`; no code changes for deployment differences |

**Technology Standards Compliance**:
- C# 14+ on .NET 10+: ✅
- XUnit with Shouldly: ✅
- System.Text.Json with polymorphic support: ✅
- Microsoft.Extensions.DependencyInjection: ✅
- IOptions pattern: ✅
- Dapr 1.16+: ✅

## Project Structure

### Documentation (this feature)

```text
specs/003-atomic-add-acid/
├── plan.md              # This file
├── research.md          # Phase 0 output - technology decisions
├── data-model.md        # Phase 1 output - entity definitions
├── quickstart.md        # Phase 1 output - usage examples
├── contracts/           # Phase 1 output - interface definitions
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/libraries/
├── Hexalith.KeyValueStorages.Abstractions/
│   ├── IKeyValueStore{TKey,TState}.cs     # MODIFY: Add AddRangeAsync method
│   ├── ProviderCapabilities.cs             # NEW: Transaction capability info
│   └── Exceptions/
│       ├── BatchAddFailureReason.cs        # NEW: Failure reason enum
│       └── BatchAddException.cs            # NEW: Batch-specific exception
│
├── Hexalith.KeyValueStorages/
│   ├── KeyValueStore{TKey,TState}.cs       # MODIFY: Abstract base implementation
│   └── InMemory/
│       └── InMemoryKeyValueStore.cs        # MODIFY: Add batch with lock-based atomicity
│
├── Hexalith.KeyValueStorages.Files/
│   └── FileKeyValueStorage.cs              # MODIFY: Add batch with temp file atomicity
│
├── Hexalith.KeyValueStorages.RedisDatabase/
│   └── RedisKeyValueStore{TKey,TState}.cs  # MODIFY: Add batch with Redis MULTI/EXEC
│
└── Hexalith.KeyValueStorages.DaprComponents/
    ├── DaprActorKeyValueStore{TKey,TState}.cs  # MODIFY: Add batch with Dapr transactions
    └── Actors/
        └── IKeyValueStoreActor{TState}.cs      # MODIFY: Add batch actor method

test/Hexalith.KeyValueStorages.Tests/
├── BatchAddTests/                          # NEW: Batch operation contract tests
│   ├── BatchAddAsyncTests.cs               # Batch add behavior tests
│   ├── BatchAtomicityTests.cs              # Rollback/all-or-nothing tests
│   └── BatchValidationTests.cs             # Duplicate key, empty batch tests
└── [existing test files...]
```

**Structure Decision**: Follows existing Hexalith vertical slice architecture. New code integrates into existing packages rather than creating new projects, preserving the established abstraction layer pattern.

## Complexity Tracking

> **No constitution violations identified.** Feature aligns with all five principles.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
