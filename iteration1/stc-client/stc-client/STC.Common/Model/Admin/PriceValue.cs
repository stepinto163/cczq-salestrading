
namespace STC.Common.Model.Admin
{
    public class PriceValue
    {
        public string MDReqId{get; set;}
        public decimal Value{get; set;}
        public string Id{get; set;}
        public string VenueGW{get; set;}
        public string Venue{get; set;}
        public decimal Qty{get; set;}
        public string Date{get; set;}
        public string Time{get; set;}
        private decimal lQty {get; set;}
        public string SettlType{get; set;}
        public string ClearingMethod{get; set;}
        public string DeliveryType{get; set;}
        public string QuoteTransType{get; set;}
        public string PartySubID{get; set;}

        public string PartySubName { get; set; }

        public int SN{get; set;}
        
        private decimal eQty {get; set;}
        public decimal Yield{get; set;}
        public decimal NetPrice{get; set;}
        public string CounterPartySubID{get; set;}
        public string BargainFlag{get; set;}
        public string OCOFlag{get; set;}
        public bool IsFirmFlag{get; set;}
        public string InternalFlag{get; set;}
        public string CounterPartySubName{get; set;}
        public bool isForbit { get; set; }

        public string WholeFlag { get; set; }
        public string Securityid { get; set; }
        public string Side { get; set; }
        public string Price { get; set; }
        public string LastQty { get; set; }
    }
}
