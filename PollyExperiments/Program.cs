using OneOf.Types;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Threading;
using static System.Console;
namespace PollyExperiments;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Give it a try");
        new LimitedPatience().GiveItATryAsync().Wait();
    }
}

public class LimitedPatience
{
    public async Task GiveItATryAsync()
    {
        AsyncTimeoutPolicy<string?> timeoutPolicy =
            Policy.TimeoutAsync<string?>(25);

        AsyncRetryPolicy<string?> retryPolicy =
            Policy.Handle<BrokenCircuitException>()
            .OrResult<string?>(s => s != "MyText")
            .RetryForeverAsync(onRetry: async (exception, retryCount, context) =>
            {
                if (exception != null) { await Task.Delay(TimeSpan.FromSeconds(2)); }
            });

        AsyncCircuitBreakerPolicy<string?> circuitBreakerPolicy =
            Policy.HandleResult<string?>(s => s == null)
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(4));

        //BrokenCircuitException
        var wrapPolicy =
            Policy.WrapAsync<string?>(timeoutPolicy, retryPolicy, circuitBreakerPolicy);

        try
        {
            string? found =
                await wrapPolicy.ExecuteAsync(async (ct) => await InterruptableSingleAttemptAsync(ct), CancellationToken.None);
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