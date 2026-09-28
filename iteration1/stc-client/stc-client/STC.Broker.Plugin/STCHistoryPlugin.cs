using STC.Broker.Plugin.View;
using STC.Broker.Plugin.View.History;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STC.Broker.Plugin
{
    public class STCHistoryPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 16;


        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.GRHistory; }
        }

        public override string PluginName
        {
            get { return "历史记录"; }
        }

        public override string Description
        {
            get { return "历史记录"; }
        }

        public override string Author
        {
            get { return "chenhu"; }
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

        #endregion

        #region load,unload
        private bool _isFirst = true;
        //待定 
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4023";
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            //第一次菜单加载
            if (_isFirst)
            {
                _isFirst = false;

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_lishichaxun.png", Description, PluginKey, "16");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_lishichaxun.png", Description, PluginKey, "16");
                }

                enabled = false;
                return;
            }
            TabPageHelper.OpenTabPage(this.Description, new OrderSearchUI(), PluginKey);
            Application.Subscribe("IDC_Clear", this);

            base.load();
        }
        public override void BeforeTerminating()
        {
            //////this.Application.Panel.Items.Remove(item2);
            //////this.Application.Panel.Items.Remove(item);

        }
        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

        }

        #endregion
    }
}
