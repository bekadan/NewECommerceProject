namespace Core.Logging.Abstractions;

/*
 Purpose of ILogger

The ILogger interface defines a contract (blueprint) for logging in your application.
Its purpose is to:

✅ Provide a consistent way to log messages across the entire system.

✅ Abstract away the underlying logging library (like Serilog, NLog, etc.).

✅ Make logging testable, flexible, and replaceable — you can change the logging implementation without touching business code.

Think of it like a universal remote control: it doesn’t care how the TV works internally, it just defines the buttons you can press. 📺
 
 */

public interface ILogger
{
    /*
     Logs a critical failure — usually something that crashes the system or requires immediate attention.

Includes an Exception to capture stack trace + details.
     */
    void Critical(Exception ex, string message, params object[] args);

    /*
     Used for developer-level diagnostic information.

Typically disabled in production (too verbose).

Useful during development or troubleshooting.
     */

    void Debug(string message, params object[] args);

    /*
     Indicates something unexpected happened, but the app can still continue.

Not an error — but worth investigating.
     */

    void Warning(string message, params object[] args);

    /*
     Logs a recoverable error — an exception occurred but the app didn’t crash.

Includes the exception details for debugging.
     */
    void Error(Exception ex, string message, params object[] args);

    /*
     Used for normal, important events in the app lifecycle.

Great for tracking workflows and high-level behavior.
     */
    void Information(string message, params object[] args);
}

/*
 Why params object[] args?
All logging methods accept a params object[] args argument.
This allows structured, formatted logging:
_logger.Information("User {UserId} created order {OrderId}", userId, orderId);

{UserId} and {OrderId} are placeholders — values are injected at runtime.

Logging frameworks (like Serilog) can then store these values as structured data (not just text), making logs more powerful and searchable.
 */
