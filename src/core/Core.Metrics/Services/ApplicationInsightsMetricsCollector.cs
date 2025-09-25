using Core.Metrics.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Core.Metrics.Services;

public class ApplicationInsightsMetricsCollector : IMetricsCollector
{
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsMetricsCollector(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    public void IncrementJobsStarted(string jobName)
    {
        _telemetryClient.GetMetric("BackgroundJob_Started", "JobName").TrackValue(1, jobName);
    }

    public void IncrementJobsCompleted(string jobName)
    {
        _telemetryClient.GetMetric("BackgroundJob_Completed", "JobName").TrackValue(1, jobName);
    }

    public void IncrementJobsFailed(string jobName)
    {
        _telemetryClient.GetMetric("BackgroundJob_Failed", "JobName").TrackValue(1, jobName);
    }

    public void IncrementJobRetries(string jobName)
    {
        _telemetryClient.GetMetric("BackgroundJob_Retries", "JobName").TrackValue(1, jobName);
    }

    public void RecordJobDuration(string jobName, TimeSpan duration)
    {
        _telemetryClient.TrackMetric(new MetricTelemetry($"BackgroundJob_Duration_{jobName}", duration.TotalMilliseconds));
    }
}
