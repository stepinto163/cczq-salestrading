using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Drp;
using STC.Common.Handler.PT;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using STC.Common.ViewModel.Drp;
using STC.Common.ViewModel.Drp.Component;
using STC.Common.ViewModel.Drp.Component.Broker;
using STC.Common.ViewModel.Drp.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Common.View.Drp.Component
{
    public class NotificationArea : Control
    {
        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));


        public int MaxItems
        {
            get { return (int)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }

        public static readonly DependencyProperty MaxItemsProperty = DependencyProperty.Register("MaxItems", typeof(int), typeof(NotificationArea), new PropertyMetadata(int.MaxValue));

        public string AreaName
        {
            get { return (string)GetValue(AreaNameProperty); }
            set { SetValue(AreaNameProperty, value); }
        }

        public static readonly DependencyProperty AreaNameProperty =
            DependencyProperty.Register("AreaName", typeof(string), typeof(NotificationArea), new PropertyMetadata(""));

        public IList _items;

        private object lockobj = new object();

        /// <summary>
        /// 窗口数量变化
        /// </summary>
        public event Action<object> NotificationSizeChanged;

        /// <summary>
        /// Screen DPI
        /// </summary>
        public double dpi;
        
        /// <summary>
        /// 可显示区域高度
        /// </summary>
        public int areaheight;


        private int remainingHeight;

        /// <summary>
        /// 剩余高度
        /// </summary>
        public int RemainingHeight
        {
            get 
            {
                return remainingHeight;
            }
            set 
            {
                if (value != remainingHeight)
                {
                    remainingHeight = value;
                    if (remainingHeight < 238)
                    {
                        RemainingCount = -1;
                    }
                    else
                    {
                        double _remain = remainingHeight / 238;
                        RemainingCount = Math.Round(_remain, 0);
                    }
                }
            }
        }

        //默认数量只需>0即可 
        private double remainingCount = 4;

        /// <summary>
        /// 剩余可停靠数量，当小于0 时，代表无空间了，需要换屏显示
        /// </summary>
        public double RemainingCount
        {
            get
            {
                return remainingCount;
            }
            set
            {
                if (value != remainingCount)
                {
                    remainingCount = value;
                    NotificationSizeChanged?.Invoke(this);
                    //STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "ShowToolTip", Msg = $"{remainingHeight} {RemainingCount}" });
                }
            }
        }

        public void Add()
        {
            NotificationManager.AddArea(this);
        }

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea), new FrameworkPropertyMetadata(typeof(NotificationArea)));
            
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var itemsControl = GetTemplateChild("PART_Items") as Panel;
            _items = itemsControl?.Children;
        }

        public async void Show(object content, TimeSpan expirationTime, Action onClick, Action onClose)
        {
            NotificationContent nc = (NotificationContent)content;
            
            switch (nc.ContentType.ToUpper())
            {
                case "DQ":
                case "DQB":
                    #region 定向回价 
                    DRPNotificationViewModel dnvm = DRPTools.DrpContentToViewModel(nc);

                    lock (lockobj)
                    {
                        string delflag = dnvm.DelFlag ?? "0";
                        bool idproced = false;
                        if (_items.OfType<NotificationWindow>().Count(i => i.MsgId == dnvm.Id) > 0)
                        {
                            if (delflag == "1")
                            {
                                _items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).Close();
                            }
                            else
                            {
                                //如果有回价，则闪烁提醒
                                if ((_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).DrpDetail.iocOrderDqList.Count != dnvm.DrpDetail.iocOrderDqList.Count)
                                {
                                    _items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).ShowNewMsgAlert();
                                }
                                //只更新界面的表格数据
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).Message = dnvm.Message;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).LeavesQtyTotal = dnvm.LeavesQtyTotal;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).ShowPrice = dnvm.ShowPrice;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).ShowSettle = dnvm.ShowSettle;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).ShowSourceSettle = dnvm.ShowSourceSettle;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).ShowLeavesQtyTotal = dnvm.ShowLeavesQtyTotal;
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).UpdateGridDataData(dnvm.GridDatas);
                                 ObservableCollection<DRPListModel> Tmp = new ObservableCollection<DRPListModel>(dnvm.GridDatas.Where(p => p.EnabledBtn));
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).IocCount = $"{Tmp.Count}";
                                (_items.OfType<NotificationWindow>().First(i => i.MsgId == dnvm.Id).DataContext as DRPNotificationViewModel).SetFirstBestPriceSelected();
                            }


                            idproced = true;

                        }

                        if (delflag == "1")
                        {
                            if (_items.OfType<NofirmPointPriceView>().Count(i => i.MsgId == $"PP{dnvm.DrpDetail.order.orderId}") > 0)
                            {
                                _items.OfType<NofirmPointPriceView>().First(i => i.MsgId == $"PP{dnvm.DrpDetail.order.orderId}").Close();
                            }

                            idproced = true;
                        }

                        if (idproced)
                        {
                            return;
                        }
                    }

                    dnvm.SetFirstBestPriceSelected();

                    if (dnvm.ContentType == "DQB")
                    {
                        //1 - 议价，2 - 成交，3 - 拒绝
                        return;
                    }

                    if (!dnvm.IsShow)
                    {
                        return;
                    }

                    NotificationWindow notification = new NotificationWindow();
                    notification.MsgId = dnvm.Id;
                    notification.DataContext = dnvm;
                    notification.NotificationClosed += (sender, args) => onClose?.Invoke();
                    notification.NotificationClosed += OnNotificationClosed;
                    notification.ShowNewMsgAlert();
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var w = Window.GetWindow(this);
                    var x = PresentationSource.FromVisual(w);
                    if (x == null)
                    {
                        return;
                    }

                    lock (lockobj)
                    {
                        _items.Add(notification);

                        if (_items.OfType<NotificationWindow>().Count(i => !i.IsClosing) > MaxItems)
                        {
                            _items.OfType<NotificationWindow>().First(i => !i.IsClosing).Close();
                        }

                    }


                    if (expirationTime != null)
                    {
                        if (expirationTime == TimeSpan.MaxValue)
                        {
                            return;
                        }

                        await Task.Delay(expirationTime);
                        notification.Close();
                    }

                    break;
                #endregion
                case AdminConsts.Admin_ER:
                    //成交回报,收到成交回报更新剩余量
                    if (!string.IsNullOrEmpty(nc.Detail))
                    {
                        ExecutionReport er = Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(nc.Detail);
                        if (_items.OfType<NotificationWindow>().Count(i => i.MsgId == $"DQLimit_{er.QuoteID}") > 0)
                        {
                            //只更新界面的表格数据
                            decimal _LeavesQty = decimal.Parse(er.LeavesQty);
                            (_items.OfType<NotificationWindow>().First(i => i.MsgId == $"DQLimit_{er.QuoteID}").DataContext as DRPNotificationViewModel).LeavesQtyTotal = _LeavesQty;
                            string _ShowLeavesQtyTotal = _LeavesQty.ToString() == "0" ? "--" : _LeavesQty.ToString();
                            (_items.OfType<NotificationWindow>().First(i => i.MsgId == $"DQLimit_{er.QuoteID}").DataContext as DRPNotificationViewModel).ShowLeavesQtyTotal = _ShowLeavesQtyTotal;
                        }

                        return;
                    }

                    break;
                case AdminConsts.Admin_IT:
                    #region 点价提醒
                    PointPriceDetailModel ppdm = null;

                    if (!string.IsNullOrEmpty(nc.Detail))
                    {
                        ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(nc.Detail);
                        string delflag = nc.DelFlag ?? "0";
                        lock (lockobj)
                        {
                            if (_items.OfType<NofirmPointPriceView>().Count(i => i.MsgId == $"PP{ppdm.order.orderId}") > 0)
                            {
                                //if (delflag == "1")
                                //{
                                //    _items.OfType<NofirmPointPriceView>().First(i => i.MsgId == $"PP{ppdm.order.orderId}").Close();
                                //    return;
                                //}
                            }
                        }

                        if (!nc.IsShow)
                        {
                            return;
                        }
                    }

                    lock (lockobj)
                    {
                        // 更新被点价窗口
                        if (_items.OfType<NofirmPointPriceView>().Count(i => i.MsgId == $"PP{ppdm.order.orderId}") > 0)
                        {
                            NofirmPointPriceViewModel vm2 = DRPTools.NotificationToNofirmPointPrice(nc);
                            NofirmPointPriceView curwin = _items.OfType<NofirmPointPriceView>().First(i => i.MsgId == $"PP{ppdm.order.orderId}");
                            vm2.ParentWindow = curwin;
                            curwin.DataContext = vm2;
                            curwin.ShowNewMsgAlert();
                            return;
                        }
                    }

                    NofirmPointPriceViewModel vm = DRPTools.NotificationToNofirmPointPrice(nc);

                    NofirmPointPriceView notificationPP = new NofirmPointPriceView();
                    notificationPP.MsgId = $"PP{ppdm.order.orderId}";
                    notificationPP.DataContext = vm;
                    notificationPP.NotificationClosed += (sender, args) => onClose?.Invoke();
                    notificationPP.NotificationClosed += OnNotificationClosed;
                    notificationPP.ShowNewMsgAlert();
                    vm.ParentWindow = notificationPP;
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var wpp = Window.GetWindow(this);
                    var xpp = PresentationSource.FromVisual(wpp);
                    if (xpp == null)
                    {
                        return;
                    }

                    lock (lockobj)
                    {
                        _items.Add(notificationPP);

                        if (_items.OfType<NofirmPointPriceView>().Count(i => !i.IsClosing) > MaxItems)
                        {
                            _items.OfType<NofirmPointPriceView>().First(i => !i.IsClosing).Close();
                        }
                    }

                    //if (expirationTime != null)
                    //{
                    //    expirationTime = TimeSpan.FromSeconds(int.Parse(vm.DrpDetail.iocOrder.iocTimeOut));
                    //    if (expirationTime == TimeSpan.MaxValue)
                    //    {
                    //        return;
                    //    }

                    //    await Task.Delay(expirationTime);
                    //    notificationPP.Close();
                    //}

                    break;
                #endregion

                case AdminConsts.PriceMatch:
                    //接收价格匹配消息，用于拒绝点价时，接收拒绝消息，关闭被点价窗口 只处理 "delflag": "1",
                    if (!string.IsNullOrEmpty(nc.Detail))
                    {
                        ExecutionReport er = Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(nc.Detail);
                        string delflag = nc.DelFlag ?? "0";
                        if (_items.OfType<NofirmPointPriceView>().Count(i => i.MsgId == $"PP{er.QuoteID}") > 0)
                        {
                            if (delflag == "1")
                            {
                                _items.OfType<NofirmPointPriceView>().First(i => i.MsgId == $"PP{er.QuoteID}").Close();
                            }
                        }

                        return;
                    }

                    break;
                case "TABLESVR_ORDER_UNREF":
                    //日初UnRef消息
                    LastWorkDayUnRefMsgModel unrefmsgmodel = JsonConvert.DeserializeObject<LastWorkDayUnRefMsgModel>(nc.Message);
                    UnRefContentModel unrefcontentmodel = JsonConvert.DeserializeObject<UnRefContentModel>(unrefmsgmodel.content);
                    UnRefMsgTypeModel unrefmsgtypemodel = JsonConvert.DeserializeObject<UnRefMsgTypeModel>(unrefcontentmodel.remark);
                    if (!string.IsNullOrEmpty(nc.Message))
                    {
                        string delflag = nc.DelFlag ?? "0";

                        if (!string.IsNullOrEmpty(unrefmsgmodel.readflag))
                        {
                            if (unrefmsgmodel.readflag == "0")
                                delflag = "0";
                            else
                                delflag = "1";
                        }

                        bool isfindwindow = false;
                        lock (lockobj)
                        {
                            if (_items.OfType<LastWorkDayUnRefView>().Count(i => i.MsgId == $"UnRef{unrefmsgtypemodel.type}") > 0)
                            {
                                isfindwindow = true;
                                if (delflag == "1")
                                {
                                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FocusAndRefresh_InnerHistoryRequireView", Msg = string.Empty });
                                    _items.OfType<LastWorkDayUnRefView>().First(i => i.MsgId == $"UnRef{unrefmsgtypemodel.type}").Close();
                                }
                                else
                                {
                                    unrefcontentmodel.type = unrefmsgtypemodel.type;
                                    unrefcontentmodel.orderTotal = unrefmsgtypemodel.orderTotal;
                                    (_items.OfType<LastWorkDayUnRefView>().First(i => i.MsgId == $"UnRef{unrefmsgtypemodel.type}").DataContext as LastWorkDayUnRefViewModel).ContentModel = unrefcontentmodel;
                                    _items.OfType<LastWorkDayUnRefView>().First(i => i.MsgId == $"UnRef{unrefmsgtypemodel.type}").ShowNewMsgAlert();
                                }

                                return;
                            }
                        }

                        if (delflag == "1")
                        {
                            if (!isfindwindow)
                            {
                                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TableSvr_FocusAndRefresh_InnerHistoryRequireView", Msg = string.Empty });
                            }

                            return;
                        }
                        
                        if (!nc.IsShow)
                        {
                            return;
                        }
                    }

                    LastWorkDayUnRefViewModel vmunref = new LastWorkDayUnRefViewModel();
                    vmunref.MsgModel = unrefmsgmodel;
                    unrefcontentmodel.type = unrefmsgtypemodel.type;
                    unrefcontentmodel.orderTotal = unrefmsgtypemodel.orderTotal;
                    vmunref.ContentModel = unrefcontentmodel;

                    vmunref.Id = nc.Id;

                    LastWorkDayUnRefView notificationNpUR = new LastWorkDayUnRefView();
                    notificationNpUR.MsgId = $"UnRef{unrefmsgtypemodel.type}";
                    notificationNpUR.DataContext = vmunref;
                    notificationNpUR.NotificationClosed += (sender, args) => onClose?.Invoke();
                    notificationNpUR.NotificationClosed += OnNotificationClosed;
                    notificationNpUR.ShowNewMsgAlert();
                    vmunref.ParentWindow = notificationNpUR;
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var nrpp = Window.GetWindow(this);
                    var nxxpp = PresentationSource.FromVisual(nrpp);
                    if (nxxpp == null)
                    {
                        return;
                    }

                    lock (lockobj)
                    {
                        _items.Add(notificationNpUR);

                        if (_items.OfType<LastWorkDayUnRefView>().Count(i => !i.IsClosing) > MaxItems)
                        {
                            _items.OfType<LastWorkDayUnRefView>().First(i => !i.IsClosing).Close();
                        }
                    }


                    break;
                case AdminConsts.Admin_MM:
                    #region 双边断报提醒
                    MMBreakAlarmModel mMBreak = null;

                    if (!string.IsNullOrEmpty(nc.Detail))
                    {
                        mMBreak = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(nc.Detail);
                        string delflag = nc.DelFlag ?? "0";
                        lock (lockobj)
                        {
                            if (_items.OfType<MMBreakAlarmReminder>().Count(i => i.MsgId == $"ANS{nc.Id}") > 0)
                            {
                                if (delflag == "1")
                                {
                                    _items.OfType<MMBreakAlarmReminder>().First(i => i.MsgId == $"ANS{nc.Id}").Close();
                                    return;
                                }
                            }
                            else 
                            {
                                if (delflag == "1") 
                                {
                                    return;
                                }
                            }
                        }

                        if (!nc.IsShow)
                        {
                            return;
                        }
                    }

                    lock (lockobj)
                    {
                        // 更新窗口
                        if (_items.OfType<MMBreakAlarmReminder>().Count(i => i.MsgId == $"ANS{nc.Id}") > 0)
                        {
                            MMBreakAlarmReminderViewModel vm2 = DRPTools.setBeakAlarmReminder(nc);
                            MMBreakAlarmReminder curwin = _items.OfType<MMBreakAlarmReminder>().First(i => i.MsgId == $"ANS{nc.Id}");
                            vm2.ParentWindow = curwin;
                            curwin.DataContext = vm2;
                            curwin.ShowNewMsgAlert();
                            return;
                        }
                    }

                    MMBreakAlarmReminderViewModel mMBreakAlarm = DRPTools.setBeakAlarmReminder(nc);

                    MMBreakAlarmReminder mMBreakAlarmReminder = new MMBreakAlarmReminder();
                    mMBreakAlarmReminder.MsgId = $"ANS{nc.Id}";
                    mMBreakAlarmReminder.DataContext = mMBreakAlarm;
                    mMBreakAlarmReminder.NotificationClosed += (sender, args) => onClose?.Invoke();
                    mMBreakAlarmReminder.NotificationClosed += OnNotificationClosed;
                    mMBreakAlarmReminder.ShowNewMsgAlert();
                    mMBreakAlarm.ParentWindow = mMBreakAlarmReminder;
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var wppm = Window.GetWindow(this);
                    var xppm = PresentationSource.FromVisual(wppm);
                    if (xppm == null)
                    {
                        return;
                    }

                    lock (lockobj)
                    {
                        _items.Add(mMBreakAlarmReminder);

                        if (_items.OfType<MMBreakAlarmReminder>().Count(i => !i.IsClosing) > MaxItems)
                        {
                            _items.OfType<MMBreakAlarmReminder>().First(i => !i.IsClosing).Close();
                        }
                    }

                    break;
                    #endregion
            }
        }

        /// <summary>
        /// //线下录入
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expirationTime"></param>
        /// <param name="onClick"></param>
        /// <param name="onClose"></param>
        /// <param name="key"></param>
        public void ShowUnderlineExec(object content, TimeSpan expirationTime, Action onClick, Action onClose, string key)
        {
            NotificationContent nc = (NotificationContent)content;
            List<ExecutionReport> list = JsonConvert.DeserializeObject<List<ExecutionReport>>(nc.Message);
            list.OrderByDescending(a => a.TransactTime).ToList();
            foreach (ExecutionReport po in list)
            {

                lock (lockobj)
                {
                    // 更新窗口
                    if (_items.OfType<UnderlineExecTpicUI>().Count(i => i.MsgId == key) > 0)
                    {
                        if ("1".Equals(po.DelFlag ?? "0") || po.OrdStatus == "G" || po.CommandType == "24" || po.CommandType == "25" || po.CommandType == "26")
                        {
                            _items.OfType<UnderlineExecTpicUI>().First(i => i.MsgId == key).Close();
                            return;
                        }
                        UnderlineExecTpicUI updateUI = _items.OfType<UnderlineExecTpicUI>().First(i => i.MsgId == key);
                        UnderlineExecTpicUIViewModel updateModel = (UnderlineExecTpicUIViewModel)updateUI.DataContext;
                        updateModel.BindingData(po);
                        LogHelper.Info("return {0}", key);
                        return;
                    }
                    else if (po.OrdStatus == "G" || po.OrdStatus == "2")
                    {
                        return;
                    }

                    if (!nc.IsShow)
                    {
                        return;
                    }
                }

                UnderlineExecTpicUI orderMatchFailedUI = new UnderlineExecTpicUI();
                orderMatchFailedUI.SetShowData(po, key, orderMatchFailedUI);
                orderMatchFailedUI.NotificationClosed += (sender, args) => onClose?.Invoke();
                orderMatchFailedUI.NotificationClosed += OnNotificationClosed;
                orderMatchFailedUI.ShowNewMsgAlert();
                if (!IsLoaded)
                {
                    return;
                }

                var wppm = Window.GetWindow(this);
                var xppm = PresentationSource.FromVisual(wppm);
                if (xppm == null)
                {
                    return;
                }

                lock (lockobj)
                {
                    _items.Add(orderMatchFailedUI);

                    if (_items.OfType<UnderlineExecTpicUI>().Count(i => !i.IsClosing) > MaxItems)
                    {
                        _items.OfType<UnderlineExecTpicUI>().First(i => !i.IsClosing).Close();
                    }
                }
            }
        }

        /// <summary>
        /// //点价失败
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expirationTime"></param>
        /// <param name="onClick"></param>
        /// <param name="onClose"></param>
        /// <param name="key"></param>
        public void ShowOrderMatchFailed(object content, TimeSpan expirationTime, Action onClick, Action onClose, string key)
        {
            NotificationContent nc = (NotificationContent)content;
            List<ExecutionReport> list = JsonConvert.DeserializeObject<List<ExecutionReport>>(nc.Message);
            list.OrderByDescending(a => a.TransactTime).ToList();
            ExecutionReport po = list[0];
            if (CommonUtils.ConvertToLong(po.CumQty) <= 0 && ("4".Equals(po.OrdStatus) || "8".Equals(po.OrdStatus)))
            {
                //过滤掉第三方弹框
                if (LocalEnv.UserInfo.IsMyself(po.UserID + "") || LocalEnv.UserInfo.IsMyself(po.CounterPartyTraderId + ""))
                {
                    DateTime today = DateTime.Today.Date;
                    //交易未匹配,弹出提醒

                    lock (lockobj)
                    {
                        // 更新窗口
                        if (_items.OfType<OrderMatchFailedUI>().Count(i => i.MsgId == key) > 0)
                        {
                            LogHelper.Info("return {0}", key);
                            return;
                        }
                        if (!nc.IsShow)
                        {
                            return;
                        }
                    }
                       
                    OrderMatchFailedUI orderMatchFailedUI = new OrderMatchFailedUI();
                    orderMatchFailedUI.SetShowData(po, key, orderMatchFailedUI);
                    orderMatchFailedUI.NotificationClosed += (sender, args) => onClose?.Invoke();
                    orderMatchFailedUI.NotificationClosed += OnNotificationClosed;
                    orderMatchFailedUI.ShowNewMsgAlert();
                    if (!IsLoaded)
                    {
                        return;
                    }

                    var wppm = Window.GetWindow(this);
                    var xppm = PresentationSource.FromVisual(wppm);
                    if (xppm == null)
                    {
                        return;
                    }

                    lock (lockobj)
                    {
                        _items.Add(orderMatchFailedUI);

                        if (_items.OfType<OrderMatchFailedUI>().Count(i => !i.IsClosing) > MaxItems)
                        {
                            _items.OfType<OrderMatchFailedUI>().First(i => !i.IsClosing).Close();
                        }
                    }

                }
            }
        }

        /// <summary> 
        /// //点价成功，我的客户发来意向窗口  //收到订单状态  //收到的成交处理消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expirationTime"></param>
        /// <param name="onClick"></param>
        /// <param name="onClose"></param>
        /// <param name="isOrder"></param>
        /// <param name="key"></param>
        public void ShowIntention(object content, TimeSpan expirationTime, Action onClick, Action onClose, bool isOrder, string key)
        {
            bool isContains = DrpHandler.Instance.DictIntention.ContainsKey(key);

            NotificationContent nc = (NotificationContent)content;
            List<ExecutionReport> list = JsonConvert.DeserializeObject<List<ExecutionReport>>(nc.Message);

            IntentionUI ui = null;
            if (isContains)
            {
                if (_items.OfType<IntentionUI>().Count(i => i.MsgId == key) == 0)
                {
                    ui = new IntentionUI();
                }
                else
                {
                    if (nc.DelFlag == "1")
                    {
                        if (_items.OfType<IntentionUI>().Count(i => i.MsgId == key) > 0)
                            _items.OfType<IntentionUI>().First(i => i.MsgId == key).Close();
                    }
                    else
                    {
                        ui = _items.OfType<IntentionUI>().First(i => i.MsgId == key);
                        IntentionUIViewModel updateModel = (IntentionUIViewModel)ui.DataContext;
                        if (list[0].OrdStatus != "N")
                            updateModel.UpdateData(list);
                    }

                    return;
                }
            }
            else
            {
                ui = new IntentionUI();
            }

            
            ui.InitData(list, ui, key);
            ui.NotificationClosed += (sender, args) => onClose?.Invoke();
            ui.NotificationClosed += OnNotificationClosed;

            lock (lockobj)
            {
                _items.Add(ui);
                ui.ShowNewMsgAlert();

                if (_items.OfType<IntentionUI>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<IntentionUI>().First(i => !i.IsClosing).Close();
                }
            }
        }

        private ConcurrentDictionary<string, string> DictFeedBackAUI = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 收到点价反馈 一个窗口有多个
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expirationTime"></param>
        /// <param name="onClick"></param>
        /// <param name="onClose"></param>
        /// <param name="key"></param>
        public void ShowFeedBackAui(object content, TimeSpan expirationTime, Action onClick, Action onClose, string key)
        {
            //如果没有包含则创建
            NotificationContent nc = (NotificationContent)content;
            List<ExecutionReport> list = JsonConvert.DeserializeObject<List<ExecutionReport>>(nc.Message);
            bool isContains = false;

            lock (lockDict)
            {
                if (!DictFeedBackAUI.ContainsKey(key))
                {
                    if (!nc.IsShow) 
                    {
                       return;
                    }

                    DictFeedBackAUI.TryAdd(key, key);
                }
                else
                {
                    isContains = true;
                }
            }

            //客户端自动确认流程，如果系统配置了自动确认A
            bool ishaveautoconfirm = false;
            foreach (ExecutionReport po in list)
            {
                if (po.OrdStatus.Equals("E") && po.OrderQty.Equals(po.LastQty) && "Y".Equals(LocalEnv.BrokerAutoConfirmParam.brokerAAutoConfirm_BrokerBFiledConfirm))
                {
                    AutoConfirmAWhenBrokerBConfirm(po);
                    ishaveautoconfirm = true;
                }

                if (po.OrdStatus.Equals("E") && !po.OrderQty.Equals(po.LastQty) && "Y".Equals(LocalEnv.BrokerAutoConfirmParam.brokerAAutoConfirm_BrokerBPartiallyFiledConfirm))
                {
                    AutoConfirmAWhenBrokerBConfirm(po);
                    ishaveautoconfirm = true;
                }
            }

            if (list.Count(p => p.OrdStatus == "2") == list.Count)
            {

            }
            else
            {
                if (ishaveautoconfirm)
                {
                    if (list.Count(p => p.OrdStatus == "2") == 0 && list.Count(p => p.OrdStatus == "E" || p.OrdStatus == "F") == 0)
                    {
                        string ok = string.Empty;
                        DictFeedBackAUI.TryRemove(key, out ok);
                        return;
                    }
                }
            }

            //如果是IDB撤单
            if (list.Count(p => p.OrdStatus == "4") == list.Count)
            {
                if (_items.OfType<FeedBackAUI>().Count(i => i.MsgId == key) == 0)
                {
                    string ok = string.Empty;
                    DictFeedBackAUI.TryRemove(key, out ok);
                    return;
                }
            }

            FeedBackAUI ui = null;
            if (isContains)
            {
                if (_items.OfType<FeedBackAUI>().Count(i => i.MsgId == key) == 0)
                {
                    ui = new FeedBackAUI();
                }
                else
                {
                    if (_items.OfType<FeedBackAUI>().Count(i => i.MsgId == key) > 0)
                    {
                        ui = _items.OfType<FeedBackAUI>().First(i => i.MsgId == key);
                        FeedBackAUIViewModel updateModel = (FeedBackAUIViewModel)ui.DataContext;
                        updateModel.UpdateData(list);
                        return;
                    }
                    else
                    {
                        ui = new FeedBackAUI();
                    }
                }
            }
            else
            {
                ui = new FeedBackAUI();
            }


            ui.InitData(list, ui, key);
            ui.NotificationClosed += (sender, args) => onClose?.Invoke();
            ui.NotificationClosed += OnNotificationClosed;

            lock (lockobj)
            {
                _items.Add(ui);
                ui.ShowNewMsgAlert();

                if (_items.OfType<FeedBackAUI>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<FeedBackAUI>().First(i => !i.IsClosing).Close();
                }
            }
            
        }

        private object lockDict = new object();
        private object lockIntentionB = new object();
        /// <summary>
        /// 我的客户被点价 确认窗口
        /// </summary>
        /// <param name="content"></param>
        /// <param name="expirationTime"></param>
        /// <param name="onClick"></param>
        /// <param name="onClose"></param>
        /// <param name="key"></param>
        public void ShowIntentionB(object content, TimeSpan expirationTime, Action onClick, Action onClose, string key)
        {
            bool isContains = DrpHandler.Instance.DictIntentionB.ContainsKey(key);

            NotificationContent nc = (NotificationContent)content;
            List<ExecutionReport> list = JsonConvert.DeserializeObject<List<ExecutionReport>>(nc.Message);

            string tmpkey = string.Empty;
            //未确认条数，只要有未确认的就打开窗口
            int notConfirmCount = list.Count(p => p.OrdStatus == "2");
            //是否是完全拒绝或只拒绝一部分
            int oneconfirmonecancel = list.Count(p => p.CommandType == "24" || p.CommandType == "25");

            if (notConfirmCount == 0)
            {
                foreach (ExecutionReport po in list)
                {
                    //点价确认或拒绝后要关闭窗口 BrokerBConfirm("E"), BrokerBCanceled("F")
                    if (po.OrdStatus.Equals("E"))
                    {
                        lock (lockDict)
                        {
                            DrpHandler.Instance.DictIntentionB.TryRemove(key, out tmpkey);
                        }
                        if (_items.OfType<IntentionBUI>().Count(i => i.MsgId == key) > 0)
                            _items.OfType<IntentionBUI>().First(i => i.MsgId == key)?.Close();
                        return;
                    }

                    if (po.OrdStatus == "G" || po.OrdStatus == "I")
                    {
                        lock (lockDict)
                        {
                            DrpHandler.Instance.DictIntentionB.TryRemove(key, out tmpkey);
                        }

                        if (_items.OfType<IntentionBUI>().Count(i => i.MsgId == key) > 0)
                            _items.OfType<IntentionBUI>().First(i => i.MsgId == key)?.Close();
                        return;
                    }

                    if ("1".Equals(po.DelFlag))
                    {
                        lock (lockDict)
                        {
                            DrpHandler.Instance.DictIntentionB.TryRemove(key, out tmpkey);
                        }
                        if (_items.OfType<IntentionBUI>().Count(i => i.MsgId == key) > 0)
                            _items.OfType<IntentionBUI>().First(i => i.MsgId == key)?.Close();
                        return;
                    }

                    //拒绝
                    if (po.OrdStatus.Equals("F"))
                    {
                        if (_items.OfType<IntentionBUI>().Count(i => i.MsgId == key) > 0)
                        {
                            lock (lockDict)
                            {
                                DrpHandler.Instance.DictIntentionB.TryRemove(key, out tmpkey);
                            }
                            _items.OfType<IntentionBUI>().First(i => i.MsgId == key)?.Close();
                            return;
                        }
                    }
                }
            }

            IntentionBUI ui = null;
            if (isContains)
            {
                if (_items.OfType<IntentionBUI>().Count(i => i.MsgId == key) == 0)
                {
                    ui = new IntentionBUI();
                }
                else
                {
                    ui = _items.OfType<IntentionBUI>().First(i => i.MsgId == key);
                    IntentionBUIViewModel updateModel = (IntentionBUIViewModel)ui.DataContext;
                    updateModel.UpdateData(list);
                    return;
                }
            }
            else
            {
                ui = new IntentionBUI();
            }


            ui.InitData(list, ui, key);
            ui.NotificationClosed += (sender, args) => onClose?.Invoke();
            ui.NotificationClosed += OnNotificationClosed;

            lock (lockobj)
            {
                _items.Add(ui);
                ui.ShowNewMsgAlert();

                if (_items.OfType<IntentionBUI>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<IntentionBUI>().First(i => !i.IsClosing).Close();
                }
            }
        }


        public void ShowPTListBroker(object content, TimeSpan expirationTime, Action onClick, Action onClose,string contentType,string key)
        {
            NotificationContent nc = (NotificationContent)content;

            if (contentType.Equals("ET"))
            {
                //线下录入
                ShowUnderlineExec(content, (TimeSpan)expirationTime, onClick, onClose, key);
            } 
            else if (contentType.Equals("OMF"))
            {
                //点价失败
                ShowOrderMatchFailed(content, (TimeSpan)expirationTime, onClick, onClose, key);
            }
            else if (contentType.Equals("IN"))
            {
                //我的客户发来意向窗口  //收到订单状态
                if ("N".Equals(LocalEnv.BrokerAutoConfirmParam.brokerAShowBox_BrokerAFilledNew))
                {
                    return;
                }

                ShowIntention(content, (TimeSpan)expirationTime, onClick, onClose,true, key);
            }
            else if (contentType.Equals("FB"))
            {
                //收到点价反馈 BrokerA
                ShowFeedBackAui(content, (TimeSpan)expirationTime, onClick, onClose, key);
            }
            else if (contentType.Equals("INB"))
            {
                //我的客户被点价 BrokerB
                ShowIntentionB(content, (TimeSpan)expirationTime, onClick, onClose, key);
            }
            else if (contentType.Equals("CIN"))
            {
                //全部为终态关掉我的意向弹框
                ClosePopupUI(content, (TimeSpan)expirationTime, onClick, onClose, key);
            }

        }

        private void ClosePopupUI(object content, TimeSpan expirationTime, Action onClick, Action onClose, string key)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (lockobj)
                {
                    // 关闭我的客户发来意向弹窗
                    if (_items.OfType<IntentionUI>().Count(i => i.MsgId == (key)) > 0)
                    {
                        lock (lockDict)
                        {
                            string tmpkey = string.Empty;
                            DrpHandler.Instance.DictIntention.TryRemove(key, out tmpkey);
                        }

                        _items.OfType<IntentionUI>().First(i => i.MsgId == (key)).Close();
                    }
                }
            });
        }


        /// <summary>
        /// 子窗体手工或自动关闭触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                if (sender is NotificationWindow)
                {
                    var notification = sender as NotificationWindow;
                    _items.Remove(notification);

                }
                else if (sender is NofirmPointPriceView)
                {
                    var notification = sender as NofirmPointPriceView;
                    _items.Remove(notification);
                }
                else if (sender is LastWorkDayUnRefView)
                {
                    var notification = sender as LastWorkDayUnRefView;
                    (notification.DataContext as LastWorkDayUnRefViewModel).OnRefuseCommand();
                    _items.Remove(notification);
                }
                else if (sender is MMBreakAlarmReminder)
                {
                    var notification = sender as MMBreakAlarmReminder;
                    _items.Remove(notification);
                }
                else if (sender is OrderMatchFailedUI)
                {
                    var notification = sender as OrderMatchFailedUI;
                    _items.Remove(notification);
                }
                else if (sender is UnderlineExecTpicUI)
                {
                    var notification = sender as UnderlineExecTpicUI;
                    _items.Remove(notification);
                }
                else if (sender is IntentionUI)
                {
                    var notification = sender as IntentionUI;
                    lock (lockDict)
                    {
                        string key = string.Empty;
                        DrpHandler.Instance.DictIntention.TryRemove(notification.MsgId, out key);
                    }
                    _items.Remove(notification);
                }
                else if (sender is IntentionBUI)
                {
                    IntentionBUI notification = sender as IntentionBUI;
                    lock (lockDict)
                    {
                        string key = string.Empty;
                        DrpHandler.Instance.DictIntentionB.TryRemove(notification.MsgId, out key);
                    }
                    _items.Remove(notification);
                }
                else if (sender is FeedBackAUI)
                {
                    var notification = sender as FeedBackAUI;
                    lock (lockDict)
                    {
                        string key = string.Empty;
                        DictFeedBackAUI.TryRemove(notification.MsgId, out key);
                    }
                    _items.Remove(notification);
                }

                NotificationSizeChanged?.Invoke(this);
            }
            catch 
            {
                LogHelper.Info("Notification Closed ...");
            }
        }

        /// <summary>
        /// 自动确认
        /// </summary>
        /// <param name="po"></param>
        private void AutoConfirmAWhenBrokerBConfirm(ExecutionReport po)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        ExecutionReport er = new ExecutionReport();
                        er.LastQty = po.LastQty;
                        er.OrdStatus = "G";
                        er.CommandType = "24";
                        er.ExecID = po.ExecID;
                        er.CloseBy = LocalEnv.UserInfo.UserName;
                        er.CloseById = LocalEnv.UserInfo.UserId;
                        PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                        if (reply != null && reply.Code.Equals("0000"))
                        {

                        }
                        else
                        {
                            LogHelper.Error($"AutoConfirmAWhenBrokerBConfirm Failed:{reply.Msg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Info($"AutoConfirmAWhenBrokerBConfirm Failed:{ex.Message}");
                    }
                });
            });
        }
    }

    public enum NotificationPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}