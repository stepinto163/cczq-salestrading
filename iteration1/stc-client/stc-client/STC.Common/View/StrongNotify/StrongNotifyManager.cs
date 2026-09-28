using com.gateway.log;
using STC.Common.Data;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace STC.Common.View.StrongNotify
{ 
    /// <summary>
    /// 强提醒窗口管理
    /// </summary>
    public class StrongNotifyManager
    {
        #region 构造函数
        public StrongNotifyMainWindow _window;
        private object lockobj = new object();

        /// <summary>
        /// 弹出的强提醒窗口
        /// </summary>
        public ConcurrentDictionary<string, object> DictPopupNotify = new ConcurrentDictionary<string, object>();

        /// 准备弹出的强提醒窗口
        /// </summary>
        public Dictionary<string, bool> ShowNotify = new Dictionary<string, bool>();

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        //GR弹窗队列
        private ConcurrentQueue<object> GRNotifyQueue = new ConcurrentQueue<object>();

        //GR弹窗用
        private ManualResetEventSlim ProcGRResetEvent = new ManualResetEventSlim(false);

        public StrongNotifyManager()
        {
            Init();
        }

        public void Init()
        {
            //订阅程序窗口移动事件
            DesignHandler.Instance.OnGlobalActivate += Instance_OnGlobalActivate;
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
            //订阅强提醒设置事件
            Common.Data.DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            //启动处理GR弹窗线程
            //Task.Factory.StartNew(() => DoProcGRNotifyQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        #endregion

        #region 程序重启或退出
        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "STC_RestartApp")
            {
                //当程序退出时，中断处理线程
                try
                {
                    //cancellationTokenSource.Cancel();
                }
                catch { }
            }
        }
        #endregion

        #region ShowGRNotify 显示GR提醒
        /// <summary>
        /// 兼容，如果需要独立处理，则在此处编写代码
        /// </summary>
        /// <param name="data"></param>
        public async void ShowGRNotify(object data, TimeSpan? expirationTime = null)
        {
            await Task.Factory.StartNew(() => 
            { 
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(async () =>
                {
                    lock (lockobj)
                    {
                        string key = StrongNotifyHandler.Instance.GetPropertyValueFromControl(data as UIElement, "MsgId");
                        ShowNotify[key] = true;
                    }

                    this.AddUserControl(data as UIElement);

                    if (expirationTime != null)
                    {
                        //if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(24 * 60 * 60);
                        var expected = TimeSpan.FromSeconds(expirationTime.Value.TotalSeconds);
                        await Task.Delay(expected);
                        StrongNotifyHandler.Instance.InvokeMethodFromControl(data as UIElement, "Close", new object[] { });
                    }
                }));
            });
        }

        #region GR Function
        /// <summary>
        /// 显示强提醒窗口
        /// </summary>
        /// <param name="data">待显示的UserControl</param>
        /// <param name="expirationTime">自动关闭时间</param>
        public void ToShowStrongNotify(object data, TimeSpan? expirationTime = null)
        {
            ShowGRNotify(data, expirationTime);
        }
        #endregion

        #endregion

        #region 初始化显示窗口处理  GetAndRefreshArea  InitStrongNotifyScreen  Instance_OnGlobalActivate
        /// <summary>
        /// 获取已显示窗体
        /// </summary>
        /// <param name="key">id</param>
        /// <param name="keytype">MsgId</param>
        /// <returns></returns>
        public object GetAndRefreshArea(string key, string keytype = "MsgId")
        {
            object curarea = null;
            if (_window != null)
            {
                // 如果key不空，则返回area
                if (!string.IsNullOrEmpty(key))
                {
                    //首先从弹出字典中查找是否弹出
                    lock (DictPopupNotify)
                    {
                        if (DictPopupNotify.ContainsKey(key))
                        {
                            DictPopupNotify.TryGetValue(key, out curarea);
                            return curarea;
                        }
                    }

                    int times = 0;
                    while (ShowNotify.ContainsKey(key))
                    {
                        curarea = GetObjectFromArea(key, keytype);
                        if (curarea == null)
                        {
                            Thread.Sleep(50);
                            times++;
                            if (times > 10)
                            {
                                break;
                            }
                        }
                        else
                        {
                            ShowNotify[key] = true;
                            break;
                        }
                    }

                    return curarea;
                }
            }
            else
            {
                InitStrongNotifyScreen();
            }

            return curarea;
        }

        /// <summary>
        /// 根据MsgId获取对象子函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keytype"></param>
        /// <returns></returns>
        private object GetObjectFromArea(string key, string keytype = "MsgId")
        {
            foreach (object area in _window.CustomPanel.Children)
            {
                // 通过反射检查 MsgId 属性
                var property = area.GetType().GetProperty(keytype);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(area) as string;
                    if (value == key)
                    {
                        return area;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 初始化显示区域
        /// </summary>
        public void InitStrongNotifyScreen()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                InitNotifyScreen(GetMainScreen());
                ////加载示例窗口
                //TestControl();
            }));
        }

        /// <summary>
        /// 主窗口位置改变
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnGlobalActivate(object obj)
        {
            if (!(bool)obj)
            {
                FollowWindowPositionAndWriteShowScreen();
            }
        }

        /// <summary>
        /// 设置改变
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSettingChange(object obj)
        {
            if (obj is Common.Model.PT.StrongNotifySettingModel)
            {
                FollowWindowPositionAndWriteShowScreen();
            }
        }

        /// <summary>
        /// 跟随窗口并且写入显示屏幕
        /// </summary>
        private void FollowWindowPositionAndWriteShowScreen()
        {
            //0 强提醒窗口跟随主窗口 1 固定强提醒窗口在主窗口屏幕  
            if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 0 || LocalEnv.ConfInfo.StrongNotifyFollowStyle == 1)
            {
                if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 0)
                {
                    FollowWindowPosition();
                    LocalEnv.ConfInfo.StrongNotifyShowScreen = View.StrongNotify.StrongNotifyHandler.Instance.ManagerInstance.GetMainScreen().DeviceName.Replace("\\", "").Replace(".", "");
                    IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "StrongNotifyShowScreen", LocalEnv.ConfInfo.StrongNotifyShowScreen);
                }
                else
                {
                    ReSetWindowsSize(LocalEnv.ConfInfo.StrongNotifyShowScreen);
                }
            }

            // 2 强提醒窗口跟随交易处理  3 固定强提醒窗口在交易处理所在屏幕
            if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 2 || LocalEnv.ConfInfo.StrongNotifyFollowStyle == 3)
            {
                if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 2)
                {
                    object transobj = WindowManager.Instance.GetExistWin($"Transaction-TransactionPanel");
                    if (transobj != null)
                    {
                        if (transobj.GetType().ToString().Split('.')[transobj.GetType().ToString().Split('.').Length - 1] == "BorderWin")
                        {
                            object res = View.StrongNotify.StrongNotifyHandler.Instance.InvokeMethodFromControl(transobj, "GetCurScreen", new object[] { });
                            if (res != null)
                            {
                                LocalEnv.ConfInfo.StrongNotifyShowScreen = res as string;
                                IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "StrongNotifyShowScreen", LocalEnv.ConfInfo.StrongNotifyShowScreen);
                                ReSetWindowsSize(res as string);
                            }
                            else
                            {
                                FollowWindowPosition();
                                LocalEnv.ConfInfo.StrongNotifyShowScreen = View.StrongNotify.StrongNotifyHandler.Instance.ManagerInstance.GetMainScreen().DeviceName.Replace("\\", "").Replace(".", "");
                                IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "StrongNotifyShowScreen", LocalEnv.ConfInfo.StrongNotifyShowScreen);
                            }
                        }
                    }
                    else
                    {
                        FollowWindowPosition();
                        LocalEnv.ConfInfo.StrongNotifyShowScreen = View.StrongNotify.StrongNotifyHandler.Instance.ManagerInstance.GetMainScreen().DeviceName.Replace("\\", "").Replace(".", "");
                        IniFileHelper.WriteValue($"GrLayoutSetting_{LocalEnv.UserInfo.UserName}", "StrongNotifyShowScreen", LocalEnv.ConfInfo.StrongNotifyShowScreen);
                    }
                }
                else
                {
                    ReSetWindowsSize(LocalEnv.ConfInfo.StrongNotifyShowScreen);
                }
            }
        }

        #endregion

        #region Function  InitNotifyScreen FollowWindowPosition  GetMainScreen AddUserControl OnNotificationClosed  NotificationNpUR_NotificationPopup
        /// <summary>
        /// 弹出恢复处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NotificationNpUR_NotificationPopup(object sender, RoutedEventArgs e)
        {
            try
            {
                string sendertypestring = e.OriginalSource.GetType().ToString().Split('.').LastOrDefault();

                lock (DictPopupNotify)
                {
                    string key = StrongNotifyHandler.Instance.GetPropertyValueFromControl(sender, "MsgId");

                    if (!DictPopupNotify.ContainsKey(key))
                    {
                        ClearAdornerLayer();
                        //弹出
                        AddControlToDict(key, sender);

                        _window.CustomPanel.Children.Remove(sender as UIElement);


                        StrongNoBorderWin popupWindow = new StrongNoBorderWin();
                        StrongNotifyHandler.Instance.InvokeMethodFromControl(sender, "SetHostWindow", new object[] { popupWindow });   //popupWindow.hostWindow = popupWindow;
                        StrongNotifyHandler.Instance.InvokeMethodFromControl(popupWindow, "SetControlToMove", new object[] { sender }); //popupWindow.ControlToMove = notification;

                        //如果是拖拽
                        if (sendertypestring == "Border")
                        {
                            popupWindow.WindowStartupLocation = WindowStartupLocation.Manual;

                            Win32.POINT curPoint = new Win32.POINT();
                            if (Win32.GetCursorPos(ref curPoint))
                            {
                            }

                            System.Windows.Point currentPoint = new System.Windows.Point(curPoint.X, curPoint.Y);

                            System.Windows.Forms.Screen newScreen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(curPoint.X, curPoint.Y));

                            foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                            {
                                // 获取当前屏幕的工作区域和 DPI
                                System.Drawing.Rectangle screenBounds = sc.WorkingArea;

                                // 判断逻辑坐标点是否在当前屏幕工作区域内
                                if (CommonUtils.IsPointInScreen(currentPoint, screenBounds))
                                {
                                    // 创建一个临时的 WPF 窗口（不需要显示）
                                    Window tempWindow = new Window
                                    {
                                        WindowStartupLocation = WindowStartupLocation.Manual,
                                        Left = sc.WorkingArea.Left, // 放置在目标屏幕上
                                        Top = sc.WorkingArea.Top,
                                        Width = 1, // 尽量小，避免影响用户体验
                                        Height = 1,
                                        ShowInTaskbar = false, // 不在任务栏显示
                                        WindowStyle = WindowStyle.None // 没有边框
                                    };

                                    tempWindow.Show(); // 必须 Show 才能获取 PresentationSource

                                    // 获取 PresentationSource
                                    PresentationSource source = PresentationSource.FromVisual(tempWindow);

                                    if (source != null)
                                    {
                                        // 获取 DPI 缩放矩阵
                                        System.Windows.Media.Matrix matrix = source.CompositionTarget.TransformToDevice;
                                        if (newScreen.DeviceName == sc.DeviceName)
                                        {
                                            //将屏幕坐标转换为窗口坐标
                                            popupWindow.Left = (currentPoint.X / matrix.M11) - 2;
                                            popupWindow.Top = (currentPoint.Y / matrix.M22) - 2;
                                            tempWindow.Close();
                                            break;  // 如果找到一个屏幕，就结束循环
                                        }
                                    }

                                    tempWindow.Close();
                                }
                            }
                        }

                        popupWindow.OnReturnRequested += () =>
                        {
                            StrongNotifyHandler.Instance.InvokeMethodFromControl(popupWindow, "SetControlToMove", new object[] { null }); //必须设置为null, popupWindow.ControlToMove = null; 当新窗口请求返回控件时，将其重新添加 

                            lock (lockobj)
                            {
                                RemoveControlFromDict(key);
                                this.AddUserControl(sender as UIElement);
                            }
                        };

                        popupWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                ClearAdornerLayer();
            }
        }

        /// <summary>
        /// 清除装饰
        /// </summary>
        private void ClearAdornerLayer()
        {
            AdornerLayer mAdornerLayer = AdornerLayer.GetAdornerLayer(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
            if (mAdornerLayer != null)
            {
                var adorners = mAdornerLayer.GetAdorners(StrongNotifyHandler.Instance.GetStrongNotifyMainWindow().GetCustomPanel());
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        mAdornerLayer.Remove(adorner);
                    }

                    mAdornerLayer = null;
                }
            }
        }

        /// <summary>
        /// 初始化强提醒显示区域
        /// </summary>
        /// <param name="CurScreen"></param>
        private void InitNotifyScreen(System.Windows.Forms.Screen CurScreen)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    System.Windows.Forms.Screen mainScreen = CurScreen;

                    var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                    Rect rect = new Rect(mainScreen.Bounds.X, mainScreen.Bounds.Y, mainScreen.Bounds.Width, mainScreen.Bounds.Height);
                    PresentationSource source = PresentationSource.FromVisual(window);
                    System.Windows.Media.Matrix m = source.CompositionTarget.TransformFromDevice;
                    double widthRatio = m.M11;
                    double heightRatio = m.M22;

                    if (_window == null)
                    {
                        _window = new StrongNotifyMainWindow()
                        {
                            Width = mainScreen.WorkingArea.Width * widthRatio,
                            Height = mainScreen.WorkingArea.Height * heightRatio,
                            Left = rect.Left == 0 ? 0 : mainScreen.WorkingArea.Left * widthRatio,
                            Top = rect.Top == 0 ? 0 : mainScreen.WorkingArea.Top * heightRatio,
                        };
                        
                        _window.Show();
                        _window.SetIsReady();
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 整个消息框跟随主窗体
        /// </summary>
        private void FollowWindowPosition()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                    var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                    Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                    PresentationSource source = PresentationSource.FromVisual(window);
                    System.Windows.Media.Matrix m = source.CompositionTarget.TransformFromDevice;
                    double widthRatio = m.M11;
                    double heightRatio = m.M22;

                    _window.Width = screen.WorkingArea.Width * widthRatio;
                    _window.Height = screen.WorkingArea.Height * heightRatio;
                    _window.Left = rect.Left == 0 ? 0 : screen.WorkingArea.Left * widthRatio;
                    _window.Top = rect.Top == 0 ? 0 : screen.WorkingArea.Top * heightRatio;
                }
                catch { }
            }));
        }

        /// <summary>
        /// 当屏幕缩放比例变化时重新设置容器
        /// </summary>
        public void ReSetWindowsSize(string scrname)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                    {
                        if (sc.DeviceName.Replace("\\", "").Replace(".", "") == scrname)
                        {
                            var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                            var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                            Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                            PresentationSource source = PresentationSource.FromVisual(window);
                            System.Windows.Media.Matrix m = source.CompositionTarget.TransformFromDevice;
                            double widthRatio = m.M11;
                            double heightRatio = m.M22;
                            //this.MaxWidth = screen.WorkingArea.Width;
                            //this.MaxHeight = screen.WorkingArea.Height;
                            _window.Width = sc.WorkingArea.Width * widthRatio;
                            _window.Height = sc.WorkingArea.Height * heightRatio;
                            _window.Left = sc.WorkingArea.Left * widthRatio;
                            _window.Top = sc.WorkingArea.Top * heightRatio;
                            break;
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
        public System.Windows.Forms.Screen GetMainScreen()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen;
            //0 强提醒窗口跟随主窗口 1 固定强提醒窗口在主窗口屏幕   2 强提醒窗口跟随交易处理  3 固定强提醒窗口在交易处理所在屏幕
            if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 0)
            {
                if (Application.Current?.MainWindow != null)
                {
                    //0 强提醒窗口跟随主窗口
                    var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                    var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                    screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                    return screen;
                }
                else
                    return screen;
            }
            else if (LocalEnv.ConfInfo.StrongNotifyFollowStyle == 2)
            {
                object transobj = WindowManager.Instance.GetExistWin($"Transaction-TransactionPanel");
                if (transobj != null)
                {
                    //如果已经打开 交易处理，则取交易处理窗口所在的屏幕
                    if (transobj.GetType().ToString().Split('.')[transobj.GetType().ToString().Split('.').Length - 1] == "BorderWin")
                    {
                        object res = View.StrongNotify.StrongNotifyHandler.Instance.InvokeMethodFromControl(transobj, "GetCurScreen", new object[] { });
                        if (res != null)
                        {
                            foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (sc.DeviceName.Replace("\\", "").Replace(".", "") == res as string)
                                {
                                    return sc;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //如果还没有加载 交易处理，则取根据配置中读取位置计算出所在屏幕
                    string positionX = IniFileHelper.ReadValue($"Transaction_{ LocalEnv.UserInfo.UserName}_TransactionPanel", "PositionX");
                    string positionY = IniFileHelper.ReadValue($"Transaction_{ LocalEnv.UserInfo.UserName}_TransactionPanel", "PositionY");
                    double dY = 0;
                    double d = 0;
                    if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
                    {
                        Point pointToCheck = new Point((int)d + 10, (int)dY + 10);
                        foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                        {
                            // 获取当前屏幕的工作区域和 DPI
                            System.Drawing.Rectangle screenBounds = sc.WorkingArea;
                            System.Windows.Media.Matrix screenDpiMatrix = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;

                            // 将坐标点转换为当前屏幕的逻辑坐标点
                            Point pointOnCurrentScreen = screenDpiMatrix.Transform(pointToCheck);

                            // 判断逻辑坐标点是否在当前屏幕工作区域内
                            if (CommonUtils.IsPointInScreen(pointOnCurrentScreen, screenBounds))
                            {
                                return sc;
                            }
                        }
                    }
                }
            }
            else
            {
                //1 已固定强提醒窗口显示的屏幕 3 固定强提醒窗口在交易处理所在屏幕
                foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                {
                    if (sc != null && !string.IsNullOrEmpty(LocalEnv.ConfInfo.StrongNotifyShowScreen))
                    {
                        if (sc.DeviceName.Replace("\\", "").Replace(".", "") == LocalEnv.ConfInfo.StrongNotifyShowScreen)
                        {
                            return sc;
                        }
                    }
                }
            }

            return screen;
        }

        /// <summary>
        /// 添加控件到Panel
        /// </summary>
        /// <param name="windowControl"></param>
        public void AddUserControl(UIElement windowControl)
        {
            _window.CustomPanel.Children.Add(windowControl);
        }

        /// <summary>
        /// 从容器中移除控件
        /// </summary>
        /// <param name="windowControl"></param>
        public void RemoveUserControl(UIElement windowControl)
        {
            _window.CustomPanel.Children.Remove(windowControl);
        }

        /// <summary>
        /// 添加控件到字典
        /// </summary>
        /// <param name="key"></param>
        /// <param name="windowControl"></param>
        public void AddControlToDict(string key, object windowControl)
        {
            //添加控件到字典表中
            lock (DictPopupNotify)
            {
                if (!DictPopupNotify.ContainsKey(key))
                {
                    DictPopupNotify.TryAdd(key, windowControl);
                }
            }
        }

        /// <summary>
        /// 从字典移除控件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveControlFromDict(string key)
        {
            bool iscontains = false;
            //首先从弹出字典中移除
            lock (DictPopupNotify)
            {
                object obj = null;
                if (DictPopupNotify.ContainsKey(key))
                {
                    iscontains = true;
                    DictPopupNotify.TryRemove(key, out obj);
                }
            }

            return iscontains;
        }

        /// <summary>
        /// 添加控件到字典
        /// </summary>
        /// <param name="key"></param>
        /// <param name="windowControl"></param>
        public void AddShowNotify(string key, object windowControl)
        {
            //添加控件到字典表中
            lock (ShowNotify)
            {
                if (!ShowNotify.ContainsKey(key))
                {
                    ShowNotify.Add(key, false);
                }
            }
        }

        /// <summary>
        /// 从所有的显示字典中移除key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void RemoveShowNotify(string key)
        {
            if (ShowNotify.ContainsKey(key))
            {
                ShowNotify.Remove(key);
            }
        }

        /// <summary>
        /// 是否已经准备显示或已经显示
        /// </summary>
        /// <param name="key"></param>
        /// <returns>1 已经显示 0 显示中 -1不包含</returns>
        public string IsShowNotifyContainsKey(string key)
        {
            if (ShowNotify.ContainsKey(key))
            {
                return ShowNotify[key] ? "1" : "0";
            }
            else
            {
                return "-1";
            }
        }

        /// <summary>
        /// 子窗体手工或自动关闭触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        public void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                string key = StrongNotifyHandler.Instance.GetPropertyValueFromControl(sender, "MsgId");
                //首先从弹出字典中移除
                lock (DictPopupNotify)
                {
                    RemoveShowNotify(key);
                    //如果是独立窗口，则移除字典后直接返回
                    if (RemoveControlFromDict(key))
                        return;
                }

                if (sender != null)
                {
                    RemoveUserControl(sender as UIElement);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Notification Closed ...{ex.Message}");
            }
        }
        #endregion

        #region 示例窗口
        /// <summary>
        /// 创建测试窗口
        /// </summary>
        public void TestControl()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                // 添加一些窗口控件
                for (int i = 0; i < 7; i++)
                {
                    object obj = GetAndRefreshArea((i + 1).ToString());
                    if (obj == null)
                        AddTestWindowControl($"模板标题 ", (i + 1).ToString());
                }
            }));
        }

        public void AddTestWindowControl(string title, string count)
        {
            string key = $"{title}-{count}";
            StrongNotifyHandler.Instance.ManagerInstance.AddShowNotify(key, false);
            TestMessageControl ui = new TestMessageControl();
            ui.MsgId = key;
            ui.WinTitle = ui.MsgId;
            //if (count % 3 == 0)
            //    windowControl.Width = 150;
            ui.NotificationClosed += StrongNotifyHandler.Instance.ManagerInstance.OnNotificationClosed;
            ui.NotificationPopup += StrongNotifyHandler.Instance.ManagerInstance.NotificationNpUR_NotificationPopup;

            StrongNotifyHandler.Instance.ManagerInstance.ToShowStrongNotify(ui);
        }
        #endregion
    }
}
