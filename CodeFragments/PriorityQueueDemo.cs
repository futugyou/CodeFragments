using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeFragments
{
    public class PriorityQueueDemo
    {
        public static void Exection()
        {
            var priorityQueue = new PriorityQueue<string, int>();
            priorityQueue.Enqueue("Alice", 100);
            priorityQueue.EnqueueRange(Enumerable.Range(1, 5)
                .Select(x => ($"X_{x}", 100 - x))
            );

            while (priorityQueue.TryDequeue(out var element, out var priority))
            {
                Console.WriteLine($"Element:{element}, {priority}");
            }

            Console.WriteLine($"-------------------------------");
            var random = new Random();
            var queue = new PriorityQueue<string, (DateTime time, int priority)>(new DateTimePriorityComparer());

            var time = DateTime.UtcNow;
            Thread.Sleep(1000);
            for (var k = 0; k < 3; k++)
            {
                for (var i = 1; i <= 3; i++)
                {
                    queue.Enqueue($"Message_{i}_{k}", (i > 2 ? time : DateTime.UtcNow, random.Next(5, 10)));
                }
            }

            while (queue.TryDequeue(out var message, out var priority))
            {
                Console.WriteLine($"{message}, {priority.priority}, {priority.time}");
            }
        }

        private class DateTimePriorityComparer : IComparer<(DateTime time, int priority)>
        {
            public int Compare((DateTime time, int priority) x, (DateTime time, int priority) y)
            {
                var priorityComparison = x.priority.CompareTo(y.priority);
                if (priorityComparison != 0) return priorityComparison;
                return x.time.CompareTo(y.time);
            }
        }
    }
}
