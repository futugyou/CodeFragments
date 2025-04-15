using MQTTnet;

namespace AspnetcoreEx.MQTT;

public static class MQTTExtensions
{
    public static IServiceCollection AddMQTTExtension(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<MQTTOptions>(configuration.GetSection("MQTT"));
        services.AddHostedService<MqttBackgroundSubscriber>();
        services.AddSingleton<IMqttSubscribeHandler, MqttSubscribeHandler>();
        services.AddSingleton<IMqttService, MqttService>();

        return services;
    }
}