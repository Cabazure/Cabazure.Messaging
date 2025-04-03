using Cabazure.Messaging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("eventhub")!;
var blobsConnection = builder.Configuration
    .GetConnectionString("blobs")!;
var consumerGroup = builder.Configuration
    .GetValue<string>("CONSUMER_GROUP")
    ?? "$default";
var stateless = builder.Configuration
    .GetValue<bool>("STATELESS");

builder.Services.AddSingleton<MyEventProcessor>();

if (!stateless)
{
    builder.Services.AddCabazureEventHub(b => b
        .Configure(o => o
            .WithConnection(connectionString)
            .WithBlobStorage(blobsConnection))
        .AddProcessor<MyEvent, MyEventProcessor>("eventhub", consumerGroup, b => b
            .WithBlobContainer("container1", createIfNotExist: true)));
}
else
{
    builder.Services.AddCabazureEventHub(b => b
        .Configure(o => o
            .WithConnection(connectionString)
            .WithBlobStorage(blobsConnection))
        .AddStatelessProcessor<MyEvent, MyEventProcessor>("eventhub", consumerGroup));
}

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet(
    "/",
    ([FromServices] MyEventProcessor processor)
    => processor.ReceivedEvents);
app.MapGet(
    "/status",
    ([FromServices] IMessageProcessorService<MyEvent> service)
    => service.IsRunning);

app.Run();

sealed record MyEvent(
    DateTime Date,
    string Identifier);

sealed class MyEventProcessor : IMessageProcessor<MyEvent>
{
    public List<MyEvent> ReceivedEvents { get; } = [];

    public MyEventProcessor()
    {

    }

    public Task ProcessAsync(
        MyEvent message,
        MessageMetadata metadata,
        CancellationToken cancellationToken)
    {
        ReceivedEvents.Add(message);

        Console.WriteLine($"Message: {message.Date} - {message.Identifier}");
        return Task.CompletedTask;
    }
}
