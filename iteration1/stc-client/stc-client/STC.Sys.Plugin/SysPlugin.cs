using STC.Frame.Plugin;
using STC.Sys.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Sys.Plugin
{
    /// <summary>
    /// 系统信息插件类
    /// </summary>
    public class SysPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 100;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return 3; }
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
            get { return "系统插件"; }
        }

        public override string Description
        {
            get { return "检测系统运行情况"; }
        }

        public override string Author
        {
            get { return "孔德帅"; }
        }

        #endregion

        #region load,unload
        System.Timers.Timer timer = null;
        protected override void load()
        {
            Task.Run(() => {
                timer = new System.Timers.Timer();
                timer.Enabled = true;
                timer.Interval = 60000 * 3;
                timer.Start();
                timer.Elapsed += Timer_Elapsed;
                SysInfo.sysInfo();
            });

            base.load();
        }
        bool isRun = false;
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isRun)
            {
                isRun = true;
                LogClearHelper.Clear();
                RockDbClearHelper.Clear();
                SysInfo.sysRunInfo();
                isRun = false;
                Task.Factory.StartNew(() =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
            }
        }

        public override void BeforeTerminating()
        {
            if (timer != null) {
                timer.Close();
            }

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
