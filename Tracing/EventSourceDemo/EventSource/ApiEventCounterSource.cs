using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    /// <summary>
    /// Api.EventCounter 事件源
    /// </summary>
    [EventSource(Name = "Api.EventCounter")]
    public sealed class ApiEventCounterSource : EventSource
    {
        public static readonly ApiEventCounterSource Log = new ApiEventCounterSource();

        /// <summary>
        /// 统计指标收集器，比如平均值，最大值，最小值
        /// </summary>
        private EventCounter _requestCounter;
        /// <summary>
        /// 自定义统计指标收集器，通过自定义统计方法的方式实现对指标的统计
        /// </summary>
        private PollingCounter _workingSetCounter;

        private PollingCounter _totalRequestsCounter;
        /// <summary>
        /// 自定义累加指标收集器，通过自定义累函数，实现指标收集
        /// </summary>
        private IncrementingPollingCounter _incrementingPollingCounter;
        /// <summary>
        /// 累加指标收集器，采集一定时间段内的指标汇总
        /// </summary>
        private IncrementingEventCounter _incrementingEventCounter;

        private long _totalRequests;

        private ApiEventCounterSource()
        {
        }


        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                //请求响应耗时
                _requestCounter = new EventCounter("request-time", this)
                {
                    DisplayName = "Request Processing Time",
                    DisplayUnits = "ms"
                };

                //内存占用
                _workingSetCounter = new PollingCounter("working-set", this, () => (double)(Environment.WorkingSet / 1_000_000))
                {
                    DisplayName = "Working Set",
                    DisplayUnits = "MB"
                };

                //总请求量
                _totalRequestsCounter = new PollingCounter("total-requests", this, () => Volatile.Read(ref _totalRequests))
                {
                    DisplayName = "Total Requests",
                    DisplayUnits = "次"
                };

                //单位时间请求速率
                _incrementingPollingCounter = new IncrementingPollingCounter("Request Rate", this, () =>
                {
                    return Volatile.Read(ref _totalRequests);
                })
                {
                    DisplayName = "Request Rate",
                    DisplayUnits = "次/s",
                    //时间间隔1s
                    DisplayRateTimeScale = new TimeSpan(0, 0, 1)
                };

                _incrementingEventCounter = new IncrementingEventCounter("working-time", this)
                {
                    DisplayName = "Request Processing Time",
                    DisplayUnits = "ms"
                };
            }
        }

        public void Request(string url, float elapsedMilliseconds)
        {
            //更新请求数量（保证线程安全）
            Interlocked.Increment(ref _totalRequests);

            //写入指标值(请求处理耗时)
            _requestCounter?.WriteMetric(elapsedMilliseconds);

            _incrementingEventCounter.Increment(elapsedMilliseconds);
        }

        protected override void Dispose(bool disposing)
        {
            _requestCounter?.Dispose();
            _requestCounter = null;

            _workingSetCounter?.Dispose();
            _workingSetCounter = null;

            _totalRequestsCounter?.Dispose();
            _totalRequestsCounter = null;

            _incrementingPollingCounter?.Dispose();
            _incrementingPollingCounter = null;

            _incrementingEventCounter?.Dispose();
            _incrementingEventCounter = null;

            base.Dispose(disposing);
        }
    }
}
