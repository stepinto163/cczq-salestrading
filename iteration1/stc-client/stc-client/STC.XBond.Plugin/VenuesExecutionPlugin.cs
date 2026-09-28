using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 渠道执行插件类
    /// </summary>
    public class VenuesExecutionPlugin : AbstractPlugin
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
            get { return (int)PluginKeyEnum.VenuesExecutionPlugin; }
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
            get { return "渠道执行插件"; }
        }

        public override string Description
        {
            get { return "渠道执行"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload
        
        protected override void load()
        {
            //OrdersHandler.Instance.init();
            //EmsOrdersHandler.Instance.init();
            //ExecutionsHandler.Instance.init();

            //MyOrderProcesser.Processer.Init();
            //EmsOrderProcesser.Processer.Init();
            //MarketExecutionProcesser.Processer.Init();

            //CommonUtils.Application = this.Application;
            //LoadConfig.LoadBond = true;
            
            TabPageHelper.OpenTabPage(this.Description, new VenuesExecution(), PluginKey);

            Application.Subscribe("STC_Clear", this);

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
