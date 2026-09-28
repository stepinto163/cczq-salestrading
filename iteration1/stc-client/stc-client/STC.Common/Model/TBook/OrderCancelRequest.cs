using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class OrderCancelRequest
    {
        public string AlgoName;
        public string OrderID;
        public string ClOrdID;
        public string IntentionID;
        public string Rejecter;
        public string CxlRejReason;
        public string RejectText;
        
    }
}
