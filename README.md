[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Cabazure/Cabazure.Messaging/.github%2Fworkflows%2Fci.yml)](https://github.com/Cabazure/Cabazure.Messaging/actions/workflows/ci.yml)
[![GitHub Release Date](https://img.shields.io/github/release-date/Cabazure/Cabazure.Messaging)](https://github.com/Cabazure/Cabazure.Messaging/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Cabazure.Messaging?color=blue)](https://www.nuget.org/packages/Cabazure.Messaging)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cabazure.Messaging?color=blue)](https://www.nuget.org/stats/packages/Cabazure.Messaging?groupby=Version)

[![Branch Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Messaging/main/.github/coveragereport/badge_branchcoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Messaging/actions/workflows/ci.yml)
[![Line Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Messaging/main/.github/coveragereport/badge_linecoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Messaging/actions/workflows/ci.yml)
[![Method Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Messaging/main/.github/coveragereport/badge_methodcoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Messaging/actions/workflows/ci.yml)

# Cabazure.Messaging

Cabazure.Messaging is a set of libraries for handling message publishing and processing on Azure EventHub and ServiceBus.

The following packages are produced from this repository:
* **Cabazure.Messaging.Abstrations** - Publisher and processor abstractions
* **Cabazure.Messaging.EventHub** - Package for targeting Event Hub
* **Cabazure.Messaging.ServiceBus** - Package for targeting Service Bus

## Getting started

### 1. Add package reference

For targeting Azure Event Hub, add a reference to the `Cabazure.Messaging.EventHub` package. Please see the [Configure EventHub Connection](#2a-configure-eventhub-connection) section for how to setup the Event Hub package.

For targeting Azure Service Bus, add a reference to the `Cabazure.Messaging.ServiceBus` package. Please see the [Configure ServiceBus Connection](#2b-configure-servicebus-connection) section for how to setup the Service Bus package.

### 2a. Configure EventHub connection

Cabazure.Messaging.EventHub is configured by calling the `AddCabazureEventHub()` on the `IServiceCollection` during startup of your application, like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithSerializerOptions(JsonSerializerOptions.Web)
        .WithConnection(
            "eventhub1.servicebus.windows.net",
            new DefaultAzureCredential())
        .WithBlobStorage(
            new Uri("https://account1.blob.core.windows.net/container1"),
            new DefaultAzureCredential(),
            createIfNotExist: true)));
```

The `CabazureEventHubOptions` can also be configured using the `Microsoft.Extensions.Options` framework, by implementing `IConfigureOptions<CabazureEventHubOptions>`, and pass it to the `Configure<T>()` overload.

Multiple Event Hub connections are supported by passing a `connectionName` to the `AddCabazureEventHub()` method.

The Blob Storage configuration is only required if you need to run a message processor, as the storage is used for processor state.


### 2b. Configure ServiceBus connection

Cabazure.Messaging.ServiceBus is configured by calling the `AddCabazureServiceBus()` on the `IServiceCollection` during startup of your application, like this:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCabazureServiceBus(b => b
    .Configure(o => o
        .WithSerializerOptions(JsonSerializerOptions.Web)
        .WithConnection(
            "servicebus1.servicebus.windows.net",
            new DefaultAzureCredential()));
```

The `CabazureServiceBusOptions` can also be configured using the `Microsoft.Extensions.Options` framework, by implementing `IConfigureOptions<CabazureServiceBusOptions>`, and pass it to the `Configure<T>()` overload.

Multiple Service Bus connections are supported by passing a `connectionName` to the `AddCabazureServiceBus()` method.

### 3. Add a Publisher

To add a message publisher, the `AddPublisher<TMessage>()` method is called on the `EventHubBuilder` or `ServiceBusBuilder`, like this:

```csharp
// Event Hub publisher
builder.Services.AddCabazureEventHub(b => b
    .AddPublisher<MyEvent>("eventHub1");

// Service Bus publisher
builder.Services.AddCabazureServiceBus(b => b
    .AddPublisher<MyEvent>("topic1");
```

This will register an `IMessagePublisher<MyEvent>` service in the dependency injection container, allowing you to publish `MyEvent` messages using the `PublishAsync()` methods.

An optional builder can be passed to the `AddPublisher<TMessage>()` method. This can be used to setup default properties and partition key used when publishing, like this:

```csharp
// Event Hub publisher
builder.Services.AddCabazureEventHub(b => b
    .AddPublisher<MyEvent>("eventHub1",
    b => b
      .AddPartitionKey(m => m.PartitionKey)
      .AddProperty("Property1", m => m.Property1));

// Service Bus publisher
builder.Services.AddCabazureServiceBus(b => b
    .AddPublisher<MyEvent>("topic1",
    b => b
      .AddPartitionKey(m => m.PartitionKey)
      .AddProperty("Property1", m => m.Property1));
```

Properties and partition key can also be specified using the `PublishingOptions` that can be passed to the `IMessagePublisher<TMessage>.PublishAsync()` method when publishing messages.

### 4. Add a Processor

To add a processor, the `IMessageProcessor<TMessage>` should be implemented and registered by calling the `AddProcessor<TMessage, TProcessor>()` method on the `EventHubBuilder` or `ServiceBusBuilder`, like this:

```csharp
// Event Hub publisher
builder.Services.AddCabazureEventHub(b => b
    .AddProcessor<MyEvent, MyEventProcessor>(
        "eventhub1"
        "consumerGroup1");

// Service Bus publisher
builder.Services.AddCabazureServiceBus(b => b
    .AddProcessor<MyEvent, MyEventProcessor>(
        "topic1",
        "subscription1");
```

This will register an `IHostedService` that will start up with the application, and pass deserialized `MyEvent` messages to the `MyEventProcessor`.

If needed, the processor can be started and stopped by using the registered `IMessageProcessorService<MyEventProcessor>` service.

An optional builder can be passed to the `AddProcessor<TMessage, TProcessor>()` method. This can be used to configure processor options and configure property-based filters, like this:

```csharp
// Event Hub publisher
builder.Services.AddCabazureEventHub(b => b
    .AddProcessor<MyEvent, MyEventProcessor>(
        "eventhub1"
        "consumerGroup1",
        b => b
          .WithClientOptions(new EventProcessorClientOptions
          {
              PrefetchCount = 300,
          })
          .WithFilter(p => p.TryGetValue("Property1", out var v) && v is "Value1")));

// Service Bus publisher
builder.Services.AddCabazureServiceBus(b => b
    .AddProcessor<MyEvent, MyEventProcessor>(
        "topic1",
        "subscription1",
        b => b
          .WithClientOptions(new ServiceBusClientOptions
          {
              Identifier = "Processor1",
          })
          .WithProcessorOptions(new ServiceBusProcessorOptions
          {
              MaxConcurrentCalls = 16,
              PrefetchCount = 64,
          })
          .WithFilter(p => p.TryGetValue("Property1", out var v) && v is "Value1")));
```

## Samples

Please see the [samples](samples/) folder for sample implementation of publishers and processors targeting both [EventHub](samples/EventHub/) and [ServiceBus](samples/ServiceBus/).
