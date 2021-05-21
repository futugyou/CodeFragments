using Mastercard.Model.v60;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mastercard.ViewModel
{
    public class CreateSessionViewModel
    { 
        public string Merchant { get; set; } 
        public Session Session { get; set; } 
        public string SuccessIndicator { get; set; }
        public string OrderId { get; set; } 
        public string Currency { get; set; }  
    }
}
