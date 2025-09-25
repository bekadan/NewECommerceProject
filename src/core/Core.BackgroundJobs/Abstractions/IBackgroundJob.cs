namespace Core.BackgroundJobs.Abstractions;

public interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
