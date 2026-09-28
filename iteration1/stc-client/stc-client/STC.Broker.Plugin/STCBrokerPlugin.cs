using STC.Broker.Plugin.View.HomePage;
using STC.Broker.Plugin.View.Price;
using STC.Broker.Plugin.View.UI;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Data;
using STC.Broker.Plugin.View.Setting;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using STC.Common.Handler.PT;
using STC.Common.Handler.Base;
using STC.Broker.Plugin.View.Transaction;
using STC.Broker.Plugin.View.Transaction.BridgeDesign;
using STC.Broker.Plugin.View.History;
using com.gateway.log;
using STC.Broker.Plugin.Command;
using STC.Common.Model.Fix.Message;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Broker.Plugin.ViewModel.Price;
using STC.Frame.Utils.MultiUI;
using STC.Broker.Plugin.View.Component;

namespace STC.Broker.Plugin
{
    public class STCBrokerPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 15;


        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.GRBroker; }
        }

        public override string PluginName
        {
            get { return "行情看板插件"; }
        }

        public override string Description
        {
            get { return "行情看板"; }
        }

        public override string Author
        {
            get { return "chenhu"; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = true;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        #endregion

        #region load,unload
        private bool _isFirst = true;
        private bool _isSubSetting = false;
        //待定 
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4021";

        ////TabItem item = new TabItem();
        ////TabItem item2 = new TabItem();
        private BridgeWinNew bridgeWin = null;
        private bool IsShowBridge = false;
        private DeleteOrderWin _delorderWin = null;
        private CopyDetailUI copyUI = null;
        /// <summary>
        /// 明细是否需要更新
        /// </summary>
        private volatile bool isNeedUpdateCopyUI = false;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_isSubSetting)
                {
                    InitSetting();
                    _isSubSetting = true;
                }
            });

            //第一次菜单加载
            if (_isFirst)
            {
                BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
                //订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
                ////明细更新
                //Utils.TransOrderHandler.Instance.OnUpdateOrder += Instance_OnUpdateOrder;
                //明细过桥清零
                Utils.TransOrderHandler.Instance.OnClearOrder += Instance_OnClearOrder;

                Handler.PricingHandler.Instance.Init();

                _isFirst = false;

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_rfq_active.png", Description, PluginKey, "15");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_rfq_active.png", Description, PluginKey, "15");
                }

                //注册全局快捷键
                RegistGlobalHotKey();   

                Application.Subscribe("IDC_Clear", this);

                //LayoutSetttingCache.Instance.InitSetting();
                HomePageDataHandler.ReadHomePageDisplayMode();

                LayoutSettingPo layoutSettingPo = CommonUtils.DeserializeObject<LayoutSettingPo>(CommonUtils.SerializeObject(LocalEnv.GrLayoutSetting));
                //如果没有配置Broker布局设置
                if (LocalEnv.GrLayoutSetting == null && layoutSettingPo != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                    {
                        System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                        System.Windows.Application.Current.MainWindow.Width = layoutSettingPo.Col * 284 + (layoutSettingPo.Col + 2) * 4 + 9;
                        System.Windows.Application.Current.MainWindow.Height = layoutSettingPo.Row * 398 + 97;

                        IniFileHelper.WriteValue($"MainWindow_{ LocalEnv.UserInfo.UserName}", "GrMinHeight", System.Windows.Application.Current.MainWindow.MinHeight.ToString());
                        IniFileHelper.WriteValue($"MainWindow_{ LocalEnv.UserInfo.UserName}", "GrMinWidth", System.Windows.Application.Current.MainWindow.MinWidth.ToString());
                    }));
                }

                enabled = false;

                //初始化强提醒显示区
                Common.Handler.Drp.DrpHandler.Instance.InitScreen();
                return;
            }

            TabPageHelper.OpenTabPage(this.Description, new HomePageView(), PluginKey);

            base.load();
        }

        private void Instance_OnClearOrder()
        {
            if (copyUI != null)
                copyUI.Clear();
            if (bridgeWin != null)
                bridgeWin.OnClear();
        }

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        private void RegistGlobalHotKey()
        {
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    //添加快速打开快捷键
                    break;
                default:
                    //添加行情看板左右翻页快捷键
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "行情看板", FuncName = "上一页", KeyName = "PageUp", IsGlobal = false, Name = EHotKeySetting.GrBrokerPageUp.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.PageUp });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "行情看板", FuncName = "上一页", KeyName = "Left", IsGlobal = false, Name = EHotKeySetting.GrBrokerLeft.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.Left });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "行情看板", FuncName = "下一页", KeyName = "PageDown", IsGlobal = false, Name = EHotKeySetting.GrBrokerPageDown.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.PageDown });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "行情看板", FuncName = "下一页", KeyName = "Right", IsGlobal = false, Name = EHotKeySetting.GrBrokerRight.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.Right });
                    break;
            }
        }

        public override void BeforeTerminating()
        {
            TransactionPopupHandler.Instance.StopRefreshER = true;
            Utils.TransOrderHandler.Instance.StopRefreshER = true;
            STC.Broker.Plugin.Handler.PricingHandler.Instance.StopRefreshER = true;
            Common.Handler.Drp.DrpHandler.Instance._notificationManager.StopRefreshER = true;
            //退出时保存布局
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                {
                    if (tabItem.Content is HomePageView)
                    {
                        HomePageView hpg = tabItem.Content as HomePageView;
                        foreach (TabItem ti in hpg.GroupTabControl.Items)
                        {
                            ((ti.Content as PricingPanel).DataContext as PricingPanelViewModel).SaveLastQuoteConfig();
                            ((ti.Content as PricingPanel).DataContext as PricingPanelViewModel).SaveGrUserFavoritesSettings();
                        }
                    }
                }
            });
        }



        private void InitSetting()
        {
            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
        }

        private void Instance_ShowDesign(object obj)
        {
            if (obj != null)
            {
                SettingView settingView = obj as SettingView;
                if (settingView != null)
                {
                    TabItem userTabItem = new TabItem();
                    PageSetQuotation view = new PageSetQuotation();
                    userTabItem.Content = view;
                    userTabItem.Header = "布局设置";
                    settingView.AddTabPage(userTabItem);

                    TabItem pTabItem = new TabItem();
                    PersonalSettingView pSettingView = new PersonalSettingView();
                    pTabItem.Content = pSettingView;
                    pTabItem.Header = "操作设置";
                    settingView.AddTabPage(pTabItem);

                    bool instFlag = AdminHandler.Instance.CheckPermisionById("5000", "5009");
                    bool permisionFlag = AdminHandler.Instance.CheckPermisionById("5000", "5010");

                    if (instFlag || permisionFlag)
                    {
                        TabItem manageTabItem = new TabItem();
                        manageTabItem.Header = "管理设置";
                        manageTabItem.Content = null;
                        manageTabItem.IsEnabled = false;
                        manageTabItem.IsSelected = false;
                        settingView.AddTabPage(manageTabItem);

                        if (instFlag)
                        {
                            TabItem instTabItem = new TabItem();
                            ManageSettingView mSettingView = new ManageSettingView();
                            instTabItem.Content = mSettingView;
                            instTabItem.Header = "机构设置";
                            settingView.AddTabPage(instTabItem);
                        }

                        if (permisionFlag)
                        {
                            TabItem permissionTabItem = new TabItem();
                            PermissionSettingView permissionSettingView = new PermissionSettingView();
                            permissionTabItem.Content = permissionSettingView;
                            permissionTabItem.Header = "权限设置";
                            settingView.AddTabPage(permissionTabItem);
                        }
                    }
                }
            }
        }
        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private async void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "PopupBorkerQuotesMiniWindows" && !string.IsNullOrEmpty(obj.Info1))
            {
                //更新组
                try
                {
                    SubThreadOpenQuotesMiniWindows(obj.Info1.ToString(), true);
                }
                catch { }
            }
        }

        /// <summary>
        /// 多STA程序测试
        /// </summary>
        /// <param name="securityID"></param>
        private void OpenQuotesMiniWindowsThread(string securityID)
        {
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread DetailThread = new System.Threading.Thread(() =>
                {
                    System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

                    // WindowManager.Instance.OpenWindow($"Transaction-DetailCommand", copyUIBoardWin);
                    BorderWin myTradingPopupWin = new BorderWin();
                    // 在 UI 线程上执行某些操作
                    myTradingPopupWin.Dispatcher.Invoke(() =>
                    {
                        //myTradingPopupWin.SetApp(myTradingPopupWin.Dispatcher);
                        myTradingPopupWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F0E"));
                        myTradingPopupWin.Title = securityID;
                        //CardPriceView tradeBookView = new CardPriceView(myTradingPopupWin);
                        //myTradingPopupWin.SetMainControl(tradeBookView);
                        myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                        myTradingPopupWin.WindowStyle = WindowStyle.None;
                        myTradingPopupWin.WindowState = WindowState.Normal;
                        myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        myTradingPopupWin.Owner = null;
                        myTradingPopupWin.Width = 286;
                        myTradingPopupWin.MaxWidth = 286;
                        myTradingPopupWin.MinWidth = 286;
                        myTradingPopupWin.Height = 500;
                        myTradingPopupWin.MinHeight = 390;

                        //tradeBookView.Init(securityID, null, null);
                    });

                    myTradingPopupWin.ShowDialog();

                });

                // 设置新线程的单独 WPF 后台
                DetailThread.SetApartmentState(System.Threading.ApartmentState.STA);
                DetailThread.Start();
            });
        }

        private void _delorderWin_Closed(object sender, EventArgs e)
        {
            _delorderWin = null;
        }
        /// <summary>
        /// 用隐藏和显示功能 提升界面相应速度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void BridgeWin_Closed(object sender, EventArgs e)
        //{
        //    IsShowBridge = false;
        //}

        //private void CopyUI_Closed(object sender, EventArgs e)
        //{
        //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        try
        //        {
        //            if (copyUI != null)
        //            {
        //                //copyUI.Closed -= CopyUI_Closed;
        //                //copyUI = null;
        //                //copyUI.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.Error(ex);
        //        }
        //    });
        //}

        private void SubThreadOpenQuotesMiniWindows(string securityID, bool isAlwaysTopmost = false)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                string winkey = $"BrokerCardPriceViewWindows-{securityID}";
                if (WindowManager.Instance.IsExist(winkey) == false)
                {
                    NoBorderWin myTradingPopupWin = new NoBorderWin(securityID, isAlwaysTopmost);
                    myTradingPopupWin.BridgeWinName = "BrokerCardPrice";
                    myTradingPopupWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0F0E"));
                    myTradingPopupWin.Title = securityID;
                    CardPriceView tradeBookView = new CardPriceView(myTradingPopupWin);
                    myTradingPopupWin.SetMainControl(tradeBookView);
                    myTradingPopupWin.ResizeMode = ResizeMode.CanResize;
                    myTradingPopupWin.WindowStyle = WindowStyle.None;
                    myTradingPopupWin.WindowState = WindowState.Normal;
                    myTradingPopupWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    myTradingPopupWin.Owner = null;
                    myTradingPopupWin.Width = 286;
                    myTradingPopupWin.MaxWidth = 286;
                    myTradingPopupWin.MinWidth = 286;
                    myTradingPopupWin.Height = 500;
                    myTradingPopupWin.MinHeight = 390;

                    if (isAlwaysTopmost)
                        myTradingPopupWin.Topmost = true;

                    tradeBookView.Init(securityID, null, myTradingPopupWin);


                    WindowManager.Instance.OpenWindow(winkey, myTradingPopupWin);
                }
            }));
        }
        #endregion

        #region 系统债券数据加载后和系统准备好
        private bool isInit = false;

        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                if (!isInit)
                {
                    isInit = true;
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        TransactionPopupHandler.Instance.loadData();
                    });
                }
            }
        }

        /// <summary>
        /// 系统准备好后加载行情小窗
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSystemReady(object obj)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //加载行情小窗
                    string lstkeys = IniFileHelper.ReadValue($"BrokerCardPrice-{LocalEnv.UserInfo.UserName}", "Keys");
                    string[] keyList = lstkeys.Split(',');
                    for (int i = 0; i < keyList.Length; i++)
                    {
                        string securityID = keyList[i];
                        if (!string.IsNullOrEmpty(securityID))
                        {
                            SubThreadOpenQuotesMiniWindows(securityID);
                        }
                    }

                    //加载明细
                    string cpkeys = IniFileHelper.ReadValue($"GrWin-{LocalEnv.UserInfo.UserName}", "Keys");
                    string[] keyListw = cpkeys.Split(',');
                    for (int i = 0; i < keyListw.Length; i++)
                    {
                        string winid = keyListw[i];
                        if (!string.IsNullOrEmpty(winid))
                        {
                            //发布消息
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "TriggerGrCommand", Info1 = winid });
                        }
                    }
                }));
            });
        }
        #endregion


        /// <summary>
        /// 明细是否需要更新
        /// </summary>
        private void Instance_OnUpdateOrder(Common.Model.Fix.Message.ExecutionReport obj)
        {
            // 更新明细
            //Task.Factory.StartNew(() =>
            //{
            //    System.Windows.Application.Current.Dispatcher.Invoke((Action)(async () =>
            //    {
            //        if (copyUI != null)
            //        {
            //            if (copyUI.IsVisible)
            //            {
            //                copyUI.Addline(obj);
            //            }
            //            else
            //            {
            //                //copyUI.refresh();
            //                isNeedUpdateCopyUI = true;
            //            }
            //        }
            //    }));
            //});
        }


        /// <summary>
        /// 大小变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void InnerBoardWin_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            BorderWin innerBoardWin = sender as BorderWin;
            if (innerBoardWin != null)
            {
                TransactionPanel transactionPanel = innerBoardWin.GetMainControl() as TransactionPanel;
                if (transactionPanel != null)
                {

                    transactionPanel.Width = innerBoardWin.ActualWidth - 10;
                }
            }
        }
    }
}