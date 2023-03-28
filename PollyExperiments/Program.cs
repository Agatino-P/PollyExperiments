using Dapper;
using MySqlConnector;
using Polly;
using Polly.Retry;
using static System.Console;

namespace PollyExperiments;

internal class Program
{
    private static int expectedFailures { get; set; } = 3;
    private static int attempts  { get; set; }
    static void Main(string[] args)
    {
        pollyQueryCountDb();

    }
    public static void pollyQueryCountDb()
    {
        RetryPolicy<long> increasingRetryOnzero = Policy.HandleResult<long>(r => r == 0).WaitAndRetry(2, rc => TimeSpan.FromSeconds(1));
        
        long howMany;
        
        howMany=increasingRetryOnzero.Execute(() => queryCountDb("one"));
        WriteLine(howMany);
        howMany = increasingRetryOnzero.Execute(() => queryCountDb("pone")); 
        WriteLine(howMany);
        howMany = increasingRetryOnzero.Execute(() => queryCountDb("two"));
        WriteLine(howMany);

    }


    public static long queryCountDb(string letters)
    {
        WriteLine($"Going to query for....{letters}");
        string sqlQuery = @"
            SELECT COUNT(*) FROM Polly.ToNumber where letters=@letters;
        ";

        using MySqlConnection mySqlConnection = new("Server=localhost;Database=Polly;Uid=root;Pwd=123456Ab;");
        long number = mySqlConnection.ExecuteScalar<long>(sqlQuery, new { letters });
        return number;
    }


    public static void pollyLimitedPatience()
    {
        //WriteLine("Give it a try");
        //new LimitedPatience().GiveItATryAsync().Wait();

    }
    public static void pollyTrueAfter()
    {
        RetryPolicy<bool> increasingRetry = Policy.HandleResult<bool>(r=>r==false).WaitAndRetry(4, rc=>TimeSpan.FromSeconds(rc*2));
        WriteLine(increasingRetry.Execute(trueAfterN));
    }


    public static void pollyQueryDb()
    {
        RetryPolicy<object?> increasingRetryOnNull = Policy.HandleResult<object?>(r => r == null).WaitAndRetry(4, rc => TimeSpan.FromSeconds(1));

        object? ret;
        ret = increasingRetryOnNull.Execute(() => queryDb("one"));
        WriteLine($"{(ret == null ? ret : (int)ret!)}");
        ret = increasingRetryOnNull.Execute(() => queryDb("pone"));
        WriteLine($"{(ret == null ? ret : (int)ret!)}");
        ret = increasingRetryOnNull.Execute(() => queryDb("two"));
        WriteLine($"{(ret == null ? ret : (int)ret!)}");
    }

    public static bool trueAfterN()
    {
        WriteLine($"Called at {DateTime.Now.ToString("HH:mm:ss.fff")}");
        ++attempts;
        return attempts%(expectedFailures+1) == 0;  
    }

    public static object? queryDb(string letters)
    {
        WriteLine($"Going to query for....{letters}");
        string sqlQuery = @"
            SELECT number FROM Polly.ToNumber where letters=@letters;
        ";
        
        using MySqlConnection mySqlConnection = new("Server=localhost;Database=Polly;Uid=root;Pwd=123456Ab;");
        int? number = mySqlConnection.QuerySingleOrDefault<int?>(sqlQuery, new{letters });
        return number;
    }



}


