namespace Core.Exceptions.Types;

public class InternalServerException : AppException
{
    public InternalServerException(string message)
        : base(message, 500) { }

    public InternalServerException(string message, Exception innerException)
        : base(message, innerException, 500) { }
}
