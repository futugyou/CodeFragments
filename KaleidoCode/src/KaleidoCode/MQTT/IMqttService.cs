namespace KaleidoCode.MQTT;

public interface IMqttService
{
    Task PublishAsync(string topic, string payload);
}
