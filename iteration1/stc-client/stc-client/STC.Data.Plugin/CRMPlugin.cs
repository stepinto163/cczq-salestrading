
using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Data.Plugin.CRM;
using STC.Data.Plugin.Handler;
using STC.Frame.Common;
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
    /// CRM数据插件
    /// </summary>
    public class CRMPlugin : AbstractPlugin
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
            get { return (int)PluginKeyEnum.CrmPlugin; }
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


        private readonly string LoadCRM = "loadCRM";

        protected override void load()
        {
            //bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            //if (!isPermission)
            //    return;

            this.subscribe(LoadCRM, this);


            //LogHelper.Info("LoadConfig.LoadBond:" + LoadConfig.LoadBond);
            ////启动线程加载数据
            //STC.Data.Plugin.CRM.Handler.DataHandler.Instance.Start();
            //STC.Data.Plugin.CRM.Handler.SyncDataHandler.Instance.Start();


            //registerObject("CRM_Inst", STC.Data.Plugin.CRM.Handler.DataHandler.Instance.GetAllInstData());
            //registerObject("CRM_Contact", STC.Data.Plugin.CRM.Handler.DataHandler.Instance.GetAllContactData());
            base.load();
        }




        public override void BeforeTerminating()
        {
            SqliteManager.Instance.Close();
        }
        #endregion


        private bool isLoadCRM = false;

        #region Notify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            LogHelper.Info(string.Format("onNotify name:{0}, o:{1}, isLoadCRM:{2}", name, o, isLoadCRM));
            if (name.Equals(LoadCRM) && !isLoadCRM)
            {
                isLoadCRM = true;
                LogHelper.Info("start load CRM data");

                //启动线程加载数据
                System.Threading.ThreadPool.QueueUserWorkItem(obj =>
                {
                    try
                    {
                        LogHelper.Debug("Start sync insts.");
                        STC.Data.Plugin.CRM.Handler.DataHandler.Instance.SyncInsts();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("Sync insts exception.");
                        LogHelper.Error(ex);
                    }
                    try
                    {
                        LogHelper.Debug("Start sync contact.");
                        STC.Data.Plugin.CRM.Handler.DataHandler.Instance.SyncContact();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("Sync contact exception.");
                        LogHelper.Error(ex);
                    }
                    try
                    {
                        LogHelper.Debug("Start sync broker.");
                        STC.Data.Plugin.CRM.Handler.DataHandler.Instance.SyncBroker();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("Sync broker exception.");
                        LogHelper.Error(ex);
                    }

                    STC.Data.Plugin.CRM.Handler.DataHandler.Instance.Start();
                    STC.Data.Plugin.CRM.Handler.SyncDataHandler.Instance.Start();
                    STC.Data.Plugin.CRM.Handler.DataHandler.Instance.SubscribeCrmInfo();
                });

                registerObject("CRM_Inst", STC.Data.Plugin.CRM.Handler.DataHandler.Instance.GetAllInstData());
                registerObject("CRM_Contact", STC.Data.Plugin.CRM.Handler.DataHandler.Instance.GetAllContactData());
                registerObject("CRM_Broker", STC.Data.Plugin.CRM.Handler.DataHandler.Instance.GetAllBrokerData());
            }

        }

        #endregion
    }
}
