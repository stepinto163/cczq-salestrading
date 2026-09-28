using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.View.Setting;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.InnerBoard.Plugin.View;
using STC.InnerBoard.Plugin.ViewModel;
using STC.InnerBoard.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.WorkBench.Plugin.InnerOrders
{
    public class InnerBoardPlugin : AbstractPlugin
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
            get { return (int)PluginKeyEnum.InnerBoardPlugin; }
        }

        public override string PluginName
        {
            get { return "内部看板插件"; }
        }

        public override string Description
        {
            get { return "内部看板"; }
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
        private readonly string _Code = "4013";

        private bool _isFirst = true;

        private bool _isSubSetting = false;

        private bool _isIntentionManagement = false;


        protected override void load()
        {
            if (_isIntentionManagement == false)
            {
                // 如果有需求工作台权限，加载“需求管理”
                bool isIntentionPermission = AdminHandler.Instance.CheckPermisionById("4000", "4008");
                if (!isIntentionPermission || (Common.Utils.CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions != "1"))
                { }
                else
                {
                    if (_isIntentionManagement == false)
                    {
                        _isIntentionManagement = true;
                        InitSetting();

                        SystemParameterReq req = new SystemParameterReq();
                        req.ParamKey = CommonUtils.PARAM_KEY_COPY;
                        req.Username = LocalEnv.UserInfo.UserName;
                        TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                        if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                        {
                            CommonUtils.CopyInfoJson = reply.Datas.tbookSystemParameter.param_value;
                        }

                        SystemParameterReq req_sale = new SystemParameterReq();
                        req_sale.ParamKey = CommonUtils.STC_SALE_INTENTION;
                        req_sale.Username = LocalEnv.UserInfo.UserName;
                        TbookSystemParameterReplyPo reply_sale = AdminHandler.Instance.querySystemParameter(req_sale);
                        if (reply_sale != null && reply_sale.IsSuccess() && reply_sale.Datas != null && reply_sale.Datas.tbookSystemParameter != null && JsonSplit.IsJson(reply_sale.Datas.tbookSystemParameter.param_value))
                        {
                            CommonUtils.SaleInfoJson = reply_sale.Datas.tbookSystemParameter.param_value;
                        }
                        //拉取设置中发布名单
                        CommonUtils.loadInnerSetingBroker();
                    }
                }
            }

            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission || (CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions != "1"))
                return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_isSubSetting)
                {
                    if (_isIntentionManagement == false)
                    {
                        InitSetting();
                    }
                    _isSubSetting = true;
                }

                ThreadPoolHelper.InvokeThreadPool(o =>
                {
                    //CommonUtils.QueryAutoSendQQParam();

                    if (_isIntentionManagement == false)
                    {
                        _isIntentionManagement = true;

                        SystemParameterReq req = new SystemParameterReq();
                        req.ParamKey = CommonUtils.PARAM_KEY_COPY;
                        req.Username = LocalEnv.UserInfo.UserName;
                        TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                        if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                        {
                            CommonUtils.CopyInfoJson = reply.Datas.tbookSystemParameter.param_value;
                        }

                        SystemParameterReq req_sale = new SystemParameterReq();
                        req_sale.ParamKey = CommonUtils.STC_SALE_INTENTION;
                        req_sale.Username = LocalEnv.UserInfo.UserName;
                        TbookSystemParameterReplyPo reply_sale = AdminHandler.Instance.querySystemParameter(req_sale);
                        if (reply_sale != null && reply_sale.IsSuccess() && reply_sale.Datas != null && reply_sale.Datas.tbookSystemParameter != null && JsonSplit.IsJson(reply_sale.Datas.tbookSystemParameter.param_value))
                        {
                            CommonUtils.SaleInfoJson = reply_sale.Datas.tbookSystemParameter.param_value;
                        }
                        //拉取设置中发布名单
                        CommonUtils.loadInnerSetingBroker();
                    }
                });

                CommonUtils.isOnlyRate = AdminHandler.Instance.CheckPermission(CommonUtils.MODULEID_BONDSUBTYPE, CommonUtils.RATE);
                CommonUtils.isOnlyCredit = AdminHandler.Instance.CheckPermission(CommonUtils.MODULEID_BONDSUBTYPE, CommonUtils.CREDIT);
                //第一次菜单加载
                if (_isFirst)
                {
                    if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                    {
                        AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_neibukanban.png", Description, PluginKey, "3");
                    }
                    else
                    {
                        AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_neibukanban.png", Description, PluginKey, "3");
                    }

                    _isFirst = false;
                    enabled = false;

                    //注册全局快捷键
                    RegistGlobalHotKey();

                    return;
                }


                this.broadcast("loadCRM", string.Empty);
                this.broadcast("loadTags", string.Empty);


                DesignHandler.Instance.OnHotKey += Instance_OnHotKey;
                TabPageHelper.OpenTabPage(this.Description, new InnerBoardView(){ DataContext = new InnerBoardViewModel() }, PluginKey);

                Application.Subscribe("STC_Clear", this);

                //订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

                base.load();

                //接收数据
                try
                {
                    IntentionOldHandler.Instance.initInnerBoard();
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
            });
        }

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        private void RegistGlobalHotKey()
        {
            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    //添加快速打开快捷键
                    break;
                default:
                    //添加快速打开快捷键
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "内部看板", FuncName = "打开内部看板", KeyName = "F10", IsGlobal = true, Name = EHotKeySetting.打开内部看板.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F10 });
                    break;
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
                    TabItem saleTabItem = new TabItem();
                    SalesSettingView salesSettingView = new SalesSettingView();
                    saleTabItem.Content = salesSettingView;
                    saleTabItem.Header = "需求管理";
                    settingView.AddTabPage(saleTabItem);

                    bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
                    if (!isPermission || (CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions != "1"))
                        return;

                    TabItem userTabItem = new TabItem();
                    InnerSettingView view = new InnerSettingView();
                    userTabItem.Content = view;
                    userTabItem.Header = "内部看板";
                    settingView.AddTabPage(userTabItem);
                }
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
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty, orderid = string.Empty, securityID = string.Empty });
                if (jsonObj.marketindicator != "4")
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

                    pocCopy(groupid, jsonObj.orderid, jsonObj.securityID, obj.Info1 == "1");
                });
            }
            else if (obj.Code == "TableSvr_FOCUS_TAG" && !string.IsNullOrEmpty(obj.Msg))
            {
                //设置内部看板焦点
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                string groupid = msgPo.groupId;
                TMessageRemark remark = JsonConvert.DeserializeObject<TMessageRemark>(msgPo.remark);
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty, orderid = string.Empty });
                if (jsonObj.marketindicator != "4")
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
                        pocBond(remark.focusBond, jsonObj.orderid);
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
                            if (otm.IsFromIRSorInner.ToLower() == "irs")
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
            else if (obj.Code == "TableSvr_Focus_InnerHistoryRequireView")
            {
                //点击右下角提醒中的 查看按钮 自动选择历史作废区
                //作废区查询权限
                bool rootRight = AdminHandler.Instance.CheckPermisionById("5000", "5004");
                bool noRight = AdminHandler.Instance.CheckPermission("5004", "500401");
                Visibility IsShowIHR = rootRight ? noRight ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;
                
                if (IsShowIHR == Visibility.Collapsed)
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

                    TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

                    if (tabItem.Content is InnerBoardView)
                    {
                        InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                        innerBoardView.historyreqTab.IsSelected = true;
                    }
                });
            }
            else if (obj.Code == "FQ_Copy")
            {
                TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

                if (tabItem.Content is InnerBoardView)
                {
                    InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;

                    string copyMode = "1";
                    if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                       && CommonUtils.CopyDic["COPYMODE"].Count > 0
                       && CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
                    {
                        copyMode = "2";
                    }
                    else if (CommonUtils.CopyDic.ContainsKey("COPYMODE")
                       && CommonUtils.CopyDic["COPYMODE"].Count > 0
                       && CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
                    {
                        copyMode = "3";
                    }

                    if (innerBoardView.singleTab.IsSelected == true)
                    {
                        innerBoardView.InnerBoard_dataGrid.CopyData_Single(false, copyMode);
                    }
                    else if (innerBoardView.bestTab.IsSelected == true)
                    {
                        innerBoardView.InnerBoard_Best_dataGrid.CopyData_Best(false, copyMode);
                    }
                }
            }
        }

        private void pocCopy(string groupid, string orderid, string securityID, bool needPrompt = true)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is InnerBoardView)
            {
                InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                innerBoardView.singleTab.IsSelected = true;
                if (innerBoardView.DataContext is InnerBoardViewModel)
                {
                    InnerBoardViewModel vm = innerBoardView.DataContext as InnerBoardViewModel;
                    InnerOrderData record = vm.SelectedRowBySecurtiyID_Single_Copy(groupid, orderid, securityID);

                    if (record != null)
                    {
                        if (innerBoardView.InnerBoard_dataGrid.Items.Count > 0)
                        {
                            innerBoardView.InnerBoard_dataGrid.ScrollIntoView(record);
                        }

                        innerBoardView.InnerBoard_dataGrid.CopyData_Single(needPrompt);
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
                    if (innerBoardView.InnerBoard_dataGrid.Items.Count > 0)
                    {
                        innerBoardView.InnerBoard_dataGrid.ScrollIntoView(innerBoardView.InnerBoard_dataGrid.Items[0]);
                    }
                    if (innerBoardView.InnerBoard_Best_dataGrid.Items.Count > 0)
                    {
                        innerBoardView.InnerBoard_Best_dataGrid.ScrollIntoView(innerBoardView.InnerBoard_Best_dataGrid.Items[0]);
                    }
                }
            }
        }

        private void pocBond(string securityID, string orderID)
        {
            TabItem tabItem =  (TabItem)AppUtils.Application.Panel.SelectedItem;

            if(tabItem.Content is InnerBoardView)
            {
                InnerBoardView innerBoardView = tabItem.Content as InnerBoardView;
                innerBoardView.bestTab.IsSelected = true;
                if (innerBoardView.DataContext is InnerBoardViewModel)
                {
                    InnerBoardViewModel vm = innerBoardView.DataContext as InnerBoardViewModel;
                    InnerOrderData record = vm.SelectedRowBySecurtiyID_Best(securityID, orderID);

                    if (record != null)
                    {
                        if (innerBoardView.InnerBoard_Best_dataGrid.Items.Count > 0)
                        {
                            innerBoardView.InnerBoard_Best_dataGrid.ScrollIntoView(record);
                        }
                    }
                }
            }
        }
        #endregion
    }
}