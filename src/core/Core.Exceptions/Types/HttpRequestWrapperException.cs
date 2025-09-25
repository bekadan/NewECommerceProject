namespace Core.Exceptions.Types;

public class HttpRequestWrapperException : AppException
{
    public HttpRequestWrapperException(string message, Exception innerException, int statusCode = 500) : base(message, innerException, statusCode)
    {
    }
}
