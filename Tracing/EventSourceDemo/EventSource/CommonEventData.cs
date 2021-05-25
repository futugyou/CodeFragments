using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    [EventData]
    public class Payload
    {
        public SubPayload Sub { get; set; }
        public IEnumerable<SubPayload> SubPayloads { get; set; }
        public IDictionary<Guid, SubPayload> KeyValuePairs { get; set; }
    }

    [EventData]
    public class SubPayload
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SubPayload(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
