using System;

namespace SpanTest
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            await PipelinesTest.Show();
            Console.ReadLine();
        }
    }
}
