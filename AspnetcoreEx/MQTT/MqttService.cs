using MQTTnet;

namespace AspnetcoreEx.MQTT;

public class MqttService : IMqttService
{
    private readonly IMqttClient _mqttClient;

    public MqttService(IMqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}
