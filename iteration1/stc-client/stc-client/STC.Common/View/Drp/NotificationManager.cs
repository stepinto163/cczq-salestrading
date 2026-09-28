using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Drp.Component;
using STC.Common.View.Drp.Component.Broker;
using STC.Common.ViewModel.Drp;
using STC.Common.ViewModel.Drp.Utils;
using Point = System.Windows.Point;

namespace STC.Common.View.Drp
{
    public class NotificationManager : INotificationManager
    {
        private static NotificationManager _instance = new NotificationManager();
        public static NotificationManager Instance
        {
            get { return _instance; }
        }

        private static readonly List<NotificationArea> Areas = new List<NotificationArea>();
        private static NotificationsOverlayWindow _window;

        public Dictionary<string, List<NotificationsOverlayWindow>> LstScreen = new Dictionary<string, List<NotificationsOverlayWindow>>();


        private ConcurrentQueue<NotificationContent> queue = new ConcurrentQueue<NotificationContent>();
        private object lockqueue = new object();
        private Thread NotificationThread = null;
        public bool StopRefreshER = false;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private NotificationManager()
        {
            if (NotificationThread == null)
            {
                NotificationThread = new Thread(RefreshNM);
                NotificationThread.IsBackground = true;
                NotificationThread.Start();
            }
        }

        private void RefreshNM()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    NotificationContent item = null;

