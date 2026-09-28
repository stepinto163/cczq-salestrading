using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Model.Drp;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Mm.Plugin.Utils.Data;
using STC.Mm.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Mm.Plugin
{
    public class MmPlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 2;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.MmPlugin; }
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

        private bool isFullScreen = false;
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

        public override string PluginName
        {
            get { return "双边做市插件"; }
        }

        public override string Description
        {
            get { return "双边做市"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4005";

        private bool _isFirst = true;

        private bool _isSubSetting = false;
        private MainView oldView;
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            if (!_isSubSetting)
            {
                InitSetting();
                _isSubSetting = true;
            }
            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_shuangbianzuoshi.png", Description, PluginKey, "6");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_shuangbianzuoshi.png", Description, PluginKey, "6");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                return;
            }

            MyOrderProcesser.Processer.Init();
            EmsOrderProcesser.Processer.Init();
            LastExecutionProcesser.Processer.Init();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    OrdersHandler.Instance.init();
                    EmsOrdersHandler.Instance.init();
                    ExecutionsHandler.Instance.init();
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequXBondCommonPlugin load", e);
                }
            });


            MMGroupHandler.Instance.init();
            AnsDataHandler.Instance.init();

            ViewModel.MainViewModel vm = new ViewModel.MainViewModel();
            oldView = new MainView() { DataContext = vm };
            vm.parentView = oldView;
            TabPageHelper.OpenTabPage(this.Description, oldView, PluginKey);

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
            base.load();
        }

        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            if (obj.Code == "MMAgencyMsg") 
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    BreakAlarmModel msgPo = JsonConvert.DeserializeObject<BreakAlarmModel>(obj.Msg);

                    //if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                    //{
                    //    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    //}
                    //System.Windows.Application.Current.MainWindow.Activate();
                    if (!TabPageHelper.IsExistTabItem(this.Description))
                    {
                        ViewModel.MainViewModel vm = new ViewModel.MainViewModel();
                        oldView = new MainView() { DataContext = vm };
                        vm.parentView = oldView;
                        TabPageHelper.OpenTabPage(this.Description, oldView, PluginKey);
                    }
                    oldView.BreakAlarmGroupID = msgPo.groupId;
                    oldView.OpenTab(msgPo.groupId, msgPo.securityId);
                });
            }
        }

        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            SaveOrderControlPosition();
            CloseAllOpenedOrderControl();
            
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is MainView)
                    {
                        LogHelper.Debug("MmList:" + tabItem.Header + " UserControl_Unloaded ");
                        MainView mainView = tabItem.Content as MainView;
                        mainView.Dispose();
                    }
                }
            }
        }

       
        protected override void unload()
        {
            base.unload();
            _isSubSetting = false;
        }
        #endregion

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
                    MMSettingView view = new MMSettingView();
                    userTabItem.Content = view;
                    userTabItem.Header = "双边做市";
                    settingView.AddTabPage(userTabItem);
                }
            }
        }

        public override void BeforeTerminating()
        {
            if (notifyCloseTabItem != null)
            {
                notifyCloseTabItem.UnSubscribe(NotifyCloseTabItem);
            }
        }

        private void SaveOrderControlPosition()
        {
            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("STC.Mm.Plugin"))
                {
                    w.Close();
                }
            }

            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("OrderControl") && (!key.Contains("OrderControlShe")))
                {
                    w.Close();
                }
            }
        }

        private void CloseAllOpenedOrderControl()
        {
            ////关闭所有打开的小窗
            //foreach (var item2 in WindowManager.Instance._UniqueKeyMapWindowDic)
            //{
            //    string key = item2.Key;
            //    Window w = item2.Value;

            //    if (key.IndexOf("OrderControl") >= 0 && WindowManager.Instance.IsExist(key))
            //    {
            //        WindowManager.Instance.CloseWindow(key);
            //    }
            //}

        }

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(Consts.Topic_Clear))
                {

                }

                if (name.Equals(Consts.Topic_RestartApp))
                {
                    SaveOrderControlPosition();
                }
            }
        }

        #endregion
    }
}
