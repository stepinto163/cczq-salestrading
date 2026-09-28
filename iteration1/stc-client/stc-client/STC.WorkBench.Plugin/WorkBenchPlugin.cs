using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Setting;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.WorkBench.Plugin.Utils;
using STC.WorkBench.Plugin.Utils.Data;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.WorkBench.Plugin
{
    public class WorkBenchPlugin : AbstractPlugin
    {
        #region 插件信息
        private int _tag = 15;
        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain
        {
            get{ return isMain; }
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
        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.WorkBench; }
        }

        public override string PluginName
        {
            get { return "需求工作台插件"; }
        }

        public override string Description
        {
            get { return "需求工作台"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }
        #endregion

        #region load,unload

        //待定好补上
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4008";

        private bool _isFirst = true;

        private bool _isSubSetting = false;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission || (Common.Utils.CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions != "1"))
                return;
            //STC.Common.Model.Admin.Module module = AdminHandler.Instance.GetModuleById("1001");
            //if (module == null)
            //{
            //    return;
            //}

            //ThreadPoolHelper.InvokeThreadPool(o =>
            //{
            //    STC.Common.Utils.CommonUtils.QueryAutoSendQQParam();
            //});

            STC.Common.Utils.CommonUtils.isOnlyRate = AdminHandler.Instance.CheckPermission(STC.Common.Utils.CommonUtils.MODULEID_BONDSUBTYPE, STC.Common.Utils.CommonUtils.RATE);
            STC.Common.Utils.CommonUtils.isOnlyCredit = AdminHandler.Instance.CheckPermission(STC.Common.Utils.CommonUtils.MODULEID_BONDSUBTYPE, STC.Common.Utils.CommonUtils.CREDIT);

            if (!_isSubSetting)
            {
                InitSetting();
                _isSubSetting = true;
            }

            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = "STC_Intention_DecimalPlaces";
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                    {
                        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(reply.Datas.tbookSystemParameter.param_value);
                        if (dict.ContainsKey("CREDIT"))
                        {
                            STC.Common.Utils.CommonUtils.CreditDP = string.IsNullOrEmpty(dict["CREDIT"]) ? "1" : dict["CREDIT"];
                        }
                        if (dict.ContainsKey("RATE"))
                        {
                            STC.Common.Utils.CommonUtils.RateDP = string.IsNullOrEmpty(dict["RATE"]) ? "1" : dict["RATE"];
                        }
                    }
                }
            });

            //第一次菜单加载
            if (_isFirst)
            {
                _isFirst = false;

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_jiaoyibao.png", Description, PluginKey, "7");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_jiaoyibao.png", Description, PluginKey, "7");
                }

                //内部看板批量识别
                DesignHandler.Instance.OnShowRecognizeIntentionQuoteWin += Instance_OnShowRecognizeIntentionQuoteWin;
                //IRS看板批量识别
                DesignHandler.Instance.OnShowIRSRecognizeIntentionQuoteWin += Instance_OnShowIRSRecognizeIntentionQuoteWin;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                WorkBenchHandler.Instance.RegSystemReadyEvent();

                //注册全局快捷键
                RegistGlobalHotKey();
                DesignHandler.Instance.OnHotKey += Instance_OnHotKey;
                return;
            }


            this.broadcast("loadCRM", string.Empty);
            this.broadcast("loadTags", string.Empty);

            LoadConfig.LoadBond = true;
            
            TabPageHelper.OpenTabPage(this.Description, new WorkTableView(), PluginKey);

            Application.Subscribe("STC_Clear", this);

            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);

            base.load();

            //接收数据
            try
            {
                IntentionOldHandler.Instance.init();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void InitSetting()
        {
            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
        }

        private void Instance_ShowDesign(object obj)
        {
            //if (obj != null)
            //{
            //    SettingView settingView = obj as SettingView;
            //    if (settingView != null)
            //    {
            //        TabItem userTabItem = new TabItem();
            //        InnerSettingView view = new InnerSettingView();
            //        userTabItem.Content = view;
            //        userTabItem.Header = "需求工作台";
            //        settingView.AddTabPage(userTabItem);
            //    }
            //}
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
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "需求工作台", FuncName = "打开需求工作台", KeyName = "F9", IsGlobal = true, Name = EHotKeySetting.打开需求工作台.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F9 });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "需求工作台", FuncName = "需求工作台普通确认", KeyName = "F1", IsGlobal = true, Name = EHotKeySetting.需求工作台普通确认.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F1 });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "需求工作台", FuncName = "需求工作台闪电确认", KeyName = "F2", IsGlobal = true, Name = EHotKeySetting.需求工作台闪电确认.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F2 });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "需求工作台", FuncName = "需求工作台TG", KeyName = "F3", IsGlobal = true, Name = EHotKeySetting.需求工作台TG.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F3 });
                    (AppUtils.Application as STC.Frame.Window1).DictHotKey.Add(new HotKeyModel { ModelName = "需求工作台", FuncName = "需求工作台忽略", KeyName = "F4", IsGlobal = true, Name = EHotKeySetting.需求工作台忽略.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = false, IsSelectShift = false, SelectKey = EKey.F4 });
                    break;
            }
        }

        private void Instance_OnHotKey(EHotKeySetting obj)
        {
            if (obj == EHotKeySetting.打开需求工作台)
            {
                if (TabPageHelper.IsExistTabItem(this.Description))
                {
                    return;
                }
                TabPageHelper.OpenTabPage(this.Description, new WorkTableView(), PluginKey);
            }
        }

        #region 批量识别
        /// <summary>
        /// 内部看板批量识别
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnShowRecognizeIntentionQuoteWin(object obj, List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> positionList)
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.RECOGENIZEQUOTE))
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    RecognizeIntentionQuoteWin_PA win = new RecognizeIntentionQuoteWin_PA(obj, positionList);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
                else
                {
                    RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin(obj);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
                }
            }
        }

        /// <summary>
        /// IRS看板批量识别
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnShowIRSRecognizeIntentionQuoteWin(object obj)
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.IRSRECOGENIZEQUOTE))
            {
                IRSRecognizeIntentionQuoteWin win = new IRSRecognizeIntentionQuoteWin(obj);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.IRSRECOGENIZEQUOTE, win);
            }
        }

        #endregion

        protected override void unload()
        {
            base.unload();
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

                if (key.Contains("STC.WorkBench.Plugin"))
                {
                    w.Close();
                }
            }
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

            if (obj.Code == "TableSvr_FOCUS_TAG_RW_COPY" && !string.IsNullOrEmpty(obj.Msg))
            {
                //设置需求工作台焦点
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                string groupid = string.Empty;
                TMessageRemark remark = JsonConvert.DeserializeObject<TMessageRemark>(msgPo.remark);
                if (remark != null && remark.focusType?.ToUpper() == "TAG")
                {
                    groupid = msgPo.groupId;
                }
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty, intentionId = string.Empty, securityID = string.Empty });


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                    {
                        System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    }
                    System.Windows.Application.Current.MainWindow.Activate();

                    if (!TabPageHelper.IsExistTabItem(this.Description))
                    {
                        TabPageHelper.OpenTabPage(this.Description, new MyOrderDefaultView(), PluginKey);
                    }

                    pocCopy(groupid, jsonObj.intentionId, jsonObj.securityID, obj.Info1 == "1");
                });
            }
            else if (obj.Code == "TableSvr_FOCUS_TAG_RW" && !string.IsNullOrEmpty(obj.Msg))
            {
                //设置内部看板焦点
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);
                string groupid = msgPo.groupId;
                TMessageRemark remark = JsonConvert.DeserializeObject<TMessageRemark>(msgPo.remark);
                var jsonObj = JsonConvert.DeserializeAnonymousType(msgPo.detail, new { marketindicator = string.Empty, intentionId = string.Empty });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {

                    if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                    {
                        System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    }
                    System.Windows.Application.Current.MainWindow.Activate();

                    if (!TabPageHelper.IsExistTabItem(this.Description))
                    {
                        TabPageHelper.OpenTabPage(this.Description, new MyOrderDefaultView(), PluginKey);
                    }

                    if (remark != null)
                    {
                        pocBond(remark.focusBond, jsonObj.intentionId, groupid);
                    }
                    //if (remark != null && remark.focusType == "BOND")
                    //{
                    //    pocBond(remark.focusBond, jsonObj.intentionId);
                    //}
                    //else
                    //{
                    //    if (!string.IsNullOrEmpty(groupid))
                    //    {
                    //        pocTag(groupid);
                    //    }
                    //}
                });
            }
            else if (obj.Code == "FQ_Copy")
            {
                TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

                if (tabItem.Content is WorkTableView)
                {
                    WorkTableView workTableView = tabItem.Content as WorkTableView;

                    string copyMode = "1";
                    if (Common.Utils.CommonUtils.CopyDic.ContainsKey("COPYMODE")
                        && Common.Utils.CommonUtils.CopyDic["COPYMODE"].Count > 0
                        && Common.Utils.CommonUtils.CopyDic["COPYMODE"][0].Value == "2")
                    {
                        copyMode = "2";
                    }
                    if (Common.Utils.CommonUtils.CopyDic.ContainsKey("COPYMODE")
                        && Common.Utils.CommonUtils.CopyDic["COPYMODE"].Count > 0
                        && Common.Utils.CommonUtils.CopyDic["COPYMODE"][0].Value == "3")
                    {
                        copyMode = "3";
                    }

                    workTableView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().CopyData_Single(false, copyMode);
                }
            }
            else if (obj.Code == "TableSvr_Inquiry_Fail")
            {
                TMessageContent msgPo = JsonConvert.DeserializeObject<TMessageContent>(obj.Msg);

                TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

                if (tabItem.Content != null && tabItem.Content is WorkTableView)
                {
                    WorkTableView workTableView = tabItem.Content as WorkTableView;
                    MyOrderDefaultViewModel vm = workTableView.MyOrderDefaultView.DataContext as MyOrderDefaultViewModel;
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (System.Windows.Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                        {
                            System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                        }
                        System.Windows.Application.Current.MainWindow.Activate();

                        if (!TabPageHelper.IsExistTabItem(this.Description))
                        {
                            TabPageHelper.OpenTabPage(this.Description, new MyOrderDefaultView(), PluginKey);
                        }

                        workTableView.MyOrderDefaultView.InquiryTab.IsSelected = true;

                        Common.Model.Table.TableIntentionInquiry inquiryInfo = IntentionOldHandler.Instance.GetInquiryById(msgPo.inquiryid);

                        if (inquiryInfo != null)
                        {
                            pocBond(inquiryInfo.securityId, inquiryInfo.intentionId, null);

                            IntentionMarketItem item = vm.GetIntentionMarketItemById(msgPo.inquiryid);

                            if (item != null)
                            {
                                workTableView.MyOrderDefaultView.inquiryDg.ScrollIntoView(item);
                                workTableView.MyOrderDefaultView.inquiryDg.SelectedItem = item;
                            }
                        }
                        
                    });

                   
                }
            }
        }

        private void pocCopy(string groupid, string orderid, string securityID, bool needPrompt = true)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is WorkTableView)
            {
                WorkTableView innerBoardView = tabItem.Content as WorkTableView;
                if (innerBoardView.MyOrderDefaultView.DataContext is MyOrderDefaultViewModel)
                {
                    MyOrderDefaultViewModel vm = innerBoardView.MyOrderDefaultView.DataContext as MyOrderDefaultViewModel;
                    IntentionQuoteItem record = vm.SelectedRowBySecurtiyID_Single_Copy(groupid, orderid, securityID);

                    if (record != null)
                    {
                        if (innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().Items.Count > 0)
                        {
                            innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().SelectedItem = record;
                            innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().ScrollIntoView(record);
                        }
                        if (innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().SelectedItems.Count > 0)
                        {
                            innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().CopyData_Single(needPrompt);
                        }
                        else
                        {
                            STC.Common.Utils.CommonUtils.Prompt2Second("订阅复制", "可能有其他筛选，未找到对应数据");
                        }
                    }
                    else
                    {
                        STC.Common.Utils.CommonUtils.Prompt2Second("订阅复制", "无可处理数据");
                    }
                }
            }
        }

        private void pocTag(string groupid)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is WorkTableView)
            {
                WorkTableView innerBoardView = tabItem.Content as WorkTableView;
                if (innerBoardView.MyOrderDefaultView.DataContext is MyOrderDefaultViewModel)
                {
                    MyOrderDefaultViewModel vm = innerBoardView.MyOrderDefaultView.DataContext as MyOrderDefaultViewModel;
                    vm.SelectedGroup(groupid);
                    if (innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().Items.Count > 0)
                    {
                        innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().SelectedItem = innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().Items[0];
                        innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().ScrollIntoView(innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().Items[0]);
                    }
                }
            }
        }

        private void pocBond(string securityID, string orderID, string groupid)
        {
            TabItem tabItem = (TabItem)AppUtils.Application.Panel.SelectedItem;

            if (tabItem.Content is WorkTableView)
            {
                WorkTableView innerBoardView = tabItem.Content as WorkTableView;
                if (innerBoardView.MyOrderDefaultView.DataContext is MyOrderDefaultViewModel)
                {
                    MyOrderDefaultViewModel vm = innerBoardView.MyOrderDefaultView.DataContext as MyOrderDefaultViewModel;
                    if (!string.IsNullOrEmpty(groupid))
                    {
                        vm.SelectedGroup(groupid);
                    }
                    IntentionQuoteItem record = vm.SelectedRowBySecurtiyID_Best(securityID, orderID);

                    if (record != null)
                    {
                        innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().SelectedItem = record; 
                        innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().ScrollIntoView(record);
                    }

                    if (innerBoardView.MyOrderDefaultView.IntentionPart.GetCustomDefaultDataGrid().SelectedItems.Count == 0)
                    {
                        STC.Common.Utils.CommonUtils.Prompt2Second("订阅处理", "可能有其他筛选，未找到对应数据");
                    }
                }
            }
        }
        #endregion
    }
}
