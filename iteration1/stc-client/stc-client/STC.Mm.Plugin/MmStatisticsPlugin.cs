using STC.Common.Utils;
using STC.Frame.Plugin;
using STC.Mm.Plugin.View;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Frame.Common;
using STC.Common.Handler.Admin;
using com.gateway.log;
using STC.Common.Utils.Events;

namespace STC.Mm.Plugin
{
    public class MmStatisticsPlugin : AbstractPlugin
    {
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 14;
        public override string Author
        {
            get
            {
                return "吕斌";
            }
        }

        public override string Description
        {
            get
            {
                return "做市统计";
            }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.MmStatisticsPlugin; }
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
            get
            {
                return "做市统计插件";
            }
        }

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4005";

        private bool _isFirst = true;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            //第一次菜单加载
            if (_isFirst)
            {

                //AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.XBond.Plugin;component/Images/stc_image_menulist_lishichaxun.png", Description, PluginKey, _Code);

                _isFirst = false;

                enabled = false;

                return;
            }

            LoadConfig.LoadBond = true;
            MmStatisticsView view = new MmStatisticsView();
            TabPageHelper.OpenTabPage(this.Description, view, PluginKey);

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }

            Application.Subscribe("STC_Clear", this);
            base.load();
        }

        public override void BeforeTerminating()
        {
            base.BeforeTerminating();
        }
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(Consts.Topic_Clear))
                {

                }
            }
        }

        protected override void unload()
        {
            base.unload();
        }

        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is MmStatisticsView)
                    {
                        LogHelper.Debug("MmStatisticsView:" + tabItem.Header + " UserControl_Unloaded ");
                        MmStatisticsView view = tabItem.Content as MmStatisticsView;
                        view.Dispose();
                    }
                }
            }
        }
    }
}
