using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcClient;
using GrpcServer;
using GrpcServer.Model;
using ProtoBuf.Grpc.Client;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://localhost:50001", new GrpcChannelOptions
{
    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
});
var invoker = channel.Intercept(new ClientLoggerInterceptor());
// 1. base demo
// var client = new Greeter.GreeterClient(channel);
// var reply = await client.SayHelloAsync(
//                   new HelloRequest { Name = "MessageFromClient" });
// Console.WriteLine("Greeting: " + reply.Message);

// 2. StreamingFromServer demo
// var client2 = new FourType.FourTypeClient(channel);
var client2 = new FourType.FourTypeClient(invoker);
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

// 5. code first
var client5 = channel.CreateGrpcService<IOrderService>();
var reply5 = await client5.GetOrders(new OrderRequest { OrderId = 10 });
Console.WriteLine("Code First Response : " + reply5.Amount);

// 6. Health Check
// var client6 = new Health.HealthClient(channel);
// var response = await client6.CheckAsync(new HealthCheckRequest());
// var status = response.Status;
// Console.WriteLine("Health Check Response : " + status);

// 7 field mask 
var client7 = new GrpcServer.FieldMaskService.FieldMaskServiceClient(channel);
var fieldMask = new FieldMask();
fieldMask.Paths.AddRange(new string[] { "seq", "like" });
var maskRequest = new FieldMaskRequest
{
    Message = "ok",
};
maskRequest.FieldMask = fieldMask;
var maskresponse = client7.UnaryCall(maskRequest);
Console.WriteLine("mask reply seq " + maskresponse.Seq);
Console.WriteLine("mask reply replay " + maskresponse.Replay);
Console.WriteLine("mask reply like " + string.Join(",", maskresponse.Like));
Console.WriteLine("Press any key to exit...");
Console.ReadKey();