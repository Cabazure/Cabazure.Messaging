using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("eventhub")!;
var blobsConnection = builder.Configuration
    .GetConnectionString("blobs")!;

builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(connectionString)
        .WithBlobStorage(blobsConnection))
    .AddProcessor<MyEvent, MyEventprocessor>("eventhub", "$default", b => b
        .WithBlobContainer("container1", createIfNotExist: true)));

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
