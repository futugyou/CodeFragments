namespace SignalR.Common;
public interface IHubConnectionInstance
{
    HubConnection Connection { get; }

    Task InitAsync();

    Task StartAsync();

}
