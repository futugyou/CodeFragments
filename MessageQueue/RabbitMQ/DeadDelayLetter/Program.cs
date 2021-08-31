// See https://aka.ms/new-console-template for more information

using DeadDelayLetter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!"); 
NomalDeadLetter.Consumer();
NomalDeadLetter.SendMessage(); 
Console.ReadLine();

