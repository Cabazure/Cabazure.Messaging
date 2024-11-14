var builder = DistributedApplication.CreateBuilder(args);

var eventHub = builder
    .AddAzureEventHubs("eventhub")
    .AddEventHub("eventhub")
    .RunAsEmulator(b => b
        .WithGatewayPort(5672));

var blobs = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(c => c
        .WithBlobPort(10000)
        .WithQueuePort(10001)
        .WithTablePort(10002))
    .AddBlobs("blobs");

builder.AddProject<Projects.EventHub_Producer>("eventhub-producer")
    .WithReference(eventHub);

builder.AddProject<Projects.EventHub_Processor>("eventhub-processor")
    .WithReference(eventHub)
    .WithReference(blobs);

builder.AddProject<Projects.ServiceBus_Producer>("servicebus-producer");

builder.Build().Run();
