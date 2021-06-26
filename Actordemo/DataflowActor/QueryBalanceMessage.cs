using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataflowActor
{
    public class QueryBalanceMessage : IMessage
    {
        public IActor Receiver { get; set; }
    }
}
