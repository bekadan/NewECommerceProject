using Polly;
using Polly.Retry;

namespace Core.BackgroundJobs.Utility;

public static class Policies
{
    public static AsyncRetryPolicy DefaultRetryPolicy(int retryCount = 3)
    {
        return Policy
            .Handle<Exception>() // retry on any exception
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // exponential backoff
            );
    }
}
