using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class SettlementPo
    {
        public string Settle { get; set; }
        public string TradeDate { get; set; }
        public string SettlementDate { get; set; }
        public bool IsBid { get; set; } = false;

        public string BidSettle { get; set; }
        public string BidTradeDate { get; set; }
        public string BidSettlementDate { get; set; }

        public string Remarks { get; set; }

        public string SettlDate { get; set; }
        public string SettlDate2 { get; set; }
        public string SettlType { get; set; }
        public string SettlType2 { get; set; }

        public string TradeDate2 { get; set; }

        public string GetSettle(string ofrSettle, string bidSettle)
        {
            if (bidSettle.Equals(ofrSettle))
                return bidSettle;
            else
                return string.Format("{0}/{1}", ofrSettle, bidSettle);
        }

        public bool IsSpot
        {
            get
            {
                string[] arr = Settle.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                return arr.Contains("+0") || arr.Contains("+1") || arr.Contains("明天+0");
            }
        }

        public string SettleDisplay
        {
            get
            {
                string settleDisplay = Settle;
                switch (Settle)
                {
                    case "+0":
                        settleDisplay = "1";
                        break;
                    case "+1":
                        settleDisplay = "2";
                        break;
                    case "明天+0":
                        settleDisplay = "3";
                        break;
                }
                return settleDisplay;
            }
        }


        public string BidSettleDisplay
        {
            get
            {
                string settleDisplay = BidSettle;
                switch (BidSettle)
                {
                    case "+0":
                        settleDisplay = "1";
                        break;
                    case "+1":
                        settleDisplay = "2";
                        break;
                    case "明天+0":
                        settleDisplay = "3";
                        break;
                }
                return settleDisplay;
            }
        }
    }
}
