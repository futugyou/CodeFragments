using System;
using System.Threading.Tasks;

namespace MessageConsumer
{
    public interface IEventConsunmer
    {
        Task PrepareAsync();
        Task CloseAsync();
        ValueTask DisposeAsync();
    }
}
