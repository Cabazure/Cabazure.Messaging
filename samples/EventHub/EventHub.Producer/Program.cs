using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var eventHubConnection = builder.Configuration
    .GetConnectionString("eventhub") ?? string.Empty;

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
        var forecast = new MyEvent(
            DateTime.UtcNow,
            Guid.NewGuid().ToString());

        await publisher.PublishAsync(
            forecast,
            cancellationToken);

        return Results.Ok(forecast);
    });

app.Run();

public record MyEvent(
    DateTime Date,
    string Identifier);
