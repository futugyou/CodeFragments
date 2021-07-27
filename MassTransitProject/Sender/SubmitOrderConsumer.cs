using MassTransit;
using System.Threading.Tasks;
using System;

namespace Sender
{
    public interface SubmitOrder
    {
        Guid OrderId { get; }
    }

    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            await context.RespondAsync<OrderStatus>(new
            {
                context.Message.OrderId,
                Status = "Submitted"
            });
        }
    }
}