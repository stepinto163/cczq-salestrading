using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Windows.Controls;
using STC.Common.Model.Base;
using STC.Common.Handler.CRM;
using STC.Broker.Plugin.Model.Transaction;
using static STC.Broker.Plugin.Utils.CommonEnum;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.SystemParam;
using com.gateway.log;
using STC.Common.Model.PT;
using STC.Common.Model.Fix.Message;
using System.Collections.Concurrent;
using STC.Broker.Plugin.Model;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// 加桥单的显示策略
    /// </summary>
    public class BridgeHelper
    {
        public static List<TransOrderServerData> getCacheList()
        {
            List<TransOrderServerData> allOrders = new List<TransOrderServerData>();
            ConcurrentDictionary<string, TransOrderServerData> concurrentDictionary = TransOrderHandler.Instance.GetAllOrder(true);
            if (concurrentDictionary != null && concurrentDictionary.Count > 0)
            {
                allOrders = concurrentDictionary.Values.ToList();
            }
            return allOrders;
        }


        /// <summary>
        /// 带单号
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public static string CopyBridgeInfo(BridgeOrderDetail orderDetail)
        {
            string result = string.Empty;
            result = string.Format("{0}VS{1} {2}-{3}", orderDetail.ShowBrokerNameA, orderDetail.ShowBrokerNameB, orderDetail.seq, orderDetail.OrderTxt);
            return result;
        }
        /// <summary>
        /// 带单号复制
        /// </summary>
        /// <param name="orderTxt">空格的文本</param>
        /// <returns></returns>
        public static string CopyBridgeInfoByHtml(string orderTxt)
        {
            string result = string.Empty;

            string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";

            result = string.Format(html, orderTxt);

            return result;
        }
        /// <summary>
        /// 带单号复制短融超短融
        /// </summary>
        /// <param name="orderTxt"></param>
        /// <returns></returns>
        public static string CopyBridgeInfoByHtmlShort(string orderTxt, BridgeOrderDetail bridgeOrderDetail)
        {
            string result = string.Empty;
            //债券是否为短融超短融
            if (IsShortFuse(bridgeOrderDetail.goodsSubType))
            {
                if (string.IsNullOrWhiteSpace(orderTxt))
                {
                    return result;
                }
                //获取债券发行量
                string bondIssueAmount = GetBondIssueAmount(bridgeOrderDetail.SecurityID);
                if (string.IsNullOrWhiteSpace(bondIssueAmount)
                    && (bridgeOrderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder.combinedOrderRemark))
                    && (bridgeOrderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder2.combinedOrderRemark)))
                {
                    string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                    result = string.Format(html, orderTxt);
                }
                else
                {
                    int indexStart = orderTxt.LastIndexOf('(');
                    int indexEnd = orderTxt.LastIndexOf(')');
                    if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart + 1)
                    {
                        string firstStr = orderTxt.Substring(indexStart + 1, indexEnd - indexStart - 1);
                        string secStr = orderTxt.Substring(0, indexStart);
                        string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                        result = string.Format(html, firstStr);
                        result = $"{result}<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{secStr}</span></div></br>";
                    }
                    else
                    {
                        string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                        result = string.Format(html, orderTxt);
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(orderTxt))
                {
                    return result;
                }

                if ((bridgeOrderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder.combinedOrderRemark))
                    && (bridgeOrderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder2.combinedOrderRemark)))
                {
                    string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                    result = string.Format(html, orderTxt);
                }
                else
                {
                    int indexStart = orderTxt.LastIndexOf('(');
                    int indexEnd = orderTxt.LastIndexOf(')');
                    if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart + 1)
                    {
                        string firstStr = orderTxt.Substring(indexStart + 1, indexEnd - indexStart - 1);
                        string secStr = orderTxt.Substring(0, indexStart);
                        string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                        result = string.Format(html, firstStr);
                        result = $"{result}<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{secStr}</span></div></br>";
                    }
                    else
                    {
                        string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                        result = string.Format(html, orderTxt);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 带单号复制短融超短融文本格式
        /// </summary>
        /// <param name="bridgeOrderDetail"></param>
        /// <returns></returns>
        public static string CopyBridgeInfoOrderTxtShort(BridgeOrderDetail bridgeOrderDetail, UserBridge bridge)
        {
            string result = string.Empty;
            //债券是否为短融超短融
            if (IsShortFuse(bridgeOrderDetail.goodsSubType))
            {
                if (string.IsNullOrWhiteSpace(bridgeOrderDetail.OrderTxt))
                {
                    return result;
                }
                //获取债券发行量
                string bondIssueAmount = GetBondIssueAmount(bridgeOrderDetail.SecurityID);
                if (string.IsNullOrWhiteSpace(bondIssueAmount)
                    && (bridgeOrderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder.combinedOrderRemark))
                    && (bridgeOrderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder2.combinedOrderRemark)))
                {
                    result = string.Format("{0}\r\n", bridgeOrderDetail.OrderTxt);
                }
                else
                {
                    int indexStart = bridgeOrderDetail.OrderTxt.LastIndexOf('(');
                    int indexEnd = bridgeOrderDetail.OrderTxt.LastIndexOf(')');
                    if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart + 1)
                    {
                        string firstStr = bridgeOrderDetail.OrderTxt.Substring(indexStart + 1, indexEnd - indexStart - 1);
                        string secStr = bridgeOrderDetail.OrderTxt.Substring(0, indexStart);
                        result = $"{firstStr}\r\n{secStr}\r\n";
                    }
                    else
                    {
                        result = string.Format("{0}\r\n", bridgeOrderDetail.OrderTxt);
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(bridgeOrderDetail.OrderTxt))
                {
                    return result;
                }
                if ((bridgeOrderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder.combinedOrderRemark))
                    && (bridgeOrderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(bridgeOrderDetail.bridgeOrder2.combinedOrderRemark)))
                {
                    result = string.Format("{0}\r\n", bridgeOrderDetail.OrderTxt);
                }
                else
                {
                    int indexStart = bridgeOrderDetail.OrderTxt.LastIndexOf('(');
                    int indexEnd = bridgeOrderDetail.OrderTxt.LastIndexOf(')');
                    if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart + 1)
                    {
                        string firstStr = bridgeOrderDetail.OrderTxt.Substring(indexStart + 1, indexEnd - indexStart - 1);
                        string secStr = bridgeOrderDetail.OrderTxt.Substring(0, indexStart);
                        result = $"{firstStr}\r\n{secStr}\r\n";
                    }
                    else
                    {
                        result = string.Format("{0}\r\n", bridgeOrderDetail.OrderTxt);
                    }
                }
            }
            return result;

        }
        /// <summary>
        /// 带单号的复制
        /// </summary>
        /// <param name="orderDetail">html格式文本</param>
        /// <returns></returns>
        public static string CopyBridgeInfoByHtml(BridgeOrderDetail orderDetail)
        {
            string result = string.Empty;
            //获取债券发行量
            string bondIssueAmount = GetBondIssueAmount(orderDetail.SecurityID);
            if (string.IsNullOrWhiteSpace(bondIssueAmount)
              && (orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
              && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
            {
                string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                result = string.Format("{0}VS{1} {2}-{3}", orderDetail.ShowBrokerNameA, orderDetail.ShowBrokerNameB, orderDetail.seq, orderDetail.OrderTxt);
                result = string.Format(html, result);
            }
            else
            {
                int indexStart = orderDetail.OrderTxt.LastIndexOf('(');
                int indexEnd = orderDetail.OrderTxt.LastIndexOf(')');
                if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart + 1)
                {
                    string firstStr = orderDetail.OrderTxt.Substring(indexStart + 1, indexEnd - indexStart - 1);
                    string secStr = orderDetail.OrderTxt.Substring(0, indexStart);
                    string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                    result = string.Format(html, firstStr);
                    result = $"{result}<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderDetail.ShowBrokerNameA}VS{orderDetail.ShowBrokerNameB} {orderDetail.seq}-{secStr}</span></div></br>";
                }
                else
                {
                    string html = "<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{0}</span></div></br>";
                    result = string.Format("{0}VS{1} {2}-{3}", orderDetail.ShowBrokerNameA, orderDetail.ShowBrokerNameB, orderDetail.seq, orderDetail.OrderTxt);
                    result = string.Format(html, result);
                }
            }
            return result;
        }

        private static string GetBankcode(BridgeOrderDetail orderDetailPo, string bridgeType, UserBridge bridge)
        {
            string result = string.Empty;

            if (bridgeType.Equals(orderDetailPo.BridgeType))
            {
                bool isMatch = bridge.bankTraderid.Equals(orderDetailPo.BridgeTraderId) && bridge.bankInstitutionid.Equals(orderDetailPo.BridgeInstitutionId);

                result = isMatch ? string.IsNullOrEmpty(orderDetailPo.BridgeInstitutionName2) ? orderDetailPo.bridgebankcode2 : orderDetailPo.BridgeInstitutionName2
                    : string.IsNullOrEmpty(orderDetailPo.BridgeInstitutionName) ? orderDetailPo.bridgebankcode : orderDetailPo.BridgeInstitutionName;
            }
            else if (bridgeType.Equals(orderDetailPo.BridgeType2))
            {
                bool isMatch = bridge.bankTraderid.Equals(orderDetailPo.BridgeTraderId2) && bridge.bankInstitutionid.Equals(orderDetailPo.BridgeInstitutionId2);

                result = isMatch ? string.IsNullOrEmpty(orderDetailPo.BridgeInstitutionName) ? orderDetailPo.bridgebankcode : orderDetailPo.BridgeInstitutionName
                    : string.IsNullOrEmpty(orderDetailPo.BridgeInstitutionName2) ? orderDetailPo.bridgebankcode2 : orderDetailPo.BridgeInstitutionName2;
            }


            return result;
        }

        public static string ConvertQty(decimal qty)
        {
            string result = string.Empty;

            if (qty >= 10000)
                result = string.Format("{0}E", qty / 10000);
            else
                result = qty.ToString();

            return result;
        }

        private static string GetTeaderInfo(BridgeOrderDetail orderDetailPo, bool ofr)
        {
            if (orderDetailPo.InstAShow == orderDetailPo.InstBShow || orderDetailPo.remarks == "交易所")
            {
                if (ofr)
                    return orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow;
                else
                    return orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow;
            }
            else
            {
                if (ofr)
                    return orderDetailPo.IsGVN ? orderDetailPo.InstAShow : orderDetailPo.InstBShow;
                else
                    return orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow;

            }
        }
        /// <summary>
        /// 获取桥状态
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static string getBridgeState(BridgeOrderDetail orderDetailPo, UserBridge bridge)
        {
            string result = string.Empty;
            if (orderDetailPo.BridgeType.Equals("B,O"))
            {
                result = orderDetailPo.bridgeState.GetDescription(); ;
            }
            else
            {
                if (orderDetailPo.bridgeOrder == null)
                {
                    result = STC.Common.Model.PT.EBridgeState.已确认.GetDescription();
                }
                else
                {
                    if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                        result = orderDetailPo.bridgeState.GetDescription();
                    else
                        result = orderDetailPo.bridgeState2.GetDescription();
                }
            }
            return result;
        }

        /// <summary>
        /// 获取桥状态
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="bridge"></param>
        /// <param name="flag"> 1:ofr, 2：bid</param>
        /// <returns></returns>
        public static string GetBridgeState(BridgeOrderDetail orderDetailPo, UserBridge bridge, int flag)
        {
            string result = string.Empty;

            if (orderDetailPo == null)
            {
                return result;
            }

            if (flag == 1)
            {
                //单桥
                if (orderDetailPo.BridgeType.Equals("B,O"))
                {
                    result = GetBridgeSingeState(orderDetailPo, bridge, flag);
                }
                else
                {
                    //双桥
                    if ((orderDetailPo.BridgeType.Equals("B") && orderDetailPo.BridgeType2.Equals("O"))
                   || (orderDetailPo.BridgeType.Equals("O") && orderDetailPo.BridgeType2.Equals("B")))
                    {
                        if (orderDetailPo.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetailPo.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                        {
                            if (orderDetailPo.bridgeOrder == null)
                            {
                                result = BridgeState.Confirmed.GetDescription();
                            }
                            else
                            {
                                if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                                    result = orderDetailPo.OfrbridgeState.GetDescription();
                                else
                                    result = orderDetailPo.OfrbridgeState2.GetDescription();
                            }
                        }
                        else
                        {
                            result = GetBridgeSingeState(orderDetailPo, bridge, flag);
                        }
                    }
                    else
                    {
                        result = GetBridgeStatus(orderDetailPo, bridge, flag);
                    }
                }
            }
            else
            {
                if (orderDetailPo.BridgeType.Equals("B,O"))
                {
                    result = GetBridgeBidState(orderDetailPo, bridge, flag);
                }
                else
                {
                    //双桥
                    if ((orderDetailPo.BridgeType.Equals("B") && orderDetailPo.BridgeType2.Equals("O"))
                   || (orderDetailPo.BridgeType.Equals("O") && orderDetailPo.BridgeType2.Equals("B")))
                    {
                        if (orderDetailPo.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetailPo.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                        {
                            if (orderDetailPo.bridgeOrder == null)
                            {
                                result = BridgeState.Confirmed.GetDescription();
                            }
                            else
                            {
                                if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                                    result = orderDetailPo.BidbridgeState.GetDescription();
                                else
                                    result = orderDetailPo.BidbridgeState2.GetDescription();
                            }
                        }
                        else
                        {
                            result = GetBridgeBidState(orderDetailPo, bridge, flag);
                        }
                    }
                    else
                    {
                        result = GetBridgeStatus(orderDetailPo, bridge, flag);
                    }
                }

            }
            return result;
        }


        /// <summary>
        /// 获取桥的单边状态,带发单机构状态
        /// </summary>
        /// <returns></returns>
        public static string GetBridgeSingeState(BridgeOrderDetail orderDetailPo, UserBridge bridge, int flag)
        {

            string result = string.Empty;
            if (orderDetailPo.ofrIssueInfoReqList != null && orderDetailPo.ofrIssueInfoReqList.Count > 0)
            {
                List<int> statusValue = new List<int>();
                foreach (SendInstSevieceModel item in orderDetailPo.ofrIssueInfoReqList)
                {
                    if (!statusValue.Contains(item.IssueInstState))
                    {
                        statusValue.Add(item.IssueInstState);
                    }
                    item.BridgeStateShow = item.BridgeState.GetDescription();
                }
                if (statusValue.Count > 1)
                {
                    result = Constant.BridgeStatusShow;
                }
                else
                {
                    if (statusValue.Count == 1)
                    {
                        if (statusValue[0] == 3 || statusValue[0] == 6)
                        {
                            result = GetBridgeStatus(orderDetailPo, bridge, flag);
                        }
                        else
                        {
                            result = Constant.BridgeStatusShow;
                        }
                    }
                    else
                    {
                        result = GetBridgeStatus(orderDetailPo, bridge, flag);
                    }
                }
            }
            else
            {
                result = GetBridgeStatus(orderDetailPo, bridge, flag);
            }

            return result;
        }
        /// <summary>
        /// 获取桥的单边状态
        /// </summary>
        /// <returns></returns>
        public static string GetBridgeBidState(BridgeOrderDetail orderDetailPo, UserBridge bridge, int flag)
        {
            string result = string.Empty;
            if (orderDetailPo.bidIssueInfoReqList != null && orderDetailPo.bidIssueInfoReqList.Count > 0)
            {
                List<int> statusValue = new List<int>();
                foreach (SendInstSevieceModel item in orderDetailPo.bidIssueInfoReqList)
                {
                    if (!statusValue.Contains(item.IssueInstState))
                    {
                        statusValue.Add(item.IssueInstState);
                    }
                    item.BridgeStateShow = item.BridgeState.GetDescription();
                }
                if (statusValue.Count > 1)
                {
                    result = Constant.BridgeStatusShow;
                }
                else
                {
                    if (statusValue.Count == 1)
                    {
                        if (statusValue[0] == 3 || statusValue[0] == 6)
                        {
                            result = GetBridgeStatus(orderDetailPo, bridge, flag);
                        }
                        else
                        {
                            result = Constant.BridgeStatusShow;
                        }
                    }
                    else
                    {
                        result = GetBridgeStatus(orderDetailPo, bridge, flag);
                    }
                }
            }
            else
            {
                result = GetBridgeStatus(orderDetailPo, bridge, flag);
            }

            return result;
        }

        /// <summary>
        /// 获取不同桥状态
        /// </summary>
        /// <returns></returns>
        public static string GetBridgeStatus(BridgeOrderDetail orderDetailPo, UserBridge bridge, int flag)
        {
            string result = string.Empty;

            if (orderDetailPo == null)
            {
                return result;
            }

            if (flag == 1)
            {
                if (orderDetailPo.ofrIssueInfoReqList != null && orderDetailPo.ofrIssueInfoReqList.Count > 0)
                {
                    result = orderDetailPo.ofrIssueInfoReqList[0].BridgeState.GetDescription();
                }
                else
                {
                    if (orderDetailPo.BridgeType.Equals("B,O"))
                    {
                        result = orderDetailPo.OfrbridgeState.GetDescription();
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder == null)
                        {
                            result = BridgeState.Confirmed.GetDescription();
                        }
                        else
                        {
                            if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                                result = orderDetailPo.OfrbridgeState.GetDescription();
                            else
                                result = orderDetailPo.OfrbridgeState2.GetDescription();
                        }
                    }
                }

            }
            else
            {
                if (orderDetailPo.bidIssueInfoReqList != null && orderDetailPo.bidIssueInfoReqList.Count > 0)
                {
                    result = orderDetailPo.bidIssueInfoReqList[0].BridgeState.GetDescription();
                }
                else
                {
                    if (orderDetailPo.BridgeType.Equals("B,O"))
                    {
                        result = orderDetailPo.BidbridgeState.GetDescription();
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder == null)
                        {
                            result = BridgeState.Confirmed.GetDescription();
                        }
                        else
                        {
                            if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                                result = orderDetailPo.BidbridgeState.GetDescription();
                            else
                                result = orderDetailPo.BidbridgeState2.GetDescription();
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 获取桥状态拒绝原因
        /// </summary>
        /// <returns></returns>
        public static string GetBridgeRejectReason(BridgeOrderDetail orderDetailPo, UserBridge bridge, int flag)
        {
            string result = string.Empty;

            if (orderDetailPo == null)
            {
                return result;
            }
            if (flag == 1)
            {
                if (orderDetailPo.BridgeType.Equals("B,O"))
                {
                    result = orderDetailPo.bridgeOrder != null ? orderDetailPo.bridgeOrder.ofrRejectReason : "--";
                }
                else
                {
                    if (orderDetailPo.bridgeOrder == null)
                    {
                        result = "--";
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                            result = orderDetailPo.bridgeOrder != null ? orderDetailPo.bridgeOrder.ofrRejectReason : "--";
                        else
                            result = orderDetailPo.bridgeOrder2 != null ? orderDetailPo.bridgeOrder2.ofrRejectReason : "--";
                    }
                }
            }
            else
            {
                if (orderDetailPo.BridgeType.Equals("B,O"))
                {
                    result = orderDetailPo.bridgeOrder != null ? orderDetailPo.bridgeOrder.bidRejectReason : "--";
                }
                else
                {
                    if (orderDetailPo.bridgeOrder == null)
                    {
                        result = "--";
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder.instTradeId == bridge.bankTraderid)
                            result = orderDetailPo.bridgeOrder != null ? orderDetailPo.bridgeOrder.bidRejectReason : "--";
                        else
                            result = orderDetailPo.bridgeOrder2 != null ? orderDetailPo.bridgeOrder2.bidRejectReason : "--";
                    }
                }
            }
            return result;

        }


        const string agreementNOFomrt = " 约定号：{0}";
        /// <summary>
        /// 获取桥的预订号
        /// </summary>
        /// <param name="order"></param>
        /// <param name="key"></param>
        /// <param name="o"></param>
        static string getAgreementNO(bridgeOrder border, bool isOfr)
        {
            string rt = string.Empty;
            if (border != null)
            {
                if (isOfr)
                {
                    if (!string.IsNullOrEmpty(border.ofrAgreementNo))
                        rt = string.Format(agreementNOFomrt, border.ofrAgreementNo);
                }
                else
                {
                    if (!string.IsNullOrEmpty(border.bidAgreementNo))
                        rt = string.Format(agreementNOFomrt, border.bidAgreementNo);
                }
            }
            return rt;
        }
        /// <summary>
        /// 根据订单、桥以及方向获取约定号
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="bridge"></param>
        /// <param name="isOfr"></param>
        /// <returns></returns>
        static string getAgreementNO(BridgeOrderDetail orderDetailPo, UserBridge bridge, bool isOfr)
        {
            bridgeOrder border = null;
            if ("B,O".Equals(orderDetailPo.BridgeType))
            {
                border = orderDetailPo.bridgeOrder;
            }
            else if ("B".Equals(orderDetailPo.BridgeType) || "O".Equals(orderDetailPo.BridgeType))
            {
                if (orderDetailPo.bridgeOrder?.instId == bridge.bankInstitutionid && orderDetailPo.bridgeOrder?.instTradeId == bridge.bankTraderid)
                    border = orderDetailPo.bridgeOrder;
                else
                    border = orderDetailPo.bridgeOrder2;
            }
            return getAgreementNO(border, isOfr);
        }
        /// <summary>
        /// 将桥的预定号插入到文本中
        /// </summary>
        /// <param name="str"></param>
        /// <param name="orderDetailPo"></param>
        /// <param name="bridge"></param>
        public static void setAgreementNO(ref string str, BridgeOrderDetail orderDetailPo, UserBridge bridge)
        {
            string ofrAgreementNO = getAgreementNO(orderDetailPo, bridge, true);
            string bidAgreementNO = getAgreementNO(orderDetailPo, bridge, false);
            int a = str.IndexOf(")");
            if (a > 0)
            {
                str = str.Insert(a, ofrAgreementNO);
            }
            int b = str.LastIndexOf(")");
            if (b > 0)
            {
                str = str.Insert(b, bidAgreementNO);
            }
        }
        public static string CreateBridgeTxt(BridgeOrderDetail orderDetailPo, string bridgeType, UserBridge bridge)
        {
            string result = string.Empty;
            string bridgesend = bridge.sendto;
            bool bridgeStatus = (bridgesend + "").Equals(orderDetailPo.BridgeSendTo + "");
            bool bridgeStatusO = (bridgesend + "").Equals(orderDetailPo.BridgeSendTo2 + "");
            //发单机构
            string ofrSendOrderInst = "";
            string bidSendOrderInst = "";


            if (orderDetailPo != null && orderDetailPo.ofrIssueInfoReqList != null && orderDetailPo.ofrIssueInfoReqList.Count > 0)
            {
                foreach (SendInstSevieceModel item in orderDetailPo.ofrIssueInfoReqList)
                {
                    ofrSendOrderInst += item.issueInstName + " " + item.qty + "+";
                }
                ofrSendOrderInst = ofrSendOrderInst.TrimEnd('+');
            }

            if (orderDetailPo != null && orderDetailPo.bidIssueInfoReqList != null && orderDetailPo.bidIssueInfoReqList.Count > 0)
            {
                foreach (SendInstSevieceModel item in orderDetailPo.bidIssueInfoReqList)
                {
                    bidSendOrderInst += item.issueInstName + " " + item.qty + "+";
                }
                bidSendOrderInst = bidSendOrderInst.TrimEnd('+');
            }

            decimal d = 0;
            if (bridgeType.Equals("B,O"))
            {
                //发给显示
                string bridgesendContent = string.Empty;
                //渠道显示
                string channelTxt = string.Empty;

                //发给显示
                string bridgesendContentB = string.Empty;
                //渠道显示
                string channelTxtB = string.Empty;

                string bankAgentName = "";
                string ifrInstName = orderDetailPo.IsGVN ? orderDetailPo.InstAShow : orderDetailPo.InstBShow;
                string bidInstName = orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow;
                if (orderDetailPo.bridgeOrder != null)
                {
                    //ofr方向
                    if (orderDetailPo.bridgeOrder.ofrDir == 1)
                    {
                        bankAgentName = orderDetailPo.IsGVN ? orderDetailPo.TraderName : orderDetailPo.CounterPartyTraderName;
                        if (bankAgentName == orderDetailPo.BridgeSendTo)
                        {
                            string channelContent = GetSelectChannelTxt(orderDetailPo.BridgeChannel);
                            if (orderDetailPo.BridgeChannel == "2")
                            {
                                bridgesendContent = channelContent + ifrInstName;
                            }
                            else if (orderDetailPo.BridgeChannel == "1" ||
                                     orderDetailPo.BridgeChannel == "3" ||
                                     orderDetailPo.BridgeChannel == "4" ||
                                     orderDetailPo.BridgeChannel == "5" ||
                                     orderDetailPo.BridgeChannel == "11")
                            {
                                bridgesendContent = channelContent + "发" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else if (orderDetailPo.BridgeChannel == "6" ||
                                     orderDetailPo.BridgeChannel == "7" ||
                                     orderDetailPo.BridgeChannel == "8")
                            {

                                bridgesendContent = channelContent + "联系" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else if (orderDetailPo.BridgeChannel == "9" ||
                                     orderDetailPo.BridgeChannel == "10")
                            {
                                bridgesendContent = "点" + channelContent + "," + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else
                            {
                                bridgesendContent = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            channelTxt = string.Empty;
                        }
                        else
                        {
                            bridgesendContent = orderDetailPo.BridgeSendTo;
                            channelTxt = string.Empty;
                        }

                    }
                    else
                    {
                        if (!bridgeStatus)
                        {
                            bridgesendContent = orderDetailPo.BridgeSendTo;
                            channelTxt = string.Empty;
                        }
                        else
                        {
                            channelTxt = GetSelectChannelTxt(orderDetailPo.BridgeChannel);
                        }
                    }

                    //bid方向
                    if (orderDetailPo.bridgeOrder.bidDir == 1)
                    {
                        bankAgentName = orderDetailPo.IsGVN ? orderDetailPo.CounterPartyTraderName : orderDetailPo.TraderName;

                        if (bankAgentName == orderDetailPo.BridgeSendTo2)
                        {
                            string channelContent = GetSelectChannelTxt(orderDetailPo.BridgeChannel2);
                            if (orderDetailPo.BridgeChannel2 == "2")
                            {
                                bridgesendContentB = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "1" ||
                                    orderDetailPo.BridgeChannel2 == "3" ||
                                    orderDetailPo.BridgeChannel2 == "4" ||
                                    orderDetailPo.BridgeChannel2 == "5" ||
                                    orderDetailPo.BridgeChannel2 == "11")
                            {
                                bridgesendContentB = channelContent + "发" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "6" ||
                                     orderDetailPo.BridgeChannel2 == "7" ||
                                     orderDetailPo.BridgeChannel2 == "8")
                            {

                                bridgesendContentB = channelContent + "联系" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "9" ||
                                     orderDetailPo.BridgeChannel2 == "10")
                            {
                                bridgesendContentB = "点" + channelContent + "," + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else
                            {
                                bridgesendContentB = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            channelTxtB = string.Empty;
                        }
                        else
                        {
                            bridgesendContentB = orderDetailPo.BridgeSendTo2;
                            channelTxtB = string.Empty;
                        }
                    }
                    else
                    {
                        if (!bridgeStatusO)
                        {
                            bridgesendContentB = orderDetailPo.BridgeSendTo2;
                            channelTxtB = string.Empty;
                        }
                        else
                        {
                            channelTxtB = GetSelectChannelTxt(orderDetailPo.BridgeChannel2);
                        }
                    }
                }

                //V2.3新的需求：.IB不显示 .SZ和.SH要显示 20200316
                result = string.Format("{0}  {1}{2}  {3}  {4}  {5}({6}{7}{8}{9}{10}) 出给 {11}({12}{13}{14}{15}{16})",
                    //orderDetailPo.goodsCode.Substring(0, orderDetailPo.goodsCode.Length - 3),
                    orderDetailPo.SecurityID.Replace(".IB", ""),
                    orderDetailPo.PriceFormat,

                    string.IsNullOrEmpty(orderDetailPo.fandian) ? string.Empty : decimal.TryParse(orderDetailPo.fandian, out d) ? "F" + orderDetailPo.fandian : orderDetailPo.fandian,
                    ConvertQty(orderDetailPo.qty != null ? orderDetailPo.qty.HasValue ? orderDetailPo.qty.Value : 0 : 0),
                    OrderHelper.TodayReceipt(orderDetailPo) ? OrderHelper.GetHistoryDisplaySettle(orderDetailPo) : OrderHelper.GetDisplaySettle(orderDetailPo),

                    //orderDetailPo.IsGVN ? orderDetailPo.InstAShow : orderDetailPo.TraderBInfo,
                    GetTeaderInfo(orderDetailPo, true),
                    //orderDetailPo.IsGVN ? string.IsNullOrEmpty(orderDetailPo.bankcodeshorta) ? orderDetailPo.bankCodeA : orderDetailPo.bankcodeshorta
                    //: string.IsNullOrEmpty(orderDetailPo.bankcodeshortb) ? orderDetailPo.bankCodeB : orderDetailPo.bankcodeshortb,

                    //bridgeStatus ? string.Empty : orderDetailPo.bridgesendto,
                    bridgesendContent,
                    channelTxt,
                    //bridgeStatus ? GetSelectChannelTxt(orderDetailPo.bridgechannel) : string.IsNullOrEmpty(orderDetailPo.bridgechannel) ? string.Empty : "," + GetSelectChannelTxt(orderDetailPo.bridgechannel),
                    bridgeStatus ? orderDetailPo.BridgeFee : string.IsNullOrEmpty(orderDetailPo.BridgeFee) ? string.Empty : "," + orderDetailPo.BridgeFee,
                     //发单机构
                     string.IsNullOrWhiteSpace(ofrSendOrderInst) ? string.Empty : "," + ofrSendOrderInst,

                    string.IsNullOrEmpty(orderDetailPo.BridgeRemark) ? string.Empty : (!bridgeStatus || !string.IsNullOrEmpty(orderDetailPo.BridgeChannel) || !string.IsNullOrEmpty(orderDetailPo.BridgeFee)) ? "," + orderDetailPo.BridgeRemark : orderDetailPo.BridgeRemark,

                    //orderDetailPo.IsGVN ? orderDetailPo.TraderBInfo : orderDetailPo.InstAShow,
                    GetTeaderInfo(orderDetailPo, false),
                     //orderDetailPo.IsGVN ? string.IsNullOrEmpty(orderDetailPo.bankcodeshortb) ? orderDetailPo.bankCodeB : orderDetailPo.bankcodeshortb
                     //: string.IsNullOrEmpty(orderDetailPo.bankcodeshorta) ? orderDetailPo.bankCodeA : orderDetailPo.bankcodeshorta,


                     bridgesendContentB,
                     channelTxtB,
                    //bridgeStatusO ? string.Empty : orderDetailPo.bridgesendto2,

                    //bridgeStatusO ? GetSelectChannelTxt(orderDetailPo.bridgechannel2) : string.IsNullOrEmpty(orderDetailPo.bridgechannel2) ? string.Empty : string.IsNullOrEmpty(orderDetailPo.bridgesendto2) ? GetSelectChannelTxt(orderDetailPo.bridgechannel2) : "," + GetSelectChannelTxt(orderDetailPo.bridgechannel2),
                    bridgeStatusO ? orderDetailPo.BridgeFee2 : string.IsNullOrEmpty(orderDetailPo.BridgeFee2) ? string.Empty : string.IsNullOrEmpty(orderDetailPo.BridgeSendTo2) && string.IsNullOrEmpty(orderDetailPo.BridgeChannel2) ? orderDetailPo.BridgeFee2 : "," + orderDetailPo.BridgeFee2,

                    //发单机构
                    string.IsNullOrWhiteSpace(bidSendOrderInst) ? string.Empty : "," + bidSendOrderInst,

                    string.IsNullOrEmpty(orderDetailPo.BridgeRemark2) ? string.Empty : (!string.IsNullOrEmpty(orderDetailPo.BridgeSendTo2) || !string.IsNullOrEmpty(orderDetailPo.BridgeChannel2) || !string.IsNullOrEmpty(orderDetailPo.BridgeFee2)) ? "," + orderDetailPo.BridgeRemark2 : orderDetailPo.BridgeRemark2);
            }
            else if (bridgeType.Equals("B"))
            {
                //发给显示
                string bridgesendContentB = string.Empty;
                //渠道显示
                string channelTxtB = string.Empty;

                string bankAgentName = string.Empty;

                int bidDir = 0;
                if (orderDetailPo.bridgeOrder != null)
                {
                    if (orderDetailPo.BridgeTraderId2 == orderDetailPo.bridgeOrder.instTradeId
                        && orderDetailPo.BridgeInstitutionId2 == orderDetailPo.bridgeOrder.instId)
                    {
                        bidDir = orderDetailPo.bridgeOrder.bidDir;
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder2 != null)
                        {
                            if (orderDetailPo.BridgeTraderId2 == orderDetailPo.bridgeOrder2.instTradeId
                                && orderDetailPo.BridgeInstitutionId2 == orderDetailPo.bridgeOrder2.instId)
                            {
                                bidDir = orderDetailPo.bridgeOrder2.bidDir;
                            }
                        }
                    }
                    //bid方向
                    if (bidDir == 1)
                    {
                        bankAgentName = orderDetailPo.IsGVN ? orderDetailPo.CounterPartyTraderName : orderDetailPo.TraderName;
                        if (bankAgentName == orderDetailPo.BridgeSendTo2)
                        {
                            string channelContent = GetSelectChannelTxt(orderDetailPo.BridgeChannel2);
                            if (orderDetailPo.BridgeChannel2 == "2")
                            {
                                bridgesendContentB = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "1" ||
                                    orderDetailPo.BridgeChannel2 == "3" ||
                                    orderDetailPo.BridgeChannel2 == "4" ||
                                    orderDetailPo.BridgeChannel2 == "5" ||
                                    orderDetailPo.BridgeChannel2 == "11")
                            {
                                bridgesendContentB = channelContent + "发" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "6" ||
                                     orderDetailPo.BridgeChannel2 == "7" ||
                                     orderDetailPo.BridgeChannel2 == "8")
                            {

                                bridgesendContentB = channelContent + "联系" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else if (orderDetailPo.BridgeChannel2 == "9" ||
                                     orderDetailPo.BridgeChannel2 == "10")
                            {
                                bridgesendContentB = "点" + channelContent + "," + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            else
                            {
                                bridgesendContentB = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderBShow : orderDetailPo.InstAndTraderAShow);
                            }
                            channelTxtB = string.Empty;
                        }
                        else
                        {
                            bridgesendContentB = orderDetailPo.BridgeSendTo2;
                            channelTxtB = string.Empty;
                        }

                    }
                    else
                    {
                        if (!bridgeStatusO)
                        {
                            bridgesendContentB = orderDetailPo.BridgeSendTo2;
                            channelTxtB = string.Empty;
                        }
                        else
                        {
                            channelTxtB = GetSelectChannelTxt(orderDetailPo.BridgeChannel2);
                        }
                    }
                }
                result = string.Format("{0}  {1}{2}  {3}  {4}  {5}({6}{7}{8}{9}) 出给 {10}({11}{12}{13}{14}{15})",
                  //orderDetailPo.goodsCode.Substring(0, orderDetailPo.goodsCode.Length - 3),
                  orderDetailPo.SecurityID.Replace(".IB", ""),
                orderDetailPo.PriceFormat,
                string.IsNullOrEmpty(orderDetailPo.fandian) ? string.Empty : decimal.TryParse(orderDetailPo.fandian, out d) ? "F" + orderDetailPo.fandian : orderDetailPo.fandian,
                ConvertQty(orderDetailPo.qty != null ? orderDetailPo.qty.HasValue ? orderDetailPo.qty.Value : 0 : 0),
                OrderHelper.TodayReceipt(orderDetailPo) ? GetHistoryDoubleBridgeSettle(orderDetailPo, true) : GetDefaultSettle(orderDetailPo, true),
                //GetDefaultSettle(orderDetailPo, true),

                GetBankcode(orderDetailPo, bridgeType, bridge),
                //bridgeType.Equals(orderDetailPo.bridgeType) ? string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort2) ? orderDetailPo.bridgebankcode2 : orderDetailPo.bridgebankcodeshort2
                //: string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort) ? orderDetailPo.bridgebankcode : orderDetailPo.bridgebankcodeshort,
                //orderDetailPo.IsGVN ? string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort) ? orderDetailPo.bridgebankcode : orderDetailPo.bridgebankcodeshort
                //: string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort2) ? orderDetailPo.bridgebankcode2 : orderDetailPo.bridgebankcodeshort2,

                string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? string.Empty : "发" + orderDetailPo.DoubleBridgeSendTo,
                string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel) ? !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? ",对话" : "对话" : !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? "," + GetSelectChannelTxt(orderDetailPo.DoubleBridgeChannel) : GetSelectChannelTxt(orderDetailPo.DoubleBridgeChannel),
                orderDetailPo.DoubleBridgeFee,
                string.IsNullOrEmpty(orderDetailPo.DoubleBridgeRemark) ? string.Empty : (!string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) || !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel) || !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeFee)) ? "," + orderDetailPo.DoubleBridgeRemark : orderDetailPo.DoubleBridgeRemark,
                  orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow,
                //orderDetailPo.IsGVN ? string.IsNullOrEmpty(orderDetailPo.bankcodeshortb) ? orderDetailPo.bankCodeB : orderDetailPo.bankcodeshortb
                //: string.IsNullOrEmpty(orderDetailPo.bankcodeshorta) ? orderDetailPo.bankCodeA : orderDetailPo.bankcodeshorta,


                  bridgesendContentB,
                  channelTxtB,
                //bridgeStatusO ? string.Empty : orderDetailPo.bridgesendto2,

                //bridgeStatusO ? GetSelectChannelTxt(orderDetailPo.bridgechannel2) : !string.IsNullOrEmpty(orderDetailPo.bridgechannel2) ? "," + GetSelectChannelTxt(orderDetailPo.bridgechannel2) : string.Empty,
                bridgeStatusO ? orderDetailPo.BridgeFee2 : string.IsNullOrEmpty(orderDetailPo.BridgeFee2) ? string.Empty : "," + orderDetailPo.BridgeFee2,
                //发单机构
                string.IsNullOrWhiteSpace(bidSendOrderInst) ? string.Empty : "," + bidSendOrderInst,
                string.IsNullOrEmpty(orderDetailPo.BridgeRemark2) ? string.Empty : (!bridgeStatusO || !string.IsNullOrEmpty(orderDetailPo.BridgeChannel2) || !string.IsNullOrEmpty(orderDetailPo.BridgeFee2)) ? "," + orderDetailPo.BridgeRemark2 : orderDetailPo.BridgeRemark2);
            }
            else if (bridgeType.Equals("O"))
            {
                //发给显示
                string bridgesendContent = string.Empty;
                //渠道显示
                string channelTxt = string.Empty;
                string bankAgentName = string.Empty;

                int ofrDir = 0;

                string ifrInstName = orderDetailPo.IsGVN ? orderDetailPo.InstAShow : orderDetailPo.InstBShow;
                string bidInstName = orderDetailPo.IsGVN ? orderDetailPo.InstBShow : orderDetailPo.InstAShow;
                if (orderDetailPo.bridgeOrder != null)
                {
                    if (orderDetailPo.BridgeTraderId == orderDetailPo.bridgeOrder.instTradeId
                       && orderDetailPo.BridgeInstitutionId == orderDetailPo.bridgeOrder.instId)
                    {
                        ofrDir = orderDetailPo.bridgeOrder.ofrDir;
                    }
                    else
                    {
                        if (orderDetailPo.bridgeOrder2 != null)
                        {
                            if (orderDetailPo.BridgeTraderId == orderDetailPo.bridgeOrder2.instTradeId
                                && orderDetailPo.BridgeInstitutionId == orderDetailPo.bridgeOrder2.instId)
                            {
                                ofrDir = orderDetailPo.bridgeOrder2.ofrDir;
                            }
                        }
                    }

                    //ofr方向
                    if (ofrDir == 1)
                    {
                        bankAgentName = orderDetailPo.IsGVN ? orderDetailPo.TraderName : orderDetailPo.CounterPartyTraderName;
                        if (bankAgentName == orderDetailPo.BridgeSendTo)
                        {
                            string channelContent = GetSelectChannelTxt(orderDetailPo.BridgeChannel);
                            if (orderDetailPo.BridgeChannel == "2")
                            {
                                bridgesendContent = channelContent + ifrInstName;
                            }
                            else if (orderDetailPo.BridgeChannel == "1" ||
                                    orderDetailPo.BridgeChannel == "3" ||
                                    orderDetailPo.BridgeChannel == "4" ||
                                    orderDetailPo.BridgeChannel == "5" ||
                                    orderDetailPo.BridgeChannel == "11")
                            {
                                bridgesendContent = channelContent + "发" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else if (orderDetailPo.BridgeChannel == "6" ||
                                     orderDetailPo.BridgeChannel == "7" ||
                                     orderDetailPo.BridgeChannel == "8")
                            {

                                bridgesendContent = channelContent + "联系" + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else if (orderDetailPo.BridgeChannel == "9" ||
                                     orderDetailPo.BridgeChannel == "10")
                            {
                                bridgesendContent = "点" + channelContent + "," + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            else
                            {
                                bridgesendContent = channelContent + (orderDetailPo.IsGVN ? orderDetailPo.InstAndTraderAShow : orderDetailPo.InstAndTraderBShow);
                            }
                            channelTxt = string.Empty;
                        }
                        else
                        {
                            bridgesendContent = orderDetailPo.BridgeSendTo;
                            channelTxt = string.Empty;
                        }
                    }
                    else
                    {
                        if (!bridgeStatus)
                        {
                            bridgesendContent = orderDetailPo.BridgeSendTo;
                            channelTxt = string.Empty;
                        }
                        else
                        {
                            channelTxt = GetSelectChannelTxt(orderDetailPo.BridgeChannel);
                        }
                    }
                }
                result = string.Format("{0}  {1}{2}  {3}  {4}  {5}({6}{7}{8}{9}{10}) 出给 {11}({12}{13}{14}{15})",
                      // orderDetailPo.goodsCode.Substring(0, orderDetailPo.goodsCode.Length - 3),
                      orderDetailPo.SecurityID.Replace(".IB", ""),
                    orderDetailPo.PriceFormat,
                    string.IsNullOrEmpty(orderDetailPo.fandian) ? string.Empty : decimal.TryParse(orderDetailPo.fandian, out d) ? "F" + orderDetailPo.fandian : orderDetailPo.fandian,
                    ConvertQty(orderDetailPo.qty != null ? orderDetailPo.qty.HasValue ? orderDetailPo.qty.Value : 0 : 0),

                    OrderHelper.TodayReceipt(orderDetailPo) ? GetHistoryDoubleBridgeSettle(orderDetailPo, false) : GetDefaultSettle(orderDetailPo, false),
                    orderDetailPo.IsGVN ? orderDetailPo.InstAShow : orderDetailPo.InstBShow,
                    //orderDetailPo.IsGVN ? string.IsNullOrEmpty(orderDetailPo.bankcodeshorta) ? orderDetailPo.bankCodeA : orderDetailPo.bankcodeshorta
                    //: string.IsNullOrEmpty(orderDetailPo.bankcodeshortb) ? orderDetailPo.bankCodeB : orderDetailPo.bankcodeshortb,

                    bridgesendContent,
                     channelTxt,
                    //bridgeStatus ? string.Empty : orderDetailPo.bridgesendto,

                    //bridgeStatus ? GetSelectChannelTxt(orderDetailPo.bridgechannel) : !string.IsNullOrEmpty(orderDetailPo.bridgechannel) ? "," + GetSelectChannelTxt(orderDetailPo.bridgechannel) : string.Empty,
                    bridgeStatus ? orderDetailPo.BridgeFee : string.IsNullOrEmpty(orderDetailPo.BridgeFee) ? string.Empty : "," + orderDetailPo.BridgeFee,
                   //发单机构
                   string.IsNullOrWhiteSpace(ofrSendOrderInst) ? string.Empty : "," + ofrSendOrderInst,
                    string.IsNullOrEmpty(orderDetailPo.BridgeRemark) ? string.Empty : (!bridgeStatus || !string.IsNullOrEmpty(orderDetailPo.BridgeChannel) || !string.IsNullOrEmpty(orderDetailPo.BridgeFee)) ? "," + orderDetailPo.BridgeRemark : orderDetailPo.BridgeRemark,

                     GetBankcode(orderDetailPo, bridgeType, bridge),
                    //bridgeType.Equals(orderDetailPo.bridgeType) ? string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort2) ? orderDetailPo.bridgebankcode2 : orderDetailPo.bridgebankcodeshort2
                    //: string.IsNullOrEmpty(orderDetailPo.bridgebankcodeshort) ? orderDetailPo.bridgebankcode : orderDetailPo.bridgebankcodeshort,
                    //orderDetailPo.bridgebankcode2,
                    string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? string.Empty : "发" + orderDetailPo.DoubleBridgeSendTo,
                    string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel) ? !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? ",对话" : "对话" : !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) ? "," + GetSelectChannelTxt(orderDetailPo.DoubleBridgeChannel) : GetSelectChannelTxt(orderDetailPo.DoubleBridgeChannel),
                    orderDetailPo.DoubleBridgeFee,
                    string.IsNullOrEmpty(orderDetailPo.DoubleBridgeRemark) ? string.Empty : (!string.IsNullOrEmpty(orderDetailPo.DoubleBridgeSendTo) || !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel) || !string.IsNullOrEmpty(orderDetailPo.DoubleBridgeFee)) ? "," + orderDetailPo.DoubleBridgeRemark : orderDetailPo.DoubleBridgeRemark);
            }

            if (GRSettingsHandler.IsShowShortName())
            {
                string goodsShortName = orderDetailPo.goodsShortName;
                if (GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype))
                {
                    BondsInfoPo bond = STC.Broker.Plugin.Utils.CommonUtils.GetBondsInfoByCode(orderDetailPo.SecurityID);
                    //CRMDataHandler.Instance.GetBondsInfoByCode(orderDetailPo.SecurityID);
                    if (bond != null && !string.IsNullOrWhiteSpace(bond.Asset_Status) && bond.Asset_Status == "PRI")
                    {
                        goodsShortName = goodsShortName + " " + "私募债";
                    }
                }

                result = string.Format("{0}  {1}", goodsShortName, result);
            }
            else
            {
                string goodsShortName = "";
                if (GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype))
                {
                    BondsInfoPo bond = STC.Broker.Plugin.Utils.CommonUtils.GetBondsInfoByCode(orderDetailPo.SecurityID);
                    if (bond != null && !string.IsNullOrWhiteSpace(bond.Asset_Status) && bond.Asset_Status == "PRI")
                    {
                        goodsShortName ="私募债";
                        result = string.Format("{0}  {1}", goodsShortName, result);
                    }
                }
            }


            if (GRSettingsHandler.IsShowTenor())
                result = string.Format("{0}  {1}", orderDetailPo.timeToMaturity, result);

            setAgreementNO(ref result, orderDetailPo, bridge);
            return result;
        }

        /// <summary>
        ///  //短融/超短融债券，有备注时处理
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static string GetBridgeTxt(BridgeOrderDetail orderDetailPo, UserBridge bridge, string oldTxt)
        {
            string result = oldTxt;
            //20200907 新增
            //短融/超短融债券列表
            if (IsShortFuse(orderDetailPo.goodsSubType))
            {
                //获取债券发行量
                string bondIssueAmount = GetBondIssueAmount(orderDetailPo.SecurityID);
                //发行量为空不显示
                if (string.IsNullOrWhiteSpace(bondIssueAmount))
                {
                    //先判断桥
                    if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId && bridge.bankTraderid == orderDetailPo.BridgeTraderId)
                    {
                        if (orderDetailPo.bridgeOrder != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder.combinedOrderRemark))
                        {
                            result = $"{result} ({orderDetailPo.bridgeOrder.combinedOrderRemark})";
                        }
                    }
                    else if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId2 && bridge.bankTraderid == orderDetailPo.BridgeTraderId2)
                    {
                        if (orderDetailPo.bridgeOrder2 != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder2.combinedOrderRemark))
                        {
                            result = $"{result} ({orderDetailPo.bridgeOrder2.combinedOrderRemark})";
                        }
                    }
                }
                else
                {

                    //先判断桥
                    if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId && bridge.bankTraderid == orderDetailPo.BridgeTraderId)
                    {
                        if (orderDetailPo.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder.combinedOrderRemark))
                        {
                            result = $"{result} (发行量{bondIssueAmount}亿)";
                        }
                        else
                        {
                            result = $"{result} (发行量{bondIssueAmount}亿,{orderDetailPo.bridgeOrder.combinedOrderRemark})";
                        }
                    }
                    else if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId2 && bridge.bankTraderid == orderDetailPo.BridgeTraderId2)
                    {
                        if (orderDetailPo.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder2.combinedOrderRemark))
                        {
                            result = $"{result} (发行量{bondIssueAmount}亿)";
                        }
                        else
                        {
                            result = $"{result} (发行量{bondIssueAmount}亿,{orderDetailPo.bridgeOrder2.combinedOrderRemark})";
                        }
                    }
                }
            }
            else
            {
                //先判断桥
                if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId && bridge.bankTraderid == orderDetailPo.BridgeTraderId)
                {
                    if (orderDetailPo.bridgeOrder != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder.combinedOrderRemark))
                    {
                        result = $"{result} ({orderDetailPo.bridgeOrder.combinedOrderRemark})";
                    }
                }
                else if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId2 && bridge.bankTraderid == orderDetailPo.BridgeTraderId2)
                {
                    if (orderDetailPo.bridgeOrder2 != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder2.combinedOrderRemark))
                    {
                        result = $"{result} ({orderDetailPo.bridgeOrder2.combinedOrderRemark})";
                    }
                }
            }
            return result;
        }



        /// <summary>
        /// 债券是否为短融超短融,中票
        /// </summary>
        /// <param name="bondCode"> 债券代码</param>
        /// <returns>true:是,false：否</returns>
        public static bool IsShortFuse(string bondCode)
        {
            if (string.IsNullOrWhiteSpace(bondCode))
            {
                return false;
            }
            bool isShortFuse = false;
            //HashSet<string> bondsType = new HashSet<string> { "CCP", "LCP", "SSB", "LSP", "CSP", "CMN","LMN" };
            HashSet<string> bondsType = new HashSet<string> { "CCP", "LCP", "SSB", "LSP", "CSP" };
            if (bondsType.Contains(bondCode))
                isShortFuse = true;
            return isShortFuse;
        }

        /// <summary>
        /// 获取债券发行量
        /// </summary>
        /// <returns></returns>
        private static string GetBondIssueAmount(string bondCode)
        {
            string bondIssueAmount = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(bondCode))
                {
                    return bondIssueAmount;
                }

                //BondsInfoPo bond = CRMDataHandler.Instance.GetBondsInfoByCode(bondCode);
                BondsInfoPo bond = STC.Broker.Plugin.Utils.CommonUtils.GetBondsInfoByCode(bondCode);
                if (bond != null)
                {
                    bondIssueAmount = bond.Issue_Amount.HasValue ? ConvertTextHelper.FormatIssueaMount(bond.Issue_Amount.ToString()) :"";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(BridgeHelper) + ex.ToString());
            }
            finally
            {

            }
            return bondIssueAmount;
        }
        /// <summary>
        /// 获取全量数据缓存关键字
        /// </summary>
        /// <param name="pluginShareKey"></param>
        /// <returns></returns>
        private static string GetPluginShareKey(Common.IDCEnum.EPluginShareKey pluginShareKey)
        {
            return string.Format("{0}_{1}", Math.Floor((int)pluginShareKey / 10.0), pluginShareKey.ToString());
        }

        public static DateTime ParseTime(string time)
        {
            DateTime dt;
            DateTime.TryParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            return dt;
        }
        /// <summary>
        /// 双桥结算的显示 当天的单子
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isBid"></param>
        /// <returns></returns>
        private static string GetHistoryDoubleBridgeSettle(BridgeOrderDetail orderDetail, bool isBid)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非默认结算类型，且O,B结算类型一致
            if (!string.IsNullOrEmpty(orderDetail.SettleDisplay2) && orderDetail.SettleDisplay.Equals(orderDetail.SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(orderDetail.remarks))
                {
                    dtDeal = OrderHelper.GetDate(orderDetail.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + orderDetail.remarks;
                }
                else
                {
                    dtDeal = OrderHelper.GetDate(orderDetail.dealdate);
                    DateTime dtSettle = OrderHelper.GetDate(orderDetail.SettlDate);
                    DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    var time = dtSettle - dtDeal;
                    ///成交日和结算日同一天
                    if (time.Days == 0)
                    {
                        time = dtDeal - dtNow;
                        //成交日是当天的话
                        if (time.Days == 0)
                            result = "+0";
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+0";
                    }
                    else
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = OrderHelper.GetDefautSettle(orderDetail.SettlType2);
                        else
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + OrderHelper.GetDefautSettle(orderDetail.SettlType2);
                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                //ofr
                ofrResult = OrderHelper.GetHistorySettle(orderDetail, true);
                //bid
                bidResult = OrderHelper.GetHistorySettle(orderDetail, false);
                if (isBid)
                {
                    if (orderDetail.doubleBridgeSettleBidDisplay.Contains("/"))
                    {

                        result = ofrResult + "/" + bidResult;
                    }
                    else
                    {
                        result = bidResult;
                    }
                }
                else
                {
                    if (orderDetail.doubleBridgeSettleOfrDisplay.Contains("/"))
                    {

                        result = ofrResult + "/" + bidResult;
                    }
                    else
                    {
                        result = ofrResult;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 双桥结算的显示 当天的单子
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isBid"></param>
        /// <returns></returns>
        private static string GetDefaultSettle(BridgeOrderDetail orderDetail, bool isBid)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(orderDetail.SettleDisplay2) && orderDetail.SettleDisplay.Equals(orderDetail.SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(orderDetail.remarks))
                {
                    DateTime dtDeal = OrderHelper.GetDate(orderDetail.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + orderDetail.remarks;
                }
                else
                    result = orderDetail.doubleBridgeSettleOfrDisplay;
            }
            else
            {
                if (isBid)
                    result = orderDetail.doubleBridgeSettleBidDisplay;
                else
                    result = orderDetail.doubleBridgeSettleOfrDisplay;
            }
            return result;
        }

        public static string GetSelectChannelTxt(string selectIndex)
        {
            if (selectIndex == "1")
                return "对话";
            else if (selectIndex == "2")
                return "请求";
            else if (selectIndex == "3")
                return "双边";
            else if (selectIndex == "4")
                return "xbond";
            else if (selectIndex == "5")
                return "iDeal";
            else if (selectIndex == "6")
                return "固收";
            else if (selectIndex == "7")
                return "竞价";
            else if (selectIndex == "8")
                return "大宗";
            else if (selectIndex == "9")
                return "意向";
            else if (selectIndex == "10")
                return "匿名";
            else if (selectIndex == "11")
                return "xbargain";

            return string.Empty;
        }

        public static int GetSpecialSeq(decimal? seq, string goodsType)
        {
            var baseSeq = 0;
            goodsType=CommonUtils.BizTypeConvertToCode(goodsType);
            switch (goodsType)
            {
                // 信用
                case "1":
                case "11":
                    {
                        baseSeq = 80000;
                        break;
                    }
                // 利率
                case "2":
                    {
                        baseSeq = 90000;
                        break;
                    }
                // NCD
                case "12":
                    {
                        baseSeq = 70000;
                        break;
                    }

            }
            return (int)seq + baseSeq;
        }

        /// <summary>
        /// 桥订单复制
        /// </summary>
        /// <param name="selectedCells">选中订单</param>
        /// <param name="bridge">桥</param>
        /// <param name="brigeCopyType">复制类型</param>
        public static void CopyBridgeInfoOrderTxt(IList<DataGridCellInfo> selectedCells, UserBridge bridge, BrigeCopyType brigeCopyType)
        {
            string result = string.Empty;
            string html = string.Empty;
            int recordCount = 0;
            //记录债券id
            string odbondId = string.Empty;
            //记录债券备注
            string oldRmark = string.Empty;
            for (int i = 0; i < selectedCells.Count; i = i + 11)
            {
                BridgeOrderDetail orderDetail = selectedCells[i].Item as BridgeOrderDetail;
                if (orderDetail != null && orderDetail.OfrBridgeStateShow != "已取消"
                    && orderDetail.BidBridgeStateShow != "已取消"
                    && !string.IsNullOrWhiteSpace(orderDetail.OldOrderTxt))
                {
                    string orderTxt = string.Empty;
                    switch (brigeCopyType)
                    {
                        case BrigeCopyType.Normal:
                            {
                                orderTxt = $"{orderDetail.ShowBrokerNameA}VS{orderDetail.ShowBrokerNameB} {orderDetail.seq}-{orderDetail.OldOrderTxt}";
                                break;
                            }
                        case BrigeCopyType.WithoutSeq:
                            {
                                orderTxt = orderDetail.OldOrderTxt;
                                break;
                            }
                        case BrigeCopyType.SpecialSeq:
                            {
                                orderTxt = $"{orderDetail.ShowBrokerNameA}VS{orderDetail.ShowBrokerNameB} {GetSpecialSeq(orderDetail.seq, orderDetail.goodstype)}-{orderDetail.OldOrderTxt}";
                                break;
                            }
                        case BrigeCopyType.NoBroker:
                            {
                                orderTxt = $"{orderDetail.seq}-{orderDetail.OldOrderTxt}";
                                break;
                            }
                        default:
                            break;
                    }

                    recordCount++;
                    //第一条选中的数据  
                    if (recordCount == 1)
                    {
                        odbondId = orderDetail.SecurityID;
                        //债券是否为短融超短融
                        if (IsShortFuse(orderDetail.goodsSubType))
                        {
                            //获取债券发行量
                            string bondIssueAmount = GetBondIssueAmount(orderDetail.SecurityID);
                            if (string.IsNullOrWhiteSpace(bondIssueAmount))
                            {
                                if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                                && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                                {
                                    //发行量为空，备注为空
                                    result += $"{orderTxt}\r\n";
                                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                                else
                                {
                                    string rmark = GetRemarkTxt(orderDetail, bridge);
                                    oldRmark = rmark;
                                    //发行量为空，备注不为空
                                    result += $"{rmark}\r\n{orderTxt}\r\n";
                                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{rmark}</span></div></br>";
                                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                            }
                            else
                            {
                                if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                               && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                                {
                                    //发行量不为空，备注为空
                                    result += $"发行量{bondIssueAmount}亿\r\n{orderTxt}\r\n";
                                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">发行量{bondIssueAmount}亿</span></div></br>";
                                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                                else
                                {
                                    string rmark = GetRemarkTxt(orderDetail, bridge);
                                    oldRmark = rmark;
                                    //发行量不为空，备注不为空
                                    result += $"发行量{bondIssueAmount}亿     {rmark}\r\n{orderTxt}\r\n";
                                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">发行量{bondIssueAmount}亿&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{rmark}</span></div></br>";
                                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                            }
                        }
                        else
                        {
                            if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                                 && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                            {
                                //无发行量，备注为空
                                result += $"{orderTxt}\r\n";
                                html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                            }
                            else
                            {
                                string rmark = GetRemarkTxt(orderDetail, bridge);
                                oldRmark = rmark;
                                //无发行量，备注不为空
                                result += $"{rmark}\r\n{orderTxt}\r\n";
                                html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{rmark}</span></div></br>";
                                html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                            }
                        }
                    }
                    else
                    {
                        //与上一比单子的债券id是否相同
                        if (odbondId == orderDetail.SecurityID)
                        {
                            string rmark = GetRemarkTxt(orderDetail, bridge);
                            if (string.IsNullOrWhiteSpace(rmark))
                            {
                                result += $"{orderTxt}\r\n";
                                html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                            }
                            else
                            {
                                //备注是否相同
                                if (rmark == oldRmark)
                                {
                                    result += $"{orderTxt}\r\n";
                                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                                else
                                {
                                    result += $"\r\n{rmark}\r\n{orderTxt}\r\n";
                                    html += $"<br><div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{rmark}</span></div></br>";
                                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                                }
                            }
                        }
                        else
                        {
                            string htmlStr = string.Empty;
                            //获取债券发行量
                            string bondIssueAmount = GetBondIssueAmount(orderDetail.SecurityID);
                            string rmark = GetRemarkTxt(orderDetail, bridge);
                            if (IsShortFuse(orderDetail.goodsSubType))
                            {
                                if (string.IsNullOrWhiteSpace(bondIssueAmount) && string.IsNullOrWhiteSpace(rmark))
                                {
                                    result += $"{GetFormattingText(orderDetail, bridge, orderTxt, out odbondId, out oldRmark, out htmlStr)}";
                                    html += $"{htmlStr}";
                                }
                                else
                                {
                                    result += $"\r\n{GetFormattingText(orderDetail, bridge, orderTxt, out odbondId, out oldRmark, out htmlStr)}";
                                    html += $"<br>{htmlStr}";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(rmark))
                                {
                                    result += $"{GetFormattingText(orderDetail, bridge, orderTxt, out odbondId, out oldRmark, out htmlStr)}";
                                    html += $"{htmlStr}";
                                }
                                else
                                {
                                    result += $"\r\n{GetFormattingText(orderDetail, bridge, orderTxt, out odbondId, out oldRmark, out htmlStr)}";
                                    html += $"<br>{htmlStr}";
                                }
                            }
                        }
                    }

                }
            }

            Common.Utils.ClipboardHelper.CopyToClipboard(html, result);
        }

        /// <summary>
        /// 获取格式化文本
        /// </summary>
        /// <returns></returns>
        private static string GetFormattingText(BridgeOrderDetail orderDetail, UserBridge bridge, string orderTxt, out string odbondId, out string oldRmark, out string html)
        {
            html = string.Empty;
            oldRmark = string.Empty;
            odbondId = orderDetail.SecurityID;
            string result = string.Empty;
            //债券是否为短融超短融
            if (IsShortFuse(orderDetail.goodsSubType))
            {
                //获取债券发行量
                string bondIssueAmount = GetBondIssueAmount(orderDetail.SecurityID);
                if (string.IsNullOrWhiteSpace(bondIssueAmount))
                {
                    if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                    && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                    {
                        //发行量为空，备注为空
                        result += $"{orderTxt}\r\n";
                        html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                    }
                    else
                    {
                        string rmark = GetRemarkTxt(orderDetail, bridge);
                        oldRmark = rmark;
                        //发行量为空，备注不为空
                        result += $"{rmark}\r\n{orderTxt}\r\n";
                        html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{rmark}</span></div></br>";
                        html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                    }
                }
                else
                {
                    if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                   && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                    {
                        //发行量不为空，备注为空
                        result += $"发行量{bondIssueAmount}亿\r\n{orderTxt}\r\n";
                        html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">发行量{bondIssueAmount}亿</span></div></br>";
                        html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                    }
                    else
                    {
                        string rmark = GetRemarkTxt(orderDetail, bridge);
                        oldRmark = rmark;
                        //发行量不为空，备注不为空
                        result += $"发行量{bondIssueAmount}亿     {rmark}\r\n{orderTxt}\r\n";
                        html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">发行量{bondIssueAmount}亿&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{rmark}</span></div></br>";
                        html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                    }
                }
            }
            else
            {
                if ((orderDetail.bridgeOrder == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.combinedOrderRemark))
                     && (orderDetail.bridgeOrder2 == null || string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.combinedOrderRemark)))
                {
                    //无发行量，备注为空
                    result += $"{orderTxt}\r\n";
                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                }
                else
                {
                    string rmark = GetRemarkTxt(orderDetail, bridge);
                    oldRmark = rmark;
                    //无发行量，备注不为空
                    result += $"{rmark}\r\n{orderTxt}\r\n";
                    html = $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{rmark}</span></div></br>";
                    html += $"<div><span style =\"background:#White; color:#000000; font-family:Arial MT; font-size:15\">{orderTxt}</span></div></br>";
                }
            }
            return result;
        }

        /// <summary>
        /// 获取桥合单备注信息
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        private static string GetRemarkTxt(BridgeOrderDetail orderDetailPo, UserBridge bridge)
        {
            string result = string.Empty;
            //发行量为空，备注不为空
            if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId && bridge.bankTraderid == orderDetailPo.BridgeTraderId)
            {
                if (orderDetailPo.bridgeOrder != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder.combinedOrderRemark))
                {
                    result = orderDetailPo.bridgeOrder.combinedOrderRemark;
                }
            }
            else if (bridge.bankInstitutionid == orderDetailPo.BridgeInstitutionId2 && bridge.bankTraderid == orderDetailPo.BridgeTraderId2)
            {
                if (orderDetailPo.bridgeOrder2 != null && !string.IsNullOrWhiteSpace(orderDetailPo.bridgeOrder2.combinedOrderRemark))
                {
                    result = orderDetailPo.bridgeOrder2.combinedOrderRemark;
                }
            }
            return result;
        }
    }
}
