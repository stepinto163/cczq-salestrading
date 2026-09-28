using System;
using System.Collections.Generic;
using System.Text;

namespace STC.dps.plugin.Model.Fix.Message
{
    /// <summary>
    /// Message
    /// </summary>
    /// <remarks>
    /// 2020.09.11: 创建. 模板插件(V1.3.7488.16761(build:20200702)) 模板版本(v1.0.1.1)<br/>
    /// </remarks>
    public class MarketDataSnapshotFullRefresh : FixMessage
    {
        public MarketDataSnapshotFullRefresh()
        {
            StandardHeader.MsgType = "W";
        }
        public string VenueTypeGW
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenueTypeGW)); }
            set { this.Put(MessageDictionary.Fields.VenueTypeGW, value); }
        }
        public string Venues
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Venues)); }
            set { this.Put(MessageDictionary.Fields.Venues, value); }
        }
        public string MDBookType
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDBookType)); }
            set { this.Put(MessageDictionary.Fields.MDBookType, value); }
        }
        public string MDReqID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
            set { this.Put(MessageDictionary.Fields.MDReqID, value); }
        }
        public string ClOrdID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClOrdID)); }
            set { this.Put(MessageDictionary.Fields.ClOrdID, value); }
        }
        public string MarketIndicator
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MarketIndicator)); }
            set { this.Put(MessageDictionary.Fields.MarketIndicator, value); }
        }
        public string SecurityID
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SecurityID)); }
            set { this.Put(MessageDictionary.Fields.SecurityID, value); }
        }
        public string Symbol
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Symbol)); }
            set { this.Put(MessageDictionary.Fields.Symbol, value); }
        }
        public string CCDCValuation
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CCDCValuation)); }
            set { this.Put(MessageDictionary.Fields.CCDCValuation, value); }
        }
        public string SendingTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SendingTime)); }
            set { this.Put(MessageDictionary.Fields.SendingTime, value); }
        }
        public string LocalTime
        {
            get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LocalTime)); }
            set { this.Put(MessageDictionary.Fields.LocalTime, value); }
        }

        private readonly string GROUP_NoMDEntries = MessageDictionary.Fields.NoMDEntries;
        public List<GNoMDEntries> NoMDEntries
        {
            get { return this.Get(GROUP_NoMDEntries) as List<GNoMDEntries>; }
            set { this.Put(GROUP_NoMDEntries, value); }
        }
        public class GNoMDEntries : BaseMessage
        {
            public string QuoteTransType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteTransType)); }
                set { this.Put(MessageDictionary.Fields.QuoteTransType, value); }
            }
            public string QuoteStatus
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuoteStatus)); }
                set { this.Put(MessageDictionary.Fields.QuoteStatus, value); }
            }
            public string FirmFlag
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FirmFlag)); }
                set { this.Put(MessageDictionary.Fields.FirmFlag, value); }
            }
            public string MDEntryType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntryType)); }
                set { this.Put(MessageDictionary.Fields.MDEntryType, value); }
            }
            public string MDPriceLevel
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDPriceLevel)); }
                set { this.Put(MessageDictionary.Fields.MDPriceLevel, value); }
            }
            public string MDEntryID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntryID)); }
                set { this.Put(MessageDictionary.Fields.MDEntryID, value); }
            }
            public string MDReqID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDReqID)); }
                set { this.Put(MessageDictionary.Fields.MDReqID, value); }
            }
            public string QuantID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.QuantID)); }
                set { this.Put(MessageDictionary.Fields.QuantID, value); }
            }
            public string VenueTypeGW
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VenueTypeGW)); }
                set { this.Put(MessageDictionary.Fields.VenueTypeGW, value); }
            }
            public string Venues
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Venues)); }
                set { this.Put(MessageDictionary.Fields.Venues, value); }
            }
            public string MDEntryDate
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntryDate)); }
                set { this.Put(MessageDictionary.Fields.MDEntryDate, value); }
            }
            public string MDEntryTime
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntryTime)); }
                set { this.Put(MessageDictionary.Fields.MDEntryTime, value); }
            }
            public string ValidUntilTime
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ValidUntilTime)); }
                set { this.Put(MessageDictionary.Fields.ValidUntilTime, value); }
            }
            public string NetPrice
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.NetPrice)); }
                set { this.Put(MessageDictionary.Fields.NetPrice, value); }
            }
            public string MDEntryPx
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntryPx)); }
                set { this.Put(MessageDictionary.Fields.MDEntryPx, value); }
            }
            public string Yield
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Yield)); }
                set { this.Put(MessageDictionary.Fields.Yield, value); }
            }
            public string DirtyPrice
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DirtyPrice)); }
                set { this.Put(MessageDictionary.Fields.DirtyPrice, value); }
            }
            public string UnMatchQty
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UnMatchQty)); }
                set { this.Put(MessageDictionary.Fields.UnMatchQty, value); }
            }
            public string MDEntrySize
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MDEntrySize)); }
                set { this.Put(MessageDictionary.Fields.MDEntrySize, value); }
            }
            public string TradeVolume
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradeVolume)); }
                set { this.Put(MessageDictionary.Fields.TradeVolume, value); }
            }
            public string SettlType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlType)); }
                set { this.Put(MessageDictionary.Fields.SettlType, value); }
            }
            public string OrigSettlType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OrigSettlType)); }
                set { this.Put(MessageDictionary.Fields.OrigSettlType, value); }
            }
            public string ClearingMethod
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ClearingMethod)); }
                set { this.Put(MessageDictionary.Fields.ClearingMethod, value); }
            }
            public string DeliveryType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.DeliveryType)); }
                set { this.Put(MessageDictionary.Fields.DeliveryType, value); }
            }
            public string PartyID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyID)); }
                set { this.Put(MessageDictionary.Fields.PartyID, value); }
            }
            public string PartyName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartyName)); }
                set { this.Put(MessageDictionary.Fields.PartyName, value); }
            }
            public string PartySubID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubID)); }
                set { this.Put(MessageDictionary.Fields.PartySubID, value); }
            }
            public string PartySubName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartySubName)); }
                set { this.Put(MessageDictionary.Fields.PartySubName, value); }
            }
            public string CounterPartyID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyID)); }
                set { this.Put(MessageDictionary.Fields.CounterPartyID, value); }
            }
            public string CounterPartyName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartyName)); }
                set { this.Put(MessageDictionary.Fields.CounterPartyName, value); }
            }
            public string CounterPartySubID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartySubID)); }
                set { this.Put(MessageDictionary.Fields.CounterPartySubID, value); }
            }
            public string CounterPartySubName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CounterPartySubName)); }
                set { this.Put(MessageDictionary.Fields.CounterPartySubName, value); }
            }
            public string InternalFlag
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InternalFlag)); }
                set { this.Put(MessageDictionary.Fields.InternalFlag, value); }
            }
            public string BargainFlag
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BargainFlag)); }
                set { this.Put(MessageDictionary.Fields.BargainFlag, value); }
            }
            public string OCOFlag
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OCOFlag)); }
                set { this.Put(MessageDictionary.Fields.OCOFlag, value); }
            }
            public string Text
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Text)); }
                set { this.Put(MessageDictionary.Fields.Text, value); }
            }
            public string UserID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UserID)); }
                set { this.Put(MessageDictionary.Fields.UserID, value); }
            }
            public string UserName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UserName)); }
                set { this.Put(MessageDictionary.Fields.UserName, value); }
            }
            public string BizType
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BizType)); }
                set { this.Put(MessageDictionary.Fields.BizType, value); }
            }
            public string WholeFlag
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.WholeFlag)); }
                set { this.Put(MessageDictionary.Fields.WholeFlag, value); }
            }
            public string InstitutionID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstitutionID)); }
                set { this.Put(MessageDictionary.Fields.InstitutionID, value); }
            }
            public string InstitutionName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.InstitutionName)); }
                set { this.Put(MessageDictionary.Fields.InstitutionName, value); }
            }
            public string TraderID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TraderID)); }
                set { this.Put(MessageDictionary.Fields.TraderID, value); }
            }
            public string TraderName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TraderName)); }
                set { this.Put(MessageDictionary.Fields.TraderName, value); }
            }
            public string ShowTrader
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ShowTrader)); }
                set { this.Put(MessageDictionary.Fields.ShowTrader, value); }
            }
            public string RoleName
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RoleName)); }
                set { this.Put(MessageDictionary.Fields.RoleName, value); }
            }
            public string RoleID
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.RoleID)); }
                set { this.Put(MessageDictionary.Fields.RoleID, value); }
            }
            public string Info1
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info1)); }
                set { this.Put(MessageDictionary.Fields.Info1, value); }
            }
            public string Info2
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info2)); }
                set { this.Put(MessageDictionary.Fields.Info2, value); }
            }
            public string Info3
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info3)); }
                set { this.Put(MessageDictionary.Fields.Info3, value); }
            }
            public string Info4
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info4)); }
                set { this.Put(MessageDictionary.Fields.Info4, value); }
            }
            public string Info5
            {
                get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info5)); }
                set { this.Put(MessageDictionary.Fields.Info5, value); }
            }
            public GNoMDEntries Clone()
            {
                GNoMDEntries t = new GNoMDEntries();
                t.QuoteTransType = QuoteTransType;
                t.QuoteStatus = QuoteStatus;
                t.FirmFlag = FirmFlag;
                t.MDEntryType = MDEntryType;
                t.MDPriceLevel = MDPriceLevel;
                t.MDEntryID = MDEntryID;
                t.MDReqID = MDReqID;
                t.QuantID = QuantID;
                t.VenueTypeGW = VenueTypeGW;
                t.Venues = Venues;
                t.MDEntryDate = MDEntryDate;
                t.MDEntryTime = MDEntryTime;
                t.ValidUntilTime = ValidUntilTime;
                t.NetPrice = NetPrice;
                t.MDEntryPx = MDEntryPx;
                t.Yield = Yield;
                t.DirtyPrice = DirtyPrice;
                t.UnMatchQty = UnMatchQty;
                t.MDEntrySize = MDEntrySize;
                t.TradeVolume = TradeVolume;
                t.SettlType = SettlType;
                t.OrigSettlType = OrigSettlType;
                t.ClearingMethod = ClearingMethod;
                t.DeliveryType = DeliveryType;
                t.PartyID = PartyID;
                t.PartyName = PartyName;
                t.PartySubID = PartySubID;
                t.PartySubName = PartySubName;
                t.CounterPartyID = CounterPartyID;
                t.CounterPartyName = CounterPartyName;
                t.CounterPartySubID = CounterPartySubID;
                t.CounterPartySubName = CounterPartySubName;
                t.InternalFlag = InternalFlag;
                t.BargainFlag = BargainFlag;
                t.OCOFlag = OCOFlag;
                t.Text = Text;
                t.UserID = UserID;
                t.UserName = UserName;
                t.BizType = BizType;
                t.WholeFlag = WholeFlag;
                t.InstitutionID = InstitutionID;
                t.InstitutionName = InstitutionName;
                t.TraderID = TraderID;
                t.TraderName = TraderName;
                t.ShowTrader = ShowTrader;
                t.RoleName = RoleName;
                t.RoleID = RoleID;
                t.Info1 = Info1;
                t.Info2 = Info2;
                t.Info3 = Info3;
                t.Info4 = Info4;
                t.Info5 = Info5;
                return t;
            }
        }

        public MarketDataSnapshotFullRefresh Clone()
        {
            MarketDataSnapshotFullRefresh t = new MarketDataSnapshotFullRefresh();
            t.StandardHeader = StandardHeader;
            t.StandardTrailer = StandardTrailer;
            t.VenueTypeGW = VenueTypeGW;
            t.Venues = Venues;
            t.MDBookType = MDBookType;
            t.MDReqID = MDReqID;
            t.ClOrdID = ClOrdID;
            t.MarketIndicator = MarketIndicator;
            t.SecurityID = SecurityID;
            t.Symbol = Symbol;
            t.CCDCValuation = CCDCValuation;
            t.SendingTime = SendingTime;
            t.LocalTime = LocalTime;
            t.NoMDEntries = new List<GNoMDEntries>();
            if (NoMDEntries != null)
            {
                foreach (var g in NoMDEntries)
                {
                    GNoMDEntries gNoMDEntries = g.Clone();
                    t.NoMDEntries.Add(gNoMDEntries);
                }
            }
            return t;
        }
    }
}

