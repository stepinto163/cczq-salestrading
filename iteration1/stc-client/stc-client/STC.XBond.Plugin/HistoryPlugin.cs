using STC.Common.Utils;
using STC.Frame.Plugin;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Frame.Common;
using STC.Common.Handler.Admin;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 历史查询插件类
    /// </summary>
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
                return "历史查询";
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
                return "历史查询插件";
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
        private readonly string _Code = "4003";
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
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_lishichaxun.png", Description, PluginKey, "8");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_lishichaxun.png", Description, PluginKey, "8");
                }

                _isFirst = false;

                enabled = false;

                return;
            }

            LoadConfig.LoadBond = true;
            HistoryView view = new HistoryView();
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
