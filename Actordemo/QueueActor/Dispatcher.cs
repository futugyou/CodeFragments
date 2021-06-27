using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueActor
{
    class Dispatcher
    {
        public static Dispatcher Instance = new Dispatcher();
        private Dispatcher()
        {
        }

        public void ReadToExecute(IActor actor)
        {
            if (actor.Exited)
            {
                return;
            }

            int status = Interlocked.CompareExchange(ref actor.Context.m_status, ActorContext.EXECUTING, ActorContext.WAITTING);
            if (status == ActorContext.EXECUTING)
            {
                ThreadPool.QueueUserWorkItem(this.Execute, actor);
            }
        }

        private void Execute(object state)
        {
            IActor actor = (IActor)state;
            actor.Execute();
            if (actor.Exited)
            {
                Thread.VolatileWrite(ref actor.Context.m_status, ActorContext.EXITED);
            }
            else
            {
                Thread.VolatileWrite(ref actor.Context.m_status, ActorContext.WAITTING);
            }
            if (actor.MessageCount > 0)
            {
                ReadToExecute(actor);
            }
        }
    }
}
