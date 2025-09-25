namespace Core.Metrics.Abstractions;

public interface IMetricsCollector
{
    void IncrementJobsStarted(string jobName);
    void IncrementJobsCompleted(string jobName);
    void IncrementJobsFailed(string jobName);
    void IncrementJobRetries(string jobName);
    void RecordJobDuration(string jobName, TimeSpan duration);
}
