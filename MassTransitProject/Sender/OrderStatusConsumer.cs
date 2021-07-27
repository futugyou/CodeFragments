using MassTransit;
using System.Threading.Tasks;
using System;

namespace Sender
{
    public interface OrderStatus
    {
        Guid OrderId { get; }
        string Status { get; }
    }

    public class OrderStatusConsumer : IConsumer<OrderStatus>
    {
        public async Task Consume(ConsumeContext<OrderStatus> context)
        {
            await context.RespondAsync<OrderComplete>(new
            {
                context.Message.OrderId,
                Status = "Complete"
            });
        }
    }

    public interface OrderComplete
    {
        Guid OrderId { get; }
        string Status { get; }
    }
}