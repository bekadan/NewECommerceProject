using Core.Exceptions.Types;
using Core.Integration.Abstractions;
using Core.Logging.Abstractions;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Net.Http.Json;

namespace Core.Integration.Http;

/*
 1. Purpose of HttpClientWrapper

Provides a centralized and reusable way to perform HTTP requests.

Wraps the built-in HttpClient and simplifies GET and POST requests with JSON serialization/deserialization.

Useful for integration with external services like payment gateways, shipping APIs, or third-party inventory systems.

Allows adding cross-cutting concerns: logging, retry policies, headers, timeout, authentication.

DDD Principle:

Keeps the integration/infrastructure concerns separate from the domain/application logic.

Your domain and application layers only depend on abstractions (IHttpClientWrapper could be added if needed) rather than directly calling HttpClient.
 
 HttpClient injected via DI: Prevents socket exhaustion.

ILogger injected: Logs every request, retry, and error.

Polly policies:

Timeout: Fails if a request takes too long.

Retry: Retries failed requests with exponential backoff.

Why important in DDD: Infrastructure concerns are encapsulated, domain remains clean.
 */

public class HttpClientWrapper : IHttpClientWrapper
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncTimeoutPolicy<HttpResponseMessage> _timeoutPolicy;

    /*
     Accepts HttpClient via dependency injection, which is the recommended practice.

Avoids creating HttpClient manually, which can cause socket exhaustion.

Using DI allows configuration of timeouts, base URLs, default headers, etc.
     */

    public HttpClientWrapper(HttpClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;

        /*
         Timeout: fails if HTTP call takes longer than 10 seconds.

Retry: retries failed requests (exponential backoff).

Logging: warns on each retry.
         */

        _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        _retryPolicy = Policy.Handle<HttpRequestException>()
                             .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                 (response, timespan, retryCount, context) =>
                                 {
                                     _logger.Warning("Retry {RetryCount} for {Url} after {Delay}s", retryCount, context["Url"], timespan.TotalSeconds);
                                 });
    }

    /*
     Logs every request and response.

Helps with debugging and monitoring external integrations.
     */

    /*
     Sends an HTTP GET request to the specified url.

Automatically deserializes the JSON response into type T.

Returns null if the response body is empty.

    Step by step:

Logs the GET request.

Executes retry + timeout policies using Polly.

Ensures successful HTTP response (2xx).

Reads and deserializes JSON to type T.

Logs the response.

Throws a custom exception if anything fails.

    Example USage:

    var product = await httpClientWrapper.GetAsync<ProductDto>("https://api.example.com/products/123");

    Clean, type-safe access to JSON APIs.

No need to manually handle HttpResponseMessage or call JsonSerializer.

     */

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            _logger.Information("GET Request to {Url}", url);

            var response = await _retryPolicy.ExecuteAsync(ctx =>
                _timeoutPolicy.ExecuteAsync(() => _client.GetAsync(url)), new Context { ["Url"] = url });

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<T>();
            _logger.Information("GET Response from {Url}: {@Result}", url, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "GET request to {Url} failed", url);
            throw new HttpRequestWrapperException($"GET request to {url} failed", ex);
        }
    }

    /*
     Sends an HTTP POST request with a JSON body (payload).

EnsureSuccessStatusCode() throws an exception if the HTTP response is not 2xx.

Reads the response content as JSON and deserializes it into TResponse.

    Logs the POST request and payload.

Executes retry + timeout policies.

Ensures HTTP success.

Deserializes JSON to type TResponse.

Logs the response.

Throws HttpRequestWrapperException if anything fails.

    Example:

    var orderResponse = await httpClientWrapper.PostAsync<CreateOrderRequest, OrderResponse>(
    "https://api.example.com/orders",
    new CreateOrderRequest { ProductId = 123, Quantity = 2 }
);

    Simplifies interaction with JSON-based REST APIs.

Avoids repetitive boilerplate code in integration services.
     */

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest payload)
    {
        try
        {
            _logger.Information("POST Request to {Url} with payload {@Payload}", url, payload);

            var response = await _retryPolicy.ExecuteAsync(ctx =>
                _timeoutPolicy.ExecuteAsync(() => _client.PostAsJsonAsync(url, payload)), new Context { ["Url"] = url });

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>();
            _logger.Information("POST Response from {Url}: {@Result}", url, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "POST request to {Url} failed", url);
            throw new HttpRequestWrapperException($"POST request to {url} failed", ex);
        }
    }
}

/*
 * BEnefits
 | Benefit                    | Explanation                                                                |
| -------------------------- | -------------------------------------------------------------------------- |
| **Centralized HTTP logic** | All GET/POST requests go through one wrapper.                              |
| **Type safety**            | Automatically maps JSON to strongly-typed objects.                         |
| **DI-friendly**            | Uses `HttpClient` from DI, which supports policies (retry, timeout, etc.). |
| **Easier testing**         | You can mock `HttpClientWrapper` in unit tests instead of real HTTP calls. |
| **Clean code**             | Keeps integration services simple and readable.                            |

 */

/*
 6. How it fits into DDD

✅ DDD Perspective:

Infrastructure concerns (retry, logging, HTTP exceptions) are fully encapsulated in the wrapper.

Application and Domain layers remain pure and testable.

Supports integration with external services (payments, shipping, inventory) safely.

Infrastructure Layer: Implements HttpClientWrapper.

Application Layer: Uses it to call external services without knowing HTTP details.

Domain Layer: Completely unaware of HTTP; remains pure and testable.

Example:

public class StripePaymentService : IPaymentGateway
{
    private readonly HttpClientWrapper _httpClient;

    public StripePaymentService(HttpClientWrapper httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ChargeAsync(decimal amount, string currency, string cardToken)
    {
        var response = await _httpClient.PostAsync<ChargeRequest, ChargeResponse>(
            "https://api.stripe.com/charge",
            new ChargeRequest { Amount = amount, Currency = currency, CardToken = cardToken }
        );

        return response != null && response.Success;
    }
}

Clean separation: domain logic doesn’t know about HTTP.

Integration service only delegates serialization/deserialization and network calls to HttpClientWrapper.

 */

/*
 6. Usage in DDD Microservices

Application/Integration Layer calls external APIs through HttpClientWrapper.

Domain layer remains pure and unaware of HTTP details.

Example:

public class ShippingService
{
    private readonly IHttpClientWrapper _httpClient;

    public ShippingService(IHttpClientWrapper httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SchedulePickupAsync(string orderId)
    {
        var response = await _httpClient.PostAsync<PickupRequest, PickupResponse>(
            "/api/shipping/pickup",
            new PickupRequest { OrderId = orderId }
        );
        return response?.Success ?? false;
    }
}


Clean separation between infrastructure concerns and business logic.

 */
