using STC.Common.Handler.Admin;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Chart.Plugin
{
    public class ChartPlugin : AbstractPlugin
    {
        #region 插件信息
        private int _tag = 0;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get {return (int)PluginKeyEnum.ChartPlugin; }
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

        private bool isFullScreen = false;
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
            get { return "绘图控件"; }
        }

        public override string Description
        {
            get { return "绘图"; }
        }

        public override string Author
        {
            get { return "陈彦畅"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4005";

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            base.load();
        }

        protected override void unload()
        {
            base.unload();
        }


        public override void BeforeTerminating()
        {

        }
        #endregion
    }
}
