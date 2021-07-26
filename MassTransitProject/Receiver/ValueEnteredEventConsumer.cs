using MassTransit;
using System;
using System.Threading.Tasks;

namespace Receiver
{
    public class ValueEnteredEventConsumer : IConsumer<ValueEntered>
    {
        public Task Consume(ConsumeContext<ValueEntered> context)
        {
            Console.WriteLine(context?.Message?.Value);
            return Task.CompletedTask;
        }
    }
}