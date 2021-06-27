using System;

namespace QueueActor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!"); 
            Counter counter = new Counter();
            for (int i = 0; i < 10000; i++)
            {
                counter.Post(i);
            }
            counter.Post(-1);
            Console.ReadLine();
        }
    }
}
