namespace AspnetcoreEx.MiniAspnetCore;

public class WebHostedService : IHostedService
{   
    private readonly IServer _server;
    private readonly RequestDelegate _handler;
    public WebHostedService(IServer server, RequestDelegate handler)
    {
        _server = server;
        _handler = handler;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _server.StartAsync(_handler);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}