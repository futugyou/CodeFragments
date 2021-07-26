using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Sender
{
    public class Worker : BackgroundService
    {
        readonly IBus _bus;

        public Worker(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _bus.Publish(new Message { Text = $"The time is {DateTimeOffset.Now}" });

                await Task.Delay(1000, stoppingToken);
            }
        }
    }

}