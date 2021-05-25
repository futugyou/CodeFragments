using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    [EventSource(Name = "ddd_sqllit_db")]
    public sealed class DatabaseEventSource : EventSource
    {
        public static readonly DatabaseEventSource Instance = new DatabaseEventSource();
        private DatabaseEventSource() : base(EventSourceSettings.EtwSelfDescribingEventFormat) { }

        [Event(1, Level = EventLevel.Informational, Keywords = EventKeywords.None, Opcode = EventOpcode.Info,
            Task = EventTasks.DA, Tags = Tags.SQLLIT, Version = 1, Message = "sql : type : {0}, command text : {1}")]
        public void OnCammandExecute(int commandType, string commandText)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.All, EventChannel.Debug))
            {
                WriteEvent(1, commandType, commandText);
            }
        }

        [Event(2, Level = EventLevel.Informational, Keywords = EventKeywords.None, Opcode = EventOpcode.Info)]
        public void PayloadHad(Payload payload)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.All, EventChannel.Debug))
            {
                WriteEvent(2, payload);
            }
        }

        [Event(3, Level = EventLevel.Informational, Keywords = EventKeywords.None, Opcode = EventOpcode.Info)]
        public void RegisterComplete()
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.All, EventChannel.Debug))
            {
                WriteEvent(3, "customer", " register compete");
            }
        }
    }

    public class EventTasks
    {
        public const EventTask UI = (EventTask)1;
        public const EventTask Business = (EventTask)2;
        public const EventTask DA = (EventTask)3;
    }

    public class Tags
    {
        public const EventTags MSSQL = (EventTags)1;
        public const EventTags SQLLIT = (EventTags)2;
        public const EventTags DB2 = (EventTags)3;
    }
}
