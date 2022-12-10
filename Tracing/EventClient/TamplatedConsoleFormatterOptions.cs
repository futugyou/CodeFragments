using Microsoft.Extensions.Logging.Console;

namespace EventClient;
public class TamplatedConsoleFormatterOptions : ConsoleFormatterOptions
{
    public string Tamplate { get; set; }
}