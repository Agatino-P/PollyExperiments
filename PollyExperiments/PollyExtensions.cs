using Polly;
using Polly.Retry;
namespace PollyExperiments;

public static class PollyExtensions
{


    public static AsyncRetryPolicy<TResult> WaitAndRetrySelectiveAsyncPolicy<TResult>(Func<TResult, bool> handlePredicate, Func<DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
        => Policy.HandleResult(handlePredicate).WaitAndRetryForeverAsync<TResult>((_, result, _) => sleepDurationProvider(result), (_, _, _, _) => Task.CompletedTask);

    //public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
    //    => policyBuilder.WaitAndRetryForeverAsync<TResult>((_, delegResult, _) => sleepDurationProvider(delegResult), (_, _, _, _) => Task.CompletedTask);

    public static AsyncRetryPolicy<TResult> WaitAndRetryForeverAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, Func<TResult, TimeSpan> sleepDurationProvider)
    {
        return policyBuilder.WaitAndRetryForeverAsync<TResult>((_, delegResult, _) => sleepDurationProvider(delegResult.Result), (_, _, _, _) => Task.CompletedTask);
    }
}
