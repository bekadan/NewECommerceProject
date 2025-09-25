namespace Core.Metrics.Configurations;

public class ApplicationInsightsOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableAdaptiveSampling { get; set; } = true;
    public bool EnableQuickPulseMetricStream { get; set; } = true;
}
