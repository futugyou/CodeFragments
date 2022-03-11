using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Execution;

namespace AspnetcoreEx.GraphQL;

public class SocketSessionInterceptor : DefaultSocketSessionInterceptor
{
    private readonly ILogger<SocketSessionInterceptor> logger;
    public SocketSessionInterceptor(ILogger<SocketSessionInterceptor> logger)
    {
        this.logger = logger;

    }
    public override ValueTask<ConnectionStatus> OnConnectAsync(ISocketConnection connection, InitializeConnectionMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("this log from SocketSessionInterceptor.OnConnectAsync, message is :" + message.Payload);
        return base.OnConnectAsync(connection, message, cancellationToken);
    }

    public override ValueTask OnRequestAsync(ISocketConnection connection, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        logger.LogInformation("this log from SocketSessionInterceptor.OnRequestAsync");
        return base.OnRequestAsync(connection, requestBuilder, cancellationToken);
    }

    public override ValueTask OnCloseAsync(ISocketConnection connection, CancellationToken cancellationToken)
    {
        logger.LogInformation("this log from SocketSessionInterceptor.OnCloseAsync");
        return base.OnCloseAsync(connection, cancellationToken);
    }
}
