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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
