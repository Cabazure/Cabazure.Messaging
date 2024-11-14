using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("servicebus")!;

builder.Services
    .AddCabazureServiceBus(b => b
        .Configure(o => o
            .WithConnection(connectionString))
        .AddPublisher<MyEvent>(
            "topic",
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

public record MyEvent(
    DateTime Date,
    string Identifier);