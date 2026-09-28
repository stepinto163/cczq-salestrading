using STC.Common.Model.Fix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cust
{
    public class BrokerMarketDataSnapshot
    {
        public string CCDCValuaDate { get; set; }
        public string CCDCValuaType { get; set; }
        public string CCDCValuation { get; set; }
        public string ClOrdID { get; set; }

        public string LastPrice { get; set; }

        public string Info1 { get; set; }
        public string LastWorkDay { get; set; }
        public string MDBookType { get; set; }
        public string MarketIndicator { get; set; }
        

        public string SecurityID { get; set; }
        public string SecurityType { get; set; }
        public string SendingTime { get; set; }
        public string Sign { get; set; }
        public string Sign1 { get; set; }
        public string Sign2 { get; set; }
        public string Sign3 { get; set; }
        public string Symbol { get; set; }
        public string VenueTypeGW { get; set; }

        public List<MDEntries> NoMDEntries { get; set; }
    }

    public class NoMDEntryCollection
    {
        public List<MDEntries> datas;
    }

    public class MDEntries
    {
        public string ClearingMethod { get; set; }
        public string DeliveryType { get; set; }
        public string FirmFlag { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Info4 { get; set; }
        public string InternalFlag { get; set; }
        public string MDEntryDate { get; set; }
        public string MDEntryID { get; set; }
        public string MDEntryPx { get; set; }
        public string MDEntrySize { get; set; }
        public string MDEntryTime { get; set; }
        public string MDEntryType { get; set; }
        public string MDPriceLevel { get; set; }
        public string MDReqID { get; set; }
        public string NetPrice { get; set; }
        public string PartyID { get; set; }
        public string PartyName { get; set; }
        public string PartySubID { get; set; }
        public string QuoteStatus { get; set; }
        public string SettlType { get; set; }
        public string TradeVolume { get; set; }
        public string VenueTypeGW { get; set; }
        public string Venues { get; set; }
        public string Yield { get; set; }

        public string Text { get; set; }
    }
}
