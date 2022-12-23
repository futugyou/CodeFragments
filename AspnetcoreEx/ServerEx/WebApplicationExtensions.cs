using HttpMachine;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Text;

namespace AspnetcoreEx.ServerEx;

public static async Task UseIConnectionListenerFactory()
{
    var factory = WebApplication.Create().Services.GetRequiredServoce<IConnectionListenerFactory>();
    var lietener = await factory.BindAsync(new IPEndPoint(IPAddress.Any, 6000));
    while (true)
    {
        var context = await listener.AcceptAsync();
        _ = HandlerAsync(context);        
    }

    async Task HandlerAsync(ConnectionContext connection)
        {
            var reader = connection!.Transport.Input;
            while (true)
            {
                var result = await reader.ReadAsync();
                var request = ParseRequest(result);
                reader.AdvanceTo(result.Buffer.End);
                Console.WriteLine("[{0}] Receive request: {1} {2} Connection:{3}",connection.ConnectionId, request.Method, request.Path, request.Headers?["Connection"]??"N/A");
                var response = @"HTTP1.1 200 OK
Content-Type: text/plain; charset=utf-8
Content-Length: 12

Hello World";

                await connection.Transport.Output.WriteAsync(Encoding.UTF8.GetBytes(response));

                if (request.Headers.TryGetValue("Connection", out var value) && string.Compare(value, "close", true) == 0)
                {
                    await connection.DisposeAsync();
                    return;
                }
                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

    HttpRequestFeature ParseRequest(ReadResult result)
    {
        var handler = new HttpParseerHandler();
        var parserHandler = new HttpParser(handler);
        parserHandler.Execute(new ArraySegment<byte>(result.Buffer.ToArray()));
        return handler.Request;
    }
}