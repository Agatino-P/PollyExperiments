using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using static System.Console;
namespace PollyExperiments;

public class LimitedPatience
{
    public async Task GiveItATryAsync()
    {
        AsyncTimeoutPolicy<string?> timeout =
            Policy.TimeoutAsync<string?>(25);

        AsyncRetryPolicy<string?> retryNullOrBrokenPolicy =
            Policy.Handle<BrokenCircuitException>()
            .OrResult<string?>(s => s != "MyText")
            .RetryForeverAsync(onRetry: async (exception, retryCount, context) =>
            {
                if (exception != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            });

        AsyncCircuitBreakerPolicy<string?> circuitBreakerPolicy =
            Policy.HandleResult<string?>(s => s == null)
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(4));

        //BrokenCircuitException
        var wrapPolicy1 =
            Policy.WrapAsync<string?>(timeout, retryNullOrBrokenPolicy, circuitBreakerPolicy);

        ///////////////////////
        
        var retryOnNotMatching =
            Policy.HandleResult<string?>(s => s != "MyText")
            .RetryForeverAsync();

        var waitAndRetryOnNull =
            Policy.HandleResult<string?>(s => s == null)
            .WaitAndRetryForeverAsync(counter => TimeSpan.FromSeconds(3));

        var wrapPolicy2 =
            Policy.WrapAsync<string?>(timeout, retryOnNotMatching, waitAndRetryOnNull);

        ///////////////////////
        
        AsyncRetryPolicy<string?> waitAndRetryselective = Policy.HandleResult<string?>(s => s != "MyText")
            .WaitAndRetryForeverAsync<string?>((_, s, _) => TimeSpan.FromSeconds(s == null ? 3 : 0), (_, _, _) => Task.CompletedTask);


        AsyncRetryPolicy<string?> waitAndRetrySelectiveExtension =
            Policy.HandleResult<string?>(s => s != "MyText")
            .WaitAndRetryForeverAsync<string?>(s=> TimeSpan.FromSeconds(s == null ? 3 : 0));
        var wrapPolicy3 = Policy.WrapAsync<string?>(timeout, waitAndRetrySelectiveExtension);


        try
        {
            string? found =
                await wrapPolicy3.ExecuteAsync(async (ct) => await InterruptableSingleAttemptAsync(ct), CancellationToken.None);
            if (found != null) { WriteLine(found); }
        }
        catch (TimeoutRejectedException)
        {
            WriteLine("Timeut Expired");
        }
        await Task.Delay(1000);
    }

   

    public async Task<string?> InterruptableSingleAttemptAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            WriteLine($"Going To Try at {DateTime.Now.ToString("HH:mm:ss.fff")}");

            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);
            return new Unreliable().Unpredictable();
        }
    }
}
