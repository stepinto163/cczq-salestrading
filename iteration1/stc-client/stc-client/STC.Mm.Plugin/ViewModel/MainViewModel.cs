using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using System.Data;
using System.Windows.Input;
using STC.Mm.Plugin.Utils;
using System.Windows;
using STC.Common.Model.MM;
using STC.Mm.Plugin.View.Component;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Collections.Concurrent;
using STC.Mm.Plugin.View;
using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Model.Ans;
using STC.Mm.Plugin.Utils.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using UserSetting = STC.Common.Model.MM.UserSetting;
using System.IO;
using STC.Mm.Plugin.Command;
using STC.Common.View.UI;
using System.Threading;
using System.Windows.Media;
using STC.Mm.Plugin.View.HomePage;
using STC.Common.View.Component;
using System.Linq;
using System.Timers;
using Newtonsoft.Json;
using STC.Common.Model.Drp;
using STC.Frame.ViewModel;
using STC.Frame.UI;
using AddBondWindow = STC.Mm.Plugin.View.HomePage.AddBondWindow;

namespace STC.Mm.Plugin.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainView parentView;
        private DispatcherTimer AnsCheckTimer;//做市义务定时检查
        private DispatcherTimer AnsNormalWarnTimer; //普通提醒2s一次
        private DispatcherTimer AnsStrongWarnTimer; //强提醒1s一次
        private DispatcherTimer VoiceWarnTimer; //声音提醒10s一次
        public static bool Tip1Enable = true; //	09:25前未达标提醒
        public static bool Tip2Enable = true; //	09:25-09:30未达标提醒
        public static bool Tip4Enable = true; //	09:29声音提醒
        public static string BncMoniValue; // 利率债最优点差监控
        public static string BcoMoniValue; // 信用债最优点差监控

        private const int StartHour = 9;
        private const int StrongMinute = 26;
        private const int VoiceMinute = 29;
        private const int StopMinute = 30;
        private static volatile bool needVoicePlay = false;

        private ConcurrentDictionary<string, string> _SavedOrderControlDic = new ConcurrentDictionary<string, string>();
        WindowManager windowManager = WindowManager.Instance;

        private ConcurrentDictionary<string, MMListBoxViewModel> mmListBoxVMdic = new ConcurrentDictionary<string, MMListBoxViewModel>();

        public ConcurrentDictionary<string, MMListBoxViewModel> getMMListBoxViewModel() { return mmListBoxVMdic; }
        public ICommand UpdateWidenSettingCommand { get; set; }

        public ICommand ImportGroupCommand { get; set; }

        public ICommand UpdateCancelWidenSettingCommand { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand AddBondCommand { get; set; }

        private Thickness _AnsWarnBH = new Thickness(0);
        public Thickness AnsWarnBH
        {
            get { return _AnsWarnBH; }
            set
            {
                _AnsWarnBH = value;
                OnPropertyChanged();
            }
        }
        private ANSSummaryProp _currentANSSummary = new ANSSummaryProp();
        public ANSSummaryProp CurrentANSSummary
        {
            get { return _currentANSSummary; }
            set
            {
                _currentANSSummary = value;
                OnPropertyChanged();
            }
        }

        private Visibility _IsShowGridSplitter = Visibility.Collapsed;

        public Visibility IsShowGridSplitter
        {
            get
            {
                return this._IsShowGridSplitter;
            }

            set
            {
                this._IsShowGridSplitter = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _IsShowMMOrderControl = Visibility.Collapsed;

        public Visibility IsShowMMOrderControl
        {
            get
            {
                return this._IsShowMMOrderControl;
            }

            set
            {
                this._IsShowMMOrderControl = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _MaskVisibility = Visibility.Collapsed;

        public Visibility MaskVisibility
        {
            get
            {
                return this._MaskVisibility;
            }

            set
            {
                this._MaskVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    FilterData();
                }
            }
        }

        public string _WidenBtnText = "拉宽所有报价";

        public string WidenBtnText
        {
            get
            {
                return _WidenBtnText;
            }

            set
            {
                if (_WidenBtnText != value)
                {
                    _WidenBtnText = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private Visibility _LockVisibility = Visibility.Collapsed;

        public Visibility LockVisibility
        {
            get
            {
                return this._LockVisibility;
            }

            set
            {
                this._LockVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private bool _BtnEnable = true;

        public bool BtnEnable
        {
            get
            {
                return _BtnEnable;
            }
            set
            {
                this._BtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _WidenQuoteBtnEnabled = true;

        public bool WidenQuoteBtnEnabled
        {
            get
            {
                return _WidenQuoteBtnEnabled;
            }
            set
            {
                this._WidenQuoteBtnEnabled = value;
                this.OnPropertyChanged();
            }
        }


        private bool _HasGroup = false;

        public bool HasGroup
        {
            get
            {
                return _HasGroup;
            }
            set
            {
                this._HasGroup = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGrid;

        public Visibility IsHiddenMyOrdersOperationGrid
        {
            get
            {
                return isHiddenMyOrdersOperationGrid;
            }

            set
            {
                if (isHiddenMyOrdersOperationGrid != value)
                {
                    isHiddenMyOrdersOperationGrid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "政策性金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList { get { return _interestRateDebtList; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "政府支持机构债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList { get { return _creditBondList; } }

        private ObservableCollection<FilterListBoxItemData> _policyFinancialBondList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "农发", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "口行", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "国开", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> PolicyFinancialBondList { get { return _policyFinancialBondList; } }

        private ObservableCollection<FilterListBoxItemData> _remaingingTimeList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "0~1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "1~3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3~5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5~7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7~10Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = ">10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RemaingingTimeList { get { return _remaingingTimeList; } }

        private ObservableCollection<FilterListBoxItemData> _NewBondSignList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "2Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> NewBondSignList { get { return _NewBondSignList; } }

        private ObservableCollection<FilterListBoxItemData> _ratingList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "AAA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AAA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA-", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "A+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RatingList { get { return _ratingList; } }

        private ObservableCollection<FilterListBoxItemData> _settlTypeList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "T+0", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "T+1", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> SettlTypeList { get { return _settlTypeList; } }

        private ObservableCollection<FilterListBoxItemData> _enableList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "开启", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "关闭", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> EnableList { get { return _enableList; } }


        private ObservableCollection<FilterListBoxItemData> _monitorStatusList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "已达标", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "可达标", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "危险", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "失败", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> MonitorStatusList { get { return _monitorStatusList; } }

        public MMGroupInfo SelectGroup { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel()
        {
            this.UpdateWidenSettingCommand = new RelayCommand(this.UpdateWidenSetting);
            this.ImportGroupCommand = new RelayCommand(this.ImportGroup);
            this.UpdateCancelWidenSettingCommand = new RelayCommand(this.UpdateCancelWidenSetting);
            this.ExportCommand = new RelayCommand(ExportExcel);
            this.ImportCommand = new RelayCommand(ImportExcel);
            this.AddBondCommand = new RelayCommand(AddBond);
            Utils.Data.MMGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            MMHandler.Instance.SubscribeMMSysStatus(this.OnMMSysStatus, new List<string> { LocalEnv.UserInfo.UserName });

            AnsDataHandler.Instance.OnAnsSummary += OnSubscribeAnsSummary;

            AnsCheckTimer = new DispatcherTimer();
            AnsCheckTimer.Tick += AnsCheckTimer_Tick;
            AnsCheckTimer.Interval = TimeSpan.FromSeconds(1);
            AnsCheckTimer.Start();

            AnsNormalWarnTimer = new DispatcherTimer();
            AnsNormalWarnTimer.Tick += AnsNormalWarnTimer_Tick;
            AnsNormalWarnTimer.Interval = TimeSpan.FromSeconds(2);

            AnsStrongWarnTimer = new DispatcherTimer();
            AnsStrongWarnTimer.Tick += AnsStrongWarnTimer_Tick; ;
            AnsStrongWarnTimer.Interval = TimeSpan.FromSeconds(1);

            VoiceWarnTimer = new DispatcherTimer();
            VoiceWarnTimer.Tick += VoiceWarnTimer_Tick;
            VoiceWarnTimer.Interval = TimeSpan.FromSeconds(10);

            ThreadPoolHelper.InvokeThreadPool(o => {
                GetMmUserSetting();

                ReadHistoryData_OrderControl();
                SystemMessageReq dealReq = SetSystemMsgReq("2", "00:00:00", string.Empty, 0);
                TQueryMessage systemMessage = AdminHandler.Instance.SearchSysMsg(dealReq);
                if (systemMessage != null && systemMessage.Datas != null)
                {
                    OnAdminMsg(systemMessage.Datas);
                }
                mmQuerySingleBondSetting();
            });


            AdminHandler.Instance.Init(OnAdminMsg);
            WidenQuoteBtnEnabled = true;

            //MMCommonUtils.PlayAnsAlertWave();
            MMExcelExportUtils.Instance.init(this);
            if (AdminHandler.dicGroup.Count() <= 0) 
            {
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                if ("*".Equals(AdminHandler.Instance.getSecurityGroupUser()))
                {
                    request.username = "";
                }
                else
                {
                    request.username = LocalEnv.UserInfo.UserName;
                }
                HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(request);
            }
        }

        private void mmQuerySingleBondSetting()
        {
            MMBondParamsSettingReplyPo result = MMHandler.Instance.mmQuerySingleBondSetting();
            if (result == null)
                return;
            MMCommonUtils.updateBondParamsSetting(result.Datas);
        }

        private SystemMessageReq SetSystemMsgReq(string msgType, string startTime, string endTime, int pageNum)
        {
            SystemMessageReq req = new SystemMessageReq();
            req.type = msgType;
            req.username = LocalEnv.UserInfo.UserName;
            req.begintime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " " + (string.IsNullOrEmpty(startTime) ? "00:00:00" : startTime);
            req.endtime = DateTime.Now.ToString(AdminConsts.YYYYMMdd) + " " + (string.IsNullOrEmpty(endTime) ? "23:59:59" : endTime);
            req.pageSize = 1000;
            req.pageNo = pageNum;
            req.sort = "1";
            req.contentP1 = "\"contentType\":\"Ans\"";

            return req;
        }
        private void VoiceWarnTimer_Tick(object sender, EventArgs e)
        {
            PlayVoiceAlertWave();
        }

        public async void AddBond()
        {
            try
            {
                AddBondWindow addBondWind = new AddBondWindow();
                addBondWind.SelectGroup = SelectGroup;
                addBondWind.Owner = Application.Current.MainWindow;
                if (addBondWind.ShowDialog() == false)
                {
                    return;
                }
                String groupID = addBondWind.Groupid;
                OrderDetailView odView = new OrderDetailView();
                odView.Owner = Application.Current.MainWindow;
                OrderParamProp orderParam = new OrderParamProp(null);
                orderParam = MMCommonUtils.OrderParamPropToDefault(orderParam);
                orderParam.securityid = addBondWind.securityID;
                orderParam.symbol = addBondWind.symbol;
                PResult result = MMCommonUtils.QueryCirculSizeBySecurity(addBondWind.securityID);
                if (result.IsSuccess())
                {
                    orderParam.circulSize = result.Info1;
                }
                else
                {
                    return;
                }
                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(orderParam.securityid);

                if (bondsInfoPo == null)
                {
                    return;
                }
                if ("BNC".Equals(bondsInfoPo.symbolType))
                {
                    orderParam.IsInterest = "true";
                }
                else
                {
                    orderParam.IsInterest = "false";
                }
                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                {
                    orderParam.wallEnable = "0";
                    orderParam.CDCValueType = "1";
                    orderParam.exeBondFollow = "0";
                }
                //RiskQtyBySecurityReplyPo replyPo = MMCommonUtils.QueryRiskQtyBySecurity(addBondWind.securityID);
                //if (replyPo.IsSuccess() && replyPo.Data != null)
                //{
                //    orderParam.riskMinQtySize = replyPo.Data.riskMinQtySize;
                //    orderParam.riskMaxQtyRatio = replyPo.Data.riskMaxQtyRatio;
                //}
                //else
                //{
                //    return;
                //}
                OrderDetailViewModel odVM = new OrderDetailViewModel(orderParam);
                odVM.NeedPrompt = false;
                MMOrderControlModel vm = new MMOrderControlModel()
                {
                    enable = false,
                    securityID = addBondWind.securityID,
                    Params = orderParam
                };
                odVM.SelectedItemRow = vm;
                odVM.IsAddBond = true;

                odVM.MyWindow = odView;
                odView.DataContext = odVM;

                if (odView.ShowDialog() == true)
                {
                    MMReq updateReq = new MMReq();
                    updateReq.Action = "1"; // 添加
                    STC.Common.Model.MM.MM request = new STC.Common.Model.MM.MM();
                    request.securityid = addBondWind.securityID;
                    request.symbol = addBondWind.symbol;
                    request.Action = "1"; // 添加
                    request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                    request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    request.groupid = groupID;
                    request.settltype = odVM.OrderParamReq.baseSettleType;
                    request.Params = odVM.OrderParamReq;
                    request.enable = "0";
                    updateReq.mmList = new List<STC.Common.Model.MM.MM>();
                    updateReq.mmList.Add(request);
                    PResult pResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateSecurityids(updateReq);
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.PromptTitle = "新增债券";
                    prompt.Owner = Application.Current.MainWindow;
                    if (pResult != null && pResult.IsSuccess())
                    {
                        //CustomMessageBox.Info($"新增成功！", "新增债券");
                        prompt.PromptContext = "新增成功";
                    }
                    else
                    {
                        //CustomMessageBox.Warning(pResult == null ? "新增失败！" : pResult.Msg, "新增债券");
                        prompt.PromptContext = pResult == null ? "新增失败！" : pResult.Msg;
                    }
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
        private void ReadHistoryData_OrderControl()
        {
            this._SavedOrderControlDic.Clear();
            string lstkeys = IniFileHelper.ReadValue($"OrderControl-{LocalEnv.UserInfo.UserName}", "Keys");
            string[] keyList = lstkeys.Split(',');
            for (int i = 0; i < keyList.Length; i++)
            {
                string securityID = keyList[i];
                if (!string.IsNullOrEmpty(securityID))
                {
                    _SavedOrderControlDic.TryAdd(securityID, "0");
                    MMGroupHandler.Instance.RegisterMM(securityID, Instance_ONMM);
                }
            }
        }

        private void OpenAllSavedOrderControl()
        {
            if (_SavedOrderControlDic.Count > 0)
            {
                foreach (var item in _SavedOrderControlDic)
                {
                    OpenSavedOrderControl(item.Key.ToString());
                }
            }
        }

        private void OpenSavedOrderControl(string securityID)
        {
            if (string.IsNullOrEmpty(securityID)) return;

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (windowManager.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}") == false)
                {
                    NoBorderWin popOrderWin = new NoBorderWin(securityID);
                    popOrderWin.Closed += PopOrderWin_Closed;
                    popOrderWin.Title = securityID;
                    MMOrderControl orderControl = new MMOrderControl();
                    orderControl.securityID = securityID;
                    orderControl.popupBtn.Visibility = Visibility.Collapsed;
                    orderControl.btnFixFloatButton.Visibility = Visibility.Visible;
                    orderControl.btnShutDown.Visibility = Visibility.Visible;
                    popOrderWin.SetMainControl(orderControl);
                    popOrderWin.ResizeMode = ResizeMode.CanResize;
                    popOrderWin.Height = 550;
                    popOrderWin.Width = 300;
                    popOrderWin.MinWidth = 290;
                    popOrderWin.MaxWidth = 290;
                    popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                    popOrderWin.WindowStyle = WindowStyle.None;
                    popOrderWin.WindowState = WindowState.Normal;
                    popOrderWin.Owner = null;

                    popOrderWin.Show();
                    windowManager.OpenOnlyOneWindow($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}", popOrderWin);
                    _SavedOrderControlDic[securityID] = "1";
                }
            }));
        }

        private void PopOrderWin_Closed(object sender, EventArgs e)
        {
            if (sender != null && sender is NoBorderWin)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    NoBorderWin popupWindow = sender as NoBorderWin;
                    if (popupWindow != null)
                    {
                        if (popupWindow.Content is System.Windows.Controls.Frame
                            && (popupWindow.Content as System.Windows.Controls.Frame).Content is MMOrderControl)
                        {
                            MMOrderControl mmOrderControl = (popupWindow.Content as System.Windows.Controls.Frame).Content as MMOrderControl;
                            mmOrderControl.UnregistMM();
                        }
                    }
                }));               
            }
        }

        private void Instance_ONMM(STC.Common.Model.MM.MM obj)
        {
            if (obj != null && obj.Action != "3")
            {
                if (_SavedOrderControlDic.ContainsKey(obj.securityid) && ("0").Equals(_SavedOrderControlDic[obj.securityid]))
                {
                    OpenSavedOrderControl(obj.securityid);
                    MMGroupHandler.Instance.UnregisterMM(obj.securityid, Instance_ONMM);
                }
            }
            else
            {
                if (obj != null && obj.Action == "3")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (windowManager.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{obj.securityid}") && _SavedOrderControlDic[obj.securityid].ToString().Equals("0"))
                        {
                            windowManager.CloseWindow($"{PopupTypeEnum.OrderControl.ToString()}-{obj.securityid}");
                        }

                        MMGroupHandler.Instance.UnregisterMM(obj.securityid, Instance_ONMM);

                    }));
                }

                
            }
        }

        public MMBreakAlarmModel cacheAlarmModel = new MMBreakAlarmModel();
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    foreach (TTbookSystemMessage msg in sysMsg)
                    {
                        if (msg.content != null && msg.closeby != null)
                        {
                            if ("MM".Equals(msg.closeby.ToUpper()) || AdminConsts.Admin_MM.Equals(msg.closeby.ToUpper()))
                            {
                                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(msg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);

                                LogHandler.Info(string.Format("======MMOnAdminMsg====== HeaderTitle = {0} ContentType = {1}", detail.title, detail.contentType));

                                if (detail.title == "双边_策略断报" && detail.contentType == "双边")
                                {
                                    needPlay = true;
                                    InitAnsPlayerTimer();
                                    LogHandler.Info(string.Format("======MMOnAdminMsg.PlayAnsAlertWave======"));
                                }
                                if (detail.title == "双边待办" && detail.contentType.ToUpper() == AdminConsts.Admin_MM)
                                {
                                    MMBreakAlarmModel alarmModel = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(detail.detail);
                                    cacheAlarmModel = alarmModel.Copy();
                                    string delflag = msg.delflag ?? "0";
                                    if ("1".Equals(delflag) || alarmModel.infos.Count() == 0)
                                    {
                                        this.parentView.GroupTabControl.clearWarnNum();
                                    }
                                    else
                                    {
                                        this.parentView.GroupTabControl.refreshWarnNum(alarmModel);
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        private System.Timers.Timer _AnsPlayInterval;
        private static MediaPlayer player = null;
        private static string ansAlertPath = AppDomain.CurrentDomain.BaseDirectory + @"control\alarm80.wav";
        private static volatile bool isPlaying = false;
        private static volatile bool needPlay = false;

        private void InitAnsPlayerTimer()
        {
            if (_AnsPlayInterval == null)
            {
                _AnsPlayInterval = new System.Timers.Timer(1000 * 5);
                this._AnsPlayInterval.AutoReset = true;
                this._AnsPlayInterval.Elapsed += new ElapsedEventHandler(_interval_Elapsed);
            }
            if (_AnsPlayInterval.Enabled == false)
            {
                _AnsPlayInterval.Start();
            }
        }

        void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            PlayAnsAlertWave();
        }

        private void PlayAnsAlertWave()
        {
            if (File.Exists(ansAlertPath))
            {
                DoPlayAnsAlertWave(ansAlertPath);
            }
            else
            {
                LogHelper.Info("alarm80.wav文件不存在,无法播放报警！");
            }
        }

        private void DoPlayAnsAlertWave(string path)
        {
            InitAnsPlayerTimer();

            if (isPlaying == false && needPlay == true)
            {
                isPlaying = true;
                LogHandler.Info(string.Format("======DoPlayAnsAlertWave======"));
                player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Relative));
                player.Volume = 1;
                player.Play();
                Thread.Sleep(2000);
                player.Close();
                isPlaying = false;
                needPlay = false;
            }
        }

        private void PlayVoiceAlertWave()
        {
            if (File.Exists(ansAlertPath))
            {
                DoPlayVoiceAlertWave(ansAlertPath);
            }
            else
            {
                LogHelper.Info("alarm80.wav文件不存在,无法播放报警！");
            }
        }

        private void DoPlayVoiceAlertWave(string path)
        {
            //InitAnsPlayerTimer();

            if (isPlaying == false && needVoicePlay == true)
            {
                isPlaying = true;
                LogHandler.Info(string.Format("======DoPlayAnsAlertWave======"));
                player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Relative));
                player.Volume = 1;
                player.Play();
                Thread.Sleep(2000);
                player.Close();
                isPlaying = false;
            }
        }

        private void SetAnsTwinkle()
        {
            if (CurrentANSSummary.status == "2") //需关注
            {
                if (AnsWarnBH == new Thickness(0))
                {
                    AnsWarnBH = new Thickness(1);
                }
                else
                {
                    AnsWarnBH = new Thickness(0);
                }
            }
            else
            {
                AnsWarnBH = new Thickness(0);
            }
            
        }

        private void AnsStrongWarnTimer_Tick(object sender, EventArgs e)
        {
            LogHandler.Info("======= 强提醒 ========  {0}", DateTimeEx.ServerTime.ToString("HHmmss"));
            SetAnsTwinkle();
        }

        private void AnsNormalWarnTimer_Tick(object sender, EventArgs e)
        {
            LogHandler.Info("======= 普通提醒 ========  {0}", DateTimeEx.ServerTime.ToString("HHmmss"));
            SetAnsTwinkle();
        }

        private void AnsCheckTimer_Tick(object sender, EventArgs e)
        {            
            int hour = int.Parse(DateTimeEx.ServerTime.ToString("HH"));
            int minute = int.Parse(DateTimeEx.ServerTime.ToString("mm"));

            if (hour == StartHour && minute < StrongMinute)
            {
                //普通提醒 2s闪一次
                AnsStrongWarnTimer.Stop();
                if (Tip1Enable)
                { AnsNormalWarnTimer.Start(); }
                else
                { AnsNormalWarnTimer.Stop(); AnsWarnBH = new Thickness(0); }
            }
            else if (hour == StartHour && minute >= StrongMinute && minute < StopMinute)
            {
                //声音提醒10s一次
                if (hour == StartHour && minute >= VoiceMinute && minute < StopMinute)
                {
                    if (Tip4Enable)
                    {
                        needVoicePlay = true; 
                        VoiceWarnTimer.Start(); }
                    else
                    { 
                        VoiceWarnTimer.Stop();
                        needVoicePlay = false;
                    }
                }
                //强提醒 1s闪一次
                AnsNormalWarnTimer.Stop();
                if (Tip2Enable)
                { AnsStrongWarnTimer.Start(); }
                else
                { AnsStrongWarnTimer.Stop(); AnsWarnBH = new Thickness(0); }
            }
            else if ((hour == StartHour && minute >= StopMinute) || hour > StartHour )
            {
                StopAllCheckTimer();
            }
        }

        private void OnMMSysStatus(MMSysStatus obj)
        {
            WidenQuoteBtnEnabled = true;
            if ("0" == obj.status)
            {
                WidenBtnText = "恢复所有报价";
                LockVisibility = Visibility.Visible;
                BtnEnable = false;
            }
            else
            {
                WidenBtnText = "拉宽所有报价";
                LockVisibility = Visibility.Collapsed;
                BtnEnable = true;
            }
        }

        public void GroupInit(string groupID) 
        {
            if (mmListBoxVMdic.ContainsKey(groupID) == false)
            {
                MMListBoxViewModel mMListBoxViewModel = new MMListBoxViewModel(groupID);
                mmListBoxVMdic[groupID] = mMListBoxViewModel;
                HasGroup = mmListBoxVMdic.Count > 0;
            }
        }
        private void HomePageDataHandler_OnFavoritesChanged(Utils.Data.MMGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securtityID)
        {
            if (favoriteTypeEnum == Utils.Data.MMGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                LogHandler.Info($"1.Add mm group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID) == false)
                {
                    LogHandler.Info($"2.Add mm group:[{groupID}]");
                    MMListBoxViewModel mMListBoxViewModel = new MMListBoxViewModel(groupID);
                    mmListBoxVMdic[groupID] = mMListBoxViewModel;
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
            }
            else if (favoriteTypeEnum == Utils.Data.MMGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                LogHandler.Info($"1.Del mm group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID))
                {
                    LogHandler.Info($"2.Del mm group:[{groupID}]");
                    MMListBoxViewModel mMListBoxViewModel;
                    mmListBoxVMdic.TryRemove(groupID, out mMListBoxViewModel);
                    mMListBoxViewModel.Dispose();
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
            }
        }

        public MMListBoxViewModel GetMMListBoxViewModelByGroupID(string groupid)
        {
            if (mmListBoxVMdic.ContainsKey(groupid) == false)
            {
                MMListBoxViewModel mMListBoxViewModel = new MMListBoxViewModel(groupid);
                mmListBoxVMdic[groupid] = mMListBoxViewModel;
            }
            if (mmListBoxVMdic.ContainsKey(groupid))
            {
                return mmListBoxVMdic[groupid];
            }
            HasGroup = mmListBoxVMdic.Count > 0;
            return null;
        }

        public void SetActiveMMListBox(string mainGuid, string groupid)
        {
            foreach (MMListBoxViewModel vm in mmListBoxVMdic.Values)
            {
                if (vm.groupID == groupid)
                {
                    if (vm.mainWinList.ContainsKey(mainGuid) == false)
                    {
                        vm.mainWinList[mainGuid] = mainGuid;
                    }
                }
                else
                {
                    string removeValue;
                    bool flag = vm.mainWinList.TryRemove(mainGuid, out removeValue);
                }
            }
        }

        //private void InitCurrentANSSummaryTestData()
        //{
        //    CurrentANSSummary.status = "2";
        //    CurrentANSSummary.checkTime = "16:23";
        //    CurrentANSSummary.remainTime = "3h 23min";
        //    CurrentANSSummary.bncSecurityCount = "120/40";
        //    CurrentANSSummary.bncSecurityTypeCount = "3/2";
        //    CurrentANSSummary.bncTimetomaturityCount = "4/5";
        //    CurrentANSSummary.bncNDCount = "29/10";
        //    CurrentANSSummary.bncNewNDCount = "3/5";
        //    CurrentANSSummary.bcoSecurityCount = "120/40";
        //    CurrentANSSummary.bcoSecurityTypeCount = "3/2";
        //    CurrentANSSummary.bcoIssuerCount = "9/10";
        //    CurrentANSSummary.securityTermLt = new List<SecurityTermProp>();
        //    SecurityTermProp newBond = new SecurityTermProp();
        //    newBond.securityDec = "新发国债";
        //    newBond.countSafe = "22";
        //    newBond.termCountList = new List<int>();
        //    newBond.termCountList.Add(1);
        //    newBond.termCountList.Add(2);
        //    newBond.termCountList.Add(3);
        //    newBond.termCountList.Add(4);
        //    newBond.termCountList.Add(0);
        //    newBond.termCountList.Add(6);
        //    CurrentANSSummary.securityTermLt.Add(newBond);

        //    SecurityTermProp otherBond = new SecurityTermProp();
        //    otherBond.securityDec = "国债";
        //    otherBond.countSafe = "26";
        //    otherBond.termCountList = new List<int>();
        //    otherBond.termCountList.Add(20);
        //    otherBond.termCountList.Add(2);
        //    otherBond.termCountList.Add(3);
        //    otherBond.termCountList.Add(0);
        //    otherBond.termCountList.Add(1);
        //    otherBond.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond);

        //    SecurityTermProp otherBond1 = new SecurityTermProp();
        //    otherBond1.securityDec = "金融债";
        //    otherBond1.countSafe = "80";
        //    otherBond1.termCountList = new List<int>();
        //    otherBond1.termCountList.Add(1);
        //    otherBond1.termCountList.Add(71);
        //    otherBond1.termCountList.Add(0);
        //    otherBond1.termCountList.Add(3);
        //    otherBond1.termCountList.Add(5);
        //    otherBond1.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond1);

        //    SecurityTermProp otherBond2 = new SecurityTermProp();
        //    otherBond2.securityDec = "国开";
        //    otherBond2.countSafe = "32";
        //    otherBond2.termCountList = new List<int>();
        //    otherBond2.termCountList.Add(0);
        //    otherBond2.termCountList.Add(30);
        //    otherBond2.termCountList.Add(0);
        //    otherBond2.termCountList.Add(1);
        //    otherBond2.termCountList.Add(1);
        //    otherBond2.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond2);

        //    SecurityTermProp otherBond3 = new SecurityTermProp();
        //    otherBond3.securityDec = "农发";
        //    otherBond3.countSafe = "23";
        //    otherBond3.termCountList = new List<int>();
        //    otherBond3.termCountList.Add(0);
        //    otherBond3.termCountList.Add(20);
        //    otherBond3.termCountList.Add(0);
        //    otherBond3.termCountList.Add(1);
        //    otherBond3.termCountList.Add(2);
        //    otherBond3.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond3);

        //    SecurityTermProp otherBond4 = new SecurityTermProp();
        //    otherBond4.securityDec = "口行";
        //    otherBond4.countSafe = "25";
        //    otherBond4.termCountList = new List<int>();
        //    otherBond4.termCountList.Add(1);
        //    otherBond4.termCountList.Add(21);
        //    otherBond4.termCountList.Add(0);
        //    otherBond4.termCountList.Add(1);
        //    otherBond4.termCountList.Add(2);
        //    otherBond4.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond4);

        //    SecurityTermProp otherBond5 = new SecurityTermProp();
        //    otherBond5.securityDec = "地方债";
        //    otherBond5.countSafe = "14";
        //    otherBond5.termCountList = new List<int>();
        //    otherBond5.termCountList.Add(2);
        //    otherBond5.termCountList.Add(2);
        //    otherBond5.termCountList.Add(0);
        //    otherBond5.termCountList.Add(3);
        //    otherBond5.termCountList.Add(7);
        //    otherBond5.termCountList.Add(0);
        //    CurrentANSSummary.securityTermLt.Add(otherBond5);

        //    //SecurityTermProp otherBond6 = new SecurityTermProp();
        //    //otherBond6.securityDec = "地方债2";
        //    //otherBond6.countSafe = "14";
        //    //otherBond6.termCountList = new List<int>();
        //    //otherBond6.termCountList.Add(2);
        //    //otherBond6.termCountList.Add(2);
        //    //otherBond6.termCountList.Add(0);
        //    //otherBond6.termCountList.Add(3);
        //    //otherBond6.termCountList.Add(7);
        //    //otherBond6.termCountList.Add(0);
        //    //CurrentANSSummary.securityTermLt.Add(otherBond6);

        //    //SecurityTermProp otherBond7 = new SecurityTermProp();
        //    //otherBond7.securityDec = "新发国债";
        //    //otherBond7.countSafe = "14";
        //    //otherBond7.termCountList = new List<int>();
        //    //otherBond7.termCountList.Add(2);
        //    //otherBond7.termCountList.Add(2);
        //    //otherBond7.termCountList.Add(0);
        //    //otherBond7.termCountList.Add(3);
        //    //otherBond7.termCountList.Add(7);
        //    //otherBond7.termCountList.Add(0);
        //    //CurrentANSSummary.securityTermLt.Add(otherBond7);


        //    CurrentANSSummary.InitViewSource();

        //    CurrentANSSummary.securityTypeLt = new List<SecurityTypeProp>();
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "政府债",
        //        countSafe = "10",
        //    });
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "公司债",
        //        countSafe = "20",
        //    });
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "金融债",
        //        countSafe = "20",
        //    });
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "NCD",
        //        countSafe = "30",
        //    });
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "ABS",
        //        countSafe = "20",
        //    });
        //    CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    {
        //        securityDec = "其他",
        //        countSafe = "20",
        //    });
        //    //CurrentANSSummary.securityTypeLt.Add(new SecurityTypeProp()
        //    //{
        //    //    securityDec = "其他2",
        //    //    countSafe = "30",
        //    //});

        //    CurrentANSSummary.InitViewSource();
        //}

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("MM Main Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.RestartAnsCheckTimer();
                }));
            }
        }

        private void OnSubscribeAnsSummary(ANSSummary ans)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
           {

               CurrentANSSummary = ANSSummaryProp.ANSSummary2Prop(ans);

               CurrentANSSummary.NewBondList.Source = CurrentANSSummary.securityTermLt;
               CurrentANSSummary.NewBondList.IsLiveFilteringRequested = true;
               CurrentANSSummary.NewBondList.Filter += CurrentANSSummary._NewBondList_Filter;
               if (CurrentANSSummary.NewBondList != null && CurrentANSSummary.NewBondList.View != null) { CurrentANSSummary.NewBondList.View.Refresh(); }

               CurrentANSSummary.OtherBondList.Source = CurrentANSSummary.securityTermLt;
               CurrentANSSummary.OtherBondList.IsLiveFilteringRequested = true;
               CurrentANSSummary.OtherBondList.Filter += CurrentANSSummary._OtherBondList_Filter;
               if (CurrentANSSummary.OtherBondList.View != null && CurrentANSSummary.OtherBondList.View != null) { CurrentANSSummary.OtherBondList.View.Refresh(); }
               //ANSSummaryProp.InitViewSource(CurrentANSSummary);

               int hour = int.Parse(DateTimeEx.ServerTime.ToString("HH"));
               int minute = int.Parse(DateTimeEx.ServerTime.ToString("mm"));
               LogHandler.Info(string.Format("======OnSubscribeAnsSummary====== Hour = {0} Minute = {1} AnsCheckTimer.IsEnabled = {2}", hour,minute, AnsCheckTimer.IsEnabled.ToString()));
               RestartAnsCheckTimer();

               if ((hour == StartHour && minute >= StopMinute) || hour > StartHour)
               {
                   StopAllCheckTimer();
               }
            }));
        }

        private void StopAllCheckTimer()
        {
            int hour = int.Parse(DateTimeEx.ServerTime.ToString("HH"));
            int minute = int.Parse(DateTimeEx.ServerTime.ToString("mm"));
            LogHandler.Info(string.Format("======StopAllCheckTimer====== Hour = {0}  Minute = {1}", hour,minute));

            AnsNormalWarnTimer.Stop();
            AnsStrongWarnTimer.Stop();
            needVoicePlay = false;
            VoiceWarnTimer.Stop();
            AnsCheckTimer.Stop();
            AnsWarnBH = new Thickness(0);
        }

        ~MainViewModel()
        {
            LogHandler.Info($"mm dispose");
            this.Dispose();
        }

        new public void Dispose()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (MMListBoxViewModel mmListBoxViewModel in mmListBoxVMdic.Values)
                {
                    mmListBoxViewModel.Clear();
                }
            }

            this.RestartAnsCheckTimer();

            if (AnsNormalWarnTimer != null)
            {
                AnsNormalWarnTimer.Stop();
            }
            if (AnsStrongWarnTimer != null)
            {
                AnsStrongWarnTimer.Stop();
            }
            if (AnsCheckTimer != null)
            {
                AnsCheckTimer.Stop();
            }

            if (mmListBoxVMdic != null)
            {
                foreach(MMListBoxViewModel mmListBoxViewModel in mmListBoxVMdic.Values)
                {
                    mmListBoxViewModel.Dispose();
                }
                mmListBoxVMdic.Clear();
                mmListBoxVMdic = null;
                HasGroup = false;
            }

            Utils.Data.MMGroupHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            MMHandler.Instance.UnsubscribeMMSysStatus(this.OnMMSysStatus, new List<string> { LocalEnv.UserInfo.UserName });

            base.Dispose();
        }

        private void RestartAnsCheckTimer()
        {
            LogHandler.Debug(string.Format("======Clear====== Hour = {0}", int.Parse(DateTimeEx.ServerTime.ToString("HH"))));
            int hour = int.Parse(DateTimeEx.ServerTime.ToString("HH"));
            int minute = int.Parse(DateTimeEx.ServerTime.ToString("mm"));
            LogHandler.Debug(string.Format("======RestartAnsCheckTimer====== Hour = {0} AnsCheckTimer.IsEnabled = {1}", hour, AnsCheckTimer.IsEnabled.ToString()));
            if ((hour < StartHour || (hour == StartHour && minute < StopMinute)) && AnsCheckTimer.IsEnabled == false)
            {
                LogHandler.Debug(string.Format("======RestartAnsCheckTimer.Start ====== "));
                AnsCheckTimer.Start();
            }
        }

        private void UpdateCancelWidenSetting()
        {
            UpdateCancelWidenSettingWin win = new UpdateCancelWidenSettingWin();
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true)
            {
                UpdateAllEnable("0");
            }
        }

        private async void UpdateAllEnable(string enable)
        {
            PResult result = null;
            await Task.Run(() =>
            {
                MMUpdateAllEnableReq updateAllEnableReq = new MMUpdateAllEnableReq();
                updateAllEnableReq.enable = enable;
                updateAllEnableReq.macID = EngineHelper.GetMacByNetworkInterface();
                updateAllEnableReq.insideIP = EngineHelper.GetIp();
                updateAllEnableReq.hardDiskID = EngineHelper.Getharddiskid2();
                result = MMHandler.Instance.mmUpdateAllEnable(updateAllEnableReq);
            });

            Prompt2Seconds prompt = new Prompt2Seconds();
            if ("1" == enable)
            {
                prompt.PromptTitle = "债券策略报价开启";
            }
            else
            {
                prompt.PromptTitle = "债券策略报价关闭";
            }
            prompt.Owner = Application.Current.MainWindow;
            if (result != null && result.IsSuccess())
            {
                prompt.PromptContext = "成功";
            }
            else
            {

                prompt.PromptContext = "失败";

            }
            prompt.Show();
        }

        private void UpdateWidenSetting()
        {
            if (WidenBtnText == "拉宽所有报价")
            {
                UpdateWidenSettingView view = new UpdateWidenSettingView();
                view.Owner = Application.Current.MainWindow;
                view.ShowDialog();
            }
            else
            {
                if (CustomMessageBox.Question("是否确认恢复原策略?", "恢复策略"))
                {
                    WidenQuoteBtnEnabled = false;
                    PResult result = MMHandler.Instance.mmCancelWidenSetting(null);

                    if(result == null || result.IsSuccess() == false)
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "恢复所有报价提示";
                        prompt.PromptContext = (result == null || string.IsNullOrEmpty(result.Msg)) ? "失败" : result.Msg;
    ;
                        prompt.Show();
                    }
                    
                    //if (result != null && result.IsSuccess())
                    //{
                    //WidenBtnText = "拉宽所有报价";
                    //LockVisibility = Visibility.Collapsed;
                    //BtnEnable = true;
                    //}
                }
            }
        }

        #region 过滤
        private void FilterData()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (MMListBoxViewModel mmListViewModel in mmListBoxVMdic.Values)
                {
                    mmListViewModel.FilterData(this.SearchText, this.filterBondType, this.filterCreditBondType, this.PolicyFinancialBond,
                        this.filterRemainingTime,this.filterNewBondSign, this.filterRating, this.settlTypeFilter, enableStatusFilter, this.monitorStatusFilter,this.isPauseStatusFilter);
                }
            }
        }

        StringBuilder filterBondType = new StringBuilder();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType(System.Collections.IList selectedItems)
        {
            filterBondType.Remove(0, filterBondType.Length);
            if (InterestRateDebtList != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;

                foreach (FilterListBoxItemData lbi in InterestRateDebtList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        inFilterSB.Remove(0, inFilterSB.Length);
                        notInFilter = $"{MMListBoxViewModel.GOODS_TYPE_FIELD} = 'BNC'";
                        break;
                    }

                    AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append($"{MMListBoxViewModel.BOND_CATEGORY_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterBondType.Length > 0)
                    {
                        filterBondType.Append(" or ");
                    }
                    filterBondType.Append(notInFilter);
                }
                if (filterBondType.Length > 0)
                {
                    if (filterBondType.ToString().Contains("BNC") == false)
                    {
                        filterBondType.Append($" and { MMListBoxViewModel.GOODS_TYPE_FIELD} = 'BNC'");
                    }
                }

            }
            FilterData();
        }

        StringBuilder filterCreditBondType = new StringBuilder();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType()
        {
            filterCreditBondType.Remove(0, filterCreditBondType.Length);
            if (CreditBondList != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;

                foreach (FilterListBoxItemData lbi in CreditBondList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        inFilterSB.Remove(0, inFilterSB.Length);
                        notInFilter = $"({MMListBoxViewModel.GOODS_TYPE_FIELD} <> 'BNC')";
                        break;
                    }
                    // 信用债
                    if (lbi.itemText.ToString() == "其他")
                    {
                        AddListFilter(inFilterSB, "''");
                    }
                    else
                    {
                        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    }
                }

                if (inFilterSB.Length > 0)
                {
                    if (filterCreditBondType.Length > 0)
                    {
                        filterCreditBondType.Append(" or ");
                    }
                    filterCreditBondType.Append($"{MMListBoxViewModel.BOND_CATEGORY_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterCreditBondType.Length > 0)
                    {
                        filterCreditBondType.Append(" or ");
                    }
                    filterCreditBondType.Append(notInFilter);
                }
                if (filterCreditBondType.Length > 0)
                {
                    if (filterCreditBondType.ToString().Contains("BNC") == false)
                    {
                        filterCreditBondType.Append($" and {MMListBoxViewModel.GOODS_TYPE_FIELD} <> 'BNC'");
                    }
                }
            }
            FilterData();
        }

        StringBuilder PolicyFinancialBond = new StringBuilder();
        /// <summary>
        /// 政策性金融债
        /// </summary>
        public void FilterPolicyFinancialBond()
        {
            PolicyFinancialBond.Remove(0, PolicyFinancialBond.Length);
            if (PolicyFinancialBondList != null)
            {
                foreach (FilterListBoxItemData lbi in PolicyFinancialBondList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        PolicyFinancialBond.Remove(0, PolicyFinancialBond.Length);
                        break;
                    }

                    if (PolicyFinancialBond.Length > 0)
                    {
                        PolicyFinancialBond.Append(" or ");
                    }

                    if (lbi.itemText == "国开")
                    {
                        PolicyFinancialBond.Append($"{MMListBoxViewModel.ISSUER_FIELD} = '100034'");
                    }
                    else if (lbi.itemText == "农发")
                    {
                        PolicyFinancialBond.Append($"{MMListBoxViewModel.ISSUER_FIELD} = '101915'");
                    }
                    else if (lbi.itemText == "口行")
                    {
                        PolicyFinancialBond.Append($"{MMListBoxViewModel.ISSUER_FIELD} = '100017'");
                    }

                }
            }
            this.FilterData();
        }


        private void AddListFilter(StringBuilder filterSB, string listFilterItems)
        {
            if (filterSB.Length > 0)
            {
                filterSB.Append(",");
            }
            filterSB.Append(listFilterItems);
        }

        StringBuilder filterRemainingTime = new StringBuilder();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime()
        {
            filterRemainingTime.Remove(0, filterRemainingTime.Length);
            if (RemaingingTimeList != null)
            {
                foreach (FilterListBoxItemData lbi in RemaingingTimeList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        filterRemainingTime.Remove(0, filterRemainingTime.Length);
                        break;
                    }

                    if (filterRemainingTime.Length > 0)
                    {
                        filterRemainingTime.Append(" or ");
                    }

                    if (lbi.itemText == "0~1Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.FilterData();
        }

        StringBuilder filterNewBondSign = new StringBuilder();
        /// <summary>
        /// 生成新发债券过滤条件
        /// </summary>
        public void FilterNewBondSign()
        {
            filterNewBondSign.Remove(0, filterNewBondSign.Length);
            if (NewBondSignList != null)
            {
                foreach (FilterListBoxItemData lbi in NewBondSignList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        filterNewBondSign.Remove(0, filterNewBondSign.Length);
                        break;
                    }

                    if (filterNewBondSign.Length > 0)
                    {
                        filterNewBondSign.Append(" or ");
                    }

                    if (lbi.itemText == "1Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '1Y')");
                    }
                    else if (lbi.itemText == "2Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '2Y')");
                    }
                    else if (lbi.itemText == "3Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '3Y')");
                    }
                    else if (lbi.itemText == "5Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '5Y')");
                    }
                    else if (lbi.itemText == "7Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '7Y')");
                    }
                    else if (lbi.itemText == "10Y")
                    {
                        filterNewBondSign.Append($"({MMListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MMListBoxViewModel.SECURITY_TERM_FIELD} = '10Y')");
                    }
                }
            }
            this.FilterData();
        }

        StringBuilder filterRating = new StringBuilder();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating()
        {
            this.filterRating.Remove(0, this.filterRating.Length);
            if (RatingList != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (FilterListBoxItemData lbi in RatingList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    switch (lbi.itemText)
                    {
                        case "AAA+":
                        case "AAA":
                        case "AA+":
                        case "AA":
                        case "AA-":
                        case "A+":
                            AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                            break;
                        case "其他":
                            notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                            break;
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append($"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} in ({inFilterSB})");
                }
                if (notInFilter.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append(notInFilter);
                }
            }
            this.FilterData();
        }

        StringBuilder settlTypeFilter = new StringBuilder();
        /// <summary>
        /// 清算速度过滤条件
        /// </summary>
        public void FilterSettlType()
        {
            settlTypeFilter.Remove(0, settlTypeFilter.Length);
            if (SettlTypeList != null)
            {
                foreach (FilterListBoxItemData lbi in SettlTypeList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        settlTypeFilter.Remove(0, settlTypeFilter.Length);
                        break;
                    }

                    if (settlTypeFilter.Length > 0)
                    {
                        settlTypeFilter.Append(" or ");
                    }

                    if (lbi.itemText == "T+1")
                    {
                        settlTypeFilter.Append($"({MMListBoxViewModel.SETTLE_TYPE_FIELD} = '2' or {MMListBoxViewModel.SETTLE_TYPE_FIELD} = '3')");
                    }
                    else if (lbi.itemText == "T+0")
                    {
                        settlTypeFilter.Append($"{MMListBoxViewModel.SETTLE_TYPE_FIELD} = '1'");
                    }
                }
            }
            this.FilterData();
        }

        StringBuilder enableStatusFilter = new StringBuilder();
        /// <summary>
        /// 开启状态过滤条件
        /// </summary>
        public void FilterEnableStatus()
        {
            enableStatusFilter.Remove(0, enableStatusFilter.Length);
            if (EnableList != null)
            {
                foreach (FilterListBoxItemData lbi in EnableList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        enableStatusFilter.Remove(0, enableStatusFilter.Length);
                        break;
                    }

                    if (enableStatusFilter.Length > 0)
                    {
                        enableStatusFilter.Append(" or ");
                    }

                    if (lbi.itemText == "开启")
                    {
                        enableStatusFilter.Append($"{MMListBoxViewModel.ENABLE_FIELD} = true");
                    }
                    else if (lbi.itemText == "关闭")
                    {
                        enableStatusFilter.Append($"{MMListBoxViewModel.ENABLE_FIELD} = false");
                    }
                }
            }
            this.FilterData();
        }

        StringBuilder monitorStatusFilter = new StringBuilder();
        /// <summary>
        /// 做市状态过滤条件
        /// </summary>
        public void FilterMonitorStatus()
        {
            monitorStatusFilter.Remove(0, monitorStatusFilter.Length);
            if (MonitorStatusList != null)
            {
                foreach (FilterListBoxItemData lbi in MonitorStatusList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        monitorStatusFilter.Remove(0, monitorStatusFilter.Length);
                        break;
                    }

                    if (monitorStatusFilter.Length > 0)
                    {
                        monitorStatusFilter.Append(" or ");
                    }

                    if (lbi.itemText == "危险")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '1'");
                    }
                    else if (lbi.itemText == "可达标")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '2'");
                    }
                    else if (lbi.itemText == "已达标")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '3'");
                    }
                    else if (lbi.itemText == "失败")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '4'");
                    }
                }
            }
            this.FilterData();
        }
        StringBuilder isPauseStatusFilter = new StringBuilder();
        public void FilterPauseBond(bool isPause) 
        {
            isPauseStatusFilter.Remove(0, isPauseStatusFilter.Length);
            if (isPause)
            {
                isPauseStatusFilter.Append($"{MMListBoxViewModel.ANS_PAUSE_STATUS_FIELD} = '0'");
            }
            this.FilterData();
        }

        public void FilterStrategyStatus(string[] strategyStatusArray) 
        {
            enableStatusFilter.Remove(0, enableStatusFilter.Length);
            if (strategyStatusArray != null)
            {
                foreach (string lbi in strategyStatusArray)
                {
                    if (lbi == "0001")
                    {
                        enableStatusFilter.Append($"{MMListBoxViewModel.ENABLE_FIELD} = true");
                    }
                    else if (lbi == "0010")
                    {
                        enableStatusFilter.Append($"{MMListBoxViewModel.ENABLE_FIELD} = false");
                    }
                }
            }
            this.FilterData();
        }


        public void FilterMMStatus(string[] MMStatusArray) 
        {
            monitorStatusFilter.Remove(0, monitorStatusFilter.Length);
            if (MMStatusArray != null)
            {
                foreach (string lbi in MMStatusArray)
                {
                    if (monitorStatusFilter.Length > 0)
                    {
                        monitorStatusFilter.Append(" or ");
                    }

                    if (lbi == "0011")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '1'");
                    }
                    else if (lbi == "0001")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '2'");
                    }
                    else if (lbi == "0010")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '3'");
                    }
                    else if (lbi == "0101")
                    {
                        monitorStatusFilter.Append($"{MMListBoxViewModel.ANS_MM_STATUS_FIELD} = '4'");
                    }
                }
            }
            this.FilterData();
        }

        #endregion


        #region 导入导出

        private async void ExportExcel()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"双边报价导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    this.MaskVisibility = Visibility.Visible;

                    List<string> errorList = await Sub_ExportExcel(sfd.FileName);

                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }
                this.MaskVisibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch(Exception e)
            {
                this.MaskVisibility = Visibility.Collapsed;

                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = e.Message;
                prompt.Show();
                //CustomMessageBox.Info(promptTxt, "批量导入");

                LogHelper.Error(e);
            }
        }

        private Task<List<string>> Sub_ExportExcel(string filePath)
        {
            return Task.Run(() =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }

                        File.Delete(filePath);
                    }
                   

                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    if (MMGroupHandler.Instance.CacheSortedGroups != null)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        foreach (MMGroupInfo item in MMGroupHandler.Instance.CacheSortedGroups)
                        {
                            if (item.inf1 != MMConstants.MM_GROPE_TYPE_UNIFY)
                            {
                                DataTable dt = GenerateExcelDataTable(item.id);
                                dataDic[item.groupname] = dt;
                            }
                            else 
                            {
                                DataTable dt = GroupExcelDataTable(item.id);
                                dataDic[item.groupname] = dt;
                            }
                        }
                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) errorlist.Add("无数据导出");
                    }
                    else
                    {
                        errorlist.Add("无数据导出");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }

        private DataTable GenerateExcelDataTable(string groupid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MMConstants.SecurityID_ExcelField);
            dt.Columns.Add(MMConstants.BidQuoteModel_ExcelField);
            dt.Columns.Add(MMConstants.OfrQuoteModel_ExcelField);
            dt.Columns.Add(MMConstants.BidBaseQty_ExcelField);
            dt.Columns.Add(MMConstants.OfrBaseQty_ExcelField);
            dt.Columns.Add(MMConstants.ManualBid_ExcelField);
            dt.Columns.Add(MMConstants.ManualOfr_ExcelField);
            dt.Columns.Add(MMConstants.StepLength_ExcelField);
            //dt.Columns.Add(MMConstants.BidBasePriority_ExcelField);
            //dt.Columns.Add(MMConstants.OfrBasePriority_ExcelField);
            dt.Columns.Add(MMConstants.BidBaseReSendValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrBaseReSendValue_ExcelField);
            dt.Columns.Add(MMConstants.BidBaseWidenValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrBaseWidenValue_ExcelField);
            dt.Columns.Add(MMConstants.BaseERReSendType_ExcelField);
            dt.Columns.Add(MMConstants.DealWidenLowerTime_ExcelField);
            dt.Columns.Add(MMConstants.DealWidenUpperTime_ExcelField);
            dt.Columns.Add(MMConstants.BaseSettleType_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenReferLevel_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenReferLevel_ExcelField);
            dt.Columns.Add(MMConstants.BidAlgoDefenValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrAlgoDefenValue_ExcelField);

            dt.Columns.Add(MMConstants.AlgoSafeEnable_ExcelField);//防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.BidAlgoSafeValue_ExcelField);//防守安全点
            dt.Columns.Add(MMConstants.SecondSafeEnable_ExcelField);//同向防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.QdmespSafeEnable_ExcelField);//双边防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.QdmespSafeLevel_ExcelField);//双边防守安全点档位1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
            dt.Columns.Add(MMConstants.BidQdmespSafeValue_ExcelField);//双边防守安全点

            //dt.Columns.Add(MMConstants.OfrAlgoSafeValue_ExcelField);
            dt.Columns.Add(MMConstants.BidAlgoTrigValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrAlgoTrigValue_ExcelField);
            //dt.Columns.Add(MMConstants.AlgoEnable_ExcelField);
            dt.Columns.Add(MMConstants.AlgoBaseEnable_ExcelField);
            dt.Columns.Add(MMConstants.AlgoBaseSecurityid_ExcelField);
            dt.Columns.Add(MMConstants.AlgoMidType_ExcelField);
            dt.Columns.Add(MMConstants.AlgoMidDetailDefined_ExcelField);
            dt.Columns.Add(MMConstants.AlgoMidOffsetValue_ExcelField);
            dt.Columns.Add(MMConstants.AlgoValue_ExcelField);
            //dt.Columns.Add(MMConstants.AlgoCDCEnable_ExcelField);
            // 去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
            //dt.Columns.Add(MMConstants.PosLmtEnable_ExcelField);
            //dt.Columns.Add(MMConstants.BidPosValue_ExcelField);
            //dt.Columns.Add(MMConstants.OfrPosValue_ExcelField);
            dt.Columns.Add(MMConstants.PosFakEnable_ExcelField);
            dt.Columns.Add(MMConstants.PosFakSpread_ExcelField);
            dt.Columns.Add(MMConstants.MaxFloorEnable_ExcelField);
            dt.Columns.Add(MMConstants.MaxFloor_ExcelField);
            dt.Columns.Add(MMConstants.DefenCDCEnable_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenCDCValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenCDCValue_ExcelField);
            dt.Columns.Add(MMConstants.WallEnable_ExcelField);
            dt.Columns.Add(MMConstants.ExeBondFollow_ExcelField);

            IOrderedEnumerable<STC.Common.Model.MM.MM> mmlist = null;
            if (MMGroupHandler.Instance.GetMMsByGroupID(groupid) != null)
                mmlist = MMGroupHandler.Instance.GetMMsByGroupID(groupid).OrderByDescending(x => x.createtime);

            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    if (mmlist != null)
                    {
                        foreach (STC.Common.Model.MM.MM mm in mmlist)
                        {
                            MMOrderParamReq mmOrderParamReq = mm.Params;
                            DataRow newDR = dt.NewRow();
                            if (mmOrderParamReq != null)
                            {
                                //newDR[MMConstants.BidQuoteModel_ExcelField] = "Manual" == mmOrderParamReq.bidQuoteModel ? "自动" : "手动";
                                newDR[MMConstants.SecurityID_ExcelField] = mm.securityid;
                                if ("Manual" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.BidQuoteModel_ExcelField] = MMConstants.MANUAL_QUOTEMODEL;
                                else if ("Defend" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.BidQuoteModel_ExcelField] = MMConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.BidQuoteModel_ExcelField] = MMConstants.ALGORITHM_QUOTEMODEL;
                                else if ("API" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.BidQuoteModel_ExcelField] = MMConstants.API_QUOTEMODEL;
                                //newDR[MMConstants.OfrQuoteModel_ExcelField] = "Manual" == mmOrderParamReq.ofrQuoteModel ? "自动" : "手动";
                                if ("Manual" == mmOrderParamReq.ofrQuoteModel) newDR[MMConstants.OfrQuoteModel_ExcelField] = MMConstants.MANUAL_QUOTEMODEL;
                                else if ("Defend" == mmOrderParamReq.ofrQuoteModel) newDR[MMConstants.OfrQuoteModel_ExcelField] = MMConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == mmOrderParamReq.ofrQuoteModel) newDR[MMConstants.OfrQuoteModel_ExcelField] = MMConstants.ALGORITHM_QUOTEMODEL;
                                else if ("API" == mmOrderParamReq.ofrQuoteModel) newDR[MMConstants.OfrQuoteModel_ExcelField] = MMConstants.API_QUOTEMODEL;

                                newDR[MMConstants.BidBaseQty_ExcelField] = mmOrderParamReq.bidBaseQty;
                                newDR[MMConstants.OfrBaseQty_ExcelField] = mmOrderParamReq.ofrBaseQty;
                                newDR[MMConstants.ManualBid_ExcelField] = mmOrderParamReq.manualBid;
                                newDR[MMConstants.ManualOfr_ExcelField] = mmOrderParamReq.manualOfr;
                                newDR[MMConstants.StepLength_ExcelField] = mmOrderParamReq.stepLength;
                                //newDR[MMConstants.BidBasePriority_ExcelField] = "2" == mmOrderParamReq.bidBasePriority ? "算法" : "防守"; ;
                                //newDR[MMConstants.OfrBasePriority_ExcelField] = "2" == mmOrderParamReq.ofrBasePriority ? "算法" : "防守"; ;
                                newDR[MMConstants.BidBaseReSendValue_ExcelField] = mmOrderParamReq.bidBaseReSendValue;
                                newDR[MMConstants.OfrBaseReSendValue_ExcelField] = mmOrderParamReq.ofrBaseReSendValue;
                                newDR[MMConstants.BidBaseWidenValue_ExcelField] = mmOrderParamReq.bidBaseWidenValue;
                                newDR[MMConstants.OfrBaseWidenValue_ExcelField] = mmOrderParamReq.ofrBaseWidenValue;
                                newDR[MMConstants.BaseERReSendType_ExcelField] = "2" == mmOrderParamReq.BaseERReSendType ? MMConstants.LEAVE_BASEERRESENDTYPE : MMConstants.QTY_BASEERRESENDTYPE;
                                newDR[MMConstants.DealWidenLowerTime_ExcelField] = string.IsNullOrEmpty(mmOrderParamReq.dealWidenLowerTime) ? MMConstants.DEALWIDEN_VALUE : mmOrderParamReq.dealWidenLowerTime;
                                newDR[MMConstants.DealWidenUpperTime_ExcelField] = string.IsNullOrEmpty(mmOrderParamReq.dealWidenUpperTime) ? MMConstants.DEALWIDEN_VALUE : mmOrderParamReq.dealWidenUpperTime;
                                newDR[MMConstants.BaseSettleType_ExcelField] = "2" == mm.settltype ? MMConstants.T1_SETTLTYPE : MMConstants.T0_SETTLTYPE;
                                newDR[MMConstants.BidAlgoDefenValue_ExcelField] = mmOrderParamReq.bidAlgoDefenValue;
                                newDR[MMConstants.OfrAlgoDefenValue_ExcelField] = mmOrderParamReq.ofrAlgoDefenValue;
                                newDR[MMConstants.AlgoSafeEnable_ExcelField] = "1" == mmOrderParamReq.algoSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE; 
                                newDR[MMConstants.BidAlgoSafeValue_ExcelField] = mmOrderParamReq.bidAlgoSafeValue;
                                newDR[MMConstants.SecondSafeEnable_ExcelField] = "1" == mmOrderParamReq.secondSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE; 
                                newDR[MMConstants.QdmespSafeEnable_ExcelField] = "1" == mmOrderParamReq.qdmespSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                if ("1" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL3;
                                else if ("4" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL5;
                                newDR[MMConstants.BidQdmespSafeValue_ExcelField] = mmOrderParamReq.bidQdmespSafeValue;
                                //newDR[MMConstants.OfrAlgoSafeValue_ExcelField] = mmOrderParamReq.ofrAlgoSafeValue;
                                newDR[MMConstants.BidAlgoTrigValue_ExcelField] = mmOrderParamReq.bidAlgoTrigValue;
                                newDR[MMConstants.OfrAlgoTrigValue_ExcelField] = mmOrderParamReq.ofrAlgoTrigValue;
                                //newDR[MMConstants.AlgoEnable_ExcelField] = "1" == mmOrderParamReq.algoEnable ? "是" : "否";
                                newDR[MMConstants.AlgoBaseEnable_ExcelField] = "1" == mmOrderParamReq.algoBaseEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.AlgoBaseSecurityid_ExcelField] = mmOrderParamReq.algoBaseSecurityid;
                                if ("1" == mmOrderParamReq.algoMidType) newDR[MMConstants.AlgoMidType_ExcelField] = MMConstants.Mid_ALGOMIDTYPE;
                                else if ("2" == mmOrderParamReq.algoMidType) newDR[MMConstants.AlgoMidType_ExcelField] = MMConstants.Val_ALGOMIDTYPE;
                                else if ("3" == mmOrderParamReq.algoMidType) newDR[MMConstants.AlgoMidType_ExcelField] = MMConstants.Cus_ALGOMIDTYPE;
                                newDR[MMConstants.AlgoMidDetailDefined_ExcelField] = mmOrderParamReq.algoMidDetailDefined;
                                newDR[MMConstants.AlgoMidOffsetValue_ExcelField] = mmOrderParamReq.algoMidOffsetValue;
                                newDR[MMConstants.AlgoValue_ExcelField] = mmOrderParamReq.algoValue;
                                //newDR[MMConstants.AlgoCDCEnable_ExcelField] = "1" == mmOrderParamReq.algoCDCEnable ? "是" : "否";
                                //去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
                                //newDR[MMConstants.PosLmtEnable_ExcelField] = "1" == mmOrderParamReq.posLmtEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                //newDR[MMConstants.BidPosValue_ExcelField] = mmOrderParamReq.bidPosValue;
                                //newDR[MMConstants.OfrPosValue_ExcelField] = mmOrderParamReq.ofrPosValue;
                                newDR[MMConstants.PosFakEnable_ExcelField] = "1" == mmOrderParamReq.posFakEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.PosFakSpread_ExcelField] = mmOrderParamReq.posFakSpread;

                                newDR[MMConstants.MaxFloorEnable_ExcelField] = "1" == mmOrderParamReq.maxFloorEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.MaxFloor_ExcelField] = mmOrderParamReq.maxFloor;

                                newDR[MMConstants.DefenCDCEnable_ExcelField] = "1" == mmOrderParamReq.defenCDCEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.BidDefenCDCValue_ExcelField] = mmOrderParamReq.bidDefenCDCValue;
                                newDR[MMConstants.OfrDefenCDCValue_ExcelField] = mmOrderParamReq.ofrDefenCDCValue;
                                if ("1" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.CFETS_DEFENREFERMARKETTYPE;

                                if ("1" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL3; 
                                else if ("4" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL5;

                                if ("1" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.CFETS_DEFENREFERMARKETTYPE;

                                if ("1" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL3;
                                else if ("4" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL5;
                                newDR[MMConstants.WallEnable_ExcelField] = "1" == mmOrderParamReq.wallEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.ExeBondFollow_ExcelField] = "0" == mmOrderParamReq.exeBondFollow ? "行权" : "到期";
                            }
                            else
                            {
                                newDR[MMConstants.SecurityID_ExcelField] = mm.securityid;
                                newDR[MMConstants.BidQuoteModel_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrQuoteModel_ExcelField] = string.Empty;
                                newDR[MMConstants.BidBaseQty_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrBaseQty_ExcelField] = string.Empty;
                                newDR[MMConstants.ManualBid_ExcelField] = string.Empty;
                                newDR[MMConstants.ManualOfr_ExcelField] = string.Empty;
                                newDR[MMConstants.StepLength_ExcelField] = string.Empty;
                                //newDR[MMConstants.BidBasePriority_ExcelField] = "防守"; ;
                                //newDR[MMConstants.OfrBasePriority_ExcelField] = "防守"; ;
                                newDR[MMConstants.BidBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BidBaseWidenValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrBaseWidenValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BaseERReSendType_ExcelField] = string.Empty;
                                newDR[MMConstants.DealWidenLowerTime_ExcelField] = MMConstants.DEALWIDEN_VALUE;
                                newDR[MMConstants.DealWidenUpperTime_ExcelField] = MMConstants.DEALWIDEN_VALUE;
                                newDR[MMConstants.BaseSettleType_ExcelField] = string.Empty;
                                newDR[MMConstants.BidAlgoDefenValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrAlgoDefenValue_ExcelField] = string.Empty;

                                newDR[MMConstants.AlgoSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.BidAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMConstants.SecondSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.QdmespSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.QdmespSafeLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.BidQdmespSafeValue_ExcelField] = string.Empty;

                                //newDR[MMConstants.OfrAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BidAlgoTrigValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrAlgoTrigValue_ExcelField] = string.Empty;
                                //newDR[MMConstants.AlgoEnable_ExcelField] = "否";
                                newDR[MMConstants.AlgoBaseEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.AlgoBaseSecurityid_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoMidType_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoMidDetailDefined_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoMidOffsetValue_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoValue_ExcelField] = string.Empty;
                                //newDR[MMConstants.AlgoCDCEnable_ExcelField] = "否";
                                // 去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
                                //newDR[MMConstants.PosLmtEnable_ExcelField] = MMConstants.NO_VALUE;
                                //newDR[MMConstants.BidPosValue_ExcelField] = string.Empty;
                                //newDR[MMConstants.OfrPosValue_ExcelField] = string.Empty;
                                newDR[MMConstants.PosFakEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.PosFakSpread_ExcelField] = string.Empty;
                                newDR[MMConstants.MaxFloorEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.MaxFloor_ExcelField] = string.Empty;
                                newDR[MMConstants.DefenCDCEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.BidDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BidDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMConstants.BidDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.WallEnable_ExcelField] = string.Empty;
                                newDR[MMConstants.ExeBondFollow_ExcelField] = string.Empty;
                            }

                            dt.Rows.Add(newDR);
                        }
                    }
                }
            }
            return dt;
        }


        private DataTable GroupExcelDataTable(string groupid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MMConstants.SecurityID_ExcelField);
            dt.Columns.Add(MMConstants.QuoteModel_ExcelField);
            dt.Columns.Add(MMConstants.BidBaseQty_ExcelField);
            dt.Columns.Add(MMConstants.OfrBaseQty_ExcelField);
            dt.Columns.Add(MMConstants.BidBaseReSendValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrBaseReSendValue_ExcelField);
            dt.Columns.Add(MMConstants.BaseSettleType_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenReferLevel_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenReferLevel_ExcelField);
            dt.Columns.Add(MMConstants.BidAlgoDefenValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrAlgoDefenValue_ExcelField);

            dt.Columns.Add(MMConstants.AlgoSafeEnable_ExcelField);//防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.BidAlgoSafeValue_ExcelField);//防守安全点
            dt.Columns.Add(MMConstants.SecondSafeEnable_ExcelField);//同向防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.QdmespSafeEnable_ExcelField);//双边防守安全点是否启用0：否、1：是
            dt.Columns.Add(MMConstants.QdmespSafeLevel_ExcelField);//双边防守安全点档位1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
            dt.Columns.Add(MMConstants.BidQdmespSafeValue_ExcelField);//双边防守安全点

            //dt.Columns.Add(MMConstants.OfrAlgoSafeValue_ExcelField);
            dt.Columns.Add(MMConstants.BidAlgoTrigValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrAlgoTrigValue_ExcelField);
            dt.Columns.Add(MMConstants.AlgoMidType_ExcelField);
            dt.Columns.Add(MMConstants.AlgoMidOffsetValue_ExcelField);
            dt.Columns.Add(MMConstants.AlgoValue_ExcelField);
            dt.Columns.Add(MMConstants.PosFakEnable_ExcelField);
            dt.Columns.Add(MMConstants.PosFakSpread_ExcelField);
            dt.Columns.Add(MMConstants.MaxFloorEnable_ExcelField);
            dt.Columns.Add(MMConstants.MaxFloor_ExcelField);
            dt.Columns.Add(MMConstants.DefenCDCEnable_ExcelField);
            dt.Columns.Add(MMConstants.BidDefenCDCValue_ExcelField);
            dt.Columns.Add(MMConstants.OfrDefenCDCValue_ExcelField);
            dt.Columns.Add(MMConstants.WallEnable_ExcelField);

            IOrderedEnumerable<STC.Common.Model.MM.MM> mmlist = null;
            if (MMGroupHandler.Instance.GetMMsByGroupID(groupid) != null)
                mmlist = MMGroupHandler.Instance.GetMMsByGroupID(groupid).OrderByDescending(x => x.createtime);

            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    if (mmlist != null)
                    {
                        foreach (STC.Common.Model.MM.MM mm in mmlist)
                        {
                            MMOrderParamReq mmOrderParamReq = mm.Params;
                            DataRow newDR = dt.NewRow();
                            if (mmOrderParamReq != null)
                            {
                                newDR[MMConstants.SecurityID_ExcelField] = mm.securityid;
                                if ("Defend" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.QuoteModel_ExcelField] = MMConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == mmOrderParamReq.bidQuoteModel) newDR[MMConstants.QuoteModel_ExcelField] = MMConstants.ALGORITHM_QUOTEMODEL;

                                newDR[MMConstants.BidBaseQty_ExcelField] = mmOrderParamReq.bidBaseQty;
                                newDR[MMConstants.OfrBaseQty_ExcelField] = mmOrderParamReq.ofrBaseQty;
                                newDR[MMConstants.BidBaseReSendValue_ExcelField] = mmOrderParamReq.bidBaseReSendValue;
                                newDR[MMConstants.OfrBaseReSendValue_ExcelField] = mmOrderParamReq.ofrBaseReSendValue;
                                newDR[MMConstants.BaseSettleType_ExcelField] = "2" == mm.settltype ? MMConstants.T1_SETTLTYPE : MMConstants.T0_SETTLTYPE;
                                newDR[MMConstants.BidAlgoDefenValue_ExcelField] = mmOrderParamReq.bidAlgoDefenValue;
                                newDR[MMConstants.OfrAlgoDefenValue_ExcelField] = mmOrderParamReq.ofrAlgoDefenValue;

                                newDR[MMConstants.AlgoSafeEnable_ExcelField] = "1" == mmOrderParamReq.algoSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.BidAlgoSafeValue_ExcelField] = mmOrderParamReq.bidAlgoSafeValue;
                                newDR[MMConstants.SecondSafeEnable_ExcelField] = "1" == mmOrderParamReq.secondSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.QdmespSafeEnable_ExcelField] = "1" == mmOrderParamReq.qdmespSafeEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                if ("1" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL3;
                                else if ("4" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.qdmespSafeLevel) newDR[MMConstants.QdmespSafeLevel_ExcelField] = MMConstants.LEVEL5;

                                //newDR[MMConstants.OfrAlgoSafeValue_ExcelField] = mmOrderParamReq.ofrAlgoSafeValue;
                                newDR[MMConstants.BidAlgoTrigValue_ExcelField] = mmOrderParamReq.bidAlgoTrigValue;
                                newDR[MMConstants.OfrAlgoTrigValue_ExcelField] = mmOrderParamReq.ofrAlgoTrigValue;
                                if ("2" == mmOrderParamReq.algoMidType) newDR[MMConstants.AlgoMidType_ExcelField] = MMConstants.Val_ALGOMIDTYPE;
                                newDR[MMConstants.AlgoMidOffsetValue_ExcelField] = mmOrderParamReq.algoMidOffsetValue;
                                newDR[MMConstants.AlgoValue_ExcelField] = mmOrderParamReq.algoValue;
                                newDR[MMConstants.PosFakEnable_ExcelField] = "1" == mmOrderParamReq.posFakEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.PosFakSpread_ExcelField] = mmOrderParamReq.posFakSpread;

                                newDR[MMConstants.MaxFloorEnable_ExcelField] = "1" == mmOrderParamReq.maxFloorEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.MaxFloor_ExcelField] = mmOrderParamReq.maxFloor;

                                newDR[MMConstants.DefenCDCEnable_ExcelField] = "1" == mmOrderParamReq.defenCDCEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                                newDR[MMConstants.BidDefenCDCValue_ExcelField] = mmOrderParamReq.bidDefenCDCValue;
                                newDR[MMConstants.OfrDefenCDCValue_ExcelField] = mmOrderParamReq.ofrDefenCDCValue;
                                if ("1" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == mmOrderParamReq.bidDefenReferMarketType) newDR[MMConstants.BidDefenReferMarketType_ExcelField] = MMConstants.CFETS_DEFENREFERMARKETTYPE;

                                if ("1" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL3;
                                else if ("4" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.bidDefenReferLevel) newDR[MMConstants.BidDefenReferLevel_ExcelField] = MMConstants.LEVEL5;

                                if ("1" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == mmOrderParamReq.ofrDefenReferMarketType) newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = MMConstants.CFETS_DEFENREFERMARKETTYPE;

                                if ("1" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL1;
                                else if ("2" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL2;
                                else if ("3" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL3;
                                else if ("4" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL4;
                                else if ("5" == mmOrderParamReq.ofrDefenReferLevel) newDR[MMConstants.OfrDefenReferLevel_ExcelField] = MMConstants.LEVEL5;
                                newDR[MMConstants.WallEnable_ExcelField] = "1" == mmOrderParamReq.wallEnable ? MMConstants.YES_VALUE : MMConstants.NO_VALUE;
                            }
                            else
                            {
                                newDR[MMConstants.SecurityID_ExcelField] = mm.securityid;
                                newDR[MMConstants.QuoteModel_ExcelField] = string.Empty;
                                newDR[MMConstants.BidBaseQty_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrBaseQty_ExcelField] = string.Empty;
                                newDR[MMConstants.BidBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BaseSettleType_ExcelField] = string.Empty;
                                newDR[MMConstants.BidAlgoDefenValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrAlgoDefenValue_ExcelField] = string.Empty;

                                newDR[MMConstants.AlgoSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.BidAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMConstants.SecondSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.QdmespSafeEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.QdmespSafeLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.BidQdmespSafeValue_ExcelField] = string.Empty;


                                //newDR[MMConstants.OfrAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BidAlgoTrigValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrAlgoTrigValue_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoMidType_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoMidOffsetValue_ExcelField] = string.Empty;
                                newDR[MMConstants.AlgoValue_ExcelField] = string.Empty;
                                newDR[MMConstants.PosFakEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.PosFakSpread_ExcelField] = string.Empty;
                                newDR[MMConstants.MaxFloorEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.MaxFloor_ExcelField] = string.Empty;
                                newDR[MMConstants.DefenCDCEnable_ExcelField] = MMConstants.NO_VALUE;
                                newDR[MMConstants.BidDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMConstants.BidDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMConstants.BidDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMConstants.OfrDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMConstants.WallEnable_ExcelField] = MMConstants.NO_VALUE;
                            }

                            dt.Rows.Add(newDR);
                        }
                    }
                }
            }
            return dt;
        }

        private async void ImportExcel()
        {
            try
            {
                if (MMGroupHandler.Instance.HasOpen())
                {
                    Prompt2SecondsMsg("请先关闭做市中的债券");
                    return ;
                }
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "Excel Documents|*.xls;*.xlsx";
                ofd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !ofd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    // 2.等待
                    this.MaskVisibility = Visibility.Visible;

                    // 3.导入操作
                    List<string> errorList = await Sub_ImportExcel(ofd.FileName);


                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导入成功";
                    }
                }
                else
                {
                    promptTxt = "取消导入";
                }

                this.MaskVisibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2SecondsMsg(promptTxt);
                }
            }
            catch (Exception e)
            {
                this.MaskVisibility = Visibility.Collapsed;
                Prompt2SecondsMsg(e.Message);
                LogHelper.Error(e);
            }
        }

        private void Prompt2SecondsMsg(string promptTxt) 
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.Owner = Application.Current.MainWindow;
            prompt.PromptTitle = "批量导入";
            prompt.PromptContext = promptTxt;
            prompt.Show();
        }
        private Task<List<string>> Sub_ImportExcel(string filePath)
        {

            return Task<List<string>>.Run(async () =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return errorlist;
                    }

                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    // 2.检查下group下面是否有开启债券
                    if (MMGroupHandler.Instance.HasOpen())
                    {
                        errorlist.Add("请先关闭做市中的债券");
                        return errorlist;
                    }

                    Dictionary<int, string> dic = helper.ReturnSheetList();

                    if(dic.Count > 20)
                    {
                        errorlist.Add("分组总数应不超过20个");
                        return errorlist;
                    }
                    //foreach (string groupName in dic.Values)
                    //{
                    //    // 检查是否有打开的债券
                    //    MMGroupInfo mmGroupInfo = MMGroupHandler.Instance.IsGroupNameExist(groupName);
                    //    if (mmGroupInfo != null)
                    //    {
                    //        if (string.IsNullOrEmpty(mmGroupInfo.id) == false)
                    //        {
                    //            lock (string.Intern(mmGroupInfo.id))
                    //            {
                    //                List<STC.Common.Model.MM.MM> mmlist = MMGroupHandler.Instance.GetMMsByGroupID(mmGroupInfo.id);

                    //                if (mmlist != null && mmlist.Count > 0)
                    //                {
                    //                    foreach (STC.Common.Model.MM.MM mm in mmlist)
                    //                    {
                    //                        if ("1" == mm.enable)
                    //                        {
                    //                            errorlist.Add("请先关闭做市中的债券");
                    //                            return errorlist;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    // 3.读文件
                    Dictionary<string, int[]> checkImportDic = new Dictionary<string, int[]>();
                    MMReq request = new MMReq();
                    request.Action = "4";
                    request.mmList = new List<STC.Common.Model.MM.MM>();
                    ObservableCollection<ImportErrorInfo> checkMMErrList = new ObservableCollection<ImportErrorInfo>();
                    ObservableCollection<MMGroupInfo> newMMGroupInfoList = new ObservableCollection<MMGroupInfo>();
                    ObservableCollection<MMGroupInfo> updateMMGroupInfoList = new ObservableCollection<MMGroupInfo>();
                    IOrderedEnumerable<MMGroupInfo> sortedGroups = await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                    int ordersn = 1;
                    if (sortedGroups != null && sortedGroups.Count() > 0)
                    {
                        ordersn = sortedGroups.Last().ordersn + 1;
                    }

                    List<int> unifyGroupKeys = new List<int>();
                    bool isUnifyGroupError = false;
                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        if(pair.Value.Length > 10)
                        {
                            ImportErrorInfo error = new ImportErrorInfo();
                            error.ImportErrorResean = $"分组名称【{pair.Value}】超过10个汉字";
                            checkMMErrList.Add(error);
                            continue;
                        }

                        MMGroupInfo mmGroupInfo = MMGroupHandler.Instance.IsGroupNameExist(pair.Value);

                        // 如果是整组，不做任何操作
                        /*if (mmGroupInfo != null && mmGroupInfo.inf1 == MMConstants.MM_GROPE_TYPE_UNIFY)
                        {
                            unifyGroupKeys.Add(pair.Key);
                            continue;
                        }*/

                        if (mmGroupInfo == null)
                        {
                            mmGroupInfo = new MMGroupInfo();
                            mmGroupInfo.groupname = pair.Value;
                            mmGroupInfo.terminal = LocalEnv.ServerInfo.Version;
                            mmGroupInfo.action = 1;
                            mmGroupInfo.ordersn = ordersn;
                            newMMGroupInfoList.Add(mmGroupInfo);
                            ordersn++;
                        }
                        else
                        {
                            if (mmGroupInfo.inf1 == MMConstants.MM_GROPE_TYPE_UNIFY)
                            {
                                mmGroupInfo.groupname = pair.Value;
                                mmGroupInfo.terminal = LocalEnv.ServerInfo.Version;
                                mmGroupInfo.action = 2;
                                updateMMGroupInfoList.Add(mmGroupInfo);
                            }
                        }

                        string groupID = mmGroupInfo.id;
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        int validCount = 0;

                        bool isUnifyGroup = checkUnifyGroup(importDT);
                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            Dictionary<string, string> serverCheckDic = new Dictionary<string, string>();
                            /**********************不检查重复***********************
                            List<MMCheckSecurityUserReq> reqList = new List<MMCheckSecurityUserReq>();
                            // 重用检查
                            foreach (DataRow dr in importDT.Rows)
                            {
                                string securityID = CommonUtils.ConvertToString(dr[MMConstants.SecurityID_ExcelField]);
                                if (securityID.Length > 0 && securityID.IndexOf(".") == -1 && securityID.EndsWith(".IB", StringComparison.CurrentCultureIgnoreCase) == false)
                                {
                                    securityID += ".IB";
                                }
                                string settleType = MMConstants.T1_SETTLTYPE == CommonUtils.ConvertToString(dr[MMConstants.BaseSettleType_ExcelField]) ? "2" : "1";

                                MMCheckSecurityUserReq req = new MMCheckSecurityUserReq();
                                req.securityId = securityID;
                                req.settltype = settleType;

                                
                                reqList.Add(req);

                                
                            }
                            MMCheckSecurityUserReplyPo reply = MMHandler.Instance.mmCheckSecurityUser(reqList);

                            if (reply != null && reply.IsSuccess())
                            {
                                if (reply.Datas != null && reply.Datas.Count > 0)
                                {
                                    foreach (CheckSecurityFeedback po in reply.Datas)
                                    {
                                        serverCheckDic[po.securityId] = po.feedback;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Info("检查失败");
                            }
                            *********************************************/
                            if (isUnifyGroup)
                            {
                                bool isUnifyGroupOnly = checkGroup(importDT);
                                if (!isUnifyGroupOnly)
                                {
                                    ImportErrorInfo error = new ImportErrorInfo();
                                    error.GroupName = string.Format("[{0}]", pair.Value);
                                    error.ImportErrorResean = $"整组参数不一致，请修改后导入";
                                    checkMMErrList.Add(error);
                                    isUnifyGroupError = true;
                                    continue;
                                }
                                else 
                                {
                                    if (importDT.Rows.Count > 0)
                                    {
                                        DataRow firstRow = importDT.Rows[0];
                                        mmGroupInfo.inf1 = MMConstants.MM_GROPE_TYPE_UNIFY;
                                        List<ImportErrorInfo> errList;
                                        MMOrderParamReq mMOrder = CheckImportMMList(firstRow, null, serverCheckDic, out errList, pair.Value, true,true);
                                        string inf2 = JsonConvert.SerializeObject(mMOrder);
                                        mmGroupInfo.inf2 = inf2;
                                    }
                                    else 
                                    {
                                        MMOrderParamReq mMOrder = new MMOrderParamReq();

                                        MMCommonUtils.MMOrderParamReqToDefault(mMOrder);
                                        //mMOrder.InitData();
                                        mMOrder.bidQuoteModel = mMOrder.ofrQuoteModel = "Defend";
                                        mmGroupInfo.inf1 = MMConstants.MM_GROPE_TYPE_UNIFY;
                                        mmGroupInfo.inf2 = JsonConvert.SerializeObject(mMOrder);
                                    }
                                }
                            }

                            foreach (DataRow dr in importDT.Rows)
                            {

                                // 插入前检查
                                List<ImportErrorInfo> errList;
                                MMOrderParamReq mmOrderParamReq = CheckImportMMList(dr, request.mmList, serverCheckDic, out errList,pair.Value, isUnifyGroup,false);
                                if (errList != null && errList.Count > 0)
                                {
                                    foreach (ImportErrorInfo errorInfo in errList)
                                    {
                                        checkMMErrList.Add(errorInfo);
                                    }

                                    continue;
                                }

                                request.mmList.Add(new STC.Common.Model.MM.MM()
                                {
                                    securityid = mmOrderParamReq.securityid,
                                    //symbol = addBondWind.symbol,
                                    //remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}",
                                    terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version),
                                    groupid = groupID,
                                    groupname = mmGroupInfo.groupname,
                                    settltype = mmOrderParamReq.baseSettleType,
                                    enable = "0",
                                    symbol = mmOrderParamReq.symbol,
                                    //ip = 
                                    Params = mmOrderParamReq
                                });
                                validCount++;

                            }
                        }
                        else
                        {
                            if (isUnifyGroup) 
                            {
                                mmGroupInfo.inf1 = MMConstants.MM_GROPE_TYPE_UNIFY;
                            }
                            else
                            {
                                mmGroupInfo.inf1 = MMConstants.MM_GROPE_TYPE_NORMAL;
                            }
                        }


                        checkImportDic[mmGroupInfo.groupname] = new int[] { validCount, importDT != null ? importDT.Rows.Count : 0 };
                    }

                    // 去除整组
                    /*foreach(int index in unifyGroupKeys)
                    {
                        dic.Remove(index);
                    }*/

                    if (checkImportDic.Count > 0)
                    {
                        foreach (ImportErrorInfo errorInfo in checkMMErrList) 
                        {
                            List<MM> mmList = request.mmList.Where(s => s.securityid.Equals(errorInfo.SecurityID)).Select(a=>a).ToList();
                            foreach (MM mm in mmList) 
                            {
                                int[] count = checkImportDic[mm.groupname];
                                if (count != null) 
                                {
                                    int validCount = count[0];
                                    if (validCount > 0) 
                                    {
                                        validCount = validCount - 1;
                                    }
                                    checkImportDic[mm.groupname] = new int[] { validCount,count[1]};
                                }
                                request.mmList.Remove(mm);
                            }
                        }
                        // 导入前确认
                        BatchImportConfirmViewModel batchImportConfirmViewModel = new BatchImportConfirmViewModel();
                        // 本次导入债券数量
                        ObservableCollection<ImportProp> fileImportInfoList = new ObservableCollection<ImportProp>();
                        foreach (string groupName in checkImportDic.Keys)
                        {
                            ImportProp item = new ImportProp()
                            {
                                GroupName = groupName,
                                ValidCount = checkImportDic[groupName][0].ToString(),
                                TotalCount = checkImportDic[groupName][1].ToString()
                            };
                            fileImportInfoList.Add(item);
                        }
                        batchImportConfirmViewModel.FileImportInfo = fileImportInfoList;

                        // 导入后债券数量
                        List<string> importGroupNameList = dic.Values.ToList();
                        ObservableCollection<ImportProp> afterImportInfoList = new ObservableCollection<ImportProp>();
                        ObservableCollection<MMGroupInfo> deleteGroup = new ObservableCollection<MMGroupInfo>();// 删除组
                        ObservableCollection<MMGroupInfo> clearGroup = new ObservableCollection<MMGroupInfo>();// 内容清理
                        if (MMGroupHandler.Instance.CacheSortedGroups != null)
                        {
                            foreach (MMGroupInfo item in MMGroupHandler.Instance.CacheSortedGroups)
                            {
                                /*if (item.inf1 == MMConstants.MM_GROPE_TYPE_UNIFY)
                                {
                                    continue;
                                }*/

                                int count = 0;
                                if (string.IsNullOrEmpty(item.id) == false)
                                {
                                    lock (string.Intern(item.id))
                                    {
                                        List<STC.Common.Model.MM.MM> mmList = MMGroupHandler.Instance.GetMMsByGroupID(item.id);
                                        count = checkImportDic.ContainsKey(item.groupname) ? checkImportDic[item.groupname][0] : (mmList == null ? 0 : mmList.Count);
                                    }
                                }
                                ImportProp importProp = new ImportProp()
                                {
                                    Index = importGroupNameList.IndexOf(item.groupname),
                                    GroupName = item.groupname,
                                    TotalCount = count.ToString()
                                };
                                if (checkImportDic.ContainsKey(item.groupname))
                                {
                                    if (count == 0)
                                    {
                                        clearGroup.Add(item);
                                    }
                                    afterImportInfoList.Add(importProp);
                                }
                                else
                                {
                                    deleteGroup.Add(item);
                                }
                            }
                        }
                        
                        foreach (MMGroupInfo item in newMMGroupInfoList) 
                        {
                            int count = 0;
                            if (string.IsNullOrEmpty(item.id) == true)
                            {
                                lock (string.Intern(item.groupname))
                                {
                                    count = checkImportDic.ContainsKey(item.groupname) ? checkImportDic[item.groupname][0] : (0);
                                }
                            }
                            ImportProp importProp = new ImportProp()
                            {
                                Index = importGroupNameList.IndexOf(item.groupname),
                                GroupName = item.groupname,
                                TotalCount = count.ToString()
                            };
                            if (checkImportDic.ContainsKey(item.groupname))
                            {
                                afterImportInfoList.Add(importProp);
                            }
                        }

                        // 根据Excel排序
                        batchImportConfirmViewModel.AfterImportInfo = afterImportInfoList.OrderBy(x => x.Index);

                        // 提示信息
                        batchImportConfirmViewModel.ErrorInfo = checkMMErrList;
                        if (isUnifyGroupError)
                        {
                            batchImportConfirmViewModel.ConfirmVisible = Visibility.Collapsed;
                        }

                        bool? result = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            BatchImportConfirmWindow confirmWin = new BatchImportConfirmWindow();
                            confirmWin.DataContext = batchImportConfirmViewModel;
                            confirmWin.Owner = Application.Current.MainWindow;
                            result = confirmWin.ShowDialog();
                        });


                        if (result == true)
                        {
                            foreach (MMGroupInfo item in deleteGroup) 
                            {
                                MMGroupReq mmRequest = new MMGroupReq();
                                mmRequest.action = "3";//新增
                                mmRequest.id = item.id;
                                PResult groupResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(mmRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    if (groupResult.IsSuccess())
                                    {
                                        await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                                        Utils.Data.MMGroupHandler.Instance.Notify(Utils.Data.MMGroupHandler.FavoriteTypeEnum.DelGroup, item.id, item.groupname);
                                    }
                                    else
                                    {
                                        CustomMessageBox.Warning(groupResult.Msg);
                                    }
                                }
                            }
                            foreach (MMGroupInfo item in updateMMGroupInfoList)
                            {
                                MMGroupReq mmRequest = new MMGroupReq();
                                mmRequest.groupname = item.groupname;//组名
                                mmRequest.action = "2";//更新
                                mmRequest.ordersn = item.ordersn.ToString();//顺序
                                mmRequest.terminal = LocalEnv.ServerInfo.Version; // 终端
                                mmRequest.id = item.id;
                                if (!string.IsNullOrEmpty(item.inf1))
                                {
                                    mmRequest.inf1 = item.inf1;
                                    if (mmRequest.inf1.Equals(MMConstants.MM_GROPE_TYPE_UNIFY))
                                    {
                                        mmRequest.inf2 = item.inf2;
                                    }
                                }
                                PResult groupResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(mmRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    sortedGroups = await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                                    if (sortedGroups != null)
                                    {
                                        foreach (MMGroupInfo gi in sortedGroups)
                                        {
                                            if (gi.id.Equals(mmRequest.id))
                                            {
                                                Utils.Data.MMGroupHandler.Instance.Notify(Utils.Data.MMGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, item.groupname);
                                                foreach (MM obj in request.mmList)
                                                {
                                                    if (obj.groupname.Equals(gi.groupname))
                                                    {
                                                        obj.groupid = gi.id;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (MMGroupInfo item in newMMGroupInfoList)
                            {
                                MMGroupReq mmRequest = new MMGroupReq();
                                mmRequest.groupname = item.groupname;//组名
                                mmRequest.action = "1";//新增
                                mmRequest.ordersn = item.ordersn.ToString();//顺序
                                mmRequest.terminal = LocalEnv.ServerInfo.Version; // 终端
                                if (!string.IsNullOrEmpty(item.inf1)) 
                                {
                                    mmRequest.inf1 = item.inf1;
                                    if (mmRequest.inf1.Equals(MMConstants.MM_GROPE_TYPE_UNIFY))
                                    {
                                        OrderParamProp orderParam = new OrderParamProp(null);
                                        mmRequest.inf2 = item.inf2;
                                    }
                                }
                                PResult groupResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(mmRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    sortedGroups = await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                                    if (sortedGroups != null)
                                    {
                                        MMGroupInfo gi = sortedGroups.Last();
                                        Utils.Data.MMGroupHandler.Instance.Notify(Utils.Data.MMGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, item.groupname);
                                        foreach(MM obj in request.mmList)
                                        {
                                            if (string.IsNullOrEmpty(obj.groupid) && obj.groupname.Equals(gi.groupname)) 
                                            {
                                                obj.groupid = gi.id;
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (MMGroupInfo item in clearGroup)
                            {
                                MM obj = new MM();
                                obj.groupid = item.id;
                                request.mmList.Add(obj);
                            }

                            request.mmList.Reverse();

                            #region 更新组顺序
                            try
                            {
                                if (dic != null)
                                {
                                    sortedGroups = await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                                    int sn = 0;
                                    foreach(MMGroupInfo mmGroupInfo in sortedGroups)
                                    {
                                        if (mmGroupInfo.inf1 == MMConstants.MM_GROPE_TYPE_UNIFY)
                                        {
                                            sn++;
                                            MMGroupReq requestUpdateGroup = new MMGroupReq();
                                            requestUpdateGroup.id = mmGroupInfo.id;
                                            requestUpdateGroup.groupname = mmGroupInfo.groupname;//组名
                                            requestUpdateGroup.action = "2";//更新
                                            requestUpdateGroup.ordersn = sn.ToString();//顺序
                                            requestUpdateGroup.inf1 = mmGroupInfo.inf1;
                                            requestUpdateGroup.inf2 = mmGroupInfo.inf2;
                                            PResult updateGroupResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        }
                                    }
                                    foreach (KeyValuePair<int, string> pair in dic)
                                    {
                                        MMGroupInfo mmGroupInfo = MMGroupHandler.Instance.IsGroupNameExist(pair.Value);
                                        MMGroupReq requestUpdateGroup = new MMGroupReq();
                                        requestUpdateGroup.id = mmGroupInfo.id;
                                        requestUpdateGroup.groupname = pair.Value;//组名
                                        requestUpdateGroup.action = "2";//更新
                                        requestUpdateGroup.ordersn = (pair.Key + sn + 1).ToString();//顺序
                                        requestUpdateGroup.inf1 = mmGroupInfo.inf1;
                                        requestUpdateGroup.inf2 = mmGroupInfo.inf2;
                                        PResult updateGroupResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        //Utils.Data.MMGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        //if (pResult1 != null && pResult1.Code == "0000")
                                        //{
                                        //    gi.action = 2;
                                        //    gi.ordersn = index;
                                        //    await Utils.Data.MMGroupHandler.Instance.AsyncRequestGroups();
                                        //    Utils.Data.MMGroupHandler.Instance.Notify(Utils.Data.MMGroupHandler.FavoriteTypeEnum.UpdateGroup, gi.id, item.groupname);
                                        //}
                                        //index++;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                LogHandler.Error(e);
                            }
                            Utils.Data.MMGroupHandler.Instance.Notify(Utils.Data.MMGroupHandler.FavoriteTypeEnum.UpdateGroup, null, null);
                            #endregion
                            PResult pResult = MMHandler.Instance.mmUpdate(request);
                            if (pResult != null && pResult.IsSuccess())
                            {
                                //CustomMessageBox.Info($"成功！", "批量导入");
                            }
                            else
                            {
                                errorlist.Add(pResult == null ? "服务器无响应" : pResult.Msg);
                            }
                        }
                        else
                        {
                            errorlist.Add("取消导入");
                        }
                    }
                    else
                    {
                        errorlist.Add("没有可导入组");
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }
        private bool checkUnifyGroup(DataTable importDT) 
        {
            bool isUnifyGroup = false;
            if (importDT.Columns.Contains(MMConstants.QuoteModel_ExcelField))
            {
                isUnifyGroup = true;
            }
            return isUnifyGroup;
        }

        private bool checkGroup(DataTable importDT) 
        {
            bool isUnifyGroupOnly = true;
            DataView dv = new DataView(importDT);
            /*if (importDT.Columns.Contains(MMConstants.SecurityID_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.SecurityID_ExcelField);
                if (!isGroupFieldUnique) 
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else 
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }*/
            if (importDT.Columns.Contains(MMConstants.QuoteModel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.QuoteModel_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidBaseQty_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidBaseQty_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrBaseQty_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrBaseQty_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidBaseReSendValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidBaseReSendValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrBaseReSendValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrBaseReSendValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BaseSettleType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BaseSettleType_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidDefenReferMarketType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidDefenReferMarketType_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidDefenReferLevel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidDefenReferLevel_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrDefenReferMarketType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrDefenReferMarketType_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrDefenReferLevel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrDefenReferLevel_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidAlgoDefenValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidAlgoDefenValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrAlgoDefenValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrAlgoDefenValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.AlgoSafeEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.AlgoSafeEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidAlgoSafeValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidAlgoSafeValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.SecondSafeEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.SecondSafeEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.QdmespSafeEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.QdmespSafeEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.QdmespSafeLevel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.QdmespSafeLevel_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidQdmespSafeValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidQdmespSafeValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            //if (importDT.Columns.Contains(MMConstants.OfrAlgoSafeValue_ExcelField))
            //{
            //    bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrAlgoSafeValue_ExcelField);
            //    if (!isGroupFieldUnique)
            //    {
            //        isUnifyGroupOnly = false;
            //        return isUnifyGroupOnly;
            //    }
            //}
            //else
            //{
            //    isUnifyGroupOnly = false;
            //    return isUnifyGroupOnly;
            //}
            if (importDT.Columns.Contains(MMConstants.BidAlgoTrigValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidAlgoTrigValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrAlgoTrigValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrAlgoTrigValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.AlgoMidType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.AlgoMidType_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.AlgoMidOffsetValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.AlgoMidOffsetValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.AlgoValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.AlgoValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.PosFakEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.PosFakEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.PosFakSpread_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.PosFakSpread_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.MaxFloorEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.MaxFloorEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.MaxFloor_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.MaxFloor_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.DefenCDCEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.DefenCDCEnable_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.BidDefenCDCValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.BidDefenCDCValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }
            if (importDT.Columns.Contains(MMConstants.OfrDefenCDCValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMConstants.OfrDefenCDCValue_ExcelField);
                if (!isGroupFieldUnique)
                {
                    isUnifyGroupOnly = false;
                    return isUnifyGroupOnly;
                }
            }
            else
            {
                isUnifyGroupOnly = false;
                return isUnifyGroupOnly;
            }

            return isUnifyGroupOnly;
        }
        private bool groupFieldUnique(DataView dv,string fieldName) 
        {
            DataTable newDt = dv.ToTable(true, fieldName);
            if (newDt.Rows.Count > 1) 
            {
                return false;
            }
            return true;
        }
        private MMOrderParamReq CheckImportMMList(DataRow dr, List<STC.Common.Model.MM.MM> mmList, Dictionary<string, string> severChkDic, out List<ImportErrorInfo> errorList,string sheetName, bool isGroupSheet, bool isTitle)
        {
            
            string securityID = CommonUtils.ConvertToString(dr[MMConstants.SecurityID_ExcelField]);
            if (securityID.Length > 0 && securityID.IndexOf(".") == -1 && securityID.EndsWith(".IB", StringComparison.CurrentCultureIgnoreCase) == false)
            {
                securityID += ".IB";
            }
            string quoteModel = "";
            string bidQuoteModel = "";
            string ofrQuoteModel = "";
            string manualBid = "";
            string manualOfr = "";
            string stepLength = "";
            string bidBaseWidenValue = "";
            string ofrBaseWidenValue = "";
            string baseERReSendType = "";
            string dealWidenLowerTime = "";
            string dealWidenUpperTime = "";
            string algoBaseEnable = "";
            string algoBaseSecurityid = "";
            string algoMidDetailDefined = "";
            if (isGroupSheet)
            {
                quoteModel = CommonUtils.ConvertToString(dr[MMConstants.QuoteModel_ExcelField]);
                if (isTitle)
                {
                    securityID = "";
                }
            }
            else
            {
                bidQuoteModel = CommonUtils.ConvertToString(dr[MMConstants.BidQuoteModel_ExcelField]);
                ofrQuoteModel = CommonUtils.ConvertToString(dr[MMConstants.OfrQuoteModel_ExcelField]);
                manualBid = CommonUtils.ConvertToString(dr[MMConstants.ManualBid_ExcelField]);
                manualOfr = CommonUtils.ConvertToString(dr[MMConstants.ManualOfr_ExcelField]);
                stepLength = CommonUtils.ConvertToString(dr[MMConstants.StepLength_ExcelField]);
                stepLength = string.IsNullOrEmpty(stepLength) ? "" : stepLength;
                bidBaseWidenValue = CommonUtils.ConvertToString(dr[MMConstants.BidBaseWidenValue_ExcelField]);
                bidBaseWidenValue = string.IsNullOrEmpty(bidBaseWidenValue) ? MMConstants.ZERO_VALUE : bidBaseWidenValue;
                ofrBaseWidenValue = CommonUtils.ConvertToString(dr[MMConstants.OfrBaseWidenValue_ExcelField]);
                ofrBaseWidenValue = string.IsNullOrEmpty(ofrBaseWidenValue) ? MMConstants.ZERO_VALUE : ofrBaseWidenValue;

                baseERReSendType = CommonUtils.ConvertToString(dr[MMConstants.BaseERReSendType_ExcelField]);
                // 成交拉宽时长下限
                dealWidenLowerTime = CommonUtils.ConvertToString(dr[MMConstants.DealWidenLowerTime_ExcelField]);
                if (string.IsNullOrEmpty(dealWidenLowerTime))
                {
                    dealWidenLowerTime = MMConstants.DEALWIDEN_VALUE;
                }
                // 成交拉宽时长上限
                dealWidenUpperTime = CommonUtils.ConvertToString(dr[MMConstants.DealWidenUpperTime_ExcelField]);
                if (string.IsNullOrEmpty(dealWidenUpperTime))
                {
                    dealWidenUpperTime = MMConstants.DEALWIDEN_VALUE;
                }
                algoBaseEnable = CommonUtils.ConvertToString(dr[MMConstants.AlgoBaseEnable_ExcelField]);
                algoBaseEnable = string.IsNullOrEmpty(algoBaseEnable) ? MMConstants.NO_VALUE : algoBaseEnable;
                algoBaseSecurityid = CommonUtils.ConvertToString(dr[MMConstants.AlgoBaseSecurityid_ExcelField]);
                if (algoBaseSecurityid.Length > 0 && algoBaseSecurityid.IndexOf(".") == -1 && algoBaseSecurityid.EndsWith(".IB", StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    algoBaseSecurityid += ".IB";
                }

                algoMidDetailDefined = CommonUtils.ConvertToString(dr[MMConstants.AlgoMidDetailDefined_ExcelField]);
            }
            string bidBaseQty = CommonUtils.ConvertToString(dr[MMConstants.BidBaseQty_ExcelField]);
            string ofrBaseQty = CommonUtils.ConvertToString(dr[MMConstants.OfrBaseQty_ExcelField]);
            //string bidBasePriority = CommonUtils.ConvertToString(dr[MMConstants.BidBasePriority_ExcelField]);
            //string ofrBasePriority = CommonUtils.ConvertToString(dr[MMConstants.OfrBasePriority_ExcelField]);
            string bidBaseReSendValue = CommonUtils.ConvertToString(dr[MMConstants.BidBaseReSendValue_ExcelField]);
            bidBaseReSendValue = string.IsNullOrEmpty(bidBaseReSendValue) ? MMConstants.ZERO_VALUE : bidBaseReSendValue;
            string ofrBaseReSendValue = CommonUtils.ConvertToString(dr[MMConstants.OfrBaseReSendValue_ExcelField]);
            ofrBaseReSendValue = string.IsNullOrEmpty(ofrBaseReSendValue) ? MMConstants.ZERO_VALUE : ofrBaseReSendValue;

            string settltype = CommonUtils.ConvertToString(dr[MMConstants.BaseSettleType_ExcelField]);
            string bidAlgoDefenValue = CommonUtils.ConvertToString(dr[MMConstants.BidAlgoDefenValue_ExcelField]);
            bidAlgoDefenValue = string.IsNullOrEmpty(bidAlgoDefenValue) ? MMConstants.ZERO_VALUE : bidAlgoDefenValue;
            string ofrAlgoDefenValue = CommonUtils.ConvertToString(dr[MMConstants.OfrAlgoDefenValue_ExcelField]);
            ofrAlgoDefenValue = string.IsNullOrEmpty(ofrAlgoDefenValue) ? MMConstants.ZERO_VALUE : ofrAlgoDefenValue;


            string algoSafeEnable = CommonUtils.ConvertToString(dr[MMConstants.AlgoSafeEnable_ExcelField]);
            algoSafeEnable = string.IsNullOrEmpty(algoSafeEnable) ? MMConstants.NO_VALUE : algoSafeEnable;
            string bidAlgoSafeValue = CommonUtils.ConvertToString(dr[MMConstants.BidAlgoSafeValue_ExcelField]);
            bidAlgoSafeValue = string.IsNullOrEmpty(bidAlgoSafeValue) ? MMConstants.ZERO_VALUE : bidAlgoSafeValue;
            string secondSafeEnable = CommonUtils.ConvertToString(dr[MMConstants.SecondSafeEnable_ExcelField]);
            secondSafeEnable = string.IsNullOrEmpty(secondSafeEnable) ? MMConstants.NO_VALUE : secondSafeEnable;
            string qdmespSafeEnable = CommonUtils.ConvertToString(dr[MMConstants.QdmespSafeEnable_ExcelField]);
            qdmespSafeEnable = string.IsNullOrEmpty(qdmespSafeEnable) ? MMConstants.NO_VALUE : qdmespSafeEnable;
            string qdmespSafeLevel = CommonUtils.ConvertToString(dr[MMConstants.QdmespSafeLevel_ExcelField]);
            string bidQdmespSafeValue = CommonUtils.ConvertToString(dr[MMConstants.BidQdmespSafeValue_ExcelField]);
            bidQdmespSafeValue = string.IsNullOrEmpty(bidQdmespSafeValue) ? MMConstants.ZERO_VALUE : bidQdmespSafeValue;




            //string ofrAlgoSafeValue = CommonUtils.ConvertToString(dr[MMConstants.OfrAlgoSafeValue_ExcelField]);
            //ofrAlgoSafeValue = string.IsNullOrEmpty(ofrAlgoSafeValue) ? MMConstants.ZERO_VALUE : ofrAlgoSafeValue;
            string bidAlgoTrigValue = CommonUtils.ConvertToString(dr[MMConstants.BidAlgoTrigValue_ExcelField]);
            bidAlgoTrigValue = string.IsNullOrEmpty(bidAlgoTrigValue) ? MMConstants.ZERO_VALUE : bidAlgoTrigValue;
            string ofrAlgoTrigValue = CommonUtils.ConvertToString(dr[MMConstants.OfrAlgoTrigValue_ExcelField]);
            ofrAlgoTrigValue = string.IsNullOrEmpty(ofrAlgoTrigValue) ? MMConstants.ZERO_VALUE : ofrAlgoTrigValue;
            //string algoEnable = CommonUtils.ConvertToString(dr[MMConstants.AlgoEnable_ExcelField]);
            //algoEnable = string.IsNullOrEmpty(algoEnable) ? "否" : algoEnable;
            
            string algoMidType = CommonUtils.ConvertToString(dr[MMConstants.AlgoMidType_ExcelField]);
            string algoMidOffsetValue = CommonUtils.ConvertToString(dr[MMConstants.AlgoMidOffsetValue_ExcelField]);
            algoMidOffsetValue = string.IsNullOrEmpty(algoMidOffsetValue) ? MMConstants.ZERO_VALUE : algoMidOffsetValue;
            string algoValue = CommonUtils.ConvertToString(dr[MMConstants.AlgoValue_ExcelField]);
            //string algoCDCEnable = CommonUtils.ConvertToString(dr[MMConstants.AlgoCDCEnable_ExcelField]);
            //algoCDCEnable = string.IsNullOrEmpty(algoCDCEnable) ? "否" : algoCDCEnable;
            //string algoValue = CommonUtils.ConvertToString(dr[MMConstants.AlgoValue_ExcelField]);
            //string algoCDCEnable = CommonUtils.ConvertToString(dr[MMConstants.AlgoCDCEnable_ExcelField]);
            //去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
            //string posLmtEnable = CommonUtils.ConvertToString(dr[MMConstants.PosLmtEnable_ExcelField]);
            //posLmtEnable = string.IsNullOrEmpty(posLmtEnable) ? MMConstants.NO_VALUE : posLmtEnable;
            //string bidPosValue = CommonUtils.ConvertToString(dr[MMConstants.BidPosValue_ExcelField]);
            //bidPosValue = string.IsNullOrEmpty(bidPosValue) ? MMConstants.ZERO_VALUE : bidPosValue;
            //string ofrPosValue = CommonUtils.ConvertToString(dr[MMConstants.OfrPosValue_ExcelField]);
            //ofrPosValue = string.IsNullOrEmpty(ofrPosValue) ? MMConstants.ZERO_VALUE : ofrPosValue;
            string posFakEnable = CommonUtils.ConvertToString(dr[MMConstants.PosFakEnable_ExcelField]);
            posFakEnable = string.IsNullOrEmpty(posFakEnable) ? MMConstants.NO_VALUE : posFakEnable;

            string posFakSpread = CommonUtils.ConvertToString(dr[MMConstants.PosFakSpread_ExcelField]);

            string maxFloorEnable = CommonUtils.ConvertToString(dr[MMConstants.MaxFloorEnable_ExcelField]);
            maxFloorEnable = string.IsNullOrEmpty(maxFloorEnable) ? MMConstants.NO_VALUE : maxFloorEnable;

            string maxFloor = CommonUtils.ConvertToString(dr[MMConstants.MaxFloor_ExcelField]);

            string defenCDCEnable = CommonUtils.ConvertToString(dr[MMConstants.DefenCDCEnable_ExcelField]);
            defenCDCEnable = string.IsNullOrEmpty(defenCDCEnable) ? MMConstants.NO_VALUE : defenCDCEnable;

            string bidDefenCDCValue = CommonUtils.ConvertToString(dr[MMConstants.BidDefenCDCValue_ExcelField]);
            string ofrDefenCDCValue = CommonUtils.ConvertToString(dr[MMConstants.OfrDefenCDCValue_ExcelField]);

            string bidDefenReferMarketType = CommonUtils.ConvertToString(dr[MMConstants.BidDefenReferMarketType_ExcelField]);
            string bidDefenReferLevel = CommonUtils.ConvertToString(dr[MMConstants.BidDefenReferLevel_ExcelField]);

            string ofrDefenReferMarketType = CommonUtils.ConvertToString(dr[MMConstants.OfrDefenReferMarketType_ExcelField]);
            string ofrDefenReferLevel = CommonUtils.ConvertToString(dr[MMConstants.OfrDefenReferLevel_ExcelField]);

            string wallEnable = CommonUtils.ConvertToString(dr[MMConstants.WallEnable_ExcelField]);
            wallEnable = string.IsNullOrEmpty(wallEnable) ? MMConstants.NO_VALUE : wallEnable;
            string exeBondFollow = "";
            if (!isGroupSheet)
            {
                exeBondFollow = CommonUtils.ConvertToString(dr[MMConstants.ExeBondFollow_ExcelField]);
            }

            errorList = new List<ImportErrorInfo>();

            // 债券
            ImportErrorInfo error = null;
            if (!isTitle)
            {
                bool checkWall = false;
                if (!isGroupSheet) 
                {
                    if (wallEnable.Equals(MMConstants.YES_VALUE)) 
                    {
                        checkWall = true;
                    }
                }
                error = CheckSecurityID(securityID, mmList, severChkDic, sheetName, checkWall);
                if (error != null)
                    errorList.Add(error);
            }
            if (isGroupSheet) 
            {
                error = CheckGroupQuoteModel(securityID, quoteModel, sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                // 策略类型Bid
                error = CheckQuoteModel(securityID, "Bid", bidQuoteModel, sheetName);
                if (error != null) errorList.Add(error);

                // 策略类型Ofr
                error = CheckQuoteModel(securityID, "Ofr", ofrQuoteModel, sheetName);
                if (error != null) errorList.Add(error);
            }

            // 报量Bid(KW)
            if (MMConstants.API_QUOTEMODEL == bidQuoteModel)
            {
                if (string.IsNullOrEmpty(bidBaseQty) == false)
                {
                    error = CheckBaseQty(securityID, "Bid", bidBaseQty, sheetName);
                    if (error != null)
                    {
                        bidBaseQty = string.Empty;
                    }
                }
            }
            else
            {
                if (!isTitle)
                {
                    error = CheckBaseQty(securityID, "Bid", bidBaseQty, sheetName);
                    if (error != null) errorList.Add(error);
                }

            }


            // 报量Bid(KW)
            if (MMConstants.API_QUOTEMODEL == ofrQuoteModel)
            {
                if (string.IsNullOrEmpty(ofrBaseQty) == false)
                {
                    error = CheckBaseQty(securityID, "Ofr", ofrBaseQty, sheetName);
                    if (error != null)
                    {
                        ofrBaseQty = string.Empty;
                    }
                }
            }
            else
            {
                if (!isTitle)
                {
                    error = CheckBaseQty(securityID, "Ofr", ofrBaseQty, sheetName);
                    if (error != null) errorList.Add(error);
                }
            }
            if (!isGroupSheet)
            {
                // 手动报价Bid(%)
                if (MMConstants.MANUAL_QUOTEMODEL == bidQuoteModel)
                {
                    error = CheckManualPrice(securityID, "Bid", manualBid, sheetName);
                    if (error != null) errorList.Add(error);
                }
                else
                {
                    if (string.IsNullOrEmpty(manualBid) == false)
                    {
                        error = CheckManualPrice(securityID, "Bid", manualBid, sheetName);
                        if (error != null)
                        {
                            manualBid = string.Empty;
                        }
                    }
                }

                //手动报价Ofr(%)
                if (MMConstants.MANUAL_QUOTEMODEL == ofrQuoteModel)
                {
                    error = CheckManualPrice(securityID, "Ofr", manualOfr, sheetName);
                    if (error != null) errorList.Add(error);
                }
                else
                {
                    if (string.IsNullOrEmpty(manualOfr) == false)
                    {
                        error = CheckManualPrice(securityID, "Ofr", manualOfr, sheetName);
                        if (error != null)
                        {
                            manualOfr = string.Empty;
                        }
                    }
                }
                if (MMConstants.MANUAL_QUOTEMODEL == ofrQuoteModel || MMConstants.MANUAL_QUOTEMODEL == bidQuoteModel)
                {
                    error = CheckValueStep(securityID, "", "调整步长", stepLength, "(bp)设置错误，取值需要在[0.01,99.99]范围内", 0.01m, 99.99m, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(stepLength) == false)
                    {
                        error = CheckValueStep(securityID, "", "调整步长", stepLength, "(bp)设置错误，取值需要在[0.01,99.99]范围内", 0.01m, 99.99m, sheetName);
                        if (error != null)
                        {
                            errorList.Add(error);
                        }
                    }
                }
            }
            ////报价优先级Bid
            //error = CheckBasePriority(securityID, "Bid", bidBasePriority);
            //if (error != null) errorList.Add(error);

            ////报价优先级Bid
            //error = CheckBasePriority(securityID, "Ofr", ofrBasePriority);
            //if (error != null) errorList.Add(error);

            //重发阈值Bid(bp)
            error = CheckValue(securityID, "Bid", "重发阈值", bidBaseReSendValue, "(bp)设置错误，取值需要在[0, 1000]范围内", sheetName);
            if (error != null) errorList.Add(error);

            //重发阈值Ofr(bp)
            error = CheckValue(securityID, "Ofr", "重发阈值", ofrBaseReSendValue, "(bp)设置错误，取值需要在[0, 1000]范围内", sheetName);
            if (error != null) errorList.Add(error);
            if (!isGroupSheet)
            {
                //成交拉宽Bid(bp)
                error = CheckValue(securityID, "Bid", "成交拉宽", bidBaseWidenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);

                //成交拉宽Ofr(bp)
                error = CheckValue(securityID, "Ofr", "成交拉宽", ofrBaseWidenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);

                // 部分成交后报量
                error = CheckERReSendType(securityID, baseERReSendType, sheetName);
                if (error != null) errorList.Add(error);

                // 成交拉宽时长下限
                error = CheckDealWidenLowerTime(securityID, dealWidenLowerTime, sheetName);
                if (error != null) errorList.Add(error);
                // 成交拉宽时长上限
                if (error == null)
                {
                    error = CheckDealWidenUpperTime(securityID, dealWidenLowerTime, dealWidenUpperTime, sheetName);
                    if (error != null) errorList.Add(error);
                }
            }
            // 清算速度
            error = CheckSettlType(securityID, settltype, sheetName);
            if (error != null) errorList.Add(error);

            if (MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel)
            {
                //防守点差Bid
                error = CheckValue(securityID, "Bid", "防守点差", bidAlgoDefenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidAlgoDefenValue) == false)
                {
                    error = CheckValue(securityID, "Bid", "防守点差", bidAlgoDefenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                    if (error != null)
                    {
                        bidAlgoDefenValue = MMConstants.ZERO_VALUE;
                    }
                }
            }

            if (MMConstants.DEFEND_QUOTEMODEL == ofrQuoteModel)
            {
                //防守点差Ofr
                error = CheckValue(securityID, "Ofr", "防守点差", ofrAlgoDefenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(ofrAlgoDefenValue) == false)
                {
                    error = CheckValue(securityID, "Ofr", "防守点差", ofrAlgoDefenValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                    if (error != null)
                    {
                        ofrAlgoDefenValue = MMConstants.ZERO_VALUE;
                    }
                }
            }
            //是否启用全市场反向最优安全点
            error = CheckYesOrNo(securityID, "是否启用全市场反向最优安全点", algoSafeEnable, sheetName);
            if (error != null) errorList.Add(error);

            if (MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel)
            {
                //安全点差Bid
                error = CheckDefenCDCValue(securityID, "Bid", "安全点差", bidAlgoSafeValue, "(bp)设置错误，取值需要在[-1000,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidAlgoSafeValue) == false)
                {
                    error = CheckDefenCDCValue(securityID, "Bid", "安全点差", bidAlgoSafeValue, "(bp)设置错误，取值需要在[-1000,1000]范围内", sheetName);
                    if (error != null)
                    {
                        bidAlgoSafeValue = MMConstants.ZERO_VALUE;
                    }
                }
            }
            //是否启用全市场同向次最优安全点
            error = CheckYesOrNo(securityID, "是否启用全市场同向次最优安全点", secondSafeEnable, sheetName);
            if (error != null) errorList.Add(error);
            //是否启用双边同向安全点
            error = CheckYesOrNo(securityID, "是否启用双边同向安全点", qdmespSafeEnable, sheetName);
            if (error != null) errorList.Add(error);

            //双边同向安全点档位
            error = CheckDefenReferLevel(securityID, qdmespSafeLevel, sheetName, "", "双边同向安全点");
            if (error != null) errorList.Add(error);

            //双边同向安全点差
            if (MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel)
            {
                error = CheckDefenCDCValue(securityID, "", "双边同向安全点差", bidQdmespSafeValue, "(bp)设置错误，取值需要在[-1000,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidQdmespSafeValue) == false)
                {
                    error = CheckDefenCDCValue(securityID, "", "双边同向安全点差", bidQdmespSafeValue, "(bp)设置错误，取值需要在[-1000,1000]范围内", sheetName);
                    if (error != null)
                    {
                        bidQdmespSafeValue = MMConstants.ZERO_VALUE;
                    }
                }
            }

            //if (MMConstants.DEFEND_QUOTEMODEL == ofrQuoteModel)
            //{
            //    //安全点差Ofr
            //    error = CheckValue(securityID, "Ofr", "安全点差", ofrAlgoSafeValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
            //    if (error != null) errorList.Add(error);
            //}
            //else
            //{
            //    if(string.IsNullOrEmpty(ofrAlgoSafeValue) == false)
            //    {
            //        error = CheckValue(securityID, "Ofr", "安全点差", ofrAlgoSafeValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
            //        if (error != null)
            //        {
            //            ofrAlgoSafeValue = MMConstants.ZERO_VALUE;
            //        }
            //    }
            //}

            if (MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel)
            {
                //触发安全点报价点差Bid
                error = CheckValue(securityID, "Bid", "触发安全点报价点差", bidAlgoTrigValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidAlgoTrigValue) == false)
                {
                    error = CheckValue(securityID, "Bid", "触发安全点报价点差", bidAlgoTrigValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                    if (error != null)
                    {
                        bidAlgoTrigValue = MMConstants.ZERO_VALUE;
                    }
                }
            }

            if (MMConstants.DEFEND_QUOTEMODEL == ofrQuoteModel)
            {
                // 触发安全点报价点差Ofr
                error = CheckValue(securityID, "Ofr", "触发安全点报价点差", ofrAlgoTrigValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(ofrAlgoTrigValue) == false)
                {
                    error = CheckValue(securityID, "Ofr", "触发安全点报价点差", ofrAlgoTrigValue, "(bp)设置错误，取值需要在[0,1000]范围内", sheetName);
                    if (error != null)
                    {
                        ofrAlgoTrigValue = MMConstants.ZERO_VALUE;
                    }
                }
            }


            ////是否启用算法计算
            //error = CheckYesOrNo(securityID, "是否启用算法计算", algoEnable, "设置错误，可选范围：是，否");
            //if (error != null) errorList.Add(error);
            if (!isGroupSheet)
            {
                //是否使用基准券
                error = CheckYesOrNo(securityID, "是否使用基准券", algoBaseEnable, sheetName);
                if (error != null) errorList.Add(error);

                // 基准券
                if (MMConstants.YES_VALUE == algoBaseEnable)
                {
                    error = CheckBaseSecurityID(securityID, algoBaseSecurityid, sheetName);
                    if (error != null) errorList.Add(error);
                }
                else
                {
                    if (string.IsNullOrEmpty(algoBaseSecurityid) == false)
                    {
                        error = CheckBaseSecurityID(securityID, algoBaseSecurityid, sheetName);
                        if (error != null)
                        {
                            algoBaseSecurityid = string.Empty;
                        }
                    }
                }
            }
            // 参考中间价
            if (isGroupSheet)
            {
                error = CheckGroupAlgoMidType(securityID, algoMidType, sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                error = CheckAlgoMidType(securityID, algoMidType, sheetName);
                if (error != null) errorList.Add(error);

                //自定义中间价
                if (MMConstants.Cus_ALGOMIDTYPE == algoMidType)
                {
                    error = CheckValueAlgoMidDetail(securityID, "", "自定义中间价", algoMidDetailDefined, "设置错误，取值需要在[0,1000]范围内", sheetName);
                    if (error != null) errorList.Add(error);
                }
                else
                {
                    if (string.IsNullOrEmpty(algoMidDetailDefined) == false)
                    {
                        error = CheckValueAlgoMidDetail(securityID, "", "自定义中间价", algoMidDetailDefined, "设置错误，取值需要在[0,1000]范围内", sheetName);
                        if (error != null)
                        {
                            algoMidDetailDefined = string.Empty;
                        }
                    }
                }
            }

            //中间价偏离(bp)
            error = CheckValue(securityID, "", "中间价偏离(bp)", algoMidOffsetValue, "设置错误，取值需要在[0,1000]范围内", sheetName);
            if (error != null) errorList.Add(error);

            // 算法点差
            if (MMConstants.ALGORITHM_QUOTEMODEL == bidQuoteModel || MMConstants.ALGORITHM_QUOTEMODEL == ofrQuoteModel)
            {
                error = CheckPostiveValue(securityID, "", "算法点差", algoValue, sheetName);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(algoValue) == false)
                {
                    error = CheckPostiveValue(securityID, "", "算法点差", algoValue, sheetName);
                    if (error != null)
                    {
                        algoValue = string.Empty;
                    }
                }
            }

            //// 策略无报价，是否采用中债估值报价
            //error = CheckYesOrNo(securityID, "策略无报价，是否采用中债估值报价", algoCDCEnable, "设置错误，可选范围：是，否");
            //if (error != null) errorList.Add(error);

            // 估值偏离(bp)
            // 估值点差

            //// 是否允许报价更激进
            //error = CheckYesOrNo(securityID, "是否允许报价更激进", posLmtEnable);
            //if (error != null) errorList.Add(error);

            //// 更优报价点差Bid
            //if (MMConstants.YES_VALUE == posLmtEnable)
            //{
            //    error = CheckValue(securityID, "Bid", "更优报价点差", bidPosValue, "设置错误，取值需要在[0,1000]范围内");
            //    if (error != null) errorList.Add(error);
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(bidPosValue) == false)
            //    {
            //        error = CheckValue(securityID, "Bid", "更优报价点差", bidPosValue, "设置错误，取值需要在[0,1000]范围内");
            //        if (error != null)
            //        {
            //            bidPosValue = MMConstants.ZERO_VALUE; 
            //        }
            //    }
            //}

            //// 更新报价点差Ofr
            //if (MMConstants.YES_VALUE == posLmtEnable)
            //{
            //    error = CheckValue(securityID, "Ofr", "更优报价点差", ofrPosValue, "设置错误，取值需要在[0,1000]范围内");
            //    if (error != null) errorList.Add(error);
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(ofrPosValue) == false)
            //    {
            //        error = CheckValue(securityID, "Ofr", "更优报价点差", ofrPosValue, "设置错误，取值需要在[0,1000]范围内");
            //        if (error != null)
            //        {
            //            ofrPosValue = MMConstants.ZERO_VALUE;
            //        }
            //    }
            //}

            //是否报反向内部单
            error = CheckYesOrNo(securityID, "是否报反向内部单", posFakEnable, sheetName);
            if (error != null) errorList.Add(error);
            if ("是".Equals(posFakEnable))
            {
                if (string.IsNullOrEmpty(posFakSpread)) 
                {
                    posFakSpread = "0";
                }
                else
                {
                    error = CheckValueStep(securityID, "", "内部单点差", posFakSpread, "(bp)设置错误，取值需要在[0,99.99]范围内", 0, 99.99m, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }
            else 
            {
                if (!string.IsNullOrEmpty(posFakSpread)) 
                {
                    error = CheckValueStep(securityID, "", "内部单点差", posFakSpread, "(bp)设置错误，取值需要在[0,99.99]范围内", 0, 99.99m, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }
            //是否使用冰山量
            error = CheckYesOrNo(securityID, "是否使用冰山量", maxFloorEnable, sheetName);
            if (error != null) errorList.Add(error);
            if ("是".Equals(maxFloorEnable))
            {
                if (string.IsNullOrEmpty(maxFloorEnable))
                {
                    maxFloorEnable = "0";
                }
                else
                {
                    error = CheckMaxFloor(securityID, maxFloor, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(maxFloor))
                {
                    error = CheckMaxFloor(securityID, maxFloor, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }


            //是否防守策略无可报价格时锚定中债估值
            error = CheckYesOrNo(securityID, "是否防守策略无可报价格时锚定中债估值", defenCDCEnable, sheetName);
            if (error != null) errorList.Add(error);
            if ("是".Equals(defenCDCEnable))
            {
                if (string.IsNullOrEmpty(defenCDCEnable))
                {
                    defenCDCEnable = "0";
                }
                else
                {
                    error = CheckDefenCDCValue(securityID,"Bid", "估值拉宽", bidDefenCDCValue, "设置错误，取值需要在[-1000, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                    error = CheckDefenCDCValue(securityID, "Ofr", "估值拉宽", ofrDefenCDCValue, "设置错误，取值需要在[-1000, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(bidDefenCDCValue))
                {
                    error = CheckDefenCDCValue(securityID, "Bid", "估值拉宽", bidDefenCDCValue, "设置错误，取值需要在[-1000, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
                if (!string.IsNullOrEmpty(ofrDefenCDCValue))
                {
                    error = CheckDefenCDCValue(securityID, "Ofr", "估值拉宽", ofrDefenCDCValue, "设置错误，取值需要在[-1000, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }

            if ((!string.IsNullOrEmpty(quoteModel) && MMConstants.DEFEND_QUOTEMODEL == quoteModel)
                || (!string.IsNullOrEmpty(bidQuoteModel) && MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel))
            {
                // Bid防守参考价
                error = CheckDefenReferMarketType(securityID, bidDefenReferMarketType, sheetName, "Bid");
                if (error != null) errorList.Add(error);

                error = CheckDefenReferLevel(securityID, bidDefenReferLevel, sheetName, "Bid", "防守参考市场价");
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidDefenReferMarketType))
                {
                    bidDefenReferMarketType = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                }
                if (string.IsNullOrEmpty(bidDefenReferLevel)) {
                    bidDefenReferLevel = MMConstants.LEVEL1;
                }
                   
            }
            if ((!string.IsNullOrEmpty(quoteModel) && MMConstants.DEFEND_QUOTEMODEL == quoteModel)
               || (!string.IsNullOrEmpty(ofrQuoteModel) && MMConstants.DEFEND_QUOTEMODEL == ofrQuoteModel))
            {
                // Ofr防守参考价
                error = CheckDefenReferMarketType(securityID, ofrDefenReferMarketType, sheetName, "Ofr");
                if (error != null) errorList.Add(error);

                error = CheckDefenReferLevel(securityID, ofrDefenReferLevel, sheetName, "Ofr", "防守参考市场价");
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(ofrDefenReferMarketType)) {
                    ofrDefenReferMarketType = MMConstants.QDMESP_DEFENREFERMARKETTYPE;
                }
                if (string.IsNullOrEmpty(ofrDefenReferLevel))
                {
                    ofrDefenReferLevel = MMConstants.LEVEL1;
                }     
            }
            //是否使用墙
            error = CheckYesOrNo(securityID, "是否使用墙", wallEnable, sheetName);
            if (error != null) errorList.Add(error);

            if (!isGroupSheet)
            {
                error = CheckExeFollow(securityID, exeBondFollow, sheetName);
                if (error != null) errorList.Add(error);
            }

            if (errorList != null && errorList.Count > 0)
                return null;

            MMOrderParamReq mmOrderParamReq = new MMOrderParamReq();
            mmOrderParamReq.securityid = securityID;
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (bondsInfoPo != null)
            {
                mmOrderParamReq.symbol = bondsInfoPo.Short_Name;
            }
            if (MMConstants.DEFEND_QUOTEMODEL == quoteModel) mmOrderParamReq.bidQuoteModel = "Defend";
            else if (MMConstants.ALGORITHM_QUOTEMODEL == quoteModel) mmOrderParamReq.bidQuoteModel = "Algorithm";

            if (MMConstants.DEFEND_QUOTEMODEL == quoteModel) mmOrderParamReq.ofrQuoteModel = "Defend";
            else if (MMConstants.ALGORITHM_QUOTEMODEL == quoteModel) mmOrderParamReq.ofrQuoteModel = "Algorithm";
            //mmOrderParamReq.bidQuoteModel = "自动" == bidQuoteModel ? "2" : "1";
            if (MMConstants.DEFEND_QUOTEMODEL == bidQuoteModel) mmOrderParamReq.bidQuoteModel = "Defend";
            else if (MMConstants.ALGORITHM_QUOTEMODEL == bidQuoteModel) mmOrderParamReq.bidQuoteModel = "Algorithm";
            else if (MMConstants.MANUAL_QUOTEMODEL == bidQuoteModel) mmOrderParamReq.bidQuoteModel = "Manual";
            else if (MMConstants.API_QUOTEMODEL == bidQuoteModel) mmOrderParamReq.bidQuoteModel = "API";
            //mmOrderParamReq.ofrQuoteModel = "自动" == ofrQuoteModel ? "2" : "1";
            if (MMConstants.DEFEND_QUOTEMODEL == ofrQuoteModel) mmOrderParamReq.ofrQuoteModel = "Defend";
            else if (MMConstants.ALGORITHM_QUOTEMODEL == ofrQuoteModel) mmOrderParamReq.ofrQuoteModel = "Algorithm";
            else if (MMConstants.MANUAL_QUOTEMODEL == ofrQuoteModel) mmOrderParamReq.ofrQuoteModel = "Manual";
            else if (MMConstants.API_QUOTEMODEL == ofrQuoteModel) mmOrderParamReq.ofrQuoteModel = "API";
            mmOrderParamReq.bidBaseQty = bidBaseQty;
            mmOrderParamReq.ofrBaseQty = ofrBaseQty;
            mmOrderParamReq.manualBid = manualBid;
            mmOrderParamReq.manualOfr = manualOfr;
            mmOrderParamReq.stepLength = stepLength;
            //mmOrderParamReq.bidBasePriority = "算法" == bidBasePriority ? "2" : "1";
            //mmOrderParamReq.ofrBasePriority = "算法" == ofrBasePriority ? "2" : "1";
            mmOrderParamReq.bidBaseReSendValue = bidBaseReSendValue;
            mmOrderParamReq.ofrBaseReSendValue = ofrBaseReSendValue;
            mmOrderParamReq.bidBaseWidenValue = bidBaseWidenValue;
            mmOrderParamReq.ofrBaseWidenValue = ofrBaseWidenValue;
            mmOrderParamReq.BaseERReSendType = MMConstants.LEAVE_BASEERRESENDTYPE == baseERReSendType ? "2" : "1";
            //mmOrderParamReq.bidAlgoTrigValue = bidAlgoTrigValue;
            //mmOrderParamReq.algoMidType = algoMidType;
            mmOrderParamReq.dealWidenLowerTime = dealWidenLowerTime;
            mmOrderParamReq.dealWidenUpperTime = dealWidenUpperTime;
            mmOrderParamReq.baseSettleType = MMConstants.T1_SETTLTYPE == settltype ? "2" : "1";
            mmOrderParamReq.bidAlgoDefenValue = bidAlgoDefenValue;
            mmOrderParamReq.ofrAlgoDefenValue = ofrAlgoDefenValue;

            mmOrderParamReq.algoSafeEnable = MMConstants.YES_VALUE == algoSafeEnable ? "1" : "0"; 
            mmOrderParamReq.bidAlgoSafeValue = bidAlgoSafeValue;
            mmOrderParamReq.ofrAlgoSafeValue = bidAlgoSafeValue;
            mmOrderParamReq.secondSafeEnable = MMConstants.YES_VALUE == secondSafeEnable ? "1" : "0"; 
            mmOrderParamReq.qdmespSafeEnable = MMConstants.YES_VALUE == qdmespSafeEnable ? "1" : "0";
            if (MMConstants.LEVEL1 == qdmespSafeLevel) mmOrderParamReq.qdmespSafeLevel = "1";
            else if (MMConstants.LEVEL2 == qdmespSafeLevel) mmOrderParamReq.qdmespSafeLevel = "2";
            else if (MMConstants.LEVEL3 == qdmespSafeLevel) mmOrderParamReq.qdmespSafeLevel = "3";
            else if (MMConstants.LEVEL4 == qdmespSafeLevel) mmOrderParamReq.qdmespSafeLevel = "4";
            else if (MMConstants.LEVEL5 == qdmespSafeLevel) mmOrderParamReq.qdmespSafeLevel = "5";
            mmOrderParamReq.bidQdmespSafeValue = bidQdmespSafeValue;
            mmOrderParamReq.ofrQdmespSafeValue = bidQdmespSafeValue;


            mmOrderParamReq.bidAlgoTrigValue = bidAlgoTrigValue;
            mmOrderParamReq.ofrAlgoTrigValue = ofrAlgoTrigValue;
            //mmOrderParamReq.algoEnable = "是" == algoEnable ? "1" : "0";
            mmOrderParamReq.algoBaseEnable = MMConstants.YES_VALUE == algoBaseEnable ? "1" : "0";
            mmOrderParamReq.algoBaseSecurityid = algoBaseSecurityid;

            if (MMConstants.Mid_ALGOMIDTYPE == algoMidType) mmOrderParamReq.algoMidType = "1";
            else if (MMConstants.Val_ALGOMIDTYPE == algoMidType) mmOrderParamReq.algoMidType = "2";
            else if (MMConstants.Cus_ALGOMIDTYPE == algoMidType) mmOrderParamReq.algoMidType = "3";
            mmOrderParamReq.algoMidDetailDefined = algoMidDetailDefined;
            mmOrderParamReq.algoMidOffsetValue = string.IsNullOrEmpty(algoMidOffsetValue) ? "0" : algoMidOffsetValue;
            mmOrderParamReq.algoValue = string.IsNullOrEmpty(algoValue) ? "0" : algoValue;
            //mmOrderParamReq.algoCDCEnable = "是" == algoCDCEnable ? "1" : "0";
            //mmOrderParamReq.bidAlgoTrigValue = bidAlgoTrigValue;
            //mmOrderParamReq.algoMidType = algoMidType;
            //mmOrderParamReq.posLmtEnable = MMConstants.YES_VALUE == posLmtEnable ? "1" : "0";
            //mmOrderParamReq.bidPosValue = string.IsNullOrEmpty(bidPosValue) ? "0" : bidPosValue;
            //mmOrderParamReq.ofrPosValue = string.IsNullOrEmpty(ofrPosValue) ? "0" : ofrPosValue;
            mmOrderParamReq.posFakEnable = MMConstants.YES_VALUE == posFakEnable ? "1" : "0"; ;
            mmOrderParamReq.posFakSpread = posFakSpread;
            mmOrderParamReq.maxFloorEnable = MMConstants.YES_VALUE == maxFloorEnable ? "1" : "0";
            mmOrderParamReq.maxFloor = maxFloor;
            mmOrderParamReq.defenCDCEnable = MMConstants.YES_VALUE == defenCDCEnable ? "1" : "0";
            mmOrderParamReq.bidDefenCDCValue = bidDefenCDCValue;
            mmOrderParamReq.ofrDefenCDCValue = ofrDefenCDCValue;
            if (MMConstants.QDMESP_DEFENREFERMARKETTYPE == bidDefenReferMarketType) mmOrderParamReq.bidDefenReferMarketType = "1";
            else if (MMConstants.XBOND_DEFENREFERMARKETTYPE == bidDefenReferMarketType) mmOrderParamReq.bidDefenReferMarketType = "2";
            else if (MMConstants.MARKET_DEFENREFERMARKETTYPE == bidDefenReferMarketType) mmOrderParamReq.bidDefenReferMarketType = "3";
            else if (MMConstants.CFETS_DEFENREFERMARKETTYPE == bidDefenReferMarketType) mmOrderParamReq.bidDefenReferMarketType = "4";

            if (MMConstants.LEVEL1 == bidDefenReferLevel) mmOrderParamReq.bidDefenReferLevel = "1";
            else if (MMConstants.LEVEL2 == bidDefenReferLevel) mmOrderParamReq.bidDefenReferLevel = "2";
            else if (MMConstants.LEVEL3 == bidDefenReferLevel) mmOrderParamReq.bidDefenReferLevel = "3";
            else if (MMConstants.LEVEL4 == bidDefenReferLevel) mmOrderParamReq.bidDefenReferLevel = "4";
            else if (MMConstants.LEVEL5 == bidDefenReferLevel) mmOrderParamReq.bidDefenReferLevel = "5";

            if (MMConstants.QDMESP_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) mmOrderParamReq.ofrDefenReferMarketType = "1";
            else if (MMConstants.XBOND_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) mmOrderParamReq.ofrDefenReferMarketType = "2";
            else if (MMConstants.MARKET_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) mmOrderParamReq.ofrDefenReferMarketType = "3";
            else if (MMConstants.CFETS_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) mmOrderParamReq.ofrDefenReferMarketType = "4";

            if (MMConstants.LEVEL1 == ofrDefenReferLevel) mmOrderParamReq.ofrDefenReferLevel = "1";
            else if (MMConstants.LEVEL2 == ofrDefenReferLevel) mmOrderParamReq.ofrDefenReferLevel = "2";
            else if (MMConstants.LEVEL3 == ofrDefenReferLevel) mmOrderParamReq.ofrDefenReferLevel = "3";
            else if (MMConstants.LEVEL4 == ofrDefenReferLevel) mmOrderParamReq.ofrDefenReferLevel = "4";
            else if (MMConstants.LEVEL5 == ofrDefenReferLevel) mmOrderParamReq.ofrDefenReferLevel = "5";
            mmOrderParamReq.wallEnable = MMConstants.YES_VALUE == wallEnable ? "1" : "0";
            if (mmOrderParamReq.wallEnable.Equals("0"))
            {
                if (!string.IsNullOrEmpty(manualBid))
                {
                    mmOrderParamReq.exeBondPre = "1";
                    mmOrderParamReq.exeManualBid = MMCommonUtils.CalBond(securityID, "4", mmOrderParamReq.baseSettleType, manualBid, "1");
                }
                if (!string.IsNullOrEmpty(manualOfr))
                {
                    mmOrderParamReq.exeBondPre = "1";
                    mmOrderParamReq.exeManualOfr = MMCommonUtils.CalBond(securityID, "4", mmOrderParamReq.baseSettleType, manualOfr, "2");
                }
            }
            mmOrderParamReq.exeBondFollow = "行权".Equals(exeBondFollow) ? "0" : "1";
            mmOrderParamReq.exeBondPre = "行权".Equals(exeBondFollow) ? "0" : "1";

            //含权债跟随
            return mmOrderParamReq;
        }


        private ImportErrorInfo CheckSecurityID(string securityId, List<STC.Common.Model.MM.MM> mmList, Dictionary<string, string> serverDic,string sheetName,bool checkWall)
        {

            if (string.IsNullOrEmpty(securityId))
            {
                ImportErrorInfo securityIDError = new ImportErrorInfo();
                securityIDError.GroupName = string.Format("[{0}]", sheetName);
                securityIDError.SecurityID = securityId;
                securityIDError.ImportErrorResean = "代码不能为空";
                return securityIDError;
            }
            else
            {
                if (securityId.EndsWith(".IB") == false)
                {
                    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                    securityIDError.SecurityID = securityId;
                    securityIDError.ImportErrorResean = "双边做市暂不支持";
                    return securityIDError;
                }
                /*else if (serverDic.ContainsKey(securityId))
                {
                    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    securityIDError.SecurityID = securityId;
                    securityIDError.ImportErrorResean = serverDic[securityId];
                    return securityIDError;
                }*/
                else if (mmList.Find(o => o.securityid == securityId) != null)
                {
                    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                    securityIDError.SecurityID = securityId;
                    securityIDError.ImportErrorResean = "存在重复记录";
                    return securityIDError;
                }
                else
                {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
                    if (CommonUtils.IsCIB(bondsInfoPo) == false)
                    {
                        ImportErrorInfo securityIDError = new ImportErrorInfo();
                        securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        securityIDError.SecurityID = securityId;
                        securityIDError.ImportErrorResean = "双边做市暂不支持";
                        return securityIDError;
                    }
                    else 
                    {
                        //if (checkWall && !string.IsNullOrEmpty(bondsInfoPo.Option_Type) && !bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null)
                        //{
                        //    ImportErrorInfo securityIDError = new ImportErrorInfo();
                        //    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        //    securityIDError.SecurityID = securityId;
                        //    securityIDError.ImportErrorResean = "含权债不能启用墙";
                        //    return securityIDError;
                        //}
                    }
                }
            }
            return null;
        }

        private ImportErrorInfo CheckBaseSecurityID(string securityId, string basesecurityId,string sheetName)
        {
            if (string.IsNullOrEmpty(basesecurityId) || basesecurityId.EndsWith(".IB") == false)
            {
                ImportErrorInfo securityIDError = new ImportErrorInfo();
                securityIDError.GroupName = string.Format("[{0}]", sheetName);
                securityIDError.SecurityID = securityId;
                securityIDError.ImportKey = "基准券";
                securityIDError.ImportErrorResean = "设置错误";
                return securityIDError;
            }
            else
            {
                if (securityId.Equals(basesecurityId)) 
                { 
                    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                    securityIDError.SecurityID = securityId;
                    securityIDError.ImportKey = "基准券";
                    securityIDError.ImportErrorResean = "基准券不能与当前券相同";
                    return securityIDError;
                }
                else {
                    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(basesecurityId);
                    if (CommonUtils.IsCIB(bondsInfoPo) == false)
                    {
                        ImportErrorInfo securityIDError = new ImportErrorInfo();
                        securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        securityIDError.SecurityID = securityId;
                        securityIDError.ImportKey = "基准券";
                        securityIDError.ImportErrorResean = "设置错误";
                        return securityIDError;
                    }
                }
            }

            return null;
        }

        private ImportErrorInfo CheckQuoteModel(string securityId, string side, string quoteModel,string sheetName)
        {
            if (string.IsNullOrEmpty(quoteModel)
                || (MMConstants.MANUAL_QUOTEMODEL != quoteModel && MMConstants.DEFEND_QUOTEMODEL != quoteModel 
                    && MMConstants.ALGORITHM_QUOTEMODEL != quoteModel && MMConstants.API_QUOTEMODEL != quoteModel))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "策略类型";
                error.ImportValue = side;
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.MANUAL_QUOTEMODEL}、{MMConstants.DEFEND_QUOTEMODEL}、{MMConstants.ALGORITHM_QUOTEMODEL}、{MMConstants.API_QUOTEMODEL}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckGroupQuoteModel(string securityId, string quoteModel, string sheetName)
        {
            if (string.IsNullOrEmpty(quoteModel)
                || (MMConstants.DEFEND_QUOTEMODEL != quoteModel
                    && MMConstants.ALGORITHM_QUOTEMODEL != quoteModel))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "策略类型";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.DEFEND_QUOTEMODEL}、{MMConstants.ALGORITHM_QUOTEMODEL}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckBaseQty(string securityId, string side, string BaseQty, string sheetName)
        {
            if (string.IsNullOrEmpty(BaseQty)
                || !CommonUtils.IsNumber(BaseQty, 3, 0)
                || CommonUtils.ConvertToLong(BaseQty) < 1
                || CommonUtils.ConvertToLong(BaseQty) > 999)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "报量";
                error.ImportValue = side;
                error.ImportErrorResean = "(KW)设置错误，取值需要在[1, 999]范围内";
                return error;
            }

            PResult result = MMCommonUtils.QueryCirculSizeBySecurity(securityId, false);
            //RiskQtyBySecurityReplyPo riskQtyPo = MMCommonUtils.QueryRiskQtyBySecurity(securityId,false);

            if (result == null)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "报量";
                error.ImportValue = side;
                error.ImportErrorResean = $"获取债券余额有误";
                return error;
            }

            //if (riskQtyPo == null || riskQtyPo.Data == null)
            //{
            //    ImportErrorInfo error = new ImportErrorInfo();
            //    error.SecurityID = securityId;
            //    error.ImportKey = "报量";
            //    error.ImportValue = side;
            //    error.ImportErrorResean = $"获取风控最值有误";
            //    return error;
            //}

            if (CommonUtils.ConvertToDecimalOrNull(result.Info1) != null && CommonUtils.ConvertToDecimalOrNull(result.Info1) > 0)
            {
                decimal maxQty = (decimal)CommonUtils.ConvertToDecimalOrNull(result.Info1);
                    //(decimal)CommonUtils.ConvertToDecimalOrNull(result.Info1) /10;
                if (CommonUtils.ConvertToLong(BaseQty) >= maxQty)
                {
                    ImportErrorInfo error = new ImportErrorInfo();
                    error.GroupName = string.Format("[{0}]", sheetName);
                    error.SecurityID = securityId;
                    error.ImportKey = "报量";
                    error.ImportValue = side;
                    error.ImportErrorResean = $"(KW)设置错误，取值需要小于债券余额[{maxQty}kw]";
                    return error;
                }
            }
            else
            {
                if (!result.Code.Equals("0000") && (CommonUtils.ConvertToDecimalOrNull(result.Info1) == null || CommonUtils.ConvertToDecimalOrNull(result.Info1)  == decimal.Zero)
                    && !result.Code.Equals("MM_10003"))
                {
                    ImportErrorInfo error = new ImportErrorInfo();
                    error.GroupName = string.Format("[{0}]", sheetName);
                    error.SecurityID = securityId;
                    error.ImportKey = "报量";
                    error.ImportValue = side;
                    error.ImportErrorResean = result.Msg;
                    return error;
                }

            }
            return null;
        }

        private ImportErrorInfo CheckManualPrice(string securityId, string side, string basePrice, string sheetName)
        {
            if (string.IsNullOrEmpty(basePrice)
                || (!CommonUtils.IsNumber(basePrice, 4, 4) && !CommonUtils.IsNegative(basePrice, 3, 4))
                || CommonUtils.ConvertToDecimal(basePrice) <= -100
                || CommonUtils.ConvertToDecimal(basePrice) > 9999)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "手动报价";
                error.ImportValue = side;
                error.ImportErrorResean = "(%)设置错误，取值需要在(-100, 9999]范围内";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckValue(string securityid, string side, string key, string value, string reason, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 4, 2)
                || CommonUtils.ConvertToDecimal(value) < 0
                 || CommonUtils.ConvertToDecimal(value) > 1000)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportValue = side;
                error.ImportErrorResean = reason;
                return error;
            }
            return null;
        }
        private ImportErrorInfo CheckValueAlgoMidDetail(string securityid, string side, string key, string value, string reason, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 4, 4)
                || CommonUtils.ConvertToDecimal(value) < 0
                 || CommonUtils.ConvertToDecimal(value) > 1000)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportValue = side;
                error.ImportErrorResean = reason;
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckValueStep(string securityid, string side, string key, string value, string reason,decimal min,decimal max, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 2, 2)
                || CommonUtils.ConvertToDecimal(value) < min
                 || CommonUtils.ConvertToDecimal(value) > max
                 || !MMCommonUtils.CheckDecimalValid(value, max, min, 2))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportValue = side;
                error.ImportErrorResean = reason;
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckPostiveValue(string securityid, string side, string key, string value, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 4, 2)
                || CommonUtils.ConvertToDecimal(value) <= 0
                 || CommonUtils.ConvertToDecimal(value) > 1000)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportValue = side;
                error.ImportErrorResean = "设置错误，取值需要在(0,1000]范围内";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckERReSendType(string securityid, string ERReSendType, string sheetName)
        {
            if (string.IsNullOrEmpty(ERReSendType)
                || (MMConstants.QTY_BASEERRESENDTYPE != ERReSendType && MMConstants.LEAVE_BASEERRESENDTYPE != ERReSendType))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "部分成交后报量";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.QTY_BASEERRESENDTYPE}，{MMConstants.LEAVE_BASEERRESENDTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckSettlType(string securityid, string SettlType, string sheetName)
        {
            if (string.IsNullOrEmpty(SettlType)
               || (MMConstants.T0_SETTLTYPE != SettlType && MMConstants.T1_SETTLTYPE != SettlType))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "清算速度";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.T0_SETTLTYPE}, {MMConstants.T1_SETTLTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckYesOrNo(string securityid, string key, string value, string sheetName)
        {
            if (MMConstants.YES_VALUE != value && MMConstants.NO_VALUE != value)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.YES_VALUE}，{MMConstants.NO_VALUE}";
                return error;
            }
            return null;
        }
        private ImportErrorInfo CheckGroupAlgoMidType(string securityid, string AlgoMidType, string sheetName)
        {
            if (MMConstants.Val_ALGOMIDTYPE != AlgoMidType)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "参考中间价";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.Val_ALGOMIDTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckAlgoMidType(string securityid, string AlgoMidType, string sheetName)
        {
            if (MMConstants.Mid_ALGOMIDTYPE != AlgoMidType && MMConstants.Val_ALGOMIDTYPE != AlgoMidType
                && MMConstants.Cus_ALGOMIDTYPE != AlgoMidType)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "参考中间价";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.Mid_ALGOMIDTYPE}，{MMConstants.Val_ALGOMIDTYPE}，{MMConstants.Cus_ALGOMIDTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDealWidenLowerTime(string securityid, string value, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 2, 0)
                || CommonUtils.ConvertToLong(value) < 1
                 || CommonUtils.ConvertToLong(value) > 99)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "成交拉宽时长下限";
                error.ImportErrorResean = "设置错误，可选范围：[1, 99]";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDealWidenUpperTime(string securityid, string dealWidenLowerTime, string value, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 2, 0)
                || CommonUtils.ConvertToLong(value) < dealWidenLowerTime.ConvertToDecimal()
                 || CommonUtils.ConvertToLong(value) > 99)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "成交拉宽时长上限";
                error.ImportErrorResean = "设置错误，可选范围：[拉宽下限, 99]";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckMaxFloor(string securityid, string value, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 3, 0)
                || CommonUtils.ConvertToLong(value) < 1
                 || CommonUtils.ConvertToLong(value) > 999)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "冰山量(KW)";
                error.ImportErrorResean = "冰山量设置错误，取值需要在[1, 999]";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDefenCDCValue(string securityid, string side, string key, string value, string reason, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || (!CommonUtils.IsNumber(value, 4, 2) && !CommonUtils.IsNegative(value, 4, 2))
                || decimal.Compare(CommonUtils.ConvertToDecimal(value), -1000) == -1
                 || CommonUtils.ConvertToDecimal(value) > 1000)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportValue = side;
                error.ImportErrorResean = reason;
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDefenReferMarketType(string securityid, string defenReferMarketType, string sheetName,string side)
        {
            if (MMConstants.QDMESP_DEFENREFERMARKETTYPE != defenReferMarketType && MMConstants.XBOND_DEFENREFERMARKETTYPE != defenReferMarketType
                && MMConstants.MARKET_DEFENREFERMARKETTYPE != defenReferMarketType && MMConstants.CFETS_DEFENREFERMARKETTYPE != defenReferMarketType)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = $"{side}类型";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.QDMESP_DEFENREFERMARKETTYPE}，{MMConstants.XBOND_DEFENREFERMARKETTYPE}，{MMConstants.MARKET_DEFENREFERMARKETTYPE}，{MMConstants.CFETS_DEFENREFERMARKETTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDefenReferLevel(string securityid, string defenReferLevel, string sheetName, string side,string msg)
        {
            if (MMConstants.LEVEL1 != defenReferLevel && MMConstants.LEVEL2 != defenReferLevel
                && MMConstants.LEVEL3 != defenReferLevel && MMConstants.LEVEL4 != defenReferLevel
                && MMConstants.LEVEL5 != defenReferLevel)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = $"{side}{msg}档位";
                error.ImportErrorResean = $"设置错误，可选范围：{MMConstants.LEVEL1}，{MMConstants.LEVEL2}，{MMConstants.LEVEL3}，{MMConstants.LEVEL4}，{MMConstants.LEVEL5}";
                return error;
            }
            return null;
        }
        private ImportErrorInfo CheckExeFollow(string securityid, string value, string sheetName)
        {
            if ("行权" != value && "到期" != value)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "债券跟随类型";
                error.ImportErrorResean = $"设置错误，可选范围：行权，到期";
                return error;
            }
            return null;
        }

        #endregion

        private void GetMmUserSetting()
        {
            MMSettingViewModel.MoniValueChange += MMSettingViewModel_MoniValueChange;
            UserSettingReplyPo result = MMHandler.Instance.queryMMUserSetting();
            if (result == null)
                return;

            UserSetting userSetting = new UserSetting();
            if (result.Data != null)
            {
                userSetting = result.Data;
                if (!string.IsNullOrEmpty(userSetting.Tip1Enable))
                {
                    Tip1Enable = (!string.IsNullOrEmpty(userSetting.Tip1Enable) && "1".Equals(userSetting.Tip1Enable)) ? true : false;
                }

                if (!string.IsNullOrEmpty(userSetting.Tip2Enable))
                { Tip2Enable = (!string.IsNullOrEmpty(userSetting.Tip2Enable) && "1".Equals(userSetting.Tip2Enable)) ? true : false; }

                if (!string.IsNullOrEmpty(userSetting.Tip4Enable))
                { Tip4Enable = (!string.IsNullOrEmpty(userSetting.Tip4Enable) && "1".Equals(userSetting.Tip4Enable)) ? true : false; }

                bool BncMoniEnable = (!string.IsNullOrEmpty(userSetting.BncMoniEnable) && "1".Equals(userSetting.BncMoniEnable)) ? true : false;
                string bncMoniValue = !string.IsNullOrEmpty(userSetting.BncMoniValue) ? userSetting.BncMoniValue : "";
                if (BncMoniEnable)
                {
                    BncMoniValue = bncMoniValue;
                }
                else
                {
                    BncMoniValue = string.Empty;
                }
                bool BcoMoniEnable = (!string.IsNullOrEmpty(userSetting.BcoMoniEnable) && "1".Equals(userSetting.BcoMoniEnable)) ? true : false;
                string bcoMoniValue = !string.IsNullOrEmpty(userSetting.BcoMoniValue) ? userSetting.BcoMoniValue : "";
                if (BcoMoniEnable)
                {
                    BcoMoniValue = bcoMoniValue;
                }
                else
                {
                    BcoMoniValue = string.Empty;
                }
            }
        }

        private void MMSettingViewModel_MoniValueChange()
        {
            foreach (MMListBoxViewModel mmListBoxVm in mmListBoxVMdic.Values)
            {
                mmListBoxVm.UpdateMoniValue();
            }
        }


        private void ImportGroup()
        {
            if (MMGroupHandler.Instance.HasOpen(this.SelectGroup.id))
            {
                CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再批量导入");
                return;
            }

            ImportBondsView importBondsView = new ImportBondsView(this.SelectGroup);
            importBondsView.Title = "导入债券";
            importBondsView.ShowDialog();
        //ObservableCollection<HomeGroupInfo> AddHomeGroupDic = new ObservableCollection<HomeGroupInfo>();
        //foreach (MMGroupInfo pair in MMGroupHandler.Instance.CacheSortedGroups)
        //{
        //    HomeGroupInfo info = new HomeGroupInfo();
        //    info.id = pair.id;
        //    info.groupname = pair.groupname;
        //    info.inf1 = pair.inf1;
        //    AddHomeGroupDic.Add(info);
        //}
        //RecognizeBondView recognizeBondView = new RecognizeBondView();
        //recognizeBondView.setDefaultGroupID(this.SelectGroup.id, AddHomeGroupDic);
        //recognizeBondView.ShowDialog();
        //bool result = recognizeBondView.Result;

        //if (result == true)
        //{
        //    if (recognizeBondView.UpdateList != null && recognizeBondView.SelectedAddHomeGroup != null)
        //    {
        //        if (MMGroupHandler.Instance.HasOpen(recognizeBondView.SelectedAddHomeGroup.id))
        //        {
        //            CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再批量导入");
        //            return;
        //        }
        //        MMGroupInfo saveGroupInfo = MMGroupHandler.Instance.GetGroupInfoByGroupID(recognizeBondView.SelectedAddHomeGroup.id);

        //        MMOrderParamReq param;
        //        try
        //        {
        //            if (MMConstants.MM_GROPE_TYPE_UNIFY.Equals(saveGroupInfo.inf1))
        //                param = JsonConvert.DeserializeObject<MMOrderParamReq>(saveGroupInfo.inf2);
        //            else
        //            {
        //                param = new MMOrderParamReq();
        //                param.InitData();
        //            }
        //        }
        //        catch
        //        {
        //            param = new MMOrderParamReq();
        //            param.InitData();
        //        }

        //        if (recognizeBondView.UpdateList != null && recognizeBondView.UpdateList.Count > 0)
        //        {
        //            List<STC.Common.Model.MM.MM> reqList = new List<STC.Common.Model.MM.MM>();
        //            foreach (HomeGroupSecurityidsUpdateReq drv in recognizeBondView.UpdateList)
        //            {
        //                STC.Common.Model.MM.MM request = new STC.Common.Model.MM.MM();
        //                request.Action = "1"; // 添加
        //                request.securityid = param.securityid = drv.securityid;
        //                request.symbol = drv.symbol;
        //                request.groupid = saveGroupInfo.id;
        //                request.settltype = "2";
        //                request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
        //                request.enable = "0";
        //                request.Params = param;
        //                reqList.Add(request);
        //            }
        //            if (reqList.Count > 0)
        //            {
        //                ImportBonds(reqList, saveGroupInfo);
        //            }
        //        }
        //    }
        //}
    }
        public async void ImportBonds(List<STC.Common.Model.MM.MM> reqList,MMGroupInfo saveGroupInfo)
        {
            int FailCount = 0;
            string ResultStr = "";
            int SuccessCount = 0;
            List<STC.Common.Model.MM.MM> existMMs = MMGroupHandler.Instance.GetMMsByGroupID(saveGroupInfo.id);
            try
            {
                MMReq request = new MMReq();
                request.Action = "1";
                request.mmList = new List<STC.Common.Model.MM.MM>();

                foreach (STC.Common.Model.MM.MM req in reqList)
                {
                    if (existMMs != null && existMMs.Any(item => item.securityid == req.securityid))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"当前组下债券{req.securityid}已存在" + "\r\n";
                    }
                    else if (CommonUtils.GetBondsInfoPoBySecurityID(req.securityid) == null
                        || !CommonUtils.IsCIB(CommonUtils.GetBondsInfoPoBySecurityID(req.securityid)))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"债券{req.securityid}无效" + "\r\n";
                    }
                    else
                    {
                        request.mmList.Clear();
                        request.mmList.Add(req);
                        PResult pResult = await Utils.Data.MMGroupHandler.Instance.AsyncUpdateSecurityids(request);
                        if (pResult != null && pResult.IsSuccess())
                        {
                            SuccessCount = SuccessCount + 1;
                            foreach (MM mm in request.mmList)
                            {
                                BondsInfoPo bip = CommonUtils.GetBondsInfoPoBySecurityID(mm.securityid);
                                string remainingTime = bip == null ? string.Empty : bip.RemaingingTime;
                                ResultStr += $"{mm.securityid} {mm.symbol} {remainingTime}\r\n";
                            }
                        }
                        else
                        {
                            foreach (MM mm in request.mmList)
                            {
                                string errMsg = (pResult != null && !string.IsNullOrEmpty(pResult.Msg)) ? pResult.Msg : $"{mm.securityid}导入失败";
                                FailCount = FailCount + 1;
                                ResultStr += $"{errMsg}\r\n";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            finally
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.PromptTitle = "批量导入";
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptContext = ResultStr;
                prompt.Show();
            }
        }
    }
    public class OrderControlPriceType : ViewModelBase
    {
        private string code = "1";
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }
    }
}
