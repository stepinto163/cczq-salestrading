using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.View.Transaction.BridgeDesign.EditMemory;
using STC.Broker.Plugin.View.Transaction.BridgeDesign.Reserve;
using STC.Common.Handler.CRM;
using STC.Common.Model.CRM;
using STC.Common.View.UI;
using STC.Frame.UI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using STC.Broker.Plugin.Model.Transaction;
using static STC.Broker.Plugin.Utils.CommonEnum;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.PT;
using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Handler.SystemParam;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Common.Utils;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// BridgeUI.xaml 的交互逻辑
    /// 单个桥+桥下成交单
    /// </summary>
    public partial class BridgeUI : UserControl
    {
        public event Action<object, UserBridge, UserBridge> OnUpdateBridge;
        /// <summary>
        /// 桥
        /// </summary>
        public UserBridge Bridge;
        /// <summary>
        /// 当前需要定位单
        /// </summary>
        public string FilterOrderId = string.Empty;
        private BridgeOrderDetail FilterOrder = null;
        public bool NeedSort = false;
        private readonly string TodayReceipt = "以下为今天发单的历史成交单";
        /// <summary>
        /// 缓存桥编辑按钮的状态
        /// </summary>
        private Dictionary<string, bool> editShowDic = new Dictionary<string, bool>();
        /// <summary>
        /// 缓存桥删除按钮的状态
        /// </summary>
        private Dictionary<string, bool> deleteShowDic = new Dictionary<string, bool>();
        /// <summary>
        /// 桥下的成交单-展示
        /// </summary>
        public List<BridgeOrderDetail> bridgeOrderList = new List<BridgeOrderDetail>();

        public BridgeUI()
        {
            InitializeComponent();
            this.Unloaded += BridgeUI_Unloaded;
            bridgeInfoUI.OnAddBridge += BridgeInfoUI_OnAddBridge;
            bridgeInfoUI.OnReserveBridge += BridgeInfoUI_OnReserveBridge;
        }



        /// <summary>
        /// 定位ExecOrder 20200622 wxp
        /// 成功返回true，失败返回false
        /// </summary>
        /// <param name="order"></param>
        public void gotoOrder(BridgeOrderDetail order)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
            {
                dgData.ScrollIntoView(order);
                List<BridgeOrderDetail> orders = dgData.ItemsSource as List<BridgeOrderDetail>;
                int rowIndex = orders.IndexOf(order);
                if (rowIndex < 0)
                    return;
                DataGridRow row = (DataGridRow)dgData.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                if (row != null)
                {
                    dgData.SelectedIndex = rowIndex;
                    return;
                }
                else
                    return;
            }));

        }
        private void RefreshData(List<BridgeOrderDetail> orders)
        {
            ///当设置DataContext的集合为list，但是在其他地方更改list的时候会报错“某个 ItemsControl 与它的项源不一致。”
            ///此报错不影响运行，使用tolist方法可避免报错。
            dgData.DataContext = bridgeOrderList = orders.ToList();
        }

        /// <summary>
        ///重新排序
        /// </summary>
        /// <see cref="BridgeWin.cbSort_Click"/>
        public void ReSortData()
        {
            List<BridgeOrderDetail> orders = bridgeOrderList.ToList();
            if (NeedSort)
                orders.Sort(new BridgeGroupSort());
            else
                orders.Sort(new BridgeNormalSort());
            bridgeOrderList.Clear();
        }

        /// <summary>
        /// 获取此桥下所有的单子
        /// </summary>
        /// <returns></returns>
        List<BridgeOrderDetail> getAllOrder()
        {
            List<OrderPo> selectOrders = new List<OrderPo>();//OrderHandler.Instance.GetAllOrder(true);
            //bool addTodayReceiptGroup = false;
            List<OrderDetailPo> normalOrderList = new List<OrderDetailPo>();
            List<BridgeOrderDetail> temList = new List<BridgeOrderDetail>();
            long start2 = DateTime.Now.Ticks;
            foreach (OrderPo orderPo in selectOrders)
            {
                if (orderPo.execOrders != null && orderPo.execOrders.Count > 0)
                {
                    List<OrderDetailPo> temOrders = orderPo.execOrders.Where(delegate (OrderDetailPo orderDetail)
                    {
                        //if (string.IsNullOrEmpty(FilterOrderId) || FilterOrderId.Equals(orderDetail.seq.Value + ""))
                        //{
                        if (((!string.IsNullOrEmpty(orderDetail.BridgeType) && orderDetail.BridgeTraderId.Equals(Bridge.bankTraderid)//过桥单判断
                            && orderDetail.BridgeInstitutionId.Equals(Bridge.bankInstitutionid))
                                || (!string.IsNullOrEmpty(orderDetail.BridgeType2) && !string.IsNullOrEmpty(orderDetail.BridgeTraderId2) &&
                                    orderDetail.BridgeTraderId2.Equals(Bridge.bankTraderid) &&
                                        !string.IsNullOrEmpty(orderDetail.BridgeInstitutionId2) && orderDetail.BridgeInstitutionId2.Equals(Bridge.bankInstitutionid))))
                            return true;
                        else
                            return false;
                        //}
                        //else
                        //    return false;
                    }).ToList();
                    //过滤掉删除的单子
                    //temOrders = temOrders.Where(delegate(OrderDetailPo orderDetailPo){ return (orderDetailPo.orderStatus + "") != "D" ? true : false; }).ToList();
                    temOrders = temOrders.Where(a => a.OrdStatus + "" != "D").ToList();
                    foreach (OrderDetailPo orderDetail in temOrders)
                    {
                       // orderDetail.isBidBridge = BridgeUIDomain.IsBidBridge(orderDetail, Bridge) ? "1" : "2";
                        normalOrderList.Add(orderDetail);
                    }
                }
            }
            long end = DateTime.Now.Ticks;

            ///获取桥下所有未查看的单子
            string key = Bridge.bankInstitutionid + "-" + Bridge.bankTraderid;
            //BridgeReviewsRequestPo request = new BridgeReviewsRequestPo() { instID = Bridge.bankid, instTraderID = Bridge.bankagentid };
            //BridgeReviewsReplyPo msg = BrokerCEDAServer.Instance.QueryBridgeReviews(request);
            List<BridgeReserveReviews> BridgeReviews;
            BridgeWinNew.BridgeReviewsDic.TryGetValue(key, out BridgeReviews);
            //如果桥下有单子初始化过桥单 20200422
            if (normalOrderList.Count > 0)
            {
                foreach (OrderDetailPo od in normalOrderList)
                {
                    BridgeOrderDetail orderDetail = new BridgeOrderDetail(); // BridgeUIDomain.CreateBridgeOrderDetail(od, Bridge);
                    temList.Add(orderDetail);
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

            return temList;
        }
        /// <summary>
        /// 过桥子窗口的单子处理
        /// 主窗口中调用
        /// </summary>
        /// <see cref="BridgeWin.Processdatas"/>
        public void Organizedata()
        {
            //主线程操作UI 清空界面数据，重新加载
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => { bridgeOrderList.Clear(); }));
            long start = DateTime.Now.Ticks;
            List<BridgeOrderDetail> temList = getAllOrder();
            start = DateTime.Now.Ticks;
            temList.OrderBy(o => o.seq);
            temList.Sort(new BridgeNormalSort());
            int i = 1;
            foreach (BridgeOrderDetail od in temList)
            {
                od.bridgeSeq = i++;
                BridgeUIDomain.UpdateOrderTxt(od, Bridge);
            }

            if (NeedSort)
                temList.Sort(new BridgeGroupSort());
        }

        private void BridgeUI_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.Unloaded -= BridgeUI_Unloaded;
            //bridgeInfoUI.OnAddBridge -= BridgeInfoUI_OnAddBridge;
        }
        /// <summary>
        /// 初始化桥
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="filterOrderId"></param>
        /// <param name="needSort"></param>
        public void Init(UserBridge bridge, string filterOrderId, bool needSort)
        {
            FilterOrderId = filterOrderId;
            NeedSort = needSort;
            Bridge = bridge;
            this.dgData.Bridge = bridge;
            bridgeInfoUI.Init(bridge);
            //以前的初始化过桥单 20200422
            // InitData();
            LogHelper.Debug("BridgeUI open {0}", Bridge.bankTradername);
        }
        /// <summary>
        /// 桥选择改变，更改数据源
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="orders"></param>
        public void RefreshData(KeyValuePair<UserBridge, List<BridgeOrderDetail>> bridge)
        {
            ///FilterOrderId = filterOrderId;
            Bridge = bridge.Key;
            //桥
            bridgeInfoUI.Init(Bridge);
            //数据
            RefreshData(bridge.Value);
        }

        #region 事件
        /// <summary>
        /// 选中需要操作的成交单
        /// </summary>
        private string SelectOrderId = string.Empty;

        /// <summary>
        /// 订单编辑桥按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditImage_Click(object sender, RoutedEventArgs e)
        {


            BridgeOrderDetail bo = dgData.SelectedItem as BridgeOrderDetail;
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            bo = button.DataContext as BridgeOrderDetail;

            if (bo != null)
            {
                if (editShowDic.ContainsKey(bo.ExecID))
                {
                    if (editShowDic[bo.ExecID])
                    {
                        return;
                    }
                }

                if (BridgeEditMemoryImpl.Instance.CheckEditStatus(bo.ExecID))
                    return;

                BridgeDetailWin detailWin = new BridgeDetailWin();
                detailWin.OnClose += DetailWin_OnClose;

                UserBridge bidBridge = null;
                UserBridge ofrBridge = null;
                if ((bo.BridgeType.Equals("B") && bo.BridgeType2.Equals("O"))
                    || (bo.BridgeType.Equals("O") && bo.BridgeType2.Equals("B")))
                {
                    bidBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId2, bo.BridgeTraderId2);
                    ofrBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                }
                else
                {
                    bidBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                    ofrBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                }

                detailWin.Init(ObjConvertHelper.CopyBridgeOrderDetailo(bo), ofrBridge, bidBridge, Bridge);

                var win = Window.GetWindow(this);
                detailWin.Owner = win;
                //if (win == null)
                //    detailWin.Owner = Application.Current.MainWindow;
                //else
                //    detailWin.Owner = win;

                editShowDic.Add(bo.ExecID, true);
                BridgeEditMemoryImpl.Instance.Add(bo.ExecID, true);

                detailWin.ShowDialog();
            }
        }

        private void DetailWin_OnClose(BridgeOrderDetail obj)
        {
            if (editShowDic.ContainsKey(obj.ExecID))
                editShowDic.Remove(obj.ExecID);

            BridgeEditMemoryImpl.Instance.DeleteByKey(obj.ExecID);
        }
        /// <summary>
        /// 当一个桥下的成交单被编辑的时 不可以打开其它窗口去编辑它
        /// </summary>
        private void DetailWin_OnCloses(List<BridgeOrderDetail> obj)
        {
            foreach (var o in obj)
            {
                if (editShowDic.ContainsKey(o.ExecID))
                    editShowDic.Remove(o.ExecID);
                BridgeEditMemoryImpl.Instance.DeleteByKey(o.ExecID);
            }
        }

        /// <summary>
        /// 订单删桥桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
           
            ///保证一次只可以打开一个删除窗口
            if (deleteShowDic.Count > 0)
                return;
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            deleteOrderDetail = button.DataContext as BridgeOrderDetail;
            //deleteOrderDetail = dgData.SelectedItem as BridgeOrderDetail;
            if (deleteOrderDetail != null)
            {
                if (deleteShowDic.ContainsKey(deleteOrderDetail.ExecID))
                {
                    if (deleteShowDic[deleteOrderDetail.ExecID])
                        return;
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
                    messageBoxGUI.setWarnMsg("确认删除桥?");
                    messageBoxGUI.MOK += MessageBoxGUI_MOK;
                    messageBoxGUI.MClosed += MessageBoxGUI_MClosed;
                    //var win = Window.GetWindow(this);
                    //messageBoxGUI.Owner = win;
                    messageBoxGUI.Topmost = true;
                    deleteShowDic.Add(deleteOrderDetail.ExecID, true);
                    messageBoxGUI.Show();
                }));


            }
        }

        private BridgeOrderDetail deleteOrderDetail = null;


        private void MessageBoxGUI_MClosed()
        {
            IsDeleteBridge = false;

            if (deleteOrderDetail != null && deleteShowDic.ContainsKey(deleteOrderDetail.ExecID))
                deleteShowDic.Remove(deleteOrderDetail.ExecID);
        }

        /// <summary>
        /// 确认删除订单的桥
        /// </summary>
        private void MessageBoxGUI_MOK()
        {
            if (deleteOrderDetail != null && deleteShowDic.ContainsKey(deleteOrderDetail.ExecID))
                deleteShowDic.Remove(deleteOrderDetail.ExecID);

            deleteOrderDetail = ObjConvertHelper.CopyBridgeOrderDetailo(deleteOrderDetail);
            bool isBid = BridgeUIDomain.IsBidBridge(deleteOrderDetail, Bridge);

            UserBridge bridge = null;//恢复默认桥
                                     ////获取未删除的默认桥
            if (isBid)
                bridge = UserBridgeHandler.Instance.GetBridgeById(deleteOrderDetail.BridgeInstitutionId, deleteOrderDetail.BridgeTraderId);
            else
                bridge = UserBridgeHandler.Instance.GetBridgeById(deleteOrderDetail.BridgeInstitutionId2, deleteOrderDetail.BridgeTraderId2);

            deleteOrderDetail.SendTo = deleteOrderDetail.SendTo2 = BridgeUIDomain.SetSentto(deleteOrderDetail, bridge);
            BridgeUIDomain.DeleteBridge(deleteOrderDetail, false, isBid, bridge);
        }
        private void dgData_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            //指定路由事件对UIElement的事件类型
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = Parent;
            //指定路由事件的源对象
            (Parent as UIElement).RaiseEvent(eventArg);
        }
        /// <summary>
        /// 右键弹出复制菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            //dgData.OnExecutedCopy(null);
            //V2.3需求调整：2）过桥窗口中，两种复制的方法互换，选中或按Ctrl+C直接复制时带单号，右键菜单中复制出的结果是“不带单号复制”
            //调整为不带单号赋值 20200224 
            dgData.Bridge = this.Bridge;
            dgData.OnExecutedCopy(BrigeCopyType.WithoutSeq);
        }


        /// <summary>
        /// 批量编辑桥要素按钮事件
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

            var bos = dgData.SelectedItems;
            List<BridgeOrderDetail> orderDetails = new List<BridgeOrderDetail>();
            foreach (BridgeOrderDetail bo in bos)
            {
                //过滤掉已取消的单子
                //if (bo.BridgeStateShow == "已取消")
                //    break;
                if (bo.BidBridgeStateShow == "已取消" && bo.OfrBridgeStateShow == "已取消")
                    break;
                orderDetails.Add(bo);
                if (editShowDic.ContainsKey(bo.ExecID))
                {
                    if (editShowDic[bo.ExecID])
                        return;
                }
                if (BridgeEditMemoryImpl.Instance.CheckEditStatus(bo.ExecID))
                    return;
                editShowDic.Add(bo.ExecID, true);
                BridgeEditMemoryImpl.Instance.Add(bo.ExecID, true);
            }
            if (orderDetails.Count > 0)
            {
                //批量修改，如果选中单条调用单条修改框,如果选中多条调用批量修改框
                BridgeDetailWin detailWin = new BridgeDetailWin();
                if (orderDetails.Count <= 1)
                {
                    detailWin.OnClose += DetailWin_OnClose;
                    BridgeOrderDetail bo = orderDetails.FirstOrDefault();
                    //单条
                    UserBridge bidBridge = null;
                    UserBridge ofrBridge = null;
                    if ((bo.BridgeType.Equals("B") && bo.BridgeType2.Equals("O"))
                        || (bo.BridgeType.Equals("O") && bo.BridgeType2.Equals("B")))
                    {
                        bidBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId2, bo.BridgeTraderId2);
                        ofrBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                    }
                    else
                    {
                        bidBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                        ofrBridge = UserBridgeHandler.Instance.GetBridgeById(bo.BridgeInstitutionId, bo.BridgeTraderId);
                    }
                    detailWin.Init(ObjConvertHelper.CopyBridgeOrderDetailo(bo), ofrBridge, bidBridge, Bridge);
                }
                else
                {
                    //批量单条
                    detailWin.OnCloses += DetailWin_OnCloses;
                    detailWin.Init(orderDetails, Bridge);
                }


                var win = Window.GetWindow(this);
                detailWin.Owner = win;
                //if (win == null)
                //    detailWin.Owner = Application.Current.MainWindow;
                //else
                //    detailWin.Owner = win;

                detailWin.ShowDialog();
            }
        }
        /// <summary>
        /// 解决两个桥之间来回单击相同的order拷贝问题  20200416 wxp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgData_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //dgData.Bridge = this.Bridge;
            //dgData.OnExecutedCopy(null);
        }


        static HisReocrdWin bridegHisWin;
        /// <summary>
        /// 历史记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnHiistory_Click(object sender, RoutedEventArgs e)
        {
            BridgeOrderDetail orderDetailPo = dgData.SelectedItem as BridgeOrderDetail;

            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            orderDetailPo = button.DataContext as BridgeOrderDetail;

            if (orderDetailPo == null)
                return;

            BridgeHistRequestPo request = new BridgeHistRequestPo();
            ///borderID传给后台
            request.borderID = BridgeUIDomain.GetBOrderIdByBridge(orderDetailPo, Bridge);
            var reply = GRHandler.Instance.BridgeMvtQuery(request);

            if (reply != null && "0000".Equals(reply.Code) && reply.datas != null && reply.datas.Any())
            {

                if (bridegHisWin == null)
                {
                    bridegHisWin = new HisReocrdWin();//Windows窗体
                    bridegHisWin.Closed += new EventHandler(delegate (object ob, EventArgs ev) { bridegHisWin = null; });
                    bridegHisWin.Owner = Window.GetWindow(this);
                    bridegHisWin.Show();
                }
                bridegHisWin.RefreshDataEx(reply.datas);
            }
            //GrExecOrderQueryReq request = new GrExecOrderQueryReq();
            //request.execid = orderDetailPo.ExecID;
            //request.pageNum = 0;
            //request.pageSize = 10000;
            //request.commandTypes = new List<int>();
            //request.commandTypes.AddRange(new List<int>() { 6, 7, 30, 31, 8, 9, 10, 14, 33 });

            //GrExecOrdersReplyPo reply = await GetExecOrderHistRecordData(request);

            //if (reply != null && "0000".Equals(reply.Code) && reply.Datas != null && reply.Datas.Count > 0)
            //{
            //    List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
            //    TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, reply.Datas);
            //    if (bridegHisWin == null)
            //    {
            //        bridegHisWin = new HisReocrdWin();//Windows窗体
            //        bridegHisWin.Closed += new EventHandler(delegate (object ob, EventArgs ev) { bridegHisWin = null; });
            //        bridegHisWin.Owner = Window.GetWindow(this);
            //        bridegHisWin.Show();
            //    }
            //    bridegHisWin.RefreshDataEx(orderDatas);
            //}

        }

        /// <summary>
        /// 查询流水
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<GrExecOrdersReplyPo> GetExecOrderHistRecordData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderHistRecordQuery(req);
            });

            return reply;
        }
        /// <summary>
        /// 左键抬起执行复制操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgData_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dgData.Bridge = this.Bridge;
            dgData.OnExecutedCopy(BrigeCopyType.Normal);
        }

        private bool IsShow = false;
        /// <summary>
        /// 右击编辑桥  调出添加桥的窗口，修改桥的【发给】 【渠道】，【备注】
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsShow)
            {
                AddBridgeWin addbridgeWin = new AddBridgeWin();
                addbridgeWin.Closed += AddbridgeWin_Closed;
                addbridgeWin.OnAddBridge += AddbridgeWin_OnAddBridge;
                addbridgeWin.Init(Bridge, true);
                IsShow = true;
                addbridgeWin.Show();
            }
        }

        private void AddbridgeWin_Closed(object sender, EventArgs e)
        {
            IsShow = false;
        }
        /// <summary>
        /// 编辑桥后的回调，更新桥下成交单的发给
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bridge"></param>
        /// <param name="oldBridge"></param>
        /// <param name="isUpdate"></param>
        /// <param name="needReset"></param>
        private void AddbridgeWin_OnAddBridge(object sender, UserBridge bridge, UserBridge oldBridge, bool isUpdate, bool needReset)
        {
            IsShow = false;
            bridgeInfoUI.Init(Bridge);
            ///更新桥回调处理
            OnUpdateBridge?.Invoke(this, bridge, oldBridge);
            ///如果更新桥的话，需要更新桥下成交单
            if (isUpdate && needReset)
            {
                foreach (BridgeOrderDetail orderDetail in bridgeOrderList)
                {
                    if (orderDetail.OrderGroup != 2)
                    {
                        BridgeUIDomain.ResetBridgeInfo(bridge, ObjConvertHelper.CopyBridgeOrderDetailo(orderDetail));
                    }
                }
            }
        }

        public event Action<UserBridge> OnDeleteBridge;
        private bool IsDeleteBridge = false;
        /// <summary>
        /// 右击删除桥 桥下没有单子才会显示删除桥选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDeleteBridge)
            {
                IsDeleteBridge = true;

               

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
                    messageBoxGUI.setWarnMsg("确认删除桥?");
                    messageBoxGUI.MOK += DeleteBridge; ;
                    messageBoxGUI.MClosed += MessageBoxGUI_MClosed;

                    //var win = Window.GetWindow(this);
                    //messageBoxGUI.Owner = win;
                    messageBoxGUI.Topmost = true;
                    messageBoxGUI.ShowDialog();


                    //MessageBoxGUI msgBox = new MessageBoxGUI();
                    //msgBox.setWarnMsg("保存成功", true);
                    //msgBox.ShowDialog();

                    //InnerCopyInfoChange?.Invoke();
                }));

            }
        }
        /// <summary>
        /// 从系统参数中删除桥
        /// </summary>
        private void DeleteBridge()
        {
            IsDeleteBridge = false;
            ThreadPool.QueueUserWorkItem(o =>
            {
                if (UserBridgeHandler.Instance.DelBridge(Bridge))
                {
                    this.Dispatcher.Invoke(() => { OnDeleteBridge?.Invoke(Bridge); });
                }
            });

        }

        /// <summary>
        /// 右键菜单打开处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            int count = BridgeWinNew.BridgeDatasDic[Bridge].Count;
            if (count > 0)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnCopy.Visibility = Visibility.Visible;
                btnChange.Visibility = Visibility.Visible;
                btn_SpecialSeqCopy.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Collapsed;
                btn_NoBroker.Visibility = Visibility.Visible;

                //没有选中项的时候，不显示编辑和复制以及换桥
                if (dgData.SelectedItems.Count == 0)
                {
                    btnEdit.Visibility = Visibility.Collapsed;
                    btnCopy.Visibility = Visibility.Collapsed;
                    btnChange.Visibility = Visibility.Collapsed;
                    btn_SpecialSeqCopy.Visibility = Visibility.Collapsed;
                    btn_NoBroker.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                btnChange.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnCopy.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Visible;
                btn_SpecialSeqCopy.Visibility = Visibility.Collapsed;
                btn_NoBroker.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 今天收到
        /// </summary>
        /// <param name="bridgeOrderDetails"></param>
        /// <returns></returns>
        private bool ContainTodayReceipt(ObservableCollection<BridgeOrderDetail> bridgeOrderDetails)
        {
            List<BridgeOrderDetail> filterData = bridgeOrderDetails.Where(delegate (BridgeOrderDetail orderDetail)
            {
                if (orderDetail.OrderGroup == 2)
                    return true;
                else
                    return false;
            }).ToList();

            return filterData.Count > 0;

        }

        /// <summary>
        /// 在桥里添加成交单后的回调处理
        /// </summary>
        /// <param name="orderIds"></param>
        private void BridgeInfoUI_OnAddBridge(string orderIds)
        {
            List<string> arrOrder = orderIds.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //过滤已经存在的seq
            List<string> removeId = new List<string>();
            foreach (BridgeOrderDetail orderDetail in bridgeOrderList)
            {
                ///单号加桥方式，不支持历史单
                if (orderDetail.seq != null && arrOrder.Contains(orderDetail.seq.Value + "") && !OrderHelper.TodayReceipt(orderDetail))
                {
                    //拒绝的单子可以重新添加或者约定桥，新添加的单子返回到前端的时候，删除相同的execid且已拒绝的单子
                    if (orderDetail.BidBridgeStateShow == "已取消" && orderDetail.OfrBridgeStateShow == "已取消")
                        continue;
                    arrOrder.Remove(orderDetail.seq.Value + "");
                    removeId.Add(orderDetail.seq.Value + "");
                }
            }

            //添加桥
            AddBridge(arrOrder, removeId);

            //展示已经添加的提示
            ShowFailBridge(removeId);

        }

        /// <summary>
        /// 在桥里预约成交单后的回调处理
        /// </summary>
        /// <param name="orderIds"></param>
        private void BridgeInfoUI_OnReserveBridge(string orderIds)
        {
            List<string> arrOrder = orderIds.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //过滤已经存在的seq
            List<string> removeId = new List<string>();
            foreach (BridgeOrderDetail orderDetail in bridgeOrderList)
            {
                if (orderDetail.seq != null && arrOrder.Contains(orderDetail.seq.Value + "") && !OrderHelper.TodayReceipt(orderDetail))
                {
                    arrOrder.Remove(orderDetail.seq.Value + "");
                    removeId.Add(orderDetail.seq.Value + "");
                }
            }

            //预约桥
            ReserveBridge(arrOrder, removeId);

            //展示已经添加的提示
            ShowFailBridge(removeId);

        }
        /// <summary>
        /// 添加桥
        /// </summary>
        /// <param name="arrOrder"></param>
        /// <param name="removeId"></param>
        private void AddBridge(List<string> arrOrder, List<string> removeId)
        {
            if (arrOrder.Count > 0)
            {
                List<ExecutionReport> requestPos = new List<ExecutionReport>();
                List<TransOrderServerData> TransOrderServerDataList = BridgeHelper.getCacheList();
                List<TransOrderServerData> temOrders = TransOrderServerDataList.Where(delegate (TransOrderServerData orderDetail)
                {
                    string seq = orderDetail.Sequence;
                    if (seq != null && arrOrder.Contains(seq) && !TransOrderHelper.Instance.IsDealHistDay(orderDetail.TransactTime)
                           && (CopyDetailHandler.Instance.IsMyModifys(orderDetail.PartySubID) || CopyDetailHandler.Instance.IsMyModifys(orderDetail.CounterPartySubID) || SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId)))
                    {
                        if (string.IsNullOrEmpty(orderDetail.BridgeType))
                            return true;
                        else
                        {
                            if (orderDetail.DelFlag + "" != "1")
                            {
                                if (!removeId.Contains(seq))
                                    removeId.Add(seq);
                            }
                            return false;
                        }
                    }
                    else
                        return false;
                }).ToList();

                foreach (TransOrderServerData orderDetail in temOrders)
                {
                    //OrderDetailPo o = BridgeUIDomain.ConvertOrderDetail(orderDetail);
                    requestPos.Add(BridgeUIDomain.SetBridge2(orderDetail, false, Bridge, null, true));
                    orderDetail.BorderId = orderDetail.BorderId2 = string.Empty;
                    orderDetail.BridgeOrder = orderDetail.BridgeOrder2 = string.Empty;
                }
                OrderHelper.ChangeOrderEx(requestPos);
                //});
            }
        }
        /// <summary>
        /// 预约桥
        /// </summary>
        /// <param name="arrOrder"></param>
        /// <param name="removeId"></param>
        private void ReserveBridge(List<string> arrOrder, List<string> removeId)
        {
            if (arrOrder.Count > 0)
            {
                //List<OrderPo> selectOrders = OrderHandler.Instance.GetAllOrder(true);
                //foreach (OrderPo orderPo in selectOrders)
                //{
                //    if (orderPo.execOrders != null && orderPo.execOrders.Count > 0)
                //    {
                //        List<OrderDetailPo> temOrders = orderPo.execOrders.Where(delegate (OrderDetailPo orderDetail)
                //        {
                //            string seq = orderDetail.seq.Value + "";
                //            if (orderDetail.seq != null && arrOrder.Contains(seq) && !OrderHelper.TodayReceipt(orderDetail)
                //                && (SystemParamHandler.IsMyModifys(orderDetail.brokerNameA) || SystemParamHandler.IsMyModifys(orderDetail.brokerNameB) || SystemParamHandler.IsBridgeRole()))
                //            {
                //                if (string.IsNullOrEmpty(orderDetail.bridgeType))
                //                    return true;
                //                else
                //                {
                //                    if (!removeId.Contains(seq))
                //                        removeId.Add(seq);
                //                    return false;
                //                }
                //            }
                //            else
                //                return false;
                //        }).ToList();

                //        foreach (OrderDetailPo orderDetail in temOrders)
                //            BridgeUIDomain.ReserveBridge(orderDetail, false, Bridge, null, true);
                //    }
                //}
            }
        }

        /// <summary>
        /// 订单加桥失败报警
        /// </summary>
        /// <param name="removeId"></param>
        private void ShowFailBridge(List<string> removeId)
        {
            if (removeId.Count > 0)
            {
                string msg = string.Empty;
                foreach (string seq in removeId)
                {
                    if (!string.IsNullOrEmpty(seq))
                        msg += seq + ",";
                }
                LogHelper.Debug($"订单{msg}已经加桥");
                //msg = string.Format("第 {0} 笔已有桥，加桥失败", msg.Substring(0, msg.Length - 1));
                msg = string.Format("此笔交易已预约/添加桥。");
                SysWarnGUI warGUI = new SysWarnGUI();
                warGUI.setWarnMsg(msg);
                var win = Window.GetWindow(this);
                warGUI.Owner = win;
                //if (win == null)
                //    warGUI.Owner = Application.Current.MainWindow;
                //else
                //    warGUI.Owner = win;

                warGUI.Show();
            }
        }

        #endregion

        #region  添加二桥操作和换桥操作
        /// <summary>
        /// 在弹出列表选择桥，进行双桥操作
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="isBid"></param>
        /// <param name="selectData"></param>
        private void SelectPopData(Popup pop, bool isBid, DataGridDataPo selectData)
        {
            if (selectData == null)
            {
                pop.IsOpen = false;
                return;
            }
            MessageBoxGUI3 warn = new MessageBoxGUI3();
            var win = Window.GetWindow(this);
            warn.Owner = win;

            //if (win == null)
            //    warn.Owner = Application.Current.MainWindow;
            //else
            //    warn.Owner = win;
            if (OperateType == "1" || OperateType == "2")
            {
                warn.Add += () => { warn_Add(pop, isBid, selectData); };
                warn.Reserve += () => { warn_Reserve(pop, isBid, selectData); };
            }
            if (OperateType == "3")
            {
                warn.Add += () => { warn_Change(pop, selectData, false); };
                warn.Reserve += () => { warn_Change(pop, selectData, true); };
            }
            warn.MClosed += warn_Close;
            warn.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            warn.ShowDialog();
        }

        /// <summary>
        /// 添加二桥
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="isBid"></param>
        /// <param name="selectData"></param>
        private void warn_Add(Popup pop, bool isBid, DataGridDataPo selectData)
        {

            List<BridgeOrderDetail> filterDatas = (from a in bridgeOrderList where a.ExecID == SelectOrderId select a).ToList();
            if (filterDatas != null && filterDatas.Count > 0 && selectData != null)
            {
                UserBridge bridgeTwo = UserBridgeHandler.Instance.GetBridgeById(selectData.InstIdbKey, selectData.IdbKey);
                if (bridgeTwo != null)
                {
                    //添加2桥
                    BridgeUIDomain.SetBridgeWithChangeOrder(filterDatas[0], true, Bridge, bridgeTwo, isBid);
                    SelectOrderId = string.Empty;
                }
            }
            pop.IsOpen = false;

        }
        /// <summary>
        /// 预订二桥 
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="isBid"></param>
        /// <param name="selectData"></param>
        private void warn_Reserve(Popup pop, bool isBid, DataGridDataPo selectData)
        {
            List<BridgeOrderDetail> filterDatas = (from a in bridgeOrderList where a.ExecID == SelectOrderId select a).ToList();
            if (filterDatas != null && filterDatas.Count > 0 && selectData != null)
            {
                UserBridge bridgeTwo = UserBridgeHandler.Instance.GetBridgeById(selectData.InstIdbKey, selectData.IdbKey);

                if (bridgeTwo != null)
                {
                    //预订2桥
                    BridgeUIDomain.ReserveBridge(filterDatas[0], true, Bridge, bridgeTwo, isBid);
                    SelectOrderId = string.Empty;
                }
            }
            pop.IsOpen = false;

        }
        /// <summary>
        /// 换桥-添加
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="isBid"></param>
        /// <param name="selectData"></param>
        private void warn_Change(Popup pop, DataGridDataPo selectData, bool isReserve)
        {
            //批量换桥
            List<BridgeOrderDetail> bridgeOrderDetails = new List<BridgeOrderDetail>();
            if (dgData.SelectedItems != null && dgData.SelectedItems.Count > 0)
            {
                foreach (var item in dgData.SelectedItems)
                {
                    BridgeOrderDetail detail = item as BridgeOrderDetail;
                    if (detail != null)
                    {
                        bridgeOrderDetails.Add(detail);
                    }
                }

                List<ExecutionReport> requestPos = new List<ExecutionReport>();
                foreach (BridgeOrderDetail bod in bridgeOrderDetails)
                {
                    List<BridgeOrderDetail> filterDatas = (from a in bridgeOrderList where a.ExecID == bod.ExecID select a).ToList();
                    if (filterDatas != null && filterDatas.Count > 0 && selectData != null)
                    {
                        UserBridge bridgeOne;
                        UserBridge changeBridge;

                        if (bod.BridgeType == "B,O")
                        {
                            bridgeOne = null;
                            changeBridge = UserBridgeHandler.Instance.GetBridgeById(selectData.InstIdbKey, selectData.IdbKey);
                            if (changeBridge != null)
                            {
                                //换桥
                                requestPos.Add(BridgeUIDomain.ChangeBridgeEx(bod, bridgeOne, changeBridge, isReserve));
                            }
                        }
                        else
                        {
                            changeBridge = UserBridgeHandler.Instance.GetBridgeById(selectData.InstIdbKey, selectData.IdbKey);

                            if (bod.bridgeOrder.instId == Bridge.bankInstitutionid && bod.bridgeOrder.instTradeId == Bridge.bankTraderid)
                            {
                                bridgeOne = UserBridgeHandler.Instance.GetBridgeById(bod.bridgeOrder2.instId, bod.bridgeOrder2.instTradeId);
                            }
                            else
                            {
                                bridgeOne = UserBridgeHandler.Instance.GetBridgeById(bod.bridgeOrder.instId, bod.bridgeOrder.instTradeId);
                            }
                            if (bridgeOne != null && changeBridge != null)
                            {
                                //换桥
                                requestPos.Add(BridgeUIDomain.ChangeBridgeEx(bod, bridgeOne, changeBridge, isReserve));
                            }
                        }
                    }
                    //SelectOrderId = string.Empty;
                    pop.IsOpen = false;
                }
                OrderHelper.UpdateBatchExecOrderData(requestPos);
                
                //OrderHelper.ChangeOrderEx(requestPos);
            }

        }

        /// <summary>
        /// 取消操作
        /// </summary>
        private void warn_Close()
        {
            Window win = Window.GetWindow(this);
            win.Activate();
        }
        /// <summary>
        /// 操作类型字段
        /// 添加二桥O-1
        /// 添加二桥B-2
        /// 换桥-3
        /// </summary>
        private string OperateType;
        /// <summary>
        /// 添加O二桥列表双击处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOfr_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (OperateType == "1")
            {
                SelectPopData(popOfr, true, dgOfr.SelectedItem as DataGridDataPo);
            }
            if (OperateType == "2")
            {
                SelectPopData(popOfr, false, dgOfr.SelectedItem as DataGridDataPo);
            }
            if (OperateType == "3")
            {
                SelectPopData(popOfr, false, dgOfr.SelectedItem as DataGridDataPo);
            }
        }
        /// <summary>
        /// 回车键处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOfrBridgeKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                //解决桥方向反了  20200424 wym
                if (OperateType == "1")
                {
                    SelectPopData(popOfr, true, dgOfr.SelectedItem as DataGridDataPo);
                }
                if (OperateType == "2")
                {
                    SelectPopData(popOfr, false, dgOfr.SelectedItem as DataGridDataPo);
                }
                if (OperateType == "3")
                {
                    SelectPopData(popOfr, false, dgOfr.SelectedItem as DataGridDataPo);

                    //批量换桥

                }
            }
        }
        /// <summary>
        /// 订单添加ofr桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOfrImage_Click(object sender, RoutedEventArgs e)
        {
            OperateType = "1";
            BridgeOrderDetail bod = dgData.SelectedItem as BridgeOrderDetail;

            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            bod = button.DataContext as BridgeOrderDetail;

            if (bod != null)
            {
                SelectOrderId = bod.ExecID;
                if (string.IsNullOrEmpty(bod.BridgeStatus))
                    BridgeUIDomain.UpdateBridgeStatus(bod, ECommandType.修改加桥按钮O状态, "O", Bridge);// UpdateBridgeStatus("O");
                else
                    BridgeUIDomain.UpdateBridgeStatus(bod, ECommandType.修改加桥按钮O状态, string.Empty, Bridge);//UpdateBridgeStatus(string.Empty);
            }

            ShowBridgePop(sender as Control);
        }
        /// <summary>
        /// 订单添加bid桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBidImage_Click(object sender, RoutedEventArgs e)
        {
            OperateType = "2";
            BridgeOrderDetail bod = dgData.SelectedItem as BridgeOrderDetail;
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            bod = button.DataContext as BridgeOrderDetail;

            if (bod != null)
            {
                SelectOrderId = bod.ExecID;
                if (string.IsNullOrEmpty(bod.BridgeStatus))
                    BridgeUIDomain.UpdateBridgeStatus(bod, ECommandType.修改加桥按钮B状态, "B", Bridge);//UpdateBridgeStatus("B");
                else
                    BridgeUIDomain.UpdateBridgeStatus(bod, ECommandType.修改加桥按钮B状态, string.Empty, Bridge);//UpdateBridgeStatus(string.Empty);
            }
            ShowBridgePop(sender as Control);
        }
        //增加过滤框20200225
        private Popup pop = null;
        /// <summary>
        /// 弹出列表，内容为该订单可添加的桥
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="isBid"></param>
        private void ShowBridgePop(Control btn)
        {
            //清理输入框
            tbOfrBridgeKey.Text = "";
            pop = (Popup)FindName("popOfr");
            pop.IsOpen = false;
            List<DataGridDataPo> dataList = new List<DataGridDataPo>();
            if (OperateType == "3")
            {
                List<BridgeOrderDetail> bridgeOrderDetails = new List<BridgeOrderDetail>();
                BridgeOrderDetail order = dgData.SelectedItem as BridgeOrderDetail;
                if (dgData.SelectedItems != null && dgData.SelectedItems.Count > 0)
                {
                    foreach (var item in dgData.SelectedItems)
                    {
                        BridgeOrderDetail order1 = item as BridgeOrderDetail;
                        bridgeOrderDetails.Add(order1);
                    }
                }
                filterBridge(dataList, bridgeOrderDetails);
            }
            else
            {
                filterBridge(dataList);
            }

            if (dataList.Count > 0)
            {
                if (OperateType == "3")
                {
                    //双击获取第5列的控件
                    DataGridTemplateColumn templeColumn1 = dgData.Columns[4] as DataGridTemplateColumn;

                    List<TextBlock> textBlocks = new List<TextBlock>();
                    foreach (var item in dgData.SelectedItems)
                    {
                        FrameworkElement fwElement1 = dgData.Columns[4].GetCellContent(item);
                        if (fwElement1 != null)
                        {
                            TextBlock tb = templeColumn1.CellTemplate.FindName("bridgeTxt", fwElement1) as TextBlock;
                            if (tb != null)
                            {
                                textBlocks.Add(tb);

                                //Point point = tb.TranslatePoint(new Point(), dgData);

                                //pop.PlacementTarget = tb;
                                //pop.VerticalOffset = 4;
                                //pop.HorizontalOffset = 180;

                                //if (point != null)
                                //{
                                //    if (dgData.ActualHeight - point.Y < 200)
                                //    {
                                //        pop.Placement = PlacementMode.Top;
                                //    }
                                //    else {
                                //        pop.Placement = PlacementMode.Bottom;
                                //    }
                                //}
                                //else {
                                //    pop.Placement = PlacementMode.Bottom;
                                //}

                                //break;
                            }
                        }
                    }

                    if (textBlocks.Count > 0)
                    {
                        if (textBlocks.Count == 1)
                        {
                            pop.PlacementTarget = textBlocks[0];
                            pop.VerticalOffset = 4;
                            pop.HorizontalOffset = 180;
                        }
                        else
                        {
                            int count = textBlocks.Count / 2;
                            pop.PlacementTarget = textBlocks[count];
                            pop.VerticalOffset = 4;
                            pop.HorizontalOffset = 180;
                        }
                    }
                }
                else
                {
                    pop.PlacementTarget = btn;
                    pop.VerticalOffset = 4;
                    pop.HorizontalOffset = 0;
                }
                pop.DataContext = dataList;
                pop.IsOpen = true;
                tbOfrBridgeKey.Focus();
            }
        }


        /// <summary>
        /// 换桥操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            OperateType = "3";
            BridgeOrderDetail bod = dgData.SelectedItem as BridgeOrderDetail;
            if (bod != null)
            {
                //SelectOrderId = bod.execId;
            }
            ShowBridgePop(sender as Control);
        }
        private void tbOfrBridgeKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgOfr.SelectedIndex > 0)
                    dgOfr.SelectedIndex--;
                dgOfr.ScrollIntoView(dgOfr.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgOfr.SelectedIndex < dgOfr.Items.Count - 1)
                    dgOfr.SelectedIndex++;
                dgOfr.ScrollIntoView(dgOfr.SelectedItem);
            }
        }

        //过滤关键字
        //private string bridgeKey = string.Empty;
        /// <summary>
        /// 过滤桥数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbBridgeKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (pop == null || !pop.IsOpen) return;
            //因为点击B,O按钮，订单会刷新，所以需要重新获取对象，然后设置popup的PlacementTarget，不然popup的位置会异常。王兴鹏 20201218 V2.8.0.1
            if (OperateType == "1")
            {
                //获取O按钮
                DataGridTemplateColumn templeColumn1 = dgData.Columns[8] as DataGridTemplateColumn;
                FrameworkElement fwElement1 = dgData.Columns[8].GetCellContent(dgData.SelectedItem);
                Button bt = templeColumn1.CellTemplate.FindName("btnOfrImage", fwElement1) as Button;
                pop.PlacementTarget = bt;
            }
            if (OperateType == "2")
            {
                //获取B按钮
                DataGridTemplateColumn templeColumn1 = dgData.Columns[9] as DataGridTemplateColumn;
                FrameworkElement fwElement1 = dgData.Columns[9].GetCellContent(dgData.SelectedItem);
                Button bt = templeColumn1.CellTemplate.FindName("btnBidImage", fwElement1) as Button;
                pop.PlacementTarget = bt;
            }
            TextBox obj = sender as TextBox;
            List<DataGridDataPo> dataList = new List<DataGridDataPo>();
            BridgeOrderDetail order = dgData.SelectedItem as BridgeOrderDetail;
            filterBridge(dataList, order, obj.Text);
            pop.DataContext = dataList;
            if (dataList.Count > 0)
            {
                dgOfr.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// 过滤下拉框中的桥列表
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="key"></param>
        private void filterBridge(List<DataGridDataPo> dataList, string key = "")
        {
            foreach (UserBridge ub in UserBridgeHandler.Instance.Bridges)
            {
                //过滤当前所在的桥 20200320
                if ((!ub.bankTraderid.Equals(Bridge.bankTraderid) && !ub.bankInstitutionid.Equals(Bridge.bankInstitutionid))
                    || (ub.bankInstitutionid.Equals(Bridge.bankInstitutionid) && !ub.bankTraderid.Equals(Bridge.bankTraderid)))
                {
                    if (string.IsNullOrEmpty(ub.TraderPinYin))
                    {
                        CrmInst inst = Utils.CommonUtils.GetInstByIdbKey(ub.bankInstitutionid);
                        //CRMDataHandler.Instance.GetInstByIdbKey(ub.bankInstitutionid);
                        ub.PinYin = inst.pinyin;
                        ub.PinYinFull = inst.pinyinfull;
                        DataGridDataPo contract = Utils.CommonUtils.GetContactByIdbKey(ub.bankTraderid);//CRMService.Instance.GetContactByIdbKey(ub.bankAgentId);
                        ub.TraderPinYin = contract.PinYin;
                        ub.TraderPinYinFull = contract.PinYinFull;
                    }

                    if (!string.IsNullOrEmpty(key) && !(ub.bankInstitutionname.Contains(key) || ub.bankTradername.Contains(key)
                        || ub.PinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                        || ub.PinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                        || ub.TraderPinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                        || ub.TraderPinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                        ))
                        continue;
                    //添加桥信息
                    DataGridDataPo dg = new DataGridDataPo()
                    {
                        IdbKey = ub.bankTraderid,
                        InstIdbKey = ub.bankInstitutionid,
                        InstName = ub.bankInstitutionname,
                        InstShortName = ub.bankShortName,
                        InstTraderName = string.Format("{0}({1})", ub.bankInstitutionname, ub.bankTradername),
                        TraderName = ub.bankTradername,
                        PinYin = ub.PinYin,
                    };
                    dataList.Add(dg);
                }
            }
        }
        /// <summary>
        /// 过滤下拉框中的桥列表
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="key"></param>
        private void filterBridge(List<DataGridDataPo> dataList, BridgeOrderDetail order, string key = "")
        {
            foreach (UserBridge ub in UserBridgeHandler.Instance.Bridges)
            {
                //过滤当前订单已有的桥  
                if (order != null)
                {
                    bool condition1 = true;
                    bool condition2 = false;
                    condition1 = (ub.bankTraderid.Equals(order.bridgeOrder.instTradeId) && ub.bankInstitutionid.Equals(order.bridgeOrder.instId));

                    if (order.bridgeOrder2 != null)
                    {
                        condition2 = (ub.bankTraderid.Equals(order.bridgeOrder2.instTradeId) && ub.bankInstitutionid.Equals(order.bridgeOrder2.instId));
                    }
                    if (!(condition1 || condition2))
                    {

                        if (string.IsNullOrEmpty(ub.TraderPinYin))
                        {
                            CrmInst inst = Utils.CommonUtils.GetInstByIdbKey(ub.bankInstitutionid);
                            ub.PinYin = inst.pinyin;
                            ub.PinYinFull = inst.pinyinfull;
                            DataGridDataPo contract = Utils.CommonUtils.GetContactByIdbKey(ub.bankTraderid);//CRMService.Instance.GetContactByIdbKey(ub.bankAgentId);
                            ub.TraderPinYin = contract.PinYin;
                            ub.TraderPinYinFull = contract.PinYinFull;
                        }

                        if (!string.IsNullOrEmpty(key) && !(ub.bankInstitutionname.Contains(key) || ub.bankTradername.Contains(key)
                            || ub.PinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || ub.PinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || ub.TraderPinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                            || ub.TraderPinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                            ))
                            continue;
                        //添加桥信息
                        DataGridDataPo dg = new DataGridDataPo()
                        {
                            IdbKey = ub.bankTraderid,
                            InstIdbKey = ub.bankInstitutionid,
                            InstName = ub.bankInstitutionname,
                            InstShortName = ub.bankShortName,
                            InstTraderName = string.Format("{0}({1})", ub.bankInstitutionname, ub.bankTradername),
                            TraderName = ub.bankTradername,
                            PinYin = ub.PinYin,
                        };
                        dataList.Add(dg);
                    }
                }
            }
        }
        /// <summary>
        /// 双桥换桥的时候需要过滤两座桥
        /// 过滤下拉框中的桥列表
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="key"></param>
        private void filterBridge(List<DataGridDataPo> dataList, List<BridgeOrderDetail> orders)
        {
            if (orders != null && orders.Count > 0)
            {
                foreach (UserBridge ub in UserBridgeHandler.Instance.Bridges)
                {
                    bool condition1 = false;
                    bool condition2 = false;
                    //过滤当前订单已有的桥  
                    foreach (BridgeOrderDetail order in orders)
                    {
                        condition1 = (ub.bankTraderid.Equals(order.bridgeOrder.instTradeId) && ub.bankInstitutionid.Equals(order.bridgeOrder.instId));

                        if (order.bridgeOrder2 != null)
                        {
                            condition2 = (ub.bankTraderid.Equals(order.bridgeOrder2.instTradeId) && ub.bankInstitutionid.Equals(order.bridgeOrder2.instId));
                        }
                        if (condition1 || condition2)
                        {
                            break;
                        }
                    }
                    if (!(condition1 || condition2))
                    {
                        //添加桥信息
                        DataGridDataPo dg = new DataGridDataPo()
                        {
                            IdbKey = ub.bankTraderid,
                            InstIdbKey = ub.bankInstitutionid,
                            InstName = ub.bankInstitutionname,
                            InstShortName = ub.bankShortName,
                            InstTraderName = string.Format("{0}({1})", ub.bankInstitutionname, ub.bankTradername),
                            TraderName = ub.bankTradername,
                            PinYin = ub.PinYin,
                        };
                        dataList.Add(dg);
                    }
                }
            }
            else
            {
                foreach (UserBridge ub in UserBridgeHandler.Instance.Bridges)
                {
                    //添加桥信息
                    DataGridDataPo dg = new DataGridDataPo()
                    {
                        IdbKey = ub.bankTraderid,
                        InstIdbKey = ub.bankInstitutionid,
                        InstName = ub.bankInstitutionname,
                        InstShortName = ub.bankShortName,
                        InstTraderName = string.Format("{0}({1})", ub.bankInstitutionname, ub.bankTradername),
                        TraderName = ub.bankTradername,
                        PinYin = ub.PinYin,
                    };
                    dataList.Add(dg);
                }
            }
        }
        #endregion

        #region load
        /// <summary>
        /// 如果当前控件内容是今日发单标号，则相应的按钮不与显示
        /// 显示内容应该是“--------以下为今天发单的历史成交单--------”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bdBidImage_Loaded(object sender, RoutedEventArgs e)
        {
            var sp = e.OriginalSource as Border;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    BridgeOrderDetail po = cp.Content as BridgeOrderDetail;
                    if (po != null)
                    {
                        var tbPrice = sp.Child as Button;
                        if (tbPrice != null && po.OrderGroup == 2)
                            tbPrice.Visibility = Visibility.Collapsed;
                        //如果是历史header数据，需要隐藏桥状态字段 wxp 20200624
                        var tbBridgeStatus = sp.Child as TextBlock;
                        if (tbBridgeStatus != null && tbBridgeStatus.Name == "tbBridgeStatus" && po.OrderGroup == 2)
                            tbBridgeStatus.Visibility = Visibility.Collapsed;

                    }
                }

            }
        }

        private void bdOfrImage_Loaded(object sender, RoutedEventArgs e)
        {
            bdBidImage_Loaded(sender, e);
        }

        private void bdEditImage_Loaded(object sender, RoutedEventArgs e)
        {
            bdBidImage_Loaded(sender, e);
        }

        /// <summary>
        /// 如果当前控件内容是今日发单标号，则相应的TextBlock不与显示
        /// 显示内容应该是--------以下为今天发单的历史成交单--------
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bdBrokerB_Loaded(object sender, RoutedEventArgs e)
        {
            var sp = e.OriginalSource as Border;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    BridgeOrderDetail po = cp.Content as BridgeOrderDetail;
                    if (po != null)
                    {
                        var tbPrice = sp.Child as TextBlock;
                        if (tbPrice != null && po.OrderGroup == 2)
                            tbPrice.Visibility = Visibility.Collapsed;
                    }
                }

            }
        }

        private void bdBrokerA_Loaded(object sender, RoutedEventArgs e)
        {
            bdBrokerB_Loaded(sender, e);
        }

        private void bdSeq_Loaded(object sender, RoutedEventArgs e)
        {
            bdBidImage_Loaded(sender, e);
        }

        private void bdDeleteImage_Loaded(object sender, RoutedEventArgs e)
        {
            bdBidImage_Loaded(sender, e);
        }

        /// <summary>
        /// 如果当前控件内容是今日发单标号
        /// 则文本显示内容应该是“--------以下为今天发单的历史成交单--------”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var sp = e.OriginalSource as Border;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    BridgeOrderDetail po = cp.Content as BridgeOrderDetail;
                    if (po != null)
                    {
                        var tbPrice = sp.Child as TextBlock;
                        if (tbPrice != null && po.OrderGroup == 2)
                        {
                            tbPrice.Margin = new Thickness(5, 0, 0, 5);
                            tbPrice.Padding = new Thickness(0, 2, 0, 0);
                            tbPrice.Foreground = (Brush)FindResource("ColorBrush.Green11");
                            int columnWidth = (int)this.ActualWidth - 178;
                            if (columnWidth > 166)
                            {
                                string append = string.Empty;
                                int num = (int)((columnWidth - 166) / 2) / 7;
                                for (int i = 0; i < num; i++)
                                    append += "-";
                                tbPrice.Text = append + TodayReceipt + append;
                            }
                        }
                    }
                }

            }
        }
        #endregion

        #region 拖动操作
        /// <summary>
        ///  拖拽进入，展示面罩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgData_DragEnter(object sender, DragEventArgs e)
        {
            var dragData = new List<DragOrderInfo>();
            dragData = e.Data.GetData(dragData.GetType()) as List<DragOrderInfo>;
            if (dragData != null)
            {
                gdMask.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 拖拽离开，隐藏面罩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gdMask_DragLeave(object sender, DragEventArgs e)
        {
            gdMask.Visibility = Visibility.Collapsed;
        }

        bool mouseIsLeft;
        /// <summary>
        /// 拖拽进入到预订区域，改变背景色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void reserve_DragOver(object sender, DragEventArgs e) {
        //    if (!mouseIsLeft)
        //    {
        //        mouseIsLeft = !mouseIsLeft;
        //        SolidColorBrush Color = new SolidColorBrush() { Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E"), Opacity = 0.8 };
        //        reserve.Background = Color;
        //        Color = new SolidColorBrush() { Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("Black"), Opacity = 0.8 };
        //        add.Background = Color;
        //    }

        //}
        /// <summary>
        /// 拖拽进入到添加区域，改变背景色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_DragOver(object sender, DragEventArgs e)
        {
            if (mouseIsLeft)
            {
                mouseIsLeft = !mouseIsLeft;
                SolidColorBrush Color = new SolidColorBrush() { Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E"), Opacity = 0.8 };
                add.Background = Color;
                Color = new SolidColorBrush() { Color = (Color)System.Windows.Media.ColorConverter.ConvertFromString("Black"), Opacity = 0.8 };
                //reserve.Background = Color;
            }
        }
        /// <summary>
        /// 拖放预约桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void reserve_Drop(object sender, DragEventArgs e) {
        //    e.Effects = DragDropEffects.None;

        //    List<PositionDetailPo> dragData = new List<PositionDetailPo>();
        //    dragData = e.Data.GetData(dragData.GetType()) as List<PositionDetailPo>;

        //    if (dragData != null)
        //    {
        //        List<string> removeId = new List<string>();
        //        List<OrderPo> allOrder = OrderHandler.Instance.GetAllOrder();
        //        foreach (PositionDetailPo pd in dragData)
        //        {
        //            List<BridgeOrderDetail> filterOrder = (from a in bridgeOrderList where a.execId == pd.ExecId select a).ToList();
        //            if (filterOrder == null || filterOrder.Count <= 0)
        //            {
        //                List<OrderPo> selectOrderPo = allOrder.Where(delegate (OrderPo orderPo)
        //                {
        //                    if (orderPo.execOrders.Any(o => o.execId.Equals(pd.ExecId)
        //                        && (SystemParamHandler.IsMyModifys(o.brokerNameA) || SystemParamHandler.IsMyModifys(o.brokerNameB) || SystemParamHandler.IsBridgeRole())))
        //                        return true;
        //                    else
        //                        return false;
        //                }).ToList();

        //                if (selectOrderPo != null && selectOrderPo.Count > 0)
        //                {
        //                    foreach (OrderDetailPo od in selectOrderPo[0].execOrders)
        //                    {
        //                        if (od.execId.Equals(pd.ExecId))
        //                        {
        //                            if (string.IsNullOrEmpty(od.bridgeType))
        //                                BridgeUIDomain.ReserveBridge(od, false, Bridge, null, true);
        //                            else
        //                                removeId.Add(od.seq + "");
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                removeId.Add(pd.Seq);
        //            }
        //        }
        //        ShowFailBridge(removeId);
        //    }
        //    gdMask.Visibility = Visibility.Collapsed;

        //}
        /// <summary>
        /// 拖放添加桥
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            List<DragOrderInfo> dragData = new List<DragOrderInfo>();
            dragData = e.Data.GetData(dragData.GetType()) as List<DragOrderInfo>;

            if (dragData != null)
            {
                List<ExecutionReport> requestPos = new List<ExecutionReport>();

                List<string> removeId = new List<string>();
               
                List<TransOrderServerData> allOrder = BridgeHelper.getCacheList();
                foreach (var pd in dragData)
                {
                    List<BridgeOrderDetail> filterOrder = (from a in bridgeOrderList where a.ExecID == pd.ExecId select a).ToList();
                    if (filterOrder == null || filterOrder.Count <= 0)
                    {
                        List<TransOrderServerData> selectOrderPo = allOrder.Where(_=>_.ExecID==pd.ExecId&&(SystemPowerHandler.Instance.IsCanEditOrder(_.PartySubID) || SystemPowerHandler.Instance.IsCanEditOrder(_.CounterPartySubID) || SystemPowerHandler.Instance.IsBridge)).ToList();
                        if (selectOrderPo != null && selectOrderPo.Count > 0)
                        {
                            foreach (TransOrderServerData od in selectOrderPo)
                            {
                                if (od.ExecID.Equals(pd.ExecId))
                                {
                                    if (string.IsNullOrEmpty(od.BridgeType))
                                    {
                                        requestPos.Add(BridgeUIDomain.SetBridge2(od, false, Bridge, null, true));
                                    }
                                    else
                                    {
                                        removeId.Add(od.Sequence + "");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        removeId.Add(pd.Seq);
                    }
                }

                OrderHelper.ChangeOrderEx(requestPos);
                ShowFailBridge(removeId);
            }
            gdMask.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void btn_SpecialSeqCopy_Click(object sender, RoutedEventArgs e)
        {
            dgData.Bridge = this.Bridge;
            dgData.OnExecutedCopy(BrigeCopyType.SpecialSeq);
        }

        private void btn_NoBroker_Click(object sender, RoutedEventArgs e)
        {
            dgData.Bridge = this.Bridge;
            dgData.OnExecutedCopy(BrigeCopyType.NoBroker);
        }
    }
}
