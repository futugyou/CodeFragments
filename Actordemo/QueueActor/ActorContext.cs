using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueActor
{
    public class ActorContext
    {
        public IActor Actor { get; private set; }
        public ActorContext(IActor actor)
        {
            Actor = actor;
        }
        public const int WAITTING = 0;
        public const int EXECUTING = 1;
        public const int EXITED = 2;

        public int m_status;
    }
}
