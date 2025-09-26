using Core.Events.Abstractions;
using Core.Integration.Options;
using Core.Logging.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Core.Integration.Events;

public class RabbitMqIntegrationEventBus : IIntegrationEventBus, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger _logger;
    private readonly TelemetryClient? _telemetry;
    private readonly RabbitMqOptions _options;

    // Metrics
    private readonly ConcurrentDictionary<string, int> _receivedEvents = new();
    private readonly ConcurrentDictionary<string, int> _successEvents = new();
    private readonly ConcurrentDictionary<string, int> _failedEvents = new();
    private readonly ConcurrentDictionary<string, int> _dlqEvents = new();
    private readonly ConcurrentDictionary<string, int> _retryAttempts = new();

    public RabbitMqIntegrationEventBus(
        ILogger logger,
        IOptions<RabbitMqOptions> options,
        TelemetryClient? telemetry = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _telemetry = telemetry;

        _logger.Information("RabbitMQ initialized at {Host}", _options.HostName);
    }

    public async Task InitializeAsync()
    {
        if (_connection != null && _channel != null)
            return; // already initialized

        // Synchronous connection & channel creation
        var factory = new ConnectionFactory { HostName = _options.HostName };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Async exchange declaration
        await _channel.ExchangeDeclareAsync(
            exchange: "integration_events",
            type: ExchangeType.Fanout,
            durable: true
        );

        if (!string.IsNullOrWhiteSpace(_options.DlqExchangeName))
        {
            await _channel.ExchangeDeclareAsync(
                exchange: _options.DlqExchangeName,
                type: ExchangeType.Fanout,
                durable: true
            );
        }

        _logger.Information("RabbitMQ initialized for host {HostName}", _options.HostName);
    }

    // Factory method for async initialization if needed
    public async static Task<RabbitMqIntegrationEventBus> CreateAsync(
        ILogger logger,
        IOptions<RabbitMqOptions> options,
        TelemetryClient? telemetry = null)
    {
        var bus = new RabbitMqIntegrationEventBus(logger, options, telemetry);
        await bus.InitializeAsync();
        return bus;
    }

    #region Publish

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        if (_channel is null) throw new InvalidOperationException("Channel is not initialized!");

        var eventName = typeof(TEvent).Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync("integration_events", string.Empty, mandatory: false, body: body);

        _logger.Information("Published event {EventName} with ID {EventId}", eventName, @event.Id);

        if (_options.EnableTelemetry && _telemetry != null)
        {
            _telemetry.TrackEvent("EventPublished", new Dictionary<string, string>
            {
                ["EventName"] = eventName,
                ["EventId"] = @event.Id.ToString()
            });
        }
    }

    #endregion

    #region Subscribe

    public async Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IIntegrationEvent
    {
        if (_channel is null) throw new InvalidOperationException("Channel is not initialized!");

        var eventName = typeof(TEvent).Name;

        // Queue setup
        var queueName = await _channel.QueueDeclareAsync();
        await _channel.QueueBindAsync(queueName.QueueName, "integration_events", string.Empty);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var @event = JsonSerializer.Deserialize<TEvent>(message);

            if (@event == null)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            _receivedEvents.AddOrUpdate(eventName, 1, (_, v) => v + 1);

            int attempt = 0;
            bool handled = false;

            while (attempt < _options.MaxRetryAttempts && !handled)
            {
                try
                {
                    attempt++;
                    await handler(@event);
                   await  _channel.BasicAckAsync(ea.DeliveryTag, false);

                    _successEvents.AddOrUpdate(eventName, 1, (_, v) => v + 1);
                    _retryAttempts.AddOrUpdate(eventName, attempt, (_, v) => v + attempt);

                    if (_options.EnableTelemetry && _telemetry != null)
                    {
                        _telemetry.TrackEvent("EventHandled", new Dictionary<string, string>
                        {
                            ["EventName"] = eventName,
                            ["EventId"] = @event.Id.ToString(),
                            ["Attempts"] = attempt.ToString()
                        });
                    }

                    _logger.Information(
                        "Handled event {EventName} with ID {EventId} after {Attempts} attempt(s)",
                        eventName,
                        @event.Id,
                        attempt
                    );

                    handled = true;
                }
                catch (Exception ex)
                {
                    _failedEvents.AddOrUpdate(eventName, 1, (_, v) => v + 1);
                    _logger.Error(ex, "Failed handling event {EventName} attempt {Attempt}", eventName, attempt);

                    if (attempt < _options.MaxRetryAttempts)
                    {
                        var delay = TimeSpan.FromMilliseconds(_options.BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                        await Task.Delay(delay);
                    }
                    else
                    {
                        await PublishToDlqAsync(@event);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _dlqEvents.AddOrUpdate(eventName, 1, (_, v) => v + 1);
                    }
                }
            }
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);

        _logger.Information("Subscribed to event {EventName}", eventName);
        
    }

    #endregion

    #region DLQ

    private async Task PublishToDlqAsync<TEvent>(TEvent @event)
    {
        if (_channel is null) throw new InvalidOperationException("Channel is not initialized!");

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        await _channel.BasicPublishAsync(
            _options.DlqExchangeName,
            typeof(TEvent).Name,
            mandatory: false,
            body: body
        );

        _logger.Warning("Message moved to DLQ: {EventType}", typeof(TEvent).Name);
    }

    #endregion

    #region Metrics

    public IReadOnlyDictionary<string, int> Metrics => new Dictionary<string, int>
    {
        { "Received", _receivedEvents.Values.Sum() },
        { "Success", _successEvents.Values.Sum() },
        { "Failed", _failedEvents.Values.Sum() },
        { "DLQ", _dlqEvents.Values.Sum() },
        { "RetryAttempts", _retryAttempts.Values.Sum() }
    };

    #endregion

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}