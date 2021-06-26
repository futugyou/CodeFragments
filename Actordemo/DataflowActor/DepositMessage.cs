using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowActor
{
    public class DepositMessage : IMessage
    {
        public decimal Amount { get; set; }
    }
}
