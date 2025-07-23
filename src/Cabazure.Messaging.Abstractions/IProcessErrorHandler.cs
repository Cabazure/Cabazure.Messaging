namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for handling errors that occur during message processing.
/// </summary>
public interface IProcessErrorHandler
{
    Task ProcessErrorAsync(
        Exception exception,
        CancellationToken cancellationToken);
}
