# Cabazure.Messaging - AI Coding Agent Instructions

## Architecture Overview

This is a **multi-provider Azure messaging library** with a unified abstraction layer:
- `Cabazure.Messaging.Abstractions` - Core interfaces (`IMessagePublisher<T>`, `IMessageProcessor<T>`)
- `Cabazure.Messaging.EventHub` - Azure Event Hub implementation
- `Cabazure.Messaging.ServiceBus` - Azure Service Bus implementation  
- `Cabazure.Messaging.StorageQueue` - Azure Storage Queue implementation

**Key Pattern**: Each provider implements the same abstractions but uses provider-specific configuration and builder patterns.

## Critical Fluent API Architecture

The library uses **builder pattern extensively** with three types of builders per provider:
1. **Service Builder** (`EventHubBuilder`) - Configures DI container via `AddCabazure*()` extensions
2. **Publisher Builder** (`EventHubPublisherBuilder<T>`) - Configures message properties/metadata at registration time
3. **Processor Builder** (`EventHubProcessorBuilder`) - Configures processing options and filters

**Example EventHub registration**:
```csharp
services.AddCabazureEventHub(b => b
    .Configure(o => o.WithConnection(conn).WithBlobStorage(blob))
    .AddPublisher<MyEvent>("hubName", p => p
        .WithPartitionKey(e => e.Id)
        .WithProperty(e => e.Timestamp))
    .AddProcessor<MyEvent, MyProcessor>("hubName", "consumerGroup"));
```

## Provider-Specific Patterns

### EventHub Specifics
- Requires **blob storage** for processor checkpointing (`WithBlobStorage()`)
- Publisher builders support **partition key factories** and **EventData modifiers**
- Processors need consumer groups, support stateless mode via `AddStatelessProcessor()`

### ServiceBus Specifics  
- Uses **topics/subscriptions** model
- No blob storage requirement for processors
- Supports message sessions and dead letter queues

### StorageQueue Specifics
- Simplest provider - basic queue operations
- No complex routing or partitioning concepts

## Dependency Injection Conventions

**Extensions live in `Microsoft.Extensions.DependencyInjection` namespace** (note the `#pragma warning disable IDE0130`).

**Service Registration Pattern**:
- `AddCabazure{Provider}()` registers core services + returns provider builder
- `AddPublisher<T>()` registers `IMessagePublisher<T>` and provider-specific publisher
- `AddProcessor<T, TProcessor>()` registers hosted service + `IMessageProcessorService<T>`

## Testing Patterns

**Custom FluentAssertions Extensions**: Each test project has `FluentAssertionsExtensions.cs` with DI-specific assertions:
```csharp
services.Should().Contain<IMessagePublisher<MyEvent>, EventHubPublisher<MyEvent>>(ServiceLifetime.Singleton);
```

**Build/Test Commands**:
- `dotnet build --no-restore -c:Release` 
- `dotnet test --no-build -c:Release --collect:"XPlat Code Coverage"`

## Code Generation Patterns

### Expression-Based Property Extraction
Publishers support extracting property names from expressions:
```csharp
.WithProperty(e => e.Timestamp) // Extracts "Timestamp" as property name
```
See `EventHubPublisherBuilder<T>.WithProperty(Expression<Func<TMessage, object>>)` for the pattern.

### Generic Type Erasure for Factories
Builders expose both generic and type-erased methods:
```csharp
public Func<object, string>? GetPartitionKeyFactory() 
    => PartitionKeyFactory == null ? null : o => PartitionKeyFactory.Invoke((TMessage)o);
```

## Message Processing Architecture

**Processor Interface**: `IMessageProcessor<TMessage>` with `ProcessAsync(message, metadata, cancellationToken)`

**Metadata Extraction**: Each provider extracts platform-specific metadata into unified `MessageMetadata` class with properties like `MessageId`, `CorrelationId`, `PartitionKey`, `Properties`.

**Error Handling**: Implement `IProcessErrorHandler` for custom error handling strategies.

## Sample Project Structure

`samples/` contains **Aspire-based examples** showing:
- Producer apps with publisher injection
- Processor apps with hosted service registration  
- Service defaults and app host projects for orchestration

When creating new samples, follow the `{Provider}.Producer` / `{Provider}.Processor` / `{Provider}.AppHost` naming pattern.

## Development Workflow

- **Clean builds**: `dotnet clean -c Release && dotnet nuget locals all --clear`
- **Solution file**: Use `Cabazure.Messaging.sln` 
- **Directory.Build.props**: Sets company info, analysis rules, and release settings
- **Coverage reports**: Generated to `.github/coveragereport/` by CI
