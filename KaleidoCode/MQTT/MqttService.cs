using MQTTnet;

namespace KaleidoCode.MQTT;

public class MqttService : IMqttService
{
    private readonly IOptionsMonitor<MQTTOptions> _optionsMonitor;

    public MqttService(IOptionsMonitor<MQTTOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();
        var op = _optionsMonitor.CurrentValue;
        var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(op.Host, op.Port)
                .WithCredentials(op.UserName, op.Password)
                .Build();
        await client.ConnectAsync(clientOptions, default);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await client.PublishAsync(message);
    }
}
