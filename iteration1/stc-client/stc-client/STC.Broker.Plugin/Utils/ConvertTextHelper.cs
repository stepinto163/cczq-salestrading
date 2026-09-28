using com.gateway.log;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.PT;
//using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace STC.Broker.Plugin.Utils
{
    public static class ConvertTextHelper
    {
        public static string Liquidation { get { return DateTime.Now.ToString("yyyyMMdd"); } } //new string[] { "+0", "+1" };
        /// <summary>
        /// 合并展示机构
        /// </summary>
        public static Dictionary<string, UserRoleSettingDetail> dicSet = new Dictionary<string, UserRoleSettingDetail>();

        /// <summary>
        /// 合并展示机构
        /// </summary>
        //public static Dictionary<string, List<InstDetialInfo>> _dicMergedInst = new Dictionary<string, List<InstDetialInfo>>();

        ///// <summary>
        ///// 合并展示机构
        ///// </summary>
        //public static Dictionary<string, List<InstDetialInfo>> _dicMergedInst = new Dictionary<string, List<InstDetialInfo>>();
        public static string NextDate;
        public static SystemParameter SetParam { get { return GRSettingsHandler.GlobSystemParameter; } }


       
        /// <summary>
        /// 判断机构，交易员是否相同
        /// </summary>
        /// <returns></returns>
        private static bool IsCatianInstTrader(DetailTrader detailTraderOder, DetailTrader detailTrader)
        {
            bool isCatian = false;
            //判断机构是否合并展示
            if (!string.IsNullOrWhiteSpace(detailTrader.Key))
            {
                if (dicSet.ContainsKey(detailTrader.Key.Trim()))
                {
                    if (detailTraderOder.InstID == detailTrader.InstID)
                    {
                        isCatian = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(detailTrader.TraderID) && !string.IsNullOrWhiteSpace(detailTraderOder.TraderID))
                    {
                        if (!string.IsNullOrWhiteSpace(detailTraderOder.InstID) && detailTraderOder.InstID == detailTrader.InstID && detailTraderOder.TraderID == detailTrader.TraderID)
                        {
                            isCatian = true;
                        }
                    }
                }
            }
            return isCatian;
        }


        const string agreementNOFomrt = " 约定号：{0}";
        /// <summary>
        /// 设置预订号
        /// </summary>
        /// <param name="order"></param>
        /// <param name="key"></param>
        /// <param name="o"></param>
        static void setAgreementNO(CopyDetailPo order, string key, OrderArgs o, string proxyBrokerId)
        {
            string insta = ConvertTextHelper.GetBand(order, 0);
            string instb = ConvertTextHelper.GetBand(order, 1);
            var currentId = string.IsNullOrEmpty(proxyBrokerId) ? Common.Utils.LocalEnv.UserInfo.UserId : proxyBrokerId;

            if (order.IsGVN)
            {
                ///机构A下的文本展示
                if (!string.IsNullOrWhiteSpace(insta) && (key.Contains(insta)) && currentId.Equals(order.brokerIdA))
                {
                    if ("B,O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                        }

                    }
                    else if ("B".Equals(order.BridgeType) || "O".Equals(order.BridgeType))
                    {

                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                        {
                            int a = o.Text.IndexOf(";bid");
                            if (a > 0)
                            {
                                o.Text = o.Text.Insert(a, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);

                            int b = o.HideCounterpartyText.IndexOf(";bid");
                            if (b > 0)
                            {
                                o.HideCounterpartyText = o.HideCounterpartyText.Insert(b, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);

                            int c = o.HideSendQQMsgText.IndexOf(";bid");
                            if (c > 0)
                            {
                                o.HideSendQQMsgText = o.HideSendQQMsgText.Insert(c, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);

                        }
                    }
                    else if (!string.IsNullOrEmpty(order.agreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.agreementNo);
                    }
                }
                ///机构B下的文本展示
                else if (!string.IsNullOrWhiteSpace(instb) && (key.Contains(instb)) && currentId.Equals(order.brokerIdB))
                {
                    if ("B,O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.bidAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                        }

                    }
                    else if ("B".Equals(order.BridgeType) || "O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder2 != null && !string.IsNullOrEmpty(order.bridgeOrder2.bidAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                        }

                    }
                    else if (!string.IsNullOrEmpty(order.agreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.agreementNo);
                    }
                }
            }
            else
            {
                ///机构A下的文本展示
                if (!string.IsNullOrWhiteSpace(insta) && (key.Contains(insta)) && currentId.Equals(order.brokerIdA))
                {
                    if ("B,O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.bidAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                        }

                    }
                    else if ("B".Equals(order.BridgeType) || "O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder2 != null && !string.IsNullOrEmpty(order.bridgeOrder2.bidAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                        }

                    }
                    else if (!string.IsNullOrEmpty(order.agreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.agreementNo);
                    }
                }
                ///机构B下的文本展示
                else if (!string.IsNullOrWhiteSpace(instb) && (key.Contains(instb)) && currentId.Equals(order.brokerIdB))
                {
                    if ("B,O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                        {
                            o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                            o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                            o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo);
                        }
                    }
                    else if ("B".Equals(order.BridgeType) || "O".Equals(order.BridgeType))
                    {
                        if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                        {
                            int a = o.Text.IndexOf(";bid");
                            if (a > 0)
                            {
                                o.Text = o.Text.Insert(a, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);

                            int b = o.HideCounterpartyText.IndexOf(";bid");
                            if (b > 0)
                            {
                                o.HideCounterpartyText = o.HideCounterpartyText.Insert(b, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);

                            int c = o.HideSendQQMsgText.IndexOf(";bid");
                            if (c > 0)
                            {
                                o.HideSendQQMsgText = o.HideSendQQMsgText.Insert(c, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                            }
                            else
                                o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                        }
                    }
                    else if (!string.IsNullOrEmpty(order.agreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.agreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.agreementNo);
                    }
                }
            }

            if (!currentId.Equals(order.brokerIdA) && !currentId.Equals(order.brokerIdB))
            {
                if ("B,O".Equals(order.BridgeType))
                {
                    if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                    {
                        int a = o.Text.IndexOf(";bid");
                        if (a > 0)
                        {
                            o.Text = o.Text.Insert(a, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }

                        int b = o.HideCounterpartyText.IndexOf(";bid");
                        if (b > 0)
                        {
                            o.HideCounterpartyText = o.HideCounterpartyText.Insert(b, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }

                        int c = o.HideSendQQMsgText.IndexOf(";bid");
                        if (c > 0)
                        {
                            o.HideSendQQMsgText = o.HideSendQQMsgText.Insert(c, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }
                    }
                    if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.bidAgreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder.bidAgreementNo);
                    }

                }
                else if ("B".Equals(order.BridgeType) || "O".Equals(order.BridgeType))
                {
                    if (order.bridgeOrder != null && !string.IsNullOrEmpty(order.bridgeOrder.ofrAgreementNo))
                    {
                        int a = o.Text.IndexOf(";bid");
                        if (a > 0)
                        {
                            o.Text = o.Text.Insert(a, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }
                        int b = o.HideCounterpartyText.IndexOf(";bid");
                        if (b > 0)
                        {
                            o.HideCounterpartyText = o.HideCounterpartyText.Insert(b, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }

                        int c = o.HideSendQQMsgText.IndexOf(";bid");
                        if (c > 0)
                        {
                            o.HideSendQQMsgText = o.HideSendQQMsgText.Insert(c, string.Format(agreementNOFomrt, order.bridgeOrder.ofrAgreementNo));
                        }
                    }
                    if (order.bridgeOrder2 != null && !string.IsNullOrEmpty(order.bridgeOrder2.bidAgreementNo))
                    {
                        o.Text += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                        o.HideCounterpartyText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                        o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.bridgeOrder2.bidAgreementNo);
                    }

                }
                else if (!string.IsNullOrEmpty(order.agreementNo))
                {
                    o.Text += string.Format(agreementNOFomrt, order.agreementNo);
                    o.HideCounterpartyText += string.Format(agreementNOFomrt, order.agreementNo);
                    o.HideSendQQMsgText += string.Format(agreementNOFomrt, order.agreementNo);
                }
            }
        }


        /// <summary>
        /// 拼接关联机构文本字符串
        /// </summary>
        /// <param name="order">订单</param>
        /// <param name="traderInfo">交易员信息</param>
        /// <param name="copyText">文本要素</param>
        /// <param name="side">订单方向</param>
        /// <param name="o">文本对象</param>
        /// <param name="isRecordDetail">是否是主订单列表文本</param>
        private static void SetHideBridgeInfo(CopyDetailPo order, string[] traderInfo, string[] copyText, string side, OrderArgs o, bool isRecordDetail, bool isbroker)
        {
            //机构信息
            string counterpartyA = string.Empty;
            string counterpartyB = string.Empty;
            //机构信息带交易员
            string counterpartyTraderA = string.Empty;
            string counterpartyTraderB = string.Empty;
            if (order.IsGVN)
            {
                counterpartyA = $"{traderInfo[0]}{traderInfo[1]}";
                counterpartyB = $"{traderInfo[2]}{traderInfo[3]}";

                counterpartyTraderA = $"{traderInfo[0]} {order.bankAgentNameA}";
                counterpartyTraderB = $"{traderInfo[2]} {order.bankAgentNameB}";
            }
            else
            {
                counterpartyA = $"{traderInfo[2]}{traderInfo[3]}";
                counterpartyB = $"{traderInfo[0]}{traderInfo[1]}";

                counterpartyTraderA = $"{traderInfo[2]} {order.bankAgentNameB}";
                counterpartyTraderB = $"{traderInfo[0]} {order.bankAgentNameA}";
            }
            if (order.AssociatedFlag == "1")
            {
                if (true)//(CopyDetailHelper.IsMyModifysOrder(order) && !SystemParamHandler.IsBridgeRole())
                {
                    if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                    {
                        switch (side)
                        {
                            case "Ofr":
                                counterpartyB = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);
                                //CRMService.Instance.GetInstName2(order.bridgebankid, order.goodstype);
                                counterpartyTraderB = counterpartyB;
                                break;
                            case "Bid":
                                counterpartyA = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);
                                //CRMService.Instance.GetInstName2(order.bridgebankid2, order.goodstype);
                                counterpartyTraderA = counterpartyA;
                                break;
                            default:
                                break;
                        }
                        if (isRecordDetail)
                        {
                            o.CopyText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {copyText[7]}";
                            o.WithTraderText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";

                        }
                        else
                        {
                            if (isbroker)
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {copyText[7]}";
                            }
                            else
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";
                            }

                            o.WithTraderText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {copyText[7]}";
                        }
                    }
                    else
                    {
                        switch (side)
                        {
                            case "Ofr":
                                counterpartyB = "机构待定";
                                counterpartyTraderB = counterpartyB;
                                break;
                            case "Bid":
                                counterpartyA = "机构待定";
                                counterpartyTraderA = counterpartyA;
                                break;
                            default:
                                break;
                        }
                        //获取备注代付费
                        string remarks = GetRemarkContent(order, true, o);
                        if (isRecordDetail)
                        {
                            o.CopyText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {remarks}";
                            o.WithTraderText = $"{copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                        }
                        else
                        {
                            if (isbroker)
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyA} 出给 {counterpartyB} {remarks}";
                            }
                            else
                            {
                                o.CopyText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                            }

                            o.WithTraderText = $"{copyText[0]}) {copyText[1]}{copyText[2]}  {copyText[3]}{copyText[4]}  {copyText[5]}  {copyText[6]} {counterpartyTraderA} 出给 {counterpartyTraderB} {remarks}";
                        }
                        o.HideCounterpartyText = o.CopyText;
                    }
                }
                else
                {
                    o.CopyText = o.Text;
                }
            }
            else
            {
                o.CopyText = o.Text;
            }
        }



        /// <summary>
        /// 复制标准化订单的备注内容
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isDetail"></param>
        /// <param name="bridgeType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetRemarkContent(CopyDetailPo order, bool isDetail, OrderArgs o)
        {
            string send = "";
            if (isDetail)
            {
                if (o.isShowColor)
                    return send;
                if (!"2".Equals(order.BridgeFlag) && string.IsNullOrWhiteSpace(order.BridgeType))
                {
                    //发给标准化
                    if (!string.IsNullOrWhiteSpace(order.issueState) && order.issueState == "1")
                    {
                        string staSend = "";
                        if (!string.IsNullOrWhiteSpace(order.proxyFee))
                        {
                            staSend = "," + order.proxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.issueRemark))
                        {
                            staSend = staSend + "," + order.issueRemark;
                        }
                        send = staSend;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(order.Dsendto))
                            send = "," + order.Dsendto;
                        if (!string.IsNullOrWhiteSpace(order.proxyFee))
                        {
                            send = "," + order.proxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.issueRemark))
                            send += "," + order.issueRemark;
                    }

                }
            }
            return send;
        }
        
        public static string GetSideByInstId(CopyDetailPo order, DetailTrader detailTrader, string brokerId = null)
        {
            string result = "买入";
            if (order.IsGVN)
            {
                //无机构成交单根据broker id 判断方向
                if (string.IsNullOrEmpty(detailTrader.InstID) && detailTrader.Key != null && detailTrader.Key.Contains(Constant.NoInstOrder) && !string.IsNullOrEmpty(brokerId))
                {
                    if (order.brokerIdA == order.brokerIdB)
                    {
                        if (string.IsNullOrEmpty(order.bankIdA) && !string.IsNullOrEmpty(order.bankIdB))
                        {
                            result = "卖出";
                        }
                        else if (!string.IsNullOrEmpty(order.bankIdA) && string.IsNullOrEmpty(order.bankIdB))
                        {
                            result = "买入";
                        }

                    }
                    else if (order.brokerIdA == brokerId)
                    {
                        result = "卖出";
                    }
                    else if (order.brokerIdB == brokerId)
                    {
                        result = "买入";
                    }
                }
                else if (order.bankIdA == detailTrader.InstID && (string.IsNullOrEmpty(detailTrader.TraderID) || order.bankagentIdA == detailTrader.TraderID))
                {
                    result = "卖出";
                }
                else if (order.bankIdB == detailTrader.InstID && (string.IsNullOrEmpty(detailTrader.TraderID) || order.bankAgentIdB == detailTrader.TraderID))
                {
                    result = "买入";
                }
                else
                {
                    result = "买入";
                }
            }
            else
            {
                // 无机构成交单根据broker id 判断方向
                if (string.IsNullOrEmpty(detailTrader.InstID) && detailTrader.Key != null && detailTrader.Key.Contains(Constant.NoInstOrder) && !string.IsNullOrEmpty(brokerId))
                {
                    if (order.brokerIdA == order.brokerIdB)
                    {
                        if (string.IsNullOrEmpty(order.bankIdA) && !string.IsNullOrEmpty(order.bankIdB))
                        {
                            result = "买入";
                        }
                        else if (!string.IsNullOrEmpty(order.bankIdA) && string.IsNullOrEmpty(order.bankIdB))
                        {
                            result = "卖出";
                        }

                    }
                    else if (order.brokerIdA == brokerId)
                    {
                        result = "买入";
                    }
                    else if (order.brokerIdB == brokerId)
                    {
                        result = "卖出";
                    }
                }
                else if (order.bankIdA == detailTrader.InstID && (string.IsNullOrEmpty(detailTrader.TraderID) || order.bankagentIdA == detailTrader.TraderID))
                {
                    result = "买入";
                }
                else if (order.bankIdB == detailTrader.InstID && (string.IsNullOrEmpty(detailTrader.TraderID) || order.bankAgentIdB == detailTrader.TraderID))
                {
                    result = "卖出";
                }
                else
                {
                    result = "买入";
                }
            }
            return result;
        }
 

        /// <summary>
        /// 明细有桥时候的发给内容
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isDetail"></param>
        /// <param name="bridgeType"></param>
        /// <param name="o"></param>
        /// <param name="side"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSendTo(CopyDetailPo order, bool isDetail, string bridgeType, OrderArgs o, string side, string key = null, string assistBrokerId = null)
        {
            string send = "";
            if (isDetail)
            {
                if (o.isShowColor)
                    return send;
                if (!"2".Equals(order.BridgeFlag) && string.IsNullOrWhiteSpace(order.BridgeType))
                {
                    string strChannel = "";
                    foreach (ComboxBaseModel item in EnumHelper.Instance.ChannelComboxModel)
                    {
                        if (item.ValueMemberPath == order.issueChannel)
                        {
                            switch (order.issueChannel)
                            {
                                case "1":
                                    if (order.goodsCode.EndsWith(".ib") || order.goodsCode.EndsWith(".IB"))
                                    {
                                        strChannel = "";

                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                                case "2":
                                    if (order.goodsCode.EndsWith(".ib") || order.goodsCode.EndsWith(".IB"))
                                    {
                                        strChannel = "【" + item.DisplayMemberPath + "】";
                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                                case "3":
                                case "4":
                                case "5":
                                case "6":
                                case "7":
                                case "8":
                                case "11":
                                    strChannel = item.DisplayMemberPath;
                                    break;
                                case "9":
                                case "10":
                                    //意向(9)和匿名(10)两种渠道，拼接规则参看 SetIssueShowName => issueDesc 位于渠道之前
                                    //strChannel还要和下面的issueShowName再次拼接，因此这个地方strChannel置为空字符串
                                    //当债券是SZ，SH类型的时候，要赋值为渠道名称
                                    if (order.goodsCode.EndsWith(".ib") || order.goodsCode.EndsWith(".IB"))
                                    {
                                        strChannel = "";
                                    }
                                    else
                                    {
                                        strChannel = item.DisplayMemberPath;
                                    }
                                    break;
                            }
                            break;
                        }
                    }

                    string issueShowName = SetIssueShowName(order);

                    //发给标准化
                    if (!string.IsNullOrWhiteSpace(order.issueState) && order.issueState == "1")
                    {
                        string staSend = "";
                        if (!string.IsNullOrWhiteSpace(order.issueShowType) && order.issueShowType == "1")
                        {
                            if (!string.IsNullOrWhiteSpace(order.proxyFee))
                            {
                                staSend = strChannel + issueShowName + "," + order.proxyFee + "厘";
                            }
                            else
                            {
                                staSend = strChannel + issueShowName;
                            }

                            if (!string.IsNullOrWhiteSpace(order.issueRemark))
                            {
                                staSend = staSend + "," + order.issueRemark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(order.proxyFee))
                            {
                                staSend = strChannel + "," + order.proxyFee + "厘";
                            }
                            else
                            {
                                staSend = strChannel;
                            }

                            if (!string.IsNullOrWhiteSpace(order.issueRemark))
                            {
                                staSend = staSend + "," + order.issueRemark;
                            }
                        }
                        //过桥并有加桥
                        //if (SystemParamHandler.IsBridgeRole() && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == LocalEnv.UserInfo.UserId))
                        if (false && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == Common.Utils.LocalEnv.UserInfo.UserId))
                        {
                            if (!string.IsNullOrWhiteSpace(order.Dsendto2) && !string.IsNullOrWhiteSpace(order.Dsendto))
                                send = ",ofr" + staSend + ";bid" + staSend;
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(order.Dsendto2))
                                    send = ",ofr" + staSend;
                                if (!string.IsNullOrWhiteSpace(order.Dsendto))
                                    send = ",bid" + staSend;
                            }
                        }
                        else
                        {
                            var comparedName = string.IsNullOrEmpty(assistBrokerId) ? Common.Utils.LocalEnv.UserInfo.UserId : assistBrokerId;

                            //发给根据[机构 + trade]去判断（原来根据机构来判断）
                            bool isSelf = true;
                            if (!comparedName.Equals(order.brokerIdA) && !comparedName.Equals(order.brokerIdB))
                            {
                                isSelf = false;
                            }

                            switch (side)
                            {
                                case "Ofr":
                                    if (isSelf)
                                        send = "," + staSend;
                                    else
                                        send = ",ofr" + staSend;
                                    break;
                                case "Bid":
                                    if (isSelf)
                                        send = "," + staSend;
                                    else
                                        send = ",bid" + staSend;
                                    break;
                                default:
                                    if (isSelf)
                                        send = "," + staSend + ";" + staSend;
                                    else
                                        send = ",ofr" + staSend + ";bid" + staSend;
                                    break;
                            }

                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(order.Dsendto))
                            send = "," + order.Dsendto;
                        if (!string.IsNullOrWhiteSpace(order.proxyFee))
                        {
                            send = "," + order.proxyFee + "厘";
                        }
                        if (!string.IsNullOrWhiteSpace(order.issueRemark))
                            send += "," + order.issueRemark;
                    }

                }
                else
                {//过桥并有加桥
                    //if (SystemParamHandler.IsBridgeRole() && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == LocalEnv.UserInfo.UserId))
                    if (false && (string.IsNullOrEmpty(assistBrokerId) || assistBrokerId == Common.Utils.LocalEnv.UserInfo.UserId))
                    {
                        if (!string.IsNullOrWhiteSpace(order.Dsendto2) && !string.IsNullOrWhiteSpace(order.Dsendto))
                            send = ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(order.Dsendto2))
                                send = ",ofr" + order.Dsendto2;
                            if (!string.IsNullOrWhiteSpace(order.Dsendto))
                                send = ",bid" + order.Dsendto;
                        }
                    }
                    else
                    {
                        var comparedName = string.IsNullOrEmpty(assistBrokerId) ? Common.Utils.LocalEnv.UserInfo.UserId : assistBrokerId;

                        //发给根据[机构 + trade]去判断（原来根据机构来判断）
                        bool isSelf = true;
                        if (!comparedName.Equals(order.brokerIdA) && !comparedName.Equals(order.brokerIdB))
                        {
                            isSelf = false;
                        }

                        switch (side)
                        {
                            case "Ofr":
                                if (isSelf)
                                    send = "," + order.Dsendto2;
                                else
                                    send = ",ofr" + order.Dsendto2;
                                break;
                            case "Bid":
                                if (isSelf)
                                    send = "," + order.Dsendto;
                                else
                                    send = ",bid" + order.Dsendto;
                                break;
                            default:
                                if (isSelf)
                                    send = "," + order.Dsendto2 + ";" + order.Dsendto;
                                else
                                    send = ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                                break;
                        }

                    }
                }
            }
            return send;
        }


        /// <summary>
        ///发给展示
        /// </summary>
        /// <param name="operationOrderDetail"></param>
        private static string SetIssueShowName(CopyDetailPo operationOrderDetail)
        {

            string issueInstName = operationOrderDetail.issueInstNameShort;
            if (string.IsNullOrWhiteSpace(issueInstName))
            {
                issueInstName = operationOrderDetail.issueInstName;
            }


            string idesc = "";
            if (!string.IsNullOrWhiteSpace(operationOrderDetail.goodsCode))
            {
                if (operationOrderDetail.goodsCode.EndsWith(".ib") || operationOrderDetail.goodsCode.EndsWith(".IB"))
                {
                    if (string.IsNullOrWhiteSpace(operationOrderDetail.issueChannel))
                    {
                        return idesc;
                    }
                    ChannelEnum channelEnum = (ChannelEnum)System.Enum.Parse(typeof(ChannelEnum), operationOrderDetail.issueChannel, true);
                    switch (channelEnum)
                    {
                        case ChannelEnum.Request:
                            idesc = operationOrderDetail.issueDesc + issueInstName;
                            break;
                        case ChannelEnum.FixedIncome:
                        case ChannelEnum.Bulk:
                        case ChannelEnum.Bidding:
                        case ChannelEnum.Dialogue:
                        case ChannelEnum.Bilateral:
                        case ChannelEnum.xbond:
                        case ChannelEnum.iDeal:
                        case ChannelEnum.Xbargain:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.issueTraderName))
                            {
                                idesc = operationOrderDetail.issueDesc + issueInstName + " " + operationOrderDetail.issueTraderName;
                            }
                            else
                            {
                                idesc = operationOrderDetail.issueDesc + issueInstName;
                            }
                            break;
                        case ChannelEnum.Intention:
                        case ChannelEnum.Anonymity:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.issueTraderName))
                            {
                                idesc = operationOrderDetail.issueDesc + channelEnum.GetDescription() + "," + issueInstName + " " + operationOrderDetail.issueTraderName;
                            }
                            else
                            {
                                idesc = operationOrderDetail.issueDesc + channelEnum.GetDescription() + "," + issueInstName;
                            }
                            break;
                    }
                }
                else
                {
                    return "";
                }

            }
            else
            {
                idesc = operationOrderDetail.issueDesc + " " + issueInstName;
            }

            return idesc;
        }

        public static string GetInst(string s1, string s2, string s3)
        {
            string key = "";
            if (!string.IsNullOrWhiteSpace(s1))
                key = s1;
            else
                key = s2;
            return key;
        }


        


        public static OrderArgs ConvertText(CopyDetailPo order, int i, DetailTrader detailTrader, PositionDetailPo position, bool isDetail, string proxyBrokerId)//= "Visible"re
        {
            var key = detailTrader.Key;
            string Code = "";
            string Type = "";
            string insta = "";
            string instb = "";
            string volume = "";
            OrderArgs o = new OrderArgs();
            o.NextDate = NextDate;
            Type = position.IsHist ? TransOrderHelper.Instance.GetHistoryDisplaySettle(order.SettleDisplay, order.SettleDisplay2, order.remarks, order.dealdate, order.dealdate2, order.settlementType, order.settlementtype2, order.settlementdate, order.settlementdate2)
                : TransOrderHelper.Instance.GetDisplaySettle(order.SettleDisplay, order.SettleDisplay2, order.remarks, order.dealdate, order.settlementType, order.settlementtype2);
            string bidType = "";
            string ofrType = "";
            if (!string.IsNullOrWhiteSpace(Type))
            {
                if (Type.IndexOf('/') != -1)
                {
                    string[] typeN = Type.Split('/');
                    if (typeN.Length == 2)
                    {
                        ofrType = typeN[0];
                        bidType = typeN[1];
                    }
                    else
                    {
                        ofrType = bidType = Type;
                    }
                }
                else
                {
                    ofrType = bidType = Type;
                }
            }
            #region 方向判断
            insta = ConvertTextHelper.GetBand(order, 0);
            instb = ConvertTextHelper.GetBand(order, 1);
            if (string.IsNullOrEmpty(insta))
                insta = "机构待定";
            if (string.IsNullOrEmpty(instb))
                instb = "机构待定";
            //明细方向根据trade来判断 20200402
            position.Tradera = order.OfrbankAgentName;
            position.Traderb = order.BidbankAgentName;
            ///保证明细前面的broker是ofr
            if (order.IsGVN)
            {
                position.Insta = insta;
                position.Instb = instb;
            }
            else
            {
                position.Insta = instb;
                position.Instb = insta;
            }
            position.TxtBankagentIdA = order.bankagentIdA;
            position.TxtBankagentIdB = order.bankAgentIdB;
            position.BankAgentNameA = order.OfrbankAgentName;
            position.BankAgentNameB = order.BidbankAgentName;
            if (order.qty.HasValue)
            {
                if ((order.qty.Value / 10000) >= 1)
                    volume = string.Format("{0}E", order.qty.Value / 10000);
                else
                    volume = string.Format("{0}", order.qty.Value);
            }
            string bankAgentNameA = (string.IsNullOrWhiteSpace(insta) || (dicSet.ContainsKey(detailTrader.Key.Trim()) && detailTrader.Key.Trim() == insta)) ? string.Format(" {0}", order.bankAgentNameA) : "";
            string bankAgentNameB = (string.IsNullOrWhiteSpace(instb) || (dicSet.ContainsKey(detailTrader.Key.Trim()) && detailTrader.Key.Trim() == instb)) ? string.Format(" {0}", order.bankAgentNameB) : "";
            string goodsCode = order.goodsCode.Replace(".IB", "");
            string goodsName = (SetParam != null && "true".Equals(SetParam.formatShortNameCheck)) ? string.Format("{0}  ", order.goodsShortName) : "";
            if (string.IsNullOrEmpty(goodsName) && BondHelper.IsLGBBond(order.goodsCode) && GRSettingsHandler.AppSettingIsTrue(Constant.WithShortNameLGB))
            {
                goodsName = string.Format("{0} ", order.goodsShortName);
            }

            if (GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype))
            {
                BondsInfoPo bond = CommonUtils.GetBondsInfoByCode(order.goodsCode);
                if (bond != null && !string.IsNullOrWhiteSpace(bond.Asset_Status) && bond.Asset_Status == "PRI")
                {
                    goodsName = goodsName + " " + "私募债" + "  ";
                }
            }

            if (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatDebtTypeCheck))
            {
                string fundObjectivesCategoryStr = CopyDetailHelper.GetFundObjectivesCategory(order.goodsCode);
                if (!string.IsNullOrWhiteSpace(fundObjectivesCategoryStr))
                {
                    goodsName = goodsName + " " + fundObjectivesCategoryStr;
                }
            }

            string tenor = (SetParam != null && "true".Equals(SetParam.formatTenorCheck)) ? string.Format("{0}  ", order.timeToMaturity) : "";
            // if ("2".Equals(order.bridgeflag))
            //{
            //    if (string.IsNullOrWhiteSpace(order.sendto) && string.IsNullOrWhiteSpace(order.sendto2))
            //        o.isShowColor = true;
            //}
            string tsento = "";
            string side = "";
            decimal d;
            string fandian = string.IsNullOrEmpty(order.fandian) ? string.Empty : decimal.TryParse(order.fandian, out d) ? "F" + order.fandian : order.fandian;
            string price = string.Format("{0}{1}", order.price, fandian);
            //string.IsNullOrWhiteSpace(order.fandian) ? string.Format("{0}{1}", order.PriceFormat, order.fandian) : string.Format("{0}F{1}", order.PriceFormat, order.fandian);
            //方向是否展示
            bool SideIsShow = position.IsHist ? (CopyDetailHelper.dichistSide.ContainsKey(key) ? CopyDetailHelper.dichistSide[key] : false) : (CopyDetailHelper.dicSide.ContainsKey(key) ? CopyDetailHelper.dicSide[key] : false);

            if (string.IsNullOrEmpty(proxyBrokerId))
            {
                if ((!string.IsNullOrEmpty(order.bankIdA) || !string.IsNullOrEmpty(order.bankIdB))
                    && string.IsNullOrEmpty(detailTrader.InstID)
                    && key.Contains(CopyDetailHelper.NoInstOrder))
                {
                    DetailTrader temp = new DetailTrader();
                    if (!string.IsNullOrEmpty(order.bankIdA))
                    {
                        temp.InstID = order.bankIdA;
                        temp.TraderID = order.bankagentIdA;
                    }
                    else
                    {
                        temp.InstID = order.bankIdB;
                        temp.TraderID = order.bankAgentIdB;

                    }
                    position.SideShow = GetReverseDirection(GetSideByInstId(order, temp));
                }
                else
                {
                    position.SideShow = getSideByKey(order, key);
                }
            }
            else
            {
                position.SideShow = GetSideByInstId(order, detailTrader, proxyBrokerId);
            }

            string bidBridgeName = "";
            string ofrBridgeName = "";

            string counterpartyA = $"{insta}{ bankAgentNameA}";
            string counterpartyB = $"{instb}{bankAgentNameB}";

            //发给文本
            string sendToTxt = string.Empty;

            var currentId = string.IsNullOrEmpty(proxyBrokerId) ? Common.Utils.LocalEnv.UserInfo.UserId : proxyBrokerId;

            //交易a关键信息
            DetailTrader detailTraderA = GetTrader(order, 0);
            //交易B关键信息
            DetailTrader detailTraderB = GetTrader(order, 1);


            if (SideIsShow)
            {
                tenor = $"{position.SideShow} {tenor}";
            }

            //带公司名称
            string num = i.ToString();
            //if (order.seq=="80") {
            //    int xxx = 0;
            //}
            ///成交单是GVN
            if (order.IsGVN)
            {
                bool isbroker = true;
                ///同一个broker.
                if (currentId.Equals(order.brokerIdA) && currentId.Equals(order.brokerIdB))
                {
                    ///如果是机构A
                    if (IsCatianInstTrader(detailTraderA, detailTrader))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate2;
                        ///是否加粗，结算日期是否是今天
                        o.isliq = isliqMethod(order, order.dealdate2);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                         Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb, bankAgentNameB
                         );
                        tsento = order.Dsendto2;
                        side = "Ofr";
                        o.HideCounterpartyText = o.Text;

                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor,
                         Code, goodsName, price, volume, Type);
                        sendToTxt = tsento;
                    }
                    ///如果是机构B
                    else if (IsCatianInstTrader(detailTraderB, detailTrader)
                        || instb == "机构待定" && key.Contains("今日无机构成交单"))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate;
                        o.isliq = isliqMethod(order, order.dealdate);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                           Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb, bankAgentNameB
                        );
                        tsento = order.Dsendto;
                        sendToTxt = tsento;
                        side = "Bid";

                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                    }
                    else
                    {//brokerA=brokerB=自己 显示在今日无机构成交单，按ofr方显示
                        isbroker = true;
                        o.TradeDate = order.dealdate2;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                         Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb, bankAgentNameB
                         );
                        tsento = order.Dsendto2;
                        sendToTxt = tsento;
                        side = "Ofr";
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                    }

                    o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} {order.bankAgentNameA} 出给 {instb} {order.bankAgentNameB}";

                    if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                    { //过桥没有加桥（只有发送对象）
                        o.isShowColor = true;
                        if ((string.IsNullOrWhiteSpace(tsento))) { }

                        else
                        {
                            o.Text += ", " + tsento;
                            o.WithTraderText += ", " + tsento;
                            o.HideSendQQMsgText += ", " + tsento;
                            sendToTxt = tsento;
                        }

                        o.HideCounterpartyText = o.Text;
                    }
                    else
                    {

                        string sendToStr = GetSendTo(order, isDetail, "O", o, side, key, currentId);
                        o.Text += sendToStr;
                        o.WithTraderText += sendToStr;
                        o.HideSendQQMsgText += sendToStr;
                        sendToTxt = sendToStr;
                        //隐藏对手方文本
                        if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                        {
                            switch (side)
                            {
                                case "Ofr":
                                    bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);
                                    //CRMService.Instance.GetInstName2(order.bridgebankid, order.goodstype);
                                    o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {ofrType} {insta} {bankAgentNameA} 出给 {bidBridgeName}";
                                    o.HideCounterpartyText += sendToStr;
                                    break;
                                case "Bid":
                                    bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);
                                    //CRMService.Instance.GetInstName2(order.bridgebankid2, order.goodstype);
                                    o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {bidType} {bidBridgeName} 出给 {instb}{bankAgentNameB}";
                                    o.HideCounterpartyText += sendToStr;
                                    break;
                                default:
                                    o.HideCounterpartyText = o.Text;
                                    break;
                            }
                        }
                        else
                        {
                            o.HideCounterpartyText = o.Text;
                        }
                    }
                    //var result = GetHideBridgeInfo(order, insta, bankAgentNameA, instb, bankAgentNameB, side, o);
                    //counterpartyA = result.counterpartyA;
                    //counterpartyB = result.counterpartyB;
                    //o.CopyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {counterpartyA} 出给 {counterpartyB}";
                }
                else
                {
                    ///brokerA
                    if (currentId.Equals(order.brokerIdA))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate2;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                          Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb, bankAgentNameB
                        );
                        o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} {order.bankAgentNameA} 出给 {instb} {order.bankAgentNameB}";

                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        side = "Ofr";
                        if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                        { //过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(order.Dsendto2))) { }
                            else
                            {
                                o.Text += ", " + order.Dsendto2;
                                o.WithTraderText += ", " + order.Dsendto2;
                                o.HideSendQQMsgText += ", " + order.Dsendto2;
                                sendToTxt = order.Dsendto2;
                            }
                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "O", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.WithTraderText += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);
                                //CRMService.Instance.GetInstName2(order.bridgebankid, order.goodstype);
                                o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {ofrType} {insta} {bankAgentNameA} 出给 {bidBridgeName}";
                                o.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                o.HideCounterpartyText = o.Text;
                            }
                        }
                    }
                    ///brokerB
                    else if (currentId.Equals(order.brokerIdB))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate;
                        o.isliq = isliqMethod(order, order.dealdate);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                           Code, goodsName, price, volume, Type, insta, bankAgentNameA, instb, bankAgentNameB
                          );
                        o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} {order.bankAgentNameA} 出给 {instb} {order.bankAgentNameB}";

                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);

                        side = "Bid";
                        if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                        {   //过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(order.Dsendto))) { }
                            else
                            {
                                o.Text += ", " + order.Dsendto;
                                o.WithTraderText += ", " + order.Dsendto;
                                o.HideSendQQMsgText += ", " + order.Dsendto;
                                sendToTxt = order.Dsendto;
                            }

                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "B", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.WithTraderText += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid2, order.goodstype);
                                //CRMService.Instance.GetInstName2(order.bridgebankid2, order.goodstype);
                                o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {bidType} {bidBridgeName} 出给 {instb}{bankAgentNameB}";
                                o.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                o.HideCounterpartyText = o.Text;
                            }
                        }
                    }
                    ///非本broker的单子
                    else
                    {
                        Code = goodsCode;
                        isbroker = false;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7} {8} 出给 {9} {10}", num, tenor,
                             goodsCode, goodsName, price, volume, Type, insta, order.bankAgentNameA, instb, order.bankAgentNameB
                            );

                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, goodsCode, goodsName, price, volume, Type);

                        if ("2".Equals(order.BridgeFlag)&& !OrderHelper.BridgeIsAllConfirmed(order))
                        {//过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if (!string.IsNullOrWhiteSpace(order.Dsendto) && !string.IsNullOrWhiteSpace(order.Dsendto2))
                            {
                                o.Text += ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                                o.HideSendQQMsgText += ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                                sendToTxt = ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                            }
                            else
                            {
                                //if ("2".Equals(order.bridgeflag))

                                if (!string.IsNullOrWhiteSpace(order.Dsendto2))
                                {
                                    o.Text += ",ofr" + order.Dsendto2;
                                    o.HideSendQQMsgText += ",ofr" + order.Dsendto2;
                                    sendToTxt = ",ofr" + order.Dsendto2;
                                }
                                else if (!string.IsNullOrWhiteSpace(order.Dsendto))
                                {
                                    o.Text += ",bid" + order.Dsendto;
                                    o.HideSendQQMsgText += ",bid" + order.Dsendto;
                                    sendToTxt = ",bid" + order.Dsendto;
                                }

                            }
                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "OB", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                        }
                        o.HideCounterpartyText = o.Text;
                        o.WithTraderText = o.Text;
                    }
                }

                o.AssistantAText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} 卖出";
                o.AssistantBText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} 买入";

                string[] traderInfo = new string[] {
                    insta,bankAgentNameA, instb, bankAgentNameB
                };

                string[] copyText = new string[] {
                    num,tenor, Code, goodsName,price,volume, Type, sendToTxt
                };

                SetHideBridgeInfo(order, traderInfo, copyText, side, o, false, isbroker);
            }
            else
            ///成交单是TKN
            {
                bool isbroker = true;
                if (currentId.Equals(order.brokerIdA) && currentId.Equals(order.brokerIdB))
                {
                    isbroker = true;
                    if (IsCatianInstTrader(detailTraderB, detailTrader))
                    {
                        o.TradeDate = order.dealdate2;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                           Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta, bankAgentNameA
                           );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        tsento = order.Dsendto2;
                        sendToTxt = order.Dsendto2;
                        side = "Ofr";
                    }
                    else if (IsCatianInstTrader(detailTraderA, detailTrader)
                        || insta == "机构待定" && key.Contains("今日无机构成交单"))
                    {
                        o.TradeDate = order.dealdate;
                        o.isliq = isliqMethod(order, order.dealdate);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                             Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta, bankAgentNameA
                             );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        tsento = order.Dsendto;
                        sendToTxt = order.Dsendto;
                        side = "Bid";
                    }
                    else
                    {//brokerA=brokerB=自己 显示在今日无机构成交单，按ofr方显示
                        o.TradeDate = order.dealdate2;
                        o.isliq = true;//isliqMethod(order, order.dealdate2);
                        Code = order.goodsShortName + " ";//goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                           Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta, bankAgentNameA
                           );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        tsento = order.Dsendto2;
                        sendToTxt = order.Dsendto2;
                        side = "Ofr";
                    }

                    o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} {order.bankAgentNameB} 出给 {insta} {order.bankAgentNameA}";
                    o.HideCounterpartyText = o.Text;
                    if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                    {//过桥没有加桥（只有发送对象）
                        o.isShowColor = true;
                        if ((string.IsNullOrWhiteSpace(tsento))) { }
                        else
                        {
                            o.Text += ", " + tsento;
                            o.WithTraderText += ", " + tsento;
                            o.HideSendQQMsgText += ", " + tsento;
                            sendToTxt = ", " + tsento;
                        }
                        o.HideCounterpartyText = o.Text;
                    }
                    else
                    {
                        string sendToStr = GetSendTo(order, isDetail, "O", o, side, key, currentId);
                        o.Text += sendToStr;
                        o.WithTraderText += sendToStr;
                        o.HideSendQQMsgText += sendToStr;
                        sendToTxt = sendToStr;
                        //隐藏对手方文本
                        if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                        {
                            switch (side)
                            {
                                case "Ofr":
                                    bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);//CRMService.Instance.GetInstName2(order.bridgebankid, order.goodstype);
                                    o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {ofrType} {instb} {bankAgentNameB} 出给 {bidBridgeName}";
                                    o.HideCounterpartyText += sendToStr;
                                    break;
                                case "Bid":
                                    bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype);//CRMService.Instance.GetInstName2(order.bridgebankid2, order.goodstype);
                                    o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {bidType} {bidBridgeName} 出给 {insta}{bankAgentNameA}";
                                    o.HideCounterpartyText += sendToStr;
                                    break;
                                default:
                                    o.HideCounterpartyText = o.Text;
                                    break;
                            }
                        }
                        else
                        {
                            o.HideCounterpartyText = o.Text;
                        }
                    }

                }
                else
                {
                    if (currentId.Equals(order.brokerIdB))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate2;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow2;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                         Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta, bankAgentNameA
                         );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        side = "Ofr";
                        o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} {order.bankAgentNameB} 出给 {insta} {order.bankAgentNameA}";
                        o.HideCounterpartyText = o.Text;
                        if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                        {//过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(order.Dsendto2))) { }
                            else
                            {
                                o.Text += ", " + order.Dsendto2;
                                o.WithTraderText += ", " + order.Dsendto2;
                                o.HideSendQQMsgText += ", " + order.Dsendto2;
                                sendToTxt = ", " + order.Dsendto2;
                            }
                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "O", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.WithTraderText += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid2, order.goodstype);//CRMService.Instance.GetInstName2(order.bridgebankid, order.goodstype);
                                o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {ofrType} {instb} {bankAgentNameB} 出给 {bidBridgeName}";
                                o.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                o.HideCounterpartyText = o.Text;
                            }
                        }

                    }
                    else if (currentId.Equals(order.brokerIdA))
                    {
                        isbroker = true;
                        o.TradeDate = order.dealdate;
                        o.isliq = isliqMethod(order, order.dealdate);
                        //Code = (o.IsShowDate) ? string.Format("{0}({1})", goodsCode, o.Date) : goodsCode;
                        Code = order.goodsCodeShow;
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7}{8} 出给 {9}{10}", num, tenor,
                          Code, goodsName, price, volume, Type, instb, bankAgentNameB, insta, bankAgentNameA
                           );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, Code, goodsName, price, volume, Type);
                        side = "Bid";
                        o.HideCounterpartyText = o.Text;
                        o.WithTraderText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} {order.bankAgentNameB} 出给 {insta} {order.bankAgentNameA}";
                        if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                        {//过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if ((string.IsNullOrWhiteSpace(order.Dsendto))) { }
                            else
                            {
                                o.Text += ", " + order.Dsendto;
                                o.WithTraderText += ", " + order.Dsendto;
                                o.HideSendQQMsgText += ", " + order.Dsendto;
                                sendToTxt = ", " + order.Dsendto;
                            }
                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "B", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.WithTraderText += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                            if (TransOrderHelper.Instance.IsHaseBride(order.BridgeFlag, order.BridgeType, o.isShowColor))
                            {
                                bidBridgeName = CommonUtils.GetInstName2(order.bridgebankid, order.goodstype); ;//CRMService.Instance.GetInstName2(order.bridgebankid2, order.goodstype);
                                o.HideCounterpartyText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {bidType} {bidBridgeName} 出给 {insta}{bankAgentNameA}";
                                o.HideCounterpartyText += sendToStr;
                            }
                            else
                            {
                                o.HideCounterpartyText = o.Text;
                            }
                        }
                    }
                    else
                    {
                        Code = goodsCode;
                        isbroker = false;
                        o.isliq = isliqMethod(order, order.dealdate2);
                        o.Text = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6} {7} {8} 出给 {9} {10}", num, tenor,
                              goodsCode, goodsName, price, volume, Type, instb, order.bankAgentNameB, insta, order.bankAgentNameA
                              );
                        o.HideSendQQMsgText = string.Format("{0}) {1}{2}  {3}{4}  {5}  {6}", num, tenor, goodsCode, goodsName, price, volume, Type);
                        o.HideCounterpartyText = o.Text;
                        if ("2".Equals(order.BridgeFlag) && !OrderHelper.BridgeIsAllConfirmed(order))
                        {//过桥没有加桥（只有发送对象）
                            o.isShowColor = true;
                            if (!string.IsNullOrWhiteSpace(order.Dsendto) && !string.IsNullOrWhiteSpace(order.Dsendto2))
                            {
                                o.Text += ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                                o.HideSendQQMsgText += ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                                sendToTxt = ",ofr" + order.Dsendto2 + ";bid" + order.Dsendto;
                            }
                            else
                            {
                                //if ("2".Equals(order.bridgeflag))
                                if (!string.IsNullOrWhiteSpace(order.Dsendto2))
                                {
                                    o.Text += ",ofr" + order.Dsendto2;
                                    o.HideSendQQMsgText += ",ofr" + order.Dsendto2;
                                    sendToTxt = ",ofr" + order.Dsendto2;
                                }
                                else if (!string.IsNullOrWhiteSpace(order.Dsendto))
                                {
                                    o.Text += ",bid" + order.Dsendto;
                                    o.HideSendQQMsgText += ",bid" + order.Dsendto;
                                    sendToTxt = ",bid" + order.Dsendto;
                                }

                            }
                            o.HideCounterpartyText = o.Text;
                        }
                        else
                        {
                            string sendToStr = GetSendTo(order, isDetail, "OB", o, side, key, currentId);
                            o.Text += sendToStr;
                            o.HideSendQQMsgText += sendToStr;
                            sendToTxt = sendToStr;
                        }
                        o.HideCounterpartyText = o.Text;
                        o.WithTraderText = o.Text;
                    }
                }

                o.AssistantAText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {insta} 买入";
                o.AssistantBText = $"{num}) {tenor}{Code}  {goodsName}{price}  {volume}  {Type} {instb} 卖出";

                string[] traderInfo = new string[] {
                    insta,bankAgentNameA, instb, bankAgentNameB
                };
                string[] copyText = new string[] {
                    num,tenor, Code, goodsName,price,volume, Type, sendToTxt
                };
                SetHideBridgeInfo(order, traderInfo, copyText, side, o, false, isbroker);
            }
            #endregion
            setAgreementNO(order, key, o, currentId);
            return o;
        }

        /// <summary>
        /// 反转方向 买入=>卖出; 卖出=>买入
        /// </summary>
        /// <param name="dirction"></param>
        /// <returns></returns>
        public static string GetReverseDirection(string dirction)
        {
            string result = string.Empty;
            switch (dirction)
            {
                case "买入": result = "卖出"; break;
                case "卖出": result = "买入"; break;
                default: result = "买入"; break;
            }
            return result;
        }

        public class OrderArgs
        {
            /// <summary>
            /// 当天成交的单子字体为细体
            /// </summary>
            public bool isliq { get; set; }
            public string Text { get; set; }
            /// <summary>
            /// 复制文本用于拷贝时使用
            /// </summary>
            public string CopyText { get; set; }
            /// <summary>
            /// 复制文本用于拷贝时使用
            /// </summary>
            public string SendTextA { get; set; }
            /// <summary>
            /// 复制文本用于拷贝时使用
            /// </summary>
            public string SendTextB { get; set; }
            /// <summary>
            /// 隐藏对手方文本
            /// </summary>
            public string HideCounterpartyText { get; set; }
            /// <summary>
            /// 助理A文本
            /// </summary>
            public string AssistantAText { get; set; }
            /// <summary>
            /// 助理B文本
            /// </summary>
            public string AssistantBText { get; set; }

            /// <summary>
            /// 缺桥字体显示红色
            /// </summary>
            public bool isShowColor { get; set; }
            public string TradeDate { get; set; }
            public string NextDate { get; set; }
            public bool IsShowDate
            {
                get
                {
                    bool result = false;
                    if (string.IsNullOrWhiteSpace(TradeDate) || string.IsNullOrWhiteSpace(NextDate))
                        return result;
                    try
                    {

                        if (TradeDate.Length == 8)
                        {
                            TradeDate = TradeDate.Insert(4, "-").Insert(7, "-");
                        }
                        if (NextDate.Length == 8)
                        {
                            NextDate = NextDate.Insert(4, "-").Insert(7, "-");
                        }
                        DateTime t1 = Convert.ToDateTime(TradeDate);
                        DateTime t2 = Convert.ToDateTime(NextDate);
                        int compNum = DateTime.Compare(t1, t2);
                        if (compNum > 0)
                        {
                            result = true;
                            string[] s = TradeDate.Split('-');
                            if (s.Length == 3)
                                Date = string.Format("{0}.{1}", s[1], s[2]);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                    return result;
                }
            }
            public string Date { get; set; }
            /// <summary>
            /// 带交易员复制
            /// </summary>
            public string WithTraderText { get; set; }

            /// <summary>
            /// 隐藏发送文本
            /// </summary>
            public string HideSendQQMsgText { get; set; }
        }

        private static string CalSettleDate(CalWeekDateReq request)
        {
            string settlementDate = "";
            if (!request.settlementType.Contains("+"))
            {
                request.settlementType += "+0";
            }
            string result = TransOrderHelper.Instance.CalSettleDate(request);
            if (string.IsNullOrWhiteSpace(result))
            {
                settlementDate = result;
            }
            return settlementDate;
        }
        /// <summary>
        /// 每天获取一次NextDate 解决多天未登录导致NextDate未更新bug
        /// </summary>
       // private static Dictionary<string, string> todayDic = new Dictionary<string, string>();

        //public static void GetNextDate()
        //{
        //    string today = Common.Utils.DateTimeEx.ServerTime.ToString("yyyy-MM-dd");

        //    if (!todayDic.ContainsKey(today))
        //    {
        //        todayDic.Clear();
        //        CalWeekDateReq o1 = new CalWeekDateReq();
        //        o1.settlementType = "+1";
        //        todayDic[today] = CalSettleDate(o1);
        //        LogHelper.Info(string.Format("......................NextDate:{0}", todayDic[today]));
        //    }
        //    NextDate = todayDic[today];

        //}
        /// <summary>
        /// 获取成交单的指定机构名称
        /// </summary>
        /// <param name="order"></param>
        /// <param name="Ab">是否取机构A</param>
        /// <returns></returns>
        public static string GetKey(CopyDetailPo order, int Ab)
        {
            string key = "";
            switch (Ab)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshorta))
                    {
                        if (!string.IsNullOrWhiteSpace(order.bankAgentNameA))
                            key = string.Format("{0} {1}", order.bankcodeshorta, order.bankAgentNameA.Trim());
                        else
                            key = order.bankcodeshorta;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(order.bankAgentNameA))
                            key = string.Format("{0} {1}", order.bankCodeA, order.bankAgentNameA.Trim());
                        else
                            key = order.bankCodeA;
                    }
                    break;
                case 1:
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshortb))
                    {
                        if (!string.IsNullOrWhiteSpace(order.bankAgentNameB))
                            key = string.Format("{0} {1}", order.bankcodeshortb, order.bankAgentNameB.Trim());
                        else
                            key = order.bankcodeshortb;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(order.bankAgentNameB))
                            key = string.Format("{0} {1}", order.bankCodeB, order.bankAgentNameB.Trim());
                        else
                            key = order.bankCodeB;
                    }
                    break;
            }
            return key;
        }
        /// <summary>
        /// 获取指定方向交易员
        /// </summary>
        /// <param name="order"></param>
        /// <param name="Ab"></param>
        /// <returns></returns>
        public static DetailTrader GetTrader(CopyDetailPo order, int Ab)
        {
            DetailTrader detailTrader = new DetailTrader();
            detailTrader.Key = GetKey(order, Ab);
            switch (Ab)
            {
                case 0:
                    detailTrader.InstID = order.bankIdA;
                    detailTrader.TraderID = order.bankagentIdA;
                    break;
                case 1:
                    detailTrader.InstID = order.bankIdB;
                    detailTrader.TraderID = order.bankAgentIdB;
                    break;
            }
            return detailTrader;
        }
        /// <summary>
        /// 获取订单的指定机构
        /// </summary>
        /// <param name="order"></param>
        /// <param name="Ab"></param>
        /// <returns></returns>
        public static string GetBand(CopyDetailPo order, int Ab)
        {
            string key = "";
            switch (Ab)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshorta))
                        key = order.bankcodeshorta;
                    else
                        key = order.bankCodeA;
                    break;
                case 1:
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshortb))
                        key = order.bankcodeshortb;
                    else
                        key = order.bankCodeB;
                    break;
            }
            return key;
        }

        public static bool isliqMethod(CopyDetailPo order, string dealdt)
        {
            if (Liquidation.Equals(dealdt))
                return true;
            else
                return false;
        }

        public static void GetSet()
        {
            dicSet.Clear();
            SystemParameter set = GRSettingsHandler.Instance.GetSystemParameter();
            if (set != null && set.FormatInst != null && set.FormatInst.Count > 0)
            {
                foreach (UserRoleSettingDetail ins in set.FormatInst)
                {
                    if (!string.IsNullOrEmpty(ins.userDisplayName) && !dicSet.ContainsKey(ins.userDisplayName))
                    {
                        dicSet.Add(ins.userDisplayName, ins);
                    }
                }
            }
        }

        /// <summary>
        /// 获取broker的简称，中文取前两个，英文取最后一个单子
        /// </summary>
        /// <param name="broker"></param>
        /// <returns></returns>
        public static string IsChinese(string broker)
        {
            if (string.IsNullOrWhiteSpace(broker))
                return null;
            string fbroker = broker;

            Regex regChina = new Regex("^[^\x00-\xFF]");
            Regex regEnglish = new Regex("^[a-zA-Z]");
            if (regEnglish.IsMatch(broker))
            {
                try
                {
                    int index = broker.LastIndexOf(' ');
                    int indexp = broker.LastIndexOf('.');
                    if (index >= 0 && index >= indexp)
                    {
                        string tr = broker.Substring(index + 1, broker.Length - index - 1);
                        fbroker = tr;
                    }
                    else if (indexp >= 0)
                    {
                        string tr = broker.Substring(indexp + 1, broker.Length - indexp - 1);
                        fbroker = tr;
                    }
                    else
                        fbroker = broker;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    return broker;
                }
            }
            else if (regChina.IsMatch(broker))
            {
                if (broker.Length >= 2)
                    fbroker = broker.Substring(broker.Length - 2, 2);
            }
            return fbroker;
        }


        /// <summary>
        /// 获取公司名称
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetCompanyDetial(string seqNum)
        {
            string companyText = seqNum;
            SystemParameter set = GRSettingsHandler.Instance.GetSystemParameter();
            string withCompanyName;
            bool iswithCompanyName;
            string company;
            if (set.ClientAppSettings.TryGetValue(Constant.WithCompanyName, out withCompanyName)
           && bool.TryParse(withCompanyName, out iswithCompanyName))
            {
                if (iswithCompanyName)
                {
                    if (!string.IsNullOrWhiteSpace(Common.Utils.LocalEnv.ConfInfo.BrokerNameFlag)
                        && Constant.DicBrokerNames.TryGetValue(Common.Utils.LocalEnv.ConfInfo.BrokerNameFlag, out company))
                    {
                        companyText = $"【{company}】{seqNum}";
                    }
                }
            }
            return companyText;
        }

      
        /// <summary>
        /// 格式化发行量
        /// </summary>
        /// <param name="issueaMount"></param>
        /// <returns></returns>
        public static string FormatIssueaMount(string issueaMount)
        {

            string _issueaMount = issueaMount;
            if (string.IsNullOrWhiteSpace(issueaMount))
            {
                return "";
            }
            decimal number = decimal.Zero;
            if (decimal.TryParse(issueaMount, out number))
            {
                issueaMount = (number / 10000).ToString();
                if (issueaMount.IndexOf('.') != -1)
                {
                    _issueaMount = issueaMount.TrimEnd('0').TrimEnd('.');
                }
                else
                {
                    _issueaMount = issueaMount;
                }
            }
            return _issueaMount;
        }
        /// <summary>
        /// 转换数字
        /// </summary>
        public static long CharToNumber(char c)
        {
            switch (c)
            {
                case '一': return 1;
                case '二': return 2;
                case '三': return 3;
                case '四': return 4;
                case '五': return 5;
                case '六': return 6;
                case '七': return 7;
                case '八': return 8;
                case '九': return 9;
                case '零': return 0;
                default: return -1;
            }
        }
        /// <summary>
        /// 转换数字
        /// </summary>
        public static char NumberToChar(int n)
        {
            switch (n)
            {
                case 1: return '一';
                case 2: return '二';
                case 3: return '三';
                case 4: return '四';
                case 5: return '五';
                case 6: return '六';
                case 7: return '七';
                case 8: return '八';
                case 9: return '九';
                case 0: return '零';
                default: return '？';
            }
        }

        /// <summary>
        /// 单桥历史单结算汇总显示
        /// 历史但的计算类型，当天显示的时候要么是“+0”，“+1” ，要么是“日期+0”或者“日期+1”的方式
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static string GetHistoryDisplaySettle(CopyDetailPo orderDetailPo)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非默认结算类型，且O,B结算类型一致
            if (!string.IsNullOrEmpty(orderDetailPo.SettleDisplay2) && orderDetailPo.SettleDisplay.Equals(orderDetailPo.SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(orderDetailPo.remarks))
                {
                    dtDeal = GetDate(orderDetailPo.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + orderDetailPo.remarks;
                }
                else
                {
                    dtDeal = GetDate(orderDetailPo.dealdate);
                    DateTime dtSettle = GetDate(orderDetailPo.settlementdate);
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
                        {
                            //result = "+1";
                            result = GetDefautSettle(orderDetailPo.settlementtype2);
                        }
                        else
                        {
                            //result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            //                         + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+1";
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                      + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(orderDetailPo.settlementtype2);
                        }

                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(orderDetailPo.settlementtype2) && !string.IsNullOrEmpty(orderDetailPo.settlementType))
                {
                    //ofr
                    ofrResult = GetHistorySettle(orderDetailPo, true);
                    //bid
                    bidResult = GetHistorySettle(orderDetailPo, false);

                    if (ofrResult.Equals(bidResult))
                        result = ofrResult;
                    else
                        result = string.Format("{0}/{1}", ofrResult, bidResult);
                }
                else if (!string.IsNullOrEmpty(orderDetailPo.settlementType) && string.IsNullOrEmpty(orderDetailPo.settlementtype2))
                {
                    result = GetHistorySettle(orderDetailPo, false);
                }
                else if (string.IsNullOrEmpty(orderDetailPo.settlementType) && !string.IsNullOrEmpty(orderDetailPo.settlementtype2))
                {
                    //ofr
                    result = GetHistorySettle(orderDetailPo, true);
                }
            }

            return result;
        }

        /// <summary>
        /// 历史单结类型 错期使用,新版本，考虑到节假日的情况，解决 日期+0 会显示成 日期+1
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="isOfr"></param>
        /// <returns></returns>
        public static string GetHistorySettle(CopyDetailPo orderDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? orderDetailPo.settlementdate2 : orderDetailPo.settlementdate;
            string dealTime = isOfr ? orderDetailPo.dealdate2 : orderDetailPo.dealdate;
            string settle = isOfr ? orderDetailPo.settlementtype2 : orderDetailPo.settlementType;

            string result = string.Empty;

            DateTime dtDeal, dtSettle;
            dtSettle = GetDate(settleTime);
            dtDeal = GetDate(dealTime);

            var time = dtSettle - dtDeal;
            //成交和结算在同一天
            if (time.Days == 0)
            {
                time = dtDeal - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (time.Days == 0)
                    ///今天成交
                    result = "+0";
                else
                    ///远期
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                        + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+0";

            }
            else if (time.Days != 0) //+1或者跨节假日 //周周六+0的时候 ，交易日和结算日不是同一天
            {
                time = dtDeal - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (time.Days == 0)
                {
                    result = GetDefautSettle(settle);
                }
                else
                {
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "." + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(settle);
                }
            }
            return result;
        }

        public static DateTime GetDate(string time)
        {
            DateTime result;
            DateTime.TryParseExact(time, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

            return result;
        }

        /// <summary>
        /// 获取默认结算 
        /// 周三+1 =》 +1
        /// 明天+1 =》 +1
        /// 10.11+0 =》 +0
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public static string GetDefautSettle(string settle)
        {
            if (string.IsNullOrEmpty(settle))
            {
                return string.Empty;
            }
            if (settle.Contains("+0"))
                return "+0";
            if (settle.Contains("+1"))
                return "+1";
            return string.Empty;
        }

        /// <summary>
        /// 获取机构的方向，判断逻辑与汇总里面的买卖方向一致
        /// </summary>
        /// <param name="order"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static string getSideByKey(CopyDetailPo order, string key)
        {
            string insta = ConvertTextHelper.GetKey(order, 0);
            string instb = ConvertTextHelper.GetKey(order, 1);

            if (string.IsNullOrWhiteSpace(order.brokerNameA))
            {
                order.brokerNameA = "";
            }
            if (order.IsGVN)
            {
                if (key.Contains("其他人成交单"))
                {
                    if (order.brokerIdA == order.brokerIdB)
                    {
                        return "买入";
                    }
                    else
                    {
                        if (IsContianUser(order.brokerNameA) && IsContianUser(order.brokerNameB))
                        {
                            return "买入";
                        }
                        else
                        {
                            if (IsContianUser(order.brokerNameA))
                            {
                                return "卖出";
                            }
                            else if (IsContianUser(order.brokerNameB))
                            {
                                return "买入";
                            }
                            else
                            {
                                return "买入";
                            }
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(insta) && string.IsNullOrWhiteSpace(instb))
                    {
                        //traderA和B同时为空时，
                        //当前broker如果属于A方，则是 “卖出”
                        //当前broker如果属于B方，则是 “买入”
                        if (order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                        {
                            return "卖出";
                        }
                        else
                        {
                            return "买入";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(insta))
                        {
                            if (order.brokerIdA == order.brokerIdB)
                            {
                                if (instb.Contains(key))
                                {
                                    return "买入";
                                }
                                else
                                {
                                    return "卖出";
                                }
                            }
                            else
                            {
                                if (order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                                {
                                    return "卖出";
                                }
                                else
                                {
                                    return "买入";
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(insta) && insta.Contains(key) && order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                            {
                                return "卖出";
                            }
                            else
                            {
                                return "买入";
                            }
                        }
                    }

                }
            }
            else
            {
                if (key.Contains("其他人成交单"))
                {
                    if (order.brokerIdA == order.brokerIdB)
                    {
                        return "买入";
                    }
                    else
                    {
                        if (IsContianUser(order.brokerNameA) && IsContianUser(order.brokerNameB))
                        {
                            return "买入";
                        }
                        else
                        {
                            if (IsContianUser(order.brokerNameA))
                            {
                                return "买入";
                            }
                            else if (IsContianUser(order.brokerNameB))
                            {
                                return "卖出";
                            }
                            else
                            {
                                return "买入";
                            }
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(insta) && string.IsNullOrWhiteSpace(instb))
                    {
                        //traderA和B同时为空时，
                        //当前broker如果属于A方，则是 “买入”
                        //当前broker如果属于B方，则是 “卖出”
                        if (order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                        {
                            return "买入";
                        }
                        else
                        {
                            return "卖出";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(instb))
                        {
                            if (order.brokerIdA == order.brokerIdB)
                            {
                                if (insta.Contains(key))
                                {
                                    return "卖出";
                                }
                                else
                                {
                                    return "买入";
                                }
                            }
                            else
                            {
                                if (order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                                {
                                    return "买入";
                                }
                                else
                                {
                                    return "卖出";
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(insta) && insta.Contains(key) && order.brokerNameA.Equals(Common.Utils.LocalEnv.UserInfo.FullName))
                            {
                                return "买入";
                            }
                            else
                            {
                                return "卖出";
                            }
                        }
                    }

                }
            }
        }

        private static bool IsContianUser(string brokerName)
        {

            bool isContian = false;
            PowerSettingModel currentPowerSettingModel = GRSettingsHandler.PowerSettingModel;
            List<UserRoleSettingDetail> updateOrders = null;
            if (currentPowerSettingModel != null
                && currentPowerSettingModel.UpdateOrders != null
                && currentPowerSettingModel.UpdateOrders.Count > 0)
            {
                foreach (UserRoleSettingDetail item in currentPowerSettingModel.UpdateOrders)
                {
                    if (item.userName == Common.Utils.LocalEnv.UserInfo.UserName)
                    {
                        updateOrders = item.relatedUsers?.ToList();
                        break;
                    }
                }
            }
            if (updateOrders != null && updateOrders.Count > 0)
            {
                if (updateOrders.Any(_ => _.userName == brokerName))
                {
                    isContian = true;
                }
                else
                {
                    isContian = false;
                }
            }
            else
            {
                isContian = false;
            }
            return isContian;
        }

        /// <summary>
        /// 结算日大于NextDate显示日期
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public static bool IsShowDate(string settleDate, string NextDate)
        {
            string Date;
            bool result = false;
            if (string.IsNullOrWhiteSpace(settleDate) || string.IsNullOrWhiteSpace(NextDate))
                return result;
            try
            {

                if (settleDate.Length == 8)
                {
                    settleDate = settleDate.Insert(4, "-").Insert(7, "-");
                }
                if (NextDate.Length == 8)
                {
                    NextDate = NextDate.Insert(4, "-").Insert(7, "-");
                }
                DateTime t1 = Convert.ToDateTime(settleDate);
                DateTime t2 = Convert.ToDateTime(NextDate);
                int compNum = DateTime.Compare(t1, t2);
                if (compNum > 0)
                {
                    result = true;
                    string[] s = settleDate.Split('-');
                    if (s.Length == 3)
                        Date = string.Format("{0}.{1}", s[1], s[2]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return result;
        }

        /// <summary>
        /// 结算类型，当天单子
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static string GetDisplaySettle(CopyDetailPo orderDetailPo)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(orderDetailPo.SettleDisplay2) && orderDetailPo.SettleDisplay.Equals(orderDetailPo.SettleDisplay2))
            {
                //备注
                if (!string.IsNullOrEmpty(orderDetailPo.remarks))
                {
                    DateTime dtDeal = GetDate(orderDetailPo.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + orderDetailPo.remarks;
                }
                else
                    result = orderDetailPo.SettleDisplay;
            }
            else
            //错期
            {
                if (!string.IsNullOrEmpty(orderDetailPo.settlementtype2) && !string.IsNullOrEmpty(orderDetailPo.settlementType))
                    result = string.Format("{0}/{1}", orderDetailPo.SettleDisplay2, orderDetailPo.SettleDisplay);
                else if (!string.IsNullOrEmpty(orderDetailPo.settlementType) && string.IsNullOrEmpty(orderDetailPo.settlementtype2))
                    result = orderDetailPo.SettleDisplay;
                else if (string.IsNullOrEmpty(orderDetailPo.settlementType) && !string.IsNullOrEmpty(orderDetailPo.settlementtype2))
                    result = orderDetailPo.SettleDisplay2;
            }

            return result;
        }



    }
}
