# Squad Decisions

## Active Decisions

### 2026-03-13: User Directive — Cabazure.Test Migration

**By:** Ricky Kaare Engelharth (via Copilot CLI)

**What:** Execute Cabazure.Messaging test migration from Atc.Test to Cabazure.Test using `..\Cabazure.Test\MIGRATION.md` as guidance. Work in a feature branch (`feature/migrate-cabazure-test`). Keep commits focused.

**Why:** Official user request to modernize test framework and align with Cabazure.Test 1.0.1. Captured for team memory and agent coordination.

**Status:** ✅ Implemented (Commits: 4ed715a, b26e966, 2c6e1e1)

---

### 2025-01-10: Cabazure.Test Migration Status & DI Test Helper Localization

**Decided:** 2025-01-10  
**Owner:** Tank (Tester & Reviewer)  
**Status:** Applied

**Context:** Migration from Atc.Test 1.1.18 → Cabazure.Test 1.0.1 already complete. Three backend test projects (EventHub, ServiceBus, StorageQueue) verified migrated.

**Decisions:**
1. **Migration Status: COMPLETE** — All package references, global usings, and API renames applied. Build and tests pass.
2. **DI Test Assertion Helpers: KEEP LOCAL** — FluentAssertionsExtensions.cs (identical across backends) intentionally remain backend-local, not extracted to Cabazure.Test, because they are backend-specific DI testing concerns.
3. **Future Test Authoring: Use Migration Map as Reference** — Created `.squad/migration-map-cabazure-test.md` for ongoing test development guidance.

**Why:** No code work needed; migration is done. DI helpers are specific to messaging backend testing. Migration map ensures future test authors follow consistent Cabazure.Test patterns without re-discovering rules.

**Rationale:**
- Migration verified complete via package audits and API usage scans
- DI helpers don't justify bloat to Cabazure.Test public API
- Migration map creates continuity across future test work

**Evidence:**
- All 3 test projects build successfully
- 186 tests pass (79 EventHub, 66 ServiceBus, 41 StorageQueue)
- Zero residual Atc.Test API usages
- All MIGRATING.md requirements satisfied

---

### 2026-03-13: Documentation Update — Test Framework Guidance

**Author:** Oracle (Documentation)  
**Date:** 2026-03-13  
**Status:** ✅ Completed (Commit 2c6e1e1)

**What:** Updated `.github/copilot-instructions.md` to reference `Cabazure.Test` instead of `Atc.Test` in the Test conventions section.

**Why:** Test framework migration changed the team's testing stack. Documentation must reflect actual project dependencies to ensure Copilot, team members, and future contributors follow correct conventions.

**Impact:**
- Copilot and team members reference the correct test library (Cabazure.Test 1.0.1)
- Documentation stays synchronized with actual project dependencies
- Reduces confusion during code review and onboarding

---

### 2025-01-10: Final Review Decision — Migration Approved

**Author:** Tank (Tester & Reviewer)  
**Date:** 2025-01-10  
**Status:** ✅ APPROVED

**What:** Branch `feature/migrate-cabazure-test` approved for user review.

**Evidence:**
- All 186 tests pass (79 EventHub, 66 ServiceBus, 41 StorageQueue)
- Zero residual Atc.Test API usages (verified via grep scan)
- All MIGRATING.md checklist items satisfied
- Build succeeds in Release configuration

**Non-Blocking Observations:**
- ServiceBus test project has no custom generators; TestInitializer.cs not needed (correct)
- DI test assertion helpers (FluentAssertionsExtensions.cs) intentionally remain backend-local (correct)

**Recommendation:** Ready for merge after user review.

---

### 2026-03-13: FluentArgs.Match Migration Scope Decision

**Owner:** Tank (Tester & Reviewer)  
**Date:** 2026-03-13  
**Status:** ✅ IMPLEMENTED

**Context:** User requested audit of test suite for legacy `Arg.Any<T>()` + `ReceivedArg<T>()` pattern candidates for refactoring to `FluentArgs.Match<T>()` inline matchers.

**Decision:** Migrate only "truly fits" cases — single argument extraction immediately followed by assertion, with no downstream reuse or transformation.

**Implementation:**
- 3 tests migrated (commit f917525):
  - `ServiceBusProcessorServiceTests.cs`: 2 metadata assertion tests
  - `EventHubBatchHandlerTests.cs`: 1 single-message metadata assertion test
- 12 additional safe candidates identified and deferred (audit map created)
- 7 not-applicable tests (call-verify-only) correctly excluded
- 2 tests with uncertain type-filter semantics deferred pending Cabazure.Test API clarification

**Rationale:** Conservative scope appropriate for maintenance refactoring. Publisher tests requiring post-extraction transformation (`.Single()`, secondary verification calls) correctly excluded. Batch-style `ReceivedArgs<T>()` loops correctly preserved to maintain clear iteration intent.

