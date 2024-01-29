using Microsoft.Extensions.Configuration;

namespace SignalR.Common;

public static class ServiceCollectionExtensions
{
    public static void AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SignalROption>(configuration.GetSection("SignalR"));
        services.AddSingleton<ISignalRNotifier, SignalRNotifier>();
        services.AddSingleton<ISignalRPublisher, SignalRPublisher>();

        services.AddSingleton<IHubConnectionInstance, HubConnectionInstance>(sp =>
        {
            var option = sp.GetRequiredService<IOptionsMonitor<SignalROption>>();
            var hubConnectionInstance = new HubConnectionInstance(option);
            hubConnectionInstance.InitAsync().Wait();
            return hubConnectionInstance;
        });
    }
}
