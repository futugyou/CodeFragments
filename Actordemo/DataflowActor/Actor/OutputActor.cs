using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowActor
{
    public class OutputActor : AbstractActor
    {
        public override void Handle(IMessage message)
        {
            if (message is BalanceMessage)
            {
                Console.WriteLine("Balance is {0}", (message as BalanceMessage).Amount);
            }
        }
    }
}