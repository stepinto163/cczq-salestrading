using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Common.ViewModel.Drp;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.UI;
using STC.quant.plugin.Utils;
using STC.quant.plugin.Utils.Data;
using STC.quant.plugin.View;
using STC.quant.plugin.View.Component;
using STC.quant.plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowManager = STC.Common.Utils.WindowManager;

namespace STC.quant.plugin
{
    public class QuantPlugin : AbstractPlugin
    {
        #region 插件信息


        private int _tag = 13;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.QuantPlugin; }
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
            get { return "策略控制台插件"; }
        }

        public override string Description
        {
            get { return "策略控制台"; }
        }

        public override string Author
        {
            get { return "李素静"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4007";

        private bool _isFirst = true;

        //private bool _isSubSetting = false;

        protected override void load()
        {
            //内部看板批量识别
            DesignHandler.Instance.OnShowStrategyGroupManagementWin -= Instance_OnShowStrategyGroupManagementWin;
            DesignHandler.Instance.OnShowStrategyGroupManagementWin += Instance_OnShowStrategyGroupManagementWin;

            if (CommonUtils.IsBDBAppName())
            {
                return;
            }

            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            //if (!_isSubSetting)
            //{
            //    InitSetting();
            //    _isSubSetting = true;
            //}
            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_quant.png", Description, PluginKey, "5");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_quant.png", Description, PluginKey, "5");
                }
                _isFirst = false;

                

                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                QuantGroupHandler.Instance.RegSystemReadyEvent();
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    QuantMyOrderProcesser.Processer.Init();
                    QuantMyExecutionProcesser.Processer.Init();
                    //QuantOrdersHandler.Instance.init();

                    ExecutionsHandler.Instance.init();
                    QuantGroupHandler.Instance.init();
                    AdminHandler.Instance.QuerySecurityGroupModule();
                }
                catch (Exception e)
                {
                    LogHelper.Error("QuantPlugin load", e);
                }
            });

            TabPageHelper.OpenTabPage(this.Description, new MainView(), PluginKey);

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
            base.load();
        }

        private void Instance_OnShowStrategyGroupManagementWin(object obj)
        {
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GROUPMANAGER))
            {
                //SecurityGroupManagerView securityGroupManagerView = new SecurityGroupManagerView();
                //securityGroupManagerView.setDefaultParam(CommonConstants.INNER_Type);
                //WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GROUPMANAGER, securityGroupManagerView);
                GroupManagerControl group = new GroupManagerControl();
                group.setDefaultParam(CommonConstants.INNER_Type);
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GROUPMANAGER, group);
            }
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

                if (key.Contains("STC.quant.plugin"))
                {
                    w.Close();
                }
            }
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

                if (name.Equals(Consts.Topic_RestartApp))
                {
                    SaveOrderControlPosition();
                }
            }
        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            //策略处理功能
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (!string.IsNullOrEmpty(obj.Msg) && obj.Msg.Contains("Quant_Focus_Tag"))
            {
                MsgPo msgPo = JsonConvert.DeserializeObject<MsgPo>(obj.Msg);
                if ("Quant_Focus_Tag".Equals(msgPo.ContentType))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            //to do ...

                            //if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                            //{
                            //    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                            //}
                            //System.Windows.Application.Current.MainWindow.Activate();

                            if (!TabPageHelper.IsExistTabItem(this.Description))
                            {
                                TabPageHelper.OpenTabPage(this.Description, new MainView(), PluginKey);
                            }

                            if (msgPo.MessageContent != null)
                            {
                                pocQuantId(msgPo.MessageContent.id);
                            }
                        }
                        catch
                        {

                        }
                    });
                }
            }
        }

        private void pocQuantId(string id)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is MainView)
            {
                MainView innerBoardView = tabItem.Content as MainView;
                if (innerBoardView.DataContext is MainViewModel)
                {
                    MainViewModel vm = innerBoardView.DataContext as MainViewModel;
                    MMListBoxViewModel model = vm.GetMMListBoxViewModelByGroupID(QuantCommonUtils.groupID);
                    model.SelItem(id);
                }
            }
        }
        #endregion
    }
}
