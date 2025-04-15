namespace AspnetcoreEx.MQTT;

public class MqttSubscribeHandler : IMqttSubscribeHandler
{
    public void HandleMessage(string topic, string payload)
    {
        Console.WriteLine($"[Subscribed] Topic: {topic}, Payload: {payload}");
        // TODO: may save to DB or process the message
    }
}
