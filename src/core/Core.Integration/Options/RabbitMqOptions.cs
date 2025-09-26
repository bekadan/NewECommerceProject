namespace Core.Integration.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public string DlqExchangeName { get; set; } = "my-service.dlx";
    public TimeSpan MetricsReportingInterval { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableTelemetry { get; set; } = true;
}

//{
//  "RabbitMq": {
//    "HostName": "localhost",
//    "MaxRetryAttempts": 5,
//    "BaseRetryDelay": "00:00:03",
//    "DlqExchangeName": "my-service.dlx",
//    "MetricsReportingInterval": "00:00:30",
//    "EnableTelemetry": true
//  }
//}