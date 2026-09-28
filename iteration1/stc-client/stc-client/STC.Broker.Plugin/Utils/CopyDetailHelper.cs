using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static STC.Broker.Plugin.Utils.ConvertTextHelper;
using STC.Common.Utils;
using STC.Common.Model.Base;
using STC.Common.Handler.SystemParam;

namespace STC.Broker.Plugin.Utils
{
    public class CopyDetailHelper
    {
        public const string PARAM_KEY_COPY = "STC_CopyDetail_Setting";
        public const string ElseOrder = "其他人成交单";
        public static string OwnNoinstOrder = "今日无机构成交单";
        public const string ElsehistOrder = "历史其他人成交单";
        public static string OwnNoinsthistOrder = "历史无机构成交单";
        /// <summary>
        /// 无机构成交单
        /// </summary>
        public const string NoInstOrder = "无机构成交单";
        /// <summary>
        /// 其他人成交单是否按照机构交易员拆分
        /// </summary>
        public const string SplitShowOtherOrders = "splitshowotherorders";
        /// <summary>
        /// 当前登录用户可以查看的broker 列表.
        /// </summary>
        public static HashSet<string> CurrentUserBrokerId { get; } = new HashSet<string>();
        /// <summary>
        /// 经纪商标识
        /// </summary>
        public static readonly HashSet<string> BrokerFlags = new HashSet<string>() { "BGC" };
        /// <summary>
        /// 机构明细
        /// </summary>
        public static Dictionary<DetailTrader, List<PositionDetailPo>> dicData = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        /// <summary>
        /// 机构汇总明细
        /// </summary>
        public static Dictionary<DetailTrader, List<PositionDetailPo>> dicSum = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        /// <summary>
        /// 机构，成交单字典 包括其他单和无机构
        /// </summary>
        public static DetailTrader ElseOrderTrader = new DetailTrader() { Key = ElseOrder };
        /// <summary>
        /// 机构方向是否展示
        /// </summary>
        public static Dictionary<string, bool> dicSide = new Dictionary<string, bool>();
        public static Dictionary<DetailTrader, List<CopyDetailPo>> dicInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
        public static List<CopyDetailPo> ElseOrderlist = new List<CopyDetailPo>();
        public static List<CopyDetailPo> OwnNoinstOrderlist = new List<CopyDetailPo>();
        public static DetailTrader OwnNoinstOrderTrader = new DetailTrader() { Key = OwnNoinstOrder };
        public static Dictionary<DetailTrader, List<PositionDetailPo>> dichistData = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        public static Dictionary<DetailTrader, List<PositionDetailPo>> dichistSum = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        public static Dictionary<DetailTrader, List<CopyDetailPo>> dichistInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
        public static DetailTrader ElsehistOrderTrader = new DetailTrader() { Key = ElsehistOrder };
        public static List<CopyDetailPo> ElsehistOrderlist = new List<CopyDetailPo>();
        public static DetailTrader OwnNoinsthistOrderTrader = new DetailTrader() { Key = OwnNoinsthistOrder };
        public static List<CopyDetailPo> OwnNoinsthistOrderlist = new List<CopyDetailPo>();
        /// <summary>
        /// 机构方向是否展示
        /// </summary>
        public static Dictionary<string, bool> dichistSide = new Dictionary<string, bool>();

        #region 今日成交单

