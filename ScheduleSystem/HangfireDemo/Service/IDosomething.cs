using Hangfire;

namespace HangfireDemo.Service;

public interface IDosomething
{
    Task ReadMessage(string message);
    Task<int> HaveReturnValue();
    Task Loopwork();
    void Delaywork();
    Task<string> DisplayTime();

    Task<JobParaemter> ChangeAge(JobParaemter jobParaemter);

    // Default Attempt: 10
    // DefaultDelayInSecondsByAttempt
    // (int)Math.Round(Math.Pow(attempt - 1, 4.0) + 15.0 + (double)(random.Next(30) * attempt))
    Task ErrorMethod();

    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    Task ErrorMethodWithLimit();

    Task LongRunningJob();
}


public class JobParaemter
{
    public int Age { get; set; }
    public string Name { get; set; }
}