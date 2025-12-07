
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
    public IAsyncEnumerable<string> Sequential()
    {
        return _service.Sequential();
    }

    [Route("concurrent")]
    [HttpPost]
    public IAsyncEnumerable<string> Concurrent()
    {
        return _service.Concurrent();
    }
}
