using System;

namespace EventClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!"); 
            
            // var pro = new DiagnosticsTools();
            // pro.Run();
            
            // EventSourceEx6.ThisEventSource();
            // EventSourceEx6.UseDelimitedListTraceListener();
            // EventSourceEx6.TraceSourceLog();
            // EventSourceEx6.LoggerMessageUsecase();
            // EventSourceEx6.ActivityUseCase();
            // EventSourceEx6.ActivityUseCase2();
            // EventSourceEx6.SimpleConsoleFormatterUsecase(args);
            EventSourceEx6.SystemdConsoleFormatterUsecase(args);
        }
    }
}
