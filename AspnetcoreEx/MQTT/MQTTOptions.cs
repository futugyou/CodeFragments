
namespace AspnetcoreEx.MQTT;

public class MQTTOptions
{
    public string ClientId { get; set; } = "mqtt_client";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string UserName { get; set; } = "username";
    public string Password { get; set; } = "password";
    public bool CleanSession { get; set; } = true;
    public string Topic { get; set; } = "test/topic";
}