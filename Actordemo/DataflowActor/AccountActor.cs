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
        public void Handle(DepositMessage message)
        {
            _balance += message.Amount;
        }

        public void Handle(QueryBalanceMessage message)
        {
            message.Receiver.Send(new BalanceMessage { Amount = _balance });
        }
    }
}
