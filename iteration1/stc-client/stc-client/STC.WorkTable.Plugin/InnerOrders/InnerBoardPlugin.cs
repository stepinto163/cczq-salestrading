using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.WorkTable.Plugin.InnerOrders.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.InnerOrders
{
    public class InnerBoardPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 5;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.InnerBoardPlugin; }
        }

        public override string PluginName
        {
            get { return "内部看板插件"; }
        }

        public override string Description
        {
            get { return "内部看板"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
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
        #endregion

        #region load,unload

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4013";

        private bool _isFirst = true;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            //第一次菜单加载
            if (_isFirst)
            {
                AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Frame;component/Images/stc_image_menulist_neibukanban.png", Description, PluginKey, "9");

                _isFirst = false;
                enabled = false;

                return;
            }

            TabPageHelper.OpenTabPage(this.Description, new InnerBoardView(), PluginKey);

            Application.Subscribe("STC_Clear", this);

            base.load();

            //接收数据
            try
            {
                //IntentionHandler.Instance.init();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        public override void BeforeTerminating()
        {

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
