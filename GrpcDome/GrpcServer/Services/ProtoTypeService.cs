using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcServer.Services;

public class ProtoTypeService : ProtobufType.ProtobufTypeBase
{
    private readonly ILogger<ProtoTypeService> _logger;
    public ProtoTypeService(ILogger<ProtoTypeService> logger)
    {
        _logger = logger;
    }
    public override async Task<ProtobufTypeResponse> BaseCall(ProtobufTypeRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"{request.Subject}-{request.Start}-{request.Duration}");
        var meetingTime = DateTime.Now;
        TimeSpan meetingLength = new TimeSpan(2, 0, 0);
        var data = await File.ReadAllBytesAsync("./appsettings.json");
        var dic = new Dictionary<string, string>
        {
            ["key"] = "value"
        };
        var roles = new List<string> { "top" };
        var response = new ProtobufTypeResponse
        {
            Start = Timestamp.FromDateTimeOffset(meetingTime),
            Duration = Duration.FromTimeSpan(meetingLength),
            Age = null,
            Files = ByteString.CopyFrom(data),
        };
        response.Roles.Add(roles);
        response.Attributes.Add(dic);
        return response;
    }
}
