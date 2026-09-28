using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.DownloadCenter.Plugin.View;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.DownloadCenter.Plugin
{
    public class DownloadCenterPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 20;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.DownloadCenterPlugin; }
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
            get { return "下载中心插件"; }
        }

        public override string Description
        {
            get { return "下载中心"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4017";

        private bool _isFirst = true;

        private bool _isSubSetting = false;

        TabItem item = new TabItem();
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            if (!_isSubSetting)
            {
                _isSubSetting = true;
            }
            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_xiazaizhongxin.png", Description, PluginKey, "11");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_xiazaizhongxin.png", Description, PluginKey, "11");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                return;
            }

            LoadConfig.LoadBond = true;
            Utils.DownloadSqliteManager.Instance.GetType();


            TabPageHelper.OpenTabPage(this.Description, new DownloadView(), PluginKey);

            Application.Subscribe("STC_Clear", this);


            base.load();
        }

        public override void BeforeTerminating()
        {
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
