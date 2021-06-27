using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueActor
{
    public interface IActor
    {
        void Execute();
        bool Exited { get; }
        int MessageCount { get; }
        ActorContext Context { get; }
    }
}
