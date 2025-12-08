
using AgentStack.Skills;
using AgentStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/af/agent")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;
    public AgentController(AgentService agentService)
    {
        _agentService = agentService;
    }

    [Route("joker")]
    [HttpPost]
    public async Task<string> Joker(string message = "Tell me a joke about a pirate.")
    {
        return await _agentService.Joker(message);
    }

    [Route("joker-stream")]
    [HttpPost]
    public IAsyncEnumerable<string> JokerStream(string message = "Tell me a joke about a pirate.")
    {
        return _agentService.JokerStream(message);
    }

    [Route("joker-message")]
    [HttpPost]
    public async Task<string> JokerMessage(
        string message = "Tell me a joke about this image?",
        string imageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/11/Joseph_Grimaldi.jpg",
        bool hasSystemMessage = false)
    {
        return await _agentService.JokerMessage(message, imageUrl, hasSystemMessage);
    }

    [Route("thread")]
    [HttpPost]
    public IAsyncEnumerable<string> Thread()
    {
        return _agentService.Thread();
    }

    [Route("function")]
    [HttpPost]
    public IAsyncEnumerable<string> Function()
    {
        return _agentService.Function();
    }

    [Route("approval")]
    [HttpPost]
    public IAsyncEnumerable<string> Approval(bool allowChangeState = false)
    {
        return _agentService.Approval(allowChangeState);
    }

    /// <summary>
    /// This example will not run successfully, regardless of whether an object or a list is used in CreateJsonSchema. 
    /// The reason is that the stupid OpenAI requires JsonSchema to be an object, meaning GetLightsAsync cannot directly return a list; 
    /// it must be contained within an object.
    /// 
    /// The current code prints only one object in WriteLine. 
    /// If it's changed to CreateJsonSchema(typeof(LightModel[])) or CreateJsonSchema(typeof(List<LightModel>)), 
    /// an error will occur during RunAsync, indicating that it must be 'object'.
    /// </summary>
    /// <returns></returns>
    [Route("structured")]
    [HttpPost]
    public async Task<List<LightModel>> Structured()
    {
        return await _agentService.Structured();
    }

    [Route("agent-tool")]
    [HttpPost]
    public IAsyncEnumerable<string> AgentToTool()
    {
        return _agentService.AgentToTool();
    }

    [Route("history-storage")]
    [HttpPost]
    public IAsyncEnumerable<string> HistoryStorage()
    {
        return _agentService.HistoryStorage();
    }

    [Route("history-memory")]
    [HttpPost]
    public IAsyncEnumerable<string> HistoryMemory(string useid)
    {
        return _agentService.HistoryMemory(useid);
    }

    [Route("rag")]
    [HttpPost]
    public IAsyncEnumerable<string> RAG()
    {
        return _agentService.RAG();
    }

}