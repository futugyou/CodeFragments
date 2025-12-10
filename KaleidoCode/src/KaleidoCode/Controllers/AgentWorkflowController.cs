
using AgentStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/af/agent-workflow")]
[ApiController]
public class AgentWorkflowController : ControllerBase
{
    private readonly WorkflowService _service;
    public AgentWorkflowController(WorkflowService service)
    {
        _service = service;
    }

    [Route("sequential")]
    [HttpPost]
    public IAsyncEnumerable<string> Sequential(bool streaming = false)
    {
        return _service.Sequential(streaming);
    }

    [Route("concurrent")]
    [HttpPost]
    public IAsyncEnumerable<string> Concurrent()
    {
        return _service.Concurrent();
    }

    [Route("handoffs")]
    [HttpPost]
    public IAsyncEnumerable<string> Handoffs(string question = "What is the square root of 2?")
    {
        return _service.Handoffs(question);
    }
}
