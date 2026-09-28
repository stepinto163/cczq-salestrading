using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Execute.Plugin.View.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Execute.Plugin.Utils;
using STC.Execute.Plugin.Handler;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using System.Windows;
using STC.Common.Data;

namespace STC.Execute.Plugin
{
    public class ExecutePlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;

        private int _tag = 11;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            //get { return 11; }
            get { return (int)PluginKeyEnum.ExecutePlugin; }
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
            get { return "经纪商成交插件"; }
        }

        public override string Description
        {
            get { return "经纪商成交"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4006";

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
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_execute.png", Description, PluginKey, "13");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_execute.png", Description, PluginKey, "13");
                }

                enabled = false;
                _isFirst = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                ExecuterOrderHandler.Instance.RegSystemReadyEvent();
                return;
            }

            TabPageHelper.OpenTabPage(this.Description, new ExecuteOrderView(), PluginKey);

            ExecuterOrderHandler.Instance.Init();
            base.load();
        }

        public override void BeforeTerminating()
        {
            
        }

        private void SaveOrderControlPosition()
        {
            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("STC.Execute.Plugin"))
                {
                    w.Close();
                }
            }
        }
        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(Consts.Topic_Clear))
                {

                }

                if (name.Equals(Consts.Topic_RestartApp))
                {
                    SaveOrderControlPosition();
                }
            }
        }

        #endregion
    }
}
