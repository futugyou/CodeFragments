using Hangfire.HttpJob.Client;
using Microsoft.AspNetCore.Mvc;

namespace HangfireClientDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("job")]
    public async Task<AddBackgroundHangfirJobResult> Job()
    {
        var serverUrl = "https://localhost:7137/job";
        //下面用的是同步的方式，也可以使用异步： await HangfireJobClient.AddBackgroundJobAsync
        var result = await HangfireJobClient.AddBackgroundJobAsync(serverUrl, new BackgroundJob
        {
            JobName = "测试api",
            Method = "Get",
            Url = "http://localhost:5000/testaaa",
            SendSuccess = true,
            DelayFromMinutes = 1 //这里是在多少分钟后执行
                                 //RunAt = new DateTime(2020,7,25,10,5,1) // 也可以不用指定 DelayFromMinutes 参数 直接指定在什么时候运行。
        });
        return result;
    }

    [HttpGet("deletejob")]
    public async Task<HangfirJobResult> DeleteJob(string jobid)
    {
        var serverUrl = "https://localhost:7100/hangfire";
        var result = await HangfireJobClient.RemoveBackgroundJobAsync(serverUrl, jobid);
        return result;
    }


    [HttpGet("cron")]
    public async Task<HangfirJobResult> Cron()
    {
        var serverUrl = "https://localhost:7100/hangfire";
        //下面用的是同步的方式，也可以使用异步： await HangfireJobClient.AddRecurringJobAsync
        var result = await HangfireJobClient.AddRecurringJobAsync(serverUrl, new RecurringJob()
        {
            JobName = "loginsystem",
            Method = "Post",
            Data = new { name = "aaa", age = 10 },
            Url = "http://localhost:5000/testpost",
            Cron = "0/5 * * * *"
        });
        return result;
    }


    [HttpGet("deletecron")]
    public async Task<HangfirJobResult> DeleteCron(string jobid)
    {
        var serverUrl = "https://localhost:7100/hangfire";
        var result = await HangfireJobClient.RemoveRecurringJobAsync(serverUrl, jobid);
        return result;
    }

}
