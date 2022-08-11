namespace Hangfire.Cap.CapHanfireJob;

public interface ICapJob
{
    Task Excute(CapJobItem item, string jobName = null, string queuename = null, bool isretry = false, PerformContext context = null);
}
