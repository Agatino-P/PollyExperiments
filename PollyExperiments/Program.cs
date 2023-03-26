using OneOf.Types;
using Polly;
using Polly.Retry;
using System.Threading;
using static System.Console;

namespace PollyExperiments;

internal class Program
{
    private static int expectedFailures { get; set; } = 3;
    private static int attempts  { get; set; }
    static void Main(string[] args)
    {
        //WriteLine("Give it a try");
        //new LimitedPatience().GiveItATryAsync().Wait();

        RetryPolicy<bool> increasingRetry = Policy.HandleResult<bool>(r=>r==false).WaitAndRetry(4, rc=>TimeSpan.FromSeconds(rc*2));

            WriteLine(increasingRetry.Execute(trueAfterN));

    }

    public static bool trueAfterN()
    {
        WriteLine($"Called at {DateTime.Now.ToString("HH:mm:ss.fff")}");
        ++attempts;
        return attempts%(expectedFailures+1) == 0;  
    }

}


