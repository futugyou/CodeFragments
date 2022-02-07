using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace GrpcServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    [Authorize]
    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        _logger.LogInformation("Got Message: " + request.Name);
        await Task.Delay(1000);
        return new HelloReply
        {
            Message = "Hello " + request.Name + ", auth User: " + user.Identity!.Name
        };
    }
}