                    lock (lockqueue)
                    {
                        if (queue.TryDequeue(out item))
                        {
                            if (item != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                                {
                                    int oldcount = item.CurrArea._items.Count;
                                    item.CurrArea.ShowPTListBroker(item, (TimeSpan)item.expirationTime, item.onClick, item.onClose, item.contentType, item.contentskey);
                                    if (item.CurrArea._items.Count > oldcount)
                                    {
                                        (item.CurrArea.Parent as NotificationsOverlayWindow).Activate();
                                    }
                                }));
                            }
                        }
                    }

                    if (queue.Count == 0)
                        _autoResetEvent.WaitOne();

                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshNM Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshNM exception:", ex);
                }
            }
        }

        public void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(12 * 60 * 60);

                    System.Windows.Forms.Screen mainScreen = (content as NotificationContent).CurScreen;
                    Rectangle workArea = mainScreen.WorkingArea;
                    Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                    double dpi = (int)(graphics.DpiX * 1.041666667) / 100.0;
                    double ucwidth = 445;
                    double ucheight = 234;
                    double ucsplitewidth = 10;
                    double ucspliteheight = 4;


                    //每列显示多少提示
                    int maxrowcount = int.Parse(Math.Floor((workArea.Height / dpi) / (ucheight + ucspliteheight)).ToString());
                    //每屏幕显示多少列
                    int maxcolcount = int.Parse(Math.Floor((workArea.Width / dpi) / (ucwidth + ucsplitewidth)).ToString());

                    // 原始窗口是否有弹出式消息
                    bool IsPrevScreenHaveReccord = false;

                    if (!LstScreen.ContainsKey(mainScreen.DeviceName))
                    {
                        if (LstScreen.Count > 0)
                        {

                            List<string> keys = LstScreen.Keys.ToList();
                            foreach (string key in keys)
                            {
                                List<NotificationsOverlayWindow> ss = LstScreen[key].ToList();
                                for (int i = 0; i < ss.Count; i++)
                                {
                                    if (ss[i].CtrlArea._items.Count > 0)
                                    {
                                        IsPrevScreenHaveReccord = true;
                                        break;
                                    }
                                }
                            }

                            if (!IsPrevScreenHaveReccord)
                            {
                                foreach (string key in keys)
                                {
                                    List<NotificationsOverlayWindow> ss = LstScreen[key].ToList();
                                    for (int i = 0; i < ss.Count; i++)
                                    {
                                        if (ss[i].CtrlArea._items.Count > 0)
                                        {
                                            IsPrevScreenHaveReccord = true;
                                        }
                                        ss[i].Close();
                                    }

                                    LstScreen.Clear();
                                }
                            }
                        }

                        int sw = int.Parse((workArea.Width / dpi).ToString());
                        int sh = int.Parse((workArea.Height / dpi).ToString());
                        int sl = int.Parse((workArea.Left / dpi).ToString());
                        int st = int.Parse((workArea.Top / dpi).ToString());

                        List<NotificationsOverlayWindow> lstWind = new List<NotificationsOverlayWindow>();
                        for (int i = 0; i < maxcolcount; i++)
                        {
                            NotificationsOverlayWindow win0 = new NotificationsOverlayWindow($"{mainScreen.DeviceName}-{i}")
                            {
                                Left = sl + sw - ucwidth * (i + 1) - ucsplitewidth * i,
                                Top = workArea.Top / dpi,
                                Width = ucwidth,
                                Height = workArea.Height / dpi
                            };

                            //注册事件
                            (win0.CtrlArea as NotificationArea).NotificationSizeChanged += NotificationManager_NotificationSizeChanged;
                            
                            win0.Show();
                            lstWind.Add(win0);
                        }

                        LstScreen.Add(mainScreen.DeviceName, lstWind);
                    }

                    bool isShow = false;
                    NotificationContent nc = (NotificationContent)content;
                    string contentskey = string.Empty;
                    switch (nc.ContentType.ToUpper())
                    {
                        case "DQ":
                        case "DQB":
                            DRPNotificationViewModel dnvm = DRPTools.DrpContentToViewModel(nc);
                            contentskey = dnvm.Id;
                            break;
                        case AdminConsts.Admin_ER:
                        case AdminConsts.PriceMatch:
                            ExecutionReport er = Newtonsoft.Json.JsonConvert.DeserializeObject<ExecutionReport>(nc.Detail);
                            contentskey = $"DQLimit_{er.QuoteID}";
                            break;
                        case AdminConsts.Admin_IT:
                            PointPriceDetailModel ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(nc.Detail);
                            contentskey = $"PP{ppdm.order.orderId}";
                            break;
                        case "TABLESVR_ORDER_UNREF":
                            LastWorkDayUnRefViewModel vmunref = new LastWorkDayUnRefViewModel();
                            LastWorkDayUnRefMsgModel unrefmsgmodel = JsonConvert.DeserializeObject<LastWorkDayUnRefMsgModel>(nc.Message);
                            vmunref.MsgModel = unrefmsgmodel;
                            UnRefContentModel unrefcontentmodel = JsonConvert.DeserializeObject<UnRefContentModel>(unrefmsgmodel.content);
                            UnRefMsgTypeModel unrefmsgtypemodel = JsonConvert.DeserializeObject<UnRefMsgTypeModel>(unrefcontentmodel.remark);
                            contentskey = $"UnRef{unrefmsgtypemodel.type}";
                            break;
                        case AdminConsts.Admin_MM:
                            //MMBreakAlarmModel ppdm1 = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(nc.Detail);
                            contentskey = $"ANS{nc.Id}";
                            break;
                    }
                    

                    NotificationArea currarea = GetAndRefeshArea(contentskey);


                    // 如果窗口已经显示，则更新
                    if (currarea != null)
                    {
                        currarea.Show(content, (TimeSpan)expirationTime, onClick, onClose);
                        isShow = true;
                    }
                    else
                    {
                        foreach (NotificationArea area in Areas)
                        {
                            if (area.AreaName.Split('-')[0] == mainScreen.DeviceName)
                            {
                                if (area._items.Count >= 0 && area._items.Count < maxrowcount)
                                {
                                    area.dpi = dpi;
                                    area.MaxItems = maxrowcount;
                                    area.areaheight = int.Parse((workArea.Height / dpi).ToString());
                                    int oldcount = area._items.Count;
                                    area.Show(content, (TimeSpan)expirationTime, onClick, onClose);
                                    if (area._items.Count > oldcount)
                                    {
                                        (area.Parent as NotificationsOverlayWindow).Activate();
                                    }
                                    isShow = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!isShow)
                    {
                        //强提醒
                        View.Component.CustomMessageBox.Warning("强提醒窗口超出限制！请打开小玲铛查看待办事项！", "警告");
                    }
                }
                catch
                {
                }
            });
        }

        public void ShowPTListBrokerUI(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                {
                    #region Init...
                    if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(12 * 60 * 60);

                    System.Windows.Forms.Screen mainScreen = (content as NotificationContent).CurScreen;
                    Rectangle workArea = mainScreen.WorkingArea;
                    Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                    double dpi = (int)(graphics.DpiX * 1.041666667) / 100.0;
                    double ucwidth = 445;
                    double ucheight = 300;
                    double ucsplitewidth = 10;
                    double ucspliteheight = 4;

                    //每列显示多少提示
                    int maxrowcount = int.Parse(Math.Floor((workArea.Height / dpi) / (ucheight + ucspliteheight)).ToString());
                    //每屏幕显示多少列
                    int maxcolcount = int.Parse(Math.Floor((workArea.Width / dpi) / (ucwidth + ucsplitewidth)).ToString());


                    //如果移动了主窗口
                    if (!LstScreen.ContainsKey(mainScreen.DeviceName))
                    {
                        this.InitScreen(mainScreen);
                    }
                    #endregion

                    NotificationContent nc = (NotificationContent)content;
                    string contentskey = $"Broker{nc.Id}";
                    NotificationArea currarea = GetAndRefeshArea(contentskey);
                    string contentType = nc.ContentType;

                    nc.expirationTime = (TimeSpan)expirationTime;
                    nc.onClick = onClick;
                    nc.onClose = onClose;
                    nc.contentType = contentType;
                    nc.contentskey = contentskey;

                    // 如果窗口已经显示，则更新
                    if (currarea != null)
                    {
                        nc.CurrArea = currarea;

                        lock (lockqueue)
                        {
                            queue.Enqueue(nc);
                            _autoResetEvent.Set();
                        }

                        //currarea.ShowPTListBroker(nc, (TimeSpan)expirationTime, onClick, onClose, contentType, contentskey);
                    }
                    else
                    {
                        foreach (NotificationArea area in Areas)
                        {
                            if (area.AreaName.Split('-')[0] == mainScreen.DeviceName)
                            {
                                if (area._items.Count >= 0 && area._items.Count < maxrowcount)
                                {
                                    area.dpi = dpi;
                                    area.MaxItems = maxrowcount;
                                    area.areaheight = int.Parse((workArea.Height / dpi).ToString());
                                    nc.CurrArea = area;

                                    lock (lockqueue)
                                    {
                                        queue.Enqueue(nc);
                                        _autoResetEvent.Set();
                                    }

                                    //area.ShowPTListBroker(nc, (TimeSpan)expirationTime, onClick, onClose, contentType, contentskey);
                                    break;
                                }
                            }
                        }
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        
        }

        /// <summary>
        /// 获取当前屏幕的算法
        /// </summary>
        /// <returns></returns>
        public static System.Windows.Forms.Screen GetMainScreen()
        {
            var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
            return screen;
        }


        /// <summary>
        /// 初始化强提醒显示区域
        /// </summary>
        /// <param name="CurScreen"></param>
        public void InitScreen(System.Windows.Forms.Screen screen)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    double ucwidth = 445;
                    double ucheight = 300;
                    double ucsplitewidth = 10;
                    double ucspliteheight = 4;

                    var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                    Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                    PresentationSource source = PresentationSource.FromVisual(window);
                    Matrix m = source.CompositionTarget.TransformFromDevice;
                    double widthRatio = m.M11;
                    double heightRatio = m.M22;
                    Rectangle workArea = screen.WorkingArea;

                    //每列显示多少提示
                    int maxrowcount = int.Parse(Math.Floor(screen.WorkingArea.Height * heightRatio / (ucheight + ucspliteheight)).ToString());
                    //每屏幕显示多少列
                    int maxcolcount = int.Parse(Math.Floor(screen.WorkingArea.Width * widthRatio / (ucwidth + ucsplitewidth)).ToString());

                    // 原始窗口是否有弹出式消息
                    bool IsPrevScreenHaveReccord = false;

                    if (!LstScreen.ContainsKey(screen.DeviceName))
                    {
                        if (LstScreen.Count > 0)
                        {

                            List<string> keys = LstScreen.Keys.ToList();
                            foreach (string key in keys)
                            {
                                List<NotificationsOverlayWindow> ss = LstScreen[key].ToList();
                                for (int i = 0; i < ss.Count; i++)
                                {
                                    if (ss[i].CtrlArea._items.Count > 0)
                                    {
                                        IsPrevScreenHaveReccord = true;
                                        break;
                                    }
                                }
                            }
                            if (!IsPrevScreenHaveReccord)
                            {
                                foreach (string key in keys)
                                {
                                    List<NotificationsOverlayWindow> ss = LstScreen[key].ToList();
                                    for (int i = 0; i < ss.Count; i++)
                                    {
                                        if (ss[i].CtrlArea._items.Count > 0)
                                        {
                                            IsPrevScreenHaveReccord = true;
                                        }
                                        ss[i].Close();
                                    }

                                    LstScreen.Clear();
                                }
                            }
                        }

                        int sw = int.Parse((screen.WorkingArea.Width * widthRatio).ToString());
                        int sl = int.Parse((rect.Left == 0 ? 0 : screen.WorkingArea.Left * widthRatio).ToString());
                        double tmpt = screen.WorkingArea.Top * heightRatio;
                        List<NotificationsOverlayWindow> lstWind = new List<NotificationsOverlayWindow>();
                        for (int i = 0; i < maxcolcount; i++)
                        {
                            NotificationsOverlayWindow win0 = new NotificationsOverlayWindow($"{screen.DeviceName}-{i}")
                            {
                                Left = sl + sw - ucwidth * (i + 1) - ucsplitewidth * i,
                                Top = rect.Top == 0 ? 0 : screen.WorkingArea.Top * heightRatio,
                                Width = ucwidth,
                                Height = screen.WorkingArea.Height * widthRatio
                            };

                            //注册事件
                            (win0.CtrlArea as NotificationArea).NotificationSizeChanged += NotificationManager_NotificationSizeChanged;

                            win0.Show();
                            lstWind.Add(win0);
                        }

                        LstScreen.Add(screen.DeviceName, lstWind);
                    }
                });
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        internal static void AddArea(NotificationArea area)
        {
            Areas.Add(area);
        }

        /// <summary>
        /// 判断窗口是否已经显示
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private NotificationArea GetAndRefeshArea(string key = "")
        {
            foreach (NotificationArea area in Areas)
            {
                if (area._items.Count > 0)
                {
                    double nw = 80000;
                    double pw = 80000;
                    double minw = 0;

                    if (area._items.OfType<NotificationWindow>().Count() > 0)
                    {
                        NotificationWindow curnw = area._items.OfType<NotificationWindow>().Last();
                        if (curnw != null)
                        {
                            Point controlLocation = curnw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                nw = controlLocation.Y / area.dpi;
                        }
                    }

                    if (area._items.OfType<NofirmPointPriceView>().Count() > 0)
                    {
                        NofirmPointPriceView curpw = area._items.OfType<NofirmPointPriceView>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }

                    if (area._items.OfType<LastWorkDayUnRefView>().Count() > 0)
                    {
                        LastWorkDayUnRefView curpw = area._items.OfType<LastWorkDayUnRefView>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }

                    if (area._items.OfType<MMBreakAlarmReminder>().Count() > 0)
                    {
                        MMBreakAlarmReminder curpw = area._items.OfType<MMBreakAlarmReminder>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }
                    if (area._items.OfType<OrderMatchFailedUI>().Count() > 0)
                    {
                        OrderMatchFailedUI curpw = area._items.OfType<OrderMatchFailedUI>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }
                    if (area._items.OfType<IntentionUI>().Count() > 0)
                    {
                        IntentionUI curpw = area._items.OfType<IntentionUI>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }
                    if (area._items.OfType<UnderlineExecTpicUI>().Count() > 0)
                    {
                        UnderlineExecTpicUI curpw = area._items.OfType<UnderlineExecTpicUI>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }
                    if (area._items.OfType<IntentionBUI>().Count() > 0)
                    {
                        IntentionBUI curpw = area._items.OfType<IntentionBUI>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }
                    if (area._items.OfType<FeedBackAUI>().Count() > 0)
                    {
                        FeedBackAUI curpw = area._items.OfType<FeedBackAUI>().Last();
                        if (curpw != null)
                        {
                            Point controlLocation = curpw.PointToScreen(new Point(0, 0));
                            if (controlLocation != null)
                                pw = controlLocation.Y / area.dpi;
                        }
                    }

                    //剩余的高度
                    minw = Math.Min(nw, pw);
                    int he = 300;
                    if (int.TryParse(minw.ToString(), out he))
                    {
                        area.RemainingHeight = he;
                    }
                    else 
                    {
                        area.RemainingHeight = 0;
                    }
                }
                else
                {
                    area.RemainingHeight = 1024;
                }
            }

            // 如果key不空，则返回area
            if (!string.IsNullOrEmpty(key))
            {
                foreach (NotificationArea area in Areas)
                {
                    if ((area._items.OfType<NofirmPointPriceView>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<NotificationWindow>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<MMBreakAlarmReminder>().Count(i => i.MsgId == key) > 0) 
                        || (area._items.OfType<LastWorkDayUnRefView>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<OrderMatchFailedUI>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<IntentionUI>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<UnderlineExecTpicUI>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<IntentionBUI>().Count(i => i.MsgId == key) > 0)
                        || (area._items.OfType<FeedBackAUI>().Count(i => i.MsgId == key) > 0))
                    {
                        return area;
                    }
                }
            }

            return null;
        }

        

        /// <summary>
        /// 显示区域显示数量变化
        /// </summary>
        /// <param name="obj"></param>
        private void NotificationManager_NotificationSizeChanged(object obj)
        {
            // 更新Area的值
            GetAndRefeshArea();
            //    //// 重新排列窗口
            //    foreach (NotificationArea area in Areas)
            //    {
            //        area.NotificationSizeChanged -= NotificationManager_NotificationSizeChanged;
            //    }

            //    List<object> FullOrderList = new List<object>();
            //    List<NotificationArea> FullAreaList = Areas.ToList<NotificationArea>();
            //    for (int j = 0; j < FullAreaList.Count; j++)
            //    {
            //        NotificationArea area = FullAreaList[j];
            //        if (area._items.Count > 0)
            //        {
            //            object curpw = area._items.OfType<object>().Last();
            //            double pw = 0;
            //            if (curpw.GetType() == typeof(NotificationWindow))
            //            {
            //                Point controlLocation = ((NotificationWindow)curpw).PointToScreen(new Point(0, 0));
            //                pw = controlLocation.Y / area.dpi;
            //            }
            //            else if (curpw.GetType() == typeof(NofirmPointPriceView))
            //            {
            //                Point controlLocation = ((NofirmPointPriceView)curpw).PointToScreen(new Point(0, 0));
            //                pw = controlLocation.Y / area.dpi;
            //            }

            //            //如果剩余了足够的高度，则将下一个area的第一个item移到到当前area的最后
            //            if (pw > 238)
            //            {
            //                int nextAreaNumber = j + 1;
            //                NotificationArea nextarea = FullAreaList[nextAreaNumber];
            //                if (nextAreaNumber <= FullAreaList.Count)
            //                {
            //                    if (nextarea._items.Count > 0)
            //                    {
            //                        object nextpw = nextarea._items.OfType<object>().First();
            //                        if (nextpw.GetType() == typeof(NotificationWindow))
            //                            (nextpw as NotificationWindow).Close();
            //                        area._items.Add(nextpw);

            //                    }
            //                }
            //            }
            //        }

            //        //for (int i = 0; i < area._items.Count; i++)
            //        //{
            //        //   var item = area._items[i];
            //        //    // 对每个元素执行你的操作
            //        //    FullOrderList.Add(item);
            //        //}
            //    }

            //    foreach (NotificationArea area in Areas)
            //    {
            //        area.NotificationSizeChanged += NotificationManager_NotificationSizeChanged;
            //    }
            //}
        }

    }
}