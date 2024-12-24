using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDelayLetter
{
    public class DelayDeadLetter
    {
        public static async Task SendMessage()
        {
            //死信交换机
            string dlxexChange = "dlx.exchange";
            //死信队列
            string dlxQueueName = "dlx.queue";

            //消息交换机
            string exchange = "direct-exchange";
            //消息队列
            string queueName = "delay_queue";

            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "password";
            factory.HostName = "node01";
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            //创建死信交换机
            await channel.ExchangeDeclareAsync(dlxexChange, type: ExchangeType.Direct, durable: true, autoDelete: false);
            //创建死信队列
            await channel.QueueDeclareAsync(dlxQueueName, durable: true, exclusive: false, autoDelete: false);
            //死信队列绑定死信交换机
            await channel.QueueBindAsync(dlxQueueName, dlxexChange, routingKey: dlxQueueName);

            // 创建消息交换机
            await channel.ExchangeDeclareAsync(exchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
            //创建消息队列,并指定死信队列，和设置这个队列的消息过期时间为10s
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments:
                                new Dictionary<string, object?> {
                                             { "x-dead-letter-exchange",dlxexChange}, //设置当前队列的DLX(死信交换机)
                                             { "x-dead-letter-routing-key",dlxQueueName}, //设置DLX的路由key，DLX会根据该值去找到死信消息存放的队列
                                             { "x-message-ttl",10000} //设置队列的消息过期时间
                                });
            //消息队列绑定消息交换机
            await channel.QueueBindAsync(queueName, exchange, routingKey: queueName);

            string message = "hello rabbitmq message";
            var properties = new BasicProperties();
            properties.Persistent = true;
            //发布消息
            await channel.BasicPublishAsync(exchange: exchange,
                                 routingKey: queueName,
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(message));
            Console.WriteLine($"{DateTime.Now},向队列:{queueName}发送消息:{message}");
        }


        public static async Task Consumer()
        {
            //死信交换机
            string dlxexChange = "dlx.exchange";
            //死信队列
            string dlxQueueName = "dlx.queue";
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "password";
            factory.HostName = "node01";
            using var connection = await factory.CreateConnectionAsync();
            //创建信道
            var channel = await connection.CreateChannelAsync();
            {
                //创建死信交换机
                await channel.ExchangeDeclareAsync(dlxexChange, type: ExchangeType.Direct, durable: true, autoDelete: false);
                //创建死信队列
                await channel.QueueDeclareAsync(dlxQueueName, durable: true, exclusive: false, autoDelete: false);
                //死信队列绑定死信交换机
                await channel.QueueBindAsync(dlxQueueName, dlxexChange, routingKey: dlxQueueName);

                var consumer = new AsyncEventingBasicConsumer(channel);
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: true);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    //处理业务
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"{DateTime.Now}，队列{dlxQueueName}消费消息:{message}");
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                };
                await channel.BasicConsumeAsync(dlxQueueName, autoAck: false, consumer);
            }
        }
    }
}
