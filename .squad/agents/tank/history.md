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

- **[2026-03-13 Tank] Cabazure.Test MIGRATING.md Session — COMPLETE:**
   - **Phase 1:** Reviewed Oracle's draft "Tips from Migration Experience" section
   - **Phase 2:** REJECTED Oracle draft for API accuracy failures and scope overfitting
   - **Phase 3:** Reviewed Trinity's revision after generalization and API correction
   - **Phase 4:** APPROVED Trinity's final version — API-accurate, project-agnostic, suitable for future migrations
   - **Evidence:** All examples verified against Cabazure.Test source; no backend-specific framing; guidance applicable to any Atc.Test → Cabazure.Test migration
   - **Status:** ✅ COMPLETE — Cabazure.Test MIGRATING.md updated with documented decisions archived
- **[2026-03-13 Tank] Cabazure.Test MIGRATING.md Review — REJECTED:**
  - Reviewed `C:\Users\ricky\Projects\Cabazure\Cabazure.Test\MIGRATING.md` update adding "Tips from Migration Experience".
  - **Found API-invalid examples:** `FluentArg.Match<T>` accepts an assertion `Action<T>`, not a boolean predicate; the new example `m => m.TenantId == expectedTenantId` is wrong against `src\Cabazure.Test\FluentArg.cs`.
  - **Found customization example invalid:** `TypeCustomization<T>` is factory-based (`new TypeCustomization<T>(f => ...)` or `: base(f => ...)`), not an overridable `Create(IFixture)` pattern; the new sample would not compile against `src\Cabazure.Test\Customizations\TypeCustomization.cs`.
  - **Scope issue:** Heading and wording are too EventHub-specific for a general migration guide. Keep the helper guidance, but frame it generically around specimen builders/customizations.
  - Required follow-up: different author should revise the doc before approval.

- **[2026-03-13 Tank] Cabazure.Test MIGRATING.md Final Review — APPROVED:**
  - Re-reviewed `C:\Users\ricky\Projects\Cabazure\Cabazure.Test\MIGRATING.md` after the migration-tip rewrite.
  - The new section is now general-purpose: no backend-specific wording remains, and the guidance is framed around generic specimen customization and argument-verification patterns.
  - API accuracy verified against source:
    - `FluentArg.Match<T>` takes an `Action<T>` assertion delegate (`C:\Users\ricky\Projects\Cabazure\Cabazure.Test\src\Cabazure.Test\FluentArg.cs`).
    - `FixtureFactory.Customizations.Add<T>(Func<IFixture, T>)` and `TypeCustomization<T>` are valid supported customization paths (`C:\Users\ricky\Projects\Cabazure\Cabazure.Test\src\Cabazure.Test\Customizations\FixtureCustomizationCollection.cs`, `...\TypeCustomization.cs`).
  - Latest added tips are concise, accurate, and suitable as future migration guidance rather than Cabazure.Messaging-specific advice.
## Learnings
- **[2026-03-13 Tank] Cabazure.Test MIGRATING.md Inline Matcher Tip Review — APPROVED:**
  - Reviewed `C:\Users\ricky\Projects\Cabazure\Cabazure.Test\MIGRATING.md` addition covering `Arg.Any<T>()` + `ReceivedCallWithArgument<T>()` / `ReceivedArg<T>()` migration to `FluentArg.Match<T>()`.
  - API wording is accurate to current Cabazure.Test source: `FluentArg.Match<T>(Action<T> assertion)` takes an assertion action, not a predicate, and the guide now says so explicitly.
  - Scope is correct: inline matcher guidance is limited to immediate assertions and explicitly preserves `ReceivedArg<T>()` / `ReceivedArgs<T>()` for reuse, transformation, and batch inspection.
  - Framing remains general-purpose; the new example uses neutral request terminology and does not drift into Cabazure.Messaging-specific or backend-specific guidance.

- **[2026-03-13 Tank] Final Inline Matcher Documentation Review — SESSION COMPLETE:**
  - Session arc: Oracle drafted → Tank rejected for API/scope issues → Trinity revised → Tank approved
  - Trinity's revision corrected FluentArg.Match<T> examples to action-assertion syntax, simplified TypeCustomization<T> to factory pattern, removed EventHub-specific framing
  - Final approval verified API accuracy against Cabazure.Test source, confirmed general-purpose framing suitable for future migrations
  - Status: ✅ APPROVED — Ready for Cabazure.Test publication
  - Session logged: orchestration-log/20260313T090706-tank.md, session-log/20260313T090706-inline-match-migration-guide.md
  - Decision merged to decisions.md

