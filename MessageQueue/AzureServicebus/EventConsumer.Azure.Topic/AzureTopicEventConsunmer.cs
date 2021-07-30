using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using EventConsumer.Azure.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EventConsumer.Azure.Topic
{
    public class AzureTopicEventConsunmer : IEventConsunmer
    {
        private readonly ILogger<AzureTopicEventConsunmer> _logger;
        private readonly AzureTopicOptions _options;
        private readonly IEventHandler _eventHandler;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;

        private ServiceBusProcessor _processor;
        private readonly CloudEventFormatter _formatter = new JsonEventFormatter();

        public AzureTopicEventConsunmer(ILogger<AzureTopicEventConsunmer> logger, IOptionsMonitor<AzureTopicOptions> options, IEventHandler eventHandler)
        {
            _logger = logger;
            _options = options.CurrentValue;
            _eventHandler = eventHandler;

            _client = new ServiceBusClient(_options.ConnectionString);
            _adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);
        }

        public async Task CloseAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
            }
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
            var exist = await _adminClient.SubscriptionExistsAsync(_options.TopicName, _options.SubscriptionName);
            if (!exist)
            {
                await _adminClient.CreateSubscriptionAsync(_options.TopicName, _options.SubscriptionName);
            }
            _processor = _client.CreateProcessor(_options.TopicName, _options.SubscriptionName, _serviceBusProcessorOptions);
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
