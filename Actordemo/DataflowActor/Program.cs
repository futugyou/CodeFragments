using System;

namespace DataflowActor
{
    class Program
    {
        static void Main(string[] args)
        {
            var account = new AccountActor();
            var output = new OutputActor();

            account.Send(new DepositMessage { Amount = 50 });
            account.Send(new QueryBalanceMessage { Receiver = output });

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
