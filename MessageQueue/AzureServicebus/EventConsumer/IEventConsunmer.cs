using System;
using System.Threading.Tasks;

namespace EventConsumer
{
    public interface IEventConsunmer
    {
        Task PrepareAsync();
        Task CloseAsync();
        ValueTask DisposeAsync();
    }
}
