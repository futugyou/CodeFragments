using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.AspNetCore.Subscriptions.Protocols;
using HotChocolate.Execution;

namespace AspnetcoreEx.GraphQL;

public class SocketSessionInterceptor : DefaultSocketSessionInterceptor
{
    private readonly ILogger<SocketSessionInterceptor> logger;
    public SocketSessionInterceptor(ILogger<SocketSessionInterceptor> logger)
    {
        this.logger = logger;

    }
    public override ValueTask<ConnectionStatus> OnConnectAsync(ISocketSession session, IOperationMessagePayload connectionInitMessage, CancellationToken cancellationToken = default)
    {
        // logger.LogInformation("this log from SocketSessionInterceptor.OnConnectAsync, message is :" + session.Payload);
        return base.OnConnectAsync(session, connectionInitMessage, cancellationToken);
    }

    public override ValueTask OnRequestAsync(ISocketSession session, string operationSessionId, OperationRequestBuilder requestBuilder, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("this log from SocketSessionInterceptor.OnRequestAsync");
        return base.OnRequestAsync(session, operationSessionId, requestBuilder, cancellationToken);
    }

    public override ValueTask OnCloseAsync(ISocketSession session, CancellationToken cancellationToken)
    {
        logger.LogInformation("this log from SocketSessionInterceptor.OnCloseAsync");
        return base.OnCloseAsync(session, cancellationToken);
    }
}
