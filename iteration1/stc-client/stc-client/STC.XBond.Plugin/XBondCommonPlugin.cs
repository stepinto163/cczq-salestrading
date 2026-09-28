using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 公共部分插件类（公共部分包含我的订单，我的成交，我的撤单，市场成交）
    /// </summary>
    public class XBondCommonPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 3;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.XBondCommonPlugin; }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


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
            get { return "公共部分插件"; }
        }

        public override string Description
        {
            get { return "公共部分"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload
        protected override void load()
        {
            if (CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions != "1")
                return;

            //await delayLoad();
            bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
            bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
            if (isAllow || isNotAllow)
            {
                this.Application.CommonPanel.Children.Add(new CommonView());
                this.Application.CommonPanel.Visibility = System.Windows.Visibility.Visible;
            }

            Task.Run(() => {
                // 整理数据
                CrossOrdersProcesser.Processer.Init();
                MyOrderProcesser.Processer.Init();
                MyExecutionProcesser.Processer.Init();
                QuantMyExecutionProcesser.Processer.Init();
                EmsOrderProcesser.Processer.Init();
                MarketExecutionProcesser.Processer.Init();
                LoadConfig.LoadBond = true;
            });


            Application.Subscribe("STC_Clear", this);

            base.load();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //接收数据
                    OrdersHandler.Instance.init();
                    ExecutionsHandler.Instance.init();
                    EmsOrdersHandler.Instance.init();
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequXBondCommonPlugin load", e);
                }
            });
        }

        private Task delayLoad()
        {
           return Task.Run(() => {
                while (this.Application.PluginManager.GetPlugin((int)PluginKeyEnum.HomePagePlugin) != null
                    && View.HomePage.HomePageView.IsInited == false)
                {
                    System.Threading.Thread.Sleep(500);
                }
            });
        }

        public override void BeforeTerminating()
        {
            //this.Application.Panel.Items.Remove(item3);
            //this.Application.Panel.Items.Remove(item2);
            //this.Application.Panel.Items.Remove(item);
        }
        #endregion

        #region onNotify
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

        #endregion
    }
}
