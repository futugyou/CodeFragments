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
        //�����õ���ͬ���ķ�ʽ��Ҳ����ʹ���첽�� await HangfireJobClient.AddBackgroundJobAsync
        var result = await HangfireJobClient.AddBackgroundJobAsync(serverUrl, new BackgroundJob
        {
            JobName = "����api",
            Method = "Get",
            Url = "http://localhost:5000/testaaa",
            SendSuccess = true,
            DelayFromMinutes = 1 //�������ڶ��ٷ��Ӻ�ִ��
                                 //RunAt = new DateTime(2020,7,25,10,5,1) // Ҳ���Բ���ָ�� DelayFromMinutes ���� ֱ��ָ����ʲôʱ�����С�
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
        //�����õ���ͬ���ķ�ʽ��Ҳ����ʹ���첽�� await HangfireJobClient.AddRecurringJobAsync
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
