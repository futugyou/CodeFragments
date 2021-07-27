using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
