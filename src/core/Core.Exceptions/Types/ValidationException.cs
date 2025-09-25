namespace Core.Exceptions.Types;

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]> errors)
            : base(message, 400)
    {
        Errors = errors;
    }
}
