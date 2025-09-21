namespace KaleidoCode.MiniAspnetCore;

public class WebHostedService : IHostedService
{
    private readonly IServer _server;
    private readonly RequestDelegate _handler;
    public WebHostedService(IServer server, RequestDelegate handler)
    {
        _server = server;
        _handler = handler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _server.StartAsync(_handler);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("application stoped");
        return Task.CompletedTask;
    }
}