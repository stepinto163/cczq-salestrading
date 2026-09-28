using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IDC.Frame.Utils;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using STC.Broker.Plugin.Model.Transaction.History;
using STC.Common.Model.Fix.Message;
using STC.Broker.Plugin.Model.Transaction;
using HisRecordPo = STC.Broker.Plugin.Model.Transaction.History.HisRecordPo;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.Model;
using STC.Common.Model.PT;

namespace STC.Broker.Plugin.View.History
{
    /// <summary>
    /// HisReocrdWin.xaml 的交互逻辑
    /// </summary>
    public partial class HisReocrdWin : Window
    {
        AutoResetEvent are = new AutoResetEvent(false);
        public List<HisRecordPo> HisData;
        //新的加载方式 逐条加载 20200430
        private ObservableCollection<HisRecordPo> HisDataEx = new ObservableCollection<HisRecordPo>();
        public HisReocrdWin()
        {
            InitializeComponent();
            //this.Icon = LocalEnv.ServerInfo.AppImage;

            ThreadPool.QueueUserWorkItem(o =>
            {
                a(o);
            });
        }
        bool isStop = false;
        private void a(object o)
        {
            while (!isStop)
            // while(true)
            {
                are.WaitOne();
                refreshUI();
                //等等刷新信号

            };
        }
        private void refreshUI()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
           new Action(delegate () { this.DataContext = HisData; }));
        }
        public void RefreshData(List<TransOrderServerData> data)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                HisData = GetHisRecordNew(data);
                are.Set();
            });

        }
        public void RefreshDataEx(List<TransOrderServerData> data)
        {
            List<HisRecordPo> datas = GetHisRecordNew(data);
            HisDataEx.Clear();
            //初次加载数量
            int initNum = 15;
            if (datas.Count() > initNum)
            {
                for (int i = 0; i < initNum; i++)
                {
                    HisDataEx.Add(datas[i]);
                }
                this.DataContext = HisDataEx;
                for (int i = initNum; i < datas.Count(); i++)
                {
                    //使用中间变量x，避免for循环中编译器对变量i重复修改。
                    int x = i;
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                     new Action(delegate () { HisDataEx.Add(datas[x]); }));
                }
            }
            else
            {
                for (int i = 0; i < datas.Count(); i++)
                {
                    HisDataEx.Add(datas[i]);
                }
                this.DataContext = HisDataEx;
            }



        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            isStop = true;
            are.Set();
            this.Close();

        }



        /// <summary>
        /// 设置某列某行的背景色
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="item"></param>
        private void setColor(int[] arr, OrderDetailPo item)
        {
            foreach (int a in arr)
            {
                (dgRecords.Columns[a].GetCellContent(item) as TextBlock).Background = (Brush)FindResource("ColorBrush.Yellow");
                (dgRecords.Columns[a].GetCellContent(item) as TextBlock).Foreground = (Brush)FindResource("ColorBrush.Black");
            }

        }

        /// <summary>
        /// 确定操作类型
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public List<ExecutionReport> GetOperateType(List<ExecutionReport> datas)
        {
            List<ExecutionReport> sortList = datas.OrderBy(o => o.UpdateTime).ToList();
            try
            {
                if (sortList.Count > 1)
                {
                    for (int i = 1; i <= sortList.Count - 1; i++)
                    {

                        CompareData(sortList[i - 1], sortList[i]);
                    }
                }
                sortList = sortList.OrderByDescending(o => o.UpdateTime).ToList();
                ResetOrderPosition(sortList);
                return sortList;
            }
            catch (Exception e)
            {
                //LogHelper.Error(e);
                return sortList;
            }
        }
        /// <summary>
        /// 获取操作流水 新版本 ，对应流水有操作类型字段的数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public List<HisRecordPo> GetHisRecordNew(List<TransOrderServerData> datas)
        {
            //过滤掉修改加桥按钮状态 20200414 wxp
            //过滤掉queryFlag==1的单子，过桥专员和自理的查看操作过滤奥queryFlag==1；
            datas = datas.Where(o => o.CommandType != ((int)ECommandType.修改加桥按钮B状态).ToString()
            && o.CommandType != ((int)ECommandType.修改加桥按钮O状态).ToString()
            && o.QueryFlag + "" != "1").ToList();
            //根据id排序，可以保证顺序，不可根据时间排序，因为存在多条数据同一时间的情况。
            datas = datas.OrderBy(o => o.ID).ToList();
            List<HisRecordPo> hisdatas = new List<HisRecordPo>();
            for (int i = 0; i < datas.Count(); i++)
            {
                var o = datas[i];

                HisRecordPo hisdata = new HisRecordPo();
                hisdata.OperateTime = datas[i].UpdateTime;
                DateTime dt;
                if (DateTime.TryParseExact(datas[i].UpdateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    hisdata.OperateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    hisdata.OperateTime = datas[i].UpdateTime;
                }
                if (o.CommandType == "0")
                {
                    if (i > 0)
                        CompareData(datas[i - 1], datas[i]);
                    hisdata.OperateType = TransOrderHelper.Instance.GetOperateType(datas[i]);
                }
                else
                    hisdata.OperateType = (((ECommandType)Convert.ToInt32(datas[i].CommandType))).GetDescription();
                if (i == 0)
                {
                    hisdata.OperatePerson = datas[i].CloseBy + HisRecordPo.Flag + datas[i].CloseBy;
                    hisdata.TimeToMaturity = datas[i].TimeToMaturity + HisRecordPo.Flag + datas[i].TimeToMaturity;
                    hisdata.GoodsCode = datas[i].SecurityID + HisRecordPo.Flag + datas[i].SecurityID;
                    hisdata.GoodsShortName = datas[i].Symbol + HisRecordPo.Flag + datas[i].Symbol;
                    hisdata.Price =string.IsNullOrWhiteSpace(datas[i].Price) ?"0.00":(datas[i].Price.ToString() + HisRecordPo.Flag + datas[i].Price.ToString());
                    hisdata.PriceDescription = datas[i].FanDian + HisRecordPo.Flag + datas[i].FanDian;
                    hisdata.Qty = datas[i].LastQty.ToString() + HisRecordPo.Flag + datas[i].LastQty.ToString();

                    if (hisdata.OperateType == ECommandType.InternalTransaction.GetDescription())
                    {
                        hisdata.Settle = TransOrderHelper.Instance.GetSettleDisplay(datas[i].SettlType, datas[i].TradeDate) + HisRecordPo.Flag
                            + TransOrderHelper.Instance.GetSettleDisplay(datas[i].SettlType, datas[i].TradeDate);
                    }
                    else
                    {
                        hisdata.Settle = datas[i].ContentDisplay + HisRecordPo.Flag + datas[i].ContentDisplay;
                    }

                    hisdata.InstA = ("2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]))
                        + HisRecordPo.Flag +
                        ("2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]));
                    hisdata.InstB =
                        (!"2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]))
                        + HisRecordPo.Flag +
                        (!"2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]));
                    hisdata.BrokerA =
                        ("2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName)
                        + HisRecordPo.Flag +
                        ("2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName);
                    hisdata.BrokerB =
                          (!"2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName)
                        + HisRecordPo.Flag +
                        (!"2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName);

                    hisdata.SendTo = (!(datas[i].BridgeFlag + "").Equals("2") ? datas[i].SendTo : "")
                        + HisRecordPo.Flag
                        + ((datas[i].BridgeFlag + "").Equals("2") ? datas[i].SendTo : "");
                    hisdata.OfrSendTo = datas[i].OfrSendToDisplay + HisRecordPo.Flag + datas[i].OfrSendToDisplay;
                    hisdata.BidSendTo = datas[i].BidSendToDisplay + HisRecordPo.Flag + datas[i].BidSendToDisplay;

                    hisdata.BridgeSatus =
                        (string.IsNullOrEmpty(datas[i].BridgeFlag) ? "否" : datas[i].BridgeFlag.Equals("2") ? "是" : "否")
                        + HisRecordPo.Flag
                        + (string.IsNullOrEmpty(datas[i].BridgeFlag) ? "否" : datas[i].BridgeFlag.Equals("2") ? "是" : "否");
                    hisdata.DoubleBridgeSettle = datas[i].DoubleBridgeSettle + HisRecordPo.Flag + datas[i].DoubleBridgeSettle;
                    hisdata.DoubleBridgeInfo = datas[i].DoubleBridgeInfo + HisRecordPo.Flag + datas[i].DoubleBridgeInfo;
                    //是否免佣
                    hisdata.BrokerageA = GetBrokerShow(datas[i],true) + HisRecordPo.Flag + GetBrokerShow(datas[i],true);
                    hisdata.BrokerageB = GetBrokerShow(datas[i], false) + HisRecordPo.Flag + GetBrokerShow(datas[i], false);

                    //3.0.0
                    hisdata.issueDir = datas[i].IssueDir;
                    hisdata.IssueDirShow = GetIssueDirShow(datas[i]) + HisRecordPo.Flag + GetIssueDirShow(datas[i]);
                    hisdata.issueChannel = datas[i].IssueChannel;
                    hisdata.IssueChannelShow = GetIssueChannelShow(datas[i]) + HisRecordPo.Flag + GetIssueChannelShow(datas[i]);
                    hisdata.IssueTraderShow = GetIssueTraderShow(datas[i]) + HisRecordPo.Flag + GetIssueTraderShow(datas[i]);
                    hisdata.issueRemark = datas[i].IssueRemark + HisRecordPo.Flag + datas[i].IssueRemark;

                    hisdata.ForbidRefreshFlag = GetForbidRefreshFlagDisplay(datas[i].ForBidRefreshFlag);
                }
                else
                {

                    hisdata.OperatePerson = datas[i].CloseBy + HisRecordPo.Flag + datas[i-1].CloseBy;
                    hisdata.TimeToMaturity = datas[i].TimeToMaturity + HisRecordPo.Flag + datas[i - 1].TimeToMaturity;
                    hisdata.GoodsCode = datas[i].SecurityID + HisRecordPo.Flag + datas[i - 1].SecurityID;
                    hisdata.GoodsShortName = datas[i].Symbol + HisRecordPo.Flag + datas[i - 1].Symbol;
                    hisdata.Price = (string.IsNullOrWhiteSpace(datas[i].Price) ?"0.00":datas[i].Price.ToString())
                        + HisRecordPo.Flag 
                        + (string.IsNullOrWhiteSpace(datas[i-1].Price) ? "0.00" : datas[i-1].Price.ToString());
                    hisdata.PriceDescription = datas[i].FanDian + HisRecordPo.Flag + datas[i - 1].FanDian;
                    hisdata.Qty = datas[i].LastQty.ToString() + HisRecordPo.Flag + datas[i - 1].LastQty.ToString();
                    if (hisdata.OperateType == ECommandType.InternalTransaction.GetDescription())
                    {
                        hisdata.Settle = TransOrderHelper.Instance.GetSettleDisplay(datas[i].SettlType, datas[i].TradeDate) + HisRecordPo.Flag
                            + TransOrderHelper.Instance.GetSettleDisplay(datas[i-1].SettlType, datas[i-1].TradeDate);
                    }
                    else
                    {
                        hisdata.Settle = datas[i].ContentDisplay + HisRecordPo.Flag + datas[i-1].ContentDisplay;
                    }

                    hisdata.InstA = ("2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]))
                        + HisRecordPo.Flag +
                        ("2".Equals(datas[i - 1].Side) ? GetTraderAInfo(datas[i - 1]) : GetTraderBInfo(datas[i - 1]));
                    hisdata.InstB =
                        (!"2".Equals(datas[i].Side) ? GetTraderAInfo(datas[i]) : GetTraderBInfo(datas[i]))
                        + HisRecordPo.Flag +
                        (!"2".Equals(datas[i - 1].Side) ? GetTraderAInfo(datas[i - 1]) : GetTraderBInfo(datas[i - 1]));
                    hisdata.BrokerA =
                        ("2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName)
                        + HisRecordPo.Flag +
                        ("2".Equals(datas[i - 1].Side) ? datas[i - 1].PartySubName : datas[i - 1].CounterPartySubName);
                    hisdata.BrokerB =
                          (!"2".Equals(datas[i].Side) ? datas[i].PartySubName : datas[i].CounterPartySubName)
                        + HisRecordPo.Flag +
                        (!"2".Equals(datas[i - 1].Side) ? datas[i - 1].PartySubName : datas[i - 1].CounterPartySubName);

                    hisdata.SendTo = (!(datas[i].BridgeFlag + "").Equals("2") ? datas[i].SendTo : "")
                        + HisRecordPo.Flag
                        + ((datas[i - 1].BridgeFlag + "").Equals("2") ? datas[i - 1].SendTo : "");
                    hisdata.OfrSendTo =datas[i].OfrSendToDisplay + HisRecordPo.Flag + datas[i - 1].OfrSendToDisplay;
                    hisdata.BidSendTo = datas[i].BidSendToDisplay + HisRecordPo.Flag + datas[i - 1].BidSendToDisplay;

                    hisdata.BridgeSatus =
                        (string.IsNullOrEmpty(datas[i].BridgeFlag) ? "否" : datas[i].BridgeFlag.Equals("2") ? "是" : "否")
                        + HisRecordPo.Flag
                        + (string.IsNullOrEmpty(datas[i - 1].BridgeFlag) ? "否" : datas[i-1].BridgeFlag.Equals("2") ? "是" : "否");
                    hisdata.DoubleBridgeSettle = datas[i].DoubleBridgeSettle + HisRecordPo.Flag + datas[i-1].DoubleBridgeSettle;
                    hisdata.DoubleBridgeInfo = datas[i].DoubleBridgeInfo + HisRecordPo.Flag + datas[i-1].DoubleBridgeInfo;
                    //是否免佣
                    hisdata.BrokerageA = GetBrokerShow(datas[i], true) + HisRecordPo.Flag + GetBrokerShow(datas[i - 1], true);
                    hisdata.BrokerageB = GetBrokerShow(datas[i], false) + HisRecordPo.Flag + GetBrokerShow(datas[i - 1], false);

                    //3.0.0
                    hisdata.issueDir = datas[i].IssueDir;
                    hisdata.IssueDirShow = GetIssueDirShow(datas[i]) + HisRecordPo.Flag + GetIssueDirShow(datas[i - 1]);
                    hisdata.issueChannel = datas[i].IssueChannel;
                    hisdata.IssueChannelShow = GetIssueChannelShow(datas[i]) + HisRecordPo.Flag + GetIssueChannelShow(datas[i - 1]);
                    hisdata.IssueTraderShow = GetIssueTraderShow(datas[i]) + HisRecordPo.Flag + GetIssueTraderShow(datas[i - 1]);
                    hisdata.issueRemark = datas[i].IssueRemark + HisRecordPo.Flag + datas[i - 1].IssueRemark;

                    hisdata.ForbidRefreshFlag = GetForbidRefreshFlagDisplay(datas[i].ForBidRefreshFlag);

                }
                hisdatas.Add(hisdata);

                //hisdatas.Insert(0, hisdata);
            }
            dcsProcess(hisdatas);
            hisdatas.Reverse();
            return hisdatas;

        }


        public bool IsDealToday(ExecutionReport er)
        {
            DateTime dt;
            if (DateTime.TryParseExact(er.UpdateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day);
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                TimeSpan ts = now - dt;

                if (ts.Days >= 1)
                    return true;
            }
            return false;
        }

        public string GetBrokerShow(ExecutionReport er, bool isbrokerA)
        {
            bool isGVN = "1".Equals(er.Side);
            if (isbrokerA)
            {
                if (isGVN)
                {
                    return er.BrokerageA + "" == "1" ? "ofr是" : "ofr否";
                }
                else
                {
                    return er.BrokerageA + "" == "1" ? "bid是" : "bid否";
                }
            }
            else {
                if (isGVN)
                {
                    return er.BrokerageB + "" == "1" ? "bid是" : "bid否";
                }
                else
                {
                    return er.BrokerageB + "" == "1" ? "ofr是" : "ofr否";
                }
            }
        }
        /// <summary>
        /// 获取发送展示
        /// </summary>
        /// <returns></returns>
        private string GetForbidRefreshFlagDisplay(string forbidRefreshFlag)
        {
            string result = "";
            switch (forbidRefreshFlag)
            {
                case "1":
                    result = "GVN";
                    break;
                case "2":
                    result = "TKN";
                    break;
                case "3":
                    result = "TRD";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 获取发给方向
        /// </summary>
        /// <returns></returns>
        private string GetIssueDirShow(ExecutionReport orderDetailPo)
        {
            if (string.IsNullOrWhiteSpace(orderDetailPo.IssueDir))
            {
                return "";
            }

            if (orderDetailPo.IssueDir == "1")
            {
                return "ofr发bid";
            }
            else if (orderDetailPo.IssueDir == "2")
            {
                return "bid发ofr";
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 获取发给渠道
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        private string GetIssueChannelShow(ExecutionReport orderDetailPo)
        {
            if (string.IsNullOrWhiteSpace(orderDetailPo.IssueChannel))
            {
                return "";
            }

            string strChannel = "";
            foreach (ComboxBaseModel item in EnumHelper.Instance.ChannelComboxModel)
            {
                if (item.ValueMemberPath == orderDetailPo.IssueChannel)
                {
                    strChannel = item.DisplayMemberPath;
                    return strChannel;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取发单人展示
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        private string GetIssueTraderShow(ExecutionReport orderDetailPo)
        {

            string issueInstName = orderDetailPo.IssueInstNameShort;
            if (string.IsNullOrWhiteSpace(issueInstName))
            {
                issueInstName = orderDetailPo.IssueInstName;
            }

            string idesc = "";
            if (!string.IsNullOrWhiteSpace(orderDetailPo.SecurityID))
            {
                if (string.IsNullOrWhiteSpace(orderDetailPo.IssueChannel))
                {
                    return idesc;
                }
                ChannelEnum channelEnum = (ChannelEnum)System.Enum.Parse(typeof(ChannelEnum), orderDetailPo.IssueChannel, true);
                switch (channelEnum)
                {
                    case ChannelEnum.Request:
                        idesc = orderDetailPo.IssueDesc + " " + issueInstName;
                        break;
                    case ChannelEnum.FixedIncome:
                    case ChannelEnum.Bulk:
                    case ChannelEnum.Bidding:
                    case ChannelEnum.Dialogue:
                    case ChannelEnum.Bilateral:
                    case ChannelEnum.xbond:
                    case ChannelEnum.iDeal:
                    case ChannelEnum.Xbargain:
                        if (!string.IsNullOrWhiteSpace(orderDetailPo.IssueTraderName))
                        {
                            idesc = orderDetailPo.IssueDesc + issueInstName + "(" + orderDetailPo.IssueTraderName + ")";
                        }
                        else
                        {
                            idesc = orderDetailPo.IssueDesc + issueInstName;
                        }
                        break;
                    case ChannelEnum.Intention:
                    case ChannelEnum.Anonymity:
                        if (!string.IsNullOrWhiteSpace(orderDetailPo.IssueTraderName))
                        {
                            idesc = issueInstName + "(" + orderDetailPo.IssueTraderName + ")";
                        }
                        else
                        {
                            idesc = issueInstName;
                        }
                        break;
                }
            }
            else
            {
                idesc = orderDetailPo.IssueDesc + " " + issueInstName;
            }

            return string.IsNullOrWhiteSpace(idesc) ? string.Empty : idesc;
        }

        /// <summary>
        /// DCS操作对字段进行隐藏
        /// </summary>
        /// <param name="hisdata"></param>
        void dcsProcess(List<HisRecordPo> hisdata)
        {
            foreach (HisRecordPo record in hisdata)
            {
                if (record.OperateType == ECommandType.DCS操作.ToString())
                {
                    record.GoodsCode = string.Empty;
                    record.GoodsShortName = string.Empty;
                    record.TimeToMaturity = string.Empty;

                    record.Price = string.Empty;
                    record.PriceDescription = string.Empty;
                    record.Qty = string.Empty;
                    record.Settle = string.Empty;
                    record.SendTo = string.Empty;
                    record.OfrSendTo = string.Empty;
                    record.BidSendTo = string.Empty;

                    record.BrokerA = string.Empty;
                    record.BrokerB = string.Empty;
                    record.InstA = string.Empty;
                    record.InstB = string.Empty;
                    record.BrokerageA = string.Empty;
                    record.BrokerageB = string.Empty;

                    record.BridgeSatus = string.Empty;
                    record.DoubleBridgeInfo = string.Empty;
                    record.DoubleBridgeSettle = string.Empty;
                    //3.0.0
                    record.issueDir = string.Empty;
                    record.IssueDirShow = string.Empty;
                    record.issueChannel = string.Empty;
                    record.IssueChannelShow = string.Empty;
                    record.IssueTraderShow = string.Empty;
                    record.issueRemark = string.Empty;

                }
                else if (record.OperateType == ECommandType.InternalTransaction.GetDescription())
                {
                    //record.GoodsCode = string.Empty;
                    //record.GoodsShortName = string.Empty;
                    //record.TimeToMaturity = string.Empty;

                    //record.Price = string.Empty;
                    record.PriceDescription = string.Empty;
                    //record.Qty = string.Empty;
                    //record.Settle = string.Empty;
                    record.SendTo = string.Empty;
                    record.OfrSendTo = string.Empty;
                    record.BidSendTo = string.Empty;

                    //record.BrokerA = string.Empty;
                    //record.BrokerB = string.Empty;
                    //record.InstA = string.Empty;
                    //record.InstB = string.Empty;
                    //record.BrokerageA = string.Empty;
                    //record.BrokerageB = string.Empty;

                    record.BridgeSatus = string.Empty;
                    record.DoubleBridgeInfo = string.Empty;
                    record.DoubleBridgeSettle = string.Empty;
                    //3.0.0
                    record.issueDir = string.Empty;
                    record.IssueDirShow = string.Empty;
                    record.issueChannel = string.Empty;
                    record.IssueChannelShow = string.Empty;
                    record.IssueTraderShow = string.Empty;
                    record.issueRemark = string.Empty;
                }
            }
        }

        /// <summary>
        /// 重置A和B确认的位置
        /// </summary>
        /// <param name="datas"></param>
        private void ResetOrderPosition(List<ExecutionReport> datas)
        {
            ExecutionReport brokerBConfirmOrder = null;
            ExecutionReport brokerAConfirmOrder = null;
            ///B确认操作
            List<ExecutionReport> brokerBOrders = (from a in datas where a.OrdStatus == "E" select a).ToList();
            ///A确认操作且A未传DCS
            List<ExecutionReport> brokerAOrders = (from a in datas where a.OrdStatus == "G" && (a.DCSFlag == string.Empty || a.DCSFlag == null) select a).ToList();
            ///B的确认量
            //if (brokerBOrders != null && brokerBOrders.Count > 0 && brokerBOrders[0].MatchQTY!= brokerBOrders[0].BrokerBConfirmQty)
            //{
            //    brokerBOrders[0].NewMsg = brokerBOrders[0].qty + "";
            //    brokerBOrders[0].OldMsg = brokerBOrders[0].matchQty + "";
            //}

            if (brokerBOrders != null && brokerBOrders.Count > 0 && brokerAOrders != null && brokerAOrders.Count > 0)
            {
                brokerBConfirmOrder = brokerBOrders[0];
                brokerAConfirmOrder = brokerAOrders[0];
                ///如果A和B同时确认，则B应该在前
                if (brokerBConfirmOrder.UpdateTime.Equals(brokerAConfirmOrder.UpdateTime))
                {
                    int indexBConfirm = datas.IndexOf(brokerBConfirmOrder);
                    int indexAConfirm = datas.IndexOf(brokerAConfirmOrder);
                    ///调整A和B的确认顺序，B先确认，A再确认
                    if (indexAConfirm > indexBConfirm)
                    {
                        datas[indexAConfirm] = brokerBConfirmOrder;
                        datas[indexBConfirm] = brokerAConfirmOrder;
                    }
                }
            }
        }


        /// <summary>
        /// 对比哪些字段被修改，继而推到出成交单的操作类型
        /// </summary>
        /// <param name="newOrder"></param>
        /// <param name="preNewOrder"></param>
        /// <param name="oldOrder"></param>
        private void CompareData(ExecutionReport preOrder, ExecutionReport currOrder)
        {
            //修改债券代码
            if (!currOrder.SecurityID.Equals(preOrder.SecurityID))
            {
                currOrder.OrdStatus = "a";
                return;
            }
            //修改价格
            if (!currOrder.Yield.Equals(preOrder.Yield))
            {
                currOrder.OrdStatus = "b";
                //修改价格/量
                if (!(currOrder.LastQty + "").Equals(preOrder.LastQty + ""))
                {
                    currOrder.OrdStatus = "k";
                }
                return;
            }
            //修改量 需要屏蔽B的部分确认
            if (!(currOrder.LastQty + "").Equals(preOrder.LastQty + "") && currOrder.OrdStatus + "" != "E")
            {
                currOrder.OrdStatus = "c";
                return;
            }
            //修改结算方式
            //if (!(TransOrderHelper.Instance.GetDisplaySettle(currOrder)).Equals(OrderHelper.GetDisplaySettle(preOrder)))
            //{
            //    currOrder.OrdStatus = "d";
            //    return;
            //}
            //修改交易员A
            if (!(GetTraderAInfo(currOrder) + "").Equals(GetTraderAInfo(preOrder) + ""))
            {
                currOrder.OrdStatus = "e";
                return;
            }
            //修改交易员B
            if (!(GetTraderBInfo(currOrder)).Equals(GetTraderBInfo(preOrder)))
            {
                currOrder.OrdStatus = "e";
                return;
            }
            //修改经纪商A
            if (!(currOrder.PartySubID + "").Equals(preOrder.PartySubID + ""))
            {
                currOrder.OrdStatus = "f";
                return;
            }
            //修改经纪商B
            if (!(currOrder.CounterPartySubID + "").Equals(preOrder.CounterPartySubID + ""))
            {
                currOrder.OrdStatus = "f";
                return;
            }
            //bridgeStatus 修改加双桥按钮状态 过滤掉
            if (!(currOrder.BridgeStatus + "").Equals(preOrder.BridgeStatus + ""))
            {
                // currOrder.OrdStatus = "s";
            }
            //修改返点
            if (!(currOrder.FanDian + "").Equals(preOrder.FanDian + ""))
            {
                currOrder.OrdStatus = "l";
                return;
            }

            //加桥 0-单
            if (string.IsNullOrEmpty(preOrder.BridgeType) && currOrder.BridgeType + "" == "B,O")
            {

                currOrder.OrdStatus = "h";
                return;
            }
            //加桥 单-双
            if (preOrder.BridgeType + "" == "B,O" && (currOrder.BridgeType + "" == "B" || currOrder.BridgeType + "" == "O"))
            {
                currOrder.OrdStatus = "h";
                return;
            }
            //删除桥 单-0
            if (preOrder.BridgeType + "" == "B,O" && string.IsNullOrEmpty(currOrder.BridgeType))
            {
                currOrder.OrdStatus = "j";
                return;
            }
            //删除桥 双-单
            if (currOrder.BridgeType + "" == "B,O" && (preOrder.BridgeType + "" == "B" || preOrder.BridgeType + "" == "O"))
            {
                currOrder.OrdStatus = "j";
                return;
            }
            //删除桥 双-0
            if (string.IsNullOrEmpty(currOrder.BridgeType) && (preOrder.BridgeType + "" == "B" || preOrder.BridgeType + "" == "O"))
            {
                currOrder.OrdStatus = "j";
                return;
            }
            //修改桥状态
            if (!(currOrder.BridgeFlag + "").Equals(preOrder.BridgeFlag + ""))
            {
                currOrder.OrdStatus = "n";
                return;
            }
            //修改桥-明细是否显示1
            if (!(currOrder.BridgeRemarkShow + "").Equals(preOrder.BridgeRemarkShow + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-明细是否显示2
            if (!(currOrder.BridgeRemarkShow2 + "").Equals(preOrder.BridgeRemarkShow2 + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-备注1
            if (!(currOrder.BridgeRemark + "").Equals(preOrder.BridgeRemark + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-备注2
            if (!(currOrder.BridgeRemark2 + "").Equals(preOrder.BridgeRemark2 + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-费用1
            if (!(currOrder.DoubleBridgeFee + "").Equals(preOrder.DoubleBridgeFee + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-费用2
            //if (!(currOrder.DoubleBridgeFee2 + "").Equals(preOrder.DoubleBridgeFee2 + ""))
            //{
            //    currOrder.OrdStatus = "i";
            //    return;
            //}
            //修改桥-渠道1
            if (!(currOrder.BridgeChannel + "").Equals(preOrder.BridgeChannel + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥-渠道2
            if (!(currOrder.BridgeChannel2 + "").Equals(preOrder.BridgeChannel2 + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改发给对象bridegeofr
            if (!(currOrder.BridgeSendTo + "").Equals(preOrder.BridgeSendTo + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改发给对象bridegebid
            if (!(currOrder.BridgeSendTo2 + "").Equals(preOrder.BridgeSendTo2 + ""))
            {
                currOrder.OrdStatus = "i";
                return;
            }
            //修改桥 双桥信息
            //if (!(currOrder.DoubleBridgeInfo + "").Equals(preOrder.DoubleBridgeInfo + ""))
            //{
            //    currOrderOrdStatus = "i";
            //    return;
            //}
            ////修改桥 双桥结算
            //if (!(currOrder.DoubleBridgeSettle + "").Equals(preOrder.DoubleBridgeSettle + ""))
            //{
            //    currOrderOrdStatus = "i";
            //    return;
            //}
            //修改发给对象ofr
            if (!(currOrder.SendTo2 + "").Equals(preOrder.SendTo2 + ""))
            {
                currOrder.OrdStatus = "g";
                return;
            }
            //修改发给对象bid
            if (!(currOrder.SendTo + "").Equals(preOrder.SendTo + ""))
            {
                currOrder.OrdStatus = "g";
                return;
            }

            //修改双桥发给
            if (!(currOrder.DoubleBridgeSendTo + "").Equals(preOrder.DoubleBridgeSendTo + ""))
            {
                currOrder.OrdStatus = "g";
                return;
            }

            //同步dcs 
            if (!(currOrder.SyncDCSFlag + "").Equals(preOrder.SyncDCSFlag + "") && currOrder.SyncDCSFlag + "" == "1")
            {
                currOrder.OrdStatus = "m";
                return;
            }
        }



        private string GetTraderAInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.PartyCode) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.PartyName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.PartyName)))
            {
                //if (!string.IsNullOrWhiteSpace(orderDetail.PartyName))
                //    return string.Format("{0}({1})", orderDetail.PartyName, orderDetail.TraderName.Trim());
                //else
                    return string.Format("{0}({1})", orderDetail.InstitutionName, orderDetail.TraderName.Trim());
            }

            if (string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.PartyName)))
            {
                //if (!string.IsNullOrWhiteSpace(orderDetail.PartyName))
                //    return orderDetail.PartyName;
                //else
                    return orderDetail.InstitutionName;
            }
            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.PartyName))
                return orderDetail.TraderName;

            return "";
        }

        private string GetTraderBInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyName)))
            {
                //if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyName))
                //    return string.Format("{0}({1})", orderDetail.CounterPartyName, orderDetail.CounterPartyTraderName.Trim());
                //else
                    return string.Format("{0}({1})", orderDetail.CounterPartyInstitutionName, orderDetail.CounterPartyTraderName.Trim());
            }


            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyName)))
            {
                //if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyName))
                //    return orderDetail.CounterPartyName;
                //else
                    return orderDetail.CounterPartyInstitutionName;
            }

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyName))
                return orderDetail.CounterPartyTraderName;

            return "";
        }

        private void BackGround_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border sp = e.OriginalSource as Border;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;
                if (cp != null)
                {
                    HisRecordPo po = cp.Content as HisRecordPo;
                    if (po != null)
                    {
                        TextBlock TextBlockMY = sp.FindName("MY") as TextBlock;
                        TextBlock TextBlockBrokerageA = sp.FindName("BrokerageA") as TextBlock;
                        TextBlock TextBlockBrokerageB = sp.FindName("BrokerageB") as TextBlock;
                        //dcs操作，隐藏中间的逗号和文本。
                        if (po.OperateType == ECommandType.DCS操作.ToString())
                        {
                            TextBlockMY.Visibility = Visibility.Collapsed;
                            TextBlockBrokerageA.Visibility = Visibility.Collapsed;
                            TextBlockBrokerageB.Visibility = Visibility.Collapsed;
                            return;
                        }
                        //ofr和bid都是否的情况下，隐藏中间的逗号和文本。
                        int index = po.BrokerageA.IndexOf(HisRecordPo.Flag);
                        string brokerageA = index > 0 ? po.BrokerageA.Substring(0, index) : "";
                        index = po.BrokerageB.IndexOf(HisRecordPo.Flag);
                        string brokerageB = index > 0 ? po.BrokerageB.Substring(0, index) : "";

                        if (TextBlockMY != null && TextBlockBrokerageA != null && TextBlockBrokerageB != null)
                        {
                            if ((brokerageA == "ofr否" && brokerageB == "bid否") || (brokerageA == "bid否" && brokerageB == "ofr否"))
                            {
                                TextBlockMY.Visibility = Visibility.Collapsed;
                                TextBlockBrokerageA.Visibility = Visibility.Collapsed;
                                TextBlockBrokerageB.Visibility = Visibility.Collapsed;
                            }
                        }

                    }
                }

            }

        }
    }
}
