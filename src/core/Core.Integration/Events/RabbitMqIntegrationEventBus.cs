using Core.Events.Abstractions;
using Core.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Core.Integration.Events;

/*
 Implements IIntegrationEventBus → standard interface for publishing and subscribing to integration events.

Implements IDisposable → to clean up RabbitMQ connections/channels when the bus is no longer needed.
 */

public class RabbitMqIntegrationEventBus : IIntegrationEventBus, IDisposable
{
    /*
     _connection → RabbitMQ connection.

_channel → RabbitMQ channel (used to communicate with RabbitMQ).

_logger → used for logging important information.
     */

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger _logger;


    /*
     Injects a logger.

Throws ArgumentNullException if the logger is null, ensuring logging is always available.
     */
    public RabbitMqIntegrationEventBus(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /*
     Creates a RabbitMQ connection and channel asynchronously.

Declares an exchange named "integration_events" of type Fanout:

Fanout → broadcasts messages to all bound queues.

Durable → survives RabbitMQ restarts.
     */

    private async Task InitializeAsync(string hostName)
    {
        var factory = new ConnectionFactory() { HostName = hostName };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(
            exchange: "integration_events",
            type: ExchangeType.Fanout,
            durable: true
        );
    }


    /*
     Static method to create and initialize the bus.

Ensures the RabbitMQ channel and connection are ready before use.
     */
    public static async Task<RabbitMqIntegrationEventBus> CreateAsync(ILogger logger, string hostName = "localhost")
    {
        var bus = new RabbitMqIntegrationEventBus(logger);
        await bus.InitializeAsync(hostName);
        return bus;
    }


    /*
     Step 1: Check if the channel is initialized.

Step 2: Serialize the event to JSON.

Step 3: Convert the JSON string to bytes (RabbitMQ only sends byte arrays).

Step 4: Publish to the "integration_events" exchange using BasicPublishAsync.

Step 5: Log the event publication.
     */

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("The RabbitMQ channel has not been initialized.");
        }

        var eventName = typeof(TEvent).Name;
        

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        

        await _channel.BasicPublishAsync(
            exchange: "integration_events",
            routingKey: string.Empty,
            mandatory: false,
            body: Encoding.UTF8.GetBytes(message)
        );

        _logger.Information("Published event {EventName} with ID {EventId}", eventName, @event.Id);
    }

    /*
     Step 1: Ensure the channel is initialized.

Step 2: Declare the same "integration_events" exchange (idempotent in RabbitMQ).

Step 3: Declare a new queue (auto-generated name) and bind it to the exchange.

Step 4: Create an AsyncEventingBasicConsumer to receive messages asynchronously.

Step 5: Deserialize each received message back into the event type TEvent.

Step 6: Call the provided handler function to process the event.

Step 7: Acknowledge the message if successfully handled (BasicAckAsync).

Step 8: Log success or errors.
     */

    public async Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : IIntegrationEvent
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("The RabbitMQ channel has not been initialized.");
        }

        var eventName = typeof(TEvent).Name;
        await _channel.ExchangeDeclareAsync(exchange: "integration_events", type: ExchangeType.Fanout);

        var queueName = await _channel.QueueDeclareAsync();
        await _channel.QueueBindAsync(queue: queueName.QueueName, exchange: "integration_events", routingKey: "");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var @event = JsonSerializer.Deserialize<TEvent>(message);

            if (@event != null)
            {
                try
                {
                    await handler(@event);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger.Information("Handled event {EventName} with ID {EventId}", eventName, @event.Id);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error handling event {EventName}", eventName);
                    // Optionally: implement retry or dead-letter queue
                }
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);       
    }

    private async Task PublishToDlqAsync<TEvent>(TEvent @event)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("The RabbitMQ channel has not been initialized.");
        }

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: "my-service.dlx",
            routingKey: typeof(TEvent).Name,
            mandatory: false,
            body: body
        );

        _logger.Error("Message moved to DLQ: {EventType}", typeof(TEvent).Name);
    }


    /*
     Properly closes and disposes the channel and connection when the bus is no longer needed.
     */

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

/*
 Key Points

Uses RabbitMQ Fanout Exchange → broadcasts messages to all subscribers.

Handles JSON serialization for events.

Provides async/await support for both publishing and subscribing.

Logs all important actions.

Ensures channel initialization before use.

Implements disposable pattern to release resources.
 */

/*
 var bus = new RabbitMqIntegrationEventBus(logger, "localhost");
await bus.InitializeAsync();

await bus.PublishAsync(new OrderCreatedEvent(...));

await bus.SubscribeAsync<OrderCreatedEvent>(async evt =>
{
    // handle event
});

 */
