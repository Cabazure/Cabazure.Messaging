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
