namespace Core.Exceptions.Types;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}
