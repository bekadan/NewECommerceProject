using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Core.Logging.Enrichment;

//Implements ILogEventEnricher from Serilog, which requires defining the Enrich method.
//Enrichers allow you to add extra properties to Serilog logs automatically.
public class CorrelationIdEnricher : ILogEventEnricher
{
    //IHttpContextAccessor allows the class to access the current HTTP request.
    //It is used to read headers, like X-Correlation-ID, from the request.
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    //This method is called by Serilog for each log event.
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        //Reads the X-Correlation-ID HTTP header from the current request.
        //If no header exists, generate a new GUID as a fallback correlation ID.
        var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        //propertyFactory.CreateProperty("CorrelationId", correlationId) → Creates a Serilog property called "CorrelationId".
        //logEvent.AddPropertyIfAbsent(...) → Adds the property to the log only if it’s not already present.
        //as Result: Every log now contains a CorrelationId property, which can be used to trace requests across logs.
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}

/*
 
This enricher ensures every log message has a correlation ID.

It first tries to use the correlation ID from the HTTP request.

If no correlation ID exists in the request, it generates a new GUID.

This is extremely useful for tracking individual requests in distributed systems or microservices.

 */
