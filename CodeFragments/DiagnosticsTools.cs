using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFragments
{
    public class ProcessInfos
    {
        public string Name { get; internal set; }
        public int Id { get; internal set; }
        public int ThreadCount { get; internal set; }
        public TimeSpan TotalProcessorTime { get; internal set; }
        public ProcessPriorityClass PriorityClass { get; internal set; }
        public string StartTime { get; internal set; }
        public string PrivateMemory { get; internal set; }
        public string PeakVirtualMemory { get; internal set; }
        public string PeakPagedMemory { get; internal set; }
        public string PagedSystemMemory { get; internal set; }
        public string PagedMemory { get; internal set; }
        public string NonpagedSystemMemory { get; internal set; }
        public string PhysicalMemory { get; internal set; }
        public string VirtualMemory { get; internal set; }
    }
   
    public class DiagnosticsTools
    {
        private IEnumerable<Process> PrintProcessStatus()
        {
            return DiagnosticsClient.GetPublishedProcesses()
                    .Select(Process.GetProcessById)
                    .Where(process => process != null);

        }

        private void TriggerCoreDump(int processId)
        {

            var client = new DiagnosticsClient(processId);
            client.WriteDump(DumpType.Normal, $"./files/gump-{DateTime.Now.ToLongTimeString()}.dmp", false);

        }

        private ProcessInfos GetProInfo(Process info)
        {
            ProcessInfos infos = new ProcessInfos();
            infos.Name = info.ProcessName;
            infos.Id = info.Id;
            infos.ThreadCount = info.Threads.Count;
            infos.TotalProcessorTime = info.TotalProcessorTime;
            infos.PriorityClass = info.PriorityClass;
            infos.StartTime = info.StartTime.ToLongTimeString();
            infos.PrivateMemory = (info.PrivateMemorySize64 / 1024) + "K";
            infos.PeakVirtualMemory = (info.PeakVirtualMemorySize64 / 1024) + "K";
            infos.PeakPagedMemory = (info.PeakPagedMemorySize64 / 1024) + "K";
            infos.PagedSystemMemory = (info.PagedSystemMemorySize64 / 1024) + "K";
            infos.PagedMemory = (info.PagedMemorySize64 / 1024) + "K";
            infos.NonpagedSystemMemory = (info.NonpagedSystemMemorySize64 / 1024) + "K";
            infos.PhysicalMemory = (info.WorkingSet64 / 1024) + "K";
            infos.VirtualMemory = (info.VirtualMemorySize64 / 1024) + "K";
            return infos;
        }

        Dictionary<int, DiagnosticsClient> diagnosticsCache = new Dictionary<int, DiagnosticsClient>();

        private void PrintRuntime(int processId, int threshold = 90)
        {
            if (!diagnosticsCache.ContainsKey(processId))
            {
                var providers = new List<EventPipeProvider>()
                {
                    new EventPipeProvider("Microsoft-Windows-DotNETRuntime",EventLevel.Informational, (long)ClrTraceEventParser.Keywords.Default),
                    new EventPipeProvider("System.Runtime",EventLevel.Informational,(long)ClrTraceEventParser.Keywords.None, new Dictionary<string, string>() {{ "EventCounterIntervalSec", "1" }})
                };

                DiagnosticsClient client = new DiagnosticsClient(processId);
                diagnosticsCache[processId] = client;

                using EventPipeSession session = client.StartEventPipeSession(providers, false);

                var source = new EventPipeEventSource(session.EventStream);
                source.Clr.All += (TraceEvent obj) =>
                {
                    string msg = $"Clr-{obj.EventName}-";
                    if (obj.PayloadNames.Length > 0)
                    {
                        foreach (var item in obj.PayloadNames)
                            msg += $"{item}:{ obj.PayloadStringByName(item)}-";
                    }
                    Console.WriteLine(msg);
                };

                source.Dynamic.All += (TraceEvent obj) =>
                {
                    string msg = $"Dynamic-{obj.EventName}-{string.Join("|", obj.PayloadNames)}";
                    if (obj.EventName.Equals("EventCounters"))
                    {
                        var payloadFields = (IDictionary<string, object>)(obj.PayloadByName(""));
                        if (payloadFields != null)
                            payloadFields = payloadFields["Payload"] as IDictionary<string, object>;

                        if (payloadFields != null)
                        {
                            msg = $"Dynamic-{obj.EventName}-{payloadFields["DisplayName"]}:{payloadFields["Mean"]}{payloadFields["DisplayUnits"]}";
                            Console.WriteLine(msg);
                        }

                        if (payloadFields != null && payloadFields["Name"].ToString().Equals("cpu-usage"))
                        {
                            double cpuUsage = Double.Parse(payloadFields["Mean"].ToString());
                            if (cpuUsage > (double)threshold)
                            {
                                client.WriteDump(DumpType.Normal, "/files/minidump.dmp");
                            }
                        }
                    }
                    else
                    {
                        if (obj.PayloadNames.Length > 0)
                        {
                            foreach (var item in obj.PayloadNames)
                                msg += $"{item}:{ obj.PayloadStringByName(item)}-";
                        }
                        Console.WriteLine(msg);
                    }
                };

                source.Kernel.All += (TraceEvent obj) =>
                {
                    string msg = $"Kernel-{obj.EventName}-{string.Join("|", obj.PayloadNames)}";
                    Console.WriteLine(msg);
                };

                try
                {
                    source.Process();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void TraceProcessForDuration(int processId, int duration)
        {

            var cpuProviders = new List<EventPipeProvider>()
            {
                new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, (long)ClrTraceEventParser.Keywords.Default),
                new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational, (long)ClrTraceEventParser.Keywords.None),
            };

            var client = new DiagnosticsClient(processId);

            using var traceSession = client.StartEventPipeSession(cpuProviders);
            Task copyTask = Task.Run(async () =>
            {
                using FileStream fs = new FileStream($"/files/{processId}.dmp", FileMode.Create, FileAccess.Write);
                await traceSession.EventStream.CopyToAsync(fs);
            });

            copyTask.Wait(duration * 1000);
            traceSession.Stop();
        }

        public void Run()
        {
            var processes = PrintProcessStatus();
            foreach (var item in processes)
            {
                //TODO:  show it
                var _p = GetProInfo(item);
                TriggerCoreDump(item.Id);
                PrintRuntime(item.Id);
                TraceProcessForDuration(item.Id, 30);
            }

        }

    }
}
