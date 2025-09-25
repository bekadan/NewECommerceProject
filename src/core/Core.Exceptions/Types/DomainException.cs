namespace Core.Exceptions.Types;

public class DomainException : AppException
{
    public DomainException(string message)
        : base(message, 400) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException, 400) { }
}
