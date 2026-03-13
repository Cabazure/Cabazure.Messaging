# Project Context

- **Owner:** Ricky Kaare Engelharth
- **Project:** Cabazure.Messaging
- **Stack:** C#, .NET libraries, Azure SDKs, xUnit, GitHub Actions
- **Created:** 2026-03-13T07:41:24.960Z

## Learnings

- Library projects target `netstandard2.0` and the codebase favors file-scoped namespaces, nullable reference types, and DI-driven classes.
- Builder entry points and provider/factory patterns are the normal implementation seams.
- Cabazure.Messaging test projects live under `test\Cabazure.Messaging.*.Tests\` and migrated from `Atc.Test` to `Cabazure.Test` 1.0.1 on `feature/migrate-cabazure-test`.
- EventHub and StorageQueue test assemblies register custom AutoFixture specimen builders in `TestInitializer.cs` via `FixtureFactory.Customizations.Add(...)`; EventHub also needs a safe `EventHubsRetryOptions` factory for xUnit 3-era fixture creation.
- The migration touched `Cabazure.Messaging.*.Tests.csproj` files, swapped global usings to `Cabazure.Test` and `AutoFixture.Xunit3`, and replaced Atc-specific helpers like `ReceivedCallWithArgument`, `ReceivedCallsWithArguments`, `WaitForCallForAnyArgs`, and `InvokeProtectedMethod`.
- EventHub test customizations now prefer `Cabazure.Test.Customizations.SpecimenRequestHelper` over local `IsRequestFor` helpers, and `test\Cabazure.Messaging.EventHub.Tests\EventHubModelsGenerator.cs` is clearer as a `TypeCustomization<EventData>` than a hand-rolled `ISpecimenBuilder`.
- For Cabazure.Test matcher cleanup, migrate single-call `Arg.Any<T>()` plus later `ReceivedArg<T>()` validations to `FluentArg.Match<T>(...)` inline, but keep loop-based `ReceivedArgs<T>()` verification flows such as batch/foreach processor tests unchanged.

## Work Log

### 2026-03-13: Migration Execution Complete
- **Migration Implementation:** Fully executed by Trinity across 3 test backends (EventHub, ServiceBus, StorageQueue)
  - 44 test files migrated (API renames, package updates, initializer patterns)
  - xUnit 3 compliance: CancellationToken updated to TestContext.Current.CancellationToken
  - EventHub custom factory added for EventHubsRetryOptions
  - Build succeeds; 186 tests pass (79 EventHub, 66 ServiceBus, 41 StorageQueue)
- **Validation:** Confirmed by Tank and Oracle; zero Atc.Test residuals; documentation aligned
- **Commits:** 4ed715a (migration), b26e966 (docs), 2c6e1e1 (copilot guidance)

### 2026-03-13: FluentArgs.Match Migration & EventHub Customization Cleanup
- **FluentArgs.Match Refactoring:** Implemented conservative-scope migration (3 tests) in commit f917525
  - ServiceBusProcessorServiceTests.cs: 2 metadata assertion migrations
  - EventHubBatchHandlerTests.cs: 1 single-message metadata assertion migration
  - Correctly preserved: Publisher tests (require post-extraction transform), batch-style ReceivedArgs<T>() loops
  - Tank's audit identified 12 total safe candidates; Trinity migrated immediate "truly fits" 3-test subset
- **EventHub Customization Cleanup:** Commit bcc9821
  - BlobClientOptionsGenerator: Replaced local IsRequestFor<T>() with SpecimenRequestHelper.GetRequestType(request)
  - EventHubModelsGenerator: Simplified from custom ISpecimenBuilder to TypeCustomization<EventData> base class
  - Impact: -14 lines duplicate request-matching logic; behavioral equivalence verified
- **Approvals:** All work approved by Tank; 186 tests pass; zero regressions
- **Status:** ✅ COMPLETE — Feature branch ready for user merge decision
