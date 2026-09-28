using Ats.Foundation.Message;
using Ats.Foundation.Message.Serializer;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.View.Transaction;
using STC.Common.Handler.PT;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public class CopyDetailHandler
    {
        private static CopyDetailHandler _opyDetailHandler = new CopyDetailHandler();
        public static CopyDetailHandler Instance { get { return _opyDetailHandler; } }

        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public CopyDetailUI copyUI = null;

        //public void SubscribePTExec()
        //{
        //    SubscribeExec(Instance_OnExec);
        //}

        public void SubscribeExec(Action<List<TbookPtExecorders>> onMsgCallBack)
        {
            PTHandler.Instance.SubscribeExec(onMsgCallBack, user);
        }

        private void Instance_OnExec(List<TbookPtExecorders> list)
        {
            foreach (TbookPtExecorders order in list)
            {
            }
        }

        public List<ExecutionReport> ExecOrderQuery()
        {
            List<ExecutionReport> DataList = new List<ExecutionReport>();
            GrExecOrderQueryReq req = new GrExecOrderQueryReq();
            //if (!string.IsNullOrEmpty(user) && !"*".Equals(user))
            //req.userName = user;
            req.pageSize = 1000;
            req.pageNum = 0;
            req.dealTimeBegin = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            req.dealTimeEnd = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            bool flag = true;
            while (flag)
            {
                GrExecOrdersReplyPo reply = GRHandler.Instance.ExecOrderQuery(req);
                if (reply == null || reply.Datas == null)
                {
                    flag = false;
                    break;
                }
                if (reply != null && reply.Datas != null)
                {
                    DataList.AddRange(reply.Datas);
                    if (reply.Datas.Count < req.pageSize)
                    {
                        flag = false;
                    }
                }
                req.pageNum += 1;
            }
            return DataList;
        }

        #region 当日成交明细
        public List<PositionDetailPo> GetDetail(List<TransOrderServerData> templist, bool isDetail, SearchPo condition, bool isShowDir, bool isRefreshAll, TextBlockModel textBlockModel, TextBlockModel textBlockModel2)
        {
            List<CopyDetailPo> copyList = new List<CopyDetailPo>();
            foreach (TransOrderServerData o in templist)
            {
                CopyDetailPo po = new CopyDetailPo();
                po.seq = o.Sequence;
                //po.SeqSort = string.IsNullOrWhiteSpace(o.Sequence) ? 0:Convert.ToDecimal(o.Sequence);
                po.execId = o.ExecID;
                po.orderId = o.OrderID;
                //po.delflag = o.Delflag;
                po.orderStatus = o.OrdStatus;
                po.side = o.Side;
                po.goodsCode = o.SecurityID;
                po.goodsShortName = o.Symbol;
                po.goodstype = o.SecurityType;
                po.BondSubtype = o.BondSubtype;
                decimal price;
                if (decimal.TryParse(o.Price, out price))
                    po.price = price;
                decimal qty;
                if (decimal.TryParse(o.LastQty, out qty))
                    po.qty = qty;
                //po.ListedMarket = o.m
                po.dealdate = o.TradeDate;
                po.dealdate2 = o.TradeDate2;
                po.settlementdate = o.SettlDate;
                po.settlementdate2 = o.SettlDate2;
                po.settlementType = o.SettlType;
                po.settlementtype2 = o.SettlType2;
                po.remarks = o.Remark;

                po.bankCodeA = o.InstitutionName;
                po.bankAgentNameA = o.TraderName;
                po.brokerIdA = o.PartySubID;
                po.brokerNameA = o.PartySubName;

                po.bankCodeB = o.CounterPartyInstitutionName;//o.Counterpartyname;
                po.bankAgentNameB = o.CounterPartyTraderName;//o.Counterpartysubname;
                po.brokerNameB = o.CounterPartySubName;
                po.brokerIdB = o.CounterPartySubID;

                po.bankIdA = o.InstitutionID;
                po.bankagentIdA = o.TraderID;
                po.bankIdB = o.CounterPartyInstitutionId;
                po.bankAgentIdB = o.CounterPartyTraderId;
                po.bankcodeshorta = !string.IsNullOrEmpty(o.InstitutionShowName) ? o.InstitutionShowName : o.InstitutionName;
                po.bankcodeshortb = !string.IsNullOrEmpty(o.CounterPartyInstitutionShowName) ? o.CounterPartyInstitutionShowName : o.CounterPartyInstitutionName;
                po.timeToMaturity = o.TimeToMaturity;
                po.brokerunread = o.BrokerUnread;
                po.BridgeFlag = o.BridgeFlag;
                po.bridgebankid = o.BridgeInstitutionId;
                po.bridgebankid2 = o.BridgeInstitutionId2;
                po.BridgeType = o.BridgeType;
                po.bridgeType2 = o.BridgeType2;
                po.bridgebankagentid = o.BridgeTraderId;
                po.bridgebankagentid2 = o.BridgeTraderId2;
                po.flagInner = o.FirmFlag;
                //if (o.BridgeOrder != null)
                //    po.bridgeOrder = JsonConvert.DeserializeObject<bridgeOrder>(o.BridgeOrder);
                //if (o.BridgeOrder2 != null)
                //    po.bridgeOrder2 = JsonConvert.DeserializeObject<bridgeOrder>(o.BridgeOrder2);
                po.bridgeOrder = o.BridgeOrderModel;
                po.bridgeOrder2 = o.BridgeOrderModel2;
                po.bridgeremarkshow = o.BridgeRemarkShow;
                po.bridgeremarkshow2 = o.BridgeRemarkShow2;
                po.sendto = o.SendTo;
                po.sendto2 = o.SendTo2;
                po.bridgesendto = o.BridgeSendTo;
                po.bridgesendto2 = o.BridgeSendTo2;
                po.bridgeremark = o.BridgeRemark;
                po.bridgeremark2 = o.BridgeRemark2;

                po.ListedMarket = o.ListedMarket;
                po.BondSubtype = o.BondSubtype;
                po.IssuerCode = o.IssuerCode;
                po.EntCor = o.EntCor;
                po.FRNIndexID = o.FRNIndexID;

                po.DetailMark = string.IsNullOrWhiteSpace(o.DetailMark) ? "0" : o.DetailMark;
                po.issueState = o.IssueState;
                po.issueDir = o.IssueDir;
                po.issueChannel = o.IssueChannel;
                po.issueInstId = o.IssueInstId;
                po.issueInstName = o.IssueInstName;
                po.issueInstNameShort = o.IssueInstNameShort;
                po.issueTraderId = o.IssueTraderId;
                po.issueTraderName = o.IssueTraderName;
                po.issueRemark = o.IssueRemark;
                po.issueShowType = o.IssueShowType;
                po.issueDesc = o.IssueDesc;
                po.proxyFee = o.ProxyFee;
                po.forbidRefreshFlag = o.ForBidRefreshFlag;
                po.AssociatedFlag = o.AssociatedFlag;
                po.fandian = o.FanDian;
                po.SingleQQMsgSendFlaga = o.SingleQqmsgSendFlagA;
                po.SingleQQMsgSendFlagb = o.SingleQqmsgSendFlagB;
                po.IssueAmount = o.IssueAmount;
                po.IssuerRatingCurrent = o.IssuerRatingCurrent;
                TransOrderHandler.Instance.SupplementBondInfo(po);
                po.MarkDetailUserList = o.MarkDetailUserList;
                po.sendSorFlaga = o.SendSorFlagA;
                po.sendSorFlagb = o.SendSorFlagB;
                copyList.Add(po);
            }
            var newData = CopyDetailHelper.GetDetail(copyList, true, condition, isShowDir, isRefreshAll, textBlockModel, textBlockModel2);
            return newData;
        }



        public List<PositionDetailPo> GetHistDetail(List<TransOrderServerData> templist, bool isDetail, SearchPo condition, bool isShowDir, bool isRefreshAll, TextBlockModel textBlockModel, TextBlockModel textBlockModel2)
        {
            List<CopyDetailPo> copyList = new List<CopyDetailPo>();
            foreach (TransOrderServerData o in templist)
            {
                CopyDetailPo po = new CopyDetailPo();
                po.seq = o.Sequence;
                //po.SeqSort = string.IsNullOrWhiteSpace(o.Sequence) ? 0 : Convert.ToDecimal(o.Sequence);
                po.execId = o.ExecID;
                po.orderId = o.OrderID;
                //po.delflag = o.Delflag;
                po.orderStatus = o.OrdStatus;
                po.side = o.Side;
                po.goodsCode = o.SecurityID;
                po.goodsShortName = o.Symbol;
                po.goodstype = o.SecurityType;
                po.BondSubtype = o.BondSubtype;
                decimal price;
                if (decimal.TryParse(o.Price, out price))
                    po.price = price;
                decimal qty;
                if (decimal.TryParse(o.LastQty, out qty))
                    po.qty = qty;
                //po.ListedMarket = o.m
                po.dealdate = o.TradeDate;
                po.dealdate2 = o.TradeDate2;
                po.settlementdate = o.SettlDate;
                po.settlementdate2 = o.SettlDate2;
                po.settlementType = o.SettlType;
                po.settlementtype2 = o.SettlType2;
                po.remarks = o.Remark;

                po.bankCodeA = o.InstitutionName;
                po.bankAgentNameA = o.TraderName;
                po.brokerIdA = o.PartySubID;
                po.brokerNameA = o.PartySubName;

                po.bankCodeB = o.CounterPartyInstitutionName;//o.Counterpartyname;
                po.bankAgentNameB = o.CounterPartyTraderName;//o.Counterpartysubname;
                po.brokerNameB = o.CounterPartySubName;
                po.brokerIdB = o.CounterPartySubID;

                po.bankIdA = o.InstitutionID;
                po.bankagentIdA = o.TraderID;
                po.bankIdB = o.CounterPartyInstitutionId;
                po.bankAgentIdB = o.CounterPartyTraderId;
                po.bankcodeshorta = !string.IsNullOrEmpty(o.InstitutionShowName) ? o.InstitutionShowName : o.InstitutionName;
                po.bankcodeshortb = !string.IsNullOrEmpty(o.CounterPartyInstitutionShowName) ? o.CounterPartyInstitutionShowName : o.CounterPartyInstitutionName;
                po.timeToMaturity = o.TimeToMaturity;
                po.brokerunread = o.BrokerUnread;
                po.BridgeFlag = o.BridgeFlag;
                po.bridgebankid = o.BridgeInstitutionId;
                po.bridgebankid2 = o.BridgeInstitutionId2;
                po.BridgeType = o.BridgeType;
                po.bridgeType2 = o.BridgeType2;
                po.bridgebankagentid = o.BridgeTraderId;
                po.bridgebankagentid2 = o.BridgeTraderId2;
                po.flagInner = o.FirmFlag;
                //if (o.BridgeOrder != null)
                //    po.bridgeOrder = JsonConvert.DeserializeObject<bridgeOrder>(o.BridgeOrder);
                //if (o.BridgeOrder2 != null)
                //    po.bridgeOrder2 = JsonConvert.DeserializeObject<bridgeOrder>(o.BridgeOrder2);
                po.bridgeOrder = o.BridgeOrderModel;
                po.bridgeOrder2 = o.BridgeOrderModel2;
                po.bridgeremarkshow = o.BridgeRemarkShow;
                po.bridgeremarkshow2 = o.BridgeRemarkShow2;
                po.sendto = o.SendTo;
                po.sendto2 = o.SendTo2;
                po.bridgesendto = o.BridgeSendTo;
                po.bridgesendto2 = o.BridgeSendTo2;
                po.bridgeremark = o.BridgeRemark;
                po.bridgeremark2 = o.BridgeRemark2;

                po.ListedMarket = o.ListedMarket;
                po.BondSubtype = o.BondSubtype;
                po.IssuerCode = o.IssuerCode;
                po.EntCor = o.EntCor;
                po.FRNIndexID = o.FRNIndexID;

                po.DetailMark = string.IsNullOrWhiteSpace(o.DetailMark) ? "0" : o.DetailMark;
                po.issueState = o.IssueState;
                po.issueDir = o.IssueDir;
                po.issueChannel = o.IssueChannel;
                po.issueInstId = o.IssueInstId;
                po.issueInstName = o.IssueInstName;
                po.issueInstNameShort = o.IssueInstNameShort;
                po.issueTraderId = o.IssueTraderId;
                po.issueTraderName = o.IssueTraderName;
                po.issueRemark = o.IssueRemark;
                po.issueShowType = o.IssueShowType;
                po.issueDesc = o.IssueDesc;
                po.proxyFee = o.ProxyFee;
                po.forbidRefreshFlag = o.ForBidRefreshFlag;
                po.AssociatedFlag = o.AssociatedFlag;
                po.fandian = o.FanDian;
                po.SingleQQMsgSendFlaga = o.SingleQqmsgSendFlagA;
                po.SingleQQMsgSendFlagb = o.SingleQqmsgSendFlagB;
                po.IssueAmount = o.IssueAmount;
                po.IssuerRatingCurrent = o.IssuerRatingCurrent;
                TransOrderHandler.Instance.SupplementBondInfo(po);
                po.MarkDetailUserList = o.MarkDetailUserList;
                po.sendSorFlaga = o.SendSorFlagA;
                po.sendSorFlagb = o.SendSorFlagB;
                copyList.Add(po);
            }
            var newData = CopyDetailHelper.GetHistDetail(copyList, true, condition, isShowDir, isRefreshAll, textBlockModel, textBlockModel2);
            return newData;
        }
        #endregion


        #region 右键标记
        public static ExecutionReport GetUpdateOrderPoNew(string newVal, ECommandType field, string orderId, string execId)
        {
            ExecutionReport execOrderPo = new ExecutionReport();
            execOrderPo.ExecID = execId;

            decimal d = 0;
            int ct = 0;
            switch (field)
            {
                case ECommandType.UpdateDetailMark:
                    execOrderPo.MarkDetailUsers = newVal;
                    ct = (int)ECommandType.UpdateDetailMark;
                    break;
                case ECommandType.ManualSendDealA:
                    execOrderPo.SendSorFlagA = newVal;
                    ct = (int)ECommandType.ManualSendDealA;
                    break;
                case ECommandType.ManualSendDealB:
                    execOrderPo.SendSorFlagB = newVal;
                    ct = (int)ECommandType.ManualSendDealB;
                    break;
            }
          
            execOrderPo.CommandType = ct + "";
            return execOrderPo;
        }
        #endregion

        public static void ExecOrderUpdate(ExecutionReport o)
        {
            Task.Run(() =>
                    {
                        PResult reply = GRHandler.Instance.ExecOrderUpdate(o);
                    });
        }

        /// <summary>
        /// 订单是否是当前登录的broker 的
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static bool IsLoginBrokerOrder(TransOrderServerData orderDetailPo)
            => LocalEnv.UserInfo.IsMyself(orderDetailPo.PartySubID) || LocalEnv.UserInfo.IsMyself(orderDetailPo.CounterPartySubID);


        /// <summary>
        /// 解决12324包含24的问题
        /// 20200417
        /// </summary>
        /// <param name="userIds">一组userid结合，空格分开</param>
        /// <returns></returns>
        private string userIdKey = string.Empty;
        public bool IsContainsUserId(string userIds)
        {
           // if (LocalEnv.UserInfo.IsContainsUserId(order.brokerunread)
 

            if (string.IsNullOrEmpty(userIds))
                return false;
            if (string.IsNullOrEmpty(userIdKey))
                userIdKey = string.Format(",{0},", LocalEnv.UserInfo.UserId);
            string keys = string.Format(",{0},", userIds);
            if (keys.Contains(userIdKey))
                return true;
            return false;
        }
        /// <summary>
        /// 成交不刷按钮
        /// </summary>
       // public bool IsShowInternalTransaction { get; set; } = false;
        /// <summary>
        /// 经纪商名称
        /// </summary>
        public string BrokerNameFlag { get; set; } = "TP";
        /// <summary>
        /// 是否显示简称:true/false
        /// </summary>
        public string formatShortNameCheck;
        /// <summary>
        /// 手动发送按钮
        /// </summary>
        public bool IsShowSendManuallyButton { get; set; } = false;
        /// <summary>
        /// 判断当前登录用户对userid是否具有修改权限
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsMyModifys(string userid)
        {
            return SystemPowerHandler.Instance.IsCanEditOrder(userid);
        }
        /// <summary>
        /// 成交回执机构列表
        /// </summary>
        public List<ReceiptInst> SendReceiptInstDisplay
        {
            get
            {
                List<ReceiptInst> receipts = new List<ReceiptInst>();
                if (!string.IsNullOrWhiteSpace(SendReceiptInst))
                {
                   // receipts = MsgHelper.Deserialize<List<ReceiptInst>>(SendReceiptInst, MsgSerializerType.Json);
                }
                return receipts;
            }
        }
        /// <summary>
        /// QM成交回执机构列表
        /// </summary>
        public List<ReceiptInst> SendExternalInstDisplay
        {
            get
            {
                List<ReceiptInst> receipts = new List<ReceiptInst>();
                if (!string.IsNullOrWhiteSpace(sendExternalInst))
                {
                    if (!sendExternalInst.ToLower().Contains("true"))
                    {
                       receipts = MsgHelper.Deserialize<List<ReceiptInst>>(sendExternalInst, MsgSerializerType.Json);
                    }
                }
                return receipts;
            }
        }
        /// <summary>
        /// QM成交回执所有
        /// </summary>
        public bool IsAllSendExternalInst
        {
            get
            {
                bool receipts = false;
                if (!string.IsNullOrWhiteSpace(sendExternalInst))
                {
                    if (sendExternalInst.ToLower().Contains("true"))
                    {
                        receipts = true;
                    }
                }
                return receipts;
            }
        }
        /// <summary>
        /// QM成交回执机构列表
        /// </summary>
        public string sendExternalInst { get; set; }
        /// <summary>
        /// 成交回执机构列表
        /// </summary>
        public string SendReceiptInst { get; set; }
        /// <summary>
        /// 回执机构列表
        /// </summary>
        public class ReceiptInst
        {

            /// <summary>
            /// 成交回执机构id
            /// </summary>
            public string instid { get; set; }

            /// <summary>
            /// 成交回执机构名称
            /// </summary>
            public string instname { get; set; }
        }
    }
}
