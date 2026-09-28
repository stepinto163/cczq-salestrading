using Newtonsoft.Json;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model
{
    public class TransOrderServerData : ExecutionReport
    {
        public void CopyParentData(ExecutionReport parent)
        {
            // 如果还需要其他父类属性的复制，则按需添加相应的复制逻辑
            this.StandardHeader = parent.StandardHeader;
            this.StandardTrailer = parent.StandardTrailer;
            this.VenueTypeGW = parent.VenueTypeGW;
            this.Venues = parent.Venues;
            this.AlgoName = parent.AlgoName;
            this.StrategyOrderId = parent.StrategyOrderId;
            this.StrategyOrderVenues = parent.StrategyOrderVenues;
            this.MarketIndicator = parent.MarketIndicator;
            this.Terminal = parent.Terminal;
            this.OrdStatus = parent.OrdStatus;
            this.ClOrdID = parent.ClOrdID;
            this.OrderID = parent.OrderID;
            this.QuoteID = parent.QuoteID;
            this.BookID = parent.BookID;
            this.QuantID = parent.QuantID;
            this.DcsexecID = parent.DcsexecID;
            this.QuoteType = parent.QuoteType;
            this.MDReqID = parent.MDReqID;
            this.QuoteTransType = parent.QuoteTransType;
            this.QuoteStatusDesc = parent.QuoteStatusDesc;
            this.OrdStatusReqID = parent.OrdStatusReqID;
            this.ExecAckStatus = parent.ExecAckStatus;
            this.OrdRejReason = parent.OrdRejReason;
            this.RejectText = parent.RejectText;
            this.Rejecter = parent.Rejecter;
            this.TransactTime = parent.TransactTime;
            this.SecurityID = parent.SecurityID;
            this.Symbol = parent.Symbol;
            this.ValidUntilTime = parent.ValidUntilTime;
            this.FirmToNonTime = parent.FirmToNonTime;
            this.OrdType = parent.OrdType;
            this.TimeInForce = parent.TimeInForce;
            this.Side = parent.Side;
            this.Price = parent.Price;
            this.Price2 = parent.Price2;
            this.NetPrice = parent.NetPrice;
            this.NetPrice2 = parent.NetPrice2;
            this.Yield = parent.Yield;
            this.Yield2 = parent.Yield2;
            this.OrderQty = parent.OrderQty;
            this.OrderQty2 = parent.OrderQty2;
            this.LeavesQty = parent.LeavesQty;
            this.LeavesQty2 = parent.LeavesQty2;
            this.CumQty = parent.CumQty;
            this.CumQty2 = parent.CumQty2;
            this.OrdStatus2 = parent.OrdStatus2;
            this.TradeDate = parent.TradeDate;
            this.TradeTime = parent.TradeTime;
            this.SettlDate = parent.SettlDate;
            this.SettlType = parent.SettlType;
            this.ClearingMethod = parent.ClearingMethod;
            this.DeliveryType = parent.DeliveryType;
            this.ExecID = parent.ExecID;
            this.ExecType = parent.ExecType;
            this.ExecType2 = parent.ExecType2;
            this.LastPx = parent.LastPx;
            this.LastQty = parent.LastQty;
            this.LastPx2 = parent.LastPx2;
            this.LastQty2 = parent.LastQty2;
            this.PartyID = parent.PartyID;
            this.PartyName = parent.PartyName;
            this.PartySubID = parent.PartySubID;
            this.PartySubID2 = parent.PartySubID2;
            this.PartySubName = parent.PartySubName;
            this.PartyCode = parent.PartyCode;
            this.CounterPartyID = parent.CounterPartyID;
            this.CounterPartyName = parent.CounterPartyName;
            this.CounterPartyFullName = parent.CounterPartyFullName;
            this.CounterPartySubID = parent.CounterPartySubID;
            this.CounterPartySubName = parent.CounterPartySubName;
            this.CounterPartyOrdID = parent.CounterPartyOrdID;
            this.CounterPartyCode = parent.CounterPartyCode;
            this.DirtyPrice = parent.DirtyPrice;
            this.OpenQty = parent.OpenQty;
            this.UserID = parent.UserID;
            this.UserName = parent.UserName;
            this.BizType = parent.BizType;
            this.BizTypeID = parent.BizTypeID;
            this.AvgPx = parent.AvgPx;
            this.AvgPx2 = parent.AvgPx2;
            this.RefOrderID = parent.RefOrderID;
            this.FirmFlag = parent.FirmFlag;
            this.WholeFlag = parent.WholeFlag;
            this.VenuesSelectF = parent.VenuesSelectF;
            this.VenuesRdmNum = parent.VenuesRdmNum;
            this.SelfSelectVenues = parent.SelfSelectVenues;
            this.BlockNum = parent.BlockNum;
            this.DealType = parent.DealType;
            this.CrossVenues = parent.CrossVenues;
            this.AllocID = parent.AllocID;
            this.UpdateTime = parent.UpdateTime;
            this.UpdateUser = parent.UpdateUser;
            this.SynchFlag = parent.SynchFlag;
            this.OCOFlag = parent.OCOFlag;
            this.InstitutionID = parent.InstitutionID;
            this.InstitutionName = parent.InstitutionName;
            this.TraderID = parent.TraderID;
            this.TraderName = parent.TraderName;
            this.ShowTrader = parent.ShowTrader;
            this.RoleName = parent.RoleName;
            this.RoleID = parent.RoleID;
            this.SalesAgentFlag = parent.SalesAgentFlag;
            this.CounterpartySalesAgentFlag = parent.CounterpartySalesAgentFlag;
            this.OrigOrdPrice = parent.OrigOrdPrice;
            this.MacID = parent.MacID;
            this.PublishIP = parent.PublishIP;
            this.InsideIP = parent.InsideIP;
            this.HardDiskID = parent.HardDiskID;
            this.BrokerExecID = parent.BrokerExecID;
            this.AppointedID = parent.AppointedID;
            this.InitiatorID = parent.InitiatorID;
            this.InitiatorSubID = parent.InitiatorSubID;
            this.BridgecounterPartyFullName = parent.BridgecounterPartyFullName;
            this.UpdateFiled = parent.UpdateFiled;
            this.TransType = parent.TransType;
            this.RoleType = parent.RoleType;
            this.PartyOrderSource = parent.PartyOrderSource;
            this.CounterPartyOrderSource = parent.CounterPartyOrderSource;
            this.RecordNum = parent.RecordNum;
            this.SendingTime = parent.SendingTime;
            this.IocTimeOut = parent.IocTimeOut;
            this.IocPrice = parent.IocPrice;
            this.HandlInst = parent.HandlInst;
            this.IocVenueTypeGW = parent.IocVenueTypeGW;
            this.IocVenues = parent.IocVenues;
            this.CounterPartyVenues = parent.CounterPartyVenues;
            this.ParentFlag = parent.ParentFlag;
            this.ParentFlagQty = parent.ParentFlagQty;
            this.ActionFlag = parent.ActionFlag;
            this.ExecTypeReason = parent.ExecTypeReason;
            this.TradeMethod = parent.TradeMethod;
            this.SecurityType = parent.SecurityType;
            this.A1 = parent.A1;
            this.B1 = parent.B1;
            this.A0 = parent.A0;
            this.B2 = parent.B2;
            this.M5b1 = parent.M5b1;
            this.M5a1 = parent.M5a1;
            this.A2 = parent.A2;
            this.B0 = parent.B0;
            this.Vs = parent.Vs;
            this.Info1 = parent.Info1;
            this.Info2 = parent.Info2;
            this.Info3 = parent.Info3;
            this.Info4 = parent.Info4;
            this.Info5 = parent.Info5;
            this.Info6 = parent.Info6;
            this.Info7 = parent.Info7;
            this.Info8 = parent.Info8;
            this.Info9 = parent.Info9;
            this.Info10 = parent.Info10;
            this.TrigEvent = parent.TrigEvent;
            this.CancEvent = parent.CancEvent;
            this.InValidFlag = parent.InValidFlag;
            this.EREventName = parent.EREventName;
            this.CancEventName = parent.CancEventName;
            this.MaxFloor = parent.MaxFloor;
            this.IntentionID = parent.IntentionID;
            this.OffsetFlag = parent.OffsetFlag;
            this.HedgeFlag = parent.HedgeFlag;
            this.ExerciseFlag = parent.ExerciseFlag;
            this.PriceType = parent.PriceType;
            this.SSDetect = parent.SSDetect;
            this.QuoteReqID = parent.QuoteReqID;
            this.QuoteStatus = parent.QuoteStatus;
            this.OrigClOrdID = parent.OrigClOrdID;
            this.AccountID = parent.AccountID;
            this.AccountName = parent.AccountName;
            this.BBVol = parent.BBVol;
            this.BBPx = parent.BBPx;
            this.BBTime = parent.BBTime;
            this.BOVol = parent.BOVol;
            this.BOPx = parent.BOPx;
            this.BOTime = parent.BOTime;
            this.Tag1 = parent.Tag1;
            this.PriceCondition = parent.PriceCondition;
            this.DqOrders = parent.DqOrders;
            this.CounterAccountID = parent.CounterAccountID;
            this.CounterAccountName = parent.CounterAccountName;
            this.Version = parent.Version;
            this.DqInfo = parent.DqInfo;
            this.MatchStatus = parent.MatchStatus;
            this.RelateStatus = parent.RelateStatus;
            this.QuoteMethod = parent.QuoteMethod;
            this.EntryType = parent.EntryType;
            this.Option = parent.Option;
            this.CreateBy = parent.CreateBy;
            this.MacaulayDuration = parent.MacaulayDuration;
            this.ModifiedDuration = parent.ModifiedDuration;
            this.Pvbp = parent.Pvbp;
            this.Convexity = parent.Convexity;
            this.RepoMethod = parent.RepoMethod;
            this.TradeLimitDays = parent.TradeLimitDays;
            this.DeliveryType2 = parent.DeliveryType2;
            this.SettlCurrency = parent.SettlCurrency;
            this.SettlCurrFxRate = parent.SettlCurrFxRate;
            this.PledgeSubmissionCode = parent.PledgeSubmissionCode;
            this.BenchmarkInterestRate = parent.BenchmarkInterestRate;
            this.QuoteRespType = parent.QuoteRespType;
            this.Spread = parent.Spread;
            this.ContingencyIndicator = parent.ContingencyIndicator;
            this.SeatNumber = parent.SeatNumber;
            this.Channel = parent.Channel;
            this.OnLine = parent.OnLine;
            this.FanDian = parent.FanDian;
            this.FanDianFlag = parent.FanDianFlag;
            this.PartDealFlag = parent.PartDealFlag;
            this.ExchangeSignFlag = parent.ExchangeSignFlag;
            this.BargainFlag = parent.BargainFlag;
            this.VipCode = parent.VipCode;
            this.Text = parent.Text;
            this.CommandType = parent.CommandType;
            this.IsConfirmOperation = parent.IsConfirmOperation;
            this.OrderSN = parent.OrderSN;
            this.CounterPartyTraderId = parent.CounterPartyTraderId;
            this.CounterPartyTraderName = parent.CounterPartyTraderName;
            this.CounterPartyInstitutionId = parent.CounterPartyInstitutionId;
            this.CounterPartyInstitutionName = parent.CounterPartyInstitutionName;
            this.CompanyId = parent.CompanyId;
            this.MatchQTY = parent.MatchQTY;
            this.QuoteInternalFlag = parent.QuoteInternalFlag;
            this.TimeToMaturity = parent.TimeToMaturity;
            this.BrokerBConfirmQty = parent.BrokerBConfirmQty;
            this.SendQQMsgFlag = parent.SendQQMsgFlag;
            this.SendQQMsgTem = parent.SendQQMsgTem;
            this.Sequence = parent.Sequence;
            this.BridgeFlag = parent.BridgeFlag;
            this.SendTo = parent.SendTo;
            this.SendTo2 = parent.SendTo2;
            this.CloseBy = parent.CloseBy;
            this.BrokerUnread = parent.BrokerUnread;
            this.CloseById = parent.CloseById;
            this.DCSFlag = parent.DCSFlag;
            this.QueryFlag = parent.QueryFlag;
            this.CloseById2 = parent.CloseById2;
            this.InternalFlag = parent.InternalFlag;
            this.SettlType2 = parent.SettlType2;
            this.TradeDate2 = parent.TradeDate2;
            this.SettlDate2 = parent.SettlDate2;
            this.BridgeInstitutionName = parent.BridgeInstitutionName;
            this.BridgeInstitutionId = parent.BridgeInstitutionId;
            this.BridgeTraderId = parent.BridgeTraderId;
            this.BridgeTraderName = parent.BridgeTraderName;
            this.BridgeChannel = parent.BridgeChannel;
            this.BridgeSendTo = parent.BridgeSendTo;
            this.BridgeFee = parent.BridgeFee;
            this.BridgeRemark = parent.BridgeRemark;
            this.BridgeType = parent.BridgeType;
            this.BridgeInstitutionName2 = parent.BridgeInstitutionName2;
            this.BridgeInstitutionId2 = parent.BridgeInstitutionId2;
            this.BridgeTraderId2 = parent.BridgeTraderId2;
            this.BridgeTraderName2 = parent.BridgeTraderName2;
            this.BridgeChannel2 = parent.BridgeChannel2;
            this.BridgeSendTo2 = parent.BridgeSendTo2;
            this.BridgeFee2 = parent.BridgeFee2;
            this.BridgeRemark2 = parent.BridgeRemark2;
            this.BridgeType2 = parent.BridgeType2;
            this.BridgeStatus = parent.BridgeStatus;
            this.BridgeCloseById = parent.BridgeCloseById;
            this.BridgeCreateTime = parent.BridgeCreateTime;
            this.BridgeUpdateTime = parent.BridgeUpdateTime;
            this.BridgeCreateTime2 = parent.BridgeCreateTime2;
            this.BridgeUpdateTime2 = parent.BridgeUpdateTime2;
            this.BridgeRemarkShow = parent.BridgeRemarkShow;
            this.BridgeRemarkShow2 = parent.BridgeRemarkShow2;
            this.BorderId = parent.BorderId;
            this.BorderId2 = parent.BorderId2;
            this.SyncDCSFlag = parent.SyncDCSFlag;
            this.SyncDCSTime = parent.SyncDCSTime;
            this.DoubleBridgeChannel = parent.DoubleBridgeChannel;
            this.DoubleBridgeSendTo = parent.DoubleBridgeSendTo;
            this.DoubleBridgeFee = parent.DoubleBridgeFee;
            this.DoubleBridgeRemark = parent.DoubleBridgeRemark;
            this.DoubleBridgeOffSettltype = parent.DoubleBridgeOffSettltype;
            this.DoubleBridgeBidSettltype = parent.DoubleBridgeBidSettltype;
            this.BondSubtype = parent.BondSubtype;
            this.OnlineA = parent.OnlineA;
            this.OnlineB = parent.OnlineB;
            this.AutoConfirmA = parent.AutoConfirmA;
            this.AutoConfirmB = parent.AutoConfirmB;
            this.BrokerageA = parent.BrokerageA;
            this.BrokerageB = parent.BrokerageB;
            this.Remark = parent.Remark;
            this.SyncQmFlag = parent.SyncQmFlag;
            this.SyncQmTime = parent.SyncQmTime;
            this.DoubleBridgeSettleSide = parent.DoubleBridgeSettleSide;
            this.AgreementNo = parent.AgreementNo;
            this.ForBidRefreshFlag = parent.ForBidRefreshFlag;
            this.SendSorFlagA = parent.SendSorFlagA;
            this.SendSorFlagB = parent.SendSorFlagB;
            this.IssueState = parent.IssueState;
            this.IssueDir = parent.IssueDir;
            this.IssueChannel = parent.IssueChannel;
            this.IssueInstId = parent.IssueInstId;
            this.IssueInstName = parent.IssueInstName;
            this.IssueInstNameShort = parent.IssueInstNameShort;
            this.IssueTraderId = parent.IssueTraderId;
            this.IssueTraderName = parent.IssueTraderName;
            this.IssueRemark = parent.IssueRemark;
            this.ProxyInstId = parent.ProxyInstId;
            this.ProxyFee = parent.ProxyFee;
            this.ProxyFlag = parent.ProxyFlag;
            this.IssueDesc = parent.IssueDesc;
            this.IssueShowType = parent.IssueShowType;
            this.SingleQqmsgSendFlagA = parent.SingleQqmsgSendFlagA;
            this.SingleQqmsgSendFlagB = parent.SingleQqmsgSendFlagB;
            this.OfrBridgeDir = parent.OfrBridgeDir;
            this.BidBridgeDir = parent.BidBridgeDir;
            this.DetailMark = parent.DetailMark;
            //this.DetailMark = parent.m;
            this.AssociatedFlag = parent.AssociatedFlag;
            this.MakerOrderQty = parent.MakerOrderQty;
            this.OFRSpecialBridgeFee = parent.OFRSpecialBridgeFee;
            this.BIDSpecialBridgeFee = parent.BIDSpecialBridgeFee;
            this.BridgeInstitutionFullName = parent.BridgeInstitutionFullName;
            this.BridgeInstitutionFullName2 = parent.BridgeInstitutionFullName2;
            this.ReadFlag = parent.ReadFlag;
            this.SendMsgFlag = parent.SendMsgFlag;
            this.ID = parent.ID;
            this.InstitutionShowName = parent.InstitutionShowName;
            this.CounterPartyInstitutionShowName = parent.CounterPartyInstitutionShowName;
            this.BridgeInstitutionShowName = parent.BridgeInstitutionShowName;
            this.BridgeInstitutionShowName2 = parent.BridgeInstitutionShowName2;
            this.AddBridgeWay = parent.AddBridgeWay;
            this.BridgeStatus2 = parent.BridgeStatus2;
            this.InstExecOrderReqListExt = parent.InstExecOrderReqListExt;
            this.OfrDir = parent.OfrDir;
            this.BidDir = parent.BidDir;
            this.OfrDir2 = parent.OfrDir2;
            this.BidDir2 = parent.BidDir2;
            this.DelFlag = parent.DelFlag;
            this.ParamCloseById = parent.ParamCloseById;
            this.BridgeOrder = parent.BridgeOrder;
            this.BridgeOrder2 = parent.BridgeOrder2;
            this.MarkDetailUsers = parent.MarkDetailUsers;
            this.DayOffStr = parent.DayOffStr;
            this.NoUnderlyings = new List<GNoUnderlyings>();
            this.BridgeOrderModel = TransOrderHandler.Instance.GetBridgeOrder(this.BridgeOrder);
            this.BridgeOrderModel2 = TransOrderHandler.Instance.GetBridgeOrder(this.BridgeOrder2);
            if (parent.NoUnderlyings != null)
            {
                foreach (var g in parent.NoUnderlyings)
                {
                    GNoUnderlyings gNoUnderlyings = g.Clone();
                    this.NoUnderlyings.Add(gNoUnderlyings);
                }
            }
            this.NoFloorDetails = new List<GNoFloorDetails>();
            if (parent.NoFloorDetails != null)
            {
                foreach (var g in parent.NoFloorDetails)
                {
                    GNoFloorDetails gNoFloorDetails = g.Clone();
                    this.NoFloorDetails.Add(gNoFloorDetails);
                }
            }
            this.NoCrossCustodians = new List<GNoCrossCustodians>();
            if (parent.NoCrossCustodians != null)
            {
                foreach (var g in parent.NoCrossCustodians)
                {
                    GNoCrossCustodians gNoCrossCustodians = g.Clone();
                    this.NoCrossCustodians.Add(gNoCrossCustodians);
                }
            }
            if (!string.IsNullOrWhiteSpace(parent.InstExecOrderReqListExt))
            {
                BridgeOrderReq po = JsonConvert.DeserializeObject<BridgeOrderReq>(parent.InstExecOrderReqListExt);
                this.OfrIssueInfoReqList = po != null ? po.ofrInstExecOrderReqList : null;
                this.BidIssueInfoReqList = po != null ? po.bidInstExecOrderReqList : null;
            }

        }

        public bridgeOrder BridgeOrderModel
        {
            get;set;
        }

        public bridgeOrder BridgeOrderModel2
        {
            get; set;
        }


        /// <summary>
        /// ofr结算显示
        /// </summary>
        public string SettleDisplayOfr
        {
            get
            {
                return TransOrderHelper.Instance.GetSettleDisplay(SettlType2, TradeDate2);
            }
        }
        /// <summary>
        /// Bid结算显示
        /// </summary>
        public string SettleDisplayBid
        {
            get
            {
                return TransOrderHelper.Instance.GetSettleDisplay(SettlType, TradeDate);
            }
        }


        public string ContentDisplay
        {
            get
            {
                string result = string.Empty;
                if (!string.IsNullOrEmpty(SettleDisplayOfr) && SettleDisplayBid.Equals(SettleDisplayOfr))
                {
                    if (!string.IsNullOrEmpty(Remark))
                    {
                        DateTime dtDeal = OrderHelper.GetDate(TradeDate);
                        result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + Remark;
                    }
                    else
                        result = SettleDisplayBid;
                }
                else
                {
                    if (!string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = string.Format("{0}/{1}", SettleDisplayOfr, SettleDisplayBid);
                    else if (!string.IsNullOrEmpty(SettlType) && string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplayBid;
                    else if (string.IsNullOrEmpty(SettlType) && !string.IsNullOrEmpty(SettlType2))
                        result = SettleDisplayOfr;
                }

                return result;



            }
        }


        public string PriceFormat
        {
            get
            {
                return string.IsNullOrWhiteSpace(Price) ? "0" : Price;
            }
        }

        public string FandianShow
        {
            get
            {
                decimal d = 0;
                if (decimal.TryParse(FanDian, out d))
                    return !string.IsNullOrEmpty(FanDian) ? "F" + FanDian : string.Empty;
                else
                    return FanDian;
            }
        }

        public string TraderAInfo
        {
            get
            {
                return GetTraderAInfo();
            }
        }

        /// <summary>
        /// 获取交易员展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        private string GetTraderAInfo()
        {
            if (string.IsNullOrWhiteSpace(PartyCode) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(InstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(InstitutionShowName))
                    return string.Format("{0}({1})", InstitutionShowName, TraderName.Trim());
                else
                    return string.Format("{0}({1})", InstitutionName, TraderName.Trim());
            }

            if (string.IsNullOrWhiteSpace(TraderName) && (!string.IsNullOrWhiteSpace(InstitutionName) || !string.IsNullOrEmpty(InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(InstitutionShowName))
                    return InstitutionShowName;
                else
                    return InstitutionName;
            }
            if (!string.IsNullOrWhiteSpace(TraderName) && string.IsNullOrWhiteSpace(InstitutionName) && string.IsNullOrWhiteSpace(InstitutionShowName))
                return TraderName;

            return "";
        }

        public string TraderBInfo
        {
            get
            {
                return GetTraderBInfo();
            }
        }

        /// <summary>
        /// 获取交易员展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        private string GetTraderBInfo()
        {
            if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                    return string.Format("{0}({1})", CounterPartyInstitutionShowName, CounterPartyTraderName.Trim());
                else
                    return string.Format("{0}({1})", CounterPartyInstitutionName, CounterPartyTraderName.Trim());
            }


            if (string.IsNullOrWhiteSpace(CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(CounterPartyInstitutionName) || !string.IsNullOrEmpty(CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                    return CounterPartyInstitutionShowName;
                else
                    return CounterPartyInstitutionName;
            }

            if (!string.IsNullOrWhiteSpace(CounterPartyTraderName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(CounterPartyInstitutionShowName))
                return CounterPartyTraderName;

            return "";
        }

        /// <summary>
        /// broker方向显示
        /// </summary>
        public string SideDisplay
        {
            get
            {
                string sideDisplay = "";
                //if (forbidRefreshFlag == "3" && SystemParamHandler.GlobSystemParameter.showSourceDealSide)
                //{
                //    sideDisplay = "TRD";
                //}
                //else
                //{
                //    sideDisplay = Online + "" == "3" ? IsGVN ? "出给" : "买入" : IsGVN ? "GVN" : "TKN";
                //}
                if (!string.IsNullOrWhiteSpace(OnlineA) && !string.IsNullOrWhiteSpace(OnlineB))
                {

                    if (OnlineA.Equals("3") && OnlineB.Equals("3"))
                    {
                        //if (newOrder.ForbidRefreshFlag == "3" && SystemParamHandler.GlobSystemParameter.showSourceDealSide)
                        //{
                        //    oldOrder.ShowSide = "TRD";
                        //}
                        //else
                        //{
                        sideDisplay = Side == "2" ? "出给" : "买入";
                        //}
                    }
                    else
                    {
                        //if (newOrder.ForbidRefreshFlag == "3" && SystemParamHandler.GlobSystemParameter.showSourceDealSide)
                        //{
                        //    oldOrder.ShowSide = "TRD";
                        //}
                        //else
                        //{
                        sideDisplay = Side == "2" ? "GVN" : "TKN";
                        //}
                    }
                }
                else
                {
                    sideDisplay = Side == "2" ? "GVN" : "TKN";
                }
                return sideDisplay;
            }
        }


        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTimeDisplay
        {
            get
            {
                string createTimeDisplay = TransactTime;
                DateTime dt;
                if (DateTime.TryParseExact(TransactTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (TransOrderHelper.Instance.IsDealHistDay(this))
                    {
                        createTimeDisplay = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                        createTimeDisplay = dt.ToString("HH:mm:ss");
                }
                else
                {
                    createTimeDisplay = TransactTime;
                }
                return createTimeDisplay;
            }
        }


        /// <summary>
        /// 更新时间
        /// </summary>
        public string UpdateTimeDisplay
        {
            get
            {
                string createTimeDisplay = UpdateTime;
                DateTime dt;
                if (DateTime.TryParseExact(UpdateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (TransOrderHelper.Instance.IsDealHistDay(this))
                    {
                        createTimeDisplay = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                        createTimeDisplay = dt.ToString("HH:mm:ss");
                }
                else
                {
                    createTimeDisplay = UpdateTime;
                }
                return createTimeDisplay;
            }
        }

        /// <summary>
        /// 当前登录是否为brokerA
        /// </summary>
        public string BrokerARole
        {

            get
            {
                if (LocalEnv.UserInfo.IsMyself(PartySubID))
                {
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
        }


        /// <summary>
        /// 当前登录是否为brokerB
        /// </summary>
        public string BrokerBRole
        {
            get
            {
                if (LocalEnv.UserInfo.IsMyself(CounterPartySubID))
                {
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
        }

        public bool IsGVN
        {
            get
            {
                return "2".Equals(Side);
            }
        }

        public string OfrbankAgentName
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(TraderName))
                        TraderName = null;
                    return TraderName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyTraderName))
                        CounterPartyTraderName = null;
                    return CounterPartyTraderName;
                }
            }
        }

        public string BidbankAgentName
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyTraderName))
                        CounterPartyTraderName = null;
                    return CounterPartyTraderName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(TraderName))
                        TraderName = null;
                    return TraderName;
                }
            }
        }

        public string OfrInst
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(InstitutionName))
                        InstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyInstitutionName))
                        CounterPartyInstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }
            }
        }

        public string BidInst
        {
            get
            {
                if (IsGVN)
                {
                    if (string.IsNullOrWhiteSpace(CounterPartyInstitutionName))
                        CounterPartyInstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(CounterPartyName))
                        return CounterPartyName;
                    else
                        return CounterPartyInstitutionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(InstitutionName))
                        InstitutionName = null;
                    if (!string.IsNullOrWhiteSpace(PartyName))
                        return PartyName;
                    else
                        return InstitutionName;
                }
            }
        }



        /// <summary>
        /// ofr结算显示
        /// </summary>
        public string SettleDisplayBridgeOfr
        {
            get
            {
                return TransOrderHelper.Instance.GetSettleDisplay(DoubleBridgeOffSettltype, TradeDate2);
            }
        }
        /// <summary>
        /// Bid结算显示
        /// </summary>
        public string SettleDisplayBridgeBid
        {
            get
            {
                return TransOrderHelper.Instance.GetSettleDisplay(DoubleBridgeBidSettltype, TradeDate);
            }
        }

        /// <summary>
        /// 双桥结算：显示为"Ofr +1，Bid +1/明天+0“”格式，分别把双桥详情中的Ofr结算和Bid结算拼在一起显示即可。
        /// </summary>
        public string DoubleBridgeSettle
        {
            get
            {
                if (string.IsNullOrEmpty(DoubleBridgeOffSettltype) && string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return "";
                else if (string.IsNullOrEmpty(DoubleBridgeOffSettltype) && !string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return string.Format("Bid {0}", DoubleBridgeBidSettltype);
                else if (!string.IsNullOrEmpty(DoubleBridgeOffSettltype) && string.IsNullOrEmpty(DoubleBridgeBidSettltype))
                    return string.Format("Ofr {0}", DoubleBridgeOffSettltype);
                else
                    return string.Format("Ofr {0},Bid {1}", DoubleBridgeOffSettltype, DoubleBridgeBidSettltype);
            }
        }

        /// <summary>
        /// 双桥信息：显示为“发XXX，渠道费用，备注”格式，如“发国金证券，请求3厘，111”
        /// </summary>
        /// 
        public string DoubleBridgeInfo
        {

            get
            {

                if (string.IsNullOrEmpty(DoubleBridgeSendTo) && string.IsNullOrEmpty(DoubleBridgeChannel) && string.IsNullOrEmpty(DoubleBridgeFee) && string.IsNullOrEmpty(DoubleBridgeRemark))
                    return "";
                else if (string.IsNullOrEmpty(DoubleBridgeSendTo))
                {
                    if (string.IsNullOrEmpty(DoubleBridgeRemark))
                        return string.Format("{0}{1}", DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee);
                    else
                        return string.Format("{0}{1}, {2}", DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
                }
                else if (!string.IsNullOrEmpty(DoubleBridgeSendTo))
                {
                    if (string.IsNullOrEmpty(DoubleBridgeRemark))
                        return string.Format("发{0}, {1}{2}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee);
                    else
                        return string.Format("发{0}, {1}{2}, {3}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
                }
                else
                    return string.Format("发{0}, {1}{2}, {3}", DoubleBridgeSendTo, DoubleChanelConvert(DoubleBridgeChannel + ""), DoubleBridgeFee, DoubleBridgeRemark);
            }
        }

        Func<string, string> DoubleChanelConvert = delegate (string a)
        {
            switch (a)
            {
                case "1":
                    return "对话";
                case "2":
                    return "请求";
                case "3":
                    return "双边";
                case "4":
                    return "xbond";
                case "5":
                    return "ideal";
                case "6":
                    return "固收";
                case "7":
                    return "竞价";
                case "8":
                    return "大宗";
                case "9":
                    return "意向";
                case "10":
                    return "匿名";
                case "11":
                    return "xbargain";
                default:
                    return "对话";
            }
        };


        /// <summary>
        /// brokerB需要点击发送按钮
        /// </summary>
        public bool NeeSendMsgForB
        {
            get
            {
                return "2".Equals(OnlineA) && "2".Equals(OnlineB) && ("0".Equals(OrdStatus) || "1".Equals(OrdStatus) || "2".Equals(OrdStatus) || "4".Equals(OrdStatus)) && !"1".Equals(SendMsgFlag);
            }
        }
        public bool IsPartlyConfirm
        {
            get
            {
                return !string.IsNullOrWhiteSpace(MatchQTY)
                    && !string.IsNullOrWhiteSpace(OrderQty)
                    && MatchQTY != OrderQty;
            }
        }
        public string BrokerAStatus
        {
            get
            {
                string brokerAStatus = "";
                if (string.IsNullOrWhiteSpace(PartySubID))
                {
                    return brokerAStatus;
                }
                //broker状态显示
                switch (OrdStatus)
                {
                    case "0":
                    case "1":
                    case "2":
                        brokerAStatus = "1";
                        break;
                    case "I":
                        brokerAStatus = "4";
                        break;
                    case "E":
                        brokerAStatus = "1";
                        break;
                    case "H":
                        brokerAStatus = "3";
                        break;
                    case "F":
                        brokerAStatus = "4";
                        break;
                    case "G":
                        brokerAStatus = "4";
                        break;
                }
                return brokerAStatus;
            }

        }

        public string BrokerBStatus
        {
            get
            {
                string brokerBStatus = "";
                if (string.IsNullOrWhiteSpace(CounterPartySubID))
                {
                    return brokerBStatus;
                }
                //broker状态显示
                switch (OrdStatus)
                {
                    case "0":
                    case "1":
                    case "2":
                        brokerBStatus = "1";
                        break;
                    case "I":
                        brokerBStatus = "1";
                        break;
                    case "E":
                    case "H":
                        if (IsPartlyConfirm)
                        {
                            brokerBStatus = "2";
                        }
                        else
                        {
                            brokerBStatus = "4";
                        }
                        break;
                    case "F":
                        brokerBStatus = "3";
                        break;
                    case "G":
                        if (IsPartlyConfirm)
                        {
                            brokerBStatus = "2";
                        }
                        else
                        {
                            brokerBStatus = "4";
                        }
                        break;
                }
                return brokerBStatus;
            }

        }

        //操作流水展示ofr发给
        public string OfrSendToDisplay
        {
            get
            {
                if ((BridgeFlag + "").Equals("2"))
                    return SendTo2;
                else
                    return "";
            }
        }
        //操作流水展示bid发给
        public string BidSendToDisplay
        {
            get
            {
                if ((BridgeFlag + "").Equals("2"))
                    return SendTo;
                else
                    return "";
            }
        }

        public List<string> MarkDetailUserList
        {
            get
            {
                List<string> vs1 = new List<string>();
                if (!string.IsNullOrWhiteSpace(MarkDetailUsers))
                {
                    if (MarkDetailUsers.IndexOf(",") > -1)
                    {
                        string[] vs = MarkDetailUsers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        vs1 = vs.ToList();
                    }
                    else {
                        vs1.Add(MarkDetailUsers);
                    }
                }
                return vs1;
            }
        }


        #region 债券信息补全用
        public decimal? IssueAmount;
        public string IssuerRatingCurrent;
        public string IssuerCode;
        public string EntCor;
        public string FRNIndexID;
        public List<SendInstSevieceModel> OfrIssueInfoReqList { get; set; }
        public List<SendInstSevieceModel> BidIssueInfoReqList { get; set; }
        #endregion
    }
}
