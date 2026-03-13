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
