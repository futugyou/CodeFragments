using MQTTnet;

namespace KaleidoCode.MQTT;

public class MqttBackgroundSubscriber : BackgroundService
{
    private readonly IMqttSubscribeHandler _handler;
    private readonly ILogger<MqttBackgroundSubscriber> _logger;
    private readonly IOptionsMonitor<MQTTOptions> _option;
    public MqttBackgroundSubscriber(IMqttSubscribeHandler handler, IOptionsMonitor<MQTTOptions> option, ILogger<MqttBackgroundSubscriber> logger)
    {
        _handler = handler;
        _logger = logger;
        _option = option;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var op = _option.CurrentValue;
                if (!op.AllowMQTTServer)
                {
                    _logger.LogInformation("MQTT server is not allowed");
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }
                
                var factory = new MqttClientFactory();
                var client = factory.CreateMqttClient();
                var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(op.Host, op.Port)
                .WithCredentials(op.UserName, op.Password)
                .Build();

                client.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine("Received application message.");
                    var topic = e.ApplicationMessage.Topic;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _handler.HandleMessage(topic, payload);
                    return Task.CompletedTask;
                };

                await client.ConnectAsync(clientOptions, stoppingToken);

                var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithUserProperty("id", "2").Build();

                await client.SubscribeAsync(mqttSubscribeOptions, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MQTT background subscriber");
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
