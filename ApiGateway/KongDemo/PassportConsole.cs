using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KongDemo
{
    public class PassportConsole
    {
        public static void Success(string value)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Information(string value) => Console.WriteLine(value);

        public static void Warning(string value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Error(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}
