// See https://aka.ms/new-console-template for more information

using DeadDelayLetter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!");
await NomalDeadLetter.Consumer();
await NomalDeadLetter.SendMessage();
Console.WriteLine("-------------------------------------------");
await DelayDeadLetter.Consumer();
await DelayDeadLetter.SendMessage();
Console.WriteLine("-------------------------------------------");
await DelayDeadLetter.Consumer();
await DelayDeadLetterForDeffTime.SendMessage();
await DelayDeadLetterForDeffTime.DelayConsumer();
Console.ReadLine();

