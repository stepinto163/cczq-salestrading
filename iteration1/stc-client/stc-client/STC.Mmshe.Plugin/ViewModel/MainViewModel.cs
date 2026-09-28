using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using System.Data;
using System.Windows.Input;
using STC.Mmshe.Plugin.Utils;
using System.Windows;
using STC.Common.Model.MMshe;
using STC.Mmshe.Plugin.View.Component;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Collections.Concurrent;
using STC.Mmshe.Plugin.View;
using com.gateway.log;
using STC.Mmshe.Plugin.Utils.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using MMsheUserSetting = STC.Common.Model.MMshe.MMsheUserSetting;
using System.IO;
using STC.Common.View.UI;
using System.Threading;
using System.Windows.Media;
using System.Timers;
using STC.Mmshe.Plugin.View.HomePage;
using STC.Common.View.Component;
using System.Linq;
using STC.Mmshe.Plugin.Command;
using STC.MmshePlugin.Utils;
using Newtonsoft.Json;
using STC.Common.Model.Drp;
using STC.Common.Model.Admin;
using Ats.Foundation.Message;
using STC.Common.Model.Ans;
using STC.Common.Model.MM;
using STC.Frame.UI;

namespace STC.Mmshe.Plugin.ViewModel
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

        private ConcurrentDictionary<string, MmsheListBoxViewModel> mmListBoxVMdic = new ConcurrentDictionary<string, MmsheListBoxViewModel>();

        public ConcurrentDictionary<string, MmsheListBoxViewModel> getMmsheListBoxViewModel() { return mmListBoxVMdic; }
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

        private Visibility _IsShowMMsheOrderControl = Visibility.Collapsed;

        public Visibility IsShowMMsheOrderControl
        {
            get
            {
                return this._IsShowMMsheOrderControl;
            }

            set
            {
                this._IsShowMMsheOrderControl = value;
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
            //new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
            //new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            //new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false, Itemmargin="50 0 0 0"},
            //new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
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
            //new FilterListBoxItemData() {itemText = "7Y", itemIsSelected = false},
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

        public MMsheGroupInfo SelectGroup { get; set; }

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
            Utils.Data.MMsheGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            MMsheHandler.Instance.SubscribeMMSysStatus(this.OnMMSysStatus, new List<string> { LocalEnv.UserInfo.UserName });

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

            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                GetMmUserSetting();

                ReadHistoryData_OrderControl();
                //SystemMessageReq dealReq = SetSystemMsgReq("2", "00:00:00", string.Empty, 0);
                //TQueryMessage systemMessage = AdminHandler.Instance.SearchSysMsg(dealReq);
                //if (systemMessage != null && systemMessage.Datas != null)
                //{
                //    OnAdminMsg(systemMessage.Datas);
                //}
                mmQuerySingleBondSetting();
            });


            //AdminHandler.Instance.Init(OnAdminMsg);
            WidenQuoteBtnEnabled = true;

            //MMsheCommonUtils.PlayAnsAlertWave();
            MMExcelExportUtils.Instance.init(this);
            if (AdminHandler.dicGroup.Count() <= 0)
            {
                RefreshCacheGroup();
            }
        }
        private void RefreshCacheGroup()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
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
            });
        }
        private void mmQuerySingleBondSetting()
        {
            MMBondParamsSettingReplyPo result = MMsheHandler.Instance.mmQuerySingleBondSetting();
            if (result == null)
                return;
            MMsheCommonUtils.updateBondParamsSetting(result.Datas);
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
                STC.Mmshe.Plugin.View.HomePage.AddBondWindow addBondWind = new STC.Mmshe.Plugin.View.HomePage.AddBondWindow();
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
                orderParam.Venuetypegw = orderParam.Venues = addBondWind.Venue;
                orderParam = MMsheCommonUtils.OrderParamPropToDefault(orderParam);
                orderParam.securityid = addBondWind.securityID;
                orderParam.symbol = addBondWind.symbol;
                PResult result = MMsheCommonUtils.QueryCirculSizeBySecurity(addBondWind.securityID);
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
                //orderParam.Venues = addBondWind.Venue;
                //orderParam.Venuetypegw = addBondWind.Venue;
                if ("BNC".Equals(bondsInfoPo.symbolType))
                {
                    orderParam.IsInterest = "true";
                }
                else
                {
                    orderParam.IsInterest = "false";
                }
                //if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                //            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                //            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                //{
                //    orderParam.wallEnable = "0";
                //    orderParam.CDCValueType = "1";
                //    orderParam.exeBondFollow = "0";
                //}
                //RiskQtyBySecurityReplyPo replyPo = MMsheCommonUtils.QueryRiskQtyBySecurity(addBondWind.securityID);
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
                    MMsheReq updateReq = new MMsheReq();
                    updateReq.Action = "1"; // 添加
                    STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
                    request.venues = odVM.Data.Venues;
                    request.venuetypegw = odVM.Data.Venuetypegw;
                    request.securityid = addBondWind.securityID;
                    request.symbol = addBondWind.symbol;
                    request.Action = "1"; // 添加
                    request.remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}";
                    request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    request.groupid = groupID;
                    request.settltype = odVM.OrderParamReq.baseSettleType;
                    request.Params = odVM.OrderParamReq;
                    request.enable = "0";
                    updateReq.mmList = new List<STC.Common.Model.MMshe.MMshe>();
                    updateReq.mmList.Add(request);
                    PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateSecurityids(updateReq);
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
                string key = keyList[i];
                if (!string.IsNullOrEmpty(key))
                {
                    _SavedOrderControlDic.TryAdd(key, "0");
                    MMsheGroupHandler.Instance.RegisterMM(key, Instance_ONMM);
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
                    MMsheOrderControl orderControl = new MMsheOrderControl();
                    orderControl.securityID = securityID;
                    orderControl.Venue = SelectGroup.venues;
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
                    _SavedOrderControlDic[string.Format("{0}-{1}", securityID, SelectGroup.venues)] = "1";
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
                            && (popupWindow.Content as System.Windows.Controls.Frame).Content is MMsheOrderControl)
                        {
                            MMsheOrderControl MMsheOrderControl = (popupWindow.Content as System.Windows.Controls.Frame).Content as MMsheOrderControl;
                            MMsheOrderControl.UnregistMM();
                        }
                    }
                }));
            }
        }

        private void Instance_ONMM(STC.Common.Model.MMshe.MMshe obj)
        {
            if (obj != null && obj.Action != "3")
            {
                if (_SavedOrderControlDic.ContainsKey(obj.getKey()) && ("0").Equals(_SavedOrderControlDic[obj.getKey()]))
                {
                    OpenSavedOrderControl(obj.securityid);
                    MMsheGroupHandler.Instance.UnregisterMM(obj.getKey(), Instance_ONMM);
                }
            }
            else
            {
                if (obj != null && obj.Action == "3")
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (windowManager.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{obj.getKey()}") && _SavedOrderControlDic[obj.getKey()].ToString().Equals("0"))
                        {
                            windowManager.CloseWindow($"{PopupTypeEnum.OrderControl.ToString()}-{obj.getKey()}");
                        }

                        MMsheGroupHandler.Instance.UnregisterMM(obj.getKey(), Instance_ONMM);

                    }));
                }


            }
        }

        public MMBreakAlarmModel cacheAlarmModel = new MMBreakAlarmModel();
        private void OnAdminMsg(List<TTbookSystemMessage> sysMsg)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (TTbookSystemMessage msg in sysMsg)
                {
                    if (msg.content != null)
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
                        VoiceWarnTimer.Start();
                    }
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
            else if ((hour == StartHour && minute >= StopMinute) || hour > StartHour)
            {
                StopAllCheckTimer();
            }
        }

        private void OnMMSysStatus(MMsheSysStatus obj)
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
                MmsheListBoxViewModel MmsheListBoxViewModel = new MmsheListBoxViewModel(groupID);
                mmListBoxVMdic[groupID] = MmsheListBoxViewModel;
                HasGroup = mmListBoxVMdic.Count > 0;
            }
        }
        private void HomePageDataHandler_OnFavoritesChanged(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securtityID)
        {
            if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                LogHandler.Info($"1.Add mm group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID) == false)
                {
                    LogHandler.Info($"2.Add mm group:[{groupID}]");
                    MmsheListBoxViewModel MmsheListBoxViewModel = new MmsheListBoxViewModel(groupID);
                    mmListBoxVMdic[groupID] = MmsheListBoxViewModel;
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
                //RefreshCacheGroup();
            }
            else if (favoriteTypeEnum == Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                LogHandler.Info($"1.Del mm group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID))
                {
                    LogHandler.Info($"2.Del mm group:[{groupID}]");
                    MmsheListBoxViewModel MmsheListBoxViewModel;
                    mmListBoxVMdic.TryRemove(groupID, out MmsheListBoxViewModel);
                    MmsheListBoxViewModel.Dispose();
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
                //RefreshCacheGroup();
            }
        }

        public MmsheListBoxViewModel GetMmsheListBoxViewModelByGroupID(string groupid)
        {
            if (mmListBoxVMdic.ContainsKey(groupid) == false)
            {
                MmsheListBoxViewModel MmsheListBoxViewModel = new MmsheListBoxViewModel(groupid);
                mmListBoxVMdic[groupid] = MmsheListBoxViewModel;
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
            foreach (MmsheListBoxViewModel vm in mmListBoxVMdic.Values)
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
                LogHandler.Info(string.Format("======OnSubscribeAnsSummary====== Hour = {0} Minute = {1} AnsCheckTimer.IsEnabled = {2}", hour, minute, AnsCheckTimer.IsEnabled.ToString()));
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
            LogHandler.Info(string.Format("======StopAllCheckTimer====== Hour = {0}  Minute = {1}", hour, minute));

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
                foreach (MmsheListBoxViewModel MmsheListBoxViewModel in mmListBoxVMdic.Values)
                {
                    MmsheListBoxViewModel.Clear();
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
                foreach (MmsheListBoxViewModel MmsheListBoxViewModel in mmListBoxVMdic.Values)
                {
                    MmsheListBoxViewModel.Dispose();
                }
                mmListBoxVMdic.Clear();
                mmListBoxVMdic = null;
                HasGroup = false;
            }

            Utils.Data.MMsheGroupHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            MMsheHandler.Instance.UnsubscribeMMSysStatus(this.OnMMSysStatus, new List<string> { LocalEnv.UserInfo.UserName });

            base.Dispose();
        }

        private void RestartAnsCheckTimer()
        {
            LogHandler.Info(string.Format("======Clear====== Hour = {0}", int.Parse(DateTimeEx.ServerTime.ToString("HH"))));
            int hour = int.Parse(DateTimeEx.ServerTime.ToString("HH"));
            int minute = int.Parse(DateTimeEx.ServerTime.ToString("mm"));
            LogHandler.Info(string.Format("======RestartAnsCheckTimer====== Hour = {0} AnsCheckTimer.IsEnabled = {1}", hour, AnsCheckTimer.IsEnabled.ToString()));
            if ((hour < StartHour || (hour == StartHour && minute < StopMinute)) && AnsCheckTimer.IsEnabled == false)
            {
                LogHandler.Info(string.Format("======RestartAnsCheckTimer.Start ====== "));
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
                MMsheUpdateAllEnableReq updateAllEnableReq = new MMsheUpdateAllEnableReq();
                updateAllEnableReq.enable = enable;
                updateAllEnableReq.macID = EngineHelper.GetMacByNetworkInterface();
                updateAllEnableReq.insideIP = EngineHelper.GetIp();
                updateAllEnableReq.hardDiskID = EngineHelper.Getharddiskid2();
                result = MMsheHandler.Instance.mmUpdateAllEnable(updateAllEnableReq);
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
                    PResult result = MMsheHandler.Instance.mmCancelWidenSetting(null);

                    if (result == null || result.IsSuccess() == false)
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
                foreach (MmsheListBoxViewModel mmListViewModel in mmListBoxVMdic.Values)
                {
                    mmListViewModel.FilterData(this.SearchText, this.filterBondType, this.filterCreditBondType, this.PolicyFinancialBond,
                        this.filterRemainingTime, this.filterNewBondSign, this.filterRating, this.settlTypeFilter, enableStatusFilter, this.monitorStatusFilter, this.isPauseStatusFilter);
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
                        notInFilter = $"{MmsheListBoxViewModel.GOODS_TYPE_FIELD} = 'BNC'";
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
                    filterBondType.Append($"{MmsheListBoxViewModel.BOND_CATEGORY_FIELD} in ({inFilterSB})");
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
                        filterBondType.Append($" and { MmsheListBoxViewModel.GOODS_TYPE_FIELD} = 'BNC'");
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
                        notInFilter = $"({MmsheListBoxViewModel.GOODS_TYPE_FIELD} <> 'BNC')";
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
                    filterCreditBondType.Append($"{MmsheListBoxViewModel.BOND_CATEGORY_FIELD} in ({inFilterSB})");
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
                        filterCreditBondType.Append($" and {MmsheListBoxViewModel.GOODS_TYPE_FIELD} <> 'BNC'");
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
                        PolicyFinancialBond.Append($"{MmsheListBoxViewModel.ISSUER_FIELD} = '100034'");
                    }
                    else if (lbi.itemText == "农发")
                    {
                        PolicyFinancialBond.Append($"{MmsheListBoxViewModel.ISSUER_FIELD} = '101915'");
                    }
                    else if (lbi.itemText == "口行")
                    {
                        PolicyFinancialBond.Append($"{MmsheListBoxViewModel.ISSUER_FIELD} = '100017'");
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
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        filterRemainingTime.Append($"({MmsheListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
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
                        filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '1Y')");
                    }
                    else if (lbi.itemText == "2Y")
                    {
                        filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '2Y')");
                    }
                    else if (lbi.itemText == "3Y")
                    {
                        filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '3Y')");
                    }
                    else if (lbi.itemText == "5Y")
                    {
                        filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '5Y')");
                    }
                    //else if (lbi.itemText == "7Y")
                    //{
                    //    filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '7Y')");
                    //}
                    else if (lbi.itemText == "10Y")
                    {
                        filterNewBondSign.Append($"({MmsheListBoxViewModel.ANS_NEW_BOND_SIGN_FIELD} = '1' and {MmsheListBoxViewModel.SECURITY_TERM_FIELD} = '10Y')");
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
                            notInFilter = $"{MmsheListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MmsheListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                            break;
                    }
                }
                if (inFilterSB.Length > 0)
                {
                    if (filterRating.Length > 0)
                    {
                        filterRating.Append(" or ");
                    }
                    filterRating.Append($"{MmsheListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} in ({inFilterSB})");
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
                        settlTypeFilter.Append($"({MmsheListBoxViewModel.SETTLE_TYPE_FIELD} = '2' or {MmsheListBoxViewModel.SETTLE_TYPE_FIELD} = '3')");
                    }
                    else if (lbi.itemText == "T+0")
                    {
                        settlTypeFilter.Append($"{MmsheListBoxViewModel.SETTLE_TYPE_FIELD} = '1'");
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
                        enableStatusFilter.Append($"{MmsheListBoxViewModel.ENABLE_FIELD} = true");
                    }
                    else if (lbi.itemText == "关闭")
                    {
                        enableStatusFilter.Append($"{MmsheListBoxViewModel.ENABLE_FIELD} = false");
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
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '1'");
                    }
                    else if (lbi.itemText == "可达标")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '2'");
                    }
                    else if (lbi.itemText == "已达标")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '3'");
                    }
                    else if (lbi.itemText == "失败")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '4'");
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
                isPauseStatusFilter.Append($"{MmsheListBoxViewModel.ANS_PAUSE_STATUS_FIELD} = '0'");
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
                        enableStatusFilter.Append($"{MmsheListBoxViewModel.ENABLE_FIELD} = true");
                    }
                    else if (lbi == "0010")
                    {
                        enableStatusFilter.Append($"{MmsheListBoxViewModel.ENABLE_FIELD} = false");
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
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '1'");
                    }
                    else if (lbi == "0001")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '2'");
                    }
                    else if (lbi == "0010")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '3'");
                    }
                    else if (lbi == "0101")
                    {
                        monitorStatusFilter.Append($"{MmsheListBoxViewModel.ANS_MM_STATUS_FIELD} = '4'");
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
                sfd.FileName = $"做市工作台报价导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
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
            catch (Exception e)
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
                    if (MMsheGroupHandler.Instance.CacheSortedGroups != null)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        foreach (MMsheGroupInfo item in MMsheGroupHandler.Instance.CacheSortedGroups)
                        {
                            if (item.inf1 != MMsheConstants.MM_GROPE_TYPE_UNIFY)
                            {
                                DataTable dt = GenerateExcelDataTable(item.id, item.venuetypegw);
                                dataDic[item.groupname] = dt;
                            }
                            else
                            {
                                DataTable dt = GroupExcelDataTable(item.id, item.venuetypegw);
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

        private DataTable GenerateExcelDataTable(string groupid, string venue)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MMsheConstants.VenueTypeGW_ExcelField);
            dt.Columns.Add(MMsheConstants.SecurityID_ExcelField);
            dt.Columns.Add(MMsheConstants.BidQuoteModel_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrQuoteModel_ExcelField);
            dt.Columns.Add(MMsheConstants.BidBaseQty_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrBaseQty_ExcelField);
            dt.Columns.Add(MMsheConstants.ManualBid_ExcelField);
            dt.Columns.Add(MMsheConstants.ManualOfr_ExcelField);
            dt.Columns.Add(MMsheConstants.StepLength_ExcelField);
            //dt.Columns.Add(MMsheConstants.BidBasePriority_ExcelField);
            //dt.Columns.Add(MMsheConstants.OfrBasePriority_ExcelField);
            dt.Columns.Add(MMsheConstants.BidBaseReSendValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrBaseReSendValue_ExcelField);
            /*dt.Columns.Add(MMsheConstants.BidBaseWidenValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrBaseWidenValue_ExcelField);
            dt.Columns.Add(MMsheConstants.BaseERReSendType_ExcelField);
            dt.Columns.Add(MMsheConstants.DealWidenLowerTime_ExcelField);
            dt.Columns.Add(MMsheConstants.DealWidenUpperTime_ExcelField);*/
            dt.Columns.Add(MMsheConstants.BaseSettleType_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenReferLevel_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenReferLevel_ExcelField);
            dt.Columns.Add(MMsheConstants.BidAlgoDefenValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrAlgoDefenValue_ExcelField);
            /*dt.Columns.Add(MMsheConstants.BidAlgoSafeValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrAlgoSafeValue_ExcelField);
            dt.Columns.Add(MMsheConstants.BidAlgoTrigValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrAlgoTrigValue_ExcelField);*/
            //dt.Columns.Add(MMsheConstants.AlgoEnable_ExcelField);
            /*dt.Columns.Add(MMsheConstants.AlgoBaseEnable_ExcelField);
            dt.Columns.Add(MMsheConstants.AlgoBaseSecurityid_ExcelField);
            dt.Columns.Add(MMsheConstants.AlgoMidType_ExcelField);
            dt.Columns.Add(MMsheConstants.AlgoMidDetailDefined_ExcelField);
            dt.Columns.Add(MMsheConstants.AlgoMidOffsetValue_ExcelField);
            dt.Columns.Add(MMsheConstants.AlgoValue_ExcelField);*/
            //dt.Columns.Add(MMsheConstants.AlgoCDCEnable_ExcelField);
            // 去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
            //dt.Columns.Add(MMsheConstants.PosLmtEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.BidPosValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.OfrPosValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.PosFakEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.PosFakSpread_ExcelField);
            //dt.Columns.Add(MMsheConstants.MaxFloorEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.MaxFloor_ExcelField);
            dt.Columns.Add(MMsheConstants.DefenCDCEnable_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenCDCValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenCDCValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.WallEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.ExeBondFollow_ExcelField);
            dt.Columns.Add(MMsheConstants.DealClose_ExcelField);

            IOrderedEnumerable<STC.Common.Model.MMshe.MMshe> mmlist = null;
            if (MMsheGroupHandler.Instance.GetMMsByGroupID(groupid) != null)
                mmlist = MMsheGroupHandler.Instance.GetMMsByGroupID(groupid).OrderByDescending(x => x.createtime);

            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    if (mmlist != null)
                    {
                        foreach (STC.Common.Model.MMshe.MMshe mm in mmlist)
                        {
                            MMsheOrderParamReq MMsheOrderParamReq = mm.Params;
                            DataRow newDR = dt.NewRow();
                            if (MMsheOrderParamReq != null)
                            {
                                //newDR[MMsheConstants.BidQuoteModel_ExcelField] = "Manual" == MMsheOrderParamReq.bidQuoteModel ? "自动" : "手动";
                                newDR[MMsheConstants.SecurityID_ExcelField] = mm.securityid;
                                if ("Manual" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.BidQuoteModel_ExcelField] = MMsheConstants.MANUAL_QUOTEMODEL;
                                else if ("Defend" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.BidQuoteModel_ExcelField] = MMsheConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.BidQuoteModel_ExcelField] = MMsheConstants.ALGORITHM_QUOTEMODEL;
                                else if ("API" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.BidQuoteModel_ExcelField] = MMsheConstants.API_QUOTEMODEL;
                                //newDR[MMsheConstants.OfrQuoteModel_ExcelField] = "Manual" == MMsheOrderParamReq.ofrQuoteModel ? "自动" : "手动";
                                if ("Manual" == MMsheOrderParamReq.ofrQuoteModel) newDR[MMsheConstants.OfrQuoteModel_ExcelField] = MMsheConstants.MANUAL_QUOTEMODEL;
                                else if ("Defend" == MMsheOrderParamReq.ofrQuoteModel) newDR[MMsheConstants.OfrQuoteModel_ExcelField] = MMsheConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == MMsheOrderParamReq.ofrQuoteModel) newDR[MMsheConstants.OfrQuoteModel_ExcelField] = MMsheConstants.ALGORITHM_QUOTEMODEL;
                                else if ("API" == MMsheOrderParamReq.ofrQuoteModel) newDR[MMsheConstants.OfrQuoteModel_ExcelField] = MMsheConstants.API_QUOTEMODEL;

                                newDR[MMsheConstants.BidBaseQty_ExcelField] = MMsheOrderParamReq.bidBaseQty;
                                newDR[MMsheConstants.OfrBaseQty_ExcelField] = MMsheOrderParamReq.ofrBaseQty;
                                newDR[MMsheConstants.ManualBid_ExcelField] = MMsheOrderParamReq.manualBid;
                                newDR[MMsheConstants.ManualOfr_ExcelField] = MMsheOrderParamReq.manualOfr;
                                newDR[MMsheConstants.StepLength_ExcelField] = MMsheOrderParamReq.stepLength;
                                //newDR[MMsheConstants.BidBasePriority_ExcelField] = "2" == MMsheOrderParamReq.bidBasePriority ? "算法" : "防守"; ;
                                //newDR[MMsheConstants.OfrBasePriority_ExcelField] = "2" == MMsheOrderParamReq.ofrBasePriority ? "算法" : "防守"; ;
                                newDR[MMsheConstants.BidBaseReSendValue_ExcelField] = MMsheOrderParamReq.bidBaseReSendValue;
                                newDR[MMsheConstants.OfrBaseReSendValue_ExcelField] = MMsheOrderParamReq.ofrBaseReSendValue;
                                /*newDR[MMsheConstants.BidBaseWidenValue_ExcelField] = MMsheOrderParamReq.bidBaseWidenValue;
                                newDR[MMsheConstants.OfrBaseWidenValue_ExcelField] = MMsheOrderParamReq.ofrBaseWidenValue;
                                newDR[MMsheConstants.BaseERReSendType_ExcelField] = "2" == MMsheOrderParamReq.BaseERReSendType ? MMsheConstants.LEAVE_BASEERRESENDTYPE : MMsheConstants.QTY_BASEERRESENDTYPE;
                                newDR[MMsheConstants.DealWidenLowerTime_ExcelField] = string.IsNullOrEmpty(MMsheOrderParamReq.dealWidenLowerTime) ? MMsheConstants.DEALWIDEN_VALUE : MMsheOrderParamReq.dealWidenLowerTime;
                                newDR[MMsheConstants.DealWidenUpperTime_ExcelField] = string.IsNullOrEmpty(MMsheOrderParamReq.dealWidenUpperTime) ? MMsheConstants.DEALWIDEN_VALUE : MMsheOrderParamReq.dealWidenUpperTime;
                                */
                                newDR[MMsheConstants.BaseSettleType_ExcelField] = "2" == mm.settltype ? MMsheConstants.T1_SETTLTYPE : MMsheConstants.T0_SETTLTYPE;
                                newDR[MMsheConstants.BidAlgoDefenValue_ExcelField] = MMsheOrderParamReq.bidAlgoDefenValue;
                                newDR[MMsheConstants.OfrAlgoDefenValue_ExcelField] = MMsheOrderParamReq.ofrAlgoDefenValue;
                                /*newDR[MMsheConstants.BidAlgoSafeValue_ExcelField] = MMsheOrderParamReq.bidAlgoSafeValue;
                                newDR[MMsheConstants.OfrAlgoSafeValue_ExcelField] = MMsheOrderParamReq.ofrAlgoSafeValue;
                                newDR[MMsheConstants.BidAlgoTrigValue_ExcelField] = MMsheOrderParamReq.bidAlgoTrigValue;
                                newDR[MMsheConstants.OfrAlgoTrigValue_ExcelField] = MMsheOrderParamReq.ofrAlgoTrigValue;*/
                                //newDR[MMsheConstants.AlgoEnable_ExcelField] = "1" == MMsheOrderParamReq.algoEnable ? "是" : "否";
                                /*newDR[MMsheConstants.AlgoBaseEnable_ExcelField] = "1" == MMsheOrderParamReq.algoBaseEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.AlgoBaseSecurityid_ExcelField] = MMsheOrderParamReq.algoBaseSecurityid;
                                if ("1" == MMsheOrderParamReq.algoMidType) newDR[MMsheConstants.AlgoMidType_ExcelField] = MMsheConstants.Mid_ALGOMIDTYPE;
                                else if ("2" == MMsheOrderParamReq.algoMidType) newDR[MMsheConstants.AlgoMidType_ExcelField] = MMsheConstants.Val_ALGOMIDTYPE;
                                else if ("3" == MMsheOrderParamReq.algoMidType) newDR[MMsheConstants.AlgoMidType_ExcelField] = MMsheConstants.Cus_ALGOMIDTYPE;
                                newDR[MMsheConstants.AlgoMidDetailDefined_ExcelField] = MMsheOrderParamReq.algoMidDetailDefined;
                                newDR[MMsheConstants.AlgoMidOffsetValue_ExcelField] = MMsheOrderParamReq.algoMidOffsetValue;
                                newDR[MMsheConstants.AlgoValue_ExcelField] = MMsheOrderParamReq.algoValue;*/
                                //newDR[MMsheConstants.AlgoCDCEnable_ExcelField] = "1" == MMsheOrderParamReq.algoCDCEnable ? "是" : "否";
                                //去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
                                //newDR[MMsheConstants.PosLmtEnable_ExcelField] = "1" == MMsheOrderParamReq.posLmtEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.BidPosValue_ExcelField] = MMsheOrderParamReq.bidPosValue;
                                //newDR[MMsheConstants.OfrPosValue_ExcelField] = MMsheOrderParamReq.ofrPosValue;
                                //newDR[MMsheConstants.PosFakEnable_ExcelField] = "1" == MMsheOrderParamReq.posFakEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.PosFakSpread_ExcelField] = MMsheOrderParamReq.posFakSpread;

                                //newDR[MMsheConstants.MaxFloorEnable_ExcelField] = "1" == MMsheOrderParamReq.maxFloorEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.MaxFloor_ExcelField] = MMsheOrderParamReq.maxFloor;

                                newDR[MMsheConstants.DefenCDCEnable_ExcelField] = "1" == MMsheOrderParamReq.defenCDCEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.BidDefenCDCValue_ExcelField] = MMsheOrderParamReq.bidDefenCDCValue;
                                newDR[MMsheConstants.OfrDefenCDCValue_ExcelField] = MMsheOrderParamReq.ofrDefenCDCValue;
                                if ("1" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.CFETS_DEFENREFERMARKETTYPE;
                                else if ("5" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SSE_DEFENREFERMARKETTYPE;
                                else if ("6" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SSF_DEFENREFERMARKETTYPE;
                                else if ("7" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SZF_DEFENREFERMARKETTYPE;
                                else if ("8" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SZE_DEFENREFERMARKETTYPE;

                                if ("1" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL1;
                                else if ("2" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL2;
                                else if ("3" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL3;
                                else if ("4" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL4;
                                else if ("5" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL5;

                                if ("1" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.CFETS_DEFENREFERMARKETTYPE;
                                else if ("5" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SSE_DEFENREFERMARKETTYPE;
                                else if ("6" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SSF_DEFENREFERMARKETTYPE;
                                else if ("7" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SZF_DEFENREFERMARKETTYPE;
                                else if ("8" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SZE_DEFENREFERMARKETTYPE;

                                if ("1" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL1;
                                else if ("2" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL2;
                                else if ("3" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL3;
                                else if ("4" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL4;
                                else if ("5" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL5;
                                //newDR[MMsheConstants.WallEnable_ExcelField] = "1" == MMsheOrderParamReq.wallEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.ExeBondFollow_ExcelField] = "0" == MMsheOrderParamReq.exeBondFollow ? "行权" : "到期";
                                newDR[MMsheConstants.VenueTypeGW_ExcelField] = ComboSourceUtil.Instance.FindEnum(venue, "venues");
                                newDR[MMsheConstants.DealClose_ExcelField] = "1" == MMsheOrderParamReq.dealClose ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;

                            }
                            else
                            {
                                newDR[MMsheConstants.SecurityID_ExcelField] = mm.securityid;
                                newDR[MMsheConstants.BidQuoteModel_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrQuoteModel_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidBaseQty_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrBaseQty_ExcelField] = string.Empty;
                                newDR[MMsheConstants.ManualBid_ExcelField] = string.Empty;
                                newDR[MMsheConstants.ManualOfr_ExcelField] = string.Empty;
                                newDR[MMsheConstants.StepLength_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.BidBasePriority_ExcelField] = "防守"; ;
                                //newDR[MMsheConstants.OfrBasePriority_ExcelField] = "防守"; ;
                                newDR[MMsheConstants.BidBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrBaseReSendValue_ExcelField] = string.Empty;
                                /*newDR[MMsheConstants.BidBaseWidenValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrBaseWidenValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BaseERReSendType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.DealWidenLowerTime_ExcelField] = MMsheConstants.DEALWIDEN_VALUE;
                                newDR[MMsheConstants.DealWidenUpperTime_ExcelField] = MMsheConstants.DEALWIDEN_VALUE;*/
                                newDR[MMsheConstants.BaseSettleType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidAlgoDefenValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoDefenValue_ExcelField] = string.Empty;
                                /*newDR[MMsheConstants.BidAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidAlgoTrigValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoTrigValue_ExcelField] = string.Empty;*/
                                //newDR[MMsheConstants.AlgoEnable_ExcelField] = "否";
                                /*newDR[MMsheConstants.AlgoBaseEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.AlgoBaseSecurityid_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoMidType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoMidDetailDefined_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoMidOffsetValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoValue_ExcelField] = string.Empty;*/
                                //newDR[MMsheConstants.AlgoCDCEnable_ExcelField] = "否";
                                // 去掉是否允许报价更激进、更优报价点差Bid、更优报价点差Ofr
                                //newDR[MMsheConstants.PosLmtEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.BidPosValue_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.OfrPosValue_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.PosFakEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.PosFakSpread_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.MaxFloorEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.MaxFloor_ExcelField] = string.Empty;
                                newDR[MMsheConstants.DefenCDCEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.BidDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.WallEnable_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.ExeBondFollow_ExcelField] = string.Empty;
                                newDR[MMsheConstants.VenueTypeGW_ExcelField] = string.Empty;
                                newDR[MMsheConstants.DealClose_ExcelField] = string.Empty;
                            }

                            dt.Rows.Add(newDR);
                        }
                    }
                }
            }
            return dt;
        }


        private DataTable GroupExcelDataTable(string groupid, string venue)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MMsheConstants.VenueTypeGW_ExcelField);
            dt.Columns.Add(MMsheConstants.SecurityID_ExcelField);
            dt.Columns.Add(MMsheConstants.QuoteModel_ExcelField);
            dt.Columns.Add(MMsheConstants.BidBaseQty_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrBaseQty_ExcelField);
            dt.Columns.Add(MMsheConstants.BidBaseReSendValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrBaseReSendValue_ExcelField);
            dt.Columns.Add(MMsheConstants.BaseSettleType_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenReferLevel_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenReferMarketType_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenReferLevel_ExcelField);
            dt.Columns.Add(MMsheConstants.BidAlgoDefenValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrAlgoDefenValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.BidAlgoSafeValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.OfrAlgoSafeValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.BidAlgoTrigValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.OfrAlgoTrigValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.AlgoMidType_ExcelField);
            //dt.Columns.Add(MMsheConstants.AlgoMidOffsetValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.AlgoValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.PosFakEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.PosFakSpread_ExcelField);
            //dt.Columns.Add(MMsheConstants.MaxFloorEnable_ExcelField);
            //dt.Columns.Add(MMsheConstants.MaxFloor_ExcelField);
            dt.Columns.Add(MMsheConstants.DefenCDCEnable_ExcelField);
            dt.Columns.Add(MMsheConstants.BidDefenCDCValue_ExcelField);
            dt.Columns.Add(MMsheConstants.OfrDefenCDCValue_ExcelField);
            //dt.Columns.Add(MMsheConstants.WallEnable_ExcelField);

            IOrderedEnumerable<STC.Common.Model.MMshe.MMshe> mmlist = null;
            if (MMsheGroupHandler.Instance.GetMMsByGroupID(groupid) != null)
                mmlist = MMsheGroupHandler.Instance.GetMMsByGroupID(groupid).OrderByDescending(x => x.createtime);

            if (string.IsNullOrEmpty(groupid) == false)
            {
                lock (string.Intern(groupid))
                {
                    if (mmlist != null)
                    {
                        foreach (STC.Common.Model.MMshe.MMshe mm in mmlist)
                        {
                            MMsheOrderParamReq MMsheOrderParamReq = mm.Params;
                            DataRow newDR = dt.NewRow();
                            if (MMsheOrderParamReq != null)
                            {
                                newDR[MMsheConstants.SecurityID_ExcelField] = mm.securityid;
                                if ("Defend" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.QuoteModel_ExcelField] = MMsheConstants.DEFEND_QUOTEMODEL;
                                else if ("Algorithm" == MMsheOrderParamReq.bidQuoteModel) newDR[MMsheConstants.QuoteModel_ExcelField] = MMsheConstants.ALGORITHM_QUOTEMODEL;

                                newDR[MMsheConstants.BidBaseQty_ExcelField] = MMsheOrderParamReq.bidBaseQty;
                                newDR[MMsheConstants.OfrBaseQty_ExcelField] = MMsheOrderParamReq.ofrBaseQty;
                                newDR[MMsheConstants.BidBaseReSendValue_ExcelField] = MMsheOrderParamReq.bidBaseReSendValue;
                                newDR[MMsheConstants.OfrBaseReSendValue_ExcelField] = MMsheOrderParamReq.ofrBaseReSendValue;
                                newDR[MMsheConstants.BaseSettleType_ExcelField] = "2" == mm.settltype ? MMsheConstants.T1_SETTLTYPE : MMsheConstants.T0_SETTLTYPE;
                                newDR[MMsheConstants.BidAlgoDefenValue_ExcelField] = MMsheOrderParamReq.bidAlgoDefenValue;
                                newDR[MMsheConstants.OfrAlgoDefenValue_ExcelField] = MMsheOrderParamReq.ofrAlgoDefenValue;
                                /*newDR[MMsheConstants.BidAlgoSafeValue_ExcelField] = MMsheOrderParamReq.bidAlgoSafeValue;
                                newDR[MMsheConstants.OfrAlgoSafeValue_ExcelField] = MMsheOrderParamReq.ofrAlgoSafeValue;
                                newDR[MMsheConstants.BidAlgoTrigValue_ExcelField] = MMsheOrderParamReq.bidAlgoTrigValue;
                                newDR[MMsheConstants.OfrAlgoTrigValue_ExcelField] = MMsheOrderParamReq.ofrAlgoTrigValue;*/
                                /*if ("2" == MMsheOrderParamReq.algoMidType) newDR[MMsheConstants.AlgoMidType_ExcelField] = MMsheConstants.Val_ALGOMIDTYPE;
                                newDR[MMsheConstants.AlgoMidOffsetValue_ExcelField] = MMsheOrderParamReq.algoMidOffsetValue;
                                newDR[MMsheConstants.AlgoValue_ExcelField] = MMsheOrderParamReq.algoValue;*/
                                //newDR[MMsheConstants.PosFakEnable_ExcelField] = "1" == MMsheOrderParamReq.posFakEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.PosFakSpread_ExcelField] = MMsheOrderParamReq.posFakSpread;

                                //newDR[MMsheConstants.MaxFloorEnable_ExcelField] = "1" == MMsheOrderParamReq.maxFloorEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.MaxFloor_ExcelField] = MMsheOrderParamReq.maxFloor;

                                newDR[MMsheConstants.DefenCDCEnable_ExcelField] = "1" == MMsheOrderParamReq.defenCDCEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.BidDefenCDCValue_ExcelField] = MMsheOrderParamReq.bidDefenCDCValue;
                                newDR[MMsheConstants.OfrDefenCDCValue_ExcelField] = MMsheOrderParamReq.ofrDefenCDCValue;
                                if ("1" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.CFETS_DEFENREFERMARKETTYPE;
                                else if ("5" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SSE_DEFENREFERMARKETTYPE;
                                else if ("6" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SSF_DEFENREFERMARKETTYPE;
                                else if ("7" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SZF_DEFENREFERMARKETTYPE;
                                else if ("8" == MMsheOrderParamReq.bidDefenReferMarketType) newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = MMsheConstants.SZE_DEFENREFERMARKETTYPE;

                                if ("1" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL1;
                                else if ("2" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL2;
                                else if ("3" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL3;
                                else if ("4" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL4;
                                else if ("5" == MMsheOrderParamReq.bidDefenReferLevel) newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = MMsheConstants.LEVEL5;

                                if ("1" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.QDMESP_DEFENREFERMARKETTYPE;
                                else if ("2" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.XBOND_DEFENREFERMARKETTYPE;
                                else if ("3" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.MARKET_DEFENREFERMARKETTYPE;
                                else if ("4" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.CFETS_DEFENREFERMARKETTYPE;
                                else if ("5" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SSE_DEFENREFERMARKETTYPE;
                                else if ("6" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SSF_DEFENREFERMARKETTYPE;
                                else if ("7" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SZF_DEFENREFERMARKETTYPE;
                                else if ("8" == MMsheOrderParamReq.ofrDefenReferMarketType) newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = MMsheConstants.SZE_DEFENREFERMARKETTYPE;

                                if ("1" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL1;
                                else if ("2" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL2;
                                else if ("3" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL3;
                                else if ("4" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL4;
                                else if ("5" == MMsheOrderParamReq.ofrDefenReferLevel) newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = MMsheConstants.LEVEL5;
                                //newDR[MMsheConstants.WallEnable_ExcelField] = "1" == MMsheOrderParamReq.wallEnable ? MMsheConstants.YES_VALUE : MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.VenueTypeGW_ExcelField] = ComboSourceUtil.Instance.FindEnum(venue, "venues");
                            }
                            else
                            {
                                newDR[MMsheConstants.SecurityID_ExcelField] = mm.securityid;
                                newDR[MMsheConstants.QuoteModel_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidBaseQty_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrBaseQty_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrBaseReSendValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BaseSettleType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidAlgoDefenValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoDefenValue_ExcelField] = string.Empty;
                                /*newDR[MMsheConstants.BidAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoSafeValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidAlgoTrigValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrAlgoTrigValue_ExcelField] = string.Empty;*/
                                /*newDR[MMsheConstants.AlgoMidType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoMidOffsetValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.AlgoValue_ExcelField] = string.Empty;*/
                                //newDR[MMsheConstants.PosFakEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.PosFakSpread_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.MaxFloorEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                //newDR[MMsheConstants.MaxFloor_ExcelField] = string.Empty;
                                newDR[MMsheConstants.DefenCDCEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.BidDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenCDCValue_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.BidDefenReferLevel_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenReferMarketType_ExcelField] = string.Empty;
                                newDR[MMsheConstants.OfrDefenReferLevel_ExcelField] = string.Empty;
                                //newDR[MMsheConstants.WallEnable_ExcelField] = MMsheConstants.NO_VALUE;
                                newDR[MMsheConstants.VenueTypeGW_ExcelField] = string.Empty;
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
                if (MMsheGroupHandler.Instance.HasOpen())
                {
                    Prompt2SecondsMsg("请先关闭做市中的债券");
                    return;
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
                    if (MMsheGroupHandler.Instance.HasOpen())
                    {
                        errorlist.Add("请先关闭做市中的债券");
                        return errorlist;
                    }

                    Dictionary<int, string> dic = helper.ReturnSheetList();

                    if (dic.Count > 20)
                    {
                        errorlist.Add("分组总数应不超过20个");
                        return errorlist;
                    }

                    // 3.读文件
                    Dictionary<string, int[]> checkImportDic = new Dictionary<string, int[]>();
                    MMsheReq request = new MMsheReq();
                    request.Action = "4";
                    request.mmList = new List<STC.Common.Model.MMshe.MMshe>();
                    ObservableCollection<ImportErrorInfo> checkMMErrList = new ObservableCollection<ImportErrorInfo>();
                    ObservableCollection<MMsheGroupInfo> newMMsheGroupInfoList = new ObservableCollection<MMsheGroupInfo>();
                    ObservableCollection<MMsheGroupInfo> updateMMsheGroupInfoList = new ObservableCollection<MMsheGroupInfo>();
                    IOrderedEnumerable<MMsheGroupInfo> sortedGroups = await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                    int ordersn = 1;
                    if (sortedGroups != null && sortedGroups.Count() > 0)
                    {
                        ordersn = sortedGroups.Last().ordersn + 1;
                    }

                    List<int> unifyGroupKeys = new List<int>();
                    bool isUnifyGroupError = false;
                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        if (pair.Value.Length > 10)
                        {
                            ImportErrorInfo error = new ImportErrorInfo();
                            error.ImportErrorResean = $"分组名称【{pair.Value}】超过10个汉字";
                            checkMMErrList.Add(error);
                            continue;
                        }

                        MMsheGroupInfo MMsheGroupInfo = MMsheGroupHandler.Instance.IsGroupNameExist(pair.Value);

                        // 如果是整组，不做任何操作
                        if (MMsheGroupInfo == null)
                        {
                            MMsheGroupInfo = new MMsheGroupInfo();
                            MMsheGroupInfo.groupname = pair.Value;
                            MMsheGroupInfo.terminal = LocalEnv.ServerInfo.Version;
                            MMsheGroupInfo.action = 1;
                            MMsheGroupInfo.ordersn = ordersn;
                            newMMsheGroupInfoList.Add(MMsheGroupInfo);
                            ordersn++;
                        }
                        else
                        {
                            if (MMsheGroupInfo.inf1 == MMsheConstants.MM_GROPE_TYPE_UNIFY)
                            {
                                MMsheGroupInfo.groupname = pair.Value;
                                MMsheGroupInfo.terminal = LocalEnv.ServerInfo.Version;
                                MMsheGroupInfo.action = 2;
                                updateMMsheGroupInfoList.Add(MMsheGroupInfo);
                            }
                        }

                        string groupID = MMsheGroupInfo.id;
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        int validCount = 0;

                        bool isUnifyGroup = checkUnifyGroup(importDT);
                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            Dictionary<string, string> serverCheckDic = new Dictionary<string, string>();

                            if (isUnifyGroup)
                            {
                                bool isUnifyGroupOnly = checkGroup(importDT);
                                if (!isUnifyGroupOnly)
                                {
                                    ImportErrorInfo error = new ImportErrorInfo();
                                    error.GroupName = string.Format("[{0}]", pair.Value);
                                    error.ImportErrorResean = $"整组参数或交易场所不一致，请修改后导入";
                                    checkMMErrList.Add(error);
                                    isUnifyGroupError = true;
                                    continue;
                                }
                                else
                                {
                                    if (importDT.Rows.Count > 0)
                                    {
                                        DataRow firstRow = importDT.Rows[0];
                                        MMsheGroupInfo.inf1 = MMsheConstants.MM_GROPE_TYPE_UNIFY;
                                        List<ImportErrorInfo> errList;
                                        MMsheOrderParamReq mMOrder = CheckImportMMList(firstRow, null, serverCheckDic, out errList, pair.Value, true, true);
                                        string inf2 = JsonConvert.SerializeObject(mMOrder);
                                        MMsheGroupInfo.inf2 = inf2;
                                    }
                                    else
                                    {
                                        MMsheOrderParamReq mMOrder = new MMsheOrderParamReq();
                                        mMOrder.InitData();
                                        mMOrder.bidQuoteModel = mMOrder.ofrQuoteModel = "Defend";
                                        MMsheGroupInfo.inf1 = MMsheConstants.MM_GROPE_TYPE_UNIFY;
                                        MMsheGroupInfo.inf2 = JsonConvert.SerializeObject(mMOrder);
                                    }
                                }
                            }
                            else
                            {
                                bool checkVenueStatus = checkVenue(importDT);
                                if (!checkVenueStatus)
                                {
                                    ImportErrorInfo error = new ImportErrorInfo();
                                    error.GroupName = string.Format("[{0}]", pair.Value);
                                    error.ImportErrorResean = $"交易场所不一致，请修改后导入";
                                    checkMMErrList.Add(error);
                                    continue;
                                }
                            }

                            foreach (DataRow dr in importDT.Rows)
                            {

                                // 插入前检查
                                List<ImportErrorInfo> errList;
                                MMsheOrderParamReq MMsheOrderParamReq = CheckImportMMList(dr, request.mmList, serverCheckDic, out errList, pair.Value, isUnifyGroup, false);
                                if (errList != null && errList.Count > 0)
                                {
                                    foreach (ImportErrorInfo errorInfo in errList)
                                    {
                                        checkMMErrList.Add(errorInfo);
                                    }

                                    continue;
                                }

                                request.mmList.Add(new STC.Common.Model.MMshe.MMshe()
                                {
                                    securityid = MMsheOrderParamReq.securityid,
                                    //symbol = addBondWind.symbol,
                                    //remark = $"{addBondWind.securityID}  {addBondWind.symbol}  {addBondWind.remainingTime}",
                                    terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version),
                                    groupid = groupID,
                                    groupname = MMsheGroupInfo.groupname,
                                    settltype = MMsheOrderParamReq.baseSettleType,
                                    enable = "0",
                                    symbol = MMsheOrderParamReq.symbol,
                                    venues = MMsheOrderParamReq.venues,
                                    venuetypegw = MMsheOrderParamReq.venuetypegw,
                                    Params = MMsheOrderParamReq
                                }); ;
                                validCount++;

                            }
                        }
                        else
                        {
                            if (isUnifyGroup)
                            {
                                MMsheGroupInfo.inf1 = MMsheConstants.MM_GROPE_TYPE_UNIFY;
                            }
                            else
                            {
                                MMsheGroupInfo.inf1 = MMsheConstants.MM_GROPE_TYPE_NORMAL;
                            }
                        }


                        checkImportDic[MMsheGroupInfo.groupname] = new int[] { validCount, importDT != null ? importDT.Rows.Count : 0 };
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
                            List<STC.Common.Model.MMshe.MMshe> mmList = request.mmList.Where(s => s.securityid.Equals(errorInfo.SecurityID)).Select(a => a).ToList();
                            foreach (STC.Common.Model.MMshe.MMshe mm in mmList)
                            {
                                int[] count = checkImportDic[mm.groupname];
                                if (count != null)
                                {
                                    int validCount = count[0];
                                    if (validCount > 0)
                                    {
                                        validCount = validCount - 1;
                                    }
                                    checkImportDic[mm.groupname] = new int[] { validCount, count[1] };
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
                        ObservableCollection<MMsheGroupInfo> deleteGroup = new ObservableCollection<MMsheGroupInfo>();// 删除组
                        ObservableCollection<MMsheGroupInfo> clearGroup = new ObservableCollection<MMsheGroupInfo>();// 内容清理
                        if (MMsheGroupHandler.Instance.CacheSortedGroups != null)
                        {
                            foreach (MMsheGroupInfo item in MMsheGroupHandler.Instance.CacheSortedGroups)
                            {
                                int count = 0;
                                if (string.IsNullOrEmpty(item.id) == false)
                                {
                                    lock (string.Intern(item.id))
                                    {
                                        List<STC.Common.Model.MMshe.MMshe> mmList = MMsheGroupHandler.Instance.GetMMsByGroupID(item.id);
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

                        foreach (MMsheGroupInfo item in newMMsheGroupInfoList)
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
                            foreach (MMsheGroupInfo item in deleteGroup)
                            {
                                MMsheGroupReq MMsheRequest = new MMsheGroupReq();
                                MMsheRequest.action = "3";//删除
                                MMsheRequest.id = item.id;
                                MMsheRequest.venuetypegw = MMsheRequest.venues = item.venuetypegw;
                                PResult groupResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(MMsheRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    if (groupResult.IsSuccess())
                                    {
                                        await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                                        Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.DelGroup, item.id, item.groupname);
                                    }
                                    else
                                    {
                                        CustomMessageBox.Warning(groupResult.Msg);
                                    }
                                }
                            }
                            foreach (MMsheGroupInfo item in updateMMsheGroupInfoList)
                            {
                                MMsheGroupReq MMsheRequest = new MMsheGroupReq();
                                MMsheRequest.groupname = item.groupname;//组名
                                MMsheRequest.action = "2";//更新
                                MMsheRequest.ordersn = item.ordersn.ToString();//顺序
                                MMsheRequest.terminal = LocalEnv.ServerInfo.Version; // 终端
                                MMsheRequest.id = item.id;
                                if (!string.IsNullOrEmpty(item.inf1))
                                {
                                    MMsheRequest.inf1 = item.inf1;
                                    if (MMsheRequest.inf1.Equals(MMsheConstants.MM_GROPE_TYPE_UNIFY))
                                    {
                                        MMsheRequest.inf2 = item.inf2;
                                    }
                                }
                                string venue = "";
                                foreach (STC.Common.Model.MMshe.MMshe obj in request.mmList)
                                {
                                    if (obj.groupname.Equals(MMsheRequest.groupname))
                                    {
                                        venue = obj.venues;
                                        break;
                                    }
                                }
                                MMsheRequest.venues = venue;
                                MMsheRequest.venuetypegw = venue;
                                PResult groupResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(MMsheRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    sortedGroups = await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                                    if (sortedGroups != null)
                                    {
                                        foreach (MMsheGroupInfo gi in sortedGroups)
                                        {
                                            if (gi.id.Equals(MMsheRequest.id))
                                            {
                                                Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, item.groupname);
                                                foreach (STC.Common.Model.MMshe.MMshe obj in request.mmList)
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
                            foreach (MMsheGroupInfo item in newMMsheGroupInfoList)
                            {
                                MMsheGroupReq MMsheRequest = new MMsheGroupReq();
                                MMsheRequest.groupname = item.groupname;//组名
                                MMsheRequest.action = "1";//新增
                                MMsheRequest.ordersn = item.ordersn.ToString();//顺序
                                MMsheRequest.terminal = LocalEnv.ServerInfo.Version; // 终端
                                if (!string.IsNullOrEmpty(item.inf1))
                                {
                                    MMsheRequest.inf1 = item.inf1;
                                    if (MMsheRequest.inf1.Equals(MMsheConstants.MM_GROPE_TYPE_UNIFY))
                                    {
                                        OrderParamProp orderParam = new OrderParamProp(null);
                                        MMsheRequest.inf2 = item.inf2;
                                    }
                                }
                                string venue = "";
                                foreach (STC.Common.Model.MMshe.MMshe obj in request.mmList)
                                {
                                    if (obj.groupname.Equals(MMsheRequest.groupname))
                                    {
                                        venue = obj.venues;
                                        break;
                                    }
                                }
                                MMsheRequest.venues = venue;
                                MMsheRequest.venuetypegw = venue;
                                PResult groupResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(MMsheRequest);
                                if (groupResult != null && groupResult.Code == "0000")
                                {
                                    sortedGroups = await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                                    if (sortedGroups != null)
                                    {
                                        MMsheGroupInfo gi = sortedGroups.Last();
                                        Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.AddGroup, gi.id, item.groupname);
                                        foreach (STC.Common.Model.MMshe.MMshe obj in request.mmList)
                                        {
                                            if (string.IsNullOrEmpty(obj.groupid) && obj.groupname.Equals(gi.groupname))
                                            {
                                                obj.groupid = gi.id;
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (MMsheGroupInfo item in clearGroup)
                            {
                                STC.Common.Model.MMshe.MMshe obj = new STC.Common.Model.MMshe.MMshe();
                                obj.groupid = item.id;
                                request.mmList.Add(obj);
                            }

                            request.mmList.Reverse();

                            #region 更新组顺序
                            try
                            {
                                if (dic != null)
                                {
                                    sortedGroups = await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                                    int sn = 0;
                                    foreach (MMsheGroupInfo MMsheGroupInfo in sortedGroups)
                                    {
                                        if (MMsheGroupInfo.inf1 == MMsheConstants.MM_GROPE_TYPE_UNIFY)
                                        {
                                            sn++;
                                            MMsheGroupReq requestUpdateGroup = new MMsheGroupReq();
                                            requestUpdateGroup.id = MMsheGroupInfo.id;
                                            requestUpdateGroup.groupname = MMsheGroupInfo.groupname;//组名
                                            requestUpdateGroup.action = "2";//更新
                                            requestUpdateGroup.ordersn = sn.ToString();//顺序
                                            requestUpdateGroup.inf1 = MMsheGroupInfo.inf1;
                                            requestUpdateGroup.inf2 = MMsheGroupInfo.inf2;
                                            PResult updateGroupResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        }
                                    }
                                    foreach (KeyValuePair<int, string> pair in dic)
                                    {
                                        MMsheGroupInfo MMsheGroupInfo = MMsheGroupHandler.Instance.IsGroupNameExist(pair.Value);
                                        MMsheGroupReq requestUpdateGroup = new MMsheGroupReq();
                                        requestUpdateGroup.id = MMsheGroupInfo.id;
                                        requestUpdateGroup.groupname = pair.Value;//组名
                                        requestUpdateGroup.action = "2";//更新
                                        requestUpdateGroup.ordersn = (pair.Key + sn + 1).ToString();//顺序
                                        requestUpdateGroup.inf1 = MMsheGroupInfo.inf1;
                                        requestUpdateGroup.inf2 = MMsheGroupInfo.inf2;
                                        PResult updateGroupResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        //Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateGroups(requestUpdateGroup);
                                        //if (pResult1 != null && pResult1.Code == "0000")
                                        //{
                                        //    gi.action = 2;
                                        //    gi.ordersn = index;
                                        //    await Utils.Data.MMsheGroupHandler.Instance.AsyncRequestGroups();
                                        //    Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.UpdateGroup, gi.id, item.groupname);
                                        //}
                                        //index++;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                LogHandler.Error(e);
                            }
                            Utils.Data.MMsheGroupHandler.Instance.Notify(Utils.Data.MMsheGroupHandler.FavoriteTypeEnum.UpdateGroup, null, null);
                            #endregion
                            PResult pResult = MMsheHandler.Instance.mmUpdate(request);
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

        private MMsheOrderParamReq covertGroupData(DataRow firstRow)
        {
            MMsheOrderParamReq req = new MMsheOrderParamReq();
            req.InitData();

            return req;
        }

        private bool checkUnifyGroup(DataTable importDT)
        {
            bool isUnifyGroup = false;
            if (importDT.Columns.Contains(MMsheConstants.QuoteModel_ExcelField))
            {
                isUnifyGroup = true;
            }
            return isUnifyGroup;
        }

        private bool checkGroup(DataTable importDT)
        {
            bool isUnifyGroupOnly = true;
            DataView dv = new DataView(importDT);
            if (importDT.Columns.Contains(MMsheConstants.QuoteModel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.QuoteModel_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidBaseQty_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidBaseQty_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrBaseQty_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrBaseQty_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidBaseReSendValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidBaseReSendValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrBaseReSendValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrBaseReSendValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BaseSettleType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BaseSettleType_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidDefenReferMarketType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidDefenReferMarketType_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidDefenReferLevel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidDefenReferLevel_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrDefenReferMarketType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrDefenReferMarketType_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrDefenReferLevel_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrDefenReferLevel_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidAlgoDefenValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidAlgoDefenValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrAlgoDefenValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrAlgoDefenValue_ExcelField);
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
            /*if (importDT.Columns.Contains(MMsheConstants.BidAlgoSafeValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidAlgoSafeValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrAlgoSafeValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrAlgoSafeValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidAlgoTrigValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidAlgoTrigValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrAlgoTrigValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrAlgoTrigValue_ExcelField);
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
            /*if (importDT.Columns.Contains(MMsheConstants.AlgoMidType_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.AlgoMidType_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.AlgoMidOffsetValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.AlgoMidOffsetValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.AlgoValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.AlgoValue_ExcelField);
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
            //if (importDT.Columns.Contains(MMsheConstants.PosFakEnable_ExcelField))
            //{
            //    bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.PosFakEnable_ExcelField);
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
            //if (importDT.Columns.Contains(MMsheConstants.PosFakSpread_ExcelField))
            //{
            //    bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.PosFakSpread_ExcelField);
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
            //if (importDT.Columns.Contains(MMsheConstants.MaxFloorEnable_ExcelField))
            //{
            //    bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.MaxFloorEnable_ExcelField);
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
            //if (importDT.Columns.Contains(MMsheConstants.MaxFloor_ExcelField))
            //{
            //    bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.MaxFloor_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.DefenCDCEnable_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.DefenCDCEnable_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.BidDefenCDCValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.BidDefenCDCValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.OfrDefenCDCValue_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.OfrDefenCDCValue_ExcelField);
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
            if (importDT.Columns.Contains(MMsheConstants.VenueTypeGW_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.VenueTypeGW_ExcelField);
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
        private bool groupFieldUnique(DataView dv, string fieldName)
        {
            DataTable newDt = dv.ToTable(true, fieldName);
            if (newDt.Rows.Count > 1)
            {
                return false;
            }
            return true;
        }
        private MMsheOrderParamReq CheckImportMMList(DataRow dr, List<STC.Common.Model.MMshe.MMshe> mmList, Dictionary<string, string> severChkDic, out List<ImportErrorInfo> errorList, string sheetName, bool isGroupSheet, bool isTitle)
        {

            string securityID = CommonUtils.ConvertToString(dr[MMsheConstants.SecurityID_ExcelField]);
            //if (securityID.Length > 0 && securityID.IndexOf(".") == -1 && securityID.EndsWith(".IB", StringComparison.CurrentCultureIgnoreCase) == false)
            //{
            //    securityID += ".IB";
            //}
            string quoteModel = "";
            string bidQuoteModel = "";
            string ofrQuoteModel = "";
            string manualBid = "";
            string manualOfr = "";
            string stepLength = "";
            /*string bidBaseWidenValue = "";
            string ofrBaseWidenValue = "";
            string baseERReSendType = "";
            string dealWidenLowerTime = "";
            string dealWidenUpperTime = "";*/
            string algoBaseEnable = "";
            string algoBaseSecurityid = "";
            string algoMidDetailDefined = "";
            if (isGroupSheet)
            {
                quoteModel = CommonUtils.ConvertToString(dr[MMsheConstants.QuoteModel_ExcelField]);
                if (isTitle)
                {
                    securityID = "";
                }
            }
            else
            {
                bidQuoteModel = CommonUtils.ConvertToString(dr[MMsheConstants.BidQuoteModel_ExcelField]);
                ofrQuoteModel = CommonUtils.ConvertToString(dr[MMsheConstants.OfrQuoteModel_ExcelField]);
                manualBid = CommonUtils.ConvertToString(dr[MMsheConstants.ManualBid_ExcelField]);
                manualOfr = CommonUtils.ConvertToString(dr[MMsheConstants.ManualOfr_ExcelField]);
                stepLength = CommonUtils.ConvertToString(dr[MMsheConstants.StepLength_ExcelField]);
                stepLength = string.IsNullOrEmpty(stepLength) ? "" : stepLength;
            }
            string bidBaseQty = CommonUtils.ConvertToString(dr[MMsheConstants.BidBaseQty_ExcelField]);
            string ofrBaseQty = CommonUtils.ConvertToString(dr[MMsheConstants.OfrBaseQty_ExcelField]);
            string bidBaseReSendValue = CommonUtils.ConvertToString(dr[MMsheConstants.BidBaseReSendValue_ExcelField]);
            bidBaseReSendValue = string.IsNullOrEmpty(bidBaseReSendValue) ? MMsheConstants.ZERO_VALUE : bidBaseReSendValue;
            string ofrBaseReSendValue = CommonUtils.ConvertToString(dr[MMsheConstants.OfrBaseReSendValue_ExcelField]);
            ofrBaseReSendValue = string.IsNullOrEmpty(ofrBaseReSendValue) ? MMsheConstants.ZERO_VALUE : ofrBaseReSendValue;

            string settltype = CommonUtils.ConvertToString(dr[MMsheConstants.BaseSettleType_ExcelField]);
            string bidAlgoDefenValue = CommonUtils.ConvertToString(dr[MMsheConstants.BidAlgoDefenValue_ExcelField]);
            bidAlgoDefenValue = string.IsNullOrEmpty(bidAlgoDefenValue) ? MMsheConstants.ZERO_VALUE : bidAlgoDefenValue;
            string ofrAlgoDefenValue = CommonUtils.ConvertToString(dr[MMsheConstants.OfrAlgoDefenValue_ExcelField]);
            ofrAlgoDefenValue = string.IsNullOrEmpty(ofrAlgoDefenValue) ? MMsheConstants.ZERO_VALUE : ofrAlgoDefenValue;

            string defenCDCEnable = CommonUtils.ConvertToString(dr[MMsheConstants.DefenCDCEnable_ExcelField]);
            defenCDCEnable = string.IsNullOrEmpty(defenCDCEnable) ? MMsheConstants.NO_VALUE : defenCDCEnable;

            string bidDefenCDCValue = CommonUtils.ConvertToString(dr[MMsheConstants.BidDefenCDCValue_ExcelField]);
            string ofrDefenCDCValue = CommonUtils.ConvertToString(dr[MMsheConstants.OfrDefenCDCValue_ExcelField]);

            string bidDefenReferMarketType = CommonUtils.ConvertToString(dr[MMsheConstants.BidDefenReferMarketType_ExcelField]);
            string bidDefenReferLevel = CommonUtils.ConvertToString(dr[MMsheConstants.BidDefenReferLevel_ExcelField]);

            string ofrDefenReferMarketType = CommonUtils.ConvertToString(dr[MMsheConstants.OfrDefenReferMarketType_ExcelField]);
            string ofrDefenReferLevel = CommonUtils.ConvertToString(dr[MMsheConstants.OfrDefenReferLevel_ExcelField]);
            //string exeBondFollow = "";
            //if (!isGroupSheet)
            //{
            //    exeBondFollow = CommonUtils.ConvertToString(dr[MMsheConstants.ExeBondFollow_ExcelField]);
            //}
            string dealOpen = CommonUtils.ConvertToString(dr[MMsheConstants.DealClose_ExcelField]);
            dealOpen = string.IsNullOrEmpty(dealOpen) ? MMsheConstants.NO_VALUE : dealOpen;
            errorList = new List<ImportErrorInfo>();

            // 债券
            ImportErrorInfo error = null;

            string venue = CommonUtils.ConvertToString(dr[MMsheConstants.VenueTypeGW_ExcelField]);
            if (string.IsNullOrEmpty(venue) || (venue != MMsheConstants.SSE_DEFENREFERMARKETTYPE
                && venue != MMsheConstants.SSF_DEFENREFERMARKETTYPE
                && venue != MMsheConstants.SZF_DEFENREFERMARKETTYPE
                && venue != MMsheConstants.SZE_DEFENREFERMARKETTYPE))
            {
                ImportErrorInfo errorInfo = new ImportErrorInfo();
                errorInfo.GroupName = string.Format("[{0}]", sheetName);
                errorInfo.SecurityID = securityID;
                errorInfo.ImportKey = "交易场所";
                errorInfo.ImportErrorResean = $"不能为空，可选范围：{MMsheConstants.SSE_DEFENREFERMARKETTYPE}, " +
                    $"{MMsheConstants.SSF_DEFENREFERMARKETTYPE}, {MMsheConstants.SZF_DEFENREFERMARKETTYPE}, {MMsheConstants.SZE_DEFENREFERMARKETTYPE}";
                errorList.Add(errorInfo);
            }
            venue = ComboSourceUtil.Instance.FindEnum(venue, "venues_cn");
            error = CheckSecurityID(securityID, mmList, severChkDic, sheetName, false, venue);
            if (error != null)
                errorList.Add(error);

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
            if (MMsheConstants.API_QUOTEMODEL == bidQuoteModel)
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
            if (MMsheConstants.API_QUOTEMODEL == ofrQuoteModel)
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
                if (MMsheConstants.MANUAL_QUOTEMODEL == bidQuoteModel)
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
                if (MMsheConstants.MANUAL_QUOTEMODEL == ofrQuoteModel)
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
                if (MMsheConstants.MANUAL_QUOTEMODEL == ofrQuoteModel || MMsheConstants.MANUAL_QUOTEMODEL == bidQuoteModel)
                {
                    error = CheckValueStep(securityID, "", "调整步长", stepLength, "(bp)设置错误，取值需要在[0,100]范围内", 0m, 100m, sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(stepLength) == false)
                    {
                        error = CheckValueStep(securityID, "", "调整步长", stepLength, "(bp)设置错误，取值需要在[0,100]范围内", 0m, 100m, sheetName);
                        if (error != null)
                        {
                            errorList.Add(error);
                        }
                    }
                }
            }

            //重发阈值Bid(bp)
            error = CheckValue(securityID, "Bid", "重发阈值", bidBaseReSendValue, "(bp)设置错误，取值需要在[0, 1000]范围内", sheetName, 0, 1000);
            if (error != null) errorList.Add(error);

            //重发阈值Ofr(bp)
            error = CheckValue(securityID, "Ofr", "重发阈值", ofrBaseReSendValue, "(bp)设置错误，取值需要在[0, 1000]范围内", sheetName, 0, 1000);
            if (error != null) errorList.Add(error);
            // 清算速度
            error = CheckSettlType(securityID, settltype, sheetName);
            if (error != null) errorList.Add(error);

            if (MMsheConstants.DEFEND_QUOTEMODEL == bidQuoteModel)
            {
                //跟随点差Bid
                error = CheckValue(securityID, "Bid", "跟随点差", bidAlgoDefenValue, "(bp)设置错误，取值需要在[-100,100]范围内", sheetName, -100, 100);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidAlgoDefenValue) == false)
                {
                    error = CheckValue(securityID, "Bid", "跟随点差", bidAlgoDefenValue, "(bp)设置错误，取值需要在[-100,100]范围内", sheetName, -100, 100);
                    if (error != null)
                    {
                        bidAlgoDefenValue = MMsheConstants.ZERO_VALUE;
                    }
                }
            }

            if (MMsheConstants.DEFEND_QUOTEMODEL == ofrQuoteModel)
            {
                //跟随点差Ofr
                error = CheckValue(securityID, "Ofr", "跟随点差", ofrAlgoDefenValue, "(bp)设置错误，取值需要在[-100,100]范围内", sheetName, -100, 100);
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(ofrAlgoDefenValue) == false)
                {
                    error = CheckValue(securityID, "Ofr", "跟随点差", ofrAlgoDefenValue, "(bp)设置错误，取值需要在[-100,100]范围内", sheetName, -100, 100);
                    if (error != null)
                    {
                        ofrAlgoDefenValue = MMsheConstants.ZERO_VALUE;
                    }
                }
            }
            //是否跟随策略无可报价格时锚定中债估值
            error = CheckYesOrNo(securityID, "是否跟随策略无可报价格时锚定中债估值", defenCDCEnable, sheetName);
            if (error != null) errorList.Add(error);
            if ("是".Equals(defenCDCEnable))
            {
                if (string.IsNullOrEmpty(defenCDCEnable))
                {
                    defenCDCEnable = "0";
                }
                else
                {
                    error = CheckDefenCDCValue(securityID, "Bid", "估值拉宽", bidDefenCDCValue, "设置错误，取值需要在[0, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                    error = CheckDefenCDCValue(securityID, "Ofr", "估值拉宽", ofrDefenCDCValue, "设置错误，取值需要在[0, 1000]范围内", sheetName);
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
                    error = CheckDefenCDCValue(securityID, "Bid", "估值拉宽", bidDefenCDCValue, "设置错误，取值需要在[0, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
                if (!string.IsNullOrEmpty(ofrDefenCDCValue))
                {
                    error = CheckDefenCDCValue(securityID, "Ofr", "估值拉宽", ofrDefenCDCValue, "设置错误，取值需要在[0, 1000]范围内", sheetName);
                    if (error != null)
                    {
                        errorList.Add(error);
                    }
                }
            }

            if ((!string.IsNullOrEmpty(quoteModel) && MMsheConstants.DEFEND_QUOTEMODEL == quoteModel)
                || (!string.IsNullOrEmpty(bidQuoteModel) && MMsheConstants.DEFEND_QUOTEMODEL == bidQuoteModel))
            {
                // Bid防守参考价
                error = CheckDefenReferMarketType(securityID, bidDefenReferMarketType, sheetName, "Bid", venue);
                if (error != null) errorList.Add(error);

                error = CheckDefenReferLevel(securityID, bidDefenReferLevel, sheetName, "Bid");
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(bidDefenReferMarketType))
                {
                    bidDefenReferMarketType = MMsheConstants.SSE_VENUE.Equals(venue) ? MMsheConstants.SSE_DEFENREFERMARKETTYPE :
                        MMsheConstants.SSF_VENUE.Equals(venue) ? MMsheConstants.SSF_DEFENREFERMARKETTYPE :
                        MMsheConstants.SZF_VENUE.Equals(venue) ? MMsheConstants.SZF_DEFENREFERMARKETTYPE : MMsheConstants.SZE_DEFENREFERMARKETTYPE;
                }
                if (string.IsNullOrEmpty(bidDefenReferLevel))
                {
                    bidDefenReferLevel = MMsheConstants.LEVEL1;
                }

            }
            if ((!string.IsNullOrEmpty(quoteModel) && MMsheConstants.DEFEND_QUOTEMODEL == quoteModel)
               || (!string.IsNullOrEmpty(ofrQuoteModel) && MMsheConstants.DEFEND_QUOTEMODEL == ofrQuoteModel))
            {
                // Ofr防守参考价
                error = CheckDefenReferMarketType(securityID, ofrDefenReferMarketType, sheetName, "Ofr", venue);
                if (error != null) errorList.Add(error);

                error = CheckDefenReferLevel(securityID, ofrDefenReferLevel, sheetName, "Ofr");
                if (error != null) errorList.Add(error);
            }
            else
            {
                if (string.IsNullOrEmpty(ofrDefenReferMarketType))
                {
                    ofrDefenReferMarketType = MMsheConstants.SSE_VENUE.Equals(venue) ? MMsheConstants.SSE_DEFENREFERMARKETTYPE :
                         MMsheConstants.SSF_VENUE.Equals(venue) ? MMsheConstants.SSF_DEFENREFERMARKETTYPE :
                        MMsheConstants.SZF_VENUE.Equals(venue) ? MMsheConstants.SZF_DEFENREFERMARKETTYPE : MMsheConstants.SZE_DEFENREFERMARKETTYPE;
                }
                if (string.IsNullOrEmpty(ofrDefenReferLevel))
                {
                    ofrDefenReferLevel = MMsheConstants.LEVEL1;
                }
            }

            /*if (!isGroupSheet)
            {
                error = CheckExeFollow(securityID, exeBondFollow, sheetName);
                if (error != null) errorList.Add(error);
            }*/
            error = CheckYesOrNo(securityID, "成交后是否开启策略", dealOpen, sheetName);
            if (error != null) errorList.Add(error);
            if (errorList != null && errorList.Count > 0)
                return null;

            MMsheOrderParamReq MMsheOrderParamReq = new MMsheOrderParamReq();
            MMsheOrderParamReq.securityid = securityID;
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (bondsInfoPo != null)
            {
                MMsheOrderParamReq.symbol = bondsInfoPo.Short_Name;
            }
            if (MMsheConstants.DEFEND_QUOTEMODEL == bidQuoteModel) MMsheOrderParamReq.bidQuoteModel = "Defend";
            else if (MMsheConstants.MANUAL_QUOTEMODEL == bidQuoteModel) MMsheOrderParamReq.bidQuoteModel = "Manual";
            if (MMsheConstants.DEFEND_QUOTEMODEL == ofrQuoteModel) MMsheOrderParamReq.ofrQuoteModel = "Defend";
            else if (MMsheConstants.MANUAL_QUOTEMODEL == ofrQuoteModel) MMsheOrderParamReq.ofrQuoteModel = "Manual";
            MMsheOrderParamReq.bidBaseQty = bidBaseQty;
            MMsheOrderParamReq.ofrBaseQty = ofrBaseQty;
            MMsheOrderParamReq.manualBid = manualBid;
            MMsheOrderParamReq.manualOfr = manualOfr;
            MMsheOrderParamReq.stepLength = stepLength;
            MMsheOrderParamReq.bidBaseReSendValue = bidBaseReSendValue;
            MMsheOrderParamReq.ofrBaseReSendValue = ofrBaseReSendValue;
            MMsheOrderParamReq.bidBaseWidenValue = "5";
            MMsheOrderParamReq.ofrBaseWidenValue = "5";
            MMsheOrderParamReq.BaseERReSendType = "1";
            MMsheOrderParamReq.dealWidenLowerTime = "30";
            MMsheOrderParamReq.dealWidenUpperTime = "30";
            MMsheOrderParamReq.baseSettleType = MMsheConstants.T1_SETTLTYPE == settltype ? "2" : "1";
            MMsheOrderParamReq.bidAlgoDefenValue = bidAlgoDefenValue;
            MMsheOrderParamReq.ofrAlgoDefenValue = ofrAlgoDefenValue;
            MMsheOrderParamReq.algoBaseEnable = MMsheConstants.YES_VALUE == algoBaseEnable ? "1" : "0";
            MMsheOrderParamReq.algoBaseSecurityid = algoBaseSecurityid;
            MMsheOrderParamReq.algoMidDetailDefined = algoMidDetailDefined;
            MMsheOrderParamReq.posFakEnable = "0";
            MMsheOrderParamReq.posFakSpread = "0";
            MMsheOrderParamReq.defenCDCEnable = MMsheConstants.YES_VALUE == defenCDCEnable ? "1" : "0";
            MMsheOrderParamReq.bidDefenCDCValue = bidDefenCDCValue;
            MMsheOrderParamReq.ofrDefenCDCValue = ofrDefenCDCValue;
            if (MMsheConstants.QDMESP_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "1";
            else if (MMsheConstants.XBOND_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "2";
            else if (MMsheConstants.MARKET_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "3";
            else if (MMsheConstants.CFETS_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "4";
            else if (MMsheConstants.SSE_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "5";
            else if (MMsheConstants.SSF_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "6";
            else if (MMsheConstants.SZF_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "7";
            else if (MMsheConstants.SZE_DEFENREFERMARKETTYPE == bidDefenReferMarketType) MMsheOrderParamReq.bidDefenReferMarketType = "8";

            if (MMsheConstants.LEVEL1 == bidDefenReferLevel) MMsheOrderParamReq.bidDefenReferLevel = "1";
            else if (MMsheConstants.LEVEL2 == bidDefenReferLevel) MMsheOrderParamReq.bidDefenReferLevel = "2";
            else if (MMsheConstants.LEVEL3 == bidDefenReferLevel) MMsheOrderParamReq.bidDefenReferLevel = "3";
            else if (MMsheConstants.LEVEL4 == bidDefenReferLevel) MMsheOrderParamReq.bidDefenReferLevel = "4";
            else if (MMsheConstants.LEVEL5 == bidDefenReferLevel) MMsheOrderParamReq.bidDefenReferLevel = "5";

            if (MMsheConstants.QDMESP_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "1";
            else if (MMsheConstants.XBOND_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "2";
            else if (MMsheConstants.MARKET_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "3";
            else if (MMsheConstants.CFETS_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "4";
            else if (MMsheConstants.SSE_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "5";
            else if (MMsheConstants.SSF_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "6";
            else if (MMsheConstants.SZF_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "7";
            else if (MMsheConstants.SZE_DEFENREFERMARKETTYPE == ofrDefenReferMarketType) MMsheOrderParamReq.ofrDefenReferMarketType = "8";

            if (MMsheConstants.LEVEL1 == ofrDefenReferLevel) MMsheOrderParamReq.ofrDefenReferLevel = "1";
            else if (MMsheConstants.LEVEL2 == ofrDefenReferLevel) MMsheOrderParamReq.ofrDefenReferLevel = "2";
            else if (MMsheConstants.LEVEL3 == ofrDefenReferLevel) MMsheOrderParamReq.ofrDefenReferLevel = "3";
            else if (MMsheConstants.LEVEL4 == ofrDefenReferLevel) MMsheOrderParamReq.ofrDefenReferLevel = "4";
            else if (MMsheConstants.LEVEL5 == ofrDefenReferLevel) MMsheOrderParamReq.ofrDefenReferLevel = "5";
            MMsheOrderParamReq.wallEnable = "0";
            if (MMsheOrderParamReq.wallEnable.Equals("0"))
            {
                if (!string.IsNullOrEmpty(manualBid))
                {
                    MMsheOrderParamReq.exeBondPre = "1";
                    MMsheOrderParamReq.exeManualBid = MMsheCommonUtils.CalBond(securityID, "4", MMsheOrderParamReq.baseSettleType, manualBid, "1");
                }
                if (!string.IsNullOrEmpty(manualOfr))
                {
                    MMsheOrderParamReq.exeBondPre = "1";
                    MMsheOrderParamReq.exeManualOfr = MMsheCommonUtils.CalBond(securityID, "4", MMsheOrderParamReq.baseSettleType, manualOfr, "2");
                }
            }
            //含权债跟随
            /* MMsheOrderParamReq.exeBondFollow = "行权".Equals(exeBondFollow) ? "0" : "1";
             MMsheOrderParamReq.exeBondPre = "行权".Equals(exeBondFollow) ? "0" : "1";*/
            MMsheOrderParamReq.venues = venue;
            MMsheOrderParamReq.venuetypegw = venue;
            MMsheOrderParamReq.dealClose = MMsheConstants.YES_VALUE == dealOpen ? "1" : "0";
            return MMsheOrderParamReq;
        }


        private ImportErrorInfo CheckSecurityID(string securityId, List<STC.Common.Model.MMshe.MMshe> mmList, Dictionary<string, string> serverDic, string sheetName, bool checkWall, string venue)
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
                if (mmList.Find(o => o.securityid == securityId && o.venues == venue) != null)
                {
                    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                    securityIDError.SecurityID = securityId;
                    securityIDError.ImportErrorResean = "存在重复记录";
                    return securityIDError;
                }
                else
                {
                    if ((MMsheConstants.SSE_VENUE.Equals(venue) || MMsheConstants.SSF_VENUE.Equals(venue)) && !securityId.EndsWith(".SH"))
                    {
                        ImportErrorInfo securityIDError = new ImportErrorInfo();
                        securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        securityIDError.SecurityID = securityId;
                        securityIDError.ImportErrorResean = "暂不支持";
                        return securityIDError;
                    }
                    if ((MMsheConstants.SZF_VENUE.Equals(venue) || MMsheConstants.SZE_VENUE.Equals(venue)) && !securityId.EndsWith(".SZ"))
                    {
                        ImportErrorInfo securityIDError = new ImportErrorInfo();
                        securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        securityIDError.SecurityID = securityId;
                        securityIDError.ImportErrorResean = "暂不支持";
                        return securityIDError;
                    }
                    //BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
                    //if (CommonUtils.checkMarket(bondsInfoPo,venue) == false)
                    //{
                    //    ImportErrorInfo securityIDError = new ImportErrorInfo();
                    //    securityIDError.GroupName = string.Format("[{0}]", sheetName);
                    //    securityIDError.SecurityID = securityId;
                    //    securityIDError.ImportErrorResean = "暂不支持";
                    //    return securityIDError;
                    //}
                }
            }
            return null;
        }

        private ImportErrorInfo CheckBaseSecurityID(string securityId, string basesecurityId, string sheetName, string venue)
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
                else
                {

                    /*BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(basesecurityId);
                    if (CommonUtils.checkMarket(bondsInfoPo, venue) == false)
                    {
                        ImportErrorInfo securityIDError = new ImportErrorInfo();
                        securityIDError.GroupName = string.Format("[{0}]", sheetName);
                        securityIDError.SecurityID = securityId;
                        securityIDError.ImportKey = "基准券";
                        securityIDError.ImportErrorResean = "设置错误";
                        return securityIDError;
                    }*/
                }
            }

            return null;
        }

        private ImportErrorInfo CheckQuoteModel(string securityId, string side, string quoteModel, string sheetName)
        {
            if (string.IsNullOrEmpty(quoteModel)
                || (MMsheConstants.MANUAL_QUOTEMODEL != quoteModel && MMsheConstants.DEFEND_QUOTEMODEL != quoteModel))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "策略类型";
                error.ImportValue = side;
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.MANUAL_QUOTEMODEL}、{MMsheConstants.DEFEND_QUOTEMODEL}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckGroupQuoteModel(string securityId, string quoteModel, string sheetName)
        {
            if (string.IsNullOrEmpty(quoteModel)
                || (MMsheConstants.DEFEND_QUOTEMODEL != quoteModel))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "策略类型";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.DEFEND_QUOTEMODEL}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckBaseQty(string securityId, string side, string BaseQty, string sheetName)
        {
            if (string.IsNullOrEmpty(BaseQty)
                || !CommonUtils.IsNumber(BaseQty, 7, 0)
                || CommonUtils.ConvertToLong(BaseQty) < 10
                || CommonUtils.ConvertToLong(BaseQty) > 1000000)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityId;
                error.ImportKey = "报量";
                error.ImportValue = side;
                error.ImportErrorResean = "(W)设置错误，取值需要在[10, 1000000]范围内";
                return error;
            }

            PResult result = MMsheCommonUtils.QueryCirculSizeBySecurity(securityId, false);

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

            if (CommonUtils.ConvertToDecimalOrNull(result.Info1) != null && CommonUtils.ConvertToDecimalOrNull(result.Info1) > 0)
            {
                decimal maxQty = (decimal)CommonUtils.ConvertToDecimalOrNull(result.Info1);
                if (CommonUtils.ConvertToLong(BaseQty) >= maxQty)
                {
                    ImportErrorInfo error = new ImportErrorInfo();
                    error.GroupName = string.Format("[{0}]", sheetName);
                    error.SecurityID = securityId;
                    error.ImportKey = "报量";
                    error.ImportValue = side;
                    error.ImportErrorResean = $"(W)设置错误，取值需要小于债券余额[{maxQty}w]";
                    return error;
                }
            }
            else
            {
                if (!result.Code.Equals("0000") && (CommonUtils.ConvertToDecimalOrNull(result.Info1) == null || CommonUtils.ConvertToDecimalOrNull(result.Info1) == decimal.Zero)
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
                || (!CommonUtils.IsNumber(basePrice, 4, 3) && !CommonUtils.IsNegative(basePrice, 4, 3))
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

        private ImportErrorInfo CheckValue(string securityid, string side, string key, string value, string reason, string sheetName, decimal min, decimal max)
        {
            bool status = false;
            status = !CommonUtils.IsNumber(value, 4, 3);
            if (string.IsNullOrEmpty(value)
                || (!CommonUtils.IsNumber(value, 4, 3) && !CommonUtils.IsNegative(value, 4, 3))
                || CommonUtils.ConvertToDecimal(value) < min
                 || CommonUtils.ConvertToDecimal(value) > max)
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

        private ImportErrorInfo CheckValueStep(string securityid, string side, string key, string value, string reason, decimal min, decimal max, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || !CommonUtils.IsNumber(value, 2, 3)
                || CommonUtils.ConvertToDecimal(value) < min
                 || CommonUtils.ConvertToDecimal(value) > max
                 || !MMsheCommonUtils.CheckDecimalValid(value, max, min, 3))
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
                || (MMsheConstants.QTY_BASEERRESENDTYPE != ERReSendType && MMsheConstants.LEAVE_BASEERRESENDTYPE != ERReSendType))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "部分成交后报量";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.QTY_BASEERRESENDTYPE}，{MMsheConstants.LEAVE_BASEERRESENDTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckSettlType(string securityid, string SettlType, string sheetName)
        {
            if (string.IsNullOrEmpty(SettlType)
               || (MMsheConstants.T0_SETTLTYPE != SettlType && MMsheConstants.T1_SETTLTYPE != SettlType))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "清算速度";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.T0_SETTLTYPE}, {MMsheConstants.T1_SETTLTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckYesOrNo(string securityid, string key, string value, string sheetName)
        {
            if (MMsheConstants.YES_VALUE != value && MMsheConstants.NO_VALUE != value)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = key;
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.YES_VALUE}，{MMsheConstants.NO_VALUE}";
                return error;
            }
            return null;
        }
        private ImportErrorInfo CheckGroupAlgoMidType(string securityid, string AlgoMidType, string sheetName)
        {
            if (MMsheConstants.Val_ALGOMIDTYPE != AlgoMidType)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "参考中间价";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.Val_ALGOMIDTYPE}";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckAlgoMidType(string securityid, string AlgoMidType, string sheetName)
        {
            if (MMsheConstants.Mid_ALGOMIDTYPE != AlgoMidType && MMsheConstants.Val_ALGOMIDTYPE != AlgoMidType
                && MMsheConstants.Cus_ALGOMIDTYPE != AlgoMidType)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "参考中间价";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.Mid_ALGOMIDTYPE}，{MMsheConstants.Val_ALGOMIDTYPE}，{MMsheConstants.Cus_ALGOMIDTYPE}";
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
                || !CommonUtils.IsNumber(value, 6, 0)
                || CommonUtils.ConvertToLong(value) < 10
                 || CommonUtils.ConvertToLong(value) > 999990)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = "冰山量(W)";
                error.ImportErrorResean = "冰山量设置错误，取值需要在[1, 999]";
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDefenCDCValue(string securityid, string side, string key, string value, string reason, string sheetName)
        {
            if (string.IsNullOrEmpty(value)
                || (!CommonUtils.IsNumber(value, 4, 2) && !CommonUtils.IsNegative(value, 4, 2))
                || decimal.Compare(CommonUtils.ConvertToDecimal(value), 0) == -1
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

        private ImportErrorInfo CheckDefenReferMarketType(string securityid, string defenReferMarketType, string sheetName, string side, string venue)
        {
            string errorresean = "";
            if (MMsheConstants.SSE_VENUE.Equals(venue) && MMsheConstants.SSE_DEFENREFERMARKETTYPE != defenReferMarketType)
            {
                errorresean = $"设置错误，可选范围：{MMsheConstants.SSE_DEFENREFERMARKETTYPE}";
            }
            if (MMsheConstants.SSF_VENUE.Equals(venue) && MMsheConstants.SSF_DEFENREFERMARKETTYPE != defenReferMarketType)
            {
                errorresean = $"设置错误，可选范围：{MMsheConstants.SSF_DEFENREFERMARKETTYPE}";
            }
            if (MMsheConstants.SZF_VENUE.Equals(venue) && MMsheConstants.SZF_DEFENREFERMARKETTYPE != defenReferMarketType)
            {
                errorresean = $"设置错误，可选范围：{MMsheConstants.SZF_DEFENREFERMARKETTYPE}";
            }
            if (MMsheConstants.SZE_VENUE.Equals(venue) && MMsheConstants.SZE_DEFENREFERMARKETTYPE != defenReferMarketType)
            {
                errorresean = $"设置错误，可选范围：{MMsheConstants.SZE_DEFENREFERMARKETTYPE}";
            }
            if (!string.IsNullOrEmpty(errorresean))
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = $"{side}跟随参考市场价类型";
                error.ImportErrorResean = errorresean;
                return error;
            }
            return null;
        }

        private ImportErrorInfo CheckDefenReferLevel(string securityid, string defenReferLevel, string sheetName, string side)
        {
            if (MMsheConstants.LEVEL1 != defenReferLevel && MMsheConstants.LEVEL2 != defenReferLevel
                && MMsheConstants.LEVEL3 != defenReferLevel && MMsheConstants.LEVEL4 != defenReferLevel
                && MMsheConstants.LEVEL5 != defenReferLevel)
            {
                ImportErrorInfo error = new ImportErrorInfo();
                error.GroupName = string.Format("[{0}]", sheetName);
                error.SecurityID = securityid;
                error.ImportKey = $"{side}跟随参考市场价档位";
                error.ImportErrorResean = $"设置错误，可选范围：{MMsheConstants.LEVEL1}，{MMsheConstants.LEVEL2}，{MMsheConstants.LEVEL3}，{MMsheConstants.LEVEL4}，{MMsheConstants.LEVEL5}";
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
            MmsheSettingViewModel.MoniValueChange += MMSettingViewModel_MoniValueChange;
            STC.Common.Model.MMshe.UserSettingReplyPo result = MMsheHandler.Instance.queryMMUserSetting();
            if (result == null)
                return;

            STC.Common.Model.MMshe.MMsheUserSetting userSetting = new MMsheUserSetting();
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
            foreach (MmsheListBoxViewModel mmListBoxVm in mmListBoxVMdic.Values)
            {
                mmListBoxVm.UpdateMoniValue();
            }
        }


        private void ImportGroup()
        {
            if (MMsheGroupHandler.Instance.HasOpen(this.SelectGroup.id))
            {
                CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再批量导入");
                return;
            }

            ImportBondsView importBondsView = new ImportBondsView(this.SelectGroup);
            importBondsView.Title = "导入债券";
            importBondsView.ShowDialog();
            ///*ObservableCollection<HomeGroupInfo> AddHomeGroupDic = new ObservableCollection<HomeGroupInfo>();
            //foreach (MMsheGroupInfo pair in MMsheGroupHandler.Instance.CacheSortedGroups)
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
            //        if (MMsheGroupHandler.Instance.HasOpen(recognizeBondView.SelectedAddHomeGroup.id))
            //        {
            //            CustomMessageBox.Warning("当前分组内存在未关闭策略，请确认关闭后再批量导入");
            //            return;
            //        }
            //        MMsheGroupInfo saveGroupInfo = MMsheGroupHandler.Instance.GetGroupInfoByGroupID(recognizeBondView.SelectedAddHomeGroup.id);

            //        MMsheOrderParamReq param;
            //        try
            //        {
            //            if (MMsheConstants.MM_GROPE_TYPE_UNIFY.Equals(saveGroupInfo.inf1))
            //                param = JsonConvert.DeserializeObject<MMsheOrderParamReq>(saveGroupInfo.inf2);
            //            else
            //            {
            //                param = new MMsheOrderParamReq();
            //                param.InitData();
            //            }
            //        }
            //        catch
            //        {
            //            param = new MMsheOrderParamReq();
            //            param.InitData();
            //        }

            //        if (recognizeBondView.UpdateList != null && recognizeBondView.UpdateList.Count > 0)
            //        {
            //            List<STC.Common.Model.MMshe.MMshe> reqList = new List<STC.Common.Model.MMshe.MMshe>();
            //            foreach (HomeGroupSecurityidsUpdateReq drv in recognizeBondView.UpdateList)
            //            {
            //                STC.Common.Model.MMshe.MMshe request = new STC.Common.Model.MMshe.MMshe();
            //                request.Action = "1"; // 添加
            //                request.securityid = param.securityid = drv.securityid;
            //                request.symbol = drv.symbol;
            //                request.groupid = saveGroupInfo.id;
            //                request.settltype = "2";
            //                request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //                request.enable = "0";
            //                request.Params = param;
            //                request.venues = saveGroupInfo.venues;
            //                request.venuetypegw = saveGroupInfo.venuetypegw;
            //                reqList.Add(request);
            //            }
            //            if (reqList.Count > 0)
            //            {
            //                ImportBonds(reqList, saveGroupInfo);
            //            }
            //        }
            //    }
            //}*/
        }
        public async void ImportBonds(List<STC.Common.Model.MMshe.MMshe> reqList, MMsheGroupInfo saveGroupInfo)
        {
            int FailCount = 0;
            string ResultStr = "";
            int SuccessCount = 0;
            List<STC.Common.Model.MMshe.MMshe> existMMs = MMsheGroupHandler.Instance.GetMMsByGroupID(saveGroupInfo.id);
            try
            {
                MMsheReq request = new MMsheReq();
                request.Action = "1";
                request.mmList = new List<STC.Common.Model.MMshe.MMshe>();

                foreach (STC.Common.Model.MMshe.MMshe req in reqList)
                {
                    if (existMMs != null && existMMs.Any(item => item.securityid == req.securityid))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"当前组下债券{req.securityid}已存在" + "\r\n";
                    }
                    else if (CommonUtils.GetBondsInfoPoBySecurityID(req.securityid) == null
                        || !CommonUtils.checkBondVenue(saveGroupInfo.venuetypegw, CommonUtils.GetBondsInfoPoBySecurityID(req.securityid)))
                    {
                        FailCount = FailCount + 1;
                        ResultStr += $"债券{req.securityid}无效" + "\r\n";
                    }
                    else
                    {
                        request.mmList.Clear();
                        request.mmList.Add(req);
                        PResult pResult = await Utils.Data.MMsheGroupHandler.Instance.AsyncUpdateSecurityids(request);
                        if (pResult != null && pResult.IsSuccess())
                        {
                            SuccessCount = SuccessCount + 1;
                            foreach (STC.Common.Model.MMshe.MMshe mm in request.mmList)
                            {
                                BondsInfoPo bip = CommonUtils.GetBondsInfoPoBySecurityID(mm.securityid);
                                string remainingTime = bip == null ? string.Empty : bip.RemaingingTime;
                                ResultStr += $"{mm.securityid} {mm.symbol} {remainingTime}\r\n";
                            }
                        }
                        else
                        {
                            foreach (STC.Common.Model.MMshe.MMshe mm in request.mmList)
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
        private bool checkVenue(DataTable importDT)
        {
            bool venueSuccess = true;
            DataView dv = new DataView(importDT);
            if (importDT.Columns.Contains(MMsheConstants.VenueTypeGW_ExcelField))
            {
                bool isGroupFieldUnique = groupFieldUnique(dv, MMsheConstants.VenueTypeGW_ExcelField);
                if (!isGroupFieldUnique)
                {
                    venueSuccess = false;
                    return venueSuccess;
                }
            }
            else
            {
                venueSuccess = false;
                return venueSuccess;
            }
            return venueSuccess;
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
