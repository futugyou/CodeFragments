using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueActor
{
    public class Counter : AbstractActor<int>
    {
        private int m_value;
        public Counter() : this(0) { }
        public Counter(int init)
        {
            m_value = init;
        }
        protected override void Receive(int message)
        {
            m_value += message;
            if (message == -1)
            {
                Console.WriteLine(m_value);
                this.Exit();
            }
        }
    }
}
