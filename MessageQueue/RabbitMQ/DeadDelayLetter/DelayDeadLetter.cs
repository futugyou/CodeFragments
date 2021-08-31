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
        public static void SendMessage()
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
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            //创建死信交换机
            channel.ExchangeDeclare(dlxexChange, type: ExchangeType.Direct, durable: true, autoDelete: false);
            //创建死信队列
            channel.QueueDeclare(dlxQueueName, durable: true, exclusive: false, autoDelete: false);
            //死信队列绑定死信交换机
            channel.QueueBind(dlxQueueName, dlxexChange, routingKey: dlxQueueName);

            // 创建消息交换机
            channel.ExchangeDeclare(exchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
            //创建消息队列,并指定死信队列，和设置这个队列的消息过期时间为10s
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments:
                                new Dictionary<string, object> {
                                             { "x-dead-letter-exchange",dlxexChange}, //设置当前队列的DLX(死信交换机)
                                             { "x-dead-letter-routing-key",dlxQueueName}, //设置DLX的路由key，DLX会根据该值去找到死信消息存放的队列
                                             { "x-message-ttl",10000} //设置队列的消息过期时间
                                });
            //消息队列绑定消息交换机
            channel.QueueBind(queueName, exchange, routingKey: queueName);

            string message = "hello rabbitmq message";
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            //发布消息
            channel.BasicPublish(exchange: exchange,
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(message));
            Console.WriteLine($"{DateTime.Now},向队列:{queueName}发送消息:{message}");
        }


        public static void Consumer()
        {
            //死信交换机
            string dlxexChange = "dlx.exchange";
            //死信队列
            string dlxQueueName = "dlx.queue";
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "password";
            factory.HostName = "node01";
            using var connection = factory.CreateConnection();
            //创建信道
            var channel = connection.CreateModel();
            {
                //创建死信交换机
                channel.ExchangeDeclare(dlxexChange, type: ExchangeType.Direct, durable: true, autoDelete: false);
                //创建死信队列
                channel.QueueDeclare(dlxQueueName, durable: true, exclusive: false, autoDelete: false);
                //死信队列绑定死信交换机
                channel.QueueBind(dlxQueueName, dlxexChange, routingKey: dlxQueueName);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: true);
                consumer.Received += (model, ea) =>
                {
                        //处理业务
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"{DateTime.Now}，队列{dlxQueueName}消费消息:{message}");
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(dlxQueueName, autoAck: false, consumer);
            }
        }
    }
}
