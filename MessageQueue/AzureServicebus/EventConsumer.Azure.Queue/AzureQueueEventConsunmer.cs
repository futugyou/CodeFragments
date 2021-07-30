using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace MessageConsumer.Azure.Queue
{
    public class AzureQueueEventConsunmer : IEventConsunmer
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<AzureQueueEventConsunmer> _logger;
        private readonly IEventHandler _eventHandler;
        private readonly AzureQueueOptions _options;
        private ServiceBusProcessor _processor;

        private readonly CloudEventFormatter _formatter = new JsonEventFormatter();

        public AzureQueueEventConsunmer(ILogger<AzureQueueEventConsunmer> logger, IOptionsMonitor<AzureQueueOptions> options, IEventHandler eventHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventHandler = eventHandler;
            _options = options.CurrentValue;
            _client = new ServiceBusClient(_options.ConnectionString);
        }

        public async Task CloseAsync()
        {
            await _processor.CloseAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
            }

            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task PrepareAsync()
        {
            ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
            };

            _processor = _client.CreateProcessor(_options.QueueName, _serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync().ConfigureAwait(false);
        }

        #region private
        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var @event = args.Message.ToCloudEvent(_formatter);
            await _eventHandler.HandlerEvent(@event).ConfigureAwait(false);
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }
        #endregion
    }
}
