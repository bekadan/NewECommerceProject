namespace Core.BackgroundJobs.Configurations;

public class BackgroundJobOptions
{
    public int TimeoutSeconds { get; set; } = 10; // default
    public int RetryCount { get; set; } = 3;      // default
    public string DlqExchange { get; set; } = "my-service.dlx";
}
