using Cabazure.Messaging;
using Cabazure.Messaging.EventHub;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var eventHubConnection = builder.Configuration
    .GetConnectionString("eventhub")!;

builder.Services
    .AddCabazureEventHub(b => b
        .Configure(o => o
            .WithConnection(eventHubConnection))
        .AddPublisher<MyEvent>("eventHub", b => b
            .WithMessageId(e => e.Identifier)
            .WithProperty(e => e.Date.Year)));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet(
    "/",
    async (IMessagePublisher<MyEvent> publisher, CancellationToken cancellationToken) =>
    {
        var evt = new MyEvent(
            DateTime.UtcNow,
            Guid.NewGuid().ToString());

        await publisher.PublishAsync(
            evt,
            new EventHubPublishingOptions
            {
                PartitionKey = evt.Identifier,
            },
            cancellationToken);

        return Results.Ok(evt);
    });

app.Run();

sealed record MyEvent(
    DateTime Date,
    string Identifier);
