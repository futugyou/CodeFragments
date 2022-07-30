using Quartz;

namespace QuartzDemo;

public class HelloJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync($"Greetings from HelloJob!{context.JobDetail.Key} job executing, triggered by {context.Trigger.Key}");
    }
}
