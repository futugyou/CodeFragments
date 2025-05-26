namespace Actors;

using System.Threading.Channels;
using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Dapr.Proto.Placement.V1;

public class PlacementStreamClient
{
    private readonly IActorTables _table;
    private readonly PlacementHealthState _healthReporter;
    private readonly ILogger _logger;
    private readonly Channel<Host> _sendChannel = Channel.CreateUnbounded<Host>();
    private readonly string[] _placementAddresses;
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(3);

    public PlacementStreamClient(
        string[] placementAddresses,
        IActorTables table,
        PlacementHealthState healthReporter,
        ILogger logger)
    {
        _placementAddresses = placementAddresses;
        _table = table;
        _healthReporter = healthReporter;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var currentAddressIndex = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var address = _placementAddresses[currentAddressIndex % _placementAddresses.Length];
            currentAddressIndex++;

            try
            {
                using var channel = GrpcChannel.ForAddress(address);
                var client = new Placement.PlacementClient(channel);
                using var stream = client.ReportDaprStatus(cancellationToken: cancellationToken);

                _healthReporter.ReportHealthy();
                _logger.LogInformation("Connected to placement at {Address}", address);

                var sendTask = SendLoopAsync(stream.RequestStream, cancellationToken);
                var recvTask = ReceiveLoopAsync(stream.ResponseStream, cancellationToken);

                await Task.WhenAny(sendTask, recvTask);
                _logger.LogWarning("Stream disconnected, retrying...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with placement service");
            }

            _healthReporter.ReportUnhealthy();
            await _table.HaltAllAsync(cancellationToken);

            try
            {
                await Task.Delay(_reconnectDelay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task SendLoopAsync(IClientStreamWriter<Host> requestStream, CancellationToken cancellationToken)
    {
        await foreach (var host in _sendChannel.Reader.ReadAllAsync(cancellationToken))
        {
            await requestStream.WriteAsync(host, cancellationToken);
        }
    }

    private async Task ReceiveLoopAsync(IAsyncStreamReader<PlacementOrder> responseStream, CancellationToken cancellationToken)
    {
        while (await responseStream.MoveNext(cancellationToken))
        {
            var order = responseStream.Current;
            await _table.UpdateAsync(order, cancellationToken);
        }
    }

    public async Task SendHostAsync(Host host, CancellationToken cancellationToken = default)
    {
        await _sendChannel.Writer.WriteAsync(host, cancellationToken);
    }
}
