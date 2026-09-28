using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Model.Chrome;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.CustomUI;
using STC.Frame.Plugin;
using STC.Position.Plugin.View;
using STC.Position.Plugin.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Position.Plugin
{
    public class PositionPlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 0;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get {return (int)PluginKeyEnum.PositionPlugin; }
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

        public override string PluginName
        {
            get { return "持仓管理插件"; }
        }

        public override string Description
        {
            get { return "持仓管理"; }
        }

        public override string Author
        {
            get { return "陈彦畅"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4012";

        private bool _isFirst = true;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_chicangguanli.png", Description, PluginKey, "12");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_chicangguanli.png", Description, PluginKey, "12");
                }

                _isFirst = false;
                enabled = false;

                return;
            }


            MainView view = new MainView();
            view.Load();

            TabPageHelper.OpenTabPage(this.Description, view, PluginKey);
            Application.Subscribe("STC_Clear", this);

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }

            base.load();
        }

        /// <summary>
        /// 释放消息处理
        /// </summary>
        /// <param name="tabItemHashCode"></param>
        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is MainView)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            LogHelper.Info("MainView:" + tabItem.Header + " UserControl_Unloaded ");
                            MainView mainView = tabItem.Content as MainView;
                            mainView.TryDispose();
                            GC.Collect();
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// 接受的消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals("OpenPosition"))
                {
                    //OpenChrome((ChromeWinModel)o);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    // 接收到清空复位消息
                    if (name.Equals(Consts.Topic_Clear))
                    {

                    }
                }
            }
        }

        public object GetObject(string name)
        {
            if (name == "HistoryPositionView")
            { 
                //注册历史持仓查询
                string objname = $"{(int)PluginKeyEnum.PositionPlugin}_{name}";
                if (Common.Utils.AppUtils.Application.GetObject(objname) == null)
                {
                    STC.Position.Plugin.View.HistoryPositionView historyPositionView = new HistoryPositionView();
                    registerObject("HistoryPositionView", historyPositionView);
                }
            }

            return Common.Utils.AppUtils.Application.GetObject($"{(int)PluginKeyEnum.PositionPlugin}_{name}");
        }

        protected override void unload()
        {
            base.unload();
        }


        public override void BeforeTerminating()
        {
            base.BeforeTerminating();
        }
        #endregion
    }
}
