using com.gateway.log;
using STC.Common.Api;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.Utils.SqlLite;
using STC.Data.Plugin.Handler;
using STC.Frame.Plugin;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin
{
    /// <summary>
    /// 债券基础数据插件类
    /// </summary>
    public class BasePlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = -1;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return 2; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = false;
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
            get { return "基础数据插件"; }
        }

        public override string Description
        {
            get { return "基础数据同步服务器数据到客户端"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
        }

        #endregion

        #region load,unload
        
        protected override void load()
        {
            DataHandler.Instance.Start();

            if (!CommonUtils.IsBDBAppName())
            {
                //启动恢复历史市场成交程序
                STC.Common.Data.STCDbHandler.Instance.Start();
            }            
            //SyncDataHandler.Instance.Start();

            // DataHandler.Instance.SyncBond();

            if (LoadConfig.LoadBond)
            {
                //启动线程加载数据
                //registerObject("Bond", DataHandler.Instance.GetAllBondData());

                registerObject("IsEndOfLoadFromDB", DataHandler.Instance.GetEndOfLoadFromDB());
                registerObject("Bond", DataHandler.Instance.GetAllBondDic());
                registerObject("Inst", DataHandler.Instance.GetAllInstInfo());

                //Application.RegisterObject("Bond", DataHandler.Instance.GetAllBondDic());
            }

            //ClientApi.Instance.OnClear += Instance_OnClear;
            //ClientApi.Instance.SubscribeBaseClear();

            //Application.Subscribe("STC_Clear", this);

            base.load();
        }

        public override void BeforeTerminating()
        {
            STC.Common.Data.STCDbHandler.Instance.Dispose();
            BondsInfoSqliteManager.Instance.Close();
        }

        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);
        }

        #endregion

        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                LogHelper.Info("receive a clear msg,so reload order");

                Application.BroadCast(Consts.Topic_Clear, obj);
                
            }
        }
    }
}
