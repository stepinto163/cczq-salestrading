using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View;
using BDB.Trade.Plugin.View.Chat;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;

namespace BDB.Trade.Plugin
{
    public class ChatPlugin : AbstractPlugin
    {
        private int _tag = 80;
        public override string Author
        {
            get
            {
                return "CYC";
            }
        }

        public override string Description
        {
            get
            {
                return "聊天窗口";
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
            get { return (int)PluginKeyEnum.ChatPlugin; }
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
                return "聊天窗口插件";
            }
        }

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        private ChatView chatView = null;

        protected override void load()
        {
            if (chatView == null)
            {
                chatView = new ChatView();
                chatView._chatViewModel.Init(chatView);
                //chatView.Closed += UserSettingView_Closed;
                chatView.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                // chatView.Activate();
            }
        }

        private void UserSettingView_Closed(object sender, EventArgs e)
        {
            // chatView.Closed -= chatView_Closed;
            // chatView = null;
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
