<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.0 → 1.1.0 (MINOR - technology standards update)
Modified principles: None
Added sections: None
Removed sections: None
Updated sections:
  - Technology Standards: .NET 8.0 → .NET 10.0, C# 12 → C# 13
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ (compatible - uses placeholder for language/version)
  - .specify/templates/spec-template.md ✅ (compatible - technology-agnostic)
  - .specify/templates/tasks-template.md ✅ (compatible - uses placeholder for language)
Follow-up TODOs: None
-->

# Hexalith.KeyValueStorages Constitution

## Core Principles

### I. Test-Driven Development (NON-NEGOTIABLE)

All new features and bug fixes MUST follow strict TDD methodology:

- Tests MUST be written before implementation code
- Tests MUST fail before implementation begins (Red phase)
- Implementation MUST only satisfy the failing tests (Green phase)
- Refactoring MUST NOT change test outcomes (Refactor phase)
- All public APIs MUST have corresponding unit tests
- Integration tests MUST cover cross-provider scenarios

**Rationale**: TDD ensures correctness, prevents regressions, and drives clean API design. A key-value storage library is foundational infrastructure where bugs have cascading effects on dependent applications.

### II. Async-First API Design

All public APIs MUST be asynchronous by default:

- All I/O-bound operations MUST return `Task<T>` or `ValueTask<T>`
- All async methods MUST accept `CancellationToken` as the final parameter
- Synchronous wrappers are FORBIDDEN in public APIs
- Async method names MUST end with `Async` suffix
- `ConfigureAwait(false)` MUST be used in library code

**Rationale**: Storage operations are inherently I/O-bound. Async-first design prevents thread pool starvation and enables efficient scaling in high-throughput scenarios.

### III. Provider Extensibility

The library MUST support easy implementation of custom storage providers:

- All storage backends MUST implement `IKeyValueStore<TKey, TValue>`
- Provider implementations MUST be isolated in separate NuGet packages
- Core abstractions MUST NOT depend on concrete implementations
- New providers MUST require only interface implementation, not inheritance
- Provider registration MUST use standard .NET dependency injection patterns

**Rationale**: Different applications have different storage requirements. The library succeeds by enabling diverse backends while maintaining a consistent API contract.

### IV. Thread Safety

All implementations MUST support concurrent access:

- Public methods MUST be safe for concurrent invocation
- Internal state MUST be protected with appropriate synchronization
- Lock contention MUST be minimized through fine-grained locking or lock-free techniques
- Thread safety guarantees MUST be documented in XML comments
- Race conditions MUST be detected via concurrent integration tests

**Rationale**: Key-value stores are frequently accessed by multiple threads or async operations simultaneously. Thread safety is a correctness requirement, not an optimization.

### V. Optimistic Concurrency

ETags MUST be used for all mutable operations:

- `AddAsync` MUST return the initial ETag
- `SetAsync` MUST validate the provided ETag and return the new ETag
- `RemoveAsync` MUST validate the provided ETag
- ETag mismatches MUST throw `ConcurrencyException`
- ETags MUST be opaque strings with no guaranteed format

**Rationale**: Distributed systems require conflict detection. Optimistic concurrency via ETags prevents silent data loss without the overhead of pessimistic locking.

### VI. Simplicity Over Abstraction

Implementation MUST favor clarity over cleverness:

- No feature MUST be added without a concrete use case
- Abstractions MUST be introduced only when three or more implementations exist
- Configuration MUST use simple, flat structures where possible
- Error messages MUST be actionable and include context
- YAGNI (You Aren't Gonna Need It) MUST guide feature decisions

**Rationale**: Library complexity becomes user complexity. Every abstraction layer is a maintenance burden and learning curve for consumers.

## Technology Standards

**.NET Version**: .NET 10.0 or later
**Language**: C# 13 with nullable reference types enabled
**Testing Framework**: xUnit with FluentAssertions
**Dependency Injection**: Microsoft.Extensions.DependencyInjection
**Serialization**: System.Text.Json (default), extensible for alternatives
**Code Analysis**: SonarCloud, Codacy, Coverity for static analysis
**Package Format**: NuGet with semantic versioning

## Development Workflow

### Code Review Requirements

- All changes MUST be submitted via pull request
- PRs MUST pass all CI checks before merge
- PRs MUST include tests for new functionality
- Breaking changes MUST be documented in PR description
- API changes MUST update XML documentation

### Quality Gates

- Code coverage MUST NOT decrease
- Static analysis MUST report zero new critical/blocker issues
- All tests MUST pass on Windows, Linux, and macOS
- NuGet package MUST build successfully

### Versioning Policy

- MAJOR: Breaking API changes, removed features
- MINOR: New features, new providers, backward-compatible additions
- PATCH: Bug fixes, documentation, performance improvements
- Pre-release versions use `-preview.N` suffix

## Governance

This constitution supersedes all other development practices for the Hexalith.KeyValueStorages project.

**Amendment Procedure**:

1. Propose changes via GitHub issue with `constitution` label
2. Changes MUST be discussed for minimum 7 days
3. Breaking principle changes require MAJOR version bump
4. All amendments MUST include migration guidance if applicable

**Compliance Review**:

- All PRs MUST verify compliance with Core Principles
- Violations MUST be documented and justified in Complexity Tracking section of plan.md
- Annual review of constitution relevance and principle effectiveness

**Version**: 1.1.0 | **Ratified**: 2026-01-04 | **Last Amended**: 2026-01-04
