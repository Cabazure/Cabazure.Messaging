# GitHub Copilot instructions for Cabazure.Messaging

## Repository overview

This repository contains four related .NET libraries:

- `src/Cabazure.Messaging.Abstractions` for transport-agnostic contracts such as `IMessagePublisher<T>`, `IMessageProcessor<T>`, `IMessageProcessorService<TProcessor>`, `MessageMetadata`, and `PublishingOptions`
- `src/Cabazure.Messaging.EventHub` for Azure Event Hubs publishing and processing, including checkpointed and stateless processors
- `src/Cabazure.Messaging.ServiceBus` for Azure Service Bus publishing and processing
- `src/Cabazure.Messaging.StorageQueue` for Azure Storage Queue publishing and processing

Tests mirror the production packages under `test/`, and runnable examples live under `samples/` for each backend.

## Public API and package boundaries

- Keep cross-backend contracts in `Cabazure.Messaging.Abstractions`
- Put Event Hubs, Service Bus, and Storage Queue behavior only in their respective backend packages
- Prefer transport-agnostic APIs in consuming code. Use `IMessagePublisher<T>` and `IMessageProcessor<T>` unless a backend-specific feature is required
- When adding backend-specific behavior, expose it through the existing backend-specific option or metadata types instead of leaking it into abstractions
- If a public API changes, update the corresponding tests and samples for that backend, and update `README.md` when usage changes

## Dependency injection and builder conventions

- Registration entry points live in `DependencyInjection/ServiceCollectionExtensions.cs` and the corresponding `*Builder.cs` types
- Follow the existing builder style: fluent methods that register factories, providers, publishers, processors, and hosted services
- Publishers are typically registered twice: as the backend-specific publisher and as `IMessagePublisher<T>`
- Processors are registered as singletons and exposed through `IMessageProcessorService<TProcessor>`, then added as hosted services
- Respect named connections. Use the existing `ConnectionName` flow and `IOptionsMonitor<TOptions>.Get(ConnectionName)` pattern instead of inventing parallel configuration mechanisms

## Messaging behavior expectations

- Preserve consistent JSON serialization behavior through the configured `JsonSerializerOptions`
- Keep mapping between `PublishingOptions` and transport-native message metadata aligned across all backends
- Message processors must remain thread-safe because they are registered as singletons
- Always honor `CancellationToken`
- For Event Hubs, checkpointed processors require blob storage; use the stateless processor path only when checkpointing is intentionally not needed
- Prefer extending existing provider or factory abstractions before adding new top-level registration concepts

## Multi-tenant and metadata guidance

- This library should support tenant-aware publishing patterns cleanly
- When working on publishing flows, preserve support for `MessageId`, `CorrelationId`, `ContentType`, `PartitionKey`, and custom properties
- Do not hardcode tenant logic into abstractions, but do keep the APIs friendly to publishing-options providers that attach tenant metadata such as `TenantId` and `Database`

## Coding conventions

- Library projects target `netstandard2.0`; tests target `net9.0`
- Nullable reference types and implicit usings are enabled everywhere
- Follow the existing C# style from `.editorconfig`: file-scoped namespaces, 4-space indentation, `var` usage, and expression-bodied members when they improve clarity
- Match the existing implementation style that favors primary constructors and straightforward DI-driven classes
- Reuse existing factories, providers, registrations, builders, metadata types, and option objects instead of duplicating logic
- Keep changes focused and surgical. Do not introduce unrelated refactors

## Test conventions

- Add or update tests in the matching test project for the package you change
- Use the current test stack: xUnit, AutoFixture with AutoNSubstitute, Cabazure.Test, FluentAssertions, and NSubstitute
- Prefer `[Theory, AutoNSubstituteData]` with `[Frozen]` collaborators for unit tests
- Verify both payload serialization and Azure SDK metadata mapping when changing publishers or processors
- Cover both DI registration behavior and runtime behavior when modifying builder or processor infrastructure

## Validation

Before finishing changes, run the standard repo commands:

```powershell
dotnet restore
dotnet build --no-restore -c Release
dotnet test --no-build -c Release
```
