using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix.Message
{
	/// <summary>
	/// Message
	/// </summary>
	/// <remarks>
	/// 2023.12.22: 创建. 模板插件(V1.3.7781.22279(build:20210421)) 模板版本(v1.0.1.1)<br/>
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
		public string SecurityType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SecurityType)); }
			set { this.Put(MessageDictionary.Fields.SecurityType, value); }
		}
		public string CCDCValuation
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CCDCValuation)); }
			set { this.Put(MessageDictionary.Fields.CCDCValuation, value); }
		}
		public string CCDCValuation2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CCDCValuation2)); }
			set { this.Put(MessageDictionary.Fields.CCDCValuation2, value); }
		}
		public string CCDCValuaDate
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CCDCValuaDate)); }
			set { this.Put(MessageDictionary.Fields.CCDCValuaDate, value); }
		}
		public string CCDCValuaType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CCDCValuaType)); }
			set { this.Put(MessageDictionary.Fields.CCDCValuaType, value); }
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
		public string OpenPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OpenPrice)); }
			set { this.Put(MessageDictionary.Fields.OpenPrice, value); }
		}
		public string HighPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HighPrice)); }
			set { this.Put(MessageDictionary.Fields.HighPrice, value); }
		}
		public string LowPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LowPrice)); }
			set { this.Put(MessageDictionary.Fields.LowPrice, value); }
		}
		public string LastPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastPrice)); }
			set { this.Put(MessageDictionary.Fields.LastPrice, value); }
		}
		public string PreClosePrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PreClosePrice)); }
			set { this.Put(MessageDictionary.Fields.PreClosePrice, value); }
		}
		public string SettlePrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SettlePrice)); }
			set { this.Put(MessageDictionary.Fields.SettlePrice, value); }
		}
		public string PreSettlePrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PreSettlePrice)); }
			set { this.Put(MessageDictionary.Fields.PreSettlePrice, value); }
		}
		public string OpenInterest
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OpenInterest)); }
			set { this.Put(MessageDictionary.Fields.OpenInterest, value); }
		}
		public string PreOpenInterest
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PreOpenInterest)); }
			set { this.Put(MessageDictionary.Fields.PreOpenInterest, value); }
		}
		public string Turnover
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Turnover)); }
			set { this.Put(MessageDictionary.Fields.Turnover, value); }
		}
		public string Volume
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Volume)); }
			set { this.Put(MessageDictionary.Fields.Volume, value); }
		}
		public string UpperLimitPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.UpperLimitPrice)); }
			set { this.Put(MessageDictionary.Fields.UpperLimitPrice, value); }
		}
		public string LowerLimitPrice
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LowerLimitPrice)); }
			set { this.Put(MessageDictionary.Fields.LowerLimitPrice, value); }
		}
		public string TradingPhaseCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradingPhaseCode)); }
			set { this.Put(MessageDictionary.Fields.TradingPhaseCode, value); }
		}
		public string Sign
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Sign)); }
			set { this.Put(MessageDictionary.Fields.Sign, value); }
		}
		public string Sign1
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Sign1)); }
			set { this.Put(MessageDictionary.Fields.Sign1, value); }
		}
		public string Sign2
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Sign2)); }
			set { this.Put(MessageDictionary.Fields.Sign2, value); }
		}
		public string Sign3
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Sign3)); }
			set { this.Put(MessageDictionary.Fields.Sign3, value); }
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
		public string TradingType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.TradingType)); }
			set { this.Put(MessageDictionary.Fields.TradingType, value); }
		}
		public string LastWorkDay
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.LastWorkDay)); }
			set { this.Put(MessageDictionary.Fields.LastWorkDay, value); }
		}


		public string BondType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.BondType)); }
			set { this.Put(MessageDictionary.Fields.BondType, value); }
		}

		public List<GNoMDEntries> NoMDEntries;


	}
	public class StandardHeader
	{
		public string MsgType = "W";
	}

	public class StandardTrailer
	{

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
		public string YieldToExecution
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.YieldToExecution)); }
			set { this.Put(MessageDictionary.Fields.YieldToExecution, value); }
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
		public string ExerciseFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExerciseFlag)); }
			set { this.Put(MessageDictionary.Fields.ExerciseFlag, value); }
		}
		public string PriceType
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PriceType)); }
			set { this.Put(MessageDictionary.Fields.PriceType, value); }
		}
		public string SSDetect
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.SSDetect)); }
			set { this.Put(MessageDictionary.Fields.SSDetect, value); }
		}
		public string CalFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.CalFlag)); }
			set { this.Put(MessageDictionary.Fields.CalFlag, value); }
		}
		public string MacaulayDuration
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.MacaulayDuration)); }
			set { this.Put(MessageDictionary.Fields.MacaulayDuration, value); }
		}
		public string ModifiedDuration
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ModifiedDuration)); }
			set { this.Put(MessageDictionary.Fields.ModifiedDuration, value); }
		}
		public string Pvbp
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Pvbp)); }
			set { this.Put(MessageDictionary.Fields.Pvbp, value); }
		}
		public string Convexity
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Convexity)); }
			set { this.Put(MessageDictionary.Fields.Convexity, value); }
		}
		public string OnLine
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.OnLine)); }
			set { this.Put(MessageDictionary.Fields.OnLine, value); }
		}
		public string FanDian
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FanDian)); }
			set { this.Put(MessageDictionary.Fields.FanDian, value); }
		}
		public string FanDianFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.FanDianFlag)); }
			set { this.Put(MessageDictionary.Fields.FanDianFlag, value); }
		}
		public string PartDealFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.PartDealFlag)); }
			set { this.Put(MessageDictionary.Fields.PartDealFlag, value); }
		}
		public string ExchangeSignFlag
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.ExchangeSignFlag)); }
			set { this.Put(MessageDictionary.Fields.ExchangeSignFlag, value); }
		}
		public string HandlInst
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.HandlInst)); }
			set { this.Put(MessageDictionary.Fields.HandlInst, value); }
		}
		public string VipCode
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.VipCode)); }
			set { this.Put(MessageDictionary.Fields.VipCode, value); }
		}

		public string Divided
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Divided)); }
			set { this.Put(MessageDictionary.Fields.Divided, value); }
		}

		public string Info6
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info6)); }
			set { this.Put(MessageDictionary.Fields.Info6, value); }
		}
		public string Info7
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info7)); }
			set { this.Put(MessageDictionary.Fields.Info7, value); }
		}
		public string Info8
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info8)); }
			set { this.Put(MessageDictionary.Fields.Info8, value); }
		}

		public string Info9
		{
			get { return DataTypeConverter.ToString(this.Get(MessageDictionary.Fields.Info9)); }
			set { this.Put(MessageDictionary.Fields.Info9, value); }
		}

	}
}
