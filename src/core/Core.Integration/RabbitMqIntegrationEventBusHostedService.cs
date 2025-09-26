using Core.Integration.Events;
using Core.Integration.Options;
using Core.Logging.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Core.Integration;

public class RabbitMqIntegrationEventBusHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly TelemetryClient? _telemetry;
    public IIntegrationEventBus Bus { get; private set; } = null!;

    public RabbitMqIntegrationEventBusHostedService(
        ILogger logger,
        IOptions<RabbitMqOptions> options,
        TelemetryClient? telemetry = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options;
        _telemetry = telemetry;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bus = new RabbitMqIntegrationEventBus(_logger, _options, _telemetry);
        await bus.InitializeAsync();
        Bus = bus;

        _logger.Information("RabbitMqIntegrationEventBus initialized via hosted service.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (Bus is RabbitMqIntegrationEventBus concreteBus)
        {
            concreteBus.Dispose();
        }
        return Task.CompletedTask;
    }
}
