using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcClient;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://localhost:50001", new GrpcChannelOptions
{
    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
});

// 1. base demo
// var client = new Greeter.GreeterClient(channel);
// var reply = await client.SayHelloAsync(
//                   new HelloRequest { Name = "MessageFromClient" });
// Console.WriteLine("Greeting: " + reply.Message);

// 2. StreamingFromServer demo
var client2 = new FourType.FourTypeClient(channel);
var call2 = client2.StreamingFromServer(new FourTypeRequest
{
    PageIndex = 1,
    PageSize = 10,
    IsDescending = true,
});

while (await call2.ResponseStream.MoveNext())
{
    Console.WriteLine("StreamingFromServer: " + call2.ResponseStream.Current.Seq);
}

// 3. StreamingFromClient
var call3 = client2.StreamingFromClient();
for (int i = 0; i < 4; i++)
{
    await call3.RequestStream.WriteAsync(new FourTypeRequest { PageIndex = i });
}
await call3.RequestStream.CompleteAsync();
var response3 = await call3;
Console.WriteLine("StreamingFromClient: " + response3.Seq);

// 4. StreamingBothWays
var call4 = client2.StreamingBothWays();
var readTask = Task.Run(async () =>
{
    await foreach (var response4 in call4.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine("StreamingBothWays Response: " + response4.Seq);
    }
});
for (int i = 0; i < 3; i++)
{
    await call4.RequestStream.WriteAsync(new FourTypeRequest { PageIndex = 70 + i });
}
await call4.RequestStream.CompleteAsync();
await readTask;
Console.WriteLine("Press any key to exit...");
Console.ReadKey();