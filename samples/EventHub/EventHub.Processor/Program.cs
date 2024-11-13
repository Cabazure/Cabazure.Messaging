using Azure.Storage.Blobs;
using Cabazure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration
    .GetConnectionString("eventhub") ?? string.Empty;

var blobsConnection = builder.Configuration
    .GetConnectionString("blobs") ?? string.Empty;

var client = new BlobServiceClient(blobsConnection);
await client.CreateBlobContainerAsync("container1");

builder.Services.AddCabazureEventHub(b => b
    .Configure(o => o
        .WithConnection(connectionString)
        .WithBlobStorage(blobsConnection, "container1"))
    .AddProcessor<MyEvent, MyEventprocessor>("eventhub"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet(
    "/",
    (MyEventprocessor processor) => Results.Ok(processor.ReceivedEvents));

app.Run();

record MyEvent(
    DateTime Date,
    string Identifier);

class MyEventprocessor : IMessageProcessor<MyEvent>
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
