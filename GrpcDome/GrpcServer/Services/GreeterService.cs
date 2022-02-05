using Grpc.Core;

namespace GrpcServer.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Got Message: " + request.Name);
        await Task.Delay(6000);
        return new HelloReply
        {
            Message = "Hello " + request.Name
        };
    }
}
