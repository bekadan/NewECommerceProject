using Serilog;

namespace Core.Logging.Implementations;

public class Logger : Abstractions.ILogger
{
    private readonly ILogger _logger;

    public Logger()
    {
        _logger = Log.ForContext<Logger>();
    }

    public void Critical(Exception ex, string message, params object[] args)
        => _logger.Fatal(ex, message, args);

    public void Debug(string message, params object[] args)
        => _logger.Debug(message, args);

    public void Error(Exception ex, string message, params object[] args)
        => _logger.Error(message, args);

    public void Information(string message, params object[] args)
        => _logger.Information(message, args);

    public void Warning(string message, params object[] args)
        => _logger.Warning(message, args);
}
