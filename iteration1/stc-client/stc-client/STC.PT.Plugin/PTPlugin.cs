using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.PT.Plugin.Handler;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowManager = STC.Common.Utils.WindowManager;

namespace STC.PT.Plugin
{
    public class PTPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 4;
        NotifyCloseTabItem notifyCloseTabItem;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.PTPlugin; }
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
            get { return "成交管理插件"; }
        }

        public override string Description
        {
            get { return "成交管理"; }
        }

        public override string Author
        {
            get { return "陈虎"; }
        }

        #endregion


        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4019";

        private bool _isFirst = true;
        private bool _isSubSetting = false;

        private PTInnerView oldView;
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_isSubSetting)
                {
                    InitSetting();
                    _isSubSetting = true;
                }
            });

            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                BaseHandler.Instance.QueryTags();
            });

            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_pt.png", Description, PluginKey, "14");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_pt.png", Description, PluginKey, "14");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);

                //RFQ订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                //QuantGroupHandler.Instance.RegSystemReadyEvent();
                //////待优化，加快系统启动速度
                ////STC.Rfq.Plugin.View.RfqOrderList rfqorderlist = new RfqOrderList();
                ////registerObject("RfqOrderList", rfqorderlist);

                ////STC.Rfq.Plugin.View.RfqTopBar rfqtopbar = new RfqTopBar();
                ////rfqtopbar.DataContext = rfqorderlist.DataContext;
                ////registerObject("RfqTopBar", rfqtopbar);
                //PTInnerViewModel.Instance.Init();
                return;
            }

            this.broadcast("loadCRM", string.Empty);
            ////RfqGroupHandler.Instance.init();

            oldView = new PTInnerView();
            //TabPageHelper.OpenTabPage(this.Description, new PostTradeGUI() { DataContext = vm }, PluginKey);
            // TabPageHelper.OpenTabPage(this.Description, new PostTradeGUI(), PluginKey);
            TabPageHelper.OpenTabPage(this.Description, oldView, PluginKey);

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }
           
            //PTExecOrderHandler.Instance.Init();
            // base.load();
        }

        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            SaveOrderControlPosition();
            //CloseAllOpenedOrderControl();

            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    ////if (tabItem != null && tabItem.Content != null && tabItem.Content is MainView)
                    ////{
                    ////    LogHelper.Debug("RfqList:" + tabItem.Header + " UserControl_Unloaded ");
                    ////    MainView mainView = tabItem.Content as MainView;
                    ////    mainView.Dispose();

                    ////    RfqCdccHandler.Instance.Clear();
                    ////}
                }
            }
        }

        private void InitSetting()
        {
            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
        }

        private void Instance_ShowDesign(object obj)
        {
            if (obj != null)
            {
                SettingView settingView = obj as SettingView;
                if (settingView != null)
                {
                    TabItem userTabItem = new TabItem();
                    PTInnerSettingView view = new PTInnerSettingView();
                    userTabItem.Content = view;
                    userTabItem.Header = "成交管理";
                    settingView.AddTabPage(userTabItem);
                }
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

                if (key.Contains("STC.Rfq.plugin"))
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
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;
            //处理消息提醒
            if (obj.Code != "PTMsg")
            {
                return;
            }
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                string execid = obj.Msg;

                //if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                //{
                //    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                //}
                //System.Windows.Application.Current.MainWindow.Activate();
                if (!TabPageHelper.IsExistTabItem(this.Description))
                {
                    oldView = new PTInnerView();
                    TabPageHelper.OpenTabPage(this.Description, oldView, PluginKey);
                }
                oldView.selectItemByExecID(execid);
            });
        }
        //Model.RfqOrderDetailData InReq = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.RfqOrderDetailData>(obj.Msg);

        //RfqOrderDetailDataInf2Model rfim = new RfqOrderDetailDataInf2Model();
        //bool isHavePrice = false;
        //if (JsonSplit.IsJson(InReq.inf2))
        //{
        //    rfim = Newtonsoft.Json.JsonConvert.DeserializeObject<RfqOrderDetailDataInf2Model>(InReq.inf2);

        //    if (!string.IsNullOrEmpty(rfim.quantity))
        //    {
        //        isHavePrice = true;
        //    }
        //}


        //// 获取订单信息
        ////先判断是否有该插件
        //IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin((int)PluginKeyEnum.RfqPlugin);
        //if (plugin != null)
        //{
        //    object postionctl = AppUtils.Application.GetObject($"{(int)PluginKeyEnum.RfqPlugin}_RfqOrderList");

        //    if (postionctl != null)
        //    {
        //        RfqOrderList rol = (RfqOrderList)postionctl;
        //        RfqOrderListViewModel rolvm = rol.DataContext as RfqOrderListViewModel;

        //        RfqOrderDetailData m2 = null;

        //        if (isHavePrice)
        //        {
        //            m2 = rolvm.FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == rfim.QuoteReqID);
        //            if (m2 != null)
        //            {
        //                //这里判断是否是策略自动计算的价格和数量  "{"quantity":"2000.00000000","price":"3.4500","QuoteReqID":"230220090411005140"}"
        //                if (isHavePrice)
        //                {
        //                    if (!string.IsNullOrEmpty(rfim.quantity))
        //                    {
        //                        decimal tmpv = CommonUtils.ConvertToDecimal(rfim.quantity) / 1000m;
        //                        m2.OrderQty = tmpv.ToString("0.##");
        //                        m2.OrderQty2 = m2.OrderQty;
        //                    }

        //                    if (!string.IsNullOrEmpty(rfim.price))
        //                    {
        //                        isHavePrice = true;
        //                        decimal d = 0m;
        //                        if (decimal.TryParse(rfim.price, out d))
        //                        {
        //                            m2.Price = d.ToString("F4");
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            m2 = InReq;

        //            if (CommonUtils.IsNumeric(m2.OrderQty))
        //            {
        //                decimal tmpv = CommonUtils.ConvertToDecimal(m2.OrderQty) / 1000m;
        //                m2.OrderQty = tmpv.ToString("0.##");
        //            }
        //            else
        //            {
        //                m2.OrderQty = "0";
        //            }
        //            if (CommonUtils.IsNumeric(m2.OrderQty2))
        //            {
        //                decimal tmpv = CommonUtils.ConvertToDecimal(m2.OrderQty2) / 1000m;
        //                m2.OrderQty2 = tmpv.ToString("0.##");
        //            }
        //            else
        //            {
        //                m2.OrderQty2 = "0";
        //            }
        //        }

        //        m2.IsSelfRfq = m2.UserName == LocalEnv.UserInfo.UserName ? "1" : "0";

        //        if (!string.IsNullOrEmpty(m2.ValidUntilTime))
        //        {
        //            DateTime date = Utils.RfqTools.ConvertCfetsDataTime(m2.ValidUntilTime);
        //            m2.ValidUntilTime = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //        }
        //        else
        //        {
        //            m2.ValidUntilTime = "";
        //        }

        //        if (!string.IsNullOrEmpty(m2.TransactTime))
        //        {
        //            DateTime date2 = Utils.RfqTools.ConvertCfetsDataTime(m2.TransactTime);
        //            m2.TransactTime = date2.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //        }
        //        else
        //        {
        //            m2.TransactTime = "";
        //        }


        //        RfqReplayRefuseView rrrv = new RfqReplayRefuseView(m2);
        //        rrrv.SetAction("ChangeOrCancelOrder");
        //        rrrv.Show();
        //    }
        //}
        #endregion
    }
}
