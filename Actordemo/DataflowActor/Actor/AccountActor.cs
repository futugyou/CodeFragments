using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowActor
{
    public class AccountActor : AbstractActor
    {
        private decimal _balance; 

        public override void Handle(IMessage message)
        {
            if (message is QueryBalanceMessage)
            {
                (message as QueryBalanceMessage).Receiver.Send(new BalanceMessage { Amount = _balance });
            }
            if (message is DepositMessage)
            {
                _balance += (message as DepositMessage).Amount;
            }
        }
    }
}
