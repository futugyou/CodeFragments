namespace KaleidoCode.MQTT;

public interface IMqttSubscribeHandler
{
    void HandleMessage(string topic, string payload);
}
