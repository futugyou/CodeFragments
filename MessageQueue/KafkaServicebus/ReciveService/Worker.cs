using KafkaServiceBus;

namespace ReciveService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceBus _serviceBus;

    public Worker(ILogger<Worker> logger, IServiceBus serviceBus)
    {
        _logger = logger;
        _serviceBus = serviceBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
            await _serviceBus.ReceiveMessage("thisistopic", stoppingToken);
        }
    }
}
