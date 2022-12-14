namespace AspnetcoreEx.Extensions;

public sealed class PerformanceMetricsCollector : IHostedService
{
    private readonly IProcessorMetricsCollector _processorMetricsCollector;
    private readonly IMemoryMetricsCollector _memoryMetricsCollector;
    private readonly INetworkMetricsCollector _networkMetricsCollector;
    private readonly IMetricsDeliver _metricsDeliver;

    private IDisposable? _scheduler;

    public PerformanceMetricsCollector(
        IProcessorMetricsCollector processorMetricsCollector,
        IMemoryMetricsCollector memoryMetricsCollector,
        INetworkMetricsCollector networkMetricsCollector,
        IMetricsDeliver metricsDeliver
    )
    {
        _processorMetricsCollector = processorMetricsCollector;
        _memoryMetricsCollector = memoryMetricsCollector;
        _networkMetricsCollector = networkMetricsCollector;
        _metricsDeliver = metricsDeliver;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = new Timer(Callback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        return Task.CompletedTask;

        async void Callback(object? state)
        {
            var counter = new PerformanceMetrics
            {
                Processor = _processorMetricsCollector.GetUsage(),
                Memory = _memoryMetricsCollector.GetUsage(),
                Network = _networkMetricsCollector.GetThroughput()
            };
            await _metricsDeliver.DeliverAsync(counter);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler?.Dispose();
        return Task.CompletedTask;
    }
}