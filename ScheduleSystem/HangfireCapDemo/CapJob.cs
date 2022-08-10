using DotNetCore.CAP;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HangfireCapDemo;

public class CapJob : ICapJob
{
    private readonly ICapPublisher _capBus;
    private readonly ILogger<CapJob> _logger;

    public CapJob(ICapPublisher capBus, ILogger<CapJob> logger)
    {
        _capBus = capBus;
        _logger = logger;
    }

    /// <summary>
    /// 发起CAP调度
    /// </summary>
    /// <param name="item">job详情</param>
    /// <param name="jobName">job名称</param>
    /// <param name="queuename">指定queue名称(Note: Hangfire queue names need to be lower case)</param>
    /// <param name="isretry">是否cap调用出错重试</param>
    /// <param name="context">上下文</param>
    [AutomaticRetrySet(Attempts = 3, DelaysInSeconds = new[] { 20, 30, 60 }, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    [CapJobDisplayName("{0}")]
    //[CapJobFilter(timeoutInSeconds: 30)]
    public async Task Excute(CapJobItem item, string jobName = null, string queuename = null, bool isretry = false, PerformContext context = null)
    {
        try
        {
            object runTimeDataItem = null;
            context?.Items.TryGetValue("Data", out runTimeDataItem);
            if (runTimeDataItem != null)
            {
                var runTimeData = runTimeDataItem as string;
                if (!string.IsNullOrEmpty(runTimeData))
                {
                    item.Data = runTimeData;
                }
            }

            await Run(item, context);
        }
        catch (Exception ex)
        {
            context.SetTextColor(ConsoleTextColor.Red);
            context.WriteLine(ex.ToString());

            if (item.RetryTimes <= 0)
            {
                throw;
            }

            //获取重试次数
            var count = context.GetJobParameter<string>("RetryCount") ?? string.Empty;
            if (count == item.RetryTimes.ToString())
            {
                AddErrToJob(context, ex);
                return;
            }

            context?.Items.Add("RetryCount", count);
            throw;
        }
    }

    private static void AddErrToJob(PerformContext context, Exception ex)
    {
        context.SetJobParameter("jobErr", ex.Message);
    }

    private async Task Run(CapJobItem item, PerformContext context)
    {
        try
        {
            context.SetTextColor(ConsoleTextColor.Yellow);
            context.WriteLine($"JobStart: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            context.WriteLine($"JobName: {item.JobName ?? string.Empty}|QueuenName: {(string.IsNullOrEmpty(item.QueueName) ? "default" : item.QueueName)}");
            context.WriteLine($"JobParam: 【{JsonConvert.SerializeObject(item)}】");

            //await _capBus.PublishAsync(item.CapEventName, JsonConvert.DeserializeObject<dynamic>(item.Data));
            await _capBus.PublishAsync(item.CapEventName, item.Data);

            context.WriteLine($"JobEnd: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            context.SetTextColor(ConsoleTextColor.Red);
            context.WriteLine("【HttpJob Timeout】：" + ex.Message);
            throw;
        }
    }
}
