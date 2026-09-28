using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.IRS.Plugin.View;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace STC.IRS.Plugin
{
    public class IRSPlugin : AbstractPlugin
    {
        #region 插件信息
        NotifyCloseTabItem notifyCloseTabItem;
        private int _tag = 1;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.IRSPlugin; }
        }

        public override string PluginName
        {
            get { return "IRS看板插件"; }
        }

        public override string Description
        {
            get { return "IRS看板"; }
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

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4018";

        private bool _isFirst = true;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                BaseHandler.Instance.QueryTags();
            });

            //第一次菜单加载
            if (_isFirst)
            {

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_neibukanban.png", Description, PluginKey, "4");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_neibukanban.png", Description, PluginKey, "4");
                }

                _isFirst = false;
                enabled = false;

                return;
            }


            this.broadcast("loadCRM", string.Empty);
            this.broadcast("loadTags", string.Empty);

            DesignHandler.Instance.OnHotKey += Instance_OnHotKey;
            TabPageHelper.OpenTabPage(this.Description, new InnerBoardView() { DataContext = new InnerBoardViewModel() }, PluginKey);

            Application.Subscribe("STC_Clear", this);

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

            base.load();

            //接收数据
            try
            {
                IntentionIRSHandler.Instance.initInnerBoard();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            if (notifyCloseTabItem == null)
            {
                notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }
        }

        private void Instance_OnHotKey(EHotKeySetting obj)
        {
            if (obj == EHotKeySetting.打开内部看板)
            {
                if (TabPageHelper.IsExistTabItem(this.Description))
                {
                    return;
                }
                TabPageHelper.OpenTabPage(this.Description, new InnerBoardView() { DataContext = new InnerBoardViewModel() }, PluginKey);
            }
        }

        public override void BeforeTerminating()
        {
            //点击右下角提醒中的 查看按钮 自动选择历史作废区
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (TabPageHelper.IsExistTabItem(this.Description))
                {
                    TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

                    if (tabItem.Content is InnerBoardView)
                    {
                        InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                        innerBoardView.SaveLastExecutionsHeight();
                    }
                }
            });
        }

        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            //点击右下角提醒中的 查看按钮 自动选择历史作废区
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
                {
                    if (tabItem.GetHashCode() == tabItemHashCode)
                    {
                        if (tabItem.Content is InnerBoardView)
                        {
                            InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                            innerBoardView.SaveLastExecutionsHeight();
                        }
                    }
                }
            });
        }
        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

        }


        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "TableSvr_FOCUS_TAG_COPY" && !string.IsNullOrEmpty(obj.Msg))
            {
                //设置内部看板焦点
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                string groupid = string.Empty;
                TMessageRemark remark = JsonConvert.DeserializeObject<TMessageRemark>(msgPo.remark);
                if (remark != null && remark.focusType?.ToUpper() == "TAG")
                {
                    groupid = msgPo.groupId;
                }
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty, orderid = string.Empty });
                if (jsonObj.marketindicator != "2")
                {
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                    //if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                    //{
                    //    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    //}
                    //System.Windows.Application.Current.MainWindow.Activate();

                    if (!TabPageHelper.IsExistTabItem(this.Description))
                    {
                        TabPageHelper.OpenTabPage(this.Description, new InnerBoardView() { DataContext = new InnerBoardViewModel() }, PluginKey);
                    }

                    pocCopy(groupid, jsonObj.orderid, obj.Info1 == "1");
                });
            }
            else if (obj.Code == "TableSvr_FOCUS_TAG" && !string.IsNullOrEmpty(obj.Msg))
            {
                //设置内部看板焦点
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                string groupid = msgPo.groupId;
                TMessageRemark remark = JsonConvert.DeserializeObject<TMessageRemark>(msgPo.remark);
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty });
                if (jsonObj.marketindicator != "2")
                {
                    return;
                }


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                    //if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                    //{
                    //    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    //}
                    //System.Windows.Application.Current.MainWindow.Activate();

                    if (!TabPageHelper.IsExistTabItem(this.Description))
                    {
                        TabPageHelper.OpenTabPage(this.Description, new InnerBoardView() { DataContext = new InnerBoardViewModel() }, PluginKey);
                    }

                    if (remark != null && remark.focusType == "BOND")
                    {
                        pocBond(remark.focusBond);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(groupid))
                        {
                            pocTag(groupid);
                        }
                    }
                });
            }
            else if (obj.Code == "InnerBoard_Open_SendRequireView")
            {
                //打开发布需求并设置识别文本，自动识别
                try
                {
                    string tmp = Clipboard.GetText().Trim();
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        string key = $"SendRequireView-TradeBook";
                        if (tmp.Contains("\r\n") || tmp.Contains("\n"))
                        {
                            //打开批量识别
                            DesignHandler.Instance.ShowRecognizeIntentionQuoteWin(tmp);
                        }
                        else
                        {
                            Common.View.Order.InnerPanel.SendRequireView sendRequireView = new Common.View.Order.InnerPanel.SendRequireView("TradeBook", true, true);
                            WindowManager.Instance.OpenOnlyOneWindow(key, sendRequireView);
                        }
                    }
                }
                catch { }
            }
            else if (obj.Code == "EditFuzzyOrder")
            {
                //打开模糊需求
                try
                {
                    if (JsonSplit.IsJson(obj.Msg))
                    {
                        PriceMatchParam pm = JsonConvert.DeserializeObject<PriceMatchParam>(obj.Msg);
                        OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(pm.Info1);
                        if (otm.IsFromIRSorInner != null)
                        {
                            if (otm.IsFromIRSorInner.ToLower() == "innerboard")
                            {
                                return;
                            }
                        }
                    }

                    
                    string key = obj.RequestID;
                    if (!WindowManager.Instance.IsExist(key))
                    {
                        InnerFuzzyRequireView sendPointPriceView = new InnerFuzzyRequireView(obj.Msg);
                        WindowManager.Instance.OpenOnlyOneWindow(key, sendPointPriceView);
                    }
                }
                catch { }
            }

        }


        private void pocCopy(string groupid, string orderid, bool needPrompt)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is InnerBoardView)
            {
                InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                innerBoardView.singleTab.IsSelected = true;
                if (innerBoardView.DataContext is InnerBoardViewModel)
                {
                    InnerBoardViewModel vm = innerBoardView.DataContext as InnerBoardViewModel;
                    InnerOrderData record = vm.SelectedRowBySecurtiyID_Single_Copy(groupid, orderid);

                    if (record != null)
                    {
                        if (innerBoardView.InnerBoard_dataGrid_IRS.Items.Count > 0)
                        {
                            innerBoardView.InnerBoard_dataGrid_IRS.ScrollIntoView(record);
                        }

                        innerBoardView.InnerBoard_dataGrid_IRS.CopyData_Single(needPrompt);
                    }
                    else
                    {
                        CommonUtils.Prompt2Second("订阅复制", "无可处理数据");
                    }
                }
            }
        }


        private void pocTag(string groupid)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is InnerBoardView)
            {
                InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                innerBoardView.bestTab.IsSelected = true;
                if (innerBoardView.DataContext is InnerBoardViewModel)
                {
                    InnerBoardViewModel vm = innerBoardView.DataContext as InnerBoardViewModel;
                    vm.SelectedGroup(groupid);
                    if (innerBoardView.InnerBoard_dataGrid_IRS.Items.Count > 0)
                    {
                        innerBoardView.InnerBoard_dataGrid_IRS.ScrollIntoView(innerBoardView.InnerBoard_dataGrid_IRS.Items[0]);
                    }
                    if (innerBoardView.InnerBoard_Best_dataGrid_IRS.Items.Count > 0)
                    {
                        innerBoardView.InnerBoard_Best_dataGrid_IRS.ScrollIntoView(innerBoardView.InnerBoard_Best_dataGrid_IRS.Items[0]);
                    }
                }
            }
        }

        private void pocBond(string securityID)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is InnerBoardView)
            {
                InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                innerBoardView.bestTab.IsSelected = true;
                if (innerBoardView.DataContext is InnerBoardViewModel)
                {
                    InnerBoardViewModel vm = innerBoardView.DataContext as InnerBoardViewModel;
                    InnerOrderData record = vm.SelectedRowBySecurtiyID_Best(securityID);

                    if (record != null)
                    {
                        if (innerBoardView.InnerBoard_Best_dataGrid_IRS.Items.Count > 0)
                        {
                            innerBoardView.InnerBoard_Best_dataGrid_IRS.ScrollIntoView(record);
                        }
                    }
                }
            }
        }
        #endregion 
    }
}
