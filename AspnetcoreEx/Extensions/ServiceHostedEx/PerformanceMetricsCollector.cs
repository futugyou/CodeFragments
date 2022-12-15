namespace AspnetcoreEx.Extensions;

public sealed class PerformanceMetricsCollector : IHostedService
{
    private readonly IProcessorMetricsCollector _processorMetricsCollector;
    private readonly IMemoryMetricsCollector _memoryMetricsCollector;
    private readonly INetworkMetricsCollector _networkMetricsCollector;
    private readonly IMetricsDeliver _metricsDeliver;
    private readonly IHostApplicationLifetime _lifeTime;

    private IDisposable? _scheduler;
    private IDisposable? _tokenSource;

    public PerformanceMetricsCollector(
        IProcessorMetricsCollector processorMetricsCollector,
        IMemoryMetricsCollector memoryMetricsCollector,
        INetworkMetricsCollector networkMetricsCollector,
        IMetricsDeliver metricsDeliver,
        IHostApplicationLifetime lifeTime
    )
    {
        _processorMetricsCollector = processorMetricsCollector;
        _memoryMetricsCollector = memoryMetricsCollector;
        _networkMetricsCollector = networkMetricsCollector;
        _metricsDeliver = metricsDeliver;
        _lifeTime = lifeTime;

        _lifeTime.ApplicationStarted.Register(()=>Console.WriteLine("[{0}] Application Started", DateTimeOffset.Now));
        _lifeTime.ApplicationStopping.Register(()=>Console.WriteLine("[{0}] Application Stopping", DateTimeOffset.Now));
        _lifeTime.ApplicationStopped.Register(()=>Console.WriteLine("[{0}] Application Stopped", DateTimeOffset.Now));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = new Timer(Callback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        // _tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token.Register(_lifeTime.StopApplication);
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
        // _tokenSource?.Dispose();
        return Task.CompletedTask;
    }
}