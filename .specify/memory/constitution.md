<!--
================================================================================
SYNC IMPACT REPORT
================================================================================
Version Change: 0.0.0 → 1.0.0 (MAJOR - initial ratification)
Modified Principles: N/A (initial version)
Added Sections:
  - Core Principles (5 principles)
  - Technology Standards
  - Quality Gates
  - Governance
Removed Sections: N/A (initial version)
Templates Requiring Updates:
  - .specify/templates/plan-template.md: ✅ Compatible (Constitution Check section exists)
  - .specify/templates/spec-template.md: ✅ Compatible (requirements align with principles)
  - .specify/templates/tasks-template.md: ✅ Compatible (test-first workflow supported)
  - .specify/templates/commands/*.md: N/A (no command files found)
Follow-up TODOs: None
================================================================================
-->

# Hexalith.KeyValueStorages Constitution

## Core Principles

### I. Abstraction-First Design

Every storage feature MUST be defined as an interface in `Hexalith.KeyValueStorages.Abstractions` before implementation.

- All public APIs MUST depend on abstractions, never concrete implementations
- Storage providers MUST implement `IKeyValueStore<TKey, TValue>` and `IKeyValueProvider` interfaces
- New storage backends MUST be pluggable without modifying existing code
- Consumers MUST be able to switch backends via dependency injection configuration alone

**Rationale**: Multiple storage backends (in-memory, file-based, Dapr) require a stable contract. Abstraction-first ensures backends remain interchangeable and testable in isolation.

### II. Concurrency Safety (NON-NEGOTIABLE)

All storage operations MUST support optimistic concurrency control via ETags.

- `Add`, `Set`, and `Remove` operations MUST validate ETags when provided
- ETag mismatches MUST throw `ConcurrencyException` with clear diagnostics
- All implementations MUST be thread-safe for concurrent read/write access
- State mutations MUST be atomic at the key level

**Rationale**: Key-value stores are inherently concurrent. Without strict ETag enforcement and thread safety, data corruption and lost updates become inevitable in production systems.

### III. Test-First Development

Tests MUST be written before implementation code. No exceptions.

- Unit tests use XUnit framework with Shouldly assertions (per Hexalith standards)
- Each storage provider MUST pass the same contract test suite
- Tests MUST cover: basic CRUD, ETag validation, TTL expiration, concurrent access
- Red-Green-Refactor cycle strictly enforced: tests fail first, then pass

**Rationale**: A storage library's reliability is paramount. Test-first ensures all edge cases (expiration, conflicts, missing keys) are considered before implementation, preventing production failures.

### IV. Minimal API Surface

Public APIs MUST be minimal, discoverable, and consistent across all providers.

- Core operations limited to: `AddAsync`, `GetAsync`, `TryGetAsync`, `SetAsync`, `RemoveAsync`, `ContainsKeyAsync`
- All async methods MUST accept `CancellationToken` as final parameter
- Method signatures MUST be identical across all `IKeyValueStore` implementations
- Avoid provider-specific methods in the public interface; use options/settings for customization

**Rationale**: Consumers should learn one API and use any backend. Divergent APIs per provider would defeat the purpose of the abstraction layer and increase integration burden.

### V. Configuration Over Code

Storage behavior MUST be configurable without code changes.

- Connection strings, paths, and timeouts MUST come from `IConfiguration` (e.g., `appsettings.json`)
- Default values MUST be sensible for local development (e.g., `./data` for file storage)
- Provider selection MUST be achievable via DI registration, not compile-time decisions
- Settings classes MUST be immutable records with validation

**Rationale**: Deployment environments vary (dev/staging/prod). Code changes for configuration defeat CI/CD automation and increase deployment risk.

## Technology Standards

**Language/Version**: C# 14+ on .NET 10+
**Testing Framework**: XUnit with Shouldly assertions
**Serialization**: System.Text.Json with polymorphic support
**DI Container**: Microsoft.Extensions.DependencyInjection
**Configuration**: Microsoft.Extensions.Configuration (IOptions pattern)
**Distributed Runtime**: Dapr 1.16+ (for DaprComponents package)

**Package Structure**:
| Package | Purpose |
|---------|---------|
| `Hexalith.KeyValueStorages.Abstractions` | Interfaces, base classes, exceptions |
| `Hexalith.KeyValueStorages` | In-memory implementation |
| `Hexalith.KeyValueStorages.Files` | JSON file-based implementation |
| `Hexalith.KeyValueStorages.DaprComponents` | Dapr state store integration |

## Quality Gates

All pull requests MUST pass before merge:

1. **Build**: `dotnet build` succeeds with zero warnings (treat warnings as errors)
2. **Tests**: All unit and integration tests pass (`dotnet test`)
3. **Coverage**: New code MUST have test coverage (measured via SonarCloud)
4. **Static Analysis**: Coverity, Codacy, and SonarCloud checks pass
5. **API Compatibility**: No breaking changes to public interfaces without MAJOR version bump
6. **Documentation**: Public APIs MUST have XML documentation comments

## Governance

This constitution supersedes all informal practices and ad-hoc decisions.

**Amendment Process**:
1. Propose change via pull request modifying this file
2. Document rationale and impact on existing code
3. Require approval from at least one maintainer
4. Update `CONSTITUTION_VERSION` following semantic versioning:
   - MAJOR: Principle removal, redefinition, or backward-incompatible governance change
   - MINOR: New principle added or existing principle materially expanded
   - PATCH: Clarifications, typo fixes, non-semantic refinements

**Compliance Review**:
- Code reviewers MUST verify PR compliance with all principles
- Violations MUST be justified in the Complexity Tracking section of implementation plans
- Unjustified violations block merge

**Commit Messages**: All commits MUST follow Angular Conventional Commits specification (per Hexalith.Builds/CLAUDE.md).

**Version**: 1.0.0 | **Ratified**: 2025-01-04 | **Last Amended**: 2025-01-04
