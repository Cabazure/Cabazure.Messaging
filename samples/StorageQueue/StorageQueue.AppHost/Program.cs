var builder = DistributedApplication.CreateBuilder(args);

var queues = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(c => c
        .WithBlobPort(10000)
        .WithQueuePort(10001)
        .WithTablePort(10002))
    .AddQueues("queues");

builder.AddProject<Projects.StorageQueue_Producer>("queue-producer")
    .WithReference(queues);

builder.AddProject<Projects.StorageQueue_Processor>("queue-processor")
    .WithReference(queues);

builder.Build().Run();
