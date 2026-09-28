using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.WorkTable.Plugin.Utils.Data.CRM.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin
{
    public class WTCrmPlugin : AbstractPlugin
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
            get { return (int)PluginKeyEnum.WorkstationCrm; }
        }

        public override string PluginName
        {
            get { return "CRM数据插件"; }
        }

        public override string Description
        {
            get { return "CRM数据同步服务器数据到客户端"; }
        }

        public override string Author
        {
            get { return "饶嘉琪"; }
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

        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get { return isFullScreen; }
            set { isFullScreen = value; }
        }
        #endregion

        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4008";

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            LogHelper.Info("LoadConfig.LoadBond:" + LoadConfig.LoadBond);
            //启动线程加载数据
            DataHandler.Instance.Start();
            SyncDataHandler.Instance.Start();

            //if (LoadConfig.LoadBond)
            //{
            //    //启动线程加载数据
            //    registerObject("Bond", DataHandler.Instance.GetAllBondData());
            //}

            registerObject("CRM_Inst", DataHandler.Instance.GetAllInstData());
            registerObject("CRM_Contact", DataHandler.Instance.GetAllContactData());
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

        }

        #endregion
    }
}