**Evidence:**
- Build: ✅ Success (Release configuration)
- Tests: ✅ 186 pass (79 EventHub, 66 ServiceBus, 41 StorageQueue)
- No regressions

---

### 2026-03-13: EventHub Test Customization Best Practices

**Owner:** Trinity (Implementer) & Tank (Reviewer)  
**Date:** 2026-03-13  
**Status:** ✅ APPROVED

**Context:** EventHub test customizations used local `IsRequestFor<T>()` helper implementation and custom `ISpecimenBuilder` where simpler approaches exist in Cabazure.Test.

**Decision:**
1. Prefer `SpecimenRequestHelper.GetRequestType(request)` over local `IsRequestFor<T>()` helpers
2. Prefer `TypeCustomization<T>` when specimen builder creates only one concrete type
3. These patterns keep request-matching in Cabazure.Test; local code focuses on specimen construction

**Implementation (commit bcc9821):**
- `BlobClientOptionsGenerator.cs`: Replaced local `IsRequestFor<T>()` with `SpecimenRequestHelper.GetRequestType(request)`
- `EventHubModelsGenerator.cs`: Simplified from custom `ISpecimenBuilder` to `TypeCustomization<EventData>` base class

**Impact:** -14 lines of duplicate request-matching logic; behavioral equivalence verified (broader Type request coverage, not narrower)

**Evidence:**
- Build: ✅ Success
- EventHub tests: ✅ 79 pass (no regressions)

---

### 2026-03-13: Final Approval — Feature Branch Ready for Merge

**Owner:** Tank (Tester & Reviewer)  
**Date:** 2026-03-13  
**Status:** ✅ APPROVED

**What:** Branch `feature/migrate-cabazure-test` approved for user merge after comprehensive review of FluentArgs migration and EventHub customizations.

**Verification:**
- Build succeeds (Release configuration)
- All 186 tests pass (79 EventHub, 66 ServiceBus, 41 StorageQueue)
- Zero residual Atc.Test API usages (grep verified)
- Migration scope correctly applied (conservative, justified)
- Customization cleanups behaviorally equivalent

**Recommendation:** Ready for user review and merge.

---

### 2026-03-13: Cabazure.Test MIGRATING.md — Migration Tips Documentation

**Author:** Oracle (Documentation), revised by Trinity (Implementation)  
**Date:** 2026-03-13  
**Status:** ✅ COMPLETED

**Context:** Following successful Cabazure.Messaging test migration (Atc.Test → Cabazure.Test 1.0.1), team identified two high-value patterns for documentation in `Cabazure.Test/MIGRATING.md`:
1. **Specimen Customization Patterns:** Prefer `SpecimenRequestHelper.GetRequestType()` for request-type filtering and `TypeCustomization<T>` base class for single-type builders
2. **Assertion Refactoring Scope:** Use `FluentArg.Match<T>()` only for single-argument extraction with inline assertions; preserve `ReceivedArg<T>()` and `ReceivedArgs<T>()` for reuse, transformation, and batch flows

**Revision History:**
- **Draft (Oracle):** Initial tips submitted; Tank identified API accuracy issues (FluentArg.Match<T> and TypeCustomization<T> examples did not match real API signatures) and scope overfitting (too EventHub-specific)
- **Revision (Trinity):** Corrected examples to match actual Cabazure.Test API; generalized scope from "EventHub Test Customization" to "Specimen Customization Patterns"; removed backend-specific framing
- **Approval (Tank):** Final review confirmed API accuracy and project-agnostic framing suitable for future migrations

**Evidence:**
- FluentArg.Match<T> now correctly uses assertion-action syntax: `m => m.TenantId.Should().Be(expectedTenantId)`
- TypeCustomization<T> now correctly uses constructor/factory patterns: `new TypeCustomization<T>(f => ...)` or subclass with `base(...)`
- Guidance framed as general migration advice, not Cabazure.Messaging-specific cleanup

**Impact:**
- Cabazure.Test MIGRATING.md updated with practical, API-accurate tips for customizing fixtures and assertions
- Future test authors (inside and outside Cabazure.Messaging) have reusable guidance
- No backend-specific examples; guidance applies to any Atc.Test → Cabazure.Test migration

---

### 2026-03-13: User Directive — Keep MIGRATING.md Project-Agnostic

**By:** Ricky Kaare Engelharth (via Copilot CLI)  
**Date:** 2026-03-13  
**Status:** ✅ APPLIED

**What:** Keep `Cabazure.Test/MIGRATING.md` general-purpose; do not reference backend-specific examples like EventHub tests.

**Why:** User constraint to ensure Cabazure.Test documentation remains broadly applicable across projects, not become a record of one specific migration.

**Applied In:** Trinity's revision phase; all EventHub-specific framing removed; guidance recast as universal specimen customization and assertion patterns.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
