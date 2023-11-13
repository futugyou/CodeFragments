namespace AspnetcoreEx.Extensions;

public interface IProcessorMetricsCollector
{
    int GetUsage();
}

public interface IMemoryMetricsCollector
{
    long GetUsage();
}

public interface INetworkMetricsCollector
{
    long GetThroughput();
}

public interface IMetricsDeliver
{
    Task DeliverAsync(PerformanceMetrics counter);
}

public class MetricsCollector: IProcessorMetricsCollector, IMemoryMetricsCollector, INetworkMetricsCollector
{
    int IProcessorMetricsCollector.GetUsage() => PerformanceMetrics.Create().Processor;
    long IMemoryMetricsCollector.GetUsage() => PerformanceMetrics.Create().Memory;
    long INetworkMetricsCollector.GetThroughput() => PerformanceMetrics.Create().Network;
}

public class  MetricsDeliver:IMetricsDeliver
{
    public Task DeliverAsync(PerformanceMetrics counter)
    {
        // Console.WriteLine($"[{DateTimeOffset.UtcNow}]{counter}");
        return Task.CompletedTask;
    }
}