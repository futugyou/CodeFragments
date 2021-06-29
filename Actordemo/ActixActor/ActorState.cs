using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActixActor
{
    public enum ActorState
    { /// Actor is started.
        Started,
        /// Actor is running.
        Running,
        /// Actor is stopping.
        Stopping,
        /// Actor is stopped.
        Stopped,
    }

    public static class ActorStateExpand
    {
        public static bool Alive(this ActorState actorState)
        {
            return actorState == ActorState.Started || actorState == ActorState.Running;
        }
        public static bool Stopping(this ActorState actorState)
        {
            return actorState == ActorState.Stopping || actorState == ActorState.Stopped;
        }
    }

    public enum Running
    {
        Stop,
        Continue,
    }
}
