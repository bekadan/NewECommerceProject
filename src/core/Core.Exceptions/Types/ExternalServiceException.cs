namespace Core.Exceptions.Types;

public class ExternalServiceException : AppException
{
    public ExternalServiceException(string message)
        : base(message, 502) { }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException, 502) { }
}
