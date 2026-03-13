# Cabazure.Test migration notes

- Cabazure.Messaging test projects should reference `Cabazure.Test` 1.0.1 and `xunit.v3` 3.2.2 when migrating off `Atc.Test`.
- Atc.Test `[AutoRegister]` custom specimen builders were replaced with assembly-local `TestInitializer.cs` files that register builders through `FixtureFactory.Customizations.Add(...)`.
- EventHub tests also require a project-wide `EventHubsRetryOptions` factory to avoid invalid AutoFixture-generated retry option values under the Cabazure.Test/xUnit 3 stack.
