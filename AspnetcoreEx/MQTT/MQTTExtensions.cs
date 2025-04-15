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
        // This is for publishing messages
        services.AddSingleton(async sp =>
        {
            var op = sp.GetService<IOptionsMonitor<MQTTOptions>>()!.CurrentValue;
            var factory = new MqttClientFactory();
            var client = factory.CreateMqttClient();
            var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(op.Host, op.Port)
            .WithCredentials(op.UserName, op.Password)
            .Build();

            await client.ConnectAsync(clientOptions, default);
            return factory.CreateMqttClient();
        });
        services.AddSingleton<IMqttService, MqttService>();

        return services;
    }
}