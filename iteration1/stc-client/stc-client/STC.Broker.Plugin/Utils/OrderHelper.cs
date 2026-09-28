using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.Utils
{
    public class OrderHelper
    {
        public static string ShowCodeOrName(OrderDetailPo orderDetail)
        {
            string result = string.Empty;

            if (orderDetail != null && !string.IsNullOrEmpty(orderDetail.goodstype))
            {//1信用,2利率,11 ABS,12 NCD
                if (orderDetail.goodstype.Equals("2") || orderDetail.goodstype.Equals("BNC"))
                {
                    if (!string.IsNullOrEmpty(orderDetail.goodsSubType) && (orderDetail.goodsSubType.Equals("LLB") || orderDetail.goodsSubType.Equals("TLB")))
                        result = orderDetail.goodsShortName;
                    else
                        result = orderDetail.SecurityID;
                }
                else if (orderDetail.goodstype.Equals("1") || orderDetail.goodstype.Equals("11") || orderDetail.goodstype.Equals("12")
                    || orderDetail.goodstype.Equals("BCO") || orderDetail.goodstype.Equals("ABS") || orderDetail.goodstype.Equals("NCD"))
                    result = orderDetail.goodsShortName;
            }

            if (string.IsNullOrEmpty(result))
                result = orderDetail.SecurityID;

            return result;
        }

        /// <summary>
        /// 结算类型，当天单子
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static string GetDisplaySettle(OrderDetailPo orderDetailPo)
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
                if (!string.IsNullOrEmpty(orderDetailPo.SettlType2) && !string.IsNullOrEmpty(orderDetailPo.SettlType))
                    result = string.Format("{0}/{1}", orderDetailPo.SettleDisplay2, orderDetailPo.SettleDisplay);
                else if (!string.IsNullOrEmpty(orderDetailPo.SettlType) && string.IsNullOrEmpty(orderDetailPo.SettlType2))
                    result = orderDetailPo.SettleDisplay;
                else if (string.IsNullOrEmpty(orderDetailPo.SettlType) && !string.IsNullOrEmpty(orderDetailPo.SettlType2))
                    result = orderDetailPo.SettleDisplay2;
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
        /// 判断单子是否是今日发单的的历史单
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static bool TodayReceipt(OrderDetailPo orderDetailPo)
        {
            DateTime dt;
            if (DateTime.TryParseExact(orderDetailPo.createTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }

        public static bool TodayReceipt(string TransactTime)
        {
            DateTime dt;
            if (DateTime.TryParseExact(TransactTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 判断单子是否成功添加所有的桥
        /// 根据bridgeorder的状态判断
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static bool BridgeIsAllConfirmed(OrderDetailPo order)
        {
            bool flag = true;
            //不需要加桥
            if (!(order.BridgeFlag + "").Equals("2"))
            {
                flag = true;
            }
            //需要加桥
            else
            {   //无桥
                if (string.IsNullOrEmpty(order.BridgeType))
                {
                    flag = false;
                }
                //单桥
                else if (!string.IsNullOrEmpty(order.BridgeType) && order.BridgeType == "B,O")
                {
                    //老单子bridgeOrder为空，默认确认
                    if (order.bridgeOrder != null)
                    {
                        if (order.bridgeOrder != null
                           && IsConfirmed(order.bridgeOrder.BidbridgeStateEnum)
                           && IsConfirmed(order.bridgeOrder.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = true;

                }
                //双桥
                else
                {
                    if (order.bridgeOrder != null)
                    {
                        if (order.bridgeOrder != null
                             && IsConfirmed(order.bridgeOrder.BidbridgeStateEnum)
                             && IsConfirmed(order.bridgeOrder.OfrbridgeStateEnum)
                             && order.bridgeOrder2 != null
                             && IsConfirmed(order.bridgeOrder2.BidbridgeStateEnum)
                             && IsConfirmed(order.bridgeOrder2.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;

                    }
                    else
                        flag = true;
                }
            }
            return flag;
        }

        public static bool BridgeIsAllConfirmed(CopyDetailPo order)
        {
            bool flag = true;
            //不需要加桥
            if (!(order.BridgeFlag + "").Equals("2"))
            {
                flag = true;
            }
            //需要加桥
            else
            {   //无桥
                if (string.IsNullOrEmpty(order.BridgeType))
                {
                    flag = false;
                }
                //单桥
                else if (!string.IsNullOrEmpty(order.BridgeType) && order.BridgeType == "B,O")
                {
                    //老单子bridgeOrder为空，默认确认
                    if (order.bridgeOrder != null)
                    {
                        if (order.bridgeOrder != null
                           && IsConfirmed(order.bridgeOrder.BidbridgeStateEnum)
                           && IsConfirmed(order.bridgeOrder.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = true;

                }
                //双桥
                else
                {
                    if (order.bridgeOrder != null)
                    {
                        if (order.bridgeOrder != null
                             && IsConfirmed(order.bridgeOrder.BidbridgeStateEnum)
                             && IsConfirmed(order.bridgeOrder.OfrbridgeStateEnum)
                             && order.bridgeOrder2 != null
                             && IsConfirmed(order.bridgeOrder2.BidbridgeStateEnum)
                             && IsConfirmed(order.bridgeOrder2.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;

                    }
                    else
                        flag = true;
                }
            }
            return flag;
        }

        public static bool BridgeIsAllConfirmedEx(TransOrderServerData order)
        {
            bool flag = true;
            //不需要加桥
            if (!(order.BridgeFlag + "").Equals("2"))
            {
                flag = true;
            }
            //需要加桥
            else
            {   //无桥
                if (string.IsNullOrEmpty(order.BridgeType))
                {
                    flag = false;
                }
                //单桥
                else if (!string.IsNullOrEmpty(order.BridgeType) && order.BridgeType == "B,O")
                {
                    //老单子bridgeOrder为空，默认确认
                    if (order.BridgeOrderModel != null)
                    {
                        if (order.BridgeOrderModel != null
                           && IsConfirmed(order.BridgeOrderModel.BidbridgeStateEnum)
                           && IsConfirmed(order.BridgeOrderModel.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;
                    }
                    else
                        flag = true;

                }
                //双桥
                else
                {
                    if (order.BridgeOrderModel != null)
                    {
                        if (order.BridgeOrderModel != null
                             && IsConfirmed(order.BridgeOrderModel.BidbridgeStateEnum)
                             && IsConfirmed(order.BridgeOrderModel.OfrbridgeStateEnum)
                             && order.BridgeOrderModel2 != null
                             && IsConfirmed(order.BridgeOrderModel2.BidbridgeStateEnum)
                             && IsConfirmed(order.BridgeOrderModel2.OfrbridgeStateEnum))
                        {
                            flag = true;
                        }
                        else
                            flag = false;

                    }
                    else
                        flag = true;
                }
            }
            return flag;
        }

        /// <summary>
        /// 是否确认状态
        /// </summary>
        /// <param name="bridgeState"></param>
        /// <returns></returns>
        public static bool IsConfirmed(BridgeState bridgeState)
        {
            bool isConfirmed = false;
            if (bridgeState == BridgeState.Confirmed ||
                bridgeState == BridgeState.Finished ||
                bridgeState == BridgeState.Transferred ||
                bridgeState == BridgeState.RequestReturned ||
                bridgeState == BridgeState.IssuedOrder)
            {
                isConfirmed = true;
            }

            return isConfirmed;
        }

        /// <summary>
        /// 更新成交单状态
        /// </summary>
        /// <param name="request"></param>
        public static void ChangeOrder(UpdateOrderRequestPo request)
        {
            try
            {
                //if (OrderHandler.Instance.IsLoading)
                //    return;
                //if (!OrderHandler.Instance.ContainExecOrder(request))
                //    return;
                ////队列里面订单不允许重复，防止同一个订单重复提交修改请求
                ////此处过滤会导致修改traderA补录其他单子的请求被过滤，三笔以上可以复现
                ////if (requestQueue.Where(t => t.id == request.id).ToList().Count() == 0)
                //requestQueue.Add(request);
            }
            catch (InvalidOperationException e)
            {
                LogHelper.Error("ChangeOrder requestQueue:" + e.Message);
            }

        }

        /// <summary>
        /// 分批次提交  wxp 20200430
        /// </summary>
        public static void ChangeOrderEx(List<ExecutionReport> request)
        {
            ChangeOrderEx2(request);
        }


        public static void UpdateBatchExecOrderData(List<ExecutionReport> req)
        {
            Task.Run(() =>
            {
                PResult reply = null;
                reply = GRHandler.Instance.ExecBatchOrderUpdate(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });
        }
        /// <summary>
        /// 批量更新成交单状态
        /// 1.批量更新查看状态
        /// 2.批量更新桥要素
        /// </summary>
        /// <param name="request"></param>
        //public static void ChangeOrder(List<UpdateOrderRequestPo> request, List<TransOrderServerData> orderDetail)
        //{
        //    //Task.Run(() => BrokerCEDAServer.Instance.UpdateExecOrder(request));
        //    int i = 0;
        //    foreach (UpdateOrderRequestPo o in request)
        //    {
        //        TransOrderServerData req = new TransOrderServerData();
        //        if (orderDetail.Count == request.Count)
        //            ConvertER(orderDetail[i], o);
        //        i++;

        //        Task.Run(() =>
        //        {
        //            PResult reply = GRHandler.Instance.ExecOrderUpdate(req);
        //        });
        //    }
        //}

        public static void ChangeOrderEx2(List<ExecutionReport> orderDetail)
        {
            Task.Run(() =>
            {
                foreach (ExecutionReport o in orderDetail)
                {
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(o);
                    if (reply != null && reply.Code != "0000")
                    {
                        ShowMessage(reply.Msg);
                    }
                }
            });
        }

        private static void ShowMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }


        public static TransOrderServerData ConvertER(TransOrderServerData po, UpdateOrderRequestPo r)
        {
            UpdateExecOrderPo o = r.execOrders[0];
            po.BridgeFee = o.bridgefee;
            po.BridgeFee2 = o.bridgefee2;
            po.SendTo = o.sendto;
            po.SendTo2 = o.sendto2;
            po.BridgeRemark = o.bridgeremark;
            po.BridgeRemark2 = o.bridgeremark2;
            po.BridgeSendTo = o.bridgesendto;
            po.BridgeSendTo2 = o.bridgesendto2;
            po.BridgeChannel = o.bridgechannel;
            po.BridgeChannel2 = o.bridgechannel2;
            po.BridgeRemarkShow = o.bridgeremarkshow;
            po.BridgeRemarkShow2 = o.bridgeremarkshow2;
            po.DoubleBridgeBidSettltype = o.doublebridgebidsettlementtype;
            po.BridgeRemarkShow2 = o.doublebridgeoffersettlementtype;
            po.BridgeRemarkShow2 = o.doublebridgechannel;
            return po;
        }

        public static TransOrderServerData ConvertER(UpdateOrderRequestPo r)
        {
            UpdateExecOrderPo o = r.execOrders[0];

            TransOrderServerData po = new TransOrderServerData();
            //  po.Sequence = decimal.Parse(o.id);
            po.ExecID = o.execId;

            //po.goodsCode = o.SecurityID;
            //po.goodsShortName = o.Symbol;
            //po.goodstype = o.SecurityType;
            //po.BondSubtype = o.BondSubtype;
            //decimal price;
            //if (decimal.TryParse(o.Yield, out price))
            //    po.price = price;
            //decimal qty;
            //if (decimal.TryParse(o.OrderQty, out qty))
            //    po.qty = qty;
            ////po.ListedMarket = o.m
            //po.settlementdate = o.SettlDate;
            //po.settlementdate2 = o.SettlDate2;
            //po.settlementType = o.SettlType;
            //po.settlementtype2 = o.SettlType2;
            //po.remarks = o.Remark;

            //po.bankCodeA = o.InstitutionName;
            //po.bankAgentNameA = o.TraderName;
            //po.brokerIdA = o.PartySubID;
            //po.brokerNameA = o.PartySubName;

            //po.bankCodeB = o.CounterPartyInstitutionName;//o.Counterpartyname;
            //po.bankAgentNameB = o.CounterPartyTraderName;//o.Counterpartysubname;
            //po.brokerNameB = o.CounterPartySubName;
            //po.CounterPartySubID  = o.brokerIdB;
            //po.dealdate = o.TransactTime;
            //po.dealdate2 = o.TransactTime;

            po.BridgeFee = o.bridgefee;
            po.BridgeFee2 = o.bridgefee2;
            po.SendTo = o.sendto;
            po.SendTo2 = o.sendto2;
            po.BridgeRemark = o.bridgeremark;
            po.BridgeRemark2 = o.bridgeremark2;
            po.BridgeSendTo = o.bridgesendto;
            po.BridgeSendTo2 = o.bridgesendto2;
            po.BridgeChannel = o.bridgechannel;
            po.BridgeChannel2 = o.bridgechannel2;
            po.BridgeRemarkShow = o.bridgeremarkshow;
            po.BridgeRemarkShow2 = o.bridgeremarkshow2;
            po.DoubleBridgeBidSettltype = o.doublebridgebidsettlementtype;
            po.BridgeRemarkShow2 = o.doublebridgeoffersettlementtype;
            po.BridgeRemarkShow2 = o.doublebridgechannel;

            //po.BidbankAgentName = 
            //.OfrbankAgentName
            //orderDetailPo.bridgeOrder.bidDir
            //orderDetailPo.bridgeOrder.ofrDir
            //orderDetailPo.BidInst
            //orderDetailPo.OfrInst
            //orderDetailPo.bridgeOrder.bidOriginalBridgeRemark
            //orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark
            return po;
        }


        /// <summary>
        /// 单桥历史单结算汇总显示
        /// 历史但的计算类型，当天显示的时候要么是“+0”，“+1” ，要么是“日期+0”或者“日期+1”的方式
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        public static string GetHistoryDisplaySettle(OrderDetailPo orderDetailPo)
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
                    DateTime dtSettle = GetDate(orderDetailPo.SettlDate);
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
                                + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(orderDetailPo.SettlType2);
                    }
                    else
                    {
                        time = dtDeal - dtNow;
                        if (time.Days == 0)
                            result = "+1";
                        else
                        {
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                      + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(orderDetailPo.SettlType2);
                        }
                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(orderDetailPo.SettlType2) && !string.IsNullOrEmpty(orderDetailPo.SettlType))
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
                else if (!string.IsNullOrEmpty(orderDetailPo.SettlType) && string.IsNullOrEmpty(orderDetailPo.SettlType2))
                {
                    result = GetHistorySettle(orderDetailPo, false);
                }
                else if (string.IsNullOrEmpty(orderDetailPo.SettlType) && !string.IsNullOrEmpty(orderDetailPo.SettlType2))
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
        public static string GetHistorySettle(OrderDetailPo orderDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? orderDetailPo.SettlDate2 : orderDetailPo.SettlDate;
            string dealTime = isOfr ? orderDetailPo.DealDate2 : orderDetailPo.dealdate;
            string settle = isOfr ? orderDetailPo.SettlType2 : orderDetailPo.SettlType;

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
        /// 历史单结类型 错期使用
        /// </summary>
        /// <param name="settleTime"></param>
        /// <param name="dealTime"></param>
        /// <returns></returns>
        public static string GetHistorySettle(string settleTime, string dealTime)
        {
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
            else if (time.Days != 0) //+1或者跨节假日
            {
                time = dtDeal - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (time.Days == 0)
                {
                    result = "+1";
                }
                else
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                        + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+1";
            }

            return result;
        }

        /// <summary>
        /// 是否对应桥
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static bool IsSpecialBridgeOrder(TransOrderServerData orderDetail, UserBridge bridge)
           => !string.IsNullOrEmpty(orderDetail.BridgeType) && orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid)
           || !string.IsNullOrEmpty(orderDetail.BridgeType2) && !string.IsNullOrEmpty(orderDetail.BridgeTraderId2) && orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && !string.IsNullOrEmpty(orderDetail.BridgeInstitutionId2) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid);

    }
}