        /// <summary>
        /// 获取今日成交单展示数据
        /// 结构：机构头-机构成交-汇总-空格
        /// </summary>
        /// <param name="templist">订单</param>
        /// <param name="isDetail">二次搜索标记</param>
        /// <param name="condition">二次搜索条件</param>
        /// <param name="isShowDir">是否展示方向</param>
        /// <param name="isForce">是否强制刷新方向</param>
        /// <returns></returns>
        public static List<PositionDetailPo> GetDetail(List<CopyDetailPo> templist, bool isDetail, SearchPo condition, bool isShowDir, bool isForce, TextBlockModel textBlockModel, TextBlockModel textBlockModel2)
        {
            dicInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
            ElseOrderlist.Clear();
            OwnNoinstOrderlist.Clear();
            dicData = new Dictionary<DetailTrader, List<PositionDetailPo>>();
            List<PositionDetailPo> data = new List<PositionDetailPo>();

            #region 机构组装
            ///获取brokerA确认的成交单
            List<CopyDetailPo> temp = new List<CopyDetailPo>();
            foreach (var order in templist)
            {
                //2.4.3临时bug，修改broker的单子，不需要显示，直接删除的单子不要了
                //判断逻辑   
                if ("D".Equals(order.orderStatus) || "C".Equals(order.orderStatus) || order.delflag + "" == "1")
                {
                    continue;
                }
                if ("G".Equals(order.orderStatus) && !"1".Equals(order.delflag))
                    if ("G".Equals(order.orderStatus) || "D".Equals(order.orderStatus))
                    {//删除的单子也要显示 20200402
                        temp.Add(order);
                    }
            }
            var list = new List<CopyDetailPo>();
            if (isDetail)
            {
                list = condition == null ? temp.OrderBy(o => o.TransactionTime).ThenBy(o => o.SeqSort).ToList() : Search(temp, condition);
            }
            else
            {
                list = temp;
            }

            foreach (var order in list)
            {

                if (order.seq == "1370")
                {
                    int xx = 0;
                }
                // 如果当前broker是成交单的A方
                if (isContain(0, order))
                {
                    string key = ConvertTextHelper.GetKey(order, 0);
                    DetailTrader detailTrader = ConvertTextHelper.GetTrader(order, 0);

                    //判断机构是否合并展示
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshorta))
                    {
                        if (dicSet.ContainsKey(order.bankcodeshorta.Trim()))
                        {
                            key = order.bankcodeshorta;
                            detailTrader.Key = order.bankcodeshorta;
                        }

                    }
                    else
                    {
                        if (dicSet.ContainsKey(order.bankCodeA.Trim()))
                        {
                            key = order.bankCodeA;
                            detailTrader.Key = order.bankCodeA;
                        }
                    }
                    //将成交单加入字典
                    if (!dicInst.Keys.Any(_ => _.Key == key))
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dicInst.Add(detailTrader, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date != null && date.CompareTo(Today) >= 0)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dicInst.Add(detailTrader, li);
                            }
                        }
                    }
                    else
                    {
                        //根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader trader = dicInst.Keys.FirstOrDefault(_ => _.Key == key);
                                List<CopyDetailPo> keylist = dicInst[trader];
                                if (!keylist.Contains(order))
                                {
                                    dicInst[trader].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date != null && date.CompareTo(Today) >= 0)
                            {
                                DetailTrader trader = dicInst.Keys.FirstOrDefault(_ => _.Key == key);
                                List<CopyDetailPo> keylist = dicInst[trader];
                                if (!keylist.Contains(order))
                                {
                                    dicInst[trader].Add(order);
                                }
                            }
                        }
                    }
                }

                // 如果当前broker是成交单的B方
                if (isContain(1, order))
                {
                    String keyb = ConvertTextHelper.GetKey(order, 1);
                    DetailTrader detailTraderB = ConvertTextHelper.GetTrader(order, 1);

                    if (!string.IsNullOrWhiteSpace(order.bankcodeshortb))
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankcodeshortb))
                        {
                            keyb = order.bankcodeshortb;
                            detailTraderB.Key = order.bankcodeshortb;
                        }
                    }
                    else
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankCodeB))
                        {
                            keyb = order.bankCodeB;
                            detailTraderB.Key = order.bankCodeB;
                        }
                    }
                    if (!dicInst.Keys.Any(_ => _.Key == keyb))
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dicInst.Add(detailTraderB, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dicInst.Add(detailTraderB, li);
                            }
                        }
                    }
                    else
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader traderB = dicInst.Keys.FirstOrDefault(_ => _.Key == keyb);
                                List<CopyDetailPo> keylist = dicInst[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dicInst[traderB].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                DetailTrader traderB = dicInst.Keys.FirstOrDefault(_ => _.Key == keyb);
                                List<CopyDetailPo> keylist = dicInst[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dicInst[traderB].Add(order);
                                }
                            }
                        }
                    }
                }

                // 加入其他单， A和B的Broker都不是当前登录用户
                if ((!LocalEnv.UserInfo.FullName.Equals(order.brokerNameA) && !LocalEnv.UserInfo.FullName.Equals(order.brokerNameB))
                    || (LocalEnv.UserInfo.FullName.Equals(order.brokerNameA) && SystemPowerHandler.Instance.IsCanLookOrder(order.brokerIdB, false))
                    || (LocalEnv.UserInfo.FullName.Equals(order.brokerNameB) && SystemPowerHandler.Instance.IsCanLookOrder(order.brokerIdA, false)))
                {
                    ElseOrderlist.Add(order);
                }

                // 如果是其他人成交单，且有按机构交易员拆分设置，需要使用被代理的broker name 判断，是否是无机构成交单
                if (GRSettingsHandler.AppSettingIsTrue(SplitShowOtherOrders))
                {
                    if (IsNoInstOrder(order, CurrentUserBrokerId))
                    {
                        OwnNoinstOrderlist.Add(order);
                    }
                }
                // 加入无机构单，有一方机构为空
                else if (isContain(2, order))
                {
                    OwnNoinstOrderlist.Add(order);
                }
            }

            HashSet<string> otherOrderIds = new HashSet<string>();

            // 机构字典汇总
            if (ElseOrderlist != null && ElseOrderlist.Count > 0)
            {
                if (GRSettingsHandler.AppSettingIsTrue(SplitShowOtherOrders))
                {
                    ElseOrderlist.ForEach(x => otherOrderIds.Add(x.execId));
                    dicInst.Add(ElseOrderTrader, new List<CopyDetailPo>());
                    SplitOtherOrders(ElseOrderlist, condition, dicInst);
                }
                else
                {
                    dicInst.Add(ElseOrderTrader, ElseOrderlist);
                }
            }
            if (OwnNoinstOrderlist != null && OwnNoinstOrderlist.Count > 0)
            {
                dicInst.Add(OwnNoinstOrderTrader, OwnNoinstOrderlist);
            }

            #endregion 机构组装

            Dictionary<decimal?, CopyDetailPo> dicDetail = new Dictionary<decimal?, CopyDetailPo>();
            int j = 0; int z = 1;

            //wxp 对每一个机构的成交单的显示明细进行组装

            List<DetailTrader> detailTraderKeys = dicInst.Keys.ToList();
            foreach (DetailTrader Key in detailTraderKeys)
            {

                if (condition.IsMaturityOrder)
                {
                    dicInst[Key] = dicInst[Key].OrderBy(_ => _.maturityDays).ToList();

                }

                string key = Key.Key;

                if (ElseOrder.Equals(key) && GRSettingsHandler.AppSettingIsTrue(SplitShowOtherOrders))
                {
                    //proxyBrokerName = 

                    // 其他人成交单头
                    var dpOther = GenerateOtherOrderHeader(key, textBlockModel, textBlockModel2);
                    data.Add(dpOther);
                    dicData[Key] = new List<PositionDetailPo> { dpOther };

                    //加入空格
                    var empty = GenerateEmpty();
                    data.Add(empty);
                    dicData[Key].Add(empty);
                    continue;
                }

                List<CopyDetailPo> keyValues = new List<CopyDetailPo>();
                ///获取改机构下的所有成交单
                dicInst.TryGetValue(Key, out keyValues);

                //if (condition.IsMaturityOrder)
                //{
                //    keyValues = (from kv in keyValues
                //                 orderby kv.maturityDays ascending
                //                 select kv).ToList();
                //}

                if (isForce)
                {
                    //强制刷新方向
                    dicSide[key] = isShowDir;
                }
                else
                {
                    if (!dicSide.ContainsKey(key))
                    {
                        dicSide.Add(key, isShowDir);
                    }
                }

                dicData.Add(Key, new List<PositionDetailPo>());
                //wxp 加入机构组的Header显示
                PositionDetailPo dp = new PositionDetailPo();
                dp.DetailTrader = Key;
                dp.DataType = EDataType.头;
                dp.Seq =null;
                //dp.Text = toFormat(txb, key);
                ///设置header和复制文本

                //window.Dispatcher.Invoke(new Action(() => 
                //{

                //}));

                textBlockModel.Text = string.IsNullOrEmpty(dp.Text) ? string.Empty : dp.Text;
                dp.Text = toFormat(textBlockModel, key, false);
                dp.Key = key;
                dp.Text2 = toFormat2(textBlockModel, textBlockModel2, key, false);
                dp.TotalText = toFormatCopy(textBlockModel, key, false);
                dp.WithContactsText = toFormatCopy(textBlockModel, key, false);
                dp.CopyText = toFormatCopy(textBlockModel, key, false);
                dp.WithoutBrokerText = toFormatCopy(textBlockModel, key, false);
                dp.Padding = string.Format("Padding--0");
                dp.FontFamily = string.Format("Family--0");
                dp.FontSize = string.Format("Size--0");

                ///汇总按钮显示
                if (!key.Equals(ElseOrder) && !key.Equals(OwnNoinstOrder))
                    dp.SumIsVisible = "Visible";
                else
                    dp.SumIsVisible = "Hidden";
                //发送按钮是否显示, 合并显示的不需要显示发送，合并显示的只有机构所以不带空格，这个写法很不好
                if (key.Trim().Contains(" "))
                    dp.SendIsVisible = "Visible";
                else
                    dp.SendIsVisible = "Hidden";
                //是否是header
                dp.IsHeader = string.Format("Header--0");
                dp.IsMargin = string.Format("Margin--0");
                dp.IsWidth = string.Format("Width--0");
                dp.IsShowColor = string.Format("IsShow--2");
                dp.Isbrokerunread = string.Format("Isbroker--1");
                dp.Isliq = string.Format("Isliq--3");
                data.Add(dp);
                if (!dicData.ContainsKey(Key))
                    dicData.Add(Key, new List<PositionDetailPo>());
                dicData[Key].Add(dp);

                j++;
                int i = 1;
                //wxp 对机构下的每一条成交明细进行组装
                #region MyRegion
                if (keyValues == null)
                    dicInst.TryGetValue(Key, out keyValues);
                else
                {
                    foreach (CopyDetailPo order in keyValues)
                    {
                        PositionDetailPo detailPo = new PositionDetailPo();
                        detailPo.DataType = EDataType.数据;
                        detailPo.Seq =  string.IsNullOrWhiteSpace(order.seq)? 0 : Convert.ToDecimal(order.seq);

                        var proxyBrokerId = string.Empty;
                        if (otherOrderIds.Contains(order.execId))
                        {
                            // 其他人分机构交易员下的key 多一个空格，用来区分自己的分机构交易员标题，所以要trim
                            var trueKey = key.Trim();
                            if (!dicSet.ContainsKey(trueKey))
                            {
                                proxyBrokerId = GetProxyBrokerIdByInstTraderName(order, trueKey);
                            }
                            else
                            {
                                proxyBrokerId = GetProxyBrokerId(order, trueKey);
                            }
                        }
                        OrderArgs reply = new OrderArgs();
                        if (isDetail)
                            reply = ConvertTextHelper.ConvertText(order, i, Key, detailPo, isDetail, proxyBrokerId);
                        else
                            reply = ConvertTextHelper.ConvertText(order, z, Key, detailPo, isDetail, proxyBrokerId);
                        if (string.IsNullOrEmpty(reply.CopyText))
                            reply.CopyText = reply.Text;
                        detailPo.Text = reply.Text;
                        detailPo.HideCounterpartyText = ConvertTextHelper.GetCompanyDetial(reply.HideCounterpartyText);
                        detailPo.TotalText = ConvertTextHelper.GetCompanyDetial(reply.CopyText);
                        detailPo.BrokerCompanyName = ConvertTextHelper.GetCompanyDetial("");
                        //带联系人复制
                        detailPo.WithContactsText = ConvertTextHelper.GetCompanyDetial(GetWithContactsText(reply.CopyText, order, Key));
                        //带交易员复制
                        detailPo.WithTraderText = ConvertTextHelper.GetCompanyDetial(reply.WithTraderText);
                        //隐藏机构交易员发送qq消息
                        detailPo.HideSendQQMsgText = ConvertTextHelper.GetCompanyDetial(reply.HideSendQQMsgText);
                        detailPo.ToolText = reply.Text;
                        detailPo.BrokerOfr = ConvertTextHelper.IsChinese(order.OfrBroker);
                        detailPo.BrokerBid = ConvertTextHelper.IsChinese(order.BidBroker);
                        //助理明细发送文本
                        detailPo.AssistantAText = ConvertTextHelper.GetCompanyDetial(reply.AssistantAText);
                        detailPo.AssistantBText = ConvertTextHelper.GetCompanyDetial(reply.AssistantBText);
                        int index = reply.CopyText.IndexOf(')');
                        //tr 取的是明细的序号
                        string tr = reply.CopyText.Substring(0, index + 1);
                        detailPo.CopyText = string.Format("{0}vs{1} {2}-{3}{4}", detailPo.BrokerOfr, detailPo.BrokerBid, order.seq, tr, reply.CopyText.Substring(tr.Length));
                        detailPo.WithoutBrokerText = string.Format("{0}-{1}{2}", order.seq, tr, reply.CopyText.Substring(tr.Length));
                        detailPo.CopyText = ConvertTextHelper.GetCompanyDetial(detailPo.CopyText);
                        detailPo.WithoutBrokerText = ConvertTextHelper.GetCompanyDetial(detailPo.WithoutBrokerText);

                        //有未读信息颜色配置
                        //if (order.brokerunread != null && order.brokerunread. Contains(LocalEnv.UserInfo.UserId))
                        if (CopyDetailHandler.Instance.IsContainsUserId(order.brokerunread))
                        {
                            detailPo.Isbrokerunread = string.Format("Isbroker-{0}-{1}", order.seq, 0);
                            detailPo.IsBackgColor = true;
                        }
                        else
                        {
                            detailPo.Isbrokerunread = string.Format("Isbroker-{0}-{1}", order.seq, 1);
                            detailPo.IsBackgColor = false;
                        }
                        //缺桥
                        detailPo.IsShowColor = string.Format("IsShow-{0}-{1}", order.seq, reply.isShowColor ? 0 : 1);
                        //detailPo.IsShowColor = string.Format("IsShow-{0}-{1}", order.seq, order.DetailMark == "1" ? 2 : 1);
                        detailPo.IsForeColor = reply.isShowColor ? true : false;
                        //当日成交，细体
                        detailPo.Isliq = string.Format("Isliq-{0}-{1}", order.seq, reply.isliq ? 0 : 1);
                        detailPo.IsFontWeight = !reply.isliq;

                        detailPo.Padding = string.Format("Padding--1");
                        detailPo.FontFamily = string.Format("Family--1");
                        detailPo.FontSize = string.Format("Size--1");
                        //汇总不显示
                        detailPo.SumIsVisible = "Hidden";
                        detailPo.SendIsVisible = "Hidden";
                        //不是header
                        detailPo.IsHeader = string.Format("Header--1");
                        detailPo.IsWidth = string.Format("Width--1");
                        detailPo.IsMargin = string.Format("Margin--1");
                        detailPo.BondCode = order.goodsCode.Replace(".IB", "");
                        if (order.price.HasValue)
                            detailPo.Price = order.price.Value;
                        detailPo.OfrDealdate = order.dealdate2;
                        detailPo.BidDealdate = order.dealdate;
                        detailPo.OfrSettlementdate = order.settlementdate2;
                        detailPo.BidSettlementdate = order.settlementdate;
                        detailPo.ExecId = order.execId;
                        ///删除线是否显示
                        detailPo.IsShowDel = order.orderStatus + "" == "D" ? true : false;
                        ///所属分组，用于后续的过滤删选
                        detailPo.Key = key;
                        //v2.6 加入债券信息
                        detailPo.goodstype = order.goodstype;
                        detailPo.IssueAmount = order.IssueAmount;
                        detailPo.IssuerRatingCurrent = order.IssuerRatingCurrent;
                        detailPo.DetailTrader = Key;

                        //2.14加 20211026
                        if (LocalEnv.ConfInfo.IsShowReserveButton)
                        {
                            if (order.forbidRefreshFlag == "1"
                            || order.forbidRefreshFlag == "2"
                            || order.forbidRefreshFlag == "3")
                            {
                                detailPo.ForbidRefreshFlagVisible = Visibility.Visible;
                            }
                            else
                            {
                                detailPo.ForbidRefreshFlagVisible = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            detailPo.ForbidRefreshFlagVisible = Visibility.Collapsed;
                        }

                        //订单号
                        detailPo.OrderId = order.orderId;
                        //2.16加 20211217
                        SetSendInstInfo(Key, order, detailPo);

                        //2.20.2 20220715
                        SetOrderStatus(order, detailPo);

                        //v3.3.0 版本
                        SetSendQQMsg(Key, order, detailPo);

                        //detailPo.DetailMarkDisplay = order.DetailMark;

                        if (order.MarkDetailUserList != null && order.MarkDetailUserList.Contains(LocalEnv.UserInfo.UserId))
                        {
                            detailPo.DetailMarkDisplay = "1";
                        }
                        else {
                            detailPo.DetailMarkDisplay = "0";
                        }
                        detailPo.MarkDetailUserList = order.MarkDetailUserList;

                        if (isTodayLater(order))
                            detailPo.TodayLaterVisible = Visibility.Visible;
                        else
                            detailPo.TodayLaterVisible = Visibility.Collapsed;


                        if (isDetail)
                        {
                            data.Add(detailPo);
                            dicData[Key].Add(detailPo);
                            //该机构下全部成交单统计完毕的时候开始统计汇总
                            if (i == keyValues.Count)
                            {
                                #region 汇总 

                                if (dicSum.Keys.Any(a => a.Key == Key.Key))
                                {
                                    ///因为每次刷新重建DetailTrader，所以需要更换DetailTrader；
                                    dicSum.Remove(dicSum.Keys.First(a => a.Key == Key.Key));
                                    dicSum.Add(Key, new List<PositionDetailPo>());

                                    var pos = GetSumPositionDetailPo(dicInst, Key);
                                    foreach (var po in pos)
                                    {
                                        data.Add(po);
                                        dicData[Key].Add(po);
                                        dicSum[Key].Add(po);
                                    }
                                }
                                #endregion
                            }
                            if (i.Equals(keyValues.Count))
                            {
                                //wxp 加入机构末尾显示
                                PositionDetailPo po = new PositionDetailPo();
                                po.DataType = EDataType.尾;
                                po.SumIsVisible = "Hidden";
                                po.SendIsVisible = "Hidden";
                                po.Text = "";
                                po.TotalText = "";
                                //po.WithContactsText = "";
                                po.Isliq = string.Format("Isliq--0");
                                po.IsWidth = string.Format("Width--0");
                                po.IsHeader = string.Format("Header--0");
                                po.IsMargin = string.Format("Margin--0");
                                po.Padding = string.Format("Padding--0");
                                po.FontFamily = string.Format("Family--0");
                                po.FontSize = string.Format("Size--0");
                                po.IsShowColor = string.Format("IsShow--1");
                                po.Isbrokerunread = string.Format("Isbroker--1");
                                data.Add(po);
                                dicData[Key].Add(po);
                            }
                            i++;
                        }
                        else
                        {
                            ///排除相同的seq
                            if (!dicDetail.ContainsKey(decimal.Parse(order.seq)))
                            {
                                dicDetail.Add(decimal.Parse(order.seq), order);
                                data.Add(detailPo);
                                z++;
                            }
                        }
                    }
                }
                #endregion
            }
            if (!isDetail)
            {
                List<PositionDetailPo> li = new List<PositionDetailPo>();
                //wxp 空序号删除
                if (data.Count >= 2)
                {
                    var a = from m in data
                            where  m.Seq != null
                            select m;
                    li.AddRange(a.ToList());
                }
                return li;
            }
            return data;
        }
        #endregion

        private static bool isTodayLater(CopyDetailPo order)
        {
            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
            bool isTodayL = false;
            if (!string.IsNullOrEmpty(date))
            {
                DateTime dt = TransOrderHelper.Instance.GetDate(date);
                if (dt.Date > DateTime.Now.Date)
                    isTodayL = true;
            }
            return isTodayL;
        }


        #region 今日历史成交单
        /// <summary>
        /// 获取历史成交单展示数据
        /// 结构：历史-空格-机构头-机构成交-汇总-空格
        /// </summary>
        /// <param name="templist"></param>
        /// <param name="isDetail"></param>
        /// <param name="condition"></param>
        /// <param name="isShowDir">是否展示方向</param>
        /// <param name="isShowDir">是否展示方向</param>
        /// <returns></returns>
        public static List<PositionDetailPo> GetHistDetail(List<CopyDetailPo> templist, bool isDetail, SearchPo condition, bool isShowDir, bool isForce, TextBlockModel textBlockModel, TextBlockModel textBlockModel2)
        {
            List<PositionDetailPo> data = new List<PositionDetailPo>();
            dichistInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
            ElsehistOrderlist.Clear();
            OwnNoinsthistOrderlist.Clear();
            dichistData = new Dictionary<DetailTrader, List<PositionDetailPo>>();

            #region 机构组装

            List<CopyDetailPo> temp = new List<CopyDetailPo>();
            foreach (var order in templist)
            {
                //2.4.3临时bug，修改broker的单子，不需要显示，直接删除的单子不要了
                //判断逻辑   
                if ("D".Equals(order.orderStatus) || "C".Equals(order.orderStatus) || order.delflag + "" == "1")
                {
                    continue;
                }
                //if ("G".Equals(order.orderStatus) && !"1".Equals(order.delflag))
                if ("G".Equals(order.orderStatus) || "D".Equals(order.orderStatus))
                {//删除的单子也要显示 20200402
                    temp.Add(order);
                }
            }
            var list = new List<CopyDetailPo>();
            if (isDetail)
                list = condition == null ? temp.OrderBy(o => o.TransactionTime).ThenBy(o => o.SeqSort).ToList() : Search(temp, condition);
            else
                list = temp;
            foreach (var order in list)
            {
                string key = ConvertTextHelper.GetKey(order, 0);
                DetailTrader detailTrader = ConvertTextHelper.GetTrader(order, 0);
                if (isContain(0, order))
                {
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshorta))
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankcodeshorta))
                        {
                            key = order.bankcodeshorta;
                            detailTrader.Key = order.bankcodeshorta;
                        }

                    }
                    else
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankCodeA))
                        {
                            key = order.bankCodeA;
                            detailTrader.Key = order.bankCodeA;
                        }
                    }

                    if (!dichistInst.Keys.Any(_ => _.Key == key))
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dichistInst.Add(detailTrader, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dichistInst.Add(detailTrader, li);
                            }
                        }
                    }
                    else
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader trader = dichistInst.Keys.FirstOrDefault(_ => _.Key == key);
                                List<CopyDetailPo> keylist = dichistInst[trader];
                                if (!keylist.Contains(order))
                                {
                                    dichistInst[trader].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                DetailTrader trader = dichistInst.Keys.FirstOrDefault(_ => _.Key == key);
                                List<CopyDetailPo> keylist = dichistInst[trader];
                                if (!keylist.Contains(order))
                                {
                                    dichistInst[trader].Add(order);
                                }
                            }
                        }
                    }
                }

                String keyb = ConvertTextHelper.GetKey(order, 1);
                DetailTrader detailTraderB = ConvertTextHelper.GetTrader(order, 1);
                if (isContain(1, order))
                {
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshortb))
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankcodeshortb))
                        {
                            keyb = order.bankcodeshortb;
                            detailTraderB.Key = order.bankcodeshortb;
                        }
                    }
                    else
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankCodeB))
                        {
                            keyb = order.bankCodeB;
                            detailTraderB.Key = order.bankCodeB;
                        }
                    }

                    if (!dichistInst.Keys.Any(_ => _.Key == keyb))
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dichistInst.Add(detailTraderB, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dichistInst.Add(detailTraderB, li);
                            }
                        }
                    }
                    else
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader traderB = dichistInst.Keys.FirstOrDefault(_ => _.Key == keyb);
                                List<CopyDetailPo> keylist = dichistInst[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dichistInst[traderB].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                DetailTrader traderB = dichistInst.Keys.FirstOrDefault(_ => _.Key == keyb);
                                List<CopyDetailPo> keylist = dichistInst[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dichistInst[traderB].Add(order);
                                }
                            }
                        }
                    }
                }

                if (!LocalEnv.UserInfo.FullName.Equals(order.brokerNameA) && !LocalEnv.UserInfo.FullName.Equals(order.brokerNameB))
                {
                    ElsehistOrderlist.Add(order);
                }

                // 如果是其他人成交单，且有按机构交易员拆分设置，需要使用被代理的broker name 判断，是否是无机构成交单
                if (GRSettingsHandler.AppSettingIsTrue(Constant.SplitShowOtherOrders))
                {
                    if (IsNoInstOrder(order, CurrentUserBrokerId))
                    {
                        OwnNoinsthistOrderlist.Add(order);
                    }
                }
                else if (isContain(2, order))
                {
                    OwnNoinsthistOrderlist.Add(order);
                }
            }

            HashSet<string> otherOrderIds = new HashSet<string>();
            if (ElsehistOrderlist != null && ElsehistOrderlist.Count > 0)
            {
                if (GRSettingsHandler.AppSettingIsTrue(Constant.SplitShowOtherOrders))
                {
                    ElsehistOrderlist.ForEach(x => otherOrderIds.Add(x.execId));
                    dichistInst.Add(ElsehistOrderTrader, new List<CopyDetailPo>());
                    SplitOtherOrders(ElsehistOrderlist, condition, dichistInst);
                }
                else
                {
                    dichistInst.Add(ElsehistOrderTrader, ElsehistOrderlist);
                }
            }
            if (OwnNoinsthistOrderlist != null && OwnNoinsthistOrderlist.Count > 0)
            {
                dichistInst.Add(OwnNoinsthistOrderTrader, OwnNoinsthistOrderlist);
            }
            #endregion

            if (dichistInst != null && dichistInst.Count > 0)
            {
                //加入历史头
                // string key = "以下为今日发单的历史成交单";
                string key = "历史成交单";
                DetailTrader detailTrader = new DetailTrader() { Key = key };
                PositionDetailPo dp = new PositionDetailPo();
                dp.DataType = EDataType.历史头;
                dp.Seq = null;
                //dp.Text = toFormat(txb, key);
                textBlockModel.Text = string.IsNullOrEmpty(dp.Text) ? String.Empty : dp.Text;
                dp.Text = toFormat(textBlockModel, key, true);
                dp.Key = key;
                dp.Text2 = toFormat2(textBlockModel, textBlockModel2, key, true);
                dp.TotalText = toFormatCopy(textBlockModel, key, true);
                dp.CopyText = toFormatCopy(textBlockModel, key, true);
                dp.WithoutBrokerText = toFormatCopy(textBlockModel, key, true);
                dp.Padding = string.Format("Padding--0");
                dp.FontFamily = string.Format("Family--0");
                dp.FontSize = string.Format("Size--0");
                dp.SumIsVisible = "Hidden";
                dp.SendIsVisible = "Hidden";
                dp.IsHeader = string.Format("Header--0");
                dp.IsMargin = string.Format("Margin--0");

                dp.IsWidth = string.Format("Width--0");
                dp.IsShowColor = string.Format("IsShow--1");
                dp.Isbrokerunread = string.Format("Isbroker--1");
                dp.Isliq = string.Format("Isliq--0");
                dp.IsHist = true;
                data.Add(dp);
                dichistData.Add(detailTrader, new List<PositionDetailPo>());
                dichistData[detailTrader].Add(dp);
                ///加入空格
                PositionDetailPo po = new PositionDetailPo();
                po.DataType = EDataType.历史头;
                po.SumIsVisible = "Hidden";
                po.SendIsVisible = "Hidden";
                po.Isliq = string.Format("Isliq--0");
                po.IsWidth = string.Format("Width--0");
                po.IsHeader = string.Format("Header--0");
                po.IsMargin = string.Format("Margin--0");
                po.Padding = string.Format("Padding--0");
                po.FontFamily = string.Format("Family--0");
                po.FontSize = string.Format("Size--0");
                po.IsShowColor = string.Format("IsShow--1");
                po.Isbrokerunread = string.Format("Isbroker--1");
                po.Text = "";
                po.TotalText = "";
                //po.WithContactsText = "";
                po.IsHist = true;
                data.Add(po);
                dichistData[detailTrader].Add(po);///历史头，占两个
            }
            Dictionary<string, CopyDetailPo> dicDetail = new Dictionary<string, CopyDetailPo>();
            int j = 0; int z = 1;

            List<DetailTrader> detailTraderKeys = dichistInst.Keys.ToList();
            foreach (var Key in detailTraderKeys)
            {
                if (condition.IsMaturityOrder)
                {
                    dichistInst[Key] = dichistInst[Key].OrderBy(_ => _.maturityDays).ToList();

                }

                string key = Key.Key;

                if (ElsehistOrder.Equals(key) && GRSettingsHandler.AppSettingIsTrue(Constant.SplitShowOtherOrders))
                {
                    // 其他人成交单头
                    var dpOther = GenerateOtherOrderHeader(key, textBlockModel, textBlockModel2);
                    data.Add(dpOther);
                    dichistData[Key] = new List<PositionDetailPo> { dpOther };

                    //加入空格
                    var empty = GenerateEmpty();
                    data.Add(empty);
                    dichistData[Key].Add(empty);
                    continue;
                }

                if (isForce)
                {
                    //强制刷新方向
                    dichistSide[key] = isShowDir;
                }
                else
                {
                    if (!dichistSide.ContainsKey(key))
                        dichistSide.Add(key, isShowDir);
                }


                ////加入机构头
                dichistData.Add(Key, new List<PositionDetailPo>());

                PositionDetailPo dp = new PositionDetailPo();
                dp.DetailTrader = Key;
                dp.DataType = EDataType.头;
                dp.Seq = 0;
                //dp.Text = toFormat(txb, key);
                textBlockModel.Text = string.IsNullOrEmpty(dp.Text) ? String.Empty : dp.Text;
                dp.Text = toFormat(textBlockModel, key, false);
                dp.Key = key;
                dp.Text2 = toFormat2(textBlockModel, textBlockModel2, key, false);
                dp.TotalText = toFormatCopy(textBlockModel, key, false);
                dp.CopyText = toFormatCopy(textBlockModel, key, false);
                dp.WithoutBrokerText = toFormatCopy(textBlockModel, key, false);
                dp.Padding = string.Format("Padding--0");
                dp.FontFamily = string.Format("Family--0");
                dp.FontSize = string.Format("Size--0");
                //汇总按钮是否显示
                if (!key.Equals(ElsehistOrder) && !key.Equals(OwnNoinsthistOrder))
                    dp.SumIsVisible = "Visible";
                else
                    dp.SumIsVisible = "Hidden";
                //发送按钮是否显示, 合并显示的不需要显示发送，合并显示的只有机构所以不带空格，这个写法很不好
                if (key.Trim().Contains(" "))
                    dp.SendIsVisible = "Visible";
                else
                    dp.SendIsVisible = "Hidden";
                dp.IsHeader = string.Format("Header--0");
                dp.IsMargin = string.Format("Margin--0");

                dp.IsWidth = string.Format("Width--0");
                dp.IsShowColor = string.Format("IsShow--1");
                dp.Isbrokerunread = string.Format("Isbroker--1");
                dp.Isliq = string.Format("Isliq--0");
                dp.IsHist = true;
                data.Add(dp);
                dichistData[Key].Add(dp);
                j++;
                List<CopyDetailPo> keyValues = new List<CopyDetailPo>();
                dichistInst.TryGetValue(Key, out keyValues);
                //if (condition.IsMaturityOrder)
                //{
                //    keyValues = (from kv in keyValues
                //                 orderby kv.maturityDays ascending
                //                 select kv).ToList();
                //}
                int i = 1;
                ///加入成交单
                foreach (CopyDetailPo order in keyValues)
                {
                    PositionDetailPo detailPo = new PositionDetailPo();
                    detailPo.DataType = EDataType.数据;
                    detailPo.Seq = order.seq == null ? 0 : Convert.ToDecimal(order.seq);
                    detailPo.IsHist = true;

                    string proxyBrokerId = string.Empty;
                    if (otherOrderIds.Contains(order.execId))
                    {
                        // 其他人分机构交易员下的key 多一个空格，用来区分自己的分机构交易员标题，所以要trim
                        var trueKey = key.Trim();
                        if (!dicSet.ContainsKey(trueKey))
                        {
                            proxyBrokerId = GetProxyBrokerIdByInstTraderName(order, trueKey);
                        }
                        else
                        {

                            proxyBrokerId = GetProxyBrokerId(order, trueKey);
                        }
                    }

                    OrderArgs reply = new OrderArgs();
                    if (isDetail)
                        reply = ConvertTextHelper.ConvertText(order, i, Key, detailPo, isDetail, proxyBrokerId);
                    else
                        reply = ConvertTextHelper.ConvertText(order, z, Key, detailPo, isDetail, proxyBrokerId);

                    detailPo.HideCounterpartyText = ConvertTextHelper.GetCompanyDetial(reply.HideCounterpartyText);
                    detailPo.Text = reply.Text;
                    //detailPo.TotalText = ConvertTextHelper.GetCompanyDetial(reply.Text);
                    detailPo.TotalText = ConvertTextHelper.GetCompanyDetial(reply.CopyText);
                    detailPo.WithContactsText = ConvertTextHelper.GetCompanyDetial(CopyDetailHelper.GetWithContactsText(reply.CopyText, order, Key));
                    detailPo.BrokerCompanyName = ConvertTextHelper.GetCompanyDetial("");
                    detailPo.WithTraderText = ConvertTextHelper.GetCompanyDetial(reply.WithTraderText);
                    //隐藏机构交易员发送qq消息
                    detailPo.HideSendQQMsgText = ConvertTextHelper.GetCompanyDetial(reply.HideSendQQMsgText);
                    detailPo.ToolText = reply.Text;
                    detailPo.BrokerOfr = ConvertTextHelper.IsChinese(order.OfrBroker);
                    detailPo.BrokerBid = ConvertTextHelper.IsChinese(order.BidBroker);
                    //助理明细发送文本
                    detailPo.AssistantAText = ConvertTextHelper.GetCompanyDetial(reply.AssistantAText);
                    detailPo.AssistantBText = ConvertTextHelper.GetCompanyDetial(reply.AssistantBText);
                    int index = reply.CopyText.IndexOf(')');
                    string tr = reply.CopyText.Substring(0, index + 1);
                    detailPo.CopyText = ConvertTextHelper.GetCompanyDetial(string.Format("{0}vs{1} {2}-{3}{4}", detailPo.BrokerOfr, detailPo.BrokerBid, order.seq, tr, reply.CopyText.Substring(tr.Length)));
                    detailPo.WithoutBrokerText = ConvertTextHelper.GetCompanyDetial(string.Format("{0}-{1}{2}", order.seq, tr, reply.CopyText.Substring(tr.Length)));
                    //if (order.brokerunread != null && order.brokerunread. Contains(LocalEnv.UserInfo.UserId))
                    if (LocalEnv.UserInfo.IsContainsUserId(order.brokerunread))
                    {
                        detailPo.Isbrokerunread = string.Format("Isbroker-{0}-{1}", order.seq, 0);
                        detailPo.IsBackgColor = true;
                    }
                    else
                    {
                        detailPo.Isbrokerunread = string.Format("Isbroker-{0}-{1}", order.seq, 1);
                        detailPo.IsBackgColor = false;
                    }
                    detailPo.IsShowColor = string.Format("IsShow-{0}-{1}", order.seq, reply.isShowColor ? 0 : 1);
                    detailPo.IsForeColor = reply.isShowColor ? true : false;
                    detailPo.Isliq = string.Format("Isliq-{0}-{1}", order.seq, reply.isliq ? 0 : 1);
                    detailPo.IsFontWeight = !reply.isliq;
                    detailPo.Padding = string.Format("Padding--1");
                    detailPo.FontFamily = string.Format("Family--1");
                    detailPo.FontSize = string.Format("Size--1");
                    detailPo.SumIsVisible = "Hidden";
                    detailPo.SendIsVisible = "Hidden";
                    detailPo.IsHeader = string.Format("Header--1");
                    detailPo.IsWidth = string.Format("Width--1");
                    detailPo.IsMargin = string.Format("Margin--1");
                    detailPo.BondCode = order.goodsCode.Replace(".IB", "");
                    detailPo.Price = order.price.Value;
                    detailPo.OfrDealdate = order.dealdate2;
                    detailPo.BidDealdate = order.dealdate;
                    detailPo.OfrSettlementdate = order.settlementdate2;
                    detailPo.BidSettlementdate = order.settlementdate;
                    detailPo.ExecId = order.execId;
                    ///删除线是否显示
                    detailPo.IsShowDel = order.orderStatus + "" == "D" ? true : false;
                    //v2.6 加入债券信息
                    detailPo.goodstype = order.goodstype;
                    detailPo.IssueAmount = order.IssueAmount;
                    detailPo.IssuerRatingCurrent = order.IssuerRatingCurrent;

                    detailPo.DetailTrader = Key;

                    //2.14加 20211026
                    if (LocalEnv.ConfInfo.IsShowReserveButton)//if (LocalEnv.ConfInfo.IsShowInternalTransaction)
                    {
                        if (order.forbidRefreshFlag == "1"
                        || order.forbidRefreshFlag == "2"
                        || order.forbidRefreshFlag == "3")
                        {
                            detailPo.ForbidRefreshFlagVisible = Visibility.Visible;
                        }
                        else
                        {
                            detailPo.ForbidRefreshFlagVisible = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        detailPo.ForbidRefreshFlagVisible = Visibility.Collapsed;
                    }

                    //if (order.forbidRefreshFlag == "1")
                    //{
                    //    detailPo.ForbidRefreshBrush = "#F0892A";
                    //}
                    //else if (order.forbidRefreshFlag == "2")
                    //{
                    //    detailPo.ForbidRefreshBrush = "#1AA1FF";
                    //}
                    //else if (order.forbidRefreshFlag == "3")
                    //{
                    //    detailPo.ForbidRefreshBrush = "#67C139";
                    //}
                    //else
                    //{
                    //    detailPo.ForbidRefreshBrush = "#F0892A";
                    //}

                    //2.16加 20211217
                    SetSendInstInfo(Key, order, detailPo);

                    //订单号
                    detailPo.OrderId = order.orderId;

                    //2.20.2 20220715
                    SetOrderStatus(order, detailPo);

                    //v3.3.0 版本
                    SetSendQQMsg(Key, order, detailPo);

                    //detailPo.DetailMarkDisplay = order.DetailMark;

                    if (order.MarkDetailUserList != null && order.MarkDetailUserList.Contains(LocalEnv.UserInfo.UserId))
                    {
                        detailPo.DetailMarkDisplay = "1";
                    }
                    else
                    {
                        detailPo.DetailMarkDisplay = "0";
                    }
                    detailPo.MarkDetailUserList = order.MarkDetailUserList;

                    if (isTodayLater(order))
                        detailPo.TodayLaterVisible = Visibility.Visible;
                    else
                        detailPo.TodayLaterVisible = Visibility.Collapsed;

                    ///汇总
                    if (isDetail)
                    {
                        data.Add(detailPo);
                        dichistData[Key].Add(detailPo);
                        if (i == keyValues.Count)
                        {
                            #region 汇总
                            if (dichistSum.Keys.Any(a => a.Key == Key.Key))
                            {
                                ///因为每次刷新重建DetailTrader，所以需要更换DetailTrader；
                                dichistSum.Remove(dichistSum.Keys.First(a => a.Key == Key.Key));
                                dichistSum.Add(Key, new List<PositionDetailPo>());

                                var pos = GetSumPositionDetailPo(dichistInst, Key);
                                foreach (var po in pos)
                                {
                                    data.Add(po);
                                    dichistData[Key].Add(po);
                                    dichistSum[Key].Add(po);
                                }
                            }
                            #endregion
                        }
                        //加入尾部
                        if (i.Equals(keyValues.Count))
                        {
                            PositionDetailPo po = new PositionDetailPo();
                            po.DataType = EDataType.尾;
                            po.SumIsVisible = "Hidden";
                            po.SendIsVisible = "Hidden";
                            po.Text = "";
                            po.TotalText = "";
                            //detailPo.WithContactsText ="";
                            po.IsHist = true;
                            po.Isliq = string.Format("Isliq--0");
                            po.IsWidth = string.Format("Width--0");
                            po.IsHeader = string.Format("Header--0");
                            po.IsMargin = string.Format("Margin--0");
                            po.Padding = string.Format("Padding--0");
                            po.FontFamily = string.Format("Family--0");
                            po.FontSize = string.Format("Size--0");
                            po.IsShowColor = string.Format("IsShow--1");
                            po.Isbrokerunread = string.Format("Isbroker--1");
                            data.Add(po);
                            dichistData[Key].Add(po);
                        }
                        i++;
                    }
                    else
                    {
                        if (!dicDetail.ContainsKey(order.execId))
                        {
                            dicDetail.Add(order.execId, order);
                            data.Add(detailPo);
                            z++;
                        }
                    }
                }
            }
            if (!isDetail)
            {
                List<PositionDetailPo> li = new List<PositionDetailPo>();
                if (data.Count >= 2)
                {
                    var a = from m in data
                            where m.Seq != null
                            select m;
                    li.AddRange(a.ToList());
                }
                return li;
            }
            return data;
        }
        #endregion


        /// <summary>
        /// 获取汇总信息
        /// </summary>
        /// <param name="dicInst"></param>
        /// <param name="key"></param>
        public static List<PositionDetailPo> GetSumPositionDetailPo(Dictionary<DetailTrader, List<CopyDetailPo>> dicInst, DetailTrader key)
        {
            List<PositionDetailPo> result = new List<PositionDetailPo>();
            List<string> dic = new List<string>();
            //汇总该机构下所有的成交单
            foreach (CopyDetailPo order in dicInst[key])
            {
                //删除的单子不需要汇总
                if (order.orderStatus == "D")
                    continue;
                //一个合约，只计算一次，去重
                if (dic.Contains(order.goodsCode))
                    continue;
                bool isS = false;
                bool isB = false;
                decimal totalG = decimal.Zero; decimal totalT = decimal.Zero;
                string insta = ConvertTextHelper.GetKey(order, 0);//GetInst(order.bankcodeshorta, order.bankCodeA);

                // 如果是其他人成交单，只按机构交易员判断方向
                bool isOther = GRSettingsHandler.AppSettingIsTrue(SplitShowOtherOrders)
                    && !LocalEnv.UserInfo.IsMyself(order.brokerIdA)
                    && !LocalEnv.UserInfo.IsMyself(order.brokerIdB);

                var trueKey = key.Key.Trim();

                //如果是GVN单
                if (order.IsGVN)
                {
                    //如果是机构A
                    if (!string.IsNullOrWhiteSpace(insta) && insta.Contains(trueKey) && (order.brokerNameA.Equals(LocalEnv.UserInfo.FullName) || isOther))
                    {
                        totalG += order.qty.HasValue ? order.qty.Value : decimal.Zero;
                        isS = true;
                    }
                    else
                    {
                        totalT += order.qty.HasValue ? order.qty.Value : decimal.Zero;
                        isB = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(insta) && insta.Contains(trueKey) && (order.brokerNameA.Equals(LocalEnv.UserInfo.FullName) || isOther))
                    {
                        totalT += order.qty.HasValue ? order.qty.Value : decimal.Zero;
                        isB = true;
                    }
                    else
                    {
                        totalG += order.qty.HasValue ? order.qty.Value : decimal.Zero;
                        isS = true;
                    }
                }
                #region 汇总合约下所有成交单

                foreach (CopyDetailPo order2 in dicInst[key])
                {
                    //删除的单子不需要汇总
                    if (order2.orderStatus == "D")
                        continue;
                    string insta2 = ConvertTextHelper.GetKey(order2, 0);//GetInst(order2.bankcodeshorta, order2.bankCodeA);
                    ///汇总该合约下所有的成交单，去除重复的成交单
                    //if (string.IsNullOrEmpty(order.seq) || !order.seq.Equals(order2.seq))
                    //    continue;
                    if (!order.seq.Equals(order2.seq) && order.goodsCode.Equals(order2.goodsCode))
                    {
                        if (order2.IsGVN)
                        {
                            if (!string.IsNullOrWhiteSpace(insta2) && insta2.Contains(trueKey) && (order2.brokerNameA.Equals(LocalEnv.UserInfo.FullName) || isOther))
                            {
                                totalG += order2.qty.HasValue ? order2.qty.Value : decimal.Zero;
                                isS = true;
                            }
                            else
                            {
                                totalT += order2.qty.HasValue ? order2.qty.Value : decimal.Zero;
                                isB = true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(insta2) && insta2.Contains(trueKey) && (order2.brokerNameA.Equals(LocalEnv.UserInfo.FullName) || isOther))
                            {
                                totalT += order2.qty.HasValue ? order2.qty.Value : decimal.Zero;
                                isB = true;
                            }
                            else
                            {
                                totalG += order2.qty.HasValue ? order2.qty.Value : decimal.Zero;
                                isS = true;
                            }
                        }
                    }
                }
                #endregion
                string Text = "";
                string goodcode = order.goodsCode.Replace(".IB", "");
                string goodnName = (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatShortNameCheck)) ? string.Format("{0} ", order.goodsShortName) : "";
                //goodnName = (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatShortNameCheck)) ? string.Format("{0} ", order.goodsShortName) : "";

                if (string.IsNullOrEmpty(goodnName) && BondHelper.IsLGBBond(order.goodsCode) && GRSettingsHandler.AppSettingIsTrue(Constant.WithShortNameLGB))
                {
                    goodnName = string.Format("{0} ", order.goodsShortName);
                }

                if (GRSettingsHandler.AppSettingIsTrue(Constant.WithpPivatebondtype))
                {
                    BondsInfoPo bond = Utils.CommonUtils.GetBondsInfoByCode(order.goodsCode);
                    //CRMDataHandler.Instance.GetBondsInfoByCode(order.goodsCode);
                    if (bond != null && !string.IsNullOrWhiteSpace(bond.Asset_Status) && bond.Asset_Status == "PRI")
                    {
                        goodnName = goodnName + " " + "私募债" + "  ";
                    }
                }

                if (ConvertTextHelper.SetParam != null && "true".Equals(ConvertTextHelper.SetParam.formatDebtTypeCheck))
                {
                    string fundObjectivesCategoryStr = CopyDetailHelper.GetFundObjectivesCategory(order.goodsCode);
                    if (!string.IsNullOrWhiteSpace(fundObjectivesCategoryStr))
                    {
                        goodnName = goodnName + " " + fundObjectivesCategoryStr;
                    }
                }
                string blank = "          【";
                string copytx = "";
                if (isS && isB)
                {
                    Text = string.Format("{0}{1} {2}total 买入 {3}E 卖出 {4}E】", blank, goodcode, goodnName, totalT / 10000, totalG / 10000);
                    copytx = Text.TrimStart();
                }
                else if (isS)
                {
                    Text = string.Format("{0}{1} {2}total 卖出 {3}E】", blank, goodcode, goodnName, totalG / 10000);
                    copytx = Text.TrimStart();
                }
                else if (isB)
                {
                    Text = string.Format("{0}{1} {2}total 买入 {3}E】", blank, goodcode, goodnName, totalT / 10000);
                    copytx = Text.TrimStart();
                }
                LogHelper.Info("汇总计算结果，当前交易员为：{1}：\r\n{0}", copytx, trueKey);
                PositionDetailPo po = new PositionDetailPo();
                po.DataType = EDataType.汇总;
                po.Padding = string.Format("Padding--1");
                po.FontFamily = string.Format("Family--2");
                po.FontSize = string.Format("Size--1");
                po.Isliq = string.Format("Isliq-1-1");
                po.IsFontWeight = true;
                po.SumIsVisible = "Hidden";
                po.SendIsVisible = "Hidden";
                po.IsWidth = string.Format("Width--0");
                po.IsHeader = string.Format("Header--0");
                po.IsMargin = string.Format("Margin--0");
                po.IsShowColor = string.Format("IsShow--3");
                po.Isbrokerunread = string.Format("Isbroker--1");
                po.Text = Text;
                po.TotalText = copytx;
                po.WithContactsText = CopyDetailHelper.GetWithContactsText(copytx, order, key);
                po.CopyText = copytx;
                po.WithoutBrokerText = copytx;
                po.WithContactsText = copytx;
                po.IsSum = true;

                result.Add(po);
                //if (index < data.Count)
                //    data.Insert(index, po);
                //else
                //    data.Add(po);
                //index++;
                ////一条汇总数据加入机构
                //dicData[key].Add(po);

                //排除已汇总的合约
                dic.Add(order.goodsCode);
            }

            return result;
        }

        /// <summary>
        /// 获取地方债备注说明
        /// </summary>
        /// <param name="bondCode"></param>
        /// <returns></returns>
        public static string GetFundObjectivesCategory(string bondCode)
        {
            string bondtype = "";
            BondsInfoPo bond = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(bondCode);
            if (bond != null)
            {
                if (!string.IsNullOrWhiteSpace(bond.Bond_Key) && (bond.Bond_Subtype.ToUpper() == "TLB" || bond.Bond_Subtype.ToUpper() == "LLB"))
                {
                    if (bond.Fund_Objectives_Category == "一般债券" || bond.Fund_Objectives_Category == "普通专项")
                    {
                        bondtype = bond.Fund_Objectives_Category + "  ";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(bond.Category_flag))
                        {
                            bondtype = bond.Category_flag.Replace("专项", "") + "  ";
                        }
                    }
                }
            }
            return bondtype;
        }

        //public static string GetPluginShareKey(EPluginShareKey pluginShareKey)
        //{
        //    return string.Format("{0}_{1}", Math.Floor((int)pluginShareKey / 10.0), pluginShareKey.ToString());
        //}
        /// <summary>
        /// 设置发送单条qq消息状态
        /// </summary>
        private static void SetSendQQMsg(DetailTrader Key, CopyDetailPo order, PositionDetailPo detailPo)
        {
            if (string.IsNullOrWhiteSpace(Key.InstID))
            {
                detailPo.SendQQMsgVisible = Visibility.Collapsed;
            }
            else
            {
                if (order.bankIdA == order.bankIdB)
                {
                    if (order.bankagentIdA == Key.TraderID)
                    {
                        if (order.SingleQQMsgSendFlaga == "0" && CopyDetailHandler.Instance.IsMyModifys(order.brokerIdA))
                        {
                            detailPo.SendQQMsgVisible = Visibility.Visible;
                        }
                        else
                        {
                            detailPo.SendQQMsgVisible = Visibility.Collapsed;
                        }
                        detailPo.SendQQbankagentId = order.bankagentIdA;
                        detailPo.SendQQbankagentName = order.bankAgentNameA;
                        detailPo.SendQQbankId = order.bankIdA;
                        detailPo.BrokerID = order.brokerIdA;
                    }
                    else
                    {
                        if (order.SingleQQMsgSendFlagb == "0" && CopyDetailHandler.Instance.IsMyModifys(order.brokerIdB))
                        {
                            detailPo.SendQQMsgVisible = Visibility.Visible;
                        }
                        else
                        {
                            detailPo.SendQQMsgVisible = Visibility.Collapsed;
                        }
                        detailPo.SendQQbankagentId = order.bankAgentIdB;
                        detailPo.SendQQbankagentName = order.bankAgentNameB;
                        detailPo.SendQQbankId = order.bankIdB;
                        detailPo.BrokerID = order.brokerIdB;
                    }
                }
                else
                {
                    if (Key.InstID == order.bankIdA)
                    {
                        if (order.SingleQQMsgSendFlaga == "0" && CopyDetailHandler.Instance.IsMyModifys(order.brokerIdA))
                        {
                            detailPo.SendQQMsgVisible = Visibility.Visible;
                        }
                        else
                        {
                            detailPo.SendQQMsgVisible = Visibility.Collapsed;
                        }
                        detailPo.SendQQbankagentId = order.bankagentIdA;
                        detailPo.SendQQbankagentName = order.bankAgentNameA;
                        detailPo.SendQQbankId = order.bankIdA;
                        detailPo.BrokerID = order.brokerIdA;
                    }
                    else
                    {
                        if (order.SingleQQMsgSendFlagb == "0" && CopyDetailHandler.Instance.IsMyModifys(order.brokerIdB))
                        {
                            detailPo.SendQQMsgVisible = Visibility.Visible;
                        }
                        else
                        {
                            detailPo.SendQQMsgVisible = Visibility.Collapsed;
                        }
                        detailPo.SendQQbankagentId = order.bankAgentIdB;
                        detailPo.SendQQbankagentName = order.bankAgentNameB;
                        detailPo.SendQQbankId = order.bankIdB;
                        detailPo.BrokerID = order.brokerIdB;
                    }
                }
            }
        }

        /// <summary>
        /// 设置外发机构标识
        /// </summary>
        private static void SetSendInstInfo(DetailTrader Key, CopyDetailPo order, PositionDetailPo detailPo)
        {
            if (LocalEnv.ConfInfo.IsShowSendManuallyButton)
            {
                if (!string.IsNullOrWhiteSpace(Key.TraderID))
                {
                    if (Key.TraderID == order.bankagentIdA)
                    {
                        if (SystemPowerHandler.Instance.SendReceiptInstDisplay.Any(_ => _.instId == order.bankIdA)
                            || CopyDetailHandler.Instance.IsAllSendExternalInst)
                        {
                            detailPo.IsBrokerATag = "1";
                            if (order.sendSorFlaga == "0" || order.sendSorFlaga == "1" || order.sendSorFlaga == "2" || order.sendSorFlaga == "3" || order.sendSorFlaga == "4")
                            {
                                if (order.sendSorFlaga == "1" || order.sendSorFlaga == "2")
                                {
                                    //亮
                                    detailPo.SendManuallyNoVisible = Visibility.Visible;
                                    detailPo.SendManuallyVisible = Visibility.Collapsed;
                                    detailPo.SendManuallyTag = "1";
                                }
                                else
                                {
                                    detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                                    detailPo.SendManuallyVisible = Visibility.Visible;
                                    detailPo.SendManuallyTag = "3";
                                }
                            }
                            else
                            {
                                detailPo.SendManuallyVisible = Visibility.Collapsed;
                                detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                                detailPo.SendManuallyTag = "-1";
                            }
                        }
                        else
                        {
                            detailPo.SendManuallyVisible = Visibility.Collapsed;
                            detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                            detailPo.SendManuallyTag = "-1";
                        }

                    }
                    else if (Key.TraderID == order.bankAgentIdB)
                    {
                        if (SystemPowerHandler.Instance.SendReceiptInstDisplay.Any(_ => _.instId == order.bankIdB)
                            || CopyDetailHandler.Instance.SendExternalInstDisplay.Any(_ => _.instid == order.bankIdB)
                            || CopyDetailHandler.Instance.IsAllSendExternalInst)
                        {
                            detailPo.IsBrokerATag = "2";
                            if (order.sendSorFlagb == "0" || order.sendSorFlagb == "1" || order.sendSorFlagb == "2" || order.sendSorFlagb == "3" || order.sendSorFlagb == "4")
                            {
                                if (order.sendSorFlagb == "1" || order.sendSorFlagb == "2")
                                {
                                    //亮
                                    detailPo.SendManuallyNoVisible = Visibility.Visible;
                                    detailPo.SendManuallyVisible = Visibility.Collapsed;
                                    detailPo.SendManuallyTag = "1";
                                }
                                else
                                {
                                    detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                                    detailPo.SendManuallyVisible = Visibility.Visible;
                                    detailPo.SendManuallyTag = "3";
                                }
                            }
                            else
                            {
                                detailPo.SendManuallyVisible = Visibility.Collapsed;
                                detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                                detailPo.SendManuallyTag = "-1";
                            }
                        }
                        else
                        {
                            detailPo.SendManuallyVisible = Visibility.Collapsed;
                            detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                            detailPo.SendManuallyTag = "-1";
                        }
                    }
                    else
                    {
                        detailPo.SendManuallyVisible = Visibility.Collapsed;
                        detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                        detailPo.SendManuallyTag = "-1";
                    }
                }
                else
                {
                    detailPo.SendManuallyVisible = Visibility.Collapsed;
                    detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                    detailPo.SendManuallyTag = "-1";
                }
            }
            else
            {
                detailPo.SendManuallyVisible = Visibility.Collapsed;
                detailPo.SendManuallyNoVisible = Visibility.Collapsed;
                detailPo.SendManuallyTag = "-1";
            }
        }

        /// <summary>
        /// 设置订单同步状态
        /// </summary>
        private static void SetOrderStatus(CopyDetailPo order, PositionDetailPo detailPo)
        {
            //if (BrokerFlags.Contains(CopyDetailHandler.Instance.BrokerNameFlag))
            //{
            //    if (OrderHandler.Instance.dicOrderStatusDcs.TryGetValue(order.execId, out var orderSync))
            //    {
            //        order.ProcessStatus = orderSync.ProcessStatus;
            //        order.FailedReason = orderSync.FailedReason;
            //    }
            //    if (order.ProcessStatus == "2")
            //    {
            //        detailPo.SynsStatusS = Visibility.Visible;
            //        detailPo.SynsStatusF = Visibility.Collapsed;
            //    }
            //    else if (order.ProcessStatus == "4")
            //    {
            //        detailPo.SynsStatusS = Visibility.Collapsed;
            //        detailPo.SynsStatusF = Visibility.Visible;
            //        detailPo.FailureReason = order.FailedReason;
            //    }
            //    else
            //    {
            //        detailPo.SynsStatusS = Visibility.Collapsed;
            //        detailPo.SynsStatusF = Visibility.Collapsed;
            //    }
            //}
            //else
            //{
            //    detailPo.SynsStatusS = Visibility.Collapsed;
            //    detailPo.SynsStatusF = Visibility.Collapsed;
            //}
            detailPo.SynsStatusS = Visibility.Visible;
            detailPo.SynsStatusF = Visibility.Visible;
        }

        /// <summary>
        /// 带联系人文本
        /// </summary>
        /// <returns></returns>
        public static string GetWithContactsText(string oldTxt, CopyDetailPo order, DetailTrader detailTrader)
        {
            if (string.IsNullOrWhiteSpace(order.BridgeType))
            {
                return oldTxt;
            }
            string resultTxt = oldTxt;
            if (detailTrader != null)
            {
                if (!string.IsNullOrWhiteSpace(detailTrader.TraderID))
                {
                    if (order.IsGVN)
                    {
                        if (detailTrader.TraderID == order.bankagentIdA)
                        {
                            //当前交易员是of方
                            if (order.bridgeOrder != null && (order.bridgeOrder.bridgeType == 2 || order.bridgeOrder.bridgeType == 0))
                            {
                                if (order.bridgebankagentid.Equals(order.bridgeOrder.instTradeId)
                                       && order.bridgebankid.Equals(order.bridgeOrder.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.contactType;
                                    }
                                }
                            }
                            else if (order.bridgeOrder2 != null && order.bridgeOrder2.bridgeType == 2)
                            {
                                if (order.bridgebankagentid2.Equals(order.bridgeOrder2.instTradeId)
                                       && order.bridgebankid2.Equals(order.bridgeOrder2.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.contactType;
                                    }
                                }
                            }
                        }
                        else if (detailTrader.TraderID == order.bankAgentIdB)
                        {
                            //当前交易员是bid方
                            if (order.bridgeOrder != null && (order.bridgeOrder.bridgeType == 1 || order.bridgeOrder.bridgeType == 0))
                            {
                                if (order.bridgebankagentid.Equals(order.bridgeOrder.instTradeId)
                                       && order.bridgebankid.Equals(order.bridgeOrder.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.contactType;
                                    }
                                }
                            }
                            else if (order.bridgeOrder2 != null && order.bridgeOrder2.bridgeType == 1)
                            {
                                if (order.bridgebankagentid2.Equals(order.bridgeOrder2.instTradeId)
                                       && order.bridgebankid2.Equals(order.bridgeOrder2.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.contactType;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (detailTrader.TraderID == order.bankagentIdA)
                        {
                            if (order.bridgeOrder != null && (order.bridgeOrder.bridgeType == 1 || order.bridgeOrder.bridgeType == 0))
                            {
                                if (order.bridgebankagentid.Equals(order.bridgeOrder.instTradeId)
                                       && order.bridgebankid.Equals(order.bridgeOrder.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.contactType;
                                    }
                                }
                            }
                            else if (order.bridgeOrder2 != null && order.bridgeOrder2.bridgeType == 1)
                            {
                                if (order.bridgebankagentid2.Equals(order.bridgeOrder2.instTradeId)
                                      && order.bridgebankid2.Equals(order.bridgeOrder2.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.contactType;
                                    }
                                }

                            }
                        }
                        else if (detailTrader.TraderID == order.bankAgentIdB)
                        {
                            //当前交易员是ofr方
                            if (order.bridgeOrder != null && (order.bridgeOrder.bridgeType == 2 || order.bridgeOrder.bridgeType == 0))
                            {
                                if (order.bridgebankagentid.Equals(order.bridgeOrder.instTradeId)
                                       && order.bridgebankid.Equals(order.bridgeOrder.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder.contactType;
                                    }
                                }
                            }
                            else if (order.bridgeOrder2 != null && order.bridgeOrder2.bridgeType == 2)
                            {
                                if (order.bridgebankagentid2.Equals(order.bridgeOrder2.instTradeId)
                                       && order.bridgebankid2.Equals(order.bridgeOrder2.instId))
                                {
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.instTradeName))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.instTradeName;
                                    }
                                    if (!string.IsNullOrWhiteSpace(order.bridgeOrder2.contactType))
                                    {
                                        resultTxt += "  " + order.bridgeOrder2.contactType;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            return resultTxt;
        }

        /// <summary>
        /// 获取订单中属于被合并的机构的brokerid
        /// </summary>
        /// <param name="CopyDetailPo"></param>
        /// <param name="combineInst"></param>
        /// <returns></returns>
        public static string GetProxyBrokerId(CopyDetailPo CopyDetailPo, string combineInst)
        {
            var proxyId = string.Empty;
            var instA = string.IsNullOrEmpty(CopyDetailPo.bankcodeshorta) ? CopyDetailPo.bankCodeA : CopyDetailPo.bankcodeshorta;
            var instB = string.IsNullOrEmpty(CopyDetailPo.bankcodeshortb) ? CopyDetailPo.bankCodeB : CopyDetailPo.bankcodeshortb;
            if (combineInst == instA && CurrentUserBrokerId.Contains(CopyDetailPo.brokerIdA))
            {
                proxyId = CopyDetailPo.brokerIdA;
            }
            else if (combineInst == instB && CurrentUserBrokerId.Contains(CopyDetailPo.brokerIdB))
            {
                proxyId = CopyDetailPo.brokerIdB;
            }

            return proxyId;
        }


        /// <summary>
        /// 获取被代理的broker的ID
        /// </summary>
        /// <returns></returns>
        public static string GetProxyBrokerIdByInstTraderName(CopyDetailPo CopyDetailPo, string instTraderName)
        {
            var proxyNameA = GetKey(CopyDetailPo, 0);
            var proxyNameB = GetKey(CopyDetailPo, 1);
            var proxyId = string.Empty;

            if (instTraderName.Contains(NoInstOrder))
            {
                proxyId = GetNoInstBrokerId(CopyDetailPo);
            }
            else if (instTraderName == proxyNameA && CurrentUserBrokerId.Contains(CopyDetailPo.brokerIdA))
            {
                proxyId = CopyDetailPo.brokerIdA;
            }
            else if (instTraderName == proxyNameB && CurrentUserBrokerId.Contains(CopyDetailPo.brokerIdB))
            {
                proxyId = CopyDetailPo.brokerIdB;
            }
            return proxyId;
        }
        /// <summary>
        /// 获取被代理的无机构brokerid
        /// </summary>
        /// <returns></returns>
        public static string GetNoInstBrokerId(CopyDetailPo order)
        {
            string brokerId = string.Empty;
            if (string.IsNullOrEmpty(order.bankCodeA) && !string.IsNullOrEmpty(order.bankCodeB) || LocalEnv.UserInfo.IsMyself(order.brokerIdA))
            {
                brokerId = order.brokerIdA;
            }
            else if (!string.IsNullOrEmpty(order.bankCodeA) && string.IsNullOrEmpty(order.bankCodeB) || LocalEnv.UserInfo.IsMyself(order.brokerIdB))
            {
                brokerId = order.brokerIdB;
            }
            else
            {
                brokerId = CurrentUserBrokerId.Contains(order.brokerIdA) ? order.brokerIdA : order.brokerIdB;
            }
            return brokerId;
        }

        /// <summary>
        /// 生成空格行
        /// </summary>
        /// <returns></returns>
        public static PositionDetailPo GenerateEmpty()
        {
            PositionDetailPo po = new PositionDetailPo();
            po.DataType = EDataType.空格;
            po.SumIsVisible = "Hidden";
            po.SendIsVisible = "Hidden";
            po.Isliq = string.Format("Isliq--0");
            po.IsWidth = string.Format("Width--0");
            po.IsHeader = string.Format("Header--0");
            po.IsMargin = string.Format("Margin--0");
            po.Padding = string.Format("Padding--0");
            po.FontFamily = string.Format("Family--0");
            po.FontSize = string.Format("Size--0");
            po.IsShowColor = string.Format("IsShow--1");
            po.Isbrokerunread = string.Format("Isbroker--1");
            po.Text = "";
            po.TotalText = "";
            return po;
        }
        /// <summary>
        /// 生成其他人成交单头
        /// </summary>
        /// <param name="key"></param>
        /// <param name="textBlockModel"></param>
        /// <param name="textBlockModel2"></param>
        /// <returns></returns>
        public static PositionDetailPo GenerateOtherOrderHeader(string key, TextBlockModel textBlockModel, TextBlockModel textBlockModel2)
        {
            PositionDetailPo dpOther = new PositionDetailPo();
            dpOther.DataType = EDataType.其他人成交单头;
            dpOther.Seq = 0;
            textBlockModel.Text = string.IsNullOrEmpty(dpOther.Text) ? string.Empty : dpOther.Text;
            dpOther.Text = toFormat(textBlockModel, key, true);
            dpOther.Key = key;
            dpOther.Text2 = toFormat2(textBlockModel, textBlockModel2, key, true);
            dpOther.TotalText = toFormatCopy(textBlockModel, key, true);
            dpOther.CopyText = toFormatCopy(textBlockModel, key, true);
            dpOther.WithoutBrokerText = toFormatCopy(textBlockModel, key, true);
            dpOther.Padding = string.Format("Padding--0");
            dpOther.FontFamily = string.Format("Family--0");
            dpOther.FontSize = string.Format("Size--0");
            dpOther.SumIsVisible = "Hidden";
            dpOther.SendIsVisible = "Hidden";
            dpOther.IsHeader = string.Format("Header--0");
            dpOther.IsMargin = string.Format("Margin--0");

            dpOther.IsWidth = string.Format("Width--0");
            dpOther.IsShowColor = string.Format("IsShow--1");
            dpOther.Isbrokerunread = string.Format("Isbroker--1");
            dpOther.Isliq = string.Format("Isliq--0");
            return dpOther;
        }

        /// <summary>
        /// 无机构成交单，brokera 或者 brokerb 属于被代理的broker
        /// </summary>
        /// <param name="o"></param>
        /// <param name="proxyBrokerIds"></param>
        /// <returns></returns>
        public static bool IsNoInstOrder(CopyDetailPo o, HashSet<string> proxyBrokerIds)
            => string.IsNullOrWhiteSpace(o.bankCodeA) && o.brokerIdA != null && proxyBrokerIds.Contains(o.brokerIdA)
                || string.IsNullOrWhiteSpace(o.bankCodeB) && o.brokerIdB != null && proxyBrokerIds.Contains(o.brokerIdB);

        /// <summary>
        /// 根据broker方向判断机构是否为空   //broker=0:bankCodeA  broker=1:bankCodeB broker=2:今日无机构成交单
        /// 0：A方向机构非空
        /// 1：B方向机构非空
        /// 2：A或者B的机构为空
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool isContain(int broker, CopyDetailPo o, string proxyBrokerId = null)
        {
            var brokerId = string.IsNullOrEmpty(proxyBrokerId) ? LocalEnv.UserInfo.UserId : proxyBrokerId;//string.IsNullOrEmpty(proxyBrokerId) ? LocalEnv.UserInfo.FullName : proxyBrokerId;//
            bool isContain = false;
            switch (broker)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(o.bankCodeA))
                    {
                        if (o.brokerIdA != null && o.brokerIdA.Equals(brokerId))
                        {
                            isContain = true;
                        }
                    }
                    break;
                case 1:
                    if (!string.IsNullOrWhiteSpace(o.bankCodeB))
                    {
                        if (o.brokerIdB != null && o.brokerIdB.Equals(brokerId))
                        {
                            isContain = true;
                        }
                    }
                    break;
                case 2:
                    if (string.IsNullOrWhiteSpace(o.bankCodeA))
                    {
                        if (o.brokerIdA != null && o.brokerIdA.Equals(brokerId))
                        {
                            isContain = true;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(o.bankCodeB))
                    {
                        if (o.brokerIdB != null && o.brokerIdB.Equals(brokerId))
                            isContain = true;
                    }
                    //}
                    break;
            }
            return isContain;
        }

        /// <summary>
        /// 按照机构交易员拆分其他人成交单
        /// </summary>
        /// <param name="otherOrders"></param>
        public static void SplitOtherOrders(List<CopyDetailPo> otherOrders, SearchPo condition, Dictionary<DetailTrader, List<CopyDetailPo>> dic)
        {
            foreach (var order in otherOrders)
            {

                if (order.seq == "1370")
                {
                    int xx = 0;
                }
                // 如果当前broker是成交单的A方               
                if (IsBelongBroker(0, order))
                {
                    string key = GetKey(order, 0);
                    DetailTrader detailTrader = GetTrader(order, 0);

                    //判断机构是否合并展示
                    if (!string.IsNullOrWhiteSpace(order.bankcodeshorta))
                    {
                        if (dicSet.ContainsKey(order.bankcodeshorta.Trim()))
                        {
                            key = order.bankcodeshorta;
                            detailTrader.Key = order.bankcodeshorta;
                        }

                    }
                    else
                    {
                        if (dicSet.ContainsKey(order.bankCodeA.Trim()))
                        {
                            key = order.bankCodeA;
                            detailTrader.Key = order.bankCodeA;
                        }
                    }

                    key = $"{key} ";

                    //将成交单加入字典
                    if (!dic.Keys.Any(_ => _.Key == key && _.IsOtherOrder))
                    {
                        // 根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                detailTrader.Key = key;
                                detailTrader.IsOtherOrder = true;
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dic.Add(detailTrader, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                detailTrader.Key = key;
                                detailTrader.IsOtherOrder = true;
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dic.Add(detailTrader, li);
                            }
                        }
                    }
                    else
                    {
                        //根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader trader = dic.Keys.FirstOrDefault(_ => _.Key == key && _.IsOtherOrder);
                                List<CopyDetailPo> keylist = dic[trader];
                                if (!keylist.Contains(order))
                                {
                                    dic[trader].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate2 : order.dealdate;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                DetailTrader trader = dic.Keys.FirstOrDefault(_ => _.Key == key && _.IsOtherOrder);
                                List<CopyDetailPo> keylist = dic[trader];
                                if (!keylist.Contains(order))
                                {
                                    dic[trader].Add(order);
                                }
                            }
                        }
                    }
                }

                // 如果当前broker是成交单的B方             
                if (IsBelongBroker(1, order))
                {
                    String keyb = GetKey(order, 1);
                    DetailTrader detailTraderB = GetTrader(order, 1);

                    if (!string.IsNullOrWhiteSpace(order.bankcodeshortb))
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankcodeshortb))
                        {
                            keyb = order.bankcodeshortb;
                            detailTraderB.Key = order.bankcodeshortb;
                        }
                    }
                    else
                    {
                        if (ConvertTextHelper.dicSet.ContainsKey(order.bankCodeB))
                        {
                            keyb = order.bankCodeB;
                            detailTraderB.Key = order.bankCodeB;
                        }
                    }

                    keyb = $"{keyb} ";

                    if (!dic.Keys.Any(_ => _.Key == keyb && _.IsOtherOrder))
                    {
                        // 根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                detailTraderB.IsOtherOrder = true;
                                detailTraderB.Key = keyb;
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dic.Add(detailTraderB, li);
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                detailTraderB.IsOtherOrder = true;
                                detailTraderB.Key = keyb;
                                List<CopyDetailPo> li = new List<CopyDetailPo>();
                                li.Add(order);
                                dic.Add(detailTraderB, li);
                            }
                        }
                    }
                    else
                    {
                        ///根据机构交易员对应方向去过滤成交日期。
                        if (!string.IsNullOrEmpty(condition.TradeDate))
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            if (condition.TradeDate == date)
                            {
                                DetailTrader traderB = dic.Keys.FirstOrDefault(_ => _.Key == keyb && _.IsOtherOrder);
                                List<CopyDetailPo> keylist = dic[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dic[traderB].Add(order);
                                }
                            }
                        }
                        else
                        {
                            string date = order.IsGVN ? order.dealdate : order.dealdate2;
                            string Today = DateTime.Now.Date.ToString("yyyyMMdd");
                            if (date.CompareTo(Today) >= 0)
                            {
                                DetailTrader traderB = dic.Keys.FirstOrDefault(_ => _.Key == keyb && _.IsOtherOrder);
                                List<CopyDetailPo> keylist = dic[traderB];
                                if (!keylist.Contains(order))
                                {
                                    dic[traderB].Add(order);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断当前用户是属于哪一方的broker 的人
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsBelongBroker(int brokerSide, CopyDetailPo o)
        {
            bool isContain = false;
            switch (brokerSide)
            {
                case 0:
                    {
                        if (!string.IsNullOrWhiteSpace(o.bankCodeA))
                        {
                            if (o.brokerIdA != null && CurrentUserBrokerId.Contains(o.brokerIdA) && LocalEnv.UserInfo.UserId != o.brokerIdA)
                            {
                                isContain = true;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (!string.IsNullOrWhiteSpace(o.bankCodeB))
                        {
                            if (o.brokerIdB != null && CurrentUserBrokerId.Contains(o.brokerIdB) && LocalEnv.UserInfo.UserId != o.brokerIdB)
                            {
                                isContain = true;
                            }
                        }
                        break;
                    }
            }
            return isContain;
        }


        /// <summary>
        /// 前端过滤明细，卷码，机构，价格，日期
        /// </summary>
        /// <param name="templist">过滤原</param>
        /// <param name="condition">过滤条件</param>
        /// <returns></returns>
        public static List<CopyDetailPo> Search(List<CopyDetailPo> templist, SearchPo condition)
        {
            List<CopyDetailPo> list = new List<CopyDetailPo>();
            if (condition.IsOfr || condition.IsBid)
            {
                if (condition.IsOfr && condition.IsBid)
                {
                    string ofrcode = string.Format("{0}.IB", condition.Bid.BondCode);
                    var a = from m in templist
                            where ((m.OfrInst + "" == condition.Ofr.Insta + "")
                            && (m.OfrbankAgentName == condition.Ofr.bankAgentName)
                            && m.goodsCode == ofrcode && m.price == condition.Ofr.Price && m.dealdate2 == condition.Ofr.Dealdate
                            && m.settlementdate2 == condition.Ofr.Settlementdate)
                            || ((m.BidInst + "" == condition.Bid.Instb + "")
                            && (m.BidbankAgentName == condition.Bid.bankAgentName)
                            && m.price == condition.Bid.Price && m.dealdate == condition.Bid.Dealdate
                            && m.settlementdate == condition.Bid.Settlementdate)
                            select m;
                    list.AddRange(a.ToList());
                }
                else if (condition.IsOfr)
                {
                    string ofrcode = string.Format("{0}.IB", condition.Ofr.BondCode);
                    var a = from m in templist
                            where (m.OfrInst + "" == condition.Ofr.Insta + "")
                            && (m.OfrbankAgentName == condition.Ofr.bankAgentName)
                            && m.goodsCode == ofrcode && m.price == condition.Ofr.Price && m.dealdate2 == condition.Ofr.Dealdate
                            && m.settlementdate2 == condition.Ofr.Settlementdate
                            select m;
                    list.AddRange(a.ToList());
                }
                else if (condition.IsBid)
                {
                    string bidcode = string.Format("{0}.IB", condition.Bid.BondCode);
                    var a = from m in templist
                            where (m.BidInst + "" == condition.Bid.Instb + "")
                            && (m.BidbankAgentName == condition.Bid.bankAgentName)
                            && m.goodsCode == bidcode && m.price == condition.Bid.Price && m.dealdate == condition.Bid.Dealdate
                            && m.settlementdate == condition.Bid.Settlementdate
                            select m;
                    list.AddRange(a.ToList());
                }
                //if (list != null && list.Count == 0)
                //{
                var b = from m in templist
                        where m.execId == condition.ExcId
                        select m;
                List<CopyDetailPo> list2 = new List<CopyDetailPo>();
                list.AddRange(b);
                list2.AddRange(list.Distinct());
                List<CopyDetailPo> list3 = list2.OrderBy(o => o.TransactionTime).ThenBy(o => o.SeqSort).ToList();//.OrderBy(o => o.seq);
                list3 = FilterByOrderBroker(list3, condition.brokerNames);
                return list3;
            }
            else
            {
                list = templist.Where(a =>
       string.IsNullOrWhiteSpace(condition.BondCode) ? true : condition.BondCode.Equals(a.goodsCode.Replace(".IB", ""))
            ).ToList();
                list = list.Where(a =>
        string.IsNullOrWhiteSpace(condition.Inst) ? true : condition.Inst.Equals(a.bankCodeA) || condition.Inst.Equals(a.bankcodeshorta) || condition.Inst.Equals(a.bankCodeB) || condition.Inst.Equals(a.bankcodeshortb)
        || condition.InstID.Equals(a.bankIdA) || condition.InstID.Equals(a.bankIdB)
             ).ToList();
                list = list.Where(a =>
                string.IsNullOrWhiteSpace(condition.TraderInst) ? true : condition.TraderInst.Equals(a.bankCodeA) || condition.TraderInst.Equals(a.bankcodeshorta) || condition.TraderInst.Equals(a.bankCodeB) || condition.TraderInst.Equals(a.bankcodeshortb)
                ).ToList();
                list = list.Where(a =>
           string.IsNullOrWhiteSpace(condition.Trader) ? true : condition.Trader.Equals(a.bankAgentNameA) || condition.Trader.Equals(a.bankAgentNameB)
             ).ToList();
                list = list.Where(a =>
        string.IsNullOrWhiteSpace(condition.Priceformat) ? true : condition.Price.Equals(a.price)
                ).ToList();
                list = list.Where(a =>
       string.IsNullOrWhiteSpace(condition.TradeDate) ? true : condition.TradeDate.Equals(a.dealdate) || condition.TradeDate.Equals(a.dealdate2)
            ).ToList();
                list = list.Where(a =>
       string.IsNullOrWhiteSpace(condition.SeachBridgeFlag) ? true : a.BridgeFlag + "" == "2" && string.IsNullOrWhiteSpace(a.BridgeType) //缺桥查询
            ).ToList();
                list = list.Where(a =>
       string.IsNullOrWhiteSpace(condition.SeachDelFlag) ? true : a.orderStatus + "" != "D"  //过滤删除的单子
            ).ToList();
                list = list.Where(a =>
       condition.Seq == decimal.Zero ? true : a.seq == condition.Seq + "" //单号查询
            ).ToList();
                list = filterOrdersByBondInfos(list, condition);
                list = FilterByOrderType(list, condition.OrderTypes);
                list = FilterByOrderBroker(list, condition.brokerNames);
                if (condition.IsSort)
                {
                    list = (from m in list
                            orderby m.goodsCode ascending, m.price ascending, m.dealdate2 ascending, m.settlementdate2 ascending, m.OfrInst ascending, m.OfrbankAgentName ascending, m.BidInst ascending, m.BidbankAgentName ascending, m.seq ascending
                            select m).ToList();
                }
                else
                {
                    list = list.OrderBy(o => o.TransactionTime).ThenBy(o => o.SeqSort).ToList();
                }
                if (condition.IsMaturityOrder)
                {
                    list = HandleMaturityOrder(list);
                    list = list.OrderBy(o => o.TransactionTime).ThenBy(o => o.SeqSort).ToList();
                }
                return list;
            }
        }

        private static List<CopyDetailPo> HandleMaturityOrder(List<CopyDetailPo> orders)
        {
            if (orders == null || orders.Count <= 0)
                return new List<CopyDetailPo>();
            foreach (var item in orders)
            {
                item.maturityDays = GetMaturityDays(item.timeToMaturity);
            }
            return orders;
        }

        /// <summary>
        /// 剩余期限转成天数
        /// </summary>
        /// <param name="maturity">
        /// 剩余期限字符串，可能的格式有：
        /// xxY，xxD，xxY + xxD，xxY + N，xxD + N
        /// </param>
        /// <param name="maturityEndDate">债券到期日</param>
        /// <returns></returns>
        public static int GetMaturityDays(string maturity, string maturityEndDate = "")
        {
            int days = 0;
            if (string.IsNullOrWhiteSpace(maturity)) return days;
            bool leapYear = false;
            if (!string.IsNullOrWhiteSpace(maturityEndDate))
            {
                DateTime dateTime;
                DateTime.TryParse(maturityEndDate, out dateTime);
                leapYear = DateTime.IsLeapYear(dateTime.Year);
            }
            double yearDays = leapYear ? 366.0 : 365.0;
            var maturityArray = maturity.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ma in maturityArray)
            {
                if (ma.Trim().EndsWith("N")) continue;
                if (ma.Trim().EndsWith("Y"))
                {
                    double year;
                    double.TryParse(ma.Trim().Replace("Y", string.Empty), out year);
                    days += (int)Math.Ceiling(year * yearDays);
                }
                else if (ma.Trim().EndsWith("D"))
                {
                    double day;
                    double.TryParse(ma.Trim().Replace("D", string.Empty), out day);
                    days += (int)day;
                }
            }

            return days;
        }

        /// <summary>
        /// 根据订单类型过滤
        /// </summary>
        /// <param name="orders">订单类型，意向单、二级市场成交单</param>
        /// <param name="orderTypes">筛选种类</param>
        /// <returns></returns>
        private static List<CopyDetailPo> FilterByOrderType(List<CopyDetailPo> orders, List<string> orderTypes)
        {
            var result = new List<CopyDetailPo>();
            if (orderTypes.Count == 0)
            {
                result.AddRange(orders);
            }
            else
            {
                foreach (var item in orders)
                {
                    string flag;

                    // 升级前没有标志的订单，默认为二级订单
                    if (string.IsNullOrEmpty(item.flagInner) && orderTypes.Contains("二级"))
                    {
                        result.Add(item);
                    }
                    else if (!string.IsNullOrEmpty(item.flagInner) && orderTypes.Contains(item.flagInner))
                    //(!string.IsNullOrEmpty(item.flagInner) && SearchPo.OrderTypeDic.TryGetValue(item.flagInner, out flag) && orderTypes.Contains(flag))
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 根据broker过滤
        /// </summary>
        /// <param name="orders">订单列表</param>
        /// <param name="orderBrokers">筛选种类</param>
        /// <returns></returns>
        private static List<CopyDetailPo> FilterByOrderBroker(List<CopyDetailPo> orders, List<string> orderBrokers)
        {
            List<CopyDetailPo> result = new List<CopyDetailPo>();
            if (orderBrokers.Count == 0)
            {
                return orders;
            }
            else
            {
                if (orders != null && orders.Count > 0)
                {
                    return orders.Where(_ => orderBrokers.Contains(_.brokerNameA) || orderBrokers.Contains(_.brokerNameB)).ToList();
                }
            }
            return result;
        }


        /// <summary>
        /// 根据选择的债券类型和交易场所进行过滤
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static List<CopyDetailPo> filterOrdersByBondInfos(List<CopyDetailPo> orders, SearchPo condition)
        {
            if ((condition.TradingPlaces == null || condition.TradingPlaces.Count == 0) && (condition.BondTypes == null || condition.BondTypes.Count == 0))
            {
                return orders;
            }
            List<CopyDetailPo> filterOrders = new List<CopyDetailPo>();
            if (condition.TradingPlaces != null && condition.TradingPlaces.Count > 0)
            {
                List<string> ListedMarkets = TradingPlacesStrTransfer(condition.TradingPlaces);
                orders = orders.Where(a => ListedMarkets.Any(e => e.Equals(a.ListedMarket + ""))).ToList();
            }
            if (condition.BondTypes != null && condition.BondTypes.Count > 0)
            {

                string typeStr = "国债、地方债、短融/超短融、中票、次级债";
                List<string> typeList = new List<string>(typeStr.Split(new char[] { '、' }));
                if (condition.BondTypes.Any(a => typeList.Contains(a)))
                {
                    List<string> typeListSelect = BondTypesStrTransfer(condition.BondTypes);
                    filterOrders.AddRange(orders.Where(a => typeListSelect.Any(e => e.Equals(a.BondSubtype))).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("国开")))
                {
                    filterOrders.AddRange(orders.Where(a => gkJudgment(a)).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("进出口")))
                {
                    filterOrders.AddRange(orders.Where(a => jckJudgment(a)).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("农发")))
                {
                    filterOrders.AddRange(orders.Where(a => nfJudgment(a)).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("企业/公司债")))
                {
                    filterOrders.AddRange(orders.Where(a => EntCopJudgment(a)).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("PPN")))
                {
                    filterOrders.AddRange(orders.Where(a => ppnJudgment(a)).ToList());
                }
                if (condition.BondTypes.Any(a => a.Equals("其他")))
                {
                    filterOrders.AddRange(orders.Where(a => otherJudgment(a)).ToList());
                }
                return filterOrders;
            }
            return orders;
        } /// <summary>
          /// 国开	PBB，PSB	Issuer_Code = G000124
          /// </summary>
          /// <param name="order"></param>
          /// <returns></returns>
        static bool gkJudgment(CopyDetailPo order)
        {
            bool gk = false;
            if (order != null)
            {
                if ((order.BondSubtype + "" == "PBB" || order.BondSubtype + "" == "PSB") && order.IssuerCode + "" == "G000124")
                    gk = true;
            }
            return gk;
        } /// <summary>
          /// 进出口  PBB，PSB    Issuer_Code = Z000189
          /// </summary>
          /// <param name="order"></param>
          /// <returns></returns>
        static bool jckJudgment(CopyDetailPo order)
        {
            bool jck = false;
            if (order != null)
            {
                if ((order.BondSubtype + "" == "PBB" || order.BondSubtype + "" == "PSB") && order.IssuerCode + "" == "Z000189")
                    jck = true;
            }
            return jck;
        }
        /// <summary>
        /// 农发    PBB，PSB    Issuer_Code = Z000207
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        static bool nfJudgment(CopyDetailPo order)
        {
            bool nf = false;
            if (order != null)
            {
                if ((order.BondSubtype + "" == "PBB" || order.BondSubtype + "" == "PSB") && order.IssuerCode + "" == "Z000207")
                    nf = true;
            }
            return nf;
        }
        /// <summary>
        /// 债券简称中带有PPN字样
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        static bool ppnJudgment(CopyDetailPo order)
        {
            bool ppn = false;
            if (order != null)
            {
                if (order.goodsShortName.ToUpper().Contains("PPN"))
                    ppn = true;
            }
            return ppn;
        }
        /// <summary>
        /// 企业／公司债	
        /// 企业债：CCB，COB，RAB以及当CEB和LEB且Ent_Cor=ENT
        /// 公司债：SCV，CVB 以及当CEB和LEB且Ent_Cor=COP
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        static bool EntCopJudgment(CopyDetailPo order)
        {
            bool ENT = false;
            bool COP = false;
            if (order != null)
            {
                bool flag = (order.BondSubtype + "" == "CEB" || order.BondSubtype + "" == "LEB") && order.EntCor == "ENT";
                if (order.BondSubtype + "" == "CCB" || order.BondSubtype + "" == "COB" || order.BondSubtype + "" == "RAB" || flag)
                    ENT = true;
                flag = (order.BondSubtype + "" == "CEB" || order.BondSubtype + "" == "LEB") && order.EntCor == "COP";
                if (order.BondSubtype + "" == "SCV" || order.BondSubtype + "" == "CVB" || flag)
                    COP = true;
            }
            return ENT || COP;
        }
        /// <summary>
        /// 非上述筛选条件内的债券
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        static bool otherJudgment(CopyDetailPo order)
        {
            return !(gkJudgment(order) || jckJudgment(order) || nfJudgment(order) || ppnJudgment(order) || typeJudgment(order) || EntCopJudgment(order));

        }
        /// <summary>
        /// 可以直接根据类型判断的债券
        /// </summary>
        /// <returns></returns>
        static bool typeJudgment(CopyDetailPo order)
        {
            List<string> typeList = SearchPo.国债List.Union(SearchPo.地方债List).Union(SearchPo.短融超短融List).Union(SearchPo.中票List).Union(SearchPo.次级债List).ToList();
            bool type = false;
            if (order != null)
            {
                if (typeList.Contains(order.BondSubtype + ""))
                    type = true;
            }
            return type;
        }

        /// <summary>
        /// 获取可直接判断的债券类型
        /// </summary>
        /// <param name="BondTypes"></param>
        /// <returns></returns>
        static List<string> BondTypesStrTransfer(List<string> BondTypes)
        {
            List<string> BondTypesStr = new List<string>();
            foreach (var bond in BondTypes)
            {
                if (bond == "国债")
                {
                    BondTypesStr.AddRange(SearchPo.国债List);
                }
                else if (bond == "地方债")
                {
                    BondTypesStr.AddRange(SearchPo.地方债List);
                }
                else if (bond == "短融/超短融")
                {
                    BondTypesStr.AddRange(SearchPo.短融超短融List);
                }
                else if (bond == "中票")
                {
                    BondTypesStr.AddRange(SearchPo.中票List);
                }
                else if (bond == "次级债")
                {
                    BondTypesStr.AddRange(SearchPo.次级债List);
                }
            }
            return BondTypesStr;
        }
        /// <summary>
        /// 交易场所转换
        /// </summary>
        /// <param name="TradingPlaces"></param>
        /// <returns></returns>
        static List<string> TradingPlacesStrTransfer(List<string> TradingPlaces)
        {
            List<string> TradingPlacesStr = new List<string>();
            foreach (var bond in TradingPlaces)
            {
                if (bond == "银行间")
                    TradingPlacesStr.Add("CIB");
                else if (bond == "上交所")
                    TradingPlacesStr.Add("SSE");
                else if (bond == "深交所")
                    TradingPlacesStr.Add("SZE");
            }
            return TradingPlacesStr;
        }


        public static string toFormatCopy(TextBlockModel r, string inst, bool isHist)
        {
            String format = "";
            //string tip = isHist ? "-" : "=";
            //for (int i = 1; i <= 22; i++)
            //{
            //    format += tip;//"=";
            //}
            format += inst;
            //while (GetTextDisplayWidth(r, format) < 500)//504
            //{
            //    format += tip;//"=";
            //}
            format += "\r";
            return format;
        }


        public static string toFormat2(TextBlockModel r, TextBlockModel r2, String inst, bool isHist)
        {
            string format = " ";
            string tip = isHist ? "-" : "=";
            if (inst.Equals(ElseOrder) || inst.Equals(OwnNoinstOrder) || inst.Equals(ElsehistOrder) || inst.Equals(OwnNoinsthistOrder))
            {
                while (GetTextDisplayWidth(r, r.Text) + GetTextDisplayWidth(r2, format) < 490)//504
                {
                    format += tip;//"=";
                }
            }
            else
            {
                int length = isHist ? 514 : 455;
                while (GetTextDisplayWidth(r, r.Text) + GetTextDisplayWidth(r2, format) < length)//504
                {
                    format += tip;//"=";
                }
            }
            format += "\r";
            return format;
        }


        public static string toFormat(TextBlockModel textBlockModel, String inst, bool isHist)
        {
            string format = "";
            string tip = isHist ? "-" : "=";
            //while (GetTextDisplayWidth(textBlockModel, textBlockModel.Text) < 180)//504
            //{
            //    format += tip;//"=";
            //    textBlockModel.Text = format;
            //}
            format += inst;
            textBlockModel.Text = format;
            return format;
        }
        public static Double GetTextDisplayWidth(TextBlockModel label, String text)
        {
            return GetTextDisplayWidths(text, label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch, label.FontSize);
        }
        public static Double GetTextDisplayWidths(string str, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double FontSize)
        {
            var formattedText = new FormattedText(
                      str,
                      CultureInfo.CurrentUICulture,
                      FlowDirection.LeftToRight,
                      new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                      FontSize,
                      Brushes.Black
                      );
            Size size = new Size(formattedText.Width, formattedText.Height);
            return size.Width;
        }


        public static T D2E<T>(DataRow r)
        {
            T t = default(T);
            t = Activator.CreateInstance<T>();
            PropertyInfo[] ps = t.GetType().GetProperties();
            foreach (var item in ps)
            {
                if (r.Table.Columns.Contains(item.Name) && !"HtmlText".Equals(item.Name))//
                {
                    object v = r[item.Name];
                    if (v.GetType() == typeof(System.DBNull))
                        v = null;
                    item.SetValue(t, v, null);
                }
            }
            return t;
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
        /// 单桥历史单结算汇总显示
        /// 历史但的计算类型，当天显示的时候要么是“+0”，“+1” ，要么是“日期+0”或者“日期+1”的方式
        /// </summary>
        /// <param name="CopyDetailPo"></param>
        /// <returns></returns>
        public static string GetHistoryDisplaySettle(CopyDetailPo CopyDetailPo)
        {
            string result = string.Empty;
            DateTime dtDeal;
            ///非默认结算类型，且O,B结算类型一致
            if (!string.IsNullOrEmpty(CopyDetailPo.SettleDisplay2) && CopyDetailPo.SettleDisplay.Equals(CopyDetailPo.SettleDisplay2))
            {
                if (!string.IsNullOrEmpty(CopyDetailPo.remarks))
                {
                    dtDeal = GetDate(CopyDetailPo.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + CopyDetailPo.remarks;
                }
                else
                {
                    dtDeal = GetDate(CopyDetailPo.dealdate);
                    DateTime dtSettle = GetDate(CopyDetailPo.settlementdate);
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
                            result = GetDefautSettle(CopyDetailPo.settlementtype2);
                        }
                        else
                        {
                            //result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            //                         + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + "+1";
                            result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                                      + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + GetDefautSettle(CopyDetailPo.settlementtype2);
                        }

                    }
                }
            }
            //错期
            else
            {
                string bidResult = string.Empty;
                string ofrResult = string.Empty;
                if (!string.IsNullOrEmpty(CopyDetailPo.settlementtype2) && !string.IsNullOrEmpty(CopyDetailPo.settlementType))
                {
                    //ofr
                    ofrResult = GetHistorySettle(CopyDetailPo, true);
                    //bid
                    bidResult = GetHistorySettle(CopyDetailPo, false);

                    if (ofrResult.Equals(bidResult))
                        result = ofrResult;
                    else
                        result = string.Format("{0}/{1}", ofrResult, bidResult);
                }
                else if (!string.IsNullOrEmpty(CopyDetailPo.settlementType) && string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                {
                    result = GetHistorySettle(CopyDetailPo, false);
                }
                else if (string.IsNullOrEmpty(CopyDetailPo.settlementType) && !string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                {
                    //ofr
                    result = GetHistorySettle(CopyDetailPo, true);
                }
            }

            return result;
        }

        /// <summary>
        /// 历史单结类型 错期使用,新版本，考虑到节假日的情况，解决 日期+0 会显示成 日期+1
        /// </summary>
        /// <param name="CopyDetailPo"></param>
        /// <param name="isOfr"></param>
        /// <returns></returns>
        public static string GetHistorySettle(CopyDetailPo CopyDetailPo, bool isOfr)
        {
            string settleTime = isOfr ? CopyDetailPo.settlementdate2 : CopyDetailPo.settlementdate;
            string dealTime = isOfr ? CopyDetailPo.dealdate2 : CopyDetailPo.dealdate;
            string settle = isOfr ? CopyDetailPo.settlementtype2 : CopyDetailPo.settlementType;

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
        /// 结算类型，当天单子
        /// </summary>
        /// <param name="CopyDetailPo"></param>
        /// <returns></returns>
        public static string GetDisplaySettle(CopyDetailPo CopyDetailPo)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(CopyDetailPo.SettleDisplay2) && CopyDetailPo.SettleDisplay.Equals(CopyDetailPo.SettleDisplay2))
            {
                //备注
                if (!string.IsNullOrEmpty(CopyDetailPo.remarks))
                {
                    DateTime dtDeal = GetDate(CopyDetailPo.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + CopyDetailPo.remarks;
                }
                else
                    result = CopyDetailPo.SettleDisplay;
            }
            else
            //错期
            {
                if (!string.IsNullOrEmpty(CopyDetailPo.settlementtype2) && !string.IsNullOrEmpty(CopyDetailPo.settlementType))
                    result = string.Format("{0}/{1}", CopyDetailPo.SettleDisplay2, CopyDetailPo.SettleDisplay);
                else if (!string.IsNullOrEmpty(CopyDetailPo.settlementType) && string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                    result = CopyDetailPo.SettleDisplay;
                else if (string.IsNullOrEmpty(CopyDetailPo.settlementType) && !string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                    result = CopyDetailPo.SettleDisplay2;
            }

            return result;
        }
        public static string GetDisplaySettleForMain(CopyDetailPo CopyDetailPo)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(CopyDetailPo.SettleDisplayForMain2) && CopyDetailPo.SettleDisplayForMain.Equals(CopyDetailPo.SettleDisplayForMain2))
            {
                //备注
                if (!string.IsNullOrEmpty(CopyDetailPo.remarks))
                {
                    DateTime dtDeal = GetDate(CopyDetailPo.dealdate);
                    result = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString()) + CopyDetailPo.remarks;
                }
                else
                    result = CopyDetailPo.SettleDisplayForMain;
            }
            else
            //错期
            {
                if (!string.IsNullOrEmpty(CopyDetailPo.settlementtype2) && !string.IsNullOrEmpty(CopyDetailPo.settlementType))
                    result = string.Format("{0}/{1}", CopyDetailPo.SettleDisplayForMain2, CopyDetailPo.SettleDisplayForMain);
                else if (!string.IsNullOrEmpty(CopyDetailPo.settlementType) && string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                    result = CopyDetailPo.SettleDisplayForMain;
                else if (string.IsNullOrEmpty(CopyDetailPo.settlementType) && !string.IsNullOrEmpty(CopyDetailPo.settlementtype2))
                    result = CopyDetailPo.SettleDisplayForMain2;
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

        public static DateTime GetDate(string time)
        {
            DateTime result;
            DateTime.TryParseExact(time, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

            return result;
        }

        /// <summary>
        /// 获取显示的结算类型
        /// 成交日期不在显示明天或者周几，除了+0和+1以外，全部显示日期，展示为10.14+0
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="dealDate"></param>
        /// <returns></returns>
        public static string GetSettleShow(string settle, string dealDate)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(settle))
            {
                return result;
            }

            if (IsDefautSettle(settle))
                result = settle;
            else
            {
                if (string.IsNullOrEmpty(dealDate))
                {
                    result = settle;
                }
                else
                {
                    DateTime dtDeal = GetDate(dealDate);
                    string dealStr = (dtDeal.Month < 10 ? ("0" + dtDeal.Month) : dtDeal.Month.ToString()) + "."
                            + (dtDeal.Day < 10 ? ("0" + dtDeal.Day) : dtDeal.Day.ToString());
                    result = dealStr + GetDefautSettle(settle);
                }
            }
            return result;
        }
        /// <summary>
        /// 是否是默认结算
        /// </summary>
        /// <param name="settle"></param>
        /// <returns></returns>
        public static bool IsDefautSettle(string settle)
        {
            return settle + "" == "+0" || settle + "" == "+1";
        }
    }
}
