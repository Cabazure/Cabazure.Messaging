# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Messaging
- **Stack:** C#, .NET libraries, Azure SDKs, xUnit, GitHub Actions
- **Created:** 2026-03-13T07:41:24.960Z

## Learnings

- Tests use xUnit with AutoFixture AutoNSubstitute, Cabazure.Test 1.0.1, FluentAssertions, and NSubstitute.
- Publisher and processor changes should be covered with payload serialization, metadata mapping, and DI registration tests where applicable.
- **[2025-01-10 Tank] Test Migration Reconnaissance:**
  - 3 active test projects (EventHub, ServiceBus, StorageQueue); 1 empty (Abstractions.Tests)
  - 44 total test files across backends; all use `[Theory, AutoNSubstituteData]` (40+ usages)
  - 2 uses of `[InlineAutoNSubstituteData(param)]` in EventHub and ServiceBus processor service tests
  - 3 FluentAssertionsExtensions.cs files (one per backend) with identical DI assertion helpers — must check if Cabazure.Test includes these or if they stay backend-local
  - 2 scenario test files use reflection to validate DI container setup; risk point if Cabazure.Test changes ServiceCollection handling
  - Metadata tests rely on AutoFixture auto-generation of Azure SDK types (EventData, BinaryData, ServiceBusReceivedMessage, QueueMessage)
  - No breaking custom test base classes or exotic assertions; migration should be mechanical if Cabazure.Test preserves AutoNSubstituteData, [InlineAutoNSubstituteData], Frozen, Substitute, NSubstitute.Received patterns
  - Build succeeds with current stack; test count and coverage baseline needed post-migration

- **[2025-01-10 Tank] Cabazure.Test Migration Map Created:**
  - Cabazure.Messaging **already migrated** from Atc.Test → Cabazure.Test 1.0.1; no code action needed.
  - API renames **fully applied**: `ReceivedArg<T>()`, `WaitForReceivedWithAnyArgs()`, `InvokeProtected()`.
  - No `AddTimeout()`, `HasProperties()`, or old `[AutoRegister]` attributes in codebase.
  - Test initializers correctly use `[ModuleInitializer]` pattern with `FixtureFactory.Customizations.Add()`.
  - DI test assertion helpers (FluentAssertionsExtensions.cs) intentionally kept backend-local; not Cabazure.Test responsibility.
  - Migration map document created at `.squad/migration-map-cabazure-test.md` for future reference and new test authors.

- **[2025-01-10 Tank] Final Migration Review — APPROVED:**
  - Reviewed branch `feature/migrate-cabazure-test` (commits `4ed715a`, `b26e966`, `2c6e1e1`).
  - All 3 test projects migrated: `Atc.Test 1.1.18` → `Cabazure.Test 1.0.1`, `xunit 2.9.3` → `xunit.v3 3.2.2`, `AutoFixture.Xunit2` → `AutoFixture.Xunit3`.
  - API renames verified: `ReceivedCallWithArgument` → `ReceivedArg`, `ReceivedCallsWithArguments` → `ReceivedArgs`, `WaitForCallForAnyArgs` → `WaitForReceivedWithAnyArgs`, `InvokeProtectedMethod` → `InvokeProtected`.
  - `[AutoRegister]` removed from 3 generators; `[ModuleInitializer]` pattern correctly implemented in EventHub and StorageQueue TestInitializer.cs files.
  - `default` CancellationToken placeholders replaced with `TestContext.Current.CancellationToken` (xUnit 3 requirement).
  - Inline `IsRequestFor<T>()` helper added to generators to replace removed `Atc.Test.Customizations.Generators` dependency.
  - Documentation updated (`.github/copilot-instructions.md`) to reference Cabazure.Test.
  - Build succeeds; **186 tests pass** (79 EventHub, 66 ServiceBus, 41 StorageQueue).
  - Grep confirms zero residual Atc.Test API usages.
  - Branch ready for user review.

- **[2026-03-13 Session Complete]**
  - Trinity delivered full migration implementation (commits 4ed715a, b26e966)
  - Oracle updated Copilot guidance (commit 2c6e1e1)
  - Tank approved and validated all work
  - Decision inbox merged; orchestration logs created; session documented
  - Branch awaiting user merge decision

