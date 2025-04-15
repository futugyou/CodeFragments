using AspnetcoreEx.MQTT;

namespace AspnetcoreEx.Controllers;

[Route("api/mqtt")]
[ApiController]
public class MqttPublishController : ControllerBase
{
    private readonly IMqttService _mqttService;

    public MqttPublishController(IMqttService mqttService)
    {
        _mqttService = mqttService;
    }

    [HttpGet]
    [Route("publish")]
    public async Task<IActionResult> Publish([FromQuery] string topic, [FromQuery] string payload)
    {
        await _mqttService.PublishAsync(topic, payload);
        return Ok(new { message = "Message published." });
    }
}
