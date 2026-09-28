using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin
{
    public class BondGroupManagePlugin : AbstractPlugin
    {
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
                return "券组管理";
            }
        }


        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = true;
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

        public override int PluginKey
        {
            get
            {
                return (int)PluginKeyEnum.BondGroupManagePlugin;
            }
        }

        public override string PluginName
        {
            get
            {
                return "券组管理插件";
            }
        }

        private int _tag = 220;
        public override int Tag
        {
            get
            {
                return _tag;
            }

            set
            {
                _tag = value;
            }
        }


        private readonly string _ModelId = "4000";
        private readonly string _Code = "4027";
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
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_pt.png", Description, PluginKey, "19");
                }
                else

                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_pt.png", Description, PluginKey, "19");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                return;
            }
            //LoadConfig.LoadBond = true;
            SecurityGroupManagerView view = new SecurityGroupManagerView();
            TabPageHelper.OpenTabPage(this.Description, view, PluginKey);
            base.load();
        }

        public override void BeforeTerminating()
        {
        }

        public override void onNotify(string name, object o)
        {
        }
    }
}
