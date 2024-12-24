using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeadDelayLetter
{
    public class DelayDeadLetterForDeffTime
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
                                                                                          //{ "x-message-ttl",10000} //设置队列的消息过期时间
                                    });
            //消息队列绑定消息交换机
            await channel.QueueBindAsync(queueName, exchange, routingKey: queueName);

            string message = "hello rabbitmq message 10s后处理";
            var properties = new BasicProperties();
            properties.Persistent = true;
            properties.Expiration = "10000";//消息的有效期10s

            //发布消息,延时10s
            await channel.BasicPublishAsync(exchange: exchange,
                                      routingKey: queueName,
                                      mandatory: true,
                                      basicProperties: properties,
                                      body: Encoding.UTF8.GetBytes(message));
            Console.WriteLine($"{DateTime.Now},向队列:{queueName}发送消息:{message},延时:10s");



            string message2 = "hello rabbitmq message 5s后处理";
            var properties2 = new BasicProperties();
            properties2.Persistent = true;
            properties2.Expiration = "5000";//消息有效期5s

            //发布消息,延时5s
            await channel.BasicPublishAsync(exchange: exchange,
                                      routingKey: queueName,
                                      mandatory: true,
                                      basicProperties: properties2,
                                      body: Encoding.UTF8.GetBytes(message2));
            Console.WriteLine($"{DateTime.Now},向队列:{queueName}发送消息:{message2},延时:5s");
        }


        public static async Task DelayConsumer()
        {
            //延时队列
            string queueName = "delay_queue";
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "password";
            factory.HostName = "node01";
            using var connection = await factory.CreateConnectionAsync();
            //创建信道
            var channel = await connection.CreateChannelAsync();
            {
                var consumer = new AsyncEventingBasicConsumer(channel);
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: true);
                consumer.ReceivedAsync += async (model, ea) =>
              {
                  //处理业务
                  var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                  Thread.Sleep(20);//消息少的时候可以加个睡眠时间减少IO
                  await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
              };
                await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);

            }
        }
    }
}
