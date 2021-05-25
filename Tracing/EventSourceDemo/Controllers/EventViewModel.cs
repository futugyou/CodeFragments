using System;

namespace EventSourceDemo.Controllers
{
    public class EventViewModel
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
    }
}