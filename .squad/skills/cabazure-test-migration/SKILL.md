---
name: "cabazure-test-migration"
description: "Pattern for migrating Cabazure library test projects from Atc.Test to Cabazure.Test"
domain: "testing"
confidence: "high"
source: "Cabazure.Messaging migration work"
---

## Context

Use this pattern when moving a Cabazure test assembly from `Atc.Test` to `Cabazure.Test`.

## Patterns

### Package and using changes

- Replace `Atc.Test` with `Cabazure.Test` and move `xunit` package references to `xunit.v3`.
- Update global usings from `Atc.Test` to `Cabazure.Test`.
- Switch `AutoFixture.Xunit2` global usings to `AutoFixture.Xunit3`.

### Project-wide customizations

- Remove `[AutoRegister]` from specimen builders.
- Add a `TestInitializer.cs` per test assembly that uses `[ModuleInitializer]` and `FixtureFactory.Customizations.Add(...)` to register custom `ISpecimenBuilder` instances.
- If Azure SDK option types fail AutoFixture construction under xUnit 3, register safe defaults in the initializer, e.g. `FixtureFactory.Customizations.Add(_ => new EventHubsRetryOptions());`.
- When a customization only needs to check the requested type, prefer `SpecimenRequestHelper.GetRequestType(request)` over local `IsRequestFor(...)` helpers.
- When a customization only creates one type, consider `TypeCustomization<T>` (or `FixtureFactory.Customizations.Add<T>(...)`) instead of a bespoke `ISpecimenBuilder`.
- When documenting or implementing `TypeCustomization<T>`, show the real API shape: `new TypeCustomization<T>(f => ...)` or a subclass constructor calling `: base(f => ...)`. Do not document it as an overridable `Create(IFixture)` base class unless the library actually exposes that pattern.

### API renames

- `ReceivedCallWithArgument<T>()` becomes `ReceivedArg<T>()`.
- `ReceivedCallsWithArguments<T>()` becomes `ReceivedArgs<T>()`.
- `WaitForCallForAnyArgs(...)` becomes `WaitForReceivedWithAnyArgs(...)`.
- `InvokeProtectedMethod(...)` becomes `InvokeProtected(...)` or `InvokeProtectedAsync(...)` depending on the target method shape.

### Fluent matcher cleanup

- When a test verifies exactly one received call and then immediately inspects the captured argument with `ReceivedArg<T>()`, prefer `FluentArg.Match<T>(...)` and keep the FluentAssertions assertion inline in the `Received(...)` call.
- Keep `ReceivedArgs<T>()`-based flows for batch/loop assertions where calls are counted first and the collected arguments are intentionally asserted together afterward.
- When documenting `FluentArg.Match<T>`, use assertion lambdas (`arg => arg.Property.Should().Be(value)`), not boolean predicates (`arg => arg.Property == value`). The actual API accepts `Action<T>`.

### Migration guide writing

- Keep Cabazure.Test migration guidance transport-agnostic unless a backend-specific quirk is genuinely unique and clearly labeled as an example.
- Keep migration-guide tip sections concise; prefer short, reusable bullets over backend-specific narrative examples.
- Before approving migration-guide examples, verify they compile against the current public API surface in `src\Cabazure.Test\`.
- For `FluentArg.Match<T>`, document assertion lambdas (`arg => arg.Should()...`), not boolean predicates.
- For `TypeCustomization<T>`, document the constructor/factory pattern (`new TypeCustomization<T>(f => ...)` or `: base(f => ...)`), not an overridable `Create(IFixture)` model.

### xUnit 3 analyzer expectations

- xUnit 3 analyzers reject placeholder cancellation tokens in test method calls. Use `TestContext.Current.CancellationToken` in NSubstitute setups and verifications that invoke APIs taking a `CancellationToken`.

## Examples

```csharp
internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        FixtureFactory.Customizations.Add(new QueueModelsGenerator());
        FixtureFactory.Customizations.Add(_ => new EventHubsRetryOptions());
    }
}
```

```csharp
public sealed class EventDataCustomization : TypeCustomization<EventData>
{
    public EventDataCustomization()
        : base(_ => new EventData(BinaryData.FromString("...")))
    {
    }
}
```

```csharp
processor.Received(1).ProcessAsync(
    message,
    FluentArg.Match<MessageMetadata>(metadata =>
        metadata.TenantId.Should().Be(expectedTenantId)),
    cancellationToken);
```

## Anti-Patterns

- Do not keep `[AutoRegister]` attributes after moving to `Cabazure.Test`.
- Do not leave `AutoFixture.Xunit2` or `xunit` v2 references in migrated test projects.
- Do not publish migration-guide examples that use unverified API shapes.
- Do not present backend-specific cleanup tips as if they were universal migration requirements.
