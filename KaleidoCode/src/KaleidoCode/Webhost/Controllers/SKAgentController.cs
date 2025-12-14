
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/agent")]
[ApiController]
public class SKAgentController : ControllerBase
{
    private readonly AgentService _service;

    public SKAgentController(AgentService service)
    {
        _service = service;
    }

    [Route("joker")]
    [HttpPost]
    /// <summary>
    /// base sk agent demo
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<string> Joker()
    {
        return _service.Joker();
    }

    [Route("lights")]
    [HttpPost]
    /// <summary>
    /// sk agent demo with plugin
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<string> Lights()
    {
        return _service.Lights();
    }

    [Route("chat")]
    [HttpPost]
    /// <summary>
    /// sk agent demo for chat
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<string> Chat(int maximumIterations = 4)
    {
        return _service.Chat(maximumIterations);
    }

    [Route("di")]
    [HttpPost]
    /// <summary>
    /// sk agent demo for di
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<string> DI(int maximumIterations = 4)
    {
        return _service.DI(maximumIterations);
    }

    [Route("function")]
    [HttpPost]
    /// <summary>
    /// sk agent demo for agent as function
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<string> Function()
    {
        return _service.Function();
    }

    [Route("concurrent")]
    [HttpPost]
    public IAsyncEnumerable<string> Concurrent(string query = "What is temperature?")
    {
        return _service.Concurrent(query);
    }

    [Route("sequential")]
    [HttpPost]
    public IAsyncEnumerable<string> Sequential(string query = "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hours")
    {
        return _service.Sequential(query);
    }

    [Route("groupChat")]
    [HttpPost]
    public IAsyncEnumerable<string> GroupChat(string query = "Create a slogan for a new electric SUV that is affordable and fun to drive.")
    {
        return _service.GroupChat(query);
    }

    [Route("handoff")]
    [HttpPost]
    public IAsyncEnumerable<string> Handoff(string query = "I am a customer that needs help with my orders")
    {
        return _service.Handoff(query);
    }


}
