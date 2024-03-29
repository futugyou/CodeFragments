﻿namespace SignalR.Common;

public interface ISignalRPublisher
{
    Task SubscribeAsync(string topic);

    Task PublishAsync<T>(string topic, T payload);
}
