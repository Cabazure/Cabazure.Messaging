using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var queuesConnection = builder.Configuration
    .GetConnectionString("queues")!;

builder.Services.AddCabazureStorageQueue(b => b
    .Configure(o => o
        .WithConnection(queuesConnection))
    .AddProcessor<MyEvent, MyEventprocessor>("queue", b => b
        .WithInitialization(createIfNotExists: true)
        .WithPollingInterval(TimeSpan.FromSeconds(5))));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet(
    "/",
    (MyEventprocessor processor) => Results.Ok(processor.ReceivedEvents));

app.Run();

sealed record MyEvent(
    DateTime Date,
    string Identifier);

sealed class MyEventprocessor : IMessageProcessor<MyEvent>
{
    public List<MyEvent> ReceivedEvents { get; } = [];

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
