using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using Microsoft.Win32;
using STC.Frame.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Utils;
using STC.Common.Model.PT;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Base;
using STC.Broker.Plugin.Model;
using STC.Common.Api;
using STC.Common.Handler.Base;
using System.Diagnostics;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// BridgeWin.xaml 的交互逻辑
    /// 过桥主窗口
    /// </summary>
    public partial class BridgeWinNew : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 当前过滤条件
        /// </summary>
        BridgeFilterCondition condition = new BridgeFilterCondition();
        private readonly string BridgeWinName = "BridgeWin";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string BridgeOrder = "BridgeOrder";

        private BlockingCollection<int> refreshBrigeOrdersQueue = new BlockingCollection<int>();

        /// <summary>
        /// 桥界面是否打开
        /// </summary>
        private bool IsActive { get; set; }


        /// <summary>
        /// 债券拉取完成标志
        /// </summary>
        public bool BondInitCompleted = false;


        /// <summary>
        /// 桥-桥订单 键值对
        /// </summary>
        public static Dictionary<UserBridge, ObservableCollection<BridgeOrderDetail>> BridgeDatasDic = new Dictionary<UserBridge, ObservableCollection<BridgeOrderDetail>>();
        public BridgeWinNew()
        {
            InitializeComponent();
            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
            ucTimeFilter.Datas.Clear();
            ucTimeFilter.Datas.AddRange(new string[] { "不限", "录入日", "交易日" });
            ucTimeFilter.DefaultSelect = "不限";
            ucTimeFilter.SelectChanged += UcTimeFilter_SelectChanged;

            mu_BridgeSelect.Datas.Clear();
            mu_BridgeSelect.Datas.AddRange(new string[] { "不限", "我的成交", "其他人成交" });
            mu_BridgeSelect.DefaultSelect = "不限";
            mu_BridgeSelect.SelectChanged += Mu_BridgeSelect_SelectChanged;

            this.Loaded += BridgeWin_Loaded;
            //this.Closed += BridgeWinNew_Closed;
            this.Unloaded += BridgeWin_Unloaded;
            BridgeUI.OnDeleteBridge += BridgeUI_OnDeleteBridge;
            BridgeUI.OnUpdateBridge += BridgeUI_OnUpdateBridge;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
            //GRHandler.Instance.SubscribeGrExec(Instance_UpdateOrder);
            TransOrderHandler.Instance.OnUpdateOrder2 += Instance_UpdateOrder;
            TransOrderHandler.Instance.OnOtherBridgeOrderUpdateOrder += Instance_UpdateOrder;
            TransOrderHandler.Instance.UpdateOrderOnBridgeStatusChanged += Instance_UpdateOrder;

            cbself.Visibility = LocalEnv.ConfInfo.ShowBridgeRoleSetting ? Visibility.Visible : Visibility.Collapsed;
            mu_BridgeSelect.Visibility = LocalEnv.ConfInfo.ShowBridgeRoleSetting ? Visibility.Collapsed : Visibility.Visible;

            IsActive = true;
            Task.Factory.StartNew(() => DoRefreshBrigeOrders(), TaskCreationOptions.LongRunning);
            run = true;
        }

        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                BondInitCompleted = obj;
            }
        }

        private void BridgeWinNew_Activated(object sender, EventArgs e)
        {

        }

        private void DoRefreshBrigeOrders()
        {
            if (!refreshBrigeOrdersQueue.IsCompleted)
            {
                while (true)
                {
                    refreshBrigeOrdersQueue.Take();

                    if (OrderPoQue.Count == 0)
                    {
                        LogHelper.Debug($"清空桥订单刷新:{refreshBrigeOrdersQueue.Count}");
                        int temp;
                        while (refreshBrigeOrdersQueue.TryTake(out temp)) ;
                        Dispatcher.Invoke(new Action(() => lsBridge_SelectionChanged(null, null)), DispatcherPriority.Background);
                    }
                    else
                    {
                        //LogHelper.Debug($"等待桥缓存更新");
                        Thread.Sleep(20);
                        refreshBrigeOrdersQueue.Add(1);
                    }
                }
            }
        }

        public void ReFlash()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
            {
                int selectIndex = this.lsBridge.SelectedIndex;
                isloaded = false;
                BridgeDatasDic.Clear();
                bridgeDataList.Clear();
                //判断有没有桥
                if (UserBridgeHandler.Instance.Bridges != null && UserBridgeHandler.Instance.Bridges.Count > 0)
                {
                    BridgeUI.bridgeInfoUI.Visibility = Visibility.Visible;
                    //查看初始化
                    reviewsInit();
                    var authorizedNames = IDCHelper.GetAuthorizedUserName();
                    List<TransOrderServerData> allOrders = BridgeHelper.getCacheList();
                    foreach (UserBridge bridge in UserBridgeHandler.Instance.Bridges)
                    {
                        //获取桥和桥订单
                        BridgeDatasDic.Add(bridge, GetDataByBridgeEx(bridge, allOrders, authorizedNames));
                        LogHelper.Info("OnClear BridgeUI init {0}({1})；桥下订单数量：{2}", bridge.bankInstitutionname, bridge.bankTradername, BridgeDatasDic[bridge].Count());
                        UpdateBridgeDataList("1", bridge, null);
                    }
                    //保留上次记忆功能
                    ResetBridgeBySaveIni();
                    lsBridge.ItemsSource = bridgeDataList;

                    //默认选中第一个桥
                    lsBridge.SelectedIndex = selectIndex;
                    isloaded = true;
                    //数据过滤
                    filterOrder();
                    //过滤查看
                    filterReviews();
                    run = true;

                }
                else
                {
                    BridgeUI.bridgeInfoUI.Visibility = Visibility.Hidden;
                }
                //更新回调注册
                //OrderHandler.Instance.UpdateOrder -= Instance_UpdateOrder;

                ////桥转状态变化订单处理
                //OrderHandler.Instance.UpdateOrderOnBridgeStatusChanged -= Instance_UpdateOrder;
                ////更新回调注册
                //OrderHandler.Instance.UpdateOrder += Instance_UpdateOrder;

                ////预约桥转状态变化订单处理
                //OrderHandler.Instance.UpdateOrderOnBridgeStatusChanged += Instance_UpdateOrder;
                IsActive = true;
            }));
        }
        /// <summary>
        /// 事件过滤条件处理
        /// </summary>
        /// <param name="selects"></param>
        private void UcTimeFilter_SelectChanged(List<string> selects)
        {
            if (selects.Contains("录入日"))
                condition.EntryDate = true;
            else
                condition.EntryDate = false;
            if (selects.Contains("交易日"))
                condition.DealDate = true;
            else
                condition.DealDate = false;
            if (selects.Contains("不限"))
            {
                condition.DealDate = false;
                condition.EntryDate = false;
            }
            filterOrder();
        }

        /// <summary>
        /// 桥订单所属筛选
        /// </summary>
        /// <param name="selects"></param>
        private void Mu_BridgeSelect_SelectChanged(List<string> selects)
        {
            if (selects.Contains("不限") || selects.Count == 2)
            {
                condition.BridgeOrderOfBroker = "0";
            }
            else if (selects.Contains("我的成交"))
            {
                condition.BridgeOrderOfBroker = "1";
            }
            else if (selects.Contains("其他人成交"))
            {
                condition.BridgeOrderOfBroker = "2";
            }
            filterOrder();
        }


        /// <summary>
        /// 窗口关闭进行清理工作，关闭更新处理线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BridgeWinNew_Closed(object sender, EventArgs e)
        {
            this.Loaded -= BridgeWin_Loaded;
            this.Unloaded -= BridgeWin_Unloaded;
            //OrderHandler.Instance.UpdateOrder -= Instance_UpdateOrder;
            //OrderHandler.Instance.UpdateOrderOnBridgeStatusChanged -= Instance_UpdateOrder;

            //string topic = string.Format("{0}{1}", BrokerCEDAServer.Instance.Topic_Prefix_BridgeReview, LocalEnv.UserInfo.SessionID);
            //BrokerCEDAServer.Instance.OnBridgeReviewChange -= ReviewStateChange;
            //BrokerCEDAServer.Instance.UnSubScribe(new string[] { topic });

            //foreach (var item in BridgeDatasDic)
            //{
            //    item.Value.Clear();
            //}
            //BridgeDatasDic.Clear();

            run = false;
            resetEvent.Set();

            IsActive = false;
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    // cancels the window close  
        //    e.Cancel = true;
        //    // Programmatically hides the window
        //    this.HitTestCore();
        //    LogHelper.Info("==================================关闭==================================");
        //    //OrderHandler.Instance.UpdateOrder -= Instance_UpdateOrder;
        //    //OrderHandler.Instance.UpdateOrderOnBridgeStatusChanged -= Instance_UpdateOrder;
        //    IsActive = false;
        //}
        /// <summary>
        /// 经过测试，主窗口关闭的时候并不会触发过桥窗口的unloaded事件，目前在窗口的closed事件中进行清理工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BridgeWin_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BridgeWin_Loaded;
            this.Unloaded -= BridgeWin_Unloaded;
            //OrderHandler.Instance.UpdateOrder -= Instance_UpdateOrder;
            run = false;
            resetEvent.Set();
        }
        /// <summary>
        /// 过桥窗口数据加载完成标志
        /// </summary>
        bool isloaded = false;
        #region 初始化
        private void BridgeWin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string height = IniFileHelper.ReadValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), WinHeight);
                string width = IniFileHelper.ReadValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), WinWidth);

                double d = 0;
                if (double.TryParse(height, out d))
                    this.Height = d;
                if (double.TryParse(width, out d))
                    this.Width = d;

                if (LocalEnv.ConfInfo.ExecPosition)
                {
                    string positionX = IniFileHelper.ReadValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), PositionX);
                    string positionY = IniFileHelper.ReadValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), PositionY);
                    double dY = 0;
                    if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
                    {
                        bool isContain = false;
                        foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                        {
                            if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
                            {
                                isContain = true;
                                break;
                            }
                        }

                        //if (isContain)
                        //{
                        //    this.Top = dY;
                        //    this.Left = d;
                        //}
                    }
                }
                //初始化桥
                UserBridgeHandler.Instance.InitBridges();

                //查询其他人过桥单


                //判断有没有桥
                if (UserBridgeHandler.Instance.Bridges != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        //查看初始化
                        reviewsInit();
                        lsBridge.ItemsSource = bridgeDataList;
                        var authorizedNames = IDCHelper.GetAuthorizedUserName();
                        List<TransOrderServerData> allOrders = BridgeHelper.getCacheList();
                        //桥列表
                        foreach (UserBridge bridge in UserBridgeHandler.Instance.Bridges)
                        {
                            //获取桥和桥订单
                            ObservableCollection<BridgeOrderDetail> bridgeOrderDetails;
                            if (!BridgeDatasDic.TryGetValue(bridge, out bridgeOrderDetails))
                            {
                                BridgeDatasDic.Add(bridge, GetDataByBridgeEx(bridge, allOrders, authorizedNames));
                                LogHelper.Info("BridgeUI init {0}({1})；桥下订单数量：{2}", bridge.bankInstitutionname, bridge.bankTradername, BridgeDatasDic[bridge].Count());
                                UpdateBridgeDataList("1", bridge, null);
                            }
                        }
                        //保留上次记忆功能
                        ResetBridgeBySaveIni();

                        lsBridge.ItemsSource = bridgeDataList;
                        //默认选中第一个桥
                        lsBridge.SelectedIndex = 0;
                        isloaded = true;
                        //数据过滤
                        filterOrder();
                        //过滤查看
                        filterReviews();
                    }));
                }
                //更新回调注册
                //OrderHandler.Instance.UpdateOrder += Instance_UpdateOrder;

                //桥转状态变化订单处理
                // OrderHandler.Instance.UpdateOrderOnBridgeStatusChanged += Instance_UpdateOrder;

                //其他人桥订单
                // OrderHandler.Instance.UpdateOtherBridgerOrder += Instance_UpdateOrder;


                //更新处理开启
                ProcessUpdate();

                Task.Factory.StartNew(() => { DoRefresh(); }, cancellationTokenSource.Token);

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }


        private void ResetBridgeBySaveIni()
        {
            string bridgeOrderList = IniFileHelper.ReadValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), BridgeOrder);

            if (bridgeDataList != null
            && bridgeDataList.Count > 0
            && !string.IsNullOrWhiteSpace(bridgeOrderList)
            && bridgeOrderList.IndexOf('|') != -1)
            {
                string[] orderList = bridgeOrderList.Split('|');
                List<UserBridge> bridges = new List<UserBridge>(bridgeDataList);
                int kCode = 0;
                foreach (UserBridge item in bridges)
                {
                    bool isOrder = false;
                    for (int i = 0; i < orderList.Length; i++)
                    {
                        if (orderList[i].IndexOf('-') != -1)
                        {
                            string[] brideg = orderList[i].Split('-');
                            if (item.bridgeId.Trim() == brideg[0].Trim())
                            {
                                if (!string.IsNullOrEmpty(brideg[1]))
                                {
                                    item.OderCode = Convert.ToInt32(brideg[1]);
                                    isOrder = true;
                                }
                                break;
                            }
                        }
                    }
                    if (!isOrder)
                    {
                        kCode++;
                        item.OderCode = Convert.ToInt32(bridgeDataList.Count + kCode);
                    }
                }
                bridgeDataList = new ObservableCollection<UserBridge>(bridges.OrderBy(o => o.OderCode));
            }
        }


        /// <summary>
        /// 查看初始化
        /// </summary>
        private void reviewsInit()
        {
            BridgeReserveReviews o;
            while (BridgeReviewQue.TryDequeue(out o))
            {
                ;
            }
            //订阅桥查看更新
            SubscribeBridgeReview();
            //获取桥下所有未查看的单子
            BridgeReviewsReplyPo msg = GRHandler.Instance.BridgeReviewsQuery();
            List<BridgeReserveReviews> BridgeReviews = new List<BridgeReserveReviews>();
            if (msg != null && msg.Code == "0000" && msg.datas.Count > 0)
            {
                BridgeUpdateCount = msg.datas.Count.ToString();
                BridgeReviews = msg.datas.OrderBy(_ => _.createTime).ToList();
                foreach (var data in BridgeReviews)
                {
                    BridgeReviewQue.Enqueue(data);
                }
            }
            foreach (BridgeReserveReviews data in BridgeReviews)
            {
                string key = data.instId + "-" + data.instTradeId;
                if (BridgeReviewsDic.ContainsKey(key))
                {
                    BridgeReviewsDic[key].Add(data);
                }
                else
                {
                    BridgeReviewsDic[key] = new List<BridgeReserveReviews>();
                    BridgeReviewsDic[key].Add(data);
                }
            }
        }

        /// <summary>
        /// 前端不存在成交单的情况下，过滤掉此查看，不展示在前端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filterReviews()
        {
            if (BridgeReviewQue.Count == 0)
                return;
            Stack<BridgeReserveReviews> stackTemp = new Stack<BridgeReserveReviews>();
            BridgeReserveReviews reviewTemp;
            while (BridgeReviewQue.TryDequeue(out reviewTemp))
            {
                //存在就加入临时堆栈
                if (bOrderIsExsit(reviewTemp))
                    stackTemp.Push(reviewTemp);
            }
            while (stackTemp.Count > 0)
            {
                //重新加入到队列
                BridgeReviewQue.Enqueue(stackTemp.Pop());
            }
            //更新查看计数
            this.Dispatcher.Invoke(() =>
            {
                BridgeUpdateCount = BridgeReviewQue.Count().ToString();
            });
        }
        private bool bOrderIsExsit(BridgeReserveReviews review)
        {
            if (review != null)
            {
                foreach (KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridgeData in BridgeDatasDic)
                {
                    UserBridge bridge = bridgeData.Key;
                    List<BridgeOrderDetail> orders = bridgeData.Value.ToList();
                    foreach (BridgeOrderDetail order in orders)
                    {
                        if (order.MasterID == review.borderId && order.BridgeisChecked == EBridgeisChecked.未查看)
                        {
                            return true;
                        }
                    }
                }
                //LogHelper.Info("找不到桥订单，过滤掉查看(reviewId:{0},borderId:{1};execId:{2})", review.reviewId, review.borderId, review.execId);
            }
            return false;
        }

        #endregion

        #region 更新
        /// <summary> 
        /// 订单更新回调处理
        /// </summary>
        /// <param name="obj"></param>Instance_UpdateOrder
        ///  private void OnSubscribeGrExec(List<TransOrderServerData> lster)
        private void Instance_UpdateOrder(TransOrderServerData obj)
        {
            // 一键查看不需要更新桥界面
            //if (obj.execOrders != null && obj.execOrders.Any(o => o.orderStatus.Equals("G") && o.isConfirmOperation == 1))
            //{
            //    return;
            //}
            //if (obj.isc) { 
            //}
            ///更新数据压入队列

            ///通知跟新处理线程处理跟新数据
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                // 一键查看不需要更新桥界面
                if (obj.OrdStatus == "G" && obj.IsConfirmOperation == "1")
                {
                    return;
                }
                OrderPoQue.Enqueue(obj);
                resetEvent.Set();
            });

            //TransOrderServerData orderPo;
            //while (OrderPoQue.TryDequeue(out orderPo))
            //{
            //    if (IsActive && orderPo != null)
            //    {
            //        //LogHelper.Debug($"订单更新刷新桥, seq={orderPo.execOrders.FirstOrDefault()?.seq}");
            //        LogHelper.Info(orderPo.OfrIssueInfoReqList);
            //        handleOrder(orderPo);
            //    }
            //}
        }


        //子线程，更新线程
        private Thread UpdateThread;
        //更新队列数据
        ConcurrentQueue<TransOrderServerData> OrderPoQue = new ConcurrentQueue<TransOrderServerData>();
        // 线程通知
        private static AutoResetEvent resetEvent = new AutoResetEvent(false);


        /// <summary>
        /// 刷新请求队列
        /// </summary>
        private BlockingCollection<int> refreshQueue = new BlockingCollection<int>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// 独立线程处理更新数据g
        /// </summary>
        public void ProcessUpdate()
        {
            var task = Task.Run(() => updateDatas());
        }

        public bool run = true;
        /// <summary>
        /// 订单更新处理
        /// </summary>
        public void updateDatas()
        {
            while (true)
            {
                resetEvent.WaitOne();
                while (!isloaded)
                {
                    Thread.Sleep(100);
                }
                //refresh();



                TransOrderServerData orderPo;
                while (OrderPoQue.TryDequeue(out orderPo))
                {
                    if (IsActive && orderPo != null)
                    {
                        LogHelper.Info($"订单更新刷新桥, Sequence={orderPo.Sequence}");
                        handleOrder(orderPo);
                    }
                }
            }
        }


        /// <summary>
        /// 刷新请求添加到请求队列
        /// </summary>
        public void refresh()
        {
            refreshQueue.Add(1);
        }

        private void ReFreshBridge()
        {
            if (UserBridgeHandler.Instance.Bridges != null)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    //查看初始化
                    reviewsInit();
                    lsBridge.ItemsSource = bridgeDataList;
                    var authorizedNames = IDCHelper.GetAuthorizedUserName();
                    List<TransOrderServerData> allOrders = BridgeHelper.getCacheList();
                    //桥列表
                    foreach (UserBridge bridge in UserBridgeHandler.Instance.Bridges)
                    {
                        //获取桥和桥订单
                        ObservableCollection<BridgeOrderDetail> bridgeOrderDetails;
                        if (!BridgeDatasDic.TryGetValue(bridge, out bridgeOrderDetails))
                        {
                            BridgeDatasDic.Add(bridge, GetDataByBridgeEx(bridge, allOrders, authorizedNames));
                            LogHelper.Info("BridgeUI init {0}({1})；桥下订单数量：{2}", bridge.bankInstitutionname, bridge.bankTradername, BridgeDatasDic[bridge].Count());
                            UpdateBridgeDataList("1", bridge, null);
                        }
                    }
                    //保留上次记忆功能
                    ResetBridgeBySaveIni();

                    lsBridge.ItemsSource = bridgeDataList;
                    //默认选中第一个桥
                    //lsBridge.SelectedIndex = 0;
                    isloaded = true;
                    //数据过滤
                    filterOrder();
                    //过滤查看
                    filterReviews();
                }));
            }
        }

        /// <summary>
        /// 刷新订单队列处理
        /// </summary>
        private void DoRefresh()
        {
            while (!refreshQueue.IsCompleted)
            {
                refreshQueue.Take();

                LogHelper.Debug($"清空订单数量: {refreshQueue.Count}");
                // 刷新前清空刷新请求队列
                int item;
                while (refreshQueue.TryTake(out item))
                {
                }
                //ReFreshBridge();
                ReFlash();
            }
        }
        /// <param name="orders"></param>
        /// <summary>
        /// 已过桥子窗口为单位，依次处理成功加载到过桥主窗口的过桥子窗口的单子
        /// </summary>
        public void handleOrder(TransOrderServerData order)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                foreach (KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge in BridgeDatasDic)
                {
                    handleOrder(order, bridge);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            stopwatch.Stop();
            LogHelper.Info("===========费时间：" + stopwatch.ElapsedMilliseconds);
        }



        /// <summary>
        /// 更新某个桥下的单子
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="bridge"></param>
        public void handleOrder(TransOrderServerData order, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            UserBridge Bridge = bridge.Key;
            List<BridgeOrderDetail> bridgeOrderList = bridge.Value.ToList();
            List<BridgeOrderDetail> removeOrder = new List<BridgeOrderDetail>();

            List<TransOrderServerData> temp = new List<TransOrderServerData>(); //.execOrders;
            temp.Add(order);

            var existInOther = TransOrderHandler.Instance.ExistInOtherOrders(order.ExecID);
            if (existInOther)
            {
                temp = TransOrderHandler.Instance.GetOtherDetailPoById(order.ExecID);
            }

            foreach (TransOrderServerData od in temp)
            {
                var isMyOrder = SystemPowerHandler.Instance.IsCanEditOrder(order.PartySubID) || SystemPowerHandler.Instance.IsCanEditOrder(order.CounterPartySubID);
                var hasOtherPermission = SystemPowerHandler.Instance.HasOtherPermision(order, Bridge);
                bool isContain = false;
                for (int i = 0; i < bridgeOrderList.Count; i++)
                {
                    if (od.ExecID.Equals(bridgeOrderList[i].ExecID))
                    {
                        isContain = true;
                        bool result = false;
                        ///删除订单，则删除桥
                        if (!string.IsNullOrEmpty(od.DelFlag) && EDelFlag.删除.EqualsToValue(od.DelFlag) || string.IsNullOrEmpty(od.BridgeType))
                        {
                            result = true;
                        }
                        else if ((!isMyOrder && !hasOtherPermission) && !SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId))
                        {
                            result = true;
                        }
                        else if (od.BridgeType.Equals("B,O"))
                        {
                            result = !(od.BridgeTraderId.Equals(Bridge.bankTraderid)
                                && od.BridgeInstitutionId.Equals(Bridge.bankInstitutionid));
                        }
                        else if (od.BridgeType.Equals("B") || od.BridgeType.Equals("O"))
                        {
                            result = !(od.BridgeTraderId.Equals(Bridge.bankTraderid) && od.BridgeInstitutionId.Equals(Bridge.bankInstitutionid))
                               && !(od.BridgeTraderId2.Equals(Bridge.bankTraderid) && od.BridgeInstitutionId2.Equals(Bridge.bankInstitutionid));
                        }
                        if (result)
                            removeOrder.Add(bridgeOrderList[i]);
                        else if (getBridgeOrderEx(od, Bridge) != null
                            && getBridgeOrderEx(od, Bridge).BidbridgeStateEnum != BridgeState.Cancelled
                            && getBridgeOrderEx(od, Bridge).OfrbridgeStateEnum != BridgeState.Cancelled
                            && bridgeOrderList[i].OfrBridgeStateShow == "已取消"
                            && bridgeOrderList[i].BidBridgeStateShow == "已取消")
                        {
                            ///如何已拒绝的单子，重新添加桥或者预约桥，则删除本地桥订单，并添加新的桥订单
                            removeOrder.Add(bridgeOrderList[i]);
                            AddOrderEx(od, bridge);
                        }
                        else
                        {
                            UpdateOrderEx(bridgeOrderList[i], od, bridge);
                        }
                    }
                    SetBridgeDir(bridgeOrderList[i]);
                }
                //添加订单
                if (!isContain && (string.IsNullOrEmpty(od.DelFlag) || !EDelFlag.删除.EqualsToValue(od.DelFlag))
                    && (isMyOrder
                    || hasOtherPermission
                    || SystemPowerHandler.Instance.IsBridge))
                {
                    if (!string.IsNullOrEmpty(od.BridgeType) && (od.BridgeType.Equals("B,O") || (od.BridgeType.Equals("B") || od.BridgeType.Equals("O")))
                        && (od.BridgeTraderId.Equals(Bridge.bankTraderid) && od.BridgeInstitutionId.Equals(Bridge.bankInstitutionid)))//加第一座桥
                    {
                        AddOrderEx(od, bridge);
                    }
                    else if (!string.IsNullOrEmpty(od.BridgeType2) && (od.BridgeType2.Equals("B") || od.BridgeType2.Equals("O"))
                            && (od.BridgeTraderId2.Equals(Bridge.bankTraderid) && od.BridgeInstitutionId2.Equals(Bridge.bankInstitutionid)))//加第二座桥
                    {
                        AddOrderEx(od, bridge);
                    }
                }
            }

            //符合条件后移除
            if (removeOrder.Count > 0)
            {
                RemoveOrder(removeOrder, bridge);
            }

        }


        /// <summary>
        /// 设置桥之间方向
        /// </summary>
        /// <param name="bridgeOrderDetail"></param>
        private void SetBridgeDir(BridgeOrderDetail bridgeOrderDetail)
        {
            if (bridgeOrderDetail == null)
            {
                return;
            }
            if (bridgeOrderDetail.BridgeInstitutionId2 == bridgeOrderDetail.BridgeInstitutionId
                && bridgeOrderDetail.BridgeInstitutionId == bridgeOrderDetail.BridgeInstitutionId2)
            {
                //单桥
                bridgeOrderDetail.Doublebridgedir = 0;
            }
            else
            {
                if (bridgeOrderDetail.bridgeOrder != null)
                {
                    if (bridgeOrderDetail.bridgeOrder.bidDir == 1)
                    {
                        bridgeOrderDetail.Doublebridgedir = 1;
                    }
                    else
                    {
                        bridgeOrderDetail.Doublebridgedir = 2;
                    }
                }
            }

        }

        /// <summary>
        /// 解析成交单在该桥下的桥订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        bridgeOrder getBridgeOrder(OrderDetailPo order, UserBridge Bridge)
        {
            bridgeOrder bridgeorder = null;
            string type = BridgeUIDomain.GetBridgeType(order, Bridge);
            if (type == "B,O")
            {
                bridgeorder = order.bridgeOrder;
            }
            else
            {
                if (order.bridgeOrder == null)
                {
                    bridgeorder = null;
                }
                else
                {
                    if (order.bridgeOrder.instId == Bridge.bankInstitutionid && order.bridgeOrder.instTradeId == Bridge.bankTraderid)
                        bridgeorder = order.bridgeOrder;
                    else
                        bridgeorder = order.bridgeOrder2;
                }
            }

            return bridgeorder;
        }

        bridgeOrder getBridgeOrderEx(TransOrderServerData order, UserBridge Bridge)
        {
            bridgeOrder bridgeorder = null;
            string type = BridgeUIDomain.GetBridgeType2(order, Bridge);
            if (type == "B,O")
            {
                bridgeorder = order.BridgeOrderModel;
            }
            else
            {
                if (order.BridgeOrderModel == null)
                {
                    bridgeorder = null;
                }
                else
                {
                    if (order.BridgeOrderModel.instId == Bridge.bankInstitutionid && order.BridgeOrderModel.instTradeId == Bridge.bankTraderid)
                        bridgeorder = order.BridgeOrderModel;
                    else
                        bridgeorder = order.BridgeOrderModel2;
                }
            }

            return bridgeorder;
        }
        /// <summary>
        /// 重新设置序号
        /// 预约桥订单被拒绝，则序号为空，其后的成交单的序号需要+1；
        /// </summary>
        /// <param name="removeOrder"></param>
        private void resetSeq(KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridge)
        {
            UserBridge Bridge = bridge.Key;
            List<BridgeOrderDetail> bridgeOrderList = bridge.Value;
            //降序
            List<BridgeOrderDetail> temList = bridgeOrderList.ToList();

            temList.Sort(new BridgeNormalSort());
            //智能排序
            if (NeedSort)
                temList.Sort(new BridgeGroupSort());

            if (_isMaturityDaySort)
            {
                temList = temList.OrderBy(_ => _.maturityDays).ToList();
            }

            int i = 1;
            foreach (BridgeOrderDetail bo in temList)
            {
                if (bo.OfrBridgeStateShow == "已取消" && bo.BidBridgeStateShow == "已取消")
                {
                    if (string.IsNullOrWhiteSpace(bo.OldOrderTxt))
                    {
                        bo.OldOrderTxt = BridgeHelper.CreateBridgeTxt(bo, BridgeUIDomain.GetBridgeType(bo), Bridge);
                    }
                    bo.OrderTxt = BridgeHelper.GetBridgeTxt(bo, Bridge, bo.OldOrderTxt);
                    continue;
                }
                bo.bridgeSeq = i++;
                if (string.IsNullOrWhiteSpace(bo.OldOrderNoseqTxt))
                {
                    bo.OldOrderNoseqTxt = BridgeHelper.CreateBridgeTxt(bo, BridgeUIDomain.GetBridgeType(bo), Bridge);
                }
                bo.OldOrderTxt = string.Format("{0}  {1}", bo.ShowSeq, bo.OldOrderNoseqTxt);
                bo.OrderTxt = BridgeHelper.GetBridgeTxt(bo, Bridge, bo.OldOrderTxt);
            }
            filterOrdersDic[Bridge] = new List<BridgeOrderDetail>(temList);
            //更新桥下数量 
            if (bridgeDataList.Count > 0)
                bridgeDataList.First(_ => _.bankInstitutionid == bridge.Key.bankInstitutionid && _.bankTraderid == bridge.Key.bankTraderid).ExecOrderCount = bridge.Value.Count(_ => _.BidBridgeStateShow != "已取消" && _.OfrBridgeStateShow != "已取消").ToString();
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void SortOrder(KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridge)
        {
            UserBridge Bridge = bridge.Key;
            List<BridgeOrderDetail> bridgeOrderList = bridge.Value;
            //降序
            List<BridgeOrderDetail> temList = bridgeOrderList.ToList();
            temList.Sort(new BridgeNormalSort());
            //智能排序
            if (NeedSort)
                temList.Sort(new BridgeGroupSort());

            if (_isMaturityDaySort)
            {
                temList = temList.OrderBy(_ => _.maturityDays).ToList();
            }
            filterOrdersDic[Bridge] = new List<BridgeOrderDetail>(temList);
        }
        /// <summary>
        /// 删除桥中的成交单，则桥中成交单的顺序需要从新排序
        /// </summary>
        /// <param name="removeOrder"></param>
        private void RemoveOrder(List<BridgeOrderDetail> removeOrder, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            UserBridge Bridge = bridge.Key;
            ObservableCollection<BridgeOrderDetail> bridgeOrderList = bridge.Value;
            if (removeOrder != null && removeOrder.Count > 0)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    //删除单子
                    foreach (BridgeOrderDetail bo in removeOrder)
                    {
                        bridgeOrderList.Remove(bo);
                    }
                }));
            }
            //删除查看
            foreach (BridgeOrderDetail bo in removeOrder)
            {
                ///订单删除，则根据成交单id删除所有匹配查看
                List<BridgeReserveReviews> reviews = BridgeReviewQue.Where(_ => _.execId == bo.ExecID).ToList();
                List<BridgeReserveReviews> reviewsTemp = new List<BridgeReserveReviews>();
                foreach (BridgeReserveReviews item in reviews.ToList())
                {
                    if (bo.ExecID == item.execId && bo.MasterID == item.borderId)
                    {
                        reviewsTemp.Add(item);
                    }
                }
                QueueDeleteItems(BridgeReviewQue, reviewsTemp);
            }
            //更新查看计数
            this.Dispatcher.Invoke(() =>
            {
                BridgeUpdateCount = BridgeReviewQue.Count().ToString();
            });
        }
        /// <summary>
        /// 删除队列的元素
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="reviews"></param>
        private void QueueDeleteItems(ConcurrentQueue<BridgeReserveReviews> queue, List<BridgeReserveReviews> reviews)
        {
            Stack<BridgeReserveReviews> stackTemp = new Stack<BridgeReserveReviews>();
            foreach (BridgeReserveReviews review in reviews)
            {
                BridgeReserveReviews reviewTemp;
                queue.TryDequeue(out reviewTemp);
                while (reviewTemp != review)
                {
                    //非删除项加入临时堆栈
                    stackTemp.Push(reviewTemp);
                    queue.TryDequeue(out reviewTemp);
                }
                while (stackTemp.Count > 0)
                {
                    //将非删除项重新加入到队列
                    queue.Enqueue(stackTemp.Pop());
                }
            }
        }
        private object lockobj = new object();
        /// <summary>
        /// 在桥中添加成交单
        /// </summary>
        /// <param name="orderDetail"></param>
        private void AddOrder(OrderDetailPo orderDetail, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            if (orderDetail.OrdStatus == "D")
                return;
            UserBridge Bridge = bridge.Key;
            ObservableCollection<BridgeOrderDetail> bridgeOrderList = bridge.Value;
            BridgeOrderDetail newBridgeOrder = ObjConvertHelper.CopyToEx(orderDetail);
            newBridgeOrder.isBidBridge = BridgeUIDomain.IsBidBridge(newBridgeOrder, Bridge) ? "1" : "2";

            //数量需要过滤已取消的单子
            newBridgeOrder.bridgeSeq = bridgeOrderList.Count(_ => _.BidBridgeStateShow != "已取消" && _.OfrBridgeStateShow != "已取消") + 1;

            BridgeUIDomain.UpdateShowProperty(newBridgeOrder, Bridge);

            BridgeUIDomain.SetImagePath(newBridgeOrder, orderDetail, Bridge);
            newBridgeOrder.OrderGroup = 1;

            ///获取当前桥下的唯一订单ID
            newBridgeOrder.MasterID = BridgeUIDomain.GetBOrderIdByBridge(newBridgeOrder, Bridge);

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                //保证桥里订单唯一
                if (!bridgeOrderList.Any(_ => _.ExecID.Equals(newBridgeOrder.ExecID)))
                    bridgeOrderList.Add(newBridgeOrder);
            }));
        }
        private void AddOrderEx(TransOrderServerData orderDetail, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            if (orderDetail.OrdStatus == "D")
                return;
            UserBridge Bridge = bridge.Key;
            ObservableCollection<BridgeOrderDetail> bridgeOrderList = bridge.Value;
            BridgeOrderDetail newBridgeOrder = ObjConvertHelper.CopyToEx2(orderDetail);
            newBridgeOrder.isBidBridge = BridgeUIDomain.IsBidBridge(newBridgeOrder, Bridge) ? "1" : "2";

            //数量需要过滤已取消的单子
            newBridgeOrder.bridgeSeq = bridgeOrderList.Count(_ => _.BidBridgeStateShow != "已取消" && _.OfrBridgeStateShow != "已取消") + 1;

            BridgeUIDomain.UpdateShowProperty(newBridgeOrder, Bridge);

            BridgeUIDomain.SetImagePathEx(newBridgeOrder, orderDetail, Bridge);
            newBridgeOrder.OrderGroup = 1;

            ///获取当前桥下的唯一订单ID
            newBridgeOrder.MasterID = BridgeUIDomain.GetBOrderIdByBridge(newBridgeOrder, Bridge);

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                //保证桥里订单唯一
                if (!bridgeOrderList.Any(_ => _.ExecID.Equals(newBridgeOrder.ExecID)))
                    bridgeOrderList.Add(newBridgeOrder);
            }));
        }
        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="oldOrder"></param>
        /// <param name="newOrder"></param>
        /// <param name="bridge"></param>
        private void UpdateOrder(BridgeOrderDetail oldOrder, OrderDetailPo newOrder, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            UserBridge Bridge = bridge.Key;
            List<BridgeOrderDetail> bridgeOrderList = bridge.Value.ToList();

            filterOrdersDic[Bridge] = GetFilterOrder(Bridge);

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                BridgeUIDomain.UpdateOrder(BridgeUIDomain.FilterOrder(bridgeOrderList, !OrderHelper.TodayReceipt(oldOrder)),
                    oldOrder, newOrder, Bridge);
                // 重置序号，支持更新后自动智能排序
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(Bridge, filterOrdersDic[Bridge]));
                refreshBrigeOrdersQueue.Add(1);
            }));
        }

        private void UpdateOrderEx(BridgeOrderDetail oldOrder, TransOrderServerData newOrder, KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridge)
        {
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            LogHelper.Info(newOrder.OfrIssueInfoReqList);
            UserBridge Bridge = bridge.Key;
            List<BridgeOrderDetail> bridgeOrderList = bridge.Value.ToList();

            filterOrdersDic[Bridge] = GetFilterOrder(Bridge);

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                BridgeUIDomain.UpdateOrderEx(BridgeUIDomain.FilterOrder(bridgeOrderList, !OrderHelper.TodayReceipt(oldOrder)),
                    oldOrder, newOrder, Bridge);
                // 重置序号，支持更新后自动智能排序
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(Bridge, filterOrdersDic[Bridge]));
                refreshBrigeOrdersQueue.Add(1);
            }));
            stopwatch2.Stop();
            LogHelper.Info("===========费时间UUUU333333：" + oldOrder.seq + "订单运行时间    " + stopwatch2.ElapsedMilliseconds);
        }

        #endregion

        #region 断线重连
        /// <summary>
        /// 断线重连的时候重新加载过桥数据
        /// </summary>
        public void OnClear()
        {
            this.Dispatcher.Invoke(() =>
            {
                isloaded = false;
                BridgeDatasDic.Clear();
                bridgeDataList.Clear();
                //判断有没有桥
                if (UserBridgeHandler.Instance.Bridges != null)
                {
                    //查看初始化
                    reviewsInit();
                    var authorizedNames = IDCHelper.GetAuthorizedUserName();
                    List<TransOrderServerData> allOrders = BridgeHelper.getCacheList();//OrderHandler.Instance.GetAllOrder(true);
                    foreach (UserBridge bridge in UserBridgeHandler.Instance.Bridges)
                    {
                        //获取桥和桥订单
                        BridgeDatasDic.Add(bridge, GetDataByBridgeEx(bridge, allOrders, authorizedNames));
                        LogHelper.Info("OnClear BridgeUI init {0}({1})；桥下订单数量：{2}", bridge.bankInstitutionname, bridge.bankTradername, BridgeDatasDic[bridge].Count());
                        UpdateBridgeDataList("1", bridge, null);
                    }

                    //保留上次记忆功能
                    ResetBridgeBySaveIni();
                    lsBridge.ItemsSource = bridgeDataList;

                    //默认选中第一个桥
                    lsBridge.SelectedIndex = 0;
                    isloaded = true;
                    //数据过滤
                    filterOrder();
                    //过滤查看
                    filterReviews();
                }
            });
        }
        #endregion

        #region 其他功能
        /// <summary>
        /// 获取桥数据
        /// </summary>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public ObservableCollection<BridgeOrderDetail> GetDataByBridgeEx(UserBridge bridge, List<TransOrderServerData> allOrders, HashSet<string> authorizedNames)
        {
            var hasOtherOrderConfig = GRSettingsHandler.HasPermissonForOtherBridgeOrder();
            List<BridgeOrderDetail> bridgeOrderList = new List<BridgeOrderDetail>();
            List<TransOrderServerData> normalOrderList = new List<TransOrderServerData>();

            //过滤出当前桥下订单
            List<TransOrderServerData> temOrders = new List<TransOrderServerData>();
            foreach (TransOrderServerData orderDetail in allOrders)
            {
                // 缺少桥专员判断
                if (SystemPowerHandler.Instance.IsBridge)
                {
                    if (((!string.IsNullOrEmpty(orderDetail.BridgeType) && !string.IsNullOrEmpty(orderDetail.BridgeTraderId) && !string.IsNullOrEmpty(orderDetail.BridgeInstitutionId)
                        && orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                        || (!string.IsNullOrEmpty(orderDetail.BridgeType2) &&
                        !string.IsNullOrEmpty(orderDetail.BridgeTraderId2) && orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) &&
                        !string.IsNullOrEmpty(orderDetail.BridgeInstitutionId2) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))))
                    {
                        //过滤掉删除的单子
                        if (!"D".Equals(orderDetail.OrdStatus))
                        {
                            temOrders.Add(orderDetail);
                        }
                    }
                }
                else
                {
                    var isMyOrder = SystemPowerHandler.Instance.IsCanEditOrder(orderDetail.PartySubID) || SystemPowerHandler.Instance.IsCanEditOrder(orderDetail.CounterPartySubID);
                    var checkResult = false;
                    if (isMyOrder)
                    {
                        checkResult = OrderHelper.IsSpecialBridgeOrder(orderDetail, bridge);
                    }
                    else
                    {
                        if (SystemPowerHandler.Instance.HasOtherPermision(orderDetail, bridge))
                        {
                            if ((orderDetail.BridgeOrderModel != null && orderDetail.BridgeOrderModel.instId == bridge.bankInstitutionid && orderDetail.BridgeOrderModel.instTradeId == bridge.bankTraderid)
                                ||
                                (orderDetail.BridgeOrderModel2 != null && orderDetail.BridgeOrderModel2.instId == bridge.bankInstitutionid && orderDetail.BridgeOrderModel2.instTradeId == bridge.bankTraderid))
                            {
                                checkResult = true;
                            }
                        }

                    }

                    //过滤掉删除的单子
                    if (checkResult && !"D".Equals(orderDetail.OrdStatus))
                    {
                        temOrders.Add(orderDetail);
                    }
                }
            }

            //判断单子的桥方向
            foreach (TransOrderServerData orderDetail in temOrders)
            {
                // orderDetail.isBidBridge = BridgeUIDomain.IsBidBridgeEx(orderDetail, bridge) ? "1" : "2";
                normalOrderList.Add(orderDetail);
            }


            ///获取桥下所有未查看的单子
            string key = bridge.bankInstitutionid + "-" + bridge.bankTraderid;
            List<BridgeReserveReviews> BridgeReviews;
            BridgeReviewsDic.TryGetValue(key, out BridgeReviews);
            //如果桥下有单子初始化过桥单 20200422
            if (normalOrderList.Count > 0)
            {
                foreach (TransOrderServerData od in normalOrderList)
                {
                    BridgeOrderDetail orderDetail = BridgeUIDomain.CreateBridgeOrderDetailEx(od, bridge);
                    bridgeOrderList.Add(orderDetail);
                    ///如果单子未查看，则更新桥订单查看状态为未查看
                    if (BridgeReviews != null)
                    {
                        foreach (BridgeReserveReviews review in BridgeReviews)
                        {
                            if (review.borderId == orderDetail.MasterID)
                            {
                                orderDetail.BridgeisChecked = EBridgeisChecked.未查看;
                                orderDetail.Reviewid = review.reviewId;
                            }
                        }
                    }
                }
            }
            List<BridgeOrderDetail> temList = bridgeOrderList.ToList();
            temList.Sort(new BridgeNormalSort());
            //生成单号和桥文本
            int i = 1;
            foreach (BridgeOrderDetail od in temList)
            {
                if (od.OfrBridgeStateShow == "已取消" && od.BidBridgeStateShow == "已取消")
                {
                    od.OldOrderTxt = BridgeHelper.CreateBridgeTxt(od, BridgeUIDomain.GetBridgeType(od), bridge);
                    od.OrderTxt = BridgeHelper.GetBridgeTxt(od, bridge, od.OldOrderTxt);
                    continue;
                }
                od.bridgeSeq = i++;
                od.OldOrderTxt = string.Format("{0}  {1}", od.ShowSeq, BridgeHelper.CreateBridgeTxt(od, BridgeUIDomain.GetBridgeType(od), bridge));
                od.OrderTxt = BridgeHelper.GetBridgeTxt(od, bridge, od.OldOrderTxt);
            }
            ObservableCollection<BridgeOrderDetail> result = new ObservableCollection<BridgeOrderDetail>(temList);
            ///集合改变更新日期过滤数据
            result.CollectionChanged += (sender, e) => { BridgeCollectionChanged(bridge, e); };
            return result;
        }
        /// <summary>
        /// 集合更改，更新日期过滤集合
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="e"></param>
        void BridgeCollectionChanged(UserBridge bridge, NotifyCollectionChangedEventArgs e)
        {
            if (!filterOrdersDic.ContainsKey(bridge))
                return;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                List<BridgeOrderDetail> filterOrders = new List<BridgeOrderDetail>();
                foreach (BridgeOrderDetail orderDetail in e.NewItems)
                {
                    filterOrders.Add(orderDetail);
                }
                if (condition.Self)
                {
                    filterOrders = filterOrders.Where(_ => selfJudgment(_)).ToList();
                }
                if (condition.DealDate)
                {
                    filterOrders = filterOrders.Where(_ => dealDateJudgment(_)).ToList();
                }
                if (condition.EntryDate)
                {
                    filterOrders = filterOrders.Where(_ => entryDateJudgment(_)).ToList();
                }
                if (!string.IsNullOrEmpty(condition.BridgeOrderOfBroker))
                {
                    filterOrders = FilterOtherOrders(filterOrders, condition.BridgeOrderOfBroker);
                }

                //if (!string.IsNullOrWhiteSpace(condition.Price)) {
                //    filterOrders = filterOrders.Where(_=>_.price==Convert.ToDecimal(condition.Price.Trim())).ToList();
                //}
                foreach (BridgeOrderDetail orderDetail in filterOrders)
                {
                    if (!filterOrdersDic[bridge].Contains(orderDetail))
                        filterOrdersDic[bridge].Add(orderDetail);
                }
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
                //lsBridge_SelectionChanged(null, null);
                refreshBrigeOrdersQueue.Add(1);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BridgeOrderDetail orderDetail in e.OldItems)
                {
                    if (filterOrdersDic[bridge].Contains(orderDetail))
                        filterOrdersDic[bridge].Remove(orderDetail);
                }

                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
                //lsBridge_SelectionChanged(null, null);
                refreshBrigeOrdersQueue.Add(1);
            }

        }

        static bool NeedSort = false;

        private bool _isMaturityDaySort = false;
        private void SortingData(UserBridge bridge)
        {
            //智能排序
            if (NeedSort)
            {
                if (filterOrdersDic.ContainsKey(bridge))
                    filterOrdersDic[bridge].Sort(new BridgeGroupSort());
            }
            else
            {
                if (filterOrdersDic.ContainsKey(bridge))
                    filterOrdersDic[bridge].Sort(new BridgeNormalSort());
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 保证加桥窗口只打开一个
        /// </summary>
        private bool IsAddBridgeShow = false;
        /// <summary>
        /// 添加桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddBridge_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAddBridgeShow)
            {
                AddBridgeWin addBridgeWin = new AddBridgeWin();
                addBridgeWin.Closed += AddBridgeWin_Closed;
                addBridgeWin.OnAddBridge += AddBridgeWin_OnAddBridge;

                var win = Window.GetWindow(this);
                addBridgeWin.Owner = win;
                //if (win == null)
                //    addBridgeWin.Owner = Application.Current.MainWindow;
                //else
                //    addBridgeWin.Owner = win;
                IsAddBridgeShow = true;
                addBridgeWin.Show();
            }
        }
        /// <summary>
        /// 添加桥回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="obj"></param>
        /// <param name="oldBridge"></param>
        /// <param name="isUpdate"></param>
        /// <param name="needReset"></param>
        private void AddBridgeWin_OnAddBridge(object sender, UserBridge obj, UserBridge oldBridge, bool isUpdate, bool needReset)
        {
            if (!isUpdate)
            {
                var authorizedNames = IDCHelper.GetAuthorizedUserName();
                List<TransOrderServerData> allOrders = BridgeHelper.getCacheList();//OrderHandler.Instance.GetAllOrder(true);

                //获取桥和桥订单
                BridgeDatasDic.Add(obj, GetDataByBridgeEx(obj, allOrders, authorizedNames));
                //更新桥列表
                UpdateBridgeDataList("1", obj, null);
                //新添加的桥需要数据过滤
                filterOrdersDic[obj] = GetFilterOrder(obj);
            }
        }
        /// <summary>
        /// 添加桥窗口关闭处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBridgeWin_Closed(object sender, EventArgs e)
        {
            IsAddBridgeShow = false;
            AddBridgeWin addBridgeWin = sender as AddBridgeWin;
            if (addBridgeWin != null)
            {
                addBridgeWin.Closed -= AddBridgeWin_Closed;
                addBridgeWin.OnAddBridge -= AddBridgeWin_OnAddBridge;
            }
        }
        /// <summary>
        /// 编辑桥回调处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newBridge"></param>
        /// <param name="oldBridge"></param>
        private void BridgeUI_OnUpdateBridge(object sender, UserBridge newBridge, UserBridge oldBridge)
        {
            UpdateBridgeDataList("3", newBridge, oldBridge);
        }
        /// <summary>
        /// 删除桥回调处理
        /// </summary>
        /// <param name="bridgeUI"></param>
        private void BridgeUI_OnDeleteBridge(UserBridge bridge)
        {
            BridgeDatasDic.Remove(bridge);
            filterOrdersDic.Remove(bridge);
            //删除桥，不需要在加载此桥数据
            UpdateBridgeDataList("2", bridge, null);
        }

        /// <summary>
        /// 事件——关闭过桥窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //IniFileHelper.WriteValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), WinHeight, this.Height + "");
            //IniFileHelper.WriteValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), WinWidth, this.Width + "");

            //if (LocalEnv.ConfInfo.ExecPosition)
            //{
            //    IniFileHelper.WriteValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), PositionX, this.Left + "");
            //    IniFileHelper.WriteValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), PositionY, this.Top + "");
            //}
            //保留桥列表顺序
            SaveBridgeOrderListSeq();
            //this.Close();
            //this.Hide();
        }

        /// <summary>
        /// 记忆桥顺序
        /// </summary>
        private void SaveBridgeOrderListSeq()
        {
            if (bridgeDataList != null && bridgeDataList.Count > 0)
            {
                string orderList = string.Empty;
                int i = 0;
                foreach (UserBridge item in bridgeDataList)
                {
                    i++;
                    orderList += item.bridgeId + "-" + i + "|";
                }
                orderList = orderList.TrimEnd('|');
                IniFileHelper.WriteValue(string.Format("{0}_{1}", BridgeWinName, LocalEnv.UserInfo.UserId), BridgeOrder, orderList);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            btnClose_Click(null, null);
        }
        /// <summary>
        /// 排序选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSort_Click(object sender, RoutedEventArgs e)
        {
            NeedSort = cbSort.IsChecked.Value;
            if (NeedSort)
            {
                cb_Term.IsChecked = false;
                _isMaturityDaySort = false;
            }

            UserBridge bridge = getCurrentBridge();
            if (bridge != null)
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
            lsBridge_SelectionChanged(null, null);
        }
        /// <summary>
        /// 拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // this.DragMove();
        }
        /// <summary>
        /// 事件——导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "word文件（*.doc）|*.doc";
            if ((bool)saveFileDialog.ShowDialog())
            {
                string fileName = saveFileDialog.FileName;
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridgeData in filterOrdersDic)
                {
                    UserBridge bridge = bridgeData.Key;
                    List<BridgeOrderDetail> orders = bridgeData.Value;
                    sb.Append(string.Format("====={0}({1})  {2}=====\r\n\r\n", bridge.bankInstitutionname, bridge.bankTradername, bridge.sendto));
                    foreach (BridgeOrderDetail order in orders)
                    {
                        if (order != null && order.BidBridgeStateShow != "已取消" && order.OfrBridgeStateShow != "已取消")
                            sb.Append(string.Format("{0}\r\n", BridgeHelper.CopyBridgeInfo(order)));
                    }
                    sb.Append("\r\n");
                }
                ThreadPool.QueueUserWorkItem(o =>
                {
                    bool status = true;
                    try
                    {
                        FileStream fileStream = new FileStream(fileName, FileMode.Create);
                        using (fileStream)
                        {
                            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                            fileStream.Write(fileBytes, 0, fileBytes.Length);
                            fileStream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                        status = false;
                    }
                    if (!status)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            SysWarnGUI warn = new SysWarnGUI();
                            Window win = Window.GetWindow(this);
                            if (win != null)
                                warn.Owner = win;
                            warn.tbTitle.Text = "错误提示";
                            warn.tbMsg.Text = "导出失败";
                            warn.ShowDialog();
                        }));
                    }
                });
            }
        }
        #endregion

        #region 桥定位
        /// <summary>
        /// 桥列表
        /// </summary>
        private ObservableCollection<UserBridge> bridgeDataList = new ObservableCollection<UserBridge>();
        /// <summary>
        /// 桥列表 增，删 ，改 
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="userBridge"></param>
        /// <param name="oldBridge"></param>
        private void UpdateBridgeDataList(string actionType, UserBridge userBridge, UserBridge oldBridge)
        {

            if (actionType == "1")//添加
            {
                bridgeDataList.Add(new UserBridge()
                {
                    bridgeId = userBridge.bridgeId,
                    //桥
                    bankInstitutionid = userBridge.bankInstitutionid,
                    bankInstitutionname = userBridge.bankInstitutionname,
                    bankTraderid = userBridge.bankTraderid,
                    bankTradername = userBridge.bankTradername,
                    //计费
                    feeTraderid = userBridge.feeTraderid,
                    feeTradername = userBridge.feeTradername,
                    feeInstitutionid = userBridge.feeInstitutionid,
                    feeInstitutionname = userBridge.feeInstitutionname,
                    //发给，渠道，备注，联系方式
                    sendto = userBridge.sendto,
                    channel = userBridge.channel,
                    remark = userBridge.remark,
                    contactType = userBridge.contactType,

                    ExecOrderCount = BridgeDatasDic[userBridge].Count(_ => _.OfrBridgeStateShow != "已取消" && _.BidBridgeStateShow != "已取消").ToString(),
                });
                if (lsBridge.Items.Count > 0 && lsBridge.SelectedIndex == -1)
                    lsBridge.SelectedIndex = 0;
                //桥列不为空，则桥面板显示
                if (lsBridge.Items.Count == 1)
                    BridgeUI.bridgeInfoUI.Visibility = Visibility.Visible;
            }
            else if (actionType == "2")//删除
            {
                UserBridge removeBridge = null;
                foreach (UserBridge bridgeData in bridgeDataList)
                {
                    if (bridgeData.bankInstitutionid + "" == userBridge.bankInstitutionid + "" && bridgeData.bankTraderid + "" == userBridge.bankTraderid + "")
                    {
                        removeBridge = bridgeData;
                        break;
                    }
                }
                ///删除后，桥列表选中处理
                if (removeBridge != null)
                {
                    int index = lsBridge.Items.IndexOf(removeBridge);
                    bridgeDataList.Remove(removeBridge);

                    if (index > lsBridge.Items.Count - 1)
                        lsBridge.SelectedIndex = lsBridge.Items.Count - 1;
                    else
                        lsBridge.SelectedIndex = index;
                    //桥列表为空，则桥面板不显示
                    if (lsBridge.Items.Count == 0)
                        BridgeUI.bridgeInfoUI.Visibility = Visibility.Hidden;
                }
            }
            else if (actionType == "3")//修改
            {
                foreach (UserBridge bridgeData in bridgeDataList)
                {
                    if (bridgeData.bankInstitutionid + "" == oldBridge.bankInstitutionid + "" && bridgeData.bankTraderid + "" == oldBridge.bankTraderid + "")
                    {
                        bridgeData.Sendto = userBridge.Sendto;
                        bridgeData.channel = userBridge.channel;
                        bridgeData.remark = userBridge.remark;
                        bridgeData.contactType = userBridge.contactType;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 桥搜索框键盘处理——回车确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTraderAInfoChange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dgTraderA_MouseUp(null, null);
            }
        }
        /// <summary>
        /// 桥搜索框键盘处理——上下移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTraderAInfoChange_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgTraderA.SelectedIndex > 0)
                    dgTraderA.SelectedIndex--;
                dgTraderA.ScrollIntoView(dgTraderA.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgTraderA.SelectedIndex < dgTraderA.Items.Count - 1)
                    dgTraderA.SelectedIndex++;
                dgTraderA.ScrollIntoView(dgTraderA.SelectedItem);
            }
        }
        private bool TextChangeFlag = false;
        /// <summary>
        /// 事件——搜索桥文本改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTraderAInfoChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChangeFlag)
                return;


            if (!string.IsNullOrEmpty(tbTraderAInfoChange.Text))
            {
                List<UserBridge> dataList = bridgeDataList.Where(delegate (UserBridge bridgeData)
                {
                    if (bridgeData.InstTraderShow.Contains(tbTraderAInfoChange.Text))
                        return true;
                    else
                        return false;
                }).ToList();

                if (dataList.Count > 0)
                {
                    dgTraderA.DataContext = dataList;
                    if (!popTraderA.IsOpen)
                        popTraderA.IsOpen = true;
                    dgTraderA.SelectedIndex = 0;
                }
                else
                {
                    dgTraderA.DataContext = null;
                    popTraderA.IsOpen = false;
                }
            }
            else
            {
                dgTraderA.DataContext = null;
                popTraderA.IsOpen = false;
            }
        }

        /// <summary>
        /// 事件——选择搜索桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTraderA_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UserBridge dv = dgTraderA.SelectedItem as UserBridge;
            popTraderA.IsOpen = false;
            if (dv != null)
            {
                TextChangeFlag = true;
                tbTraderAInfoChange.Text = dv.InstTraderShow;
                TextChangeFlag = false;
                UserBridge Bridge = null;
                foreach (UserBridge bridge in lsBridge.Items)
                {

                    if (bridge != null && bridge.bankInstitutionid + "" == dv.bankInstitutionid + "" && bridge.bankTraderid + "" == dv.bankTraderid + "")
                    {
                        Bridge = bridge;
                        break;
                    }
                }
                if (Bridge != null)
                {
                    //选中此桥
                    int index = lsBridge.Items.IndexOf(Bridge);
                    lsBridge.SelectedIndex = index;
                    lsBridge.ScrollIntoView(lsBridge.SelectedItem);
                }
            }
        }
        /// <summary>
        /// 事件——选择桥改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lsBridge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isloaded)
                return;
            UserBridge bridge = getCurrentBridge();
            if (bridge != null)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    //SortOrder(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
                    BridgeUI.RefreshData(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
                    BridgeUI.bridgeInfoUI.tbOrderId.Text = "";
                }));
            }
            if (lsBridge.SelectedItem != null)
            {
                ListBoxItem childControl1 = lsBridge.ItemContainerGenerator.ContainerFromItem(lsBridge.SelectedItem) as ListBoxItem;
                for (int i = 0; i < lsBridge.Items.Count; i++)
                {
                    ListBoxItem childControl = lsBridge.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (childControl != null && childControl1 != null)
                    {
                        if (ReferenceEquals(childControl, childControl1))
                        {
                            childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00CDAD"));//Brushes.Yellow;
                        }
                        else
                        {
                            childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前选择的桥
        /// </summary>
        /// <returns></returns>
        private UserBridge getCurrentBridge()
        {
            UserBridge dv = lsBridge.SelectedItem as UserBridge;
            UserBridge Bridge = null;
            popTraderA.IsOpen = false;
            if (dv != null)
            {
                TextChangeFlag = true;
                //tbTraderAInfoChange.Text = dv.InstTraderName;
                TextChangeFlag = false;
                foreach (UserBridge bridge in BridgeDatasDic.Keys)
                {
                    if (bridge != null && bridge.bankInstitutionid + "" == dv.bankInstitutionid + "" && bridge.bankTraderid + "" == dv.bankTraderid + "")
                    {
                        Bridge = bridge;
                        break;
                    }
                }
            }
            return Bridge;
        }
        #endregion

        #region 过滤和定位功能
        /// <summary>
        /// 过滤后订单
        /// </summary>
        public static Dictionary<UserBridge, List<BridgeOrderDetail>> filterOrdersDic = new Dictionary<UserBridge, List<BridgeOrderDetail>>();
        /// <summary>
        /// 待定位的订单容器
        /// </summary>
        public static Dictionary<UserBridge, List<BridgeOrderDetail>> positioningOrdersDic = new Dictionary<UserBridge, List<BridgeOrderDetail>>();
        /// <summary>
        /// 当前定位桥
        /// </summary>
        UserBridge currentPositioningBridge = null;
        /// <summary>
        /// 当前定位订单位置
        /// </summary>
        int currentPositioningOrder = -1;
        /// <summary>
        /// 当前搜索序号
        /// </summary>
        string currentSeq;
        /// <summary>
        /// 当前搜索债券
        /// </summary>
        string currentBond;
        /// <summary>
        /// 当前搜索序号
        /// </summary>
        string currentPrice;
        /// <summary>
        /// 设置需要定位的订单
        /// </summary>
        void SetPositioningOrders(EFilterType FilterType)
        {
            positioningOrdersDic.Clear();
            foreach (KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridgeData in filterOrdersDic)
            {
                UserBridge bridge = bridgeData.Key;
                List<BridgeOrderDetail> orders = bridgeData.Value;
                if (bridge != null)
                {
                    if (FilterType == EFilterType.编号 && !string.IsNullOrEmpty(currentSeq))
                        positioningOrdersDic.Add(bridge, GetFilterOrder(bridge, FilterType, currentSeq));
                    if (FilterType == EFilterType.债券 && !string.IsNullOrEmpty(currentBond))
                        positioningOrdersDic.Add(bridge, GetFilterOrder(bridge, FilterType, currentBond));
                    if (FilterType == EFilterType.Price && !string.IsNullOrEmpty(currentPrice))
                    {
                        positioningOrdersDic.Add(bridge, GetFilterOrder(bridge, FilterType, currentPrice));
                    }

                }
            }
            if (positioningOrdersDic.Count > 0)

            {
                currentPositioningBridge = null;
                List<UserBridge> bridges = positioningOrdersDic.Keys.ToList();
                foreach (UserBridge bridge in bridges)
                {
                    List<BridgeOrderDetail> orders = null;
                    if (bridge != null)
                    {
                        positioningOrdersDic.TryGetValue(bridge, out orders);
                    }
                    if (orders != null && orders.Count > 0)
                    {
                        currentPositioningBridge = bridge;
                        currentPositioningOrder = 0;
                        break;
                    }

                }
            }
        }


        /// 获取筛选数据(定位用)
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="filterOrderId"></param>
        /// <returns></returns>
        public List<BridgeOrderDetail> GetFilterOrder(UserBridge Bridge, EFilterType filterType = EFilterType.全部, string filterOrderId = null)
        {

            ObservableCollection<BridgeOrderDetail> orders;
            BridgeDatasDic.TryGetValue(Bridge, out orders);
            List<BridgeOrderDetail> filterOrders;
            filterOrdersDic.TryGetValue(Bridge, out filterOrders);
            List<BridgeOrderDetail> filterResult = new List<BridgeOrderDetail>();


            if (string.IsNullOrEmpty(filterOrderId))
            {

                if (filterType == EFilterType.全部)
                {
                    filterResult = orders.ToList();
                }

            }
            else
            {
                if (filterType == EFilterType.债券)
                {
                    filterResult = filterOrders.Where(detail => detail.SecurityID.Contains(filterOrderId)).ToList();
                }

                if (filterType == EFilterType.日期)
                {
                    //filterResult = orders.Where(detail => detail.dealdate == filterOrderId || detail.dealdate2 == filterOrderId).ToList();
                    //添加桥的时间
                    foreach (BridgeOrderDetail orderDetail in orders)
                    {
                        if (!string.IsNullOrEmpty(orderDetail.BridgeCreateTime) && orderDetail.BridgeCreateTime.Count() > 10 && orderDetail.BridgeCreateTime.Substring(0, 10) == filterOrderId)
                        {
                            if (!filterResult.Contains(orderDetail))
                                filterResult.Add(orderDetail);
                        }
                        if (!string.IsNullOrEmpty(orderDetail.BridgeCreateTime2) && orderDetail.BridgeCreateTime2.Count() > 10 && orderDetail.BridgeCreateTime2.Substring(0, 10) == filterOrderId)
                        {
                            if (!filterResult.Contains(orderDetail))
                                filterResult.Add(orderDetail);
                        }
                    }
                }
                if (filterType == EFilterType.全部)
                {
                    filterResult = orders.ToList();
                }

                if (filterType == EFilterType.编号)
                {
                    filterResult = filterOrders.Where(delegate (BridgeOrderDetail orderDetail)
                    {
                        if (filterOrderId.Equals(orderDetail.seq.Value + ""))
                            return true;
                        else
                            return false;

                    }).ToList();
                }

                if (filterType == EFilterType.Price)
                {
                    filterResult = filterOrders.Where(delegate (BridgeOrderDetail orderDetail)
                    {
                        decimal decPrice;
                        if (decimal.TryParse(filterOrderId, out decPrice))
                        {
                            if (decPrice == orderDetail.price.Value)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }

                    }).ToList();
                }


            }
            return filterResult;
        }

        /// 获取筛选数据(过滤用)
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="filterOrderId"></param>
        /// <returns></returns>
        public List<BridgeOrderDetail> GetFilterOrder(UserBridge Bridge)
        {
            ObservableCollection<BridgeOrderDetail> orders;
            BridgeDatasDic.TryGetValue(Bridge, out orders);
            List<BridgeOrderDetail> filterOrders = orders.ToList();

            if (condition.Self)
            {
                filterOrders = filterOrders.Where(_ => selfJudgment(_)).ToList();
            }
            if (condition.DealDate)
            {
                filterOrders = filterOrders.Where(_ => dealDateJudgment(_)).ToList();
            }
            if (condition.EntryDate)
            {
                filterOrders = filterOrders.Where(_ => entryDateJudgment(_)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(condition.BridgeOrderOfBroker))
            {
                filterOrders = FilterOtherOrders(filterOrders, condition.BridgeOrderOfBroker);
            }
            return filterOrders;
        }

        public List<BridgeOrderDetail> FilterOtherOrders(List<BridgeOrderDetail> bridgeOrders, string otherCondition)
        {
            var filterOrders = bridgeOrders;
            if (otherCondition == "1")
            {
                filterOrders = bridgeOrders.Where(_ => SystemPowerHandler.Instance.IsCanLookOrder(_.PartySubName) || SystemPowerHandler.Instance.IsCanLookOrder(_.CounterPartySubName)).ToList();
            }
            else if (otherCondition == "2")
            {
                if (SystemPowerHandler.Instance.IsOrtherBridge)
                {
                    filterOrders = bridgeOrders.Where(_ => !(SystemPowerHandler.Instance.IsCanLookOrder(_.PartySubName) || SystemPowerHandler.Instance.IsCanLookOrder(_.CounterPartySubName))).ToList();
                }
                else
                {
                    filterOrders = new List<BridgeOrderDetail>();
                }
            }
            return filterOrders;
        }

        /// <summary>
        /// 录入日判断
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        bool entryDateJudgment(BridgeOrderDetail orderDetail)
        {
            if (condition.Date == null)
                return false;
            if (!string.IsNullOrEmpty(orderDetail.BridgeCreateTime) && orderDetail.BridgeCreateTime.Count() > 10 && orderDetail.BridgeCreateTime.Substring(0, 10) == condition.Date)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(orderDetail.BridgeCreateTime2) && orderDetail.BridgeCreateTime2.Count() > 10 && orderDetail.BridgeCreateTime2.Substring(0, 10) == condition.Date)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 成交日判断
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        bool dealDateJudgment(BridgeOrderDetail orderDetail)
        {
            if (condition.Date == null)
                return false;
            if (!string.IsNullOrEmpty(orderDetail.dealdate) && orderDetail.dealdate.Count() >= 8 && orderDetail.dealdate == condition.Date.Replace("-", ""))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(orderDetail.DealDate2) && orderDetail.DealDate2.Count() >= 8 && orderDetail.DealDate2 == condition.Date.Replace("-", ""))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 自己添加的桥判断
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        bool selfJudgment(BridgeOrderDetail orderDetail)
        {
            bridgeOrder border;
            if (orderDetail.BridgeType == "B,O")
            {
                border = orderDetail.bridgeOrder;
            }
            else
            {
                border = orderDetail.isBidBridge == "1" ? orderDetail.bridgeOrder2 : orderDetail.bridgeOrder;
            }

            if (border != null && border.createBy == LocalEnv.UserInfo.FullName)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 计算下一个匹配项
        /// </summary>
        void gotoNextMatching()
        {
            ///定位订单
            List<BridgeOrderDetail> orders = null;
            if (currentPositioningBridge != null)
            {
                positioningOrdersDic.TryGetValue(currentPositioningBridge, out orders);
            }

            if (currentPositioningOrder >= 0 && orders != null && orders.Count > 0)
            {
                UserBridge Bridge = null;
                foreach (UserBridge bridge in lsBridge.Items)
                {
                    if (bridge != null && bridge.bankInstitutionid + "" == (currentPositioningBridge.bankInstitutionid) && bridge.bankTraderid + "" == (currentPositioningBridge.bankTraderid))
                    {
                        Bridge = bridge;
                        break;
                    }
                }
                if (Bridge != null)
                {
                    //定位桥
                    int index = lsBridge.Items.IndexOf(Bridge);
                    lsBridge.SelectedIndex = index;
                    lsBridge.ScrollIntoView(Bridge);
                }
                //定位单子
                BridgeUI.gotoOrder(orders[currentPositioningOrder]);

                //如果当前桥下还有订单未匹配，则定位之
                if (currentPositioningOrder < orders.Count - 1)
                {
                    currentPositioningOrder++;
                }
                else
                {
                    //设置下一个桥为当前桥
                    setNextBridge();
                    //如果当前桥下面没有匹配项，则继续设置下一个桥为当前桥
                    while (!isHaveMatching())
                    {
                        setNextBridge();
                    }
                }
            }
        }
        /// <summary>
        /// 判断当前桥是否有匹配项
        /// </summary>
        /// <returns></returns>
        private bool isHaveMatching()
        {
            List<BridgeOrderDetail> orders = null;
            positioningOrdersDic.TryGetValue(currentPositioningBridge, out orders);
            if (orders != null && orders.Count > 0)
            {
                return true;
            }
            else
                return false;

        }
        /// <summary>
        /// 设置下一个桥
        /// </summary>
        private void setNextBridge()
        {

            currentPositioningOrder = 0;
            List<UserBridge> bridges = positioningOrdersDic.Keys.ToList();
            int index = bridges.IndexOf(currentPositioningBridge);

            if (index < bridges.Count() - 1)
            {
                currentPositioningBridge = bridges[index + 1];
            }
            else
            {
                currentPositioningBridge = bridges[0];
            }
        }

        #endregion

        #region 序号定位
        /// <summary>
        /// 桥序号键盘预处理——过滤掉空格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
            //SetPositioningOrders(EFilterType.编号);
        }
        /// <summary>
        /// seq输入框事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                gotoNextMatching();
                tbOrderId.Focus();
            }
        }
        /// <summary>
        /// 桥序号预输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;
        }
        /// <summary>
        /// 序号改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderId_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSeq = tbOrderId.Text.ToString();
            SetPositioningOrders(EFilterType.编号);
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderId_GotFocus(object sender, RoutedEventArgs e)
        {
            SetPositioningOrders(EFilterType.编号);
        }
        #endregion

        #region 债券定位
        /// <summary>
        /// 债券代码变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGoodCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbGoodCode.Text))
            {
                string param = tbGoodCode.Text.Replace("'", "");
                dgCodeResult.DataContext = STC.Common.Utils.CommonUtils.GetBondGridData(param, 20); ;//CRMService.Instance.GetBondGridData(param, 20);//CRMDataHandler.Instance.SearchBond2(param);
                dgCodeResult.SelectedIndex = 0;
                tbBCodePop.IsOpen = true;
            }
            else
            {
                tbBCodePop.IsOpen = false;
            }
        }
        /// <summary>
        /// 债券输入框事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGoodCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbBCodePop.IsOpen)
                {
                    dgCodeResult_MouseDoubleClick(null, null);
                }
                gotoNextMatching();
                tbGoodCode.Focus();
            }
        }
        private void tbGoodCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgCodeResult.SelectedIndex > 0)
                    dgCodeResult.SelectedIndex--;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgCodeResult.SelectedIndex < dgCodeResult.Items.Count - 1)
                    dgCodeResult.SelectedIndex++;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
        }
        /// <summary>
        /// 债券代码选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCodeResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgCodeResult.SelectedItem as SimpleBondsInfoPo;
            if (dv != null && !string.IsNullOrEmpty(tbGoodCode.Text))
            {
                tbGoodCode.Text = dv.Short_Name;
                currentBond = dv.Bond_ID;
                tbBCodePop.IsOpen = false;
                SetPositioningOrders(EFilterType.债券);
            }
            tbGoodCode.Focus();
            tbGoodCode.SelectionStart = tbGoodCode.Text.Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGoodCode_GotFocus(object sender, RoutedEventArgs e)
        {
            SetPositioningOrders(EFilterType.债券);
        }
        private void tbBCodePop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbGoodCode.Text))
                tbBCodePop.IsOpen = false;
        }

        #endregion

        #region 日期过滤
        /// <summary>
        /// 日期选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpMaturityDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            condition.Date = dpMaturityDate.SelectedDate?.ToString("yyyy-MM-dd");
            if (condition.EntryDate || condition.DealDate)
                filterOrder();
        }
        /// <summary>
        /// 对所有桥的订单进行过滤
        /// </summary>
        void filterOrder()
        {
            foreach (KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridgeData in BridgeDatasDic)
            {
                UserBridge bridge = bridgeData.Key;
                filterOrdersDic[bridge] = GetFilterOrder(bridge);
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
            }
            lsBridge_SelectionChanged(null, null);
        }


        #endregion

        #region 查看功能
        /// <summary>
        /// 订阅桥状态更新通知
        /// </summary>
        /// <param name="sid"></param>
        private void SubscribeBridgeReview()
        {
            string topic = string.Format("{0}{1}", ApiConsts.Topic_BridgeReview, LocalEnv.UserInfo.UserId);
            GRHandler.Instance.SubscribeBridgeReview(ReviewStateChange);
        }
        /// <summary>
        /// 桥操作状态更新处理 查看
        /// V2.5需求：
        /// 每一次收到桥机构的回复(即桥状态放生改变，或者桥机构催交易双方发前台报价），
        /// 操作记录都会记录一条新的状态，时钟字符会从白色变成红色，且右上角的“查看”的数字会+1
        /// （这里的数字做去重处理：即如果某笔过桥交易，在短时间内发生两次变化，
        /// 且第一次变化尚未被查看时，则只记做一次，“查看”按钮的数字只+1）
        /// </summary>
        /// <param name="obj"></param>
        private void ReviewStateChange(BridgeReserveReviews obj)
        {
            if (obj != null)
            {
                bool isFind = false;
                foreach (KeyValuePair<UserBridge, ObservableCollection<BridgeOrderDetail>> bridgeData in BridgeDatasDic)
                {
                    UserBridge bridge = bridgeData.Key;
                    ObservableCollection<BridgeOrderDetail> orders = bridgeData.Value;
                    if (isFind)
                        break;
                    foreach (BridgeOrderDetail order in orders)
                    {
                        if (order.MasterID == obj.borderId && order.ExecID == obj.execId)
                        {
                            isFind = true;
                            //去重
                            //删除旧查看
                            List<BridgeReserveReviews> reviews = BridgeReviewQue.Where(_ => _.borderId == obj.borderId).ToList();
                            QueueDeleteItems(BridgeReviewQue, reviews);
                            //压入新查看
                            BridgeReviewQue.Enqueue(obj);
                            this.Dispatcher.Invoke(() =>
                            {
                                //修改单子的状态
                                order.Reviewid = obj.reviewId;
                                order.BridgeisChecked = EBridgeisChecked.未查看;
                                //更新计数
                                BridgeUpdateCount = BridgeReviewQue.Count().ToString();
                            });
                            break;
                        }

                    }

                }
            }
        }
        /// <summary>
        /// 机构-查看dic
        /// </summary>
        public static Dictionary<string, List<BridgeReserveReviews>> BridgeReviewsDic = new Dictionary<string, List<BridgeReserveReviews>>();
        /// <summary>
        /// 线程安全队列，用于存储桥状态更新,存取机制：先进先出
        /// 基于V2.5需求
        /// 当点击“查看”时，则会定位到状态发生改变的那笔交易，
        /// 此时“查看”按钮上的红色数字则会相应-1，该条交易要素右边的时钟标识，则会从红色变回为白色，
        /// 再次点击“查看”，则会定位到下一条状态发生改变的交易，
        /// 顺序则按照时间顺序，每次点击“查看”定位到的交易，是状态更新时间最早的。
        /// </summary>
        public static ConcurrentQueue<BridgeReserveReviews> BridgeReviewQue = new ConcurrentQueue<BridgeReserveReviews>();
        private string bridgeUpdateCount = "";
        /// <summary>
        /// 桥操作更新计数 
        /// </summary>
        public string BridgeUpdateCount
        {
            get
            {
                return bridgeUpdateCount;
            }
            set
            {
                bridgeUpdateCount = value;
                NotifyPropertyChanged("BridgeUpdateCount");
            }
        }
        /// <summary>
        /// 点价查看按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            if (BridgeReviewQue.Count == 0)
                return;
            BridgeReserveReviews review = BridgeReviewQue.First();
            if (review != null)
            {
                foreach (KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridgeData in filterOrdersDic)
                {
                    UserBridge bridge = bridgeData.Key;
                    List<BridgeOrderDetail> orders = bridgeData.Value;
                    foreach (BridgeOrderDetail order in orders)
                    {
                        if (order.MasterID == review.borderId && order.BridgeisChecked == EBridgeisChecked.未查看)
                        {
                            //定位到这个单子
                            foreach (UserBridge item in lsBridge.ItemsSource)
                            {
                                if (item.bankInstitutionid == bridge.bankInstitutionid && item.bankTraderid == bridge.bankTraderid)
                                {
                                    lsBridge.SelectedItem = item;
                                    BridgeUI.gotoOrder(order);
                                }
                            }
                            //check这个单子
                            ThreadPool.QueueUserWorkItem(state =>
                            {
                                CheckBridgeRequestPo requstpo = new CheckBridgeRequestPo();
                                requstpo.reviewid = order.Reviewid;
                                requstpo.inst_id = bridge.bankInstitutionid;
                                requstpo.inst_trade_id = bridge.bankTraderid;
                                var reply = GRHandler.Instance.BridgeReviewState(requstpo);
                                if (reply != null && "0000".Equals(reply.Code))
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        //check成功，则改变单子的状态
                                        order.BridgeisChecked = EBridgeisChecked.以查看;
                                        //从队列中移除单子
                                        BridgeReserveReviews temp;
                                        BridgeReviewQue.TryDequeue(out temp);
                                        //更新计数
                                        BridgeUpdateCount = BridgeReviewQue.Count().ToString();
                                    });
                                }
                            });
                            return;
                        }
                    }
                }
                LogHelper.Info("check查看，找不到对应的成交单(reviewId:{0},borderId:{1};execId:{2})", review.reviewId, review.borderId, review.execId);
                //前端找不到这个单子也check掉这个单子，防止查看积压，导致正常的单子无法check。
                ThreadPool.QueueUserWorkItem(state =>
                {
                    CheckBridgeRequestPo requstpo = new CheckBridgeRequestPo();
                    requstpo.reviewid = review.reviewId;
                    requstpo.inst_id = review.instId;
                    requstpo.inst_trade_id = review.instTradeId;
                    var reply = GRHandler.Instance.BridgeReviewState(requstpo);
                    if (reply != null && "0000".Equals(reply.Code))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            //从队列中移除单子
                            BridgeReserveReviews temp;
                            BridgeReviewQue.TryDequeue(out temp);
                            //更新计数
                            BridgeUpdateCount = BridgeReviewQue.Count().ToString();
                        });
                    }
                });
            }
        }
        #endregion
        /// <summary>
        /// 我添加的桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbself_Click(object sender, RoutedEventArgs e)
        {
            condition.Self = cbself.IsChecked.Value;
            filterOrder();
        }

        private void lsBridge_Drop(object sender, DragEventArgs e)
        {
            lsBridge.Cursor = Cursors.Arrow;
            var pos = e.GetPosition(lsBridge);
            var result = VisualTreeHelper.HitTest(lsBridge, pos);
            if (result == null)
            {
                return;
            }
            //查找元数据  
            UserBridge sourcePerson = e.Data.GetData(typeof(UserBridge)) as UserBridge;
            if (sourcePerson == null)
            {
                return;
            }
            //查找目标数据  
            var listBoxItem = VisualHelper.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }
            UserBridge userBridge = listBoxItem.Content as UserBridge;
            if (userBridge != null)
            {
                if (ReferenceEquals(userBridge, sourcePerson))
                {
                    return;
                }
                if (bridgeDataList != null && bridgeDataList.Count > 0 && bridgeDataList.Any(o => o.bridgeId == sourcePerson.bridgeId))
                {
                    bridgeDataList.Remove(sourcePerson);
                    int indext = lsBridge.Items.IndexOf(userBridge);
                    if (indext != -1)
                    {
                        bridgeDataList.Insert(lsBridge.Items.IndexOf(userBridge) + 1, sourcePerson);
                        this.lsBridge.SelectedIndex = indext + 1;
                    }
                }
            }
            if (lsBridge.SelectedItem != null)
            {
                ListBoxItem childControl1 = lsBridge.ItemContainerGenerator.ContainerFromItem(lsBridge.SelectedItem) as ListBoxItem;
                for (int i = 0; i < lsBridge.Items.Count; i++)
                {
                    ListBoxItem childControl = lsBridge.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (childControl != null && childControl1 != null)
                    {
                        if (ReferenceEquals(childControl, childControl1))
                        {
                            childControl.Foreground = Brushes.Yellow;
                        }
                        else
                        {
                            childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBC8"));
                        }
                    }
                }
            }

            SaveBridgeOrderListSeq();
        }

        AdornerLayer mAdornerLayer = null;
        ListBoxItem boxItem = null;
        private void lsBridge_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxItem listBoxItem = sender as ListBoxItem;
                if (listBoxItem == null || listBoxItem.Content != lsBridge.SelectedItem)
                {
                    return;
                }
                DragDropAdorner adorner = new DragDropAdorner(listBoxItem);
                UserBridge userBridge = listBoxItem.Content as UserBridge;
                if (userBridge != null)
                {
                    mAdornerLayer = AdornerLayer.GetAdornerLayer(lsBridge);
                    mAdornerLayer.Add(adorner);
                    DataObject dataObj = new DataObject(userBridge);
                    DragDrop.DoDragDrop(listBoxItem, dataObj, DragDropEffects.Move);
                    mAdornerLayer.Remove(adorner);
                }
            }
        }

        private void lsBridge_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(lsBridge);
                HitTestResult result = VisualTreeHelper.HitTest(lsBridge, pos);
                TextBlock textBlock = e.OriginalSource as TextBlock;
                if (result == null)
                {
                    return;
                }
                ListBoxItem listBoxItem = VisualHelper.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null)
                {
                    return;
                }

                VisualBrush brush = new VisualBrush(listBoxItem);
                brush.Opacity = 0.5;
            }
        }

        private void ListBoxItem_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            mAdornerLayer.Update();
            UpdateListBoxItemExpandingState();
        }

        private void UpdateListBoxItemExpandingState()
        {
            Win32.POINT point = new Win32.POINT();
            if (Win32.GetCursorPos(ref point))
            {
                Point pos = new Point(point.X, point.Y);
                pos = lsBridge.PointFromScreen(pos);
                HitTestResult result = VisualTreeHelper.HitTest(lsBridge, pos);
                if (result != null)
                {
                    ListBoxItem selectedItem = VisualHelper.FindVisualParent<ListBoxItem>(result.VisualHit);
                    if (selectedItem != null)
                    {
                        if (boxItem != null)
                        {
                            if (!ReferenceEquals(boxItem, selectedItem))
                            {
                                boxItem = selectedItem;
                                for (int i = 0; i < lsBridge.Items.Count; i++)
                                {
                                    ListBoxItem childControl = lsBridge.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                                    if (childControl != null)
                                    {
                                        if (lsBridge.SelectedIndex != -1)
                                        {
                                            if (lsBridge.SelectedIndex == i)
                                            {
                                                childControl.Foreground = Brushes.Yellow;
                                            }
                                            else
                                            {
                                                if (ReferenceEquals(childControl, selectedItem))
                                                {
                                                    selectedItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fac152"));
                                                }
                                                else
                                                {
                                                    childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBC8"));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (ReferenceEquals(childControl, selectedItem))
                                            {
                                                selectedItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fac152"));
                                            }
                                            else
                                            {
                                                childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBC8"));
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            boxItem = selectedItem;
                        }
                    }
                }
                else
                {
                    if (lsBridge.SelectedItem != null)
                    {
                        ListBoxItem childControl1 = lsBridge.ItemContainerGenerator.ContainerFromItem(lsBridge.SelectedItem) as ListBoxItem;
                        for (int i = 0; i < lsBridge.Items.Count; i++)
                        {
                            ListBoxItem childControl = lsBridge.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                            if (childControl != null && childControl1 != null)
                            {
                                if (ReferenceEquals(childControl, childControl1))
                                {
                                    childControl.Foreground = Brushes.Yellow;
                                }
                                else
                                {
                                    childControl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBC8"));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void tb_price_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 2000;
            int min = 0;
            string text = tb.Text;

            if (text.Contains(".") && e.Text.Equals("."))
            {
                e.Handled = true;
                return;
            }
            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            decimal d = 0;
            if (decimal.TryParse(text, out d) && (d > max || d < min) && string.IsNullOrEmpty(tb.SelectedText))
            {
                e.Handled = true;
                return;
            }
            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8 && string.IsNullOrEmpty(tb.SelectedText))
                {
                    e.Handled = true;
                    return;
                }

            }
        }

        private void tb_price_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tb_price_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                gotoNextMatching();
                tb_price.Focus();
            }
        }


        BatchCrossBridgeWin batchCrossBridgeWin;

        private void Btn_BatchCrossBridge_Click(object sender, RoutedEventArgs e)
        {
            if (batchCrossBridgeWin == null)
            {
                batchCrossBridgeWin = new BatchCrossBridgeWin();
                batchCrossBridgeWin.Closed += (a, b) => { batchCrossBridgeWin = null; };
                batchCrossBridgeWin.Show();
            }
            else
            {
                batchCrossBridgeWin.Activate();
            }
        }

        private void tb_price_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentPrice = tb_price.Text.ToString();
            SetPositioningOrders(EFilterType.Price);
        }

        private void tb_price_LostFocus(object sender, RoutedEventArgs e)
        {
            //SetPositioningOrders(EFilterType.Price);
        }

        private void tb_price_GotFocus(object sender, RoutedEventArgs e)
        {
            SetPositioningOrders(EFilterType.Price);
        }

        /// <summary>
        /// 期限排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Term_Click(object sender, RoutedEventArgs e)
        {
            _isMaturityDaySort = cb_Term.IsChecked.Value;
            if (_isMaturityDaySort)
            {
                cbSort.IsChecked = false;
                NeedSort = false;
            }

            UserBridge bridge = getCurrentBridge();
            if (bridge != null)
                resetSeq(new KeyValuePair<UserBridge, List<BridgeOrderDetail>>(bridge, filterOrdersDic[bridge]));
            lsBridge_SelectionChanged(null, null);
        }


    }



    class BridgeFilterCondition
    {
        /// <summary>
        /// 交易日过滤
        /// </summary>
        public bool DealDate { get; set; }
        /// <summary>
        /// 录入日过滤
        /// </summary>
        public bool EntryDate { get; set; }
        /// <summary>
        /// 只看自己加的单子过滤
        /// </summary>
        public bool Self { get; set; }
        /// <summary>
        /// 过滤日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 桥订单所属经纪人 1 我的桥（包含助理）； 2 其他人成交过桥成交单
        /// </summary>
        public string BridgeOrderOfBroker { get; set; }

    }

    /// <summary>
    /// 定位类型
    /// </summary>
    public enum EFilterType
    {
        全部 = -1,
        编号 = 0,
        债券 = 1,
        日期 = 2,
        Price = 3
    }
}
