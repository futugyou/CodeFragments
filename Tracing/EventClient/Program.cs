using System;

namespace EventClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!"); 
            EventSourceEx6.ThisEventSource();
            var pro = new DiagnosticsTools();
            pro.Run();

        }
    }
}
