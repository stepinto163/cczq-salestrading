using com.gateway.log;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.Price;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.DataProcesser;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Simular;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using STC.Common.Data;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 原来首页BondList插件类（现已废弃）
    /// </summary>
    public class XBondPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 2;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.XBondPlugin; }
        }

        protected new bool enabled = false;
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
            get { return "xbond交易系统插件"; }
        }

        public override string Description
        {
            get { return "X-Bond做市"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload

        TabItem item = new TabItem();
        protected override void load()
        {
            return;
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

            
            
            LoadConfig.LoadBond = true;

            //PricingPanel pricingPanel = new PricingPanel();
            //OrderSearchUI orderSearchUI = new OrderSearchUI();
            //SetPanel setPanel = new SetPanel();
            //item.Content = pricingPanel;
            //item.Header = pricingPanel.Title;
            //this.Application.Panel.Items.Add(item);
            //item2.Content = orderSearchUI;
            //item2.Header = orderSearchUI.Title;
            //this.Application.Panel.Items.Add(item2);
            //item3.Content = setPanel;
            //item3.Header = "设置";
            //this.Application.Panel.Items.Add(item3);

            // TabPageHelper.OpenTabPage(this.Description, new XBondView(), PluginKey);
            TabPageHelper.OpenTabPage(this.Description, new BondList(), PluginKey);

            Application.Subscribe("STC_Clear", this);

            //MarketExecHandler.Instance.Init();
            //OwnExecHandler.Instance.Init();
            //MDHandler.Instance.Init();
            //MDHandler.Instance.SubscribeMDPriceBySymbol("151883.SH");
            //TBookHandler.Instance.Init();
            base.load();
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
