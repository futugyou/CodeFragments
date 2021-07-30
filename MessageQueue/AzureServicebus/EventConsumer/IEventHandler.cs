using CloudNative.CloudEvents;
using System.Threading.Tasks;

namespace EventConsumer
{
    public interface IEventHandler
    {
        Task HandlerEvent(CloudEvent cloudEvent);
    }
}
