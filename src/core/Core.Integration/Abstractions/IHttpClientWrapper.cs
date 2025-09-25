namespace Core.Integration.Abstractions;

/*
 This allows you to mock HTTP calls in unit tests.

The concrete class will implement this interface.
 */

/*
 Purpose: Provides an abstraction for HTTP calls.

Benefits:

Allows mocking in unit tests.

Decouples application logic from HttpClient.
 */

public interface IHttpClientWrapper
{
    Task<T?> GetAsync<T>(string url);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest payload);
}
