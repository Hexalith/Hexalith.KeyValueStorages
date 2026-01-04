# Tasks: Atomic Add with ACID Support

**Input**: Design documents from `/specs/003-atomic-add-acid/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested - test tasks are excluded.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Multi-project library solution**: `src/libraries/[ProjectName]/`
- **Tests**: `test/Hexalith.KeyValueStorages.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: New types and abstractions required by all providers

- [X] T001 [P] Create BatchAddFailureReason enum in src/libraries/Hexalith.KeyValueStorages.Abstractions/Exceptions/BatchAddFailureReason.cs
- [X] T002 [P] Create BatchAddException<TKey> class in src/libraries/Hexalith.KeyValueStorages.Abstractions/Exceptions/BatchAddException.cs
- [X] T003 [P] Create ProviderCapabilities record in src/libraries/Hexalith.KeyValueStorages.Abstractions/ProviderCapabilities.cs
- [X] T004 Add Capabilities property and AddRangeAsync method to IKeyValueStore<TKey,TState> interface in src/libraries/Hexalith.KeyValueStorages.Abstractions/IKeyValueStore{TKey,TState}.cs

**Checkpoint**: Abstractions complete - provider implementations can now begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base class implementation with shared validation logic

**⚠️ CRITICAL**: No provider implementations can begin until this phase is complete

- [X] T005 Add abstract Capabilities property to KeyValueStore<TKey,TState> base class in src/libraries/Hexalith.KeyValueStorages.Abstractions/KeyValueStore{TKey,TState}.cs
- [X] T006 Add abstract AddRangeAsync method to KeyValueStore<TKey,TState> base class in src/libraries/Hexalith.KeyValueStorages.Abstractions/KeyValueStore{TKey,TState}.cs
- [X] T007 Implement ValidateBatchNoDuplicates protected method in KeyValueStore<TKey,TState> base class in src/libraries/Hexalith.KeyValueStorages.Abstractions/KeyValueStore{TKey,TState}.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Bulk Data Import (Priority: P1) 🎯 MVP

**Goal**: Add multiple items in a single atomic operation with all-or-nothing semantics

**Independent Test**: Call batch add with multiple items and verify all items are persisted together or none are persisted on failure

### Implementation for User Story 1

- [X] T008 [US1] Implement AddRangeAsync in InMemoryKeyValueStore using Lock-based atomicity with pre-validation in src/libraries/Hexalith.KeyValueStorages/InMemory/InMemoryKeyValueStore.cs
- [X] T009 [US1] Add Capabilities property returning BestEffort to InMemoryKeyValueStore in src/libraries/Hexalith.KeyValueStorages/InMemory/InMemoryKeyValueStore.cs
- [X] T010 [US1] Implement AddRangeAsync in FileKeyValueStorage using temp directory and atomic file moves in src/libraries/Hexalith.KeyValueStorages.Files/FileKeyValueStorage.cs
- [X] T011 [US1] Add Capabilities property returning BestEffort to FileKeyValueStorage in src/libraries/Hexalith.KeyValueStorages.Files/FileKeyValueStorage.cs
- [X] T012 [US1] Add structured logging with correlation IDs for batch operations in InMemoryKeyValueStore in src/libraries/Hexalith.KeyValueStorages/InMemory/InMemoryKeyValueStore.cs
- [X] T013 [US1] Add structured logging with correlation IDs for batch operations in FileKeyValueStorage in src/libraries/Hexalith.KeyValueStorages.Files/FileKeyValueStorage.cs

**Checkpoint**: User Story 1 complete - InMemory and File providers support atomic batch add with best-effort atomicity

---

## Phase 4: User Story 2 - ACID-Compliant Batch Operations (Priority: P2)

**Goal**: Batch add operations with full ACID guarantees for transaction-capable providers

**Independent Test**: Verify concurrent batch operations either complete fully or roll back completely with no intermediate states

### Implementation for User Story 2

- [X] T014 [US2] Implement AddRangeAsync in RedisKeyValueStore using Redis MULTI/EXEC transactions in src/libraries/Hexalith.KeyValueStorages.RedisDatabase/RedisKeyValueStore{TKey,TState}.cs
- [X] T015 [US2] Add Capabilities property returning FullAcid to RedisKeyValueStore in src/libraries/Hexalith.KeyValueStorages.RedisDatabase/RedisKeyValueStore{TKey,TState}.cs
- [X] T016 [US2] Add structured logging with correlation IDs for batch operations in RedisKeyValueStore in src/libraries/Hexalith.KeyValueStorages.RedisDatabase/RedisKeyValueStore{TKey,TState}.cs
- [X] T017 [US2] DaprActorKeyValueStore updated - note: Dapr actors don't support cross-actor transactions, so uses BestEffort with compensating rollback in src/libraries/Hexalith.KeyValueStorages.DaprComponents/DaprActorKeyValueStore{TKey,TState}.cs
- [X] T018 [US2] Implement AddRangeAsync in DaprActorKeyValueStore using compensating transactions (Dapr actors are per-key, cannot do cross-actor ACID) in src/libraries/Hexalith.KeyValueStorages.DaprComponents/DaprActorKeyValueStore{TKey,TState}.cs
- [X] T019 [US2] Add Capabilities property returning BestEffort to DaprActorKeyValueStore (changed from FullAcid due to actor architecture limitation) in src/libraries/Hexalith.KeyValueStorages.DaprComponents/DaprActorKeyValueStore{TKey,TState}.cs
- [X] T020 [US2] Add structured logging with correlation IDs for batch operations in DaprActorKeyValueStore in src/libraries/Hexalith.KeyValueStorages.DaprComponents/DaprActorKeyValueStore{TKey,TState}.cs

