using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowActor
{
    public class OutputActor : AbstractActor
    {
        public void Handle(BalanceMessage message)
        {
            Console.WriteLine("Balance is {0}", message.Amount);
        }
    }
}