- **[2026-03-13 Tank] EventHub Customization Cleanup Review — APPROVED:**
  - Reviewed commit `bcc9821` on `feature/migrate-cabazure-test`
  - `BlobClientOptionsGenerator`: Correctly replaced local `IsRequestFor<T>()` with `SpecimenRequestHelper.GetRequestType(request)` from Cabazure.Test
  - `EventHubModelsGenerator`: Correctly simplified from `ISpecimenBuilder` to `TypeCustomization<EventData>` base class
  - **Behavioral equivalence verified:** Old `IsRequestFor` handled `Type` and `SeededRequest`; new approach handles `ParameterInfo`, `PropertyInfo`, `FieldInfo`, `Type` — this is broader coverage, not narrower. `SeededRequest` is only used for seeded primitive creation (rare), not complex types like `EventData`
  - **TypeCustomization internally uses SpecimenRequestHelper** (confirmed in source), so the request-matching behavior is identical
  - Build succeeds; **79 EventHub tests pass** — no regressions
  - Code is cleaner: removes 14 lines of duplicated `IsRequestFor` logic, delegates type-matching to Cabazure.Test infrastructure

- **[2025-01-10 Tank] FluentArgs.Match Pattern Audit — COMPLETE:**
  - Audited full test suite (186 tests across 4 projects) for Atc.Test era `Arg.Any<T>()` + `ReceivedArg<T>()` pattern candidates.
  - **Finding: 12 tests SAFE for FluentArgs.Match migration** — all extract single argument types for metadata/payload assertions. All follow clean match-then-extract-then-assert pattern.
  - **Finding: 7 tests NOT APPLICABLE** — use `Arg.Any<T>()` for call verification only; no ReceivedArg extraction. Leave as-is.
  - **Finding: 2 tests REQUIRE ANALYSIS** — use multiple `Arg.Any<T>()` with multiple `ReceivedArgs<T>()` extractions. Need Cabazure.Test clarification that `.ReceivedArgs<T>()` is type-filtered (safe) not position-filtered (fragile).
  - **Key observation: No deprecated `ReceivedCallWithArgument<T>()` API usage.** All tests already use modern `ReceivedArg<T>()` API.
  - Audit report: `.squad/audits/tank-fluentargs-pattern-audit.md`
  - Safe migration candidates: ServiceBus Publisher (4), EventHub Publisher (5), StorageQueue Publisher (1), ServiceBus Processor (2), EventHub Batch Handler (2).

- **[2026-03-13 Tank] FluentArgs.Match Migration Review — APPROVED:**
  - Reviewed commit `f917525` on `feature/migrate-cabazure-test`
  - Trinity migrated 3 tests: 2 in `ServiceBusProcessorServiceTests.cs`, 1 in `EventHubBatchHandlerTests.cs`
  - **Scope Choice Validated:** Trinity correctly identified "truly fits" cases — processor metadata assertions that extract for immediate equivalence check with no downstream use
  - **Publisher tests correctly excluded:** EventHub publisher uses `.ReceivedArg<IEnumerable<EventData>>().Single()` requiring post-extraction transformation; ServiceBus publisher tests use extracted values in secondary mock verification calls
  - **Batch-style tests correctly preserved:** `ReceivedArgs<T>()` (plural) loops in batch handler and processor tests left unchanged
  - Build succeeds; **186 tests pass** — no regressions
  - Trinity's conservative scope is justified: migrated only pure inline-assertion cases, left transformation and reuse patterns alone
- **[2026-03-13 Tank] Session Arc Complete — Scribe Logistics:**
   - Orchestration logs created for Trinity, Tank audit, Tank final review phases
   - Session log written at `.squad/log/20260313-094859-fluentargs-migration.md`
   - Decision inbox (7 files) merged into `.squad/decisions.md` (3 new decision records captured)
   - Agent histories updated with multi-session arc context
   - All team decisions, artifacts, and work properly archived and documented
   - **Final Status:** ✅ APPROVED — Feature branch `feature/migrate-cabazure-test` ready for user merge decision
