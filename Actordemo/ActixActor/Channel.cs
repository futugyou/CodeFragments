using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActixActor
{

    public class Inner<T> where T : IActor
    {
        //    buffer: AtomicUsize,

        //// Internal channel state. Consists of the number of messages stored in the
        //// channel as well as a flag signalling that the channel is closed.
        //state: AtomicUsize,

        //// Atomic, FIFO queue used to send messages to the receiver.
        //message_queue: Queue<Envelope<A>>,

        //// Atomic, FIFO queue used to send parked task handles to the receiver.
        //parked_queue: Queue<Arc<Mutex<SenderTask>>>,

        //// Number of senders in existence.
        //num_senders: AtomicUsize,

        //// Handle to the receiver's task.
        //recv_task: AtomicWaker,

        public uint Buffer { get; set; }
        public uint State { get; set; }
        public ConcurrentQueue<Envelope<T>> MessageQueue { get; set; }
        public ConcurrentQueue<SenderTask> ParkedQueue { get; set; }
        public uint NumSenders { get; set; }
        //public uint RecvTask { get; set; }
    }
}
