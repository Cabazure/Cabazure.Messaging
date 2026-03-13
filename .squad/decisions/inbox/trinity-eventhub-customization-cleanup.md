# EventHub customization cleanup

- Prefer `Cabazure.Test.Customizations.SpecimenRequestHelper` when an AutoFixture `ISpecimenBuilder` only needs to match a requested type instead of re-implementing local `IsRequestFor(...)` logic.
- If a test customization only produces one concrete type, prefer `TypeCustomization<T>` over a custom `ISpecimenBuilder`; it keeps the request-matching behavior in Cabazure.Test and leaves the local code focused on specimen construction.
- Applied in `test\Cabazure.Messaging.EventHub.Tests\BlobClientOptionsGenerator.cs` and `test\Cabazure.Messaging.EventHub.Tests\EventHubModelsGenerator.cs`.
