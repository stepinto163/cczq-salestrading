using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.WorkTable.Plugin.Utils.Data;
using STC.WorkTable.Plugin.View.WorkTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkTable.Plugin
{
    public class WorkTablePlugin : AbstractPlugin
    {
        #region 插件信息
        private int _tag = 15;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain
        {
            get { return isMain; }
            set { IsMain = value; }
        }

        protected new bool isAlone = true;
        public override bool IsAlone
        {
            get { return isAlone; }
            set { isAlone = value; }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = true;
        public override bool IsFullScreen
        {
            get { return isFullScreen; }
            set { isFullScreen = value; }
        }
        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.Workstation; }
        }

        public override string PluginName
        {
            get { return "需求工作台插件"; }
        }

        public override string Description
        {
            get { return "新需求工作台"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }
        #endregion

        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4014";

        private bool _isFirst = true;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            //STC.Common.Model.Admin.Module module = AdminHandler.Instance.GetModuleById("1001");
            //if (module == null)
            //{
            //    return;
            //}

            //第一次菜单加载
            if (_isFirst)
            {
                AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_jiaoyibao.png", Description, PluginKey, "9");

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                WorkTableHandler.Instance.RegSystemReadyEvent();
                return;
            }

            LoadConfig.LoadBond = true;

            TabPageHelper.OpenTabPage(this.Description, new MainView(), PluginKey);

            Application.Subscribe("STC_Clear", this);

            base.load();

            //接收数据
            try
            {
                IntentionHandler.Instance.init();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        protected override void unload()
        {
            base.unload();
        }


        public override void BeforeTerminating()
        {
            
        }

        private void SaveOpenedWindows()
        {
            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("STC.WorkTable.Plugin"))
                {
                    w.Close();
                }
            }
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

                if (name.Equals(Consts.Topic_RestartApp))
                {
                    SaveOpenedWindows();
                }
            }
        }

        #endregion
    }
}
