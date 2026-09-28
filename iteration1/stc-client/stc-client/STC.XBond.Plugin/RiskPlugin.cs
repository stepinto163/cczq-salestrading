using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Risk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 风控指标插件类
    /// </summary>
    public class RiskPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 9;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.RiskPlugin; }
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
            get { return "风控指标"; }
        }

        public override string Description
        {
            get { return "风控指标"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4004";
        private bool _isFirst = true;
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            //第一次菜单加载，需要进入菜单的模块才需要
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_fengkongshezhi.png", Description, PluginKey, "14");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_fengkongshezhi.png", Description, PluginKey, "14");
                }

                _isFirst = false;
                enabled = false;

                return;
            }

            TabPageHelper.OpenTabPage(this.Description, new RiskIndexView(), PluginKey);
            
        }

        public override void BeforeTerminating()
        {
        }

        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

        }
    }
}
