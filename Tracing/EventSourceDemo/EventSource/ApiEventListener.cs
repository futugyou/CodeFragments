﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    public delegate void WriteContent(string key, string value);
    /// <summary>
    /// In-process use
    /// </summary>
    public class ApiEventListener : EventListener
    {
        public event WriteContent WriteEvent;
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (!eventSource.Name.Equals("Api.EventCounter"))
            {
                return;
            }

            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string>()
            {
                ["EventCounterIntervalSec"] = "1"
            });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!eventData.EventName.Equals("Api.EventCounter"))
            {
                return;
            }

            for (int i = 0; i < eventData.Payload.Count; ++i)
            {
                if (eventData.Payload[i] is IDictionary<string, object> eventPayload)
                {
                    var (counterName, counterValue) = GetRelevantMetric(eventPayload);
                    Console.WriteLine($"{counterName} : {counterValue}");
                    WriteEvent?.Invoke(counterName, counterValue);
                }
            }
        }

        /// <summary>
        /// 计数器名称和计数器值
        /// </summary>
        /// <param name="eventPayload"></param>
        /// <returns></returns>
        private static (string counterName, string counterValue) GetRelevantMetric(IDictionary<string, object> eventPayload)
        {
            var counterName = "";
            var counterValue = "";

            if (eventPayload.TryGetValue("DisplayName", out object displayValue))
            {
                counterName = displayValue.ToString();
            }
            if (eventPayload.TryGetValue("Mean", out object value) ||
                eventPayload.TryGetValue("Increment", out value))
            {
                counterValue = value.ToString();
            }

            return (counterName, counterValue);
        }
    }
}
