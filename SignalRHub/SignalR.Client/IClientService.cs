namespace SignalR.Client;

public interface IClientService
{
    Task DoWorkAsync();

    Task StopAsync();

    Task StartAsync();
}
