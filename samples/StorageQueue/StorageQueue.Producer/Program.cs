using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var queuesConnection = builder.Configuration
    .GetConnectionString("queues")!;

builder.Services
    .AddCabazureStorageQueue(b => b
        .Configure(o => o
            .WithConnection(queuesConnection))
        .AddPublisher<MyEvent>("queue"));

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
