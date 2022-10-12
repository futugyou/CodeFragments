namespace SignalR.Client;

public class ClientHostedService : BackgroundService
{
    private readonly IClientService _clientService;

    public ClientHostedService(IClientService clientService)
    {
        _clientService = clientService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _clientService.StartAsync();
        Console.WriteLine("ProducerService is starting .....");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _clientService.DoWorkAsync();
        Console.WriteLine("ProducerService is running .....");
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await _clientService.StopAsync();
        Console.WriteLine("ProducerService is stopping .....");
        await Task.CompletedTask;
    }
}
