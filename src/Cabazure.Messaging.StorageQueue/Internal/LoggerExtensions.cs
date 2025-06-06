﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.StorageQueue.Internal;

[ExcludeFromCodeCoverage]
public static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "Failed to process {MessageType} message in {ProcessorType} type")]
    public static partial void FailedToProcessMessage(
        this ILogger logger,
        string MessageType,
        string ProcessorType,
        Exception Exception);
}
