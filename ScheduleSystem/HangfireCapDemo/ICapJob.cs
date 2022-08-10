using Hangfire.Server;

namespace HangfireCapDemo;

public interface ICapJob
{
    Task Excute(CapJobItem item, string jobName = null, string queuename = null, bool isretry = false, PerformContext context = null);
}
