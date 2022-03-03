using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services;

public class MaskFieldService : FieldMaskService.FieldMaskServiceBase
{
    public override Task<FieldMaskResponse> UnaryCall(FieldMaskRequest request, ServerCallContext context)
    {
        var like = new List<string> { "eat" };
        var reply = new FieldMaskResponse
        {
            Seq = 1,
            Replay = "ok",
        };
        reply.Like.Add(like);

        var mergedReply = new FieldMaskResponse();
        request.FieldMask.Merge(reply, mergedReply);
        return Task.FromResult(mergedReply);
    }
}