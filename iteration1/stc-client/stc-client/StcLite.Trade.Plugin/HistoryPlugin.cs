using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;

namespace BDB.Trade.Plugin
{
    public class HistoryPlugin : AbstractPlugin
    {
        private int _tag = 8;
        public override string Author
        {
            get
            {
                return "张方方";
            }
        }

        public override string Description
        {
            get
            {
                return "历史需求";
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
            get { return (int)PluginKeyEnum.HistoryPlugin; }
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
                return "历史需求插件";
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
            HistoryOrderView view = new HistoryOrderView();
            view.load();
            TabPageHelper.OpenTabPage(this.Description, view, PluginKey);
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
    }
}
