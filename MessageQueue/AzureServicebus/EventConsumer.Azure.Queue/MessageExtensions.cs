using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using EventConsumer.Azure.Queue;
using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace MessageConsumer.Azure.Queue
{
    internal static class MessageExtensions
    {
        private static readonly string FAKE_SOURCE = "urn:example-com:mysource:abc";
        /// <summary>
        /// Indicates whether this <see cref="Message"/> holds a single CloudEvent.
        /// </summary>
        /// <remarks>
        /// This method returns false for batch requests, as they need to be parsed differently.
        /// </remarks>
        /// <param name="message">The message to check for the presence of a CloudEvent. Must not be null.</param>
        /// <returns>true, if the message is a CloudEvent.</returns>
        public static bool IsCloudEvent(this ServiceBusReceivedMessage message)
        {
            Validation.CheckNotNull(message, nameof(message));
            return HasCloudEventsContentType(message, out _) || message.ApplicationProperties.ContainsKey(Constants.SpecVersionPropertyKey);
        }

        /// <summary>
        /// Converts this <see cref="Message"/> into a CloudEvent object.
        /// </summary>
        /// <param name="message">The message to convert. Must not be null.</param>
        /// <param name="formatter">The event formatter to use to parse the CloudEvent. Must not be null.</param>
        /// <param name="extensionAttributes">The extension attributes to use when parsing the CloudEvent.</param>
        /// <returns>A validated CloudEvent.</returns>
        public static CloudEvent ToCloudEvent(this ServiceBusReceivedMessage message,
            CloudEventFormatter formatter,
            params CloudEventAttribute[] extensionAttributes
            )
            => message.ToCloudEvent(formatter, (IEnumerable<CloudEventAttribute>)extensionAttributes);

        /// <summary>
        /// Converts this <see cref="Message"/> into a CloudEvent object.
        /// </summary>
        /// <param name="message">The message to convert. Must not be null.</param>
        /// <param name="formatter">The event formatter to use to parse the CloudEvent. Must not be null.</param>
        /// <param name="extensionAttributes">The extension attributes to use when parsing the CloudEvent. May be null.</param>
        /// <returns>A validated CloudEvent.</returns>
        public static CloudEvent ToCloudEvent(
            this ServiceBusReceivedMessage message,
            CloudEventFormatter formatter,
            IEnumerable<CloudEventAttribute>? extensionAttributes
            )
        {
            Validation.CheckNotNull(message, nameof(message));
            Validation.CheckNotNull(formatter, nameof(formatter));
            if (HasCloudEventsContentType(message, out var contentType))
            {
                return StructuredToCloudEvent(message, contentType, formatter, extensionAttributes);
            }
            else
            {
                return BinaryToCloudEvent(message, formatter, extensionAttributes);
            }
        }

        private static CloudEvent StructuredToCloudEvent(
            ServiceBusReceivedMessage message,
            string contentType,
            CloudEventFormatter formatter,
            IEnumerable<CloudEventAttribute>? extensionAttributes
            )
        {
            using (var stream = message.Body.ToStream())
            {
                var cloudEvent = formatter.DecodeStructuredModeMessage(stream, new ContentType(contentType), extensionAttributes);
                cloudEvent.Id = message.MessageId;
                return cloudEvent;
            }
        }

        private static CloudEvent BinaryToCloudEvent(
            ServiceBusReceivedMessage message,
            CloudEventFormatter formatter,
            IEnumerable<CloudEventAttribute>? extensionAttributes
            )
        {
            var propertyMap = message.ApplicationProperties;
            if (!propertyMap.TryGetValue(Constants.SpecVersionPropertyKey, out var versionId))
            {
                throw new ArgumentException("Request is not a CloudEvent", nameof(message));
            }

            var version = CloudEventsSpecVersion.FromVersionId(versionId as string)
                ?? throw new ArgumentException($"Unknown CloudEvents spec version '{versionId}'", nameof(message));

            var cloudEvent = new CloudEvent(version, extensionAttributes)
            {
                Id = message.MessageId,
                DataContentType = message.ContentType,
                Type = message.Subject,
                Time = message.EnqueuedTime,
                Source = new Uri(FAKE_SOURCE),
            };

            foreach (var property in propertyMap)
            {
                if (!property.Key.StartsWith(Constants.PropertyKeyPrefix))
                {
                    continue;
                }

                var attributeName = property.Key.Substring(Constants.PropertyKeyPrefix.Length).ToLowerInvariant();

                // We've already dealt with the spec version.
                if (attributeName == CloudEventsSpecVersion.SpecVersionAttribute.Name)
                {
                    continue;
                }

                // Timestamps are serialized via DateTime instead of DateTimeOffset.
                if (property.Value is DateTime dt)
                {
                    if (dt.Kind != DateTimeKind.Utc)
                    {
                        // This should only happen for MinValue and MaxValue...
                        // just respecify as UTC. (We could add validation that it really
                        // *is* MinValue or MaxValue if we wanted to.)
                        dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }

                    cloudEvent[attributeName] = (DateTimeOffset)dt;
                }

                // URIs are serialized as strings, but we need to convert them back to URIs.
                // It's simplest to let CloudEvent do this for us.
                else if (property.Value is string text)
                {
                    cloudEvent.SetAttributeFromString(attributeName, text);
                }
                else
                {
                    cloudEvent[attributeName] = property.Value;
                }
            }

            formatter.DecodeBinaryModeEventData(message.Body, cloudEvent);

            Validation.CheckCloudEventArgument(cloudEvent, nameof(message));

            return cloudEvent;
        }

        private static bool HasCloudEventsContentType(ServiceBusReceivedMessage message, out string contentType)
        {
            contentType = message.ContentType;
            return MimeUtilities.IsCloudEventsContentType(contentType);
        }
    }
}