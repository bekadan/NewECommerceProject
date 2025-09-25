using Core.Logging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Core.Logging.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    public LoggingMiddleware(RequestDelegate requestDelegate, ILogger logger)
    {
        _next = requestDelegate;
        _logger = logger;
    }

    //InvokeAsync is the entry point for the middleware.
    //ASP.NET Core calls this method for each HTTP request.
    public async Task InvokeAsync(HttpContext context) 
    {
        /*
         ASP.NET Core streams the request body, which can only be read once by default.

         Reset Position = 0 to read the stream (assumes EnableBuffering() has been called earlier).

         StreamReader reads the request body into a string.

         Reset Position = 0 again so downstream middleware/controllers can read the body.
         
         */
        // Log Request
        context.Request.Body.Position = 0; // EnableBuffering()
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        //Logs the HTTP method (GET, POST, etc.), path, and body.
        _logger.Information("Incoming Request: {Method} {Path} {Body}", context.Request.Method, context.Request.Path, requestBody);


        /*
         Middleware replaces the response body stream with a MemoryStream.

         This allows capturing what the downstream middleware writes to the response.
         */
        // Capture Response
        var originalBody = context.Request.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        /*
         Calls the next middleware in the pipeline.

At this point, the request is processed by the rest of the pipeline, including controllers.
         */
        await _next(context);

        /*
         After the response is written to the memory stream, seek to the beginning.

Read the response body into a string.

Seek back to the beginning so the response can still be sent to the client.
         */

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        //Logs the response status code (e.g., 200, 404) and body content.
        _logger.Information("Outgoing Response: {StatusCode} {Body}", context.Response.StatusCode, responseText);

        /*
         Writes the captured response back to the original HTTP response stream so the client actually receives it.

Ensures the middleware doesn’t break the response pipeline.
         */
        await responseBody.CopyToAsync(originalBody);
    }
}

/*
 Logs incoming HTTP requests (method, path, body).

Captures the response body in a memory stream.

Logs outgoing responses (status code, body).

Ensures the pipeline continues normally without altering request/response behavior.

Very useful for debugging or auditing requests and responses.
 */
