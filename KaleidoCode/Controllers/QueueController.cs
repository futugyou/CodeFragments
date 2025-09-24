
using KaleidoCode.HostedService;

namespace KaleidoCode.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController : ControllerBase
{
    private readonly IBackgroundTaskQueue taskQueue;
    private readonly ILogger<QueueController> logger;

    public QueueController(IBackgroundTaskQueue taskQueue, ILogger<QueueController> logger)
    {
        this.taskQueue = taskQueue;
        this.logger = logger;
    }

    [HttpGet]
    public async Task Get()
    {
        await taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        var guid = Guid.NewGuid();

        logger.LogInformation("Queued work item {Guid} is starting.", guid);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
        }
        catch (OperationCanceledException)
        {
            // Prevent throwing if the Delay is cancelled
        }

        logger.LogInformation("Queued Background Task {Guid} is complete.", guid);

    }
}
