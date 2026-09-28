using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.BrokerKanban;
using com.gateway.log;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Frame.Common;
using STC.Frame.Plugin;

namespace BDB.Trade.Plugin
{
    public class BrokerKanbanPlugin : AbstractPlugin
    {
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 8;
        public override string Author
        {
            get
            {
                return "陈彦畅";
            }
        }

        public override string Description
        {
            get
            {
                return "经纪商看板";
            }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = false;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.BrokerKanbanPlugin; }
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
                return "历史达成插件";
            }
        }

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        protected override void load()
        {
            LoadConfig.LoadBond = true;
            BrokerKanbanView view = new BrokerKanbanView();
            view.load();
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
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is BrokerKanbanView)
                    {
                        LogHelper.Debug("BrokerKanbanView:" + tabItem.Header + " UserControl_Unloaded ");
                        BrokerKanbanView mainView = tabItem.Content as BrokerKanbanView;
                        mainView.Dispose();
                        mainView.DataContext = null;
                        GC.Collect();
                    }
                }
            }
        }

        public override void BeforeTerminating()
        {
            base.BeforeTerminating();
        }
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
        }
    }
}
