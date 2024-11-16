using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var eventHubConnection = builder.Configuration
    .GetConnectionString("eventhub")!;

builder.Services
    .AddCabazureEventHub(b => b
        .Configure(o => o
            .WithConnection(eventHubConnection))
        .AddPublisher<MyEvent>(
            "eventHub",
            b => b.AddProperty(e => e.Date.Year)));

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
            cancellationToken);

        return Results.Ok(evt);
    });

app.Run();

sealed record MyEvent(
    DateTime Date,
    string Identifier);
