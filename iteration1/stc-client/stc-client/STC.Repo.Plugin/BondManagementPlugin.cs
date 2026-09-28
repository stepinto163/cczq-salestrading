using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Repo.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stc.repo.plugin
{
    public class BondManagementPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 21;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.BondManagement; }
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

        public override string PluginName
        {
            get { return "债券管理插件"; }
        }

        public override string Description
        {
            get { return "债券管理"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4026";

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
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_rfq.png", Description, PluginKey, "18");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_rfq.png", Description, PluginKey, "18");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);

                return;
            }

           

            //ViewModel.MainViewModel vm = new ViewModel.MainViewModel();
            TabPageHelper.OpenTabPage(this.Description, new BondManagementView(), PluginKey);

            //if (notifyCloseTabItem == null)
            //{
            //    notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
            //    notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            //}

            base.load();
        }

        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            
        }
        public override void BeforeTerminating()
        {

        }



        protected override void unload()
        {
            base.unload();
            //_isSubSetting = false;
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
