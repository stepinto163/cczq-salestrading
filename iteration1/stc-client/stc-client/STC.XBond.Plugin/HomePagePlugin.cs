using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.MarketExec;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.Price;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.DataProcesser;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.HomePage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using STC.Common.Data;
using AppUtils = STC.Common.Utils.AppUtils;
using STC.Common.View.Drp.Component;
using STC.XBond.Plugin.View;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 主页功能插件类
    /// </summary>
    public class HomePagePlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 2;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.HomePagePlugin; }
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
            get { return "主页插件"; }
        }

        public override string Description
        {
            get { return "主页"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4001";

        private bool _isFirst = true;


        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                //第一次菜单加载
                if (_isFirst)
                {
                    if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                    {
                        AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_zhuye.png", Description, PluginKey, "1");
                    }
                    else
                    {
                        AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_zhuye.png", Description, PluginKey, "1");
                    }

                    _isFirst = false;
                    enabled = false;


                    return;
                }

                HomePageDataHandler.ReadHomePageDisplayMode();
                TabPageHelper.OpenTabPage(this.Description, new HomePageView(), PluginKey);

                MyOrderProcesser.Processer.Init();
                EmsOrderProcesser.Processer.Init();
                LastExecutionProcesser.Processer.Init();
                if (LocalEnv.ConfInfo.IsAccountUser == "1")
                {
                    AdminHandler.Instance.queryAccountAndUsers();
                }

                LoadConfig.LoadBond = true;

                Application.Subscribe("STC_Clear", this);

                    //MarketExecHandler.Instance.Init();
                    //OwnExecHandler.Instance.Init();
                    //MDHandler.Instance.Init();
                    //MDHandler.Instance.SubscribeMDPriceBySymbol("151883.SH");
                    //TBookHandler.Instance.Init();

                //订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                base.load();

            });



            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    OrdersHandler.Instance.init();
                    EmsOrdersHandler.Instance.init();
                    ExecutionsHandler.Instance.init();
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequXBondCommonPlugin load", e);
                }
            });

            //PricingPanel pricingPanel = new PricingPanel();
            //OrderSearchUI orderSearchUI = new OrderSearchUI();
            //SetPanel setPanel = new SetPanel();
            //item.Content = pricingPanel;
            //item.Header = pricingPanel.Title;
            //this.Application.Panel.Items.Add(item);
            //item2.Content = orderSearchUI;
            //item2.Header = orderSearchUI.Title;
            //this.Application.Panel.Items.Add(item2);
            //item3.Content = setPanel;
            //item3.Header = "设置";
            //this.Application.Panel.Items.Add(item3);

            // TabPageHelper.OpenTabPage(this.Description, new XBondView(), PluginKey);
        }

        public override void BeforeTerminating()
        {
            //this.Application.Panel.Items.Remove(item3);
            //this.Application.Panel.Items.Remove(item2);
            //this.Application.Panel.Items.Remove(item);
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
            }
        }


        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            
            if (obj.Code != "DRPMsgPo" && obj.Code != "HistoryView_Focus_Tag")
            {
                return;
            }

            if (obj.Code == "DRPMsgPo")
            {
                //定向回价 价格匹配，点击 待办事项，处理按钮 
                if (obj.Info1 == "TTbookSystemMessage")
                {
                    Common.Model.Admin.TTbookSystemMessage msgpp = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Model.Admin.TTbookSystemMessage>(obj.Msg);
                    // 跳过SystemMsgToShowMsg 处理界面部分，重组消息
                    Common.Model.Admin.TMessageContent detail = Ats.Foundation.Message.MsgHelper.Deserialize<Common.Model.Admin.TMessageContent>(msgpp.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                    detail.type = "0";
                    msgpp.content = Newtonsoft.Json.JsonConvert.SerializeObject(detail);
                    msgpp.type = "0";
                    Common.ViewModel.Drp.MsgPo msgPo = Common.ViewModel.Drp.Utils.DRPTools.SystemMsgToShowMsg(msgpp, null);
                    Common.View.Drp.DirectionalReturnPriceView priceMatchView = new Common.View.Drp.DirectionalReturnPriceView(msgPo);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(msgPo.Id), priceMatchView);
                }
                else if (obj.Info1 == "MessageContent")
                {
                    Common.Model.Admin.TMessageContent detail = Ats.Foundation.Message.MsgHelper.Deserialize<Common.Model.Admin.TMessageContent>(obj.Msg, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                    Common.ViewModel.Drp.MsgPo msgPo = new Common.ViewModel.Drp.MsgPo();
                    msgPo.Id = detail.id;
                    msgPo.Detail = detail.detail;
                    msgPo.Content = detail.content;
                    Common.View.Drp.DirectionalReturnPriceView priceMatchView = new Common.View.Drp.DirectionalReturnPriceView(msgPo);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(msgPo.Id), priceMatchView);
                }
                else if (obj.Info1 == "MMBreakAlarmReminder")
                {
                    Common.Model.Admin.TMessageContent detail = Ats.Foundation.Message.MsgHelper.Deserialize<Common.Model.Admin.TMessageContent>(obj.Msg, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                    string key = $"MMBreakAlarmReminder-TradeBook";
                    Common.ViewModel.Drp.MsgPo msgPo = new Common.ViewModel.Drp.MsgPo();
                    msgPo.Id = detail.id;
                    msgPo.Detail = detail.detail;
                    msgPo.Content = detail.content;
                    MMAgencyMsgView msgView = new MMAgencyMsgView(msgPo);
                    WindowManager.Instance.OpenOnlyOneWindow(key, msgView);
                }
                else if (obj.Info1 == "MsgPo")
                {
                    Common.ViewModel.Drp.MsgPo msgPo = Newtonsoft.Json.JsonConvert.DeserializeObject<Common.ViewModel.Drp.MsgPo>(obj.Msg);
                    Common.View.Drp.DirectionalReturnPriceView priceMatchView = new Common.View.Drp.DirectionalReturnPriceView(msgPo);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(msgPo.Id), priceMatchView);
                }
            }
            else if (obj.Code == "HistoryView_Focus_Tag" )
            {
                //打开历史查询-手工跑批历史
                if (obj.Info1 == "OpenProcessingRecordHist")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!TabPageHelper.IsExistTabItem("历史查询"))
                        {
                            TabPageHelper.OpenTabPage("历史查询", new HistoryView(), (int)PluginKeyEnum.HistoryPlugin);
                        }

                        foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                        {
                            if (tabItem.Content is HistoryView)
                            {
                                tabItem.IsSelected = true;

                                HistoryView historyView = tabItem.Content as HistoryView;
                                historyView.SelectpositionRBView();
                                break;
                            }
                        }
                    });
                }
            }
        }
        #endregion
    }
}
