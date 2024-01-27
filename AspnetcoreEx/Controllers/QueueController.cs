using System.Text.Json.Serialization;
using AspnetcoreEx.Extensions;
using AspnetcoreEx.HostedService;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("[controller]")]
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
        await taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync)
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        int delayLoop = 0;
        var guid = Guid.NewGuid();

        logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++delayLoop;

            logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }

        if (delayLoop is 3)
        {
            logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
        }
        else
        {
            logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
        }
    }
}
