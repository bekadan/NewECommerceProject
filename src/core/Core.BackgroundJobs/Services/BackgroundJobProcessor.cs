using Core.BackgroundJobs.Abstractions;
using Core.BackgroundJobs.Configurations;
using Core.Events.Abstractions;
using Core.Events.Events;
using Core.Exceptions.Types;
using Core.Integration.Events;
using Core.Metrics.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Diagnostics;

namespace Core.BackgroundJobs.Services;

public class BackgroundJobProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobProcessor> _logger;
    private readonly BackgroundJobOptions _options;
    private readonly IMetricsCollector _metrics;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;
    private readonly IIntegrationEventBus _eventBus;
    private readonly IEnumerable<Type> _eventHandlerTypes;

    public BackgroundJobProcessor(
        IServiceProvider serviceProvider,
        ILogger<BackgroundJobProcessor> logger,
        IOptions<BackgroundJobOptions> options,
        IIntegrationEventBus eventBus,
        IMetricsCollector metrics,
        IEnumerable<Type> eventHandlerTypes)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _eventBus = eventBus;
        _metrics = metrics;

        // Discover all handler types dynamically
        _eventHandlerTypes = eventHandlerTypes
            .Select(h => h.GetType())
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJobHandler<>))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct();

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                _options.RetryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (ex, time, attempt, ctx) =>
                {
                    _logger.LogWarning(ex, "Retry {Attempt} after {Delay}s due to: {Message}", attempt, time.TotalSeconds, ex.Message);
                    _metrics.IncrementJobRetries(typeof(IIntegrationEvent).Name); // track retry
                });

        _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        _eventHandlerTypes = eventHandlerTypes;
    }

    public void AutoSubscribeAll()
    {
        foreach (var eventType in _eventHandlerTypes)
        {
            var method = GetType().GetMethod(nameof(SubscribeToEvent))!
                .MakeGenericMethod(eventType);

            method.Invoke(this, null);
            _logger.LogInformation("📬 Subscribed to event {EventName}", eventType.Name);
        }
    }

    public void SubscribeToEvent<TEvent>() where TEvent : IIntegrationEvent
    {
        _logger.LogInformation("📡 Subscribing to integration event: {EventType}", typeof(TEvent).Name);

        _eventBus.SubscribeAsync<TEvent>(async (@event) =>
        {
            _logger.LogInformation("📥 Received event {EventType}, processing...", typeof(TEvent).Name);
            await ProcessAsync(@event);
        });
    }

    public async Task ProcessAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventName = typeof(TEvent).Name;
        var stopwatch = Stopwatch.StartNew();

        _metrics.IncrementJobsStarted(eventName); // increment start metric

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService<IBackgroundJobHandler<TEvent>>();

        if (handler == null)
        {
            _logger.LogError("No handler found for event {EventName}", eventName);
            _metrics.IncrementJobsFailed(eventName);
            return;
        }

        _logger.LogInformation("📬 Starting background job for event {EventName}", eventName);

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _timeoutPolicy.ExecuteAsync(async ct =>
                {
                    await handler.HandleAsync(@event, ct);
                }, cancellationToken);
            });

            stopwatch.Stop();
            _logger.LogInformation("✅ Completed job {EventName} in {Elapsed} ms", eventName, stopwatch.ElapsedMilliseconds);
            _metrics.IncrementJobsCompleted(eventName);
            _metrics.RecordJobDuration(eventName, stopwatch.Elapsed);
        }
        catch (TimeoutRejectedException)
        {
            var timeoutEx = new TimeoutException($"Job {eventName} timed out after {_options.TimeoutSeconds}s");
            await SendToDeadLetterQueueAsync(@event, timeoutEx);
            _metrics.IncrementJobsFailed(eventName);
            throw new BackgroundJobFailedException(eventName, timeoutEx);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await SendToDeadLetterQueueAsync(@event, ex);
            _metrics.IncrementJobsFailed(eventName);

            _logger.LogError(ex,"❌ Job {EventName} failed after {RetryCount} retries. Sending to DLQ [Id: {EventId}]",
                    eventName, _options.RetryCount, @event.Id
             );

            var dlqBus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();
            await dlqBus.PublishAsync(new DeadLetterEvent(@event, ex.Message));
            throw new BackgroundJobFailedException(eventName, ex);
        }
    }

    /// <summary>
    /// Publishes the failed event to a DLQ exchange for inspection/reprocessing.
    /// </summary>
    private async Task SendToDeadLetterQueueAsync<TEvent>(TEvent @event, Exception ex)
        where TEvent : IIntegrationEvent
    {
        var eventName = typeof(TEvent).Name;

        _logger.LogWarning(ex, "🚨 Sending failed event {EventName} to Dead Letter Queue", eventName);

        var dlqEvent = new DeadLetterIntegrationEvent(
            originalEvent: @event,
            eventType: eventName,
            failedAt: DateTime.UtcNow,
            exceptionMessage: ex.Message,
            stackTrace: ex.StackTrace);

        await _eventBus.PublishAsync(dlqEvent);
    }
}