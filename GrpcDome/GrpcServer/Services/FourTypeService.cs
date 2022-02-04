using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services;

public class FourTypeService : FourType.FourTypeBase
{
    private readonly ILogger<FourTypeService> logger;
    public FourTypeService(ILogger<FourTypeService> logger)
    {
        this.logger = logger;
    }
    public override Task<FourTypeResponse> UnaryCall(FourTypeRequest request, ServerCallContext context)
    {
        var userAgent = context.RequestHeaders.GetValue("user-agent");
        logger.LogInformation("UnaryCall: user-agent is " + userAgent);
        var response = new FourTypeResponse();
        return Task.FromResult(response);
    }
    public override async Task StreamingFromServer(FourTypeRequest request, IServerStreamWriter<FourTypeResponse> responseStream, ServerCallContext context)
    {
        for (var i = 0; i < 5; i++)
        {
            var t = i;
            await responseStream.WriteAsync(new FourTypeResponse() { Seq = t });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public override async Task<FourTypeResponse> StreamingFromClient(IAsyncStreamReader<FourTypeRequest> requestStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var message = requestStream.Current;
            logger.LogInformation("StreamingFromClient: message is " + message);
        }
        return new FourTypeResponse() { Seq = 999 };
    }

    public override async Task StreamingBothWays(IAsyncStreamReader<FourTypeRequest> requestStream, IServerStreamWriter<FourTypeResponse> responseStream, ServerCallContext context)
    {
        var index = 10;
        await foreach (var message in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new FourTypeResponse() { Seq = index });
            index++;
        }
    }
}