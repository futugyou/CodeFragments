using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueActor
{
    public abstract class AbstractActor<T> : IActor
    {
        private ActorContext m_context;
        private Queue<T> m_messageQueue = new Queue<T>();
        private bool m_exited = false;
        protected AbstractActor()
        {
            m_context = new ActorContext(this);
        }

        public bool Exited => m_exited;

        public int MessageCount => m_messageQueue.Count;

        public ActorContext Context => m_context;

        public void Execute()
        {
            T message;
            lock (m_messageQueue)
            {
                if (m_messageQueue.TryDequeue(out message))
                {
                    Receive(message);
                }
            }
        }
        protected abstract void Receive(T message);
        public void Post(T message)
        {
            if (m_exited)
            {
                return;
            }
            lock (m_messageQueue)
            {
                m_messageQueue.Enqueue(message);
            }
            Dispatcher.Instance.ReadToExecute(this);
        }
        protected void Exit()
        {
            m_exited = true;
        }
    }
}
