using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    public class ExecutionReportReq
    {
        public string BookID { get; set; }
        public string ExecType { get; set; }
        public string FirmFlag { get; set; }
        public string LastPx { get; set; }
        public string NetPrice { get; set; }
        public string LastQty { get; set; }
        public string SecurityID { get; set; }
        public string OrdRejReason { get; set; }
        public string OrdStatus { get; set; }
        public string OrdType { get; set; }
        public string CounterPartyVenues { get; set; }
        public string PartySubID { get; set; }
        public string PartySubName { get; set; }
        public string SettlDate { get; set; }
        public string SettlType { get; set; }
        public string Side { get; set; }
        public string TradeDate { get; set; }
        public string VenueTypeGW { get; set; }
        public string Venues { get; set; }
        public string Info1 { get; set; }

        public string Symbol { get; set; }
        public string SecurityType { get; set; }

        public string MarketIndicator { get; set; }

        public string CounterPartyName { get; set; }

        public string CounterPartySubName { get; set; }
        public string Remark { get; set; }

        public string Info10 { get; set; }

        public string OffsetFlag { get; set; }
        public string Yield { get; set; }
    }
}
