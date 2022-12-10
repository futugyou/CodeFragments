using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;

namespace EventClient;

public class TamplatedConsoleFormatter : ConsoleFormatter
{
    private readonly bool _includeScopes;
    private readonly string _tamplate;

    public TamplatedConsoleFormatter(IOptions<TamplatedConsoleFormatterOptions> options) : base("tamplated")
    {
        _includeScopes = options.Value.IncludeScopes;
        _tamplate = options.Value.Tamplate ?? "[{LogLevel}]{Category}/{EventId}:{Message}\n{Scope}\n";
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var builder = new StringBuilder(_tamplate);
        builder.Replace("{Catefory}", logEntry.Category);
        builder.Replace("{Eventid}", logEntry.EventId.ToString());
        builder.Replace("{LogLevel}", logEntry.LogLevel.ToString());
        builder.Replace("{Message}", logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception));
        
        if (_includeScopes && scopeProvider != null)
        {
            var builder2 = new StringBuilder();
            var writer = new StringWriter(builder2);
            scopeProvider.ForEachScope(WriteScope, writer);
            void WriteScope(object? scope, StringWriter state)
            {
                writer.Write("=>", state);
            }
            builder.Replace("{Scope}", builder2.ToString());
        }

        textWriter.Write(builder);
    }
}