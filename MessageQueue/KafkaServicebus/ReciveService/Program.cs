using Confluent.Kafka;
using KafkaServiceBus;
using ReciveService;
using System;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IServiceBus, ServiceBus>();
        services.AddSingleton<IProducer<Null, string>>(x =>
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "192.168.0.6:9092"
            };
            var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            return producer;
        }
        );
        services.AddSingleton<IConsumer<Null, string>>(x =>
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "192.168.0.6:9092",
                GroupId = "test-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true,
            };
            var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            return consumer;
        }
        );
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
