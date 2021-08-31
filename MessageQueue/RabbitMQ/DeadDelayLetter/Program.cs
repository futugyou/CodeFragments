// See https://aka.ms/new-console-template for more information

using DeadDelayLetter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!");
NomalDeadLetter.Consumer();
NomalDeadLetter.SendMessage();
Console.WriteLine("-------------------------------------------");
DelayDeadLetter.Consumer();
DelayDeadLetter.SendMessage();
Console.WriteLine("-------------------------------------------");
DelayDeadLetter.Consumer();
DelayDeadLetterForDeffTime.SendMessage();
DelayDeadLetterForDeffTime.DelayConsumer();
Console.ReadLine();

