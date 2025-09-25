namespace Core.Exceptions.Types;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}
