using com.gateway.log;
using STC.Common.Utils.SqlLite;
using STC.Data.Plugin.Tags;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin
{
    public class TagsPlugin : AbstractPlugin
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
            get { return (int)PluginKeyEnum.TagsPlugin; }
        }

        public override string PluginName
        {
            get { return "标签数据插件"; }
        }

        public override string Description
        {
            get { return "标签数据同步服务器数据到客户端"; }
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


        private readonly string LoadTags = "loadTags";

        protected override void load()
        {
            //bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            //if (!isPermission)
            //    return;

            this.subscribe(LoadTags, this);


            //LogHelper.Info("LoadConfig.LoadBond:" + LoadConfig.LoadBond);
            ////启动线程加载数据
            //STC.Data.Plugin.Issuer.Handler.DataHandler.Instance.Start();
            //STC.Data.Plugin.Issuer.Handler.SyncDataHandler.Instance.Start();


            //registerObject("Issuer_Inst", STC.Data.Plugin.Issuer.Handler.DataHandler.Instance.GetAllInstData());
            //registerObject("Issuer_Contact", STC.Data.Plugin.Issuer.Handler.DataHandler.Instance.GetAllContactData());
            base.load();
        }




        public override void BeforeTerminating()
        {
            try
            {
                //LogHelper.Info("TagsPlugin.BeforeTerminating() starting cleanup");

                // 1. 停止同步定时器（释放定时器资源，防止继续运行）
                try
                {
                    STC.Data.Plugin.Tags.Handler.SyncDataHandler.Instance.Stop();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Error stopping SyncDataHandler:", ex);
                }

                // 2. 停止数据处理器后台线程（设置停止标志，等待任务完成）
                try
                {
                    STC.Data.Plugin.Tags.Handler.DataHandler.Instance.Stop();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Error stopping DataHandler:", ex);
                }

                // 3. 关闭数据库连接（此时所有后台任务已完成）
                try
                {
                    TagsSqliteManager.Instance.Close();
                    //LogHelper.Info("TagsSqliteManager closed successfully");
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Error closing TagsSqliteManager:", ex);
                }

                //LogHelper.Info("TagsPlugin.BeforeTerminating() cleanup completed");
            }
            catch (Exception ex)
            {
                LogHelper.Error("Fatal error in TagsPlugin.BeforeTerminating():", ex);
            }
        }
        #endregion


        private bool isLoadTags = false;

        #region Notify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            LogHelper.Info(string.Format("onNotify name:{0}, o:{1}, isLoadTags:{2}", name, o, isLoadTags));
            if (name.Equals(LoadTags) && !isLoadTags)
            {
                isLoadTags = true;
                LogHelper.Info("start load Issuer data");

                //启动线程加载数据
                System.Threading.ThreadPool.QueueUserWorkItem(obj =>
                {
                    STC.Data.Plugin.Tags.Handler.DataHandler.Instance.Start();
                    STC.Data.Plugin.Tags.Handler.SyncDataHandler.Instance.Start();
                });

                //registerObject("Issuer", STC.Data.Plugin.Tags.Handler.DataHandler.Instance.GetAllIssuerData());
                //registerObject("Guarantor", STC.Data.Plugin.Tags.Handler.DataHandler.Instance.GetAllGuarantorData());
            }
        }

        #endregion
    }
}
