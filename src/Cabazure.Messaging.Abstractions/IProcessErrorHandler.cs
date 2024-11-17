namespace Cabazure.Messaging;

public interface IProcessErrorHandler
{
    Task ProcessErrorAsync(
        Exception exception,
        CancellationToken cancellationToken);
}
