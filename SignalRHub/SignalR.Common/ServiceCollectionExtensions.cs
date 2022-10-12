using Microsoft.Extensions.Configuration;

namespace SignalR.Common;

public static class ServiceCollectionExtensions
{
    public static void AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("SignalR").Get<SignalROption>() ?? new SignalROption();
        services.AddSingleton<ISignalRNotifier, SignalRNotifier>();
        services.AddSingleton<ISignalRPublisher, SignalRPublisher>();

        services.AddSingleton<IHubConnectionInstance, HubConnectionInstance>(ctx =>
        {
            var hubConnectionInstance = new HubConnectionInstance(option);
            hubConnectionInstance.InitAsync().Wait();
            return hubConnectionInstance;
        });
    }
}
