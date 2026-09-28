using Ats.Foundation.Message;
using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public class ObjConvertHelper
    {
        public static void UpdateOrder(OrderDetailPo orderDetailPo, BridgeOrderDetail result)
        {
            result.TraderID = orderDetailPo.TraderID;
            result.CounterPartyTraderId = orderDetailPo.CounterPartyTraderId;
            result.TraderName = orderDetailPo.TraderName;
            result.CounterPartyTraderName = orderDetailPo.CounterPartyTraderName;
            result.InstitutionName = orderDetailPo.InstitutionName;
            result.CounterPartyInstitutionName = orderDetailPo.CounterPartyInstitutionName;
            result.PartyName = orderDetailPo.PartyName;
            result.CounterPartyName = orderDetailPo.CounterPartyName;
            result.InstitutionID = orderDetailPo.InstitutionID;
            result.CounterPartyInstitutionId = orderDetailPo.CounterPartyInstitutionId;
            result.BargainFlag = orderDetailPo.BargainFlag;
            result.BridgeFlag = orderDetailPo.BridgeFlag;
            result.brokerbConfirmQty = orderDetailPo.brokerbConfirmQty;
            result.PartySubID = orderDetailPo.PartySubID;
            result.CounterPartySubID = orderDetailPo.CounterPartySubID;
            result.PartySubName = orderDetailPo.PartySubName;
            result.CounterPartySubName = orderDetailPo.CounterPartySubName;
            result.brokerreadflaga = orderDetailPo.brokerreadflaga;
            result.brokerreadflagb = orderDetailPo.brokerreadflagb;
            result.BrokerUnread = orderDetailPo.BrokerUnread;
            result.closeby = orderDetailPo.closeby;
            result.CloseById = orderDetailPo.CloseById;
            result.CloseById2 = orderDetailPo.CloseById2;
            result.CompanyId = orderDetailPo.CompanyId;
            result.createTime = orderDetailPo.createTime;
            result.dcsflag = orderDetailPo.dcsflag;
            result.dealdate = orderDetailPo.dealdate;
            result.DealDate2 = orderDetailPo.DealDate2;
            result.delflag = orderDetailPo.delflag;
            result.ExecID = orderDetailPo.ExecID;
            result.SecurityID = orderDetailPo.SecurityID;
            result.goodsShortName = orderDetailPo.goodsShortName;
            result.goodstype = orderDetailPo.goodstype;
            result.goodsSubType = orderDetailPo.goodsSubType;
            result.inf1 = orderDetailPo.inf1;
            result.inf2 = orderDetailPo.inf2;
            result.inf3 = orderDetailPo.inf3;
            result.inf4 = orderDetailPo.inf4;
            result.InternalFlag = orderDetailPo.InternalFlag;
            result.matchQty = orderDetailPo.matchQty;
            result.OCOFlag = orderDetailPo.OCOFlag;
            result.OrderID = orderDetailPo.OrderID;
            result.OrdStatus = orderDetailPo.OrdStatus;
            result.price = orderDetailPo.price;
            result.priceDescription = orderDetailPo.priceDescription;
            result.queryflag = orderDetailPo.queryflag;
            result.QuoteID = orderDetailPo.QuoteID;
            result.quoteInternalFlag = orderDetailPo.quoteInternalFlag;
            result.quoteqty = orderDetailPo.quoteqty;
            result.qty = orderDetailPo.qty;
            result.rejecter = orderDetailPo.rejecter;
            result.rejecterReason = orderDetailPo.rejecterReason;
            result.SendQQMsgFlag = orderDetailPo.SendQQMsgFlag;
            result.sendQQMsgTem = orderDetailPo.sendQQMsgTem;
            result.SendTo = orderDetailPo.SendTo;
            result.SendTo2 = orderDetailPo.SendTo2;
            result.seq = orderDetailPo.seq;
            result.SettlDate = orderDetailPo.SettlDate;
            result.SettlDate2 = orderDetailPo.SettlDate2;
            result.SettlType = orderDetailPo.SettlType;
            result.SettlType2 = orderDetailPo.SettlType2;
            result.Side = orderDetailPo.Side;
            result.WholeFlag = orderDetailPo.WholeFlag;
            result.timeToMaturity = orderDetailPo.timeToMaturity;

            result.BridgeInstitutionName = orderDetailPo.BridgeInstitutionName;
            result.bridgebankcode = orderDetailPo.bridgebankcode;
            result.BridgeInstitutionId = orderDetailPo.BridgeInstitutionId;
            result.BridgeTraderId = orderDetailPo.BridgeTraderId;
            result.BridgeTraderName = orderDetailPo.BridgeTraderName;
            result.BridgeChannel = orderDetailPo.BridgeChannel;
            result.BridgeSendTo = orderDetailPo.BridgeSendTo;
            result.BridgeFee = orderDetailPo.BridgeFee;
            result.ofrSpecialBridgeFee = orderDetailPo.ofrSpecialBridgeFee;
            result.BridgeRemark = orderDetailPo.BridgeRemark;
            result.BridgeType = orderDetailPo.BridgeType;

            result.BridgeInstitutionName2 = orderDetailPo.BridgeInstitutionName2;
            result.bridgebankcode2 = orderDetailPo.bridgebankcode2;
            result.BridgeInstitutionId2 = orderDetailPo.BridgeInstitutionId2;
            result.BridgeTraderId2 = orderDetailPo.BridgeTraderId2;
            result.BridgeTraderName2 = orderDetailPo.BridgeTraderName2;
            result.BridgeChannel2 = orderDetailPo.BridgeChannel2;
            result.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
            result.BridgeFee2 = orderDetailPo.BridgeFee2;
            result.bidSpecialBridgeFee = orderDetailPo.bidSpecialBridgeFee;
            result.BridgeRemark2 = orderDetailPo.BridgeRemark2;
            result.BridgeType2 = orderDetailPo.BridgeType2;
            result.BridgeStatus = orderDetailPo.BridgeStatus;
            result.BridgeCloseById = orderDetailPo.BridgeCloseById;

            result.BridgeCreateTime = orderDetailPo.BridgeCreateTime;
            result.BridgeCreateTime2 = orderDetailPo.BridgeCreateTime2;
            result.BridgeUpdateTime = orderDetailPo.BridgeUpdateTime;
            result.BridgeUpdateTime2 = orderDetailPo.BridgeUpdateTime2;

            result.DoubleBridgeBidSettltype = orderDetailPo.DoubleBridgeBidSettltype;
            result.DoubleBridgeChannel = orderDetailPo.DoubleBridgeChannel;
            result.DoubleBridgeFee = orderDetailPo.DoubleBridgeFee;
            result.DoubleBridgeOffSettltype = orderDetailPo.DoubleBridgeOffSettltype;
            result.DoubleBridgeRemark = orderDetailPo.DoubleBridgeRemark;
            result.DoubleBridgeSendTo = orderDetailPo.DoubleBridgeSendTo;

            result.fandian = orderDetailPo.fandian;
            result.remarks = orderDetailPo.remarks;

            result.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
            result.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

            result.brokeragea = orderDetailPo.brokeragea;
            result.brokerageb = orderDetailPo.brokerageb;
            //V2.5 wxp 20200630 桥状态
            result.BorderId = orderDetailPo.BorderId;
            result.BorderId2 = orderDetailPo.BorderId2;
            result.bridgeOrder = orderDetailPo.bridgeOrder;
            result.bridgeOrder2 = orderDetailPo.bridgeOrder2;
            //合单备注
            result.combinedOrderRemark = orderDetailPo.combinedOrderRemark;
            result.combinedOrderRemark2 = orderDetailPo.combinedOrderRemark2;
            //2.9 双桥错期
            result.doubleBridgeSettleSide = orderDetailPo.doubleBridgeSettleSide;

            //2.17.0 增加
            result.ofrIssueInfoReqList = orderDetailPo.ofrIssueInfoReqList;
            result.bidIssueInfoReqList = orderDetailPo.bidIssueInfoReqList;

            //v3.4.2 增加 2023-08-09
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;
        }


        public static void UpdateOrderEx(TransOrderServerData orderDetailPo, BridgeOrderDetail result)
        {
            result.TraderID = orderDetailPo.TraderID;
            result.CounterPartyTraderId = orderDetailPo.CounterPartyTraderId;
            result.TraderName = orderDetailPo.TraderName;
            result.CounterPartyTraderName = orderDetailPo.CounterPartyTraderName;
            result.InstitutionName = orderDetailPo.InstitutionName;
            result.CounterPartyInstitutionName = orderDetailPo.CounterPartyInstitutionName;
            result.PartyName = orderDetailPo.PartyName;
            result.CounterPartyName = orderDetailPo.CounterPartyName;
            result.InstitutionID = orderDetailPo.InstitutionID;
            result.CounterPartyInstitutionId = orderDetailPo.CounterPartyInstitutionId;
            result.BargainFlag = orderDetailPo.BargainFlag;
            result.BridgeFlag = orderDetailPo.BridgeFlag;
            //result.brokerbConfirmQty = orderDetailPo.brokerbonfirmQty;
            result.PartySubID = orderDetailPo.PartySubID;
            result.CounterPartySubID = orderDetailPo.CounterPartySubID;
            result.PartySubName = orderDetailPo.PartySubName;
            result.CounterPartySubName = orderDetailPo.CounterPartySubName;
            //result.brokerreadflaga = orderDetailPo.brokerreadflaga;
            //result.brokerreadflagb = orderDetailPo.brokerreadflagb;
            result.BrokerUnread = orderDetailPo.BrokerUnread;
            result.closeby = orderDetailPo.CloseBy;
            result.CloseById = orderDetailPo.CloseById;
            result.CloseById2 = orderDetailPo.CloseById2;
            result.CompanyId = orderDetailPo.CompanyId;
            result.createTime = orderDetailPo.TransactTime;
            result.dcsflag = orderDetailPo.DCSFlag;
            result.dealdate = orderDetailPo.TradeDate;
            result.DealDate2 = orderDetailPo.TradeDate2;
            result.delflag = orderDetailPo.DelFlag;
            result.ExecID = orderDetailPo.ExecID;
            result.SecurityID = orderDetailPo.SecurityID;
            result.goodsShortName = orderDetailPo.Symbol;
            result.goodstype = orderDetailPo.SecurityType;
            result.goodsSubType = orderDetailPo.BondSubtype;
            //result.inf1 = orderDetailPo.inf1;
            //result.inf2 = orderDetailPo.inf2;
            //result.inf3 = orderDetailPo.inf3;
            //result.inf4 = orderDetailPo.inf4;
            result.InternalFlag = orderDetailPo.InternalFlag;
            decimal matchQty;
            if (decimal.TryParse(orderDetailPo.MatchQTY, out matchQty))
                result.matchQty = matchQty;
            result.OCOFlag = orderDetailPo.OCOFlag;
            result.OrderID = orderDetailPo.OrderID;
            result.OrdStatus = orderDetailPo.OrdStatus;
            decimal price;
            if (decimal.TryParse(orderDetailPo.Price, out price))
                result.price = price;
            result.priceDescription = orderDetailPo.Text;
            result.queryflag = orderDetailPo.QueryFlag;
            result.QuoteID = orderDetailPo.QuoteID;
            result.quoteInternalFlag = orderDetailPo.HandlInst;
            decimal quoteqty;
            if (decimal.TryParse(orderDetailPo.OrderQty, out quoteqty))
                result.quoteqty = quoteqty;
            decimal qty;
            if (decimal.TryParse(orderDetailPo.LastQty, out qty))
                result.qty = qty;
            result.rejecter = orderDetailPo.Rejecter;
            result.rejecterReason = orderDetailPo.RejectText;
            result.SendQQMsgFlag = orderDetailPo.SendQQMsgFlag;
            result.sendQQMsgTem = orderDetailPo.SendQQMsgTem;
            result.SendTo = orderDetailPo.SendTo;
            result.SendTo2 = orderDetailPo.SendTo2;
            result.seq = decimal.Parse(orderDetailPo.Sequence);
            result.SettlDate = orderDetailPo.SettlDate;
            result.SettlDate2 = orderDetailPo.SettlDate2;
            result.SettlType = orderDetailPo.SettlType;
            result.SettlType2 = orderDetailPo.SettlType2;
            result.Side = orderDetailPo.Side;
            result.WholeFlag = orderDetailPo.WholeFlag;
            result.timeToMaturity = orderDetailPo.TimeToMaturity;

            result.BridgeInstitutionName = orderDetailPo.BridgeInstitutionName;
            result.bridgebankcode = orderDetailPo.BridgeInstitutionFullName;
            result.BridgeInstitutionId = orderDetailPo.BridgeInstitutionId;
            result.BridgeTraderId = orderDetailPo.BridgeTraderId;
            result.BridgeTraderName = orderDetailPo.BridgeTraderName;
            result.BridgeChannel = orderDetailPo.BridgeChannel;
            result.BridgeSendTo = orderDetailPo.BridgeSendTo;
            result.BridgeFee = orderDetailPo.BridgeFee;
            result.ofrSpecialBridgeFee = orderDetailPo.OFRSpecialBridgeFee;
            result.BridgeRemark = orderDetailPo.BridgeRemark;
            result.BridgeType = orderDetailPo.BridgeType;

            result.BridgeInstitutionName2 = orderDetailPo.BridgeInstitutionName2;
            result.bridgebankcode2 = orderDetailPo.BridgeInstitutionFullName2;
            result.BridgeInstitutionId2 = orderDetailPo.BridgeInstitutionId2;
            result.BridgeTraderId2 = orderDetailPo.BridgeTraderId2;
            result.BridgeTraderName2 = orderDetailPo.BridgeTraderName2;
            result.BridgeChannel2 = orderDetailPo.BridgeChannel2;
            result.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
            result.BridgeFee2 = orderDetailPo.BridgeFee2;
            result.bidSpecialBridgeFee = orderDetailPo.BIDSpecialBridgeFee;
            result.BridgeRemark2 = orderDetailPo.BridgeRemark2;
            result.BridgeType2 = orderDetailPo.BridgeType2;
            result.BridgeStatus = orderDetailPo.BridgeStatus;
            result.BridgeCloseById = orderDetailPo.BridgeCloseById;

            result.BridgeCreateTime = orderDetailPo.BridgeCreateTime;
            result.BridgeCreateTime2 = orderDetailPo.BridgeCreateTime2;
            result.BridgeUpdateTime = orderDetailPo.BridgeUpdateTime;
            result.BridgeUpdateTime2 = orderDetailPo.BridgeUpdateTime2;

            result.DoubleBridgeBidSettltype = orderDetailPo.DoubleBridgeBidSettltype;
            result.DoubleBridgeChannel = orderDetailPo.DoubleBridgeChannel;
            result.DoubleBridgeFee = orderDetailPo.DoubleBridgeFee;
            result.DoubleBridgeOffSettltype = orderDetailPo.DoubleBridgeOffSettltype;
            result.DoubleBridgeRemark = orderDetailPo.DoubleBridgeRemark;
            result.DoubleBridgeSendTo = orderDetailPo.DoubleBridgeSendTo;

            result.fandian = orderDetailPo.FanDian;
            result.remarks = orderDetailPo.Remark;

            result.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
            result.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

            result.brokeragea = orderDetailPo.BrokerageA;
            result.brokerageb = orderDetailPo.BrokerageB;
            //V2.5 wxp 20200630 桥状态
            result.BorderId = orderDetailPo.BorderId;
            result.BorderId2 = orderDetailPo.BorderId2;
            //合单备注
            result.combinedOrderRemark = orderDetailPo.CombinedOrderRemark;
            result.combinedOrderRemark2 = orderDetailPo.CombinedOrderRemark2;
            //2.9 双桥错期
            result.doubleBridgeSettleSide = orderDetailPo.DoubleBridgeSettleSide;

            //2.17.0 增加
            //result.ofrIssueInfoReqList = orderDetailPo.ofrIssueInfoReqList;
            //result.bidIssueInfoReqList = orderDetailPo.bidIssueInfoReqList;

            //v3.4.2 增加 2023-08-09
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;

            result.bridgeOrder = orderDetailPo.BridgeOrderModel;
            result.bridgeOrder2 = orderDetailPo.BridgeOrderModel2;
            result.ofrIssueInfoReqList = orderDetailPo.OfrIssueInfoReqList;
            result.bidIssueInfoReqList = orderDetailPo.BidIssueInfoReqList;
            //try
            //{
            //    if (orderDetailPo.BridgeOrder != null)
            //        result.bridgeOrder = STC.Common.Utils.CommonUtils.DeserializeObject<bridgeOrder>(orderDetailPo.BridgeOrder);
            //    if (orderDetailPo.BridgeOrder2 != null)
            //        result.bridgeOrder2 = STC.Common.Utils.CommonUtils.DeserializeObject<bridgeOrder>(orderDetailPo.BridgeOrder2);
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("BridgeOrder||BridgeOrder2 DeserializeObject error:" + ex);
            //}
            //try
            //{
            //    if (orderDetailPo.InstExecOrderReqListExt != null)
            //    {
            //        BridgeOrderReq bridgeOrderReq = STC.Common.Utils.CommonUtils.DeserializeObject<BridgeOrderReq>(orderDetailPo.InstExecOrderReqListExt);
            //        if (bridgeOrderReq != null)
            //        {
            //            result.ofrIssueInfoReqList = bridgeOrderReq.ofrInstExecOrderReqList;
            //            result.bidIssueInfoReqList = bridgeOrderReq.bidInstExecOrderReqList;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("InstExecOrderReqListExt DeserializeObject error:" + ex);
            //}
        }


        public static BridgeOrderDetail CopyToEx2(TransOrderServerData orderDetailPo)
        {
            BridgeOrderDetail result = new BridgeOrderDetail();
            result.TraderID = orderDetailPo.TraderID;
            result.CounterPartyTraderId = orderDetailPo.CounterPartyTraderId;
            result.TraderName = orderDetailPo.TraderName;
            result.CounterPartyTraderName = orderDetailPo.CounterPartyTraderName;
            result.InstitutionName = orderDetailPo.InstitutionName;
            result.InstitutionShowName = orderDetailPo.InstitutionShowName;
            result.CounterPartyInstitutionName = orderDetailPo.CounterPartyInstitutionName;
            result.CounterPartyInstitutionShowName = orderDetailPo.CounterPartyInstitutionShowName;
            result.PartyName = orderDetailPo.PartyName;
            result.CounterPartyName = orderDetailPo.CounterPartyName;
            result.InstitutionID = orderDetailPo.InstitutionID;
            result.CounterPartyInstitutionId = orderDetailPo.CounterPartyInstitutionId;
            result.BargainFlag = orderDetailPo.BargainFlag;
            result.BridgeFlag = orderDetailPo.BridgeFlag;
            //result.brokerbConfirmQty = orderDetailPo.brokerbonfirmQty;
            result.PartySubID = orderDetailPo.PartySubID;
            result.CounterPartySubID = orderDetailPo.CounterPartySubID;
            result.PartySubName = orderDetailPo.PartySubName;
            result.CounterPartySubName = orderDetailPo.CounterPartySubName;
            //result.brokerreadflaga = orderDetailPo.brokerreadflaga;
            //result.brokerreadflagb = orderDetailPo.brokerreadflagb;
            result.BrokerUnread = orderDetailPo.BrokerUnread;
            result.closeby = orderDetailPo.CloseBy;
            result.CloseById = orderDetailPo.CloseById;
            result.CloseById2 = orderDetailPo.CloseById2;
            result.CompanyId = orderDetailPo.CompanyId;
            result.createTime = orderDetailPo.TransactTime;
            result.dcsflag = orderDetailPo.DCSFlag;
            result.dealdate = orderDetailPo.TradeDate;
            result.DealDate2 = orderDetailPo.TradeDate2;
            result.delflag = orderDetailPo.DelFlag;
            result.ExecID = orderDetailPo.ExecID;
            result.SecurityID = orderDetailPo.SecurityID;
            result.goodsShortName = orderDetailPo.Symbol;
            result.goodstype = orderDetailPo.SecurityType;
            result.goodsSubType = orderDetailPo.BondSubtype;
            //result.inf1 = orderDetailPo.inf1;
            //result.inf2 = orderDetailPo.inf2;
            //result.inf3 = orderDetailPo.inf3;
            //result.inf4 = orderDetailPo.inf4;
            result.InternalFlag = orderDetailPo.InternalFlag;
            decimal matchQty;
            if (decimal.TryParse(orderDetailPo.MatchQTY, out matchQty))
                result.matchQty = matchQty;
            result.OCOFlag = orderDetailPo.OCOFlag;
            result.OrderID = orderDetailPo.OrderID;
            result.OrdStatus = orderDetailPo.OrdStatus;
            decimal price;
            if (decimal.TryParse(orderDetailPo.Price, out price))
                result.price = price;
            result.priceDescription = orderDetailPo.Text;
            result.queryflag = orderDetailPo.QueryFlag;
            result.QuoteID = orderDetailPo.QuoteID;
            result.quoteInternalFlag = orderDetailPo.HandlInst;
            decimal quoteqty;
            if (decimal.TryParse(orderDetailPo.OrderQty, out quoteqty))
                result.quoteqty = quoteqty;
            decimal qty;
            if (decimal.TryParse(orderDetailPo.LastQty, out qty))
                result.qty = qty;
            result.rejecter = orderDetailPo.Rejecter;
            result.rejecterReason = orderDetailPo.RejectText;
            result.SendQQMsgFlag = orderDetailPo.SendQQMsgFlag;
            result.sendQQMsgTem = orderDetailPo.SendQQMsgTem;
            result.SendTo = orderDetailPo.SendTo;
            result.SendTo2 = orderDetailPo.SendTo2;
            result.seq = decimal.Parse(orderDetailPo.Sequence);
            result.SettlDate = orderDetailPo.SettlDate;
            result.SettlDate2 = orderDetailPo.SettlDate2;
            result.SettlType = orderDetailPo.SettlType;
            result.SettlType2 = orderDetailPo.SettlType2;
            result.Side = orderDetailPo.Side;
            result.WholeFlag = orderDetailPo.WholeFlag;
            result.timeToMaturity = orderDetailPo.TimeToMaturity;

            result.BridgeInstitutionName = orderDetailPo.BridgeInstitutionName;
            result.bridgebankcode = orderDetailPo.BridgeInstitutionFullName;
            result.BridgeInstitutionId = orderDetailPo.BridgeInstitutionId;
            result.BridgeTraderId = orderDetailPo.BridgeTraderId;
            result.BridgeTraderName = orderDetailPo.BridgeTraderName;
            result.BridgeChannel = orderDetailPo.BridgeChannel;
            result.BridgeSendTo = orderDetailPo.BridgeSendTo;
            result.BridgeFee = orderDetailPo.BridgeFee;
            result.ofrSpecialBridgeFee = orderDetailPo.OFRSpecialBridgeFee;
            result.BridgeRemark = orderDetailPo.BridgeRemark;
            result.BridgeType = orderDetailPo.BridgeType;

            result.BridgeInstitutionName2 = orderDetailPo.BridgeInstitutionName2;
            result.bridgebankcode2 = orderDetailPo.BridgeInstitutionFullName2;
            result.BridgeInstitutionId2 = orderDetailPo.BridgeInstitutionId2;
            result.BridgeTraderId2 = orderDetailPo.BridgeTraderId2;
            result.BridgeTraderName2 = orderDetailPo.BridgeTraderName2;
            result.BridgeChannel2 = orderDetailPo.BridgeChannel2;
            result.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
            result.BridgeFee2 = orderDetailPo.BridgeFee2;
            result.bidSpecialBridgeFee = orderDetailPo.BIDSpecialBridgeFee;
            result.BridgeRemark2 = orderDetailPo.BridgeRemark2;
            result.BridgeType2 = orderDetailPo.BridgeType2;
            result.BridgeStatus = orderDetailPo.BridgeStatus;
            result.BridgeCloseById = orderDetailPo.BridgeCloseById;

            result.BridgeCreateTime = orderDetailPo.BridgeCreateTime;
            result.BridgeCreateTime2 = orderDetailPo.BridgeCreateTime2;
            result.BridgeUpdateTime = orderDetailPo.BridgeUpdateTime;
            result.BridgeUpdateTime2 = orderDetailPo.BridgeUpdateTime2;

            result.DoubleBridgeBidSettltype = orderDetailPo.DoubleBridgeBidSettltype;
            result.DoubleBridgeChannel = orderDetailPo.DoubleBridgeChannel;
            result.DoubleBridgeFee = orderDetailPo.DoubleBridgeFee;
            result.DoubleBridgeOffSettltype = orderDetailPo.DoubleBridgeOffSettltype;
            result.DoubleBridgeRemark = orderDetailPo.DoubleBridgeRemark;
            result.DoubleBridgeSendTo = orderDetailPo.DoubleBridgeSendTo;

            result.fandian = orderDetailPo.FanDian;
            result.remarks = orderDetailPo.Remark;

            result.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
            result.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

            result.brokeragea = orderDetailPo.BrokerageA;
            result.brokerageb = orderDetailPo.BrokerageB;
            //V2.5 wxp 20200630 桥状态
            result.BorderId = orderDetailPo.BorderId;
            result.BorderId2 = orderDetailPo.BorderId2;
            result.bridgeOrder = orderDetailPo.BridgeOrderModel;
            result.bridgeOrder2 = orderDetailPo.BridgeOrderModel2;
            //合单备注
            result.combinedOrderRemark = orderDetailPo.CombinedOrderRemark;
            result.combinedOrderRemark2 = orderDetailPo.CombinedOrderRemark2;
            //2.9 双桥错期
            result.doubleBridgeSettleSide = orderDetailPo.DoubleBridgeSettleSide;

            //2.17.0 增加
            result.ofrIssueInfoReqList = orderDetailPo.OfrIssueInfoReqList;
            result.bidIssueInfoReqList = orderDetailPo.BidIssueInfoReqList;

            //v3.4.2 增加 2023-08-09
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;
            //try
            //{
            //    if (orderDetailPo.BridgeOrder != null)
            //        result.bridgeOrder = STC.Common.Utils.CommonUtils.DeserializeObject<bridgeOrder>(orderDetailPo.BridgeOrder);
            //    if (orderDetailPo.BridgeOrder2 != null)
            //        result.bridgeOrder2 = STC.Common.Utils.CommonUtils.DeserializeObject<bridgeOrder>(orderDetailPo.BridgeOrder2);
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("BridgeOrder||BridgeOrder2 DeserializeObject error:" + ex);
            //}
            //try
            //{
            //    if (orderDetailPo.InstExecOrderReqListExt != null)
            //    {
            //        BridgeOrderReq bridgeOrderReq = STC.Common.Utils.CommonUtils.DeserializeObject<BridgeOrderReq>(orderDetailPo.InstExecOrderReqListExt);
            //        if (bridgeOrderReq != null)
            //        {
            //            result.ofrIssueInfoReqList = bridgeOrderReq.ofrInstExecOrderReqList;
            //            result.bidIssueInfoReqList = bridgeOrderReq.bidInstExecOrderReqList;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error("InstExecOrderReqListExt DeserializeObject error:" + ex);
            //}
            return result;
        }

        public static BridgeOrderDetail CopyToEx(OrderDetailPo orderDetailPo)
        {
            BridgeOrderDetail result = new BridgeOrderDetail();
            result.TraderID = orderDetailPo.TraderID;
            result.CounterPartyTraderId = orderDetailPo.CounterPartyTraderId;
            result.TraderName = orderDetailPo.TraderName;
            result.CounterPartyTraderName = orderDetailPo.CounterPartyTraderName;
            result.InstitutionName = orderDetailPo.InstitutionName;
            result.CounterPartyInstitutionName = orderDetailPo.CounterPartyInstitutionName;
            result.PartyName = orderDetailPo.PartyName;
            result.CounterPartyName = orderDetailPo.CounterPartyName;
            result.InstitutionID = orderDetailPo.InstitutionID;
            result.CounterPartyInstitutionId = orderDetailPo.CounterPartyInstitutionId;
            result.BargainFlag = orderDetailPo.BargainFlag;
            result.BridgeFlag = orderDetailPo.BridgeFlag;
            result.brokerbConfirmQty = orderDetailPo.brokerbConfirmQty;
            result.PartySubID = orderDetailPo.PartySubID;
            result.CounterPartySubID = orderDetailPo.CounterPartySubID;
            result.PartySubName = orderDetailPo.PartySubName;
            result.CounterPartySubName = orderDetailPo.CounterPartySubName;
            result.brokerreadflaga = orderDetailPo.brokerreadflaga;
            result.brokerreadflagb = orderDetailPo.brokerreadflagb;
            result.BrokerUnread = orderDetailPo.BrokerUnread;
            result.closeby = orderDetailPo.closeby;
            result.CloseById = orderDetailPo.CloseById;
            result.CloseById2 = orderDetailPo.CloseById2;
            result.CompanyId = orderDetailPo.CompanyId;
            result.createTime = orderDetailPo.createTime;
            result.dcsflag = orderDetailPo.dcsflag;
            result.dealdate = orderDetailPo.dealdate;
            result.DealDate2 = orderDetailPo.DealDate2;
            result.delflag = orderDetailPo.delflag;
            result.ExecID = orderDetailPo.ExecID;
            result.SecurityID = orderDetailPo.SecurityID;
            result.goodsShortName = orderDetailPo.goodsShortName;
            result.goodstype = orderDetailPo.goodstype;
            result.goodsSubType = orderDetailPo.goodsSubType;
            result.inf1 = orderDetailPo.inf1;
            result.inf2 = orderDetailPo.inf2;
            result.inf3 = orderDetailPo.inf3;
            result.inf4 = orderDetailPo.inf4;
            result.InternalFlag = orderDetailPo.InternalFlag;
            result.matchQty = orderDetailPo.matchQty;
            result.OCOFlag = orderDetailPo.OCOFlag;
            result.OrderID = orderDetailPo.OrderID;
            result.OrdStatus = orderDetailPo.OrdStatus;
            result.price = orderDetailPo.price;
            result.priceDescription = orderDetailPo.priceDescription;
            result.queryflag = orderDetailPo.queryflag;
            result.QuoteID = orderDetailPo.QuoteID;
            result.quoteInternalFlag = orderDetailPo.quoteInternalFlag;
            result.quoteqty = orderDetailPo.quoteqty;
            result.qty = orderDetailPo.qty;
            result.rejecter = orderDetailPo.rejecter;
            result.rejecterReason = orderDetailPo.rejecterReason;
            result.SendQQMsgFlag = orderDetailPo.SendQQMsgFlag;
            result.sendQQMsgTem = orderDetailPo.sendQQMsgTem;
            result.SendTo = orderDetailPo.SendTo;
            result.SendTo2 = orderDetailPo.SendTo2;
            result.seq = orderDetailPo.seq;
            result.SettlDate = orderDetailPo.SettlDate;
            result.SettlDate2 = orderDetailPo.SettlDate2;
            result.SettlType = orderDetailPo.SettlType;
            result.SettlType2 = orderDetailPo.SettlType2;
            result.Side = orderDetailPo.Side;
            result.WholeFlag = orderDetailPo.WholeFlag;
            result.timeToMaturity = orderDetailPo.timeToMaturity;

            result.BridgeInstitutionName = orderDetailPo.BridgeInstitutionName;
            result.bridgebankcode = orderDetailPo.bridgebankcode;
            result.BridgeInstitutionId = orderDetailPo.BridgeInstitutionId;
            result.BridgeTraderId = orderDetailPo.BridgeTraderId;
            result.BridgeTraderName = orderDetailPo.BridgeTraderName;
            result.BridgeChannel = orderDetailPo.BridgeChannel;
            result.BridgeSendTo = orderDetailPo.BridgeSendTo;
            result.BridgeFee = orderDetailPo.BridgeFee;
            result.ofrSpecialBridgeFee = orderDetailPo.ofrSpecialBridgeFee;
            result.BridgeRemark = orderDetailPo.BridgeRemark;
            result.BridgeType = orderDetailPo.BridgeType;

            result.BridgeInstitutionName2 = orderDetailPo.BridgeInstitutionName2;
            result.bridgebankcode2 = orderDetailPo.bridgebankcode2;
            result.BridgeInstitutionId2 = orderDetailPo.BridgeInstitutionId2;
            result.BridgeTraderId2 = orderDetailPo.BridgeTraderId2;
            result.BridgeTraderName2 = orderDetailPo.BridgeTraderName2;
            result.BridgeChannel2 = orderDetailPo.BridgeChannel2;
            result.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
            result.BridgeFee2 = orderDetailPo.BridgeFee2;
            result.bidSpecialBridgeFee = orderDetailPo.bidSpecialBridgeFee;
            result.BridgeRemark2 = orderDetailPo.BridgeRemark2;
            result.BridgeType2 = orderDetailPo.BridgeType2;

            result.BridgeStatus = orderDetailPo.BridgeStatus;
            result.BridgeCloseById = orderDetailPo.BridgeCloseById;

            result.BridgeCreateTime = orderDetailPo.BridgeCreateTime;
            result.BridgeCreateTime2 = orderDetailPo.BridgeCreateTime2;
            result.BridgeUpdateTime = orderDetailPo.BridgeUpdateTime;
            result.BridgeUpdateTime2 = orderDetailPo.BridgeUpdateTime2;

            result.DoubleBridgeBidSettltype = orderDetailPo.DoubleBridgeBidSettltype;
            result.DoubleBridgeChannel = orderDetailPo.DoubleBridgeChannel;
            result.DoubleBridgeFee = orderDetailPo.DoubleBridgeFee;
            result.DoubleBridgeOffSettltype = orderDetailPo.DoubleBridgeOffSettltype;
            result.DoubleBridgeRemark = orderDetailPo.DoubleBridgeRemark;
            result.DoubleBridgeSendTo = orderDetailPo.DoubleBridgeSendTo;

            result.fandian = orderDetailPo.fandian;
            result.remarks = orderDetailPo.remarks;

            result.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
            result.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;
            result.brokeragea = orderDetailPo.brokeragea;
            result.brokerageb = orderDetailPo.brokerageb;

            //v2.5 wxp 20200629 加桥订单
            result.bridgeOrder = orderDetailPo.bridgeOrder;
            result.bridgeOrder2 = orderDetailPo.bridgeOrder2;
            result.BorderId = orderDetailPo.BorderId;
            result.BorderId2 = orderDetailPo.BorderId2;
            //合单备注
            result.combinedOrderRemark = orderDetailPo.combinedOrderRemark;
            result.combinedOrderRemark2 = orderDetailPo.combinedOrderRemark2;
            //2.9 双桥错期
            result.doubleBridgeSettleSide = orderDetailPo.doubleBridgeSettleSide;

            //2.17.0 增加
            result.ofrIssueInfoReqList = orderDetailPo.ofrIssueInfoReqList;
            result.bidIssueInfoReqList = orderDetailPo.bidIssueInfoReqList;

            //v3.4.2 增加 2023-08-09
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;
            return result;
        }


        public static BridgeOrderDetail CopyBridgeOrderDetailo(BridgeOrderDetail bridgeOrderDetail)
        {
            BridgeOrderDetail bridgeOrder = new BridgeOrderDetail();
            try
            {
                bridgeOrder = MsgHelper.Deserialize<BridgeOrderDetail>(MsgHelper.Serializer(bridgeOrderDetail));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return bridgeOrder;
        }

        public static OrderDetailPo CopyOrderDetailPo(OrderDetailPo orderDetailPo)
        {
            OrderDetailPo result = new OrderDetailPo();
            result.TraderID = orderDetailPo.TraderID;
            result.CounterPartyTraderId = orderDetailPo.CounterPartyTraderId;
            result.TraderName = orderDetailPo.TraderName;
            result.CounterPartyTraderName = orderDetailPo.CounterPartyTraderName;
            result.InstitutionName = orderDetailPo.InstitutionName;
            result.CounterPartyInstitutionName = orderDetailPo.CounterPartyInstitutionName;
            result.PartyName = orderDetailPo.PartyName;
            result.CounterPartyName = orderDetailPo.CounterPartyName;
            result.InstitutionID = orderDetailPo.InstitutionID;
            result.CounterPartyInstitutionId = orderDetailPo.CounterPartyInstitutionId;
            result.BargainFlag = orderDetailPo.BargainFlag;
            result.BridgeFlag = orderDetailPo.BridgeFlag;
            result.brokerbConfirmQty = orderDetailPo.brokerbConfirmQty;
            result.PartySubID = orderDetailPo.PartySubID;
            result.CounterPartySubID = orderDetailPo.CounterPartySubID;
            result.PartySubName = orderDetailPo.PartySubName;
            result.CounterPartySubName = orderDetailPo.CounterPartySubName;
            result.brokerreadflaga = orderDetailPo.brokerreadflaga;
            result.brokerreadflagb = orderDetailPo.brokerreadflagb;
            result.BrokerUnread = orderDetailPo.BrokerUnread;
            result.closeby = orderDetailPo.closeby;
            result.CloseById = orderDetailPo.CloseById;
            result.CloseById2 = orderDetailPo.CloseById2;
            result.CompanyId = orderDetailPo.CompanyId;
            result.createTime = orderDetailPo.createTime;
            result.dcsflag = orderDetailPo.dcsflag;
            result.dealdate = orderDetailPo.dealdate;
            result.DealDate2 = orderDetailPo.DealDate2;
            result.delflag = orderDetailPo.delflag;
            result.ExecID = orderDetailPo.ExecID;
            result.SecurityID = orderDetailPo.SecurityID;
            result.goodsShortName = orderDetailPo.goodsShortName;
            result.goodsSubType = orderDetailPo.goodsSubType;
            result.goodstype = orderDetailPo.goodstype;
            result.inf1 = orderDetailPo.inf1;
            result.inf2 = orderDetailPo.inf2;
            result.inf3 = orderDetailPo.inf3;
            result.inf4 = orderDetailPo.inf4;
            result.InternalFlag = orderDetailPo.InternalFlag;
            result.matchQty = orderDetailPo.matchQty;
            result.OCOFlag = orderDetailPo.OCOFlag;
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;
            result.OrderID = orderDetailPo.OrderID;
            result.OrdStatus = orderDetailPo.OrdStatus;
            result.price = orderDetailPo.price;
            result.priceDescription = orderDetailPo.priceDescription;
            result.queryflag = orderDetailPo.queryflag;
            result.QuoteID = orderDetailPo.QuoteID;
            result.quoteInternalFlag = orderDetailPo.quoteInternalFlag;
            result.quoteqty = orderDetailPo.quoteqty;
            result.qty = orderDetailPo.qty;
            result.rejecter = orderDetailPo.rejecter;
            result.rejecterReason = orderDetailPo.rejecterReason;
            result.SendQQMsgFlag = orderDetailPo.SendQQMsgFlag;
            result.sendQQMsgTem = orderDetailPo.sendQQMsgTem;
            result.SendTo = orderDetailPo.SendTo;
            result.SendTo2 = orderDetailPo.SendTo2;
            result.seq = orderDetailPo.seq;
            result.SettlDate = orderDetailPo.SettlDate;
            result.SettlDate2 = orderDetailPo.SettlDate2;
            result.SettlType = orderDetailPo.SettlType;
            result.SettlType2 = orderDetailPo.SettlType2;
            result.Side = orderDetailPo.Side;
            result.WholeFlag = orderDetailPo.WholeFlag;
            result.timeToMaturity = orderDetailPo.timeToMaturity;

            result.BridgeInstitutionName = orderDetailPo.BridgeInstitutionName;
            result.bridgebankcode = orderDetailPo.bridgebankcode;
            result.BridgeInstitutionId = orderDetailPo.BridgeInstitutionId;
            result.BridgeTraderId = orderDetailPo.BridgeTraderId;
            result.BridgeTraderName = orderDetailPo.BridgeTraderName;
            result.BridgeChannel = orderDetailPo.BridgeChannel;
            result.BridgeSendTo = orderDetailPo.BridgeSendTo;
            result.BridgeFee = orderDetailPo.BridgeFee;
            result.ofrSpecialBridgeFee = orderDetailPo.ofrSpecialBridgeFee;
            result.BridgeRemark = orderDetailPo.BridgeRemark;
            result.BridgeType = orderDetailPo.BridgeType;

            result.BridgeInstitutionName2 = orderDetailPo.BridgeInstitutionName2;
            result.bridgebankcode2 = orderDetailPo.bridgebankcode2;
            result.BridgeInstitutionId2 = orderDetailPo.BridgeInstitutionId2;
            result.BridgeTraderId2 = orderDetailPo.BridgeTraderId2;
            result.BridgeTraderName2 = orderDetailPo.BridgeTraderName2;
            result.BridgeChannel2 = orderDetailPo.BridgeChannel2;
            result.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
            result.BridgeFee2 = orderDetailPo.BridgeFee2;
            result.bidSpecialBridgeFee = orderDetailPo.bidSpecialBridgeFee;
            result.BridgeRemark2 = orderDetailPo.BridgeRemark2;
            result.BridgeType2 = orderDetailPo.BridgeType2;

            result.BridgeStatus = orderDetailPo.BridgeStatus;
            result.BridgeCloseById = orderDetailPo.BridgeCloseById;

            result.BridgeCreateTime = orderDetailPo.BridgeCreateTime;
            result.BridgeCreateTime2 = orderDetailPo.BridgeCreateTime2;
            result.BridgeUpdateTime = orderDetailPo.BridgeUpdateTime;
            result.BridgeUpdateTime2 = orderDetailPo.BridgeUpdateTime2;

            result.DoubleBridgeBidSettltype = orderDetailPo.DoubleBridgeBidSettltype;
            result.DoubleBridgeChannel = orderDetailPo.DoubleBridgeChannel;
            result.DoubleBridgeFee = orderDetailPo.DoubleBridgeFee;
            result.DoubleBridgeOffSettltype = orderDetailPo.DoubleBridgeOffSettltype;
            result.DoubleBridgeRemark = orderDetailPo.DoubleBridgeRemark;
            result.DoubleBridgeSendTo = orderDetailPo.DoubleBridgeSendTo;

            result.fandian = orderDetailPo.fandian;
            result.remarks = orderDetailPo.remarks;

            result.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
            result.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;
            result.brokeragea = orderDetailPo.brokeragea;
            result.brokerageb = orderDetailPo.brokerageb;
            //v2.5 wxp 20200629 加桥订单
            result.bridgeOrder = orderDetailPo.bridgeOrder;
            result.bridgeOrder2 = orderDetailPo.bridgeOrder2;
            result.BorderId = orderDetailPo.BorderId;
            result.BorderId2 = orderDetailPo.BorderId2;
            //合单备注
            result.combinedOrderRemark = orderDetailPo.combinedOrderRemark;
            result.combinedOrderRemark2 = orderDetailPo.combinedOrderRemark2;
            //2.6 债券信息
            result.BondType = orderDetailPo.BondType;
            result.BondSubtype = orderDetailPo.BondSubtype;
            result.ListedMarket = orderDetailPo.ListedMarket;
            result.IssueAmount = orderDetailPo.IssueAmount;
            result.IssuerRatingCurrent = orderDetailPo.IssuerRatingCurrent;
            result.IssuerCode = orderDetailPo.IssuerCode;
            result.EntCor = orderDetailPo.EntCor;
            //2.9 双桥错期
            result.doubleBridgeSettleSide = orderDetailPo.doubleBridgeSettleSide;
            //2.10
            result.onlinea = orderDetailPo.onlinea;
            result.onlineb = orderDetailPo.onlineb;
            //2.11
            result.agreementNo = orderDetailPo.agreementNo;
            //2.12
            result.isConfirmOperation = orderDetailPo.isConfirmOperation;
            //2.13.2
            result.FRNIndexID = orderDetailPo.FRNIndexID;
            //2.13增加
            result.forbidRefreshFlag = orderDetailPo.forbidRefreshFlag;
            //2.16增加
            result.sendSorFlaga = orderDetailPo.sendSorFlaga;
            result.sendSorFlagb = orderDetailPo.sendSorFlagb;

            //2.17.0 增加
            result.ofrIssueInfoReqList = orderDetailPo.ofrIssueInfoReqList;
            result.bidIssueInfoReqList = orderDetailPo.bidIssueInfoReqList;
            result.flagInner = orderDetailPo.flagInner;
            //2.20.2 增加
            result.ProcessStatus = orderDetailPo.ProcessStatus;
            result.FailedReason = orderDetailPo.FailedReason;

            //3.0.0
            result.issueDir = orderDetailPo.issueDir;
            result.issueChannel = orderDetailPo.issueChannel;
            result.issueInstId = orderDetailPo.issueInstId;
            result.issueInstName = orderDetailPo.issueInstName;
            result.issueInstNameShort = orderDetailPo.issueInstNameShort;
            result.issueTraderId = orderDetailPo.issueTraderId;
            result.issueTraderName = orderDetailPo.issueTraderName;
            result.issueRemark = orderDetailPo.issueRemark;
            result.issueDesc = orderDetailPo.issueDesc;
            result.proxyInstId = orderDetailPo.proxyInstId;
            result.proxyFee = orderDetailPo.proxyFee;
            result.proxyFlag = orderDetailPo.proxyFlag;
            result.issueState = orderDetailPo.issueState;
            result.issueShowType = orderDetailPo.issueShowType;

            //3.3.0
            result.SingleQQMsgSendFlaga = orderDetailPo.SingleQQMsgSendFlaga;
            result.SingleQQMsgSendFlagb = orderDetailPo.SingleQQMsgSendFlagb;

            //v3.4.2 增加 2023-08-09
            result.ExchangeSignFlag = orderDetailPo.ExchangeSignFlag;
            //v3.5.. 增加 2023-10-20
            result.DetailMark = orderDetailPo.DetailMark;
            result.AssociatedFlag = orderDetailPo.AssociatedFlag;
            return result;
        }
        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <param name="orderPo"></param>
        /// <returns></returns>
        public static OrderPo CopyOrderPo(OrderPo orderPo)
        {
            OrderPo result = new OrderPo();
            result.bankAgentCode = orderPo.bankAgentCode;
            result.bankAgentQbId = orderPo.bankAgentQbId;
            result.bankCode = orderPo.bankCode;
            result.bankQbCode = orderPo.bankQbCode;
            result.bankQbId = orderPo.bankQbId;
            result.brokerid = orderPo.brokerid;
            result.brokername = orderPo.brokername;
            result.cancelledQty = orderPo.cancelledQty;
            result.cid = orderPo.cid;
            result.clientOrderId = orderPo.clientOrderId;
            result.companyId = orderPo.companyId;
            result.createTime = orderPo.createTime;
            result.execOrders = new List<OrderDetailPo>();

            foreach (OrderDetailPo od in orderPo.execOrders)
                result.execOrders.Add(CopyOrderDetailPo(od));

            result.executionQty = orderPo.executionQty;
            result.goodsCode = orderPo.goodsCode;
            result.goodsName = orderPo.goodsName;
            result.id = orderPo.id;
            result.inf1 = orderPo.inf1;
            result.inf2 = orderPo.inf2;
            result.matchQty = orderPo.matchQty;
            result.name = orderPo.name;
            result.online = orderPo.online;
            result.orderStatus = orderPo.orderStatus;
            result.orderType = orderPo.orderType;
            result.price = orderPo.price;
            result.qty = orderPo.qty;
            result.rejecter = orderPo.rejecter;
            result.rejecterReason = orderPo.rejecterReason;
            result.sendmsgflag = orderPo.sendmsgflag;
            result.settlementType = orderPo.settlementType;
            result.side = orderPo.side;
            result.submitClientQty = orderPo.submitClientQty;
            result.terminal = orderPo.terminal;
            result.tif = orderPo.tif;
            result.time = orderPo.time;
            result.updateTime = orderPo.updateTime;
            result.userName = orderPo.userName;

            return result;
        }

        /// <summary>
        /// 把原始报价转换成即期报价或者组合报价
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="onlyShowSpot"></param>
        /// <returns></returns>
        public static GoodsPriceVo ConvertTo(GoodsPricePo obj, bool onlyShowSpot)
        {
            GoodsPriceVo vo = new GoodsPriceVo();
            vo.goodscode = obj.goodscode;
            vo.timetomaturityreal = obj.timetomaturityreal;
            vo.SuboptimalOffer = new List<GoodsVolumeVo>();
            vo.OptimalOffer = new List<GoodsVolumeVo>();
            vo.OptimalBid = new List<GoodsVolumeVo>();
            vo.SuboptimalBid = new List<GoodsVolumeVo>();

            //20211221  v2.16.0版本
            vo.SpotZeroBid = new List<GoodsVolumeVo>();
            vo.SpotFirstBid = new List<GoodsVolumeVo>();
            vo.SpotSecondBid = new List<GoodsVolumeVo>();
            //买方
            vo.SpotZeroOfr = new List<GoodsVolumeVo>();
            vo.SpotFirstOfr = new List<GoodsVolumeVo>();
            vo.SpotSecondOfr = new List<GoodsVolumeVo>();

            //20220222
            vo.AtSightBidPriceList = new List<GoodsVolumeVo>();
            vo.AtSightOfferPriceList = new List<GoodsVolumeVo>();


            if (obj != null && obj.SpotAllBid != null && obj.SpotAllBid.Count > 0)
            {
                foreach (GoodsVolumePo item in obj.SpotAllBid)
                {
                    GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                    vvo.Direction = EDirection.Bid;
                    vo.AtSightBidPriceList.Add(vvo);
                }
            }
            if (obj != null && obj.SpotAllOfr != null && obj.SpotAllOfr.Count > 0)
            {
                foreach (GoodsVolumePo item in obj.SpotAllOfr)
                {
                    GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                    vvo.Direction = EDirection.Offer;
                    vo.AtSightOfferPriceList.Add(vvo);
                }
            }

            // 明盘报价
            const string OpenQuote = "1";

            // 显示远期窗口
            if (onlyShowSpot)
            {
                // 买价列表
                if (obj.bid != null && obj.bid.Count > 0)
                {
                    decimal allOptimalBidPrice;
                    // 包含暗盘最优买价
                    if (obj.isCleanPrice)
                    {
                        allOptimalBidPrice = obj.bid.Max(x => x.price);
                    }
                    else
                    {
                        allOptimalBidPrice = obj.bid.Min(x => x.price);
                    }

                    var temp = obj.bid.FindAll(x => x.price == allOptimalBidPrice);
                    temp.Sort();

                    foreach (var item in temp)
                    {
                        GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                        vvo.Direction = EDirection.Bid;
                        vo.AllOptimalBid.Add(vvo);
                    }

                    // 明盘最优买报价
                    var openList = obj.bid.FindAll(x => x.quoteInternalFlag == OpenQuote);
                    if (openList != null && openList.Count > 0)
                    {
                        decimal openOptimalBidPrice;
                        if (obj.isCleanPrice)
                        {
                            openOptimalBidPrice = openList.Max(x => x.price);
                        }
                        else
                        {
                            openOptimalBidPrice = openList.Min(x => x.price);
                        }

                        var openTemp = openList.FindAll(x => x.price == openOptimalBidPrice);
                        openTemp.Sort();
                        foreach (var item in openTemp)
                        {
                            GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                            vvo.Direction = EDirection.Bid;
                            vo.OpenOptimalBid.Add(vvo);
                        }
                    }

                }

                // 卖价列表
                if (obj.ofr != null && obj.ofr.Count > 0)
                {
                    decimal allOptimalOfferPrice;
                    // 包含暗盘最优卖价
                    if (obj.isCleanPrice)
                    {
                        allOptimalOfferPrice = obj.ofr.Min(x => x.price);
                    }
                    else
                    {
                        allOptimalOfferPrice = obj.ofr.Max(x => x.price);
                    }
                    var temp = obj.ofr.FindAll(x => x.price == allOptimalOfferPrice);
                    temp.Sort();

                    // 包含暗盘最优卖价列表
                    foreach (var item in temp)
                    {
                        GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                        vvo.Direction = EDirection.Offer;
                        vvo.isBestPrice = true;
                        vo.AllOptimalOffer.Add(vvo);
                    }

                    // 明盘最优买报价
                    var openList = obj.ofr.FindAll(x => x.quoteInternalFlag == OpenQuote);
                    if (openList != null && openList.Count > 0)
                    {
                        decimal openOptimalOfferPrice;
                        if (obj.isCleanPrice)
                        {
                            openOptimalOfferPrice = openList.Min(x => x.price);
                        }
                        else
                        {
                            openOptimalOfferPrice = openList.Max(x => x.price);
                        }
                        var openTemp = openList.FindAll(x => x.price == openOptimalOfferPrice);
                        openTemp.Sort();

                        foreach (var item in openTemp)
                        {
                            GoodsVolumeVo vvo = ConvertTo(item, onlyShowSpot);
                            vvo.Direction = EDirection.Offer;
                            vvo.isBestPrice = true;
                            vo.OpenOptimalOffer.Add(vvo);
                        }
                    }
                }

                SetSpotPriceDisplay(obj, vo, onlyShowSpot);
            }
            else
            {
                if (obj.bid != null)
                {
                    List<decimal> bidps;
                    //查询最优和次优Bid
                    if (obj.isCleanPrice)
                    {
                        bidps = (from a in obj.bid select a.price).Distinct().OrderByDescending(i => i).ToList();
                    }
                    else
                    {
                        bidps = (from a in obj.bid select a.price).Distinct().OrderBy(i => i).ToList();
                    }

                    if (bidps.Count > 0)
                        vo.OptimalBidPrice = bidps[0];
                    if (bidps.Count > 1)
                        vo.SuboptimalBidPrice = bidps[1];

                    //时间优先
                    //obj.bid.Sort((x, y) => y.time.CompareTo(x.time));
                    obj.bid.Sort();

                    foreach (GoodsVolumePo vpo in obj.bid)
                    {
                        if (vpo.price == vo.OptimalBidPrice)
                        {
                            GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                            vvo.Direction = EDirection.Bid;// vvo.type = 1;
                            vvo.isBestPrice = true;
                            vo.OptimalBid.Add(vvo);
                        }
                        else if (vpo.price == vo.SuboptimalBidPrice)
                        {
                            GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                            vvo.Direction = EDirection.Bid;//vvo.type = 1;
                            vo.SuboptimalBid.Add(vvo);
                        }
                    }
                }

                if (obj.ofr != null)
                {
                    List<decimal> ofrps;
                    //查询最优和次优ofr
                    if (obj.isCleanPrice)
                    {
                        ofrps = (from a in obj.ofr select a.price).Distinct().OrderBy(i => i).ToList();
                    }
                    else
                    {
                        ofrps = (from a in obj.ofr select a.price).Distinct().OrderByDescending(i => i).ToList();
                    }

                    if (ofrps.Count > 0)
                        vo.OptimalOfferPrice = ofrps[0];
                    if (ofrps.Count > 1)
                        vo.SuboptimalOfferPrice = ofrps[1];

                    //时间优先
                    //obj.ofr.Sort((x, y) => y.time.CompareTo(x.time));
                    obj.ofr.Sort();
                    foreach (GoodsVolumePo vpo in obj.ofr)
                    {
                        if (vpo.price == vo.OptimalOfferPrice)
                        {
                            GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                            vvo.Direction = EDirection.Offer;//vvo.type = 0;
                            vvo.isBestPrice = true;
                            vo.OptimalOffer.Add(vvo);
                        }
                        else if (vpo.price == vo.SuboptimalOfferPrice)
                        {
                            GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                            vvo.Direction = EDirection.Offer;//vvo.type = 0;
                            vo.SuboptimalOffer.Add(vvo);
                        }
                    }
                }
            }

            SetZeroGoodsPrice(obj, vo, onlyShowSpot);

            return vo;
        }



        /// <summary>
        /// 设置+0报价信息
        /// </summary>
        private static void SetZeroGoodsPrice(GoodsPricePo obj, GoodsPriceVo vo, bool onlyShowSpot)
        {
            List<GoodsVolumePo> bidSpot = new List<GoodsVolumePo>();
            List<GoodsVolumePo> ofrSpot = new List<GoodsVolumePo>();
            List<GoodsVolumePo> bidSpotNotZero = new List<GoodsVolumePo>();
            List<GoodsVolumePo> ofrSpotNotZero = new List<GoodsVolumePo>();
            if (obj.spotHashMap != null)
            {
                GoodsPricePo pricePo;
                if (obj.spotHashMap.TryGetValue("+0", out pricePo))
                {
                    if (pricePo != null)
                    {
                        bidSpot = pricePo.bid;
                        ofrSpot = pricePo.ofr;
                    }
                }

                GoodsPricePo pricePo2;
                if (obj.spotHashMap.TryGetValue("+1,明天+0", out pricePo2))
                {
                    if (pricePo != null)
                    {
                        bidSpotNotZero = pricePo2.bid;
                        ofrSpotNotZero = pricePo2.ofr;
                    }
                }
            }



            if (onlyShowSpot)
            {
                // 买方报价表
                if (bidSpot != null)
                {
                    //即期+0买方报价
                    List<GoodsVolumePo> spotZeroBid = bidSpot.Where(_ => _.settlementType.Contains("+0")).ToList();
                    if (spotZeroBid != null && spotZeroBid.Count > 0)
                    {
                        List<decimal> bidZerops;
                        //查询最优Bid
                        if (obj.isCleanPrice)
                        {
                            bidZerops = (from a in spotZeroBid select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }
                        else
                        {
                            bidZerops = (from a in spotZeroBid select a.price).Distinct().OrderBy(i => i).ToList();
                        }

                        if (bidZerops.Count > 0)
                        {
                            vo.SpotZeroBidPrice = bidZerops[0];
                        }

                        //时间优先
                        spotZeroBid.Sort();
                        foreach (GoodsVolumePo vpo in spotZeroBid)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotZeroBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotZeroBid.Add(vvo);
                            }
                        }

                    }
                    //即期非+0买方报价
                    List<GoodsVolumePo> spotNotZeroBid = bidSpotNotZero.ToList();

                    if (spotNotZeroBid != null && spotNotZeroBid.Count > 0)
                    {
                        List<decimal> bidNotZerops;
                        if (obj.isCleanPrice)
                        {
                            //查询最优Bid
                            bidNotZerops = (from a in spotNotZeroBid select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }
                        else
                        {
                            //查询最优Bid
                            bidNotZerops = (from a in spotNotZeroBid select a.price).Distinct().OrderBy(i => i).ToList();
                        }

                        if (bidNotZerops.Count > 0)
                        {
                            vo.SpotFirstBidPrice = bidNotZerops[0];
                        }
                        if (bidNotZerops.Count > 1)
                        {
                            vo.SpotSecondBidPrice = bidNotZerops[1];
                        }
                        spotNotZeroBid.Sort();
                        foreach (GoodsVolumePo vpo in spotNotZeroBid)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotFirstBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotFirstBid.Add(vvo);
                            }
                            else if (vpo.price == vo.SpotSecondBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 0;
                                vo.SpotSecondBid.Add(vvo);
                            }
                        }

                    }
                }

                if (ofrSpot != null)
                {
                    //即期+0卖方报价
                    List<GoodsVolumePo> spotZeroOfr = ofrSpot.Where(_ => _.settlementType.Contains("+0")).ToList();

                    if (spotZeroOfr != null && spotZeroOfr.Count > 0)
                    {
                        List<decimal> ofrZerops;
                        if (obj.isCleanPrice)
                        {
                            //查询最优Bid
                            ofrZerops = (from a in spotZeroOfr select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        else
                        {
                            //查询最优Bid
                            ofrZerops = (from a in spotZeroOfr select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }

                        if (ofrZerops.Count > 0)
                        {
                            vo.SpotZeroOfrPrice = ofrZerops[0];
                        }

                        //时间优先
                        spotZeroOfr.Sort();
                        foreach (GoodsVolumePo vpo in spotZeroOfr)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotZeroOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Offer;//vvo.type = 0;
                                vvo.isBestPrice = true;
                                vo.SpotZeroOfr.Add(vvo);
                            }
                        }

                    }

                    //即期非+0买方报价
                    List<GoodsVolumePo> spotNotZeroOfr = ofrSpotNotZero.ToList();

                    if (spotNotZeroOfr != null && spotNotZeroOfr.Count > 0)
                    {
                        List<decimal> ofrNotZerops;
                        if (obj.isCleanPrice)
                        {
                            //查询最优Bid
                            ofrNotZerops = (from a in spotNotZeroOfr select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        else
                        {
                            //查询最优Bid
                            ofrNotZerops = (from a in spotNotZeroOfr select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }

                        if (ofrNotZerops.Count > 0)
                        {
                            vo.SpotFirstOfrPrice = ofrNotZerops[0];
                        }
                        if (ofrNotZerops.Count > 1)
                        {
                            vo.SpotSecondOfrPrice = ofrNotZerops[1];
                        }

                        spotNotZeroOfr.Sort();
                        foreach (GoodsVolumePo vpo in spotNotZeroOfr)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotFirstOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotFirstOfr.Add(vvo);
                            }
                            else if (vpo.price == vo.SpotSecondOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 0;
                                vo.SpotSecondOfr.Add(vvo);
                            }
                        }

                    }

                }
            }
            else
            {
                if (obj.bid != null)
                {
                    //即期+0买方报价
                    List<GoodsVolumePo> spotZeroBid = obj.bid.Where(_ => _.settlementType.Contains("+0")).ToList();
                    if (spotZeroBid != null && spotZeroBid.Count > 0)
                    {
                        List<decimal> bidZerops;
                        if (obj.isCleanPrice)
                        {
                            bidZerops = (from a in spotZeroBid select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }
                        else
                        {
                            bidZerops = (from a in spotZeroBid select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        //查询最优Bid

                        if (bidZerops.Count > 0)
                        {
                            vo.SpotZeroBidPrice = bidZerops[0];
                        }

                        //时间优先
                        spotZeroBid.Sort();
                        foreach (GoodsVolumePo vpo in spotZeroBid)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotZeroBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotZeroBid.Add(vvo);
                            }
                        }

                    }
                    //即期非+0买方报价
                    List<GoodsVolumePo> spotNotZeroBid = obj.bid.Where(_ => _.SettlementDisplay != "+0").ToList();

                    if (spotNotZeroBid != null && spotNotZeroBid.Count > 0)
                    {
                        List<decimal> bidNotZerops;
                        if (obj.isCleanPrice)
                        {
                            bidNotZerops = (from a in spotNotZeroBid select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }
                        else
                        {
                            bidNotZerops = (from a in spotNotZeroBid select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        //查询最优Bid

                        if (bidNotZerops.Count > 0)
                        {
                            vo.SpotFirstBidPrice = bidNotZerops[0];
                        }
                        if (bidNotZerops.Count > 1)
                        {
                            vo.SpotSecondBidPrice = bidNotZerops[1];
                        }
                        spotNotZeroBid.Sort();
                        foreach (GoodsVolumePo vpo in spotNotZeroBid)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotFirstBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotFirstBid.Add(vvo);
                            }
                            else if (vpo.price == vo.SpotSecondBidPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Bid;//vvo.type = 0;
                                vo.SpotSecondBid.Add(vvo);
                            }
                        }

                    }
                }

                if (obj.ofr != null)
                {
                    //即期+0卖方报价
                    List<GoodsVolumePo> spotZeroOfr = obj.ofr.Where(_ => _.settlementType.Contains("+0")).ToList();

                    if (spotZeroOfr != null && spotZeroOfr.Count > 0)
                    {
                        List<decimal> ofrZerops;
                        if (obj.isCleanPrice)
                        {
                            //查询最优Bid
                            ofrZerops = (from a in spotZeroOfr select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        else
                        {
                            //查询最优Bid
                            ofrZerops = (from a in spotZeroOfr select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }

                        if (ofrZerops.Count > 0)
                        {
                            vo.SpotZeroOfrPrice = ofrZerops[0];
                        }

                        //时间优先
                        spotZeroOfr.Sort();
                        foreach (GoodsVolumePo vpo in spotZeroOfr)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotZeroOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Offer;//vvo.type = 0;
                                vvo.isBestPrice = true;
                                vo.SpotZeroOfr.Add(vvo);
                            }
                        }
                    }

                    //即期非+0买方报价
                    List<GoodsVolumePo> spotNotZeroOfr = obj.ofr.Where(_ => _.SettlementDisplay != "+0").ToList();

                    if (spotNotZeroOfr != null && spotNotZeroOfr.Count > 0)
                    {
                        List<decimal> ofrNotZerops;
                        if (obj.isCleanPrice)
                        {
                            ofrNotZerops = (from a in spotNotZeroOfr select a.price).Distinct().OrderBy(i => i).ToList();
                        }
                        else
                        {
                            ofrNotZerops = (from a in spotNotZeroOfr select a.price).Distinct().OrderByDescending(i => i).ToList();
                        }
                        //查询最优Bid

                        if (ofrNotZerops.Count > 0)
                        {
                            vo.SpotFirstOfrPrice = ofrNotZerops[0];
                        }
                        if (ofrNotZerops.Count > 1)
                        {
                            vo.SpotSecondOfrPrice = ofrNotZerops[1];
                        }

                        spotNotZeroOfr.Sort();
                        foreach (GoodsVolumePo vpo in spotNotZeroOfr)
                        {
                            ///判断是不是买最优价
                            if (vpo.price == vo.SpotFirstOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Offer;//vvo.type = 1;
                                vvo.isBestPrice = true;
                                vo.SpotFirstOfr.Add(vvo);
                            }
                            else if (vpo.price == vo.SpotSecondOfrPrice)
                            {
                                GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                                vvo.Direction = EDirection.Offer;//vvo.type = 0;
                                vo.SpotSecondOfr.Add(vvo);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 设置即期报价在远期窗口显示---------------20220209新增
        /// </summary>
        /// <param name="obj"></param>
        private static void SetSpotPriceDisplay(GoodsPricePo obj, GoodsPriceVo vo, bool onlyShowSpot)
        {

            List<GoodsVolumePo> bidSpot = new List<GoodsVolumePo>();
            List<GoodsVolumePo> ofrSpot = new List<GoodsVolumePo>();
            if (obj.spotHashMap != null)
            {
                GoodsPricePo pricePo;
                if (obj.spotHashMap.TryGetValue("+1,明天+0", out pricePo))
                {
                    if (pricePo != null)
                    {
                        bidSpot = pricePo.bid;
                        ofrSpot = pricePo.ofr;
                    }
                }
            }

            if (bidSpot != null)
            {
                List<decimal> bidps;
                if (obj.isCleanPrice)
                {
                    bidps = (from a in bidSpot select a.price).Distinct().OrderByDescending(i => i).ToList();
                }
                else
                {
                    bidps = (from a in bidSpot select a.price).Distinct().OrderBy(i => i).ToList();
                }
                //查询最优和次优Bid

                if (bidps.Count > 0)
                    vo.OptimalBidPrice = bidps[0];
                if (bidps.Count > 1)
                    vo.SuboptimalBidPrice = bidps[1];

                //时间优先
                //obj.bid.Sort((x, y) => y.time.CompareTo(x.time));
                bidSpot.Sort();

                foreach (GoodsVolumePo vpo in bidSpot)
                {
                    ///判断是不是买最优价
                    if (vpo.price == vo.OptimalBidPrice)
                    {
                        GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                        vvo.Direction = EDirection.Bid;//vvo.type = 1;
                        vvo.isBestPrice = true;
                        vo.OptimalBid.Add(vvo);
                    }
                    else if (vpo.price == vo.SuboptimalBidPrice)
                    {///判断是不是买次优价
                        GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                        vvo.Direction = EDirection.Bid;//vvo.type = 1;
                        vo.SuboptimalBid.Add(vvo);
                    }
                }
            }
            //处理ofr报价列表
            if (ofrSpot != null)
            {
                List<decimal> ofrps;
                if (obj.isCleanPrice)
                {
                    //查询最优和次优ofr
                    ofrps = (from a in ofrSpot select a.price).Distinct().OrderBy(i => i).ToList();
                }
                else
                {
                    //查询最优和次优ofr
                    ofrps = (from a in ofrSpot select a.price).Distinct().OrderByDescending(i => i).ToList();
                }

                if (ofrps.Count > 0)
                    vo.OptimalOfferPrice = ofrps[0];
                if (ofrps.Count > 1)
                    vo.SuboptimalOfferPrice = ofrps[1];

                //时间优先
                //obj.ofr.Sort((x, y) => y.time.CompareTo(x.time));
                ofrSpot.Sort();
                foreach (GoodsVolumePo vpo in ofrSpot)
                {
                    if (vpo.price == vo.OptimalOfferPrice)
                    {
                        GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                        vvo.Direction = EDirection.Offer;//vvo.type = 0;
                        vvo.isBestPrice = true;
                        vo.OptimalOffer.Add(vvo);
                    }
                    else if (vpo.price == vo.SuboptimalOfferPrice)
                    {
                        GoodsVolumeVo vvo = ConvertTo(vpo, onlyShowSpot);
                        vvo.Direction = EDirection.Offer;//vvo.type = 0;
                        vo.SuboptimalOffer.Add(vvo);
                    }
                }
            }
        }

        /// <summary>
        /// 复制报价，仅更改onlyShowSpot
        /// </summary>
        /// <param name="po"></param>
        /// <param name="onlyShowSpot"></param>
        /// <returns></returns>
        public static GoodsVolumeVo ConvertTo(GoodsVolumePo po, bool onlyShowSpot)
        {
            GoodsVolumeVo vo = new GoodsVolumeVo();
            vo.bargainFlag = po.bargainFlag;
            vo.brokerID = po.brokerID;
            vo.brokerName = po.brokerName;
            vo.description = po.description;
            vo.description2 = po.description2;
            vo.id = po.id;
            vo.institutionID = po.institutionID;
            vo.institutionName = po.institutionName;
            vo.ocoFlag = po.ocoFlag;
            vo.price = po.price;
            vo.qty = po.qty;
            vo.settlementType = po.settlementType;
            vo.time = po.time;
            vo.traderID = po.traderID;
            vo.traderName = po.traderName;
            vo.wholeFlag = po.wholeFlag;
            vo.quoteInternalFlag = po.quoteInternalFlag;
            vo.bondKeyListedMarket = po.bondKeyListedMarket;

            vo.onlyShowSpot = onlyShowSpot;
            vo.isCheck = po.isCheck;
            vo.isFwdCheck = po.isFwdCheck;
            vo.goodCode = po.goodCode;
            //报价类型
            vo.onLine = po.onLine;
            vo.settlementTypeShow = po.settlementTypeShow;

            vo.ExchangeSignFlag = po.ExchangeSignFlag;
            vo.FanDian = po.FanDian;
            vo.flagUrgent = po.flagUrgent;
            return vo;
        }

        /// <summary>
        /// 获取基础利率代码
        /// </summary>
        /// <param name="goodsFrntype"></param>
        /// <returns></returns>
        public static string GetFrnType(string goodsFrntype)
        {
            string frnType = "FIXED";
            if (!string.IsNullOrWhiteSpace(goodsFrntype))
            {
                string firstTpye = "";
                //string secondType = "";
                int tagIndex = goodsFrntype.IndexOf('_');
                if (tagIndex != -1)
                {
                    string[] frnTypes = goodsFrntype.Split('_');
                    firstTpye = frnTypes[0];
                    //secondType= frnTypes[1];
                }
                else
                {
                    firstTpye = goodsFrntype;
                }
                switch (firstTpye)
                {
                    case "B2W":
                    case "B":
                        frnType = "REPO";
                        break;
                    case "DEPO":
                        frnType = "DEPO";
                        break;
                    case "SHIBOR":
                        frnType = "SHIBOR";
                        break;
                    case "LIBOR":
                    case "USD":
                        frnType = "LIBOR";
                        break;
                    case "LPR":
                        frnType = "LPR";
                        break;
                    //case "LRB":
                    //    frnType = "LRB";
                    //    break;
                    default:
                        if (firstTpye.StartsWith("DR"))
                        {
                            frnType = "DR";
                        }
                        else
                        {
                            frnType = "";
                        }
                        break;
                }
            }
            return frnType;

        }



        /// <summary>
        /// 获取基础利率代码 20220419 新增
        /// </summary>
        /// <param name="goodsFrntype"></param>
        /// <returns></returns>
        public static string GetBondFrnType(string goodsFrntype)
        {
            string frnType = "FIXED";
            if (!string.IsNullOrWhiteSpace(goodsFrntype))
            {
                string firstTpye = "";
                //string secondType = "";
                int tagIndex = goodsFrntype.IndexOf('_');
                if (tagIndex != -1)
                {
                    string[] frnTypes = goodsFrntype.Split('_');
                    firstTpye = frnTypes[0];
                    //secondType= frnTypes[1];
                }
                else
                {
                    firstTpye = goodsFrntype;
                }
                switch (firstTpye)
                {
                    case "B2W":
                    case "B":
                        frnType = "R";
                        break;
                    case "DEPO":
                        frnType = "Depo";
                        break;
                    case "SHIBOR":
                        frnType = "Sh";
                        break;
                    case "LIBOR":
                    case "USD":
                        frnType = "Li";
                        break;
                    case "LPR":
                        frnType = "LPR";
                        break;
                    //case "LRB":
                    //    frnType = "LRB";
                    //    break;
                    default:
                        if (firstTpye.StartsWith("DR"))
                        {
                            frnType = "DR";
                        }
                        else
                        {
                            frnType = "";
                        }
                        break;
                }
            }
            return frnType;

        }
    }
}
