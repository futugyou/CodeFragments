using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActixActor
{
    public class Addr<T> where T : IActor
    {
        public AddressSender<T> Tx { get; set; }
    }

    public class AddressSender<T> where T : IActor
    {
        public Inner<T> Inner { get; set; }
        public SenderTask SenderTask { get; set; }
        public bool MaybeParked { get; set; }
    }

}
//pub struct AddressSender<A: Actor> {
//    // Channel state shared between the sender and receiver.
//    inner: Arc<Inner<A>>,

//    // Handle to the task that is blocked on this sender. This handle is sent
//    // to the receiver half in order to be notified when the sender becomes
//    // unblocked.
//    sender_task: Arc<Mutex<SenderTask>>,

//    // True if the sender might be blocked. This is an optimization to avoid
//    // having to lock the mutex most of the time.
//    maybe_parked: Arc<AtomicBool>,
//}