using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowActor
{
    public abstract class AbstractActor : IActor
    {
        private readonly ActionBlock<IMessage> _action;

        public AbstractActor()
        {
            _action = new ActionBlock<IMessage>(message =>
            {
                Handle(message);
            });
        }

        public abstract void Handle(IMessage message);

        public void Send(IMessage message)
        {
            _action.Post(message);
        }
    }
}