**Checkpoint**: User Story 2 complete - Redis provider supports ACID-compliant batch add; Dapr uses best-effort with compensating rollback

---

## Phase 5: User Story 3 - Graceful Degradation for Non-Transactional Providers (Priority: P3)

**Goal**: Consistent API with clear capability reporting across all providers

**Independent Test**: Query provider capabilities and verify documented behavior matches actual atomicity guarantees

### Implementation for User Story 3

- [X] T021 [US3] Implement compensating transaction rollback in InMemoryKeyValueStore AddRangeAsync for partial failures in src/libraries/Hexalith.KeyValueStorages/InMemory/InMemoryKeyValueStore.cs
- [X] T022 [US3] Implement compensating transaction rollback in FileKeyValueStorage AddRangeAsync with temp file cleanup in src/libraries/Hexalith.KeyValueStorages.Files/FileKeyValueStorage.cs
- [X] T023 [US3] Add validation for MaxBatchSize limit (default 1000) to all providers in src/libraries/Hexalith.KeyValueStorages.Abstractions/KeyValueStore{TKey,TState}.cs

**Checkpoint**: User Story 3 complete - All providers have consistent API with clear capability reporting

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [X] T024 [P] Verify build succeeds for all projects with dotnet build
- [X] T025 [P] Run existing tests to ensure backward compatibility with dotnet test (102/104 passed - 2 pre-existing failures unrelated to batch add)
- [X] T026 Validate quickstart.md examples compile and execute correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (T001-T004) completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase (T005-T007) completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### Task Dependencies

```
T001, T002, T003 (parallel) → T004 → T005, T006, T007 (sequential)
                                    ↓
                    ┌───────────────┼───────────────┐
                    ↓               ↓               ↓
               US1 (T008-T013) US2 (T014-T020) US3 (T021-T023)
                    └───────────────┼───────────────┘
                                    ↓
                             T024, T025, T026
```

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent of US1
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Enhances US1 providers but independently testable

### Within Each User Story

- InMemory and File providers can be implemented in parallel within US1
- Redis and Dapr providers can be implemented in parallel within US2
- Each provider implementation: Core AddRangeAsync → Capabilities property → Logging

### Parallel Opportunities

- **Phase 1**: T001, T002, T003 can run in parallel (different files)
- **Phase 3 (US1)**: T008+T009 parallel with T010+T011 (different projects)
- **Phase 4 (US2)**: T014+T015+T016 parallel with T017+T018+T019+T020 (different projects)
- **Phase 6**: T024, T025 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch InMemory and File provider implementations together:
Task: "Implement AddRangeAsync in InMemoryKeyValueStore" (T008)
Task: "Implement AddRangeAsync in FileKeyValueStorage" (T010)

# Then add capabilities in parallel:
Task: "Add Capabilities property to InMemoryKeyValueStore" (T009)
Task: "Add Capabilities property to FileKeyValueStorage" (T011)
```

---

## Parallel Example: User Story 2

```bash
# Launch Redis and Dapr provider implementations together:
Task: "Implement AddRangeAsync in RedisKeyValueStore" (T014)
Task: "Update IKeyValueStoreActor interface" (T017) + "Implement AddRangeAsync in DaprActorKeyValueStore" (T018)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T007)
3. Complete Phase 3: User Story 1 (T008-T013)
4. **STOP and VALIDATE**: Test batch add on InMemory and File providers
5. Deploy/demo if ready - developers can use atomic batch add immediately

### Incremental Delivery

1. Complete Setup + Foundational → Abstraction layer ready
2. Add User Story 1 → Test independently → InMemory/File batch add (MVP!)
3. Add User Story 2 → Test independently → Redis/Dapr ACID batch add
4. Add User Story 3 → Test independently → Compensating transactions + capability queries
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (InMemory + File providers)
   - Developer B: User Story 2 (Redis + Dapr providers)
3. Developer A or B: User Story 3 (enhancements after US1/US2)

---

## Files Summary

### New Files (8)

| File | Task | Story |
|------|------|-------|
| Exceptions/BatchAddFailureReason.cs | T001 | Setup |
| Exceptions/BatchAddException.cs | T002 | Setup |
| ProviderCapabilities.cs | T003 | Setup |

### Modified Files (8)

| File | Tasks | Stories |
|------|-------|---------|
| IKeyValueStore{TKey,TState}.cs | T004 | Setup |
| KeyValueStore{TKey,TState}.cs | T005-T007, T023 | Foundation, US3 |
| InMemory/InMemoryKeyValueStore{TKey,TState}.cs | T008-T009, T012, T021 | US1, US3 |
| FileKeyValueStorage{TKey,TState}.cs | T010-T011, T013, T022 | US1, US3 |
| RedisKeyValueStore{TKey,TState}.cs | T014-T016 | US2 |
| Actors/IKeyValueStoreActor{TState}.cs | T017 | US2 |
| DaprActorKeyValueStore{TKey,TState}.cs | T018-T020 | US2 |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
