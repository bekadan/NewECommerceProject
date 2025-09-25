namespace Core.Exceptions.Types;

public sealed class BackgroundJobFailedException : AppException
{
    public BackgroundJobFailedException(string jobName, Exception innerException)
        : base($"Background job '{jobName}' failed after retries.", innerException)
    {
    }
}
