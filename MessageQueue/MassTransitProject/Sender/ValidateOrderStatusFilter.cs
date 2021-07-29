using GreenPipes;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sender
{
    public class ValidateOrderStatusFilter<T> : IFilter<SendContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
        }

        public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            if (context.Message is SubmitOrder getOrderStatus && getOrderStatus.OrderId == Guid.Empty)
                throw new ArgumentException("The OrderId must not be empty");

            return next.Send(context);
        }
    }
}
