using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View;
using BDB.Trade.Plugin.View.UserSetting;
using STC.Frame.Common;
using STC.Frame.Plugin;

namespace BDB.Trade.Plugin
{
    public class SettingPlugin : AbstractPlugin
    {
        public override string Author
        {
            get
            {
                return "饶嘉琪";
            }
        }

        public override string Description
        {
            get
            {
                return "设置插件";
            }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }

        private bool enabled = true;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
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

        public override int PluginKey
        {
            get
            {
                return (int)PluginKeyEnum.SettingPlugin;
            }
        }

        public override string PluginName
        {
            get
            {
                return "设置插件";
            }
        }

        private int _tag = 7;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        protected override void load()
        {
            STC.Common.Data.DesignHandler.Instance.ShowDesign += Instance_ShowDesign;
            base.load();
        }
        private void Instance_ShowDesign()
        {
            if (userSettingView == null)
            {
                userSettingView = new UserSettingView();
                userSettingView.Closed += UserSettingView_Closed;
                userSettingView.Show();
            }
            else
                userSettingView.Activate();

        }

        private UserSettingView userSettingView = null;

        private void UserSettingView_Closed(object sender, EventArgs e)
        {
            userSettingView.Closed -= UserSettingView_Closed;
            userSettingView = null;
        }

        public override void BeforeTerminating()
        {
        }

        public override void onNotify(string name, object o)
        {
        }
    }
}
