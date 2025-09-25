namespace Core.Exceptions.Types;

public class EmailException : AppException
{
    public EmailException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
