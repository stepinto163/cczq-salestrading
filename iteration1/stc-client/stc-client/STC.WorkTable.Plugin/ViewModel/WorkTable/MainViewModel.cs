using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Model.WorkTable;
using STC.Common.Utils;
using STC.Common.View.WorkTable;
using STC.Frame.Common;
using STC.Frame.Utils.ExcelHelper;
using STC.WorkTable.Plugin.Utils;
using STC.WorkTable.Plugin.View;
using STC.WorkTable.Plugin.View.WorkTable;
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.WorkTable.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WtUtils = STC.WorkTable.Plugin.Utils.CommonUtils;

namespace STC.WorkTable.Plugin.ViewModel.WorkTable
{
    public class MainViewModel : BaseViewModel
    {
        #region 需求列表
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
        #endregion

        #region 历史需求
        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "政策性金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList_History { get { return _interestRateDebtList_History; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "政府支持机构债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList_History { get { return _creditBondList_History; } }

        private ObservableCollection<FilterListBoxItemData> _policyFinancialBondList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "农发", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "口行", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "国开", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> PolicyFinancialBondList_History { get { return _policyFinancialBondList_History; } }

        private ObservableCollection<FilterListBoxItemData> _remaingingTimeList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "0~1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "1~3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3~5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5~7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7~10Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = ">10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RemaingingTimeList_History { get { return _remaingingTimeList_History; } }

        private ObservableCollection<FilterListBoxItemData> _ratingList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "AAA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AAA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA-", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "A+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RatingList_History { get { return _ratingList_History; } }

        private ObservableCollection<FilterListBoxItemData> _settlTypeList_History = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "T+0", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "T+1", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> SettlTypeList_History { get { return _settlTypeList_History; } }
        #endregion


        private int _CurrentTotalCount = 0;
        public int CurrentTotalCount
        {
            get
            {
                return _CurrentTotalCount;
            }
            set
            {
                _CurrentTotalCount = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount = 0;
        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
            set
            {
                _TotalCount = value;
                this.OnPropertyChanged();
            }
        }

        private int _BidCount = 0;
        public int BidCount
        {
            get
            {
                return _BidCount;
            }
            set
            {
                _BidCount = value;
                this.OnPropertyChanged();
            }
        }


        private int _OfrCount = 0;
        public int OfrCount
        {
            get
            {
                return _OfrCount;
            }
            set
            {
                _OfrCount = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _statusDic = new Dictionary<string, string>();
        public Dictionary<string, string> StatusDic
        {
            get
            {
                if (_statusDic.Count == 0)
                {
                    _statusDic[string.Empty] = "全部"; 
                    _statusDic["2"] = "报价中";
                    _statusDic["4"] = "已暂停";
                    _statusDic["5"] = "已完成";
                }
                return _statusDic;
            }
        }

        private string _selectedStatus = string.Empty;

        public string SelectedStatus
        {
            get
            {
                return _selectedStatus;
            }
            set
            {
                _selectedStatus = value;
                this.OnPropertyChanged();
                FilterDatas();
            }
        }

        private bool _isMe;

        public bool IsMe
        {
            get
            {
                return _isMe;
            }
            set
            {
                _isMe = value;
                this.OnPropertyChanged();
                FilterDatas();
            }
        }
         
        private string _searchSecurityID;

        public string SearchSecurityID
        {
            get
            {
                return _searchSecurityID;
            }
            set
            {
                _searchSecurityID = value;
                this.OnPropertyChanged();
                FilterDatas();
            }
        }


        private TradeRequestData _selectedItem_Worked;
        public TradeRequestData SelectedItem_Worked
        {
            get
            {
                return this._selectedItem_Worked;
            }
            set
            {
                this._selectedItem_Worked = value;
                this.OnPropertyChanged();
            }
        }

        private string _StopAllBtnText = "暂停";

        public string StopAllBtnText
        {
            get { return _StopAllBtnText; }
            set
            {
                _StopAllBtnText = value;
                this.OnPropertyChanged();
            }
        }


        private bool _IgnoreEnable = true;
        public bool StopAllBtnEnable
        {
            get
            {
                return _IgnoreEnable;
            }
            set
            {
                _IgnoreEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _DoFirmOrderBtnEnable = true;
        public bool DoFirmOrderBtnEnable
        {
            get
            {
                return _DoFirmOrderBtnEnable;
            }
            set
            {
                _DoFirmOrderBtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        private object _workedLock = new object();
        private CollectionViewSource workedViewSource = new CollectionViewSource();
        public CollectionViewSource WorkedViewSource
        {
            get { return workedViewSource; }
            set { workedViewSource = value; }
        }

        private CustomCollection<TradeRequestData> tradeRequestInfo = new CustomCollection<TradeRequestData>();

        public CustomCollection<TradeRequestData> TradeRequestInfo
        {
            get { return tradeRequestInfo; }
            set { tradeRequestInfo = value; }
        }

        public ICommand QuoteIdentifyCommand { get; set; }

        public ICommand StopAllCommand { get; set; }

        public ICommand EndCommand { get; set; }

        public ICommand DoFirmOrderCommand { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// 历史导出
        /// </summary>
        public ICommand ExportCommand_History { get; set; }

        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                this._ExportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ExportEnabled_History = true;

        public bool ExportEnabled_History
        {
            get
            {
                return _ExportEnabled_History;
            }
            set
            {
                this._ExportEnabled_History = value;
                this.OnPropertyChanged();
            }
        }

        List<KeyValuePair<string, string>> mainExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("IntentionId", "需求ID"),
            new KeyValuePair<string, string>("SecurityId","代码"),
            new KeyValuePair<string, string>("Symbol","简称"),
            new KeyValuePair<string, string>("InstitutionName", "客户机构"),
            new KeyValuePair<string, string>("TraderName","客户名"),
            new KeyValuePair<string, string>("Side","方向" ),
            new KeyValuePair<string, string>("Price","报价(%)" ),
            new KeyValuePair<string, string>("Qty","报量(万元)" ),
            new KeyValuePair<string, string>("SettlType","清算速度" ),
            new KeyValuePair<string, string>("NikeName","经纪人" ),
            new KeyValuePair<string, string>("CompleteRate","完成度" ),
            new KeyValuePair<string, string>("Status","状态" ),
        };

        List<KeyValuePair<string, string>> historyExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("institutionName", "客户机构"),
            new KeyValuePair<string, string>("traderName","客户名"),
            new KeyValuePair<string, string>("createTime", "时间"),
            new KeyValuePair<string, string>("securityId","代码"),
            new KeyValuePair<string, string>("symbol","简称"),
            new KeyValuePair<string, string>("side","方向" ),
            new KeyValuePair<string, string>("price","报价(%)" ),
            new KeyValuePair<string, string>("qty","报量(万元)" ),
            new KeyValuePair<string, string>("lastPrice","成交价" ),
            new KeyValuePair<string, string>("profitLoss","PNL" ),
            new KeyValuePair<string, string>("runningTime","时长" ),
        };

        public MainViewModel()
        {
            try
            {
                BaseHandler.Instance.Init(Instance_OnClear);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }

            Thread refreshCheckBoxThread = new Thread(RefreshCheckBox);
            refreshCheckBoxThread.IsBackground = true;
            refreshCheckBoxThread.Start();

            #region 需求列表
            IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;

            this.WorkedViewSource.IsLiveFilteringRequested = true;
            this.WorkedViewSource.IsLiveSortingRequested = true;
            this.WorkedViewSource.Source = TradeRequestInfo;
            this.WorkedViewSource.View.CollectionChanged += WorkedViewSource_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(this.WorkedViewSource.View, _workedLock);

            this.QuoteIdentifyCommand = new ComRelayCommand(this.DoQuoteIdentifyCommand);
            this.StopAllCommand = new ComRelayCommand(this.DoStopAllCommand);
            this.EndCommand = new ComRelayCommand(this.DoEndCommand);
            this.DoFirmOrderCommand = new ComRelayCommand<string>(this.DoFirmOrder);
            this.ExportCommand = new ComRelayCommand(ExportCSV);
            this.ExportCommand_History = new ComRelayCommand(HistoryExportCSV);
            #endregion

            #region 需求历史

            this.historyViewSource.IsLiveFilteringRequested = true;
            this.historyViewSource.IsLiveSortingRequested = true;
            this.historyViewSource.Source = TradeRequestInfo_History;
            BindingOperations.EnableCollectionSynchronization(this.historyViewSource.View, _HistoryLock);
            #endregion
        }

        private void DoFirmOrder(string obj)
        {
            if (string.IsNullOrEmpty(obj))
            {
                // todo  单条报价？？
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量闪电报价操作？", "闪电报价"))
            {
                return;
            }

            bool hasBNC = false;
            bool hasBCO = false;

            this.DoFirmOrderBtnEnable = true;
            try
            {
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                foreach (TradeRequestData data in tradeRequestInfo)
                {
                    if (data.IsChecked == false)
                    {
                        continue;
                    }

                    if (data.Status != "2")
                    {
                        continue;
                    }

                    decimal firmOrderQty = GetQty(data, obj);
                    if (firmOrderQty == 0)
                    {
                        continue;
                    }

                    if (Common.Utils.CommonUtils.IsRate(data.SecurityType))
                    {
                        hasBNC = true;
                    }
                    else
                    {
                        hasBCO = true;
                    }

                    ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                    request.intentionId = data.IntentionId;
                    request.actionType = "2";// 闪电确认
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    request.crossVenues = "2";
                    request.venueTypeGw = "INNER";
                    request.venues = "MANUAL";
                    request.orderType = "2"; //1:市价单; 2:限价单
                    request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
                    request.firmFlag = "1";
                    // 不够的余量下单
                    request.qty = firmOrderQty;

                    data.Request = request;
                    orderDic[data.IntentionId] = data;
                }

                if (orderDic.Count == 0)
                {
                    DoFirmOrderBtnEnable = true;
                    return;
                }

                AssignVenuesWindow win = new AssignVenuesWindow();
                AssignVenuesWindowViewModel viewModel = win.DataContext as AssignVenuesWindowViewModel;
                if (hasBNC) viewModel.IsShowInterestRateDebt = Visibility.Visible;
                if (hasBCO) viewModel.IsShowCreditDebt = Visibility.Visible;
                bool? result = win.ShowDialog();

                if (result == false)
                {
                    DoFirmOrderBtnEnable = true;
                    return;
                }


                string bncSelfSelectVenues = win.InterestRateDebtECV.SelfSelectVenues;
                string bcoSelfSelectsVenues = win.CreditDebtECV.SelfSelectVenues;

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (TradeRequestData item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            if (Common.Utils.CommonUtils.IsRate(item.SecurityType))
                            {
                                if (!string.IsNullOrEmpty(bncSelfSelectVenues))
                                {
                                    item.Request.crossVenues = "1";
                                    item.Request.venuesSelectF = "3";
                                    item.Request.venuesRdmNum = null;
                                    item.Request.selfSelectVenues = bncSelfSelectVenues;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(bcoSelfSelectsVenues))
                                {
                                    item.Request.crossVenues = "1";
                                    item.Request.venuesSelectF = "3";
                                    item.Request.venuesRdmNum = null;
                                    item.Request.selfSelectVenues = bcoSelfSelectsVenues;
                                }
                            }
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoFirmOrder", e);
            }
            finally
            {
                this.DoFirmOrderBtnEnable = true;
            }
        }

        private decimal GetQty(TradeRequestData data, string obj)
        {
            decimal qtyOption = data.LeftQty.Value;
            if (Common.Utils.CommonUtils.IsNumeric(obj))
                qtyOption = obj.ConvertToDecimal();

            decimal leftQtyD = data.LeftQty.ConvertToDecimal();
            if (leftQtyD < qtyOption)
            {
                return leftQtyD;
            }
            return qtyOption;
        }

        private void WorkedViewSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.CurrentTotalCount = WorkedViewSource.View.Cast<object>().Count();
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => {

                        if (this.TradeRequestInfo != null)
                        {
                            this.TradeRequestInfo.Clear();
                        }
                        this.CurrentTotalCount = 0;
                        this.TotalCount = 0;
                        this.OfrCount = 0;
                        this.BidCount = 0;
                        this.CurrentTotalCount_History = 0;
                        this.TotalCount_History = 0;
                        this.OfrCount_History = 0;
                        this._BidCount_History = 0;
                        IntentionHandler.Instance.OnIntentionQuotes -= Instance_OnIntentionQuotes;
                        IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                    }));
                }
            }
        }

        private void Instance_OnIntentionQuotes(IntentionQuoteInfo obj)
        {
            if (obj.status == "0" || obj.status == "1" || obj.status == "3")
            {
                return;
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                TradeRequestData item = null;
                var findList = TradeRequestInfo.Where(x => x.IntentionId == obj.intentionId);
                if (findList != null && findList.Count() > 0)
                {
                    item = findList.First();
                    //if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                    //    && obj.status != "0")
                    //{
                    //    this.TradeRequestInfo.Remove(item);
                    //    return;
                    //}

                    if (obj.status != "2" && obj.status != "4" && obj.status != "5")
                    {
                        this.TradeRequestInfo.Remove(item);
                        TotalCount -= 1;
                        if (obj.side == "1")
                            BidCount -= 1;
                        else if (obj.side == "2")
                            OfrCount -= 1;
                        return;
                    }


                    item.Status = obj.status;

                    //if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    //{
                    //    if (string.IsNullOrEmpty(obj.orderId) == false)
                    //    {

                    //        // 更新我的订单/我的撤单
                    //        MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //           );
                    //        // 更新我的成交
                    //        MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //            );
                    //    }
                    //}
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.Side = obj.side;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    if (obj.qty.HasValue)
                        item.DisplayQty = (long)obj.qty / 1000;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    double completeRate = 0;
                    double.TryParse(obj.completeRate == null ? "0" : obj.completeRate.ToString(), out completeRate);
                    item.CompleteRate = completeRate;
                    item.BookQty = obj.bookQty;
                    item.NikeName = obj.nickName;
                    item.LeftQty = obj.leftQty;
                    if (item.SecurityId != obj.securityId)
                    {
                        item.SecurityId = obj.securityId;
                        item.Symbol = obj.symbol;
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
                        if (bondsInfoPo != null)
                        {
                            //Symbol = bondsInfoPo.Short_Name;
                            //Security = bondsInfoPo.Bond_ID;
                            //SymbolName = bondsInfoPo.Short_Name;
                            item.RemainingTime = bondsInfoPo.RemaingingTime;
                            item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                            item.BondCategory = Utils.CommonUtils.GetBondCategoryName(bondsInfoPo.Security_Type_Id);
                            item.Issuer = bondsInfoPo.issuer;
                            item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                            item.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                        }
                    }
                }
                else
                {
                    if (obj.status == "6")
                    {
                        return;
                    }

                    item = new TradeRequestData();
                    //if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    //{
                    //    if (string.IsNullOrEmpty(obj.orderId) == false)
                    //    {

                    //        // 更新我的订单/我的撤单
                    //        MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //           );
                    //        // 更新我的成交
                    //        MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //            );
                    //    }
                    //}
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.Side = obj.side;
                    item.SecurityId = obj.securityId;
                    item.Symbol = obj.symbol;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
                    if (bondsInfoPo != null)
                    {
                        //Symbol = bondsInfoPo.Short_Name;
                        //Security = bondsInfoPo.Bond_ID;
                        //SymbolName = bondsInfoPo.Short_Name;
                        item.RemainingTime = bondsInfoPo.RemaingingTime;
                        item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        item.BondCategory = Utils.CommonUtils.GetBondCategoryName(bondsInfoPo.Security_Type_Id);
                        item.Issuer = bondsInfoPo.issuer;
                        item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        item.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    if (obj.qty.HasValue)
                        item.DisplayQty = (long)obj.qty / 1000;
                    item.SettlType = obj.settlType;
                    //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
                    //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                    //DateTime settlDateTime;
                    //int selectIndex;
                    //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
                    //item.edit_settletype = selectIndex;

                    //item.BrokerId = obj.brokerId;
                    //item.BrokerName = obj.brokerName;
                    item.Text = obj.text;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.Status = obj.status;
                    double completeRate = 0;
                    double.TryParse(obj.completeRate == null ? "0" : obj.completeRate.ToString(),out completeRate);
                    item.CompleteRate = completeRate;
                    item.BookQty = obj.bookQty;
                    item.NikeName = obj.nickName;
                    item.LeftQty = obj.leftQty;

                    this.TradeRequestInfo.Insert(0, item);

                    TotalCount += 1;
                    if (obj.side == "1")
                        BidCount += 1;
                    else if (obj.side == "2")
                        OfrCount += 1;

                }

                isAdjustAllChkStatus = true;
            }));
        }

        private void FilterDatas()
        {
            this.WorkedViewSource.View.Filter = x => {
                TradeRequestData data = x as TradeRequestData;
                if (this.IsMe && data.UserName != LocalEnv.UserInfo.UserName)
                    return false;
                if (string.IsNullOrEmpty(this.SelectedStatus) == false && data.Status != this.SelectedStatus)
                    return false;
                if (string.IsNullOrEmpty(this.SearchSecurityID) == false && data.SecurityId != this.SearchSecurityID)
                    return false;
                if (!((interest == "1" && data.SecurityType == "BNC")
                   || (bondCategoryConditions_Interest.Count > 0 && bondCategoryConditions_Interest.Contains(data.BondCategory) && data.SecurityType == "BNC")
                   || (credit == "1" && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Credit.Count > 0 && bondCategoryConditions_Credit.Contains(data.BondCategory) && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Interest.Count == 0 && bondCategoryConditions_Credit.Count == 0 && string.IsNullOrEmpty(interest) && string.IsNullOrEmpty(credit))))
                    return false;
                if (policyFinancialBondConditions.Count > 0 && !policyFinancialBondConditions.Contains(data.Issuer))
                    return false;
                if (RemainingTimeConditions.Count > 0)
                {
                    bool flag = false;
                    foreach(int[] range in RemainingTimeConditions)
                    {
                        if (range.Length == 2)
                        {
                            if (data.RemaingingTime_convert >= range[0] && data.RemaingingTime_convert < range[1])
                            {
                                flag = true;
                                break;
                            }
                        }
                        else if (range.Length == 1)
                        {
                            if (data.RemaingingTime_convert >= range[0])
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                        return false;
                }
                if (ratingConditions.Count > 0)
                {
                    bool flag = false;
                    if (ratingConditions.Contains(data.IssuerRatingCurrentNPY))
                    {
                        flag = true;
                    }
                    else if (ratingConditions.Contains("其他") && (string.IsNullOrEmpty(data.IssuerRatingCurrentNPY) || !rateOptions.Contains(data.IssuerRatingCurrentNPY)))
                    {
                        flag = true;
                    }
                    if (!flag)
                        return false;
                }
                if (settlTypeConditions.Count > 0 && !policyFinancialBondConditions.Contains(data.SettlType))
                    return false;
                return true; 
            };
        }


        private void DoQuoteIdentifyCommand()
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.RECOGENIZEQUOTE))
            {
                RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin();
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
            }
        }

        internal void DoStopOne()
        {
            string intentionId = this.SelectedItem_Worked.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "5";//暂停 
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.venueTypeGw = "INNER";
            request.venues = "MANUAL";
            request.orderType = "2"; //1:市价单; 2:限价单
            request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
            request.firmFlag = "0";
            request.qty = this.SelectedItem_Worked.Qty;

            IntentionOrder io = new IntentionOrder();
            io.request = request;
            io.detailIntentionItem = new TradeRequestData();
            io.detailIntentionItem.SecurityId = this.SelectedItem_Worked.SecurityId;
            io.detailIntentionItem.Side = this.SelectedItem_Worked.Side;
            io.detailIntentionItem.Price = this.SelectedItem_Worked.Price.ConvertToDecimal();
            io.detailIntentionItem.Qty = this.SelectedItem_Worked.Qty;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                    if (replyPo == null)
                    {
                        WtUtils.Prompt2Second("警告", $"意向id:[{intentionId}]暂停失败");
                    }
                    else
                    {
                        if (!replyPo.IsSuccess())
                        {
                            if (replyPo.Code == "10000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SendPriceIntentionWarnView warnView = new SendPriceIntentionWarnView(replyPo, io);
                                    warnView.ShowDialog();
                                }));

                            }
                            else
                            {
                                WtUtils.Prompt2Second("警告", replyPo.Msg);
                            }
                        }
                        else
                        {
                            //replyUpdateThread.Enqueue(replyPo);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoStopOne", e);
                }
            });
        }

        internal void DoStartOne()
        {
            string intentionId = this.SelectedItem_Worked.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            //request.replace
            request.crossVenues = "2";
            request.actionType = "6";//继续 
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.venueTypeGw = "INNER";
            request.venues = "MANUAL";
            request.orderType = "2"; //1:市价单; 2:限价单
            request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
            request.firmFlag = "0";
            request.qty = this.SelectedItem_Worked.Qty;

            IntentionOrder io = new IntentionOrder();
            io.request = request;
            io.detailIntentionItem = new TradeRequestData();
            io.detailIntentionItem.SecurityId = this.SelectedItem_Worked.SecurityId;
            io.detailIntentionItem.Side = this.SelectedItem_Worked.Side;
            io.detailIntentionItem.Price = this.SelectedItem_Worked.Price.ConvertToDecimal();
            io.detailIntentionItem.Qty = this.SelectedItem_Worked.Qty;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                    if (replyPo == null)
                    {
                        WtUtils.Prompt2Second("警告", $"意向id:[{intentionId}]继续失败");
                    }
                    else
                    {
                        if (!replyPo.IsSuccess())
                        {
                            if (replyPo.Code == "10000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SendPriceIntentionWarnView warnView = new SendPriceIntentionWarnView(replyPo, io);
                                    warnView.ShowDialog();
                                }));

                            }
                            else
                            {
                                WtUtils.Prompt2Second("警告", replyPo.Msg);
                            }
                        }
                        else
                        {
                            //replyUpdateThread.Enqueue(replyPo);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoStartOne", e);
                }
            });
        }



        private void DoStopAllCommand()
        {

            if (!Common.View.Component.CustomMessageBox.Question($"是否执行{this.StopAllBtnText}操作？", this.StopAllBtnText))
            {
                return;
            }

            this.StopAllBtnEnable = true;
            try
            {
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                foreach (TradeRequestData data in tradeRequestInfo)
                {
                    if (!data.IsChecked)
                    {
                        continue;
                    }

                    if (this.StopAllBtnText == "暂停")
                    {
                        if (data.Status != "2")
                        {
                            continue;
                        }
                    }
                    else if (this.StopAllBtnText == "启动")
                    {
                        if (data.Status != "4")
                        {
                            continue;
                        }
                    }

                    ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                    request.intentionId = data.IntentionId;
                    request.actionType = this.StopAllBtnText == "暂停" ? "5" : "6";// 停止
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    request.crossVenues = "2";
                    request.venueTypeGw = "INNER";
                    request.venues = "MANUAL";
                    request.orderType = "2"; //1:市价单; 2:限价单
                    request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
                    request.firmFlag = "0";
                    // 不够的余量下单
                    request.qty = data.Qty;
                    data.Request = request;
                    orderDic[data.IntentionId] = data;
                }
                this.StopAllBtnText = this.StopAllBtnText == "暂停" ? "启动" : "暂停";

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (TradeRequestData item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoStopAllCommand", e);
            }
            finally
            {
                this.StopAllBtnEnable = true;
            }
        }


        private void GetErrorInfo(ConfirmIntentionQuoteRequest request,
                ref int failCount,
                ref int totalCount,
                Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        private void DoEndCommand()
        {
            if (this.SelectedItem_Worked != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行结单操作？", "结单"))
                {
                    return;
                }

                if (!WindowManager.Instance.IsExist(WindowKeyHelper.ENDINTENTIONWINDOW))
                {
                    EndIntentionWindow win = new EndIntentionWindow();
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.ENDINTENTIONWINDOW, win);
                }
            }
        }

        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
                    if (isAdjustAllChkStatus)
                    {
                        isAdjustAllChkStatus = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus();
                        }));
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #region 待处理
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private bool _hasSelected = false;
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool isAdjustAllChkStatus = false;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {

            lock (_workedLock)
            {
                foreach (TradeRequestData item in this.workedViewSource.View)
                {

                    item.IsChecked = isAllChecked;
                }
            }


            if (this.workedViewSource.View.IsEmpty == true)
            {
                IsAllChecked = false;
                _hasSelected = false;
            }
            else
            {
                IsAllChecked = isAllChecked;
                _hasSelected = isAllChecked;
            }
        }

        public void AdjustAllChkStatus()
        {
            bool isCheckAll = true;

            lock (_workedLock)
            {
                this._hasSelected = false;
                foreach (TradeRequestData item in this.workedViewSource.View)
                {
                    if (item.IsChecked == true)
                    {
                        this._hasSelected = true;
                    }
                    if (item.IsChecked == false)
                    {
                        isCheckAll = false;
                    }
                    if (this._hasSelected == true && isCheckAll == false)
                    {
                        break;
                    }
                }
                IsAllChecked = isCheckAll && this.workedViewSource.View.IsEmpty == false;
            }

        }

        public void CheckBoxSingleClick()
        {
            isAdjustAllChkStatus = true;
        }


        private async void ExportCSV()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            this.ExportEnabled = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_需求列表";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = CSVHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(mainExportDic));

                        var exportList = this.WorkedViewSource.View.Cast<TradeRequestData>();
                        await Task.Run(() =>
                        {
                            foreach (TradeRequestData item in exportList)
                            {
                                //if (item.Status == "0" && (item.UserName == null || item.UserName == LocalEnv.UserInfo.UserName))
                                {
                                    StringBuilder lineSB = new StringBuilder();
                                    try
                                    {
                                        foreach (KeyValuePair<string, string> pair in mainExportDic)
                                        {
                                            string field = pair.Key;

                                            object fieldValue = typeof(TradeRequestData).GetProperty(field).GetValue(item);

                                            string value = Common.Utils.CommonUtils.ConvertToString(fieldValue);

                                            if (field.Equals("Side", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "side");
                                            }
                                            else if (field.Equals("SettlType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "settletype");
                                            }
                                            else if (field.Equals("Price", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = value.ConvertToDecimal().ToString("0.00#");
                                            }
                                            else if (field.Equals("Qty", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = value.ConvertToDecimal().ToString("0");
                                            }
                                            else if (field.Equals("Status", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "intentionstatus1");
                                            }
                                            if (lineSB.Length > 0)
                                            {
                                                lineSB.Append(",");
                                            }

                                            lineSB.Append($"\"{value}\"");
                                        }
                                        writer.WriteLine(lineSB);
                                        successCount++;
                                    }
                                    catch (Exception e)
                                    {
                                        LogHelper.Error(e);
                                        failCount++;
                                    }
                                    totalSize++;
                                }

                            }
                            this.ExportEnabled = true;
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
                else
                {
                    this.ExportEnabled = true;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }

        }

        private string GetHeaderLine(List<KeyValuePair<string, string>> list)
        {
            StringBuilder headerSb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in list)
            {
                if (headerSb.Length > 0)
                {
                    headerSb.Append(",");
                }
                headerSb.Append(pair.Value);
            }

            return headerSb.ToString();
        }
        #endregion

        #region 历史需求

        private TradeRequestData _selectedItem_History;
        public TradeRequestData SelectedItem_History
        {
            get
            {
                return this._selectedItem_History;
            }

            set
            {
                this._selectedItem_History = value;
                this.OnPropertyChanged();
            }
        }

        private int _CurrentPageNum = 1;

        public int CurrentPageNum
        {
            get
            {
                return _CurrentPageNum;
            }
            set
            {
                if (_CurrentPageNum != value)
                {
                    _CurrentPageNum = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private int _CurrentTotalCount_History = 0;
        public int CurrentTotalCount_History
        {
            get
            {
                return _CurrentTotalCount_History;
            }
            set
            {
                _CurrentTotalCount_History = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount_History = 0;
        public int TotalCount_History
        {
            get
            {
                return _TotalCount_History;
            }
            set
            {
                _TotalCount_History = value;
                this.OnPropertyChanged();
            }
        }

        private int _BidCount_History = 0;
        public int BidCount_History
        {
            get
            {
                return _BidCount_History;
            }
            set
            {
                _BidCount_History = value;
                this.OnPropertyChanged();
            }
        }


        private int _OfrCount_History = 0;
        public int OfrCount_History
        {
            get
            {
                return _OfrCount_History;
            }
            set
            {
                _OfrCount_History = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _fromDate = DateTimeEx.ServerTime;

        public DateTime FromDate
        {
            get { return _fromDate; }
            set 
            { 
                _fromDate = value;
                this.OnPropertyChanged();
                if (_fromDate != null && _toDate != null)
                {
                    UpdateHistoryIntentions();
                }
            }
        }

        private DateTime _toDate = DateTimeEx.ServerTime;

        public DateTime ToDate
        {
            get { return _toDate; }
            set 
            {
                _toDate = value;
                this.OnPropertyChanged();
                if (_fromDate != null && _toDate != null)
                {
                    UpdateHistoryIntentions();
                }
            }
        }

        private bool _isMe_History;

        public bool IsMe_History
        {
            get
            {
                return _isMe_History;
            }
            set
            {
                _isMe_History = value;
                this.OnPropertyChanged();
                UpdateHistoryIntentions();
            }
        }

        private List<ListBoxItem> _HistoryFilterSelectedItems = new List<ListBoxItem>();

        public List<ListBoxItem> HistoryFilterSelectedItems
        {
            get
            {
                return _HistoryFilterSelectedItems;
            }
            set
            {
                foreach (ListBoxItem item in value)
                {
                    if ("同券反向" == item.Content.ToString())
                    {
                        this.SearchSecurityID_History = string.Empty;
                    }
                }
                _HistoryFilterSelectedItems = value;
                this.OnPropertyChanged();
                UpdateHistoryIntentions();
            }
        }

        private string _searchSecurityID_History;

        public string SearchSecurityID_History
        {
            get
            {
                return _searchSecurityID_History;
            }
            set
            
            {
                _searchSecurityID_History = value;
                this.OnPropertyChanged();
                UpdateHistoryIntentions();
            }
        }

        private object _HistoryLock = new object();
        private CollectionViewSource historyViewSource = new CollectionViewSource();
        public CollectionViewSource HistoryViewSource
        {
            get { return historyViewSource; }
            set { historyViewSource = value; }
        }

        private CustomCollection<TradeRequestData> tradeRequestInfo_History = new CustomCollection<TradeRequestData>();

        public CustomCollection<TradeRequestData> TradeRequestInfo_History
        {
            get { return tradeRequestInfo_History; }
            set { tradeRequestInfo_History = value; }
        }

        internal async void UpdateHistoryIntentions()
        {
            TradeRequestInfo_History.Clear();
            IntentionQuoteRequest request = new IntentionQuoteRequest();
            request.status = "2,4,5";
            request.startDate = FromDate.ToString("yyyy-MM-dd");
            request.endDate = ToDate.ToString("yyyy-MM-dd");

            if (IsMe_History)
                request.userName = LocalEnv.UserInfo.UserName;

            if (!string.IsNullOrEmpty(this.SearchSecurityID_History))
            {
                request.securityId = this.SearchSecurityID_History;
            }

            if (this.HistoryFilterSelectedItems != null)
            {
                foreach (ListBoxItem item in this.HistoryFilterSelectedItems)
                {
                    if ("同客户" == item.Content.ToString())
                    {
                         request.institutionName = this.SelectedItem_Worked.InstitutionName;
                        request.traderName = this.SelectedItem_Worked.TraderName;
                    }
                    if ("同券反向" == item.Content.ToString())
                    {
                        request.securityId = this._selectedItem_Worked.SecurityId;
                        request.side = this._selectedItem_Worked.Side == "1" ? "2" : "1";
                    }
                }
            }
            
            if (ratingConditions_history.Length > 0)
            {
                request.issuerRating = ratingConditions_history.ToString();
            }

            if (settlTypeConditions_history.Length > 0)
            {
                request.settlType = settlTypeConditions_history.ToString();
            }

            if (RemainingTimeConditions_history.Length > 0)
            {
                request.maturityDate = RemainingTimeConditions_history.ToString();
            }

            if (policyFinancialBondConditions_history.Length > 0)
            {
                request.isssuer = policyFinancialBondConditions_history.ToString();
            }

            if (interest_history == "1")
            {
                request.securityType = "BNC";
            }
            else if (bondCategoryConditions_Interest_History.Length > 0)
            {
                request.securityTypeId = bondCategoryConditions_Interest_History.ToString();
            }

            if (credit_history == "1")
            {
                if (request.securityType.Length > 0)
                    request.securityType += ",";
                request.securityType = "BCO";
            }
            else if (bondCategoryConditions_Credit_history.Length > 0)
            {
                if (request.securityTypeId.Length > 0)
                    request.securityTypeId += ",";
                request.securityTypeId = bondCategoryConditions_Credit_history.ToString();
            }

            request.pageSize = 1000;
            request.pageNum = CurrentPageNum - 1;
            //获取订单 
            IntentionQuoteReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = TableHandler.Instance.QueryIntentionQuote(request);
            });

            if (reply != null && reply.Datas != null)
            {
                this.CurrentTotalCount_History = reply.Datas.Count;
                this.TotalCount_History = reply.Total;
                this.BidCount_History = reply.TotalBid;
                this.OfrCount_History = reply.TotalOfr;
                foreach (IntentionQuoteInfo obj in reply.Datas)
                {
                    TradeRequestData data = new TradeRequestData();
                    data.IntentionId = obj.intentionId;
                    data.InstitutionId = obj.institutionId;
                    data.InstitutionName = obj.institutionName;
                    data.TraderId = obj.traderId;
                    data.TraderName = obj.traderName;
                    data.CreateTime = obj.createTime;
                    data.SecurityId = obj.securityId;
                    data.Symbol = obj.symbol;
                    data.Side = obj.side;
                    data.Price = obj.price;
                    data.Qty = obj.qty;
                    data.ExecutionPrice = obj.lastPrice;
                    data.ExecutionQty = obj.bookQty;
                    data.PNL = obj.profitLoss;
                    data.RunningTime = obj.runningTime;
                    this.TradeRequestInfo_History.Insert(0, data);
                }
            }
        }

        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void HistoryExportCSV()
        {
            ExportEnabled_History = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_历史需求";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
            else
            {
                ExportEnabled_History = true;
            }
        }

        private void RequestExportMethod(object state)
        {
            RequestExport();
            autoEvent.Set();
        }

        public void RequestExport()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;
            StreamWriter sw = null;
            try
            {
                sw = CSVHelper.Instance.createStream(fileName);
                if (sw != null)
                {
                    LogHelper.Info("fileName:" + fileName);
                    IntentionQuoteRequest request = new IntentionQuoteRequest();
                    request.status = "2,4,5";
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.startDate = FromDate.ToString("yyyy-MM-dd");
                    request.endDate = ToDate.ToString("yyyy-MM-dd");

                    if (!string.IsNullOrEmpty(this.SearchSecurityID_History))
                    {
                        request.securityId = this.SearchSecurityID_History;
                    }

                    if (this.HistoryFilterSelectedItems != null)
                    {
                        foreach (ListBoxItem item in this.HistoryFilterSelectedItems)
                        {
                            if ("同客户" == item.Content.ToString())
                            {
                                request.institutionName = this.SelectedItem_Worked.InstitutionName;
                                request.traderName = this.SelectedItem_Worked.TraderName;
                            }
                            if ("同券反向" == item.Content.ToString())
                            {
                                request.securityId = this._selectedItem_Worked.SecurityId;
                                request.side = this._selectedItem_Worked.Side == "1" ? "2" : "1";
                            }
                        }
                    }
                    int index = 0;
                    request.pageSize = 2000;

                    ///获取订单 
                    bool flag = true;
                    while (flag)
                    {
                        request.pageNum = index;
                        ///获取订单 
                        var reply = TableHandler.Instance.QueryIntentionQuote(request);

                        if (reply == null)
                        {
                            flag = false;
                            break;
                        }

                        if (reply != null && reply.Datas != null)
                        {
                            foreach (IntentionQuoteInfo obj in reply.Datas)
                            {
                                StringBuilder lineSB = new StringBuilder();
                                try
                                {
                                    foreach (KeyValuePair<string, string> pair in historyExportDic)
                                    {
                                        string field = pair.Key;

                                        object fieldValue = typeof(IntentionQuoteInfo).GetProperty(field).GetValue(obj);

                                        string value = Common.Utils.CommonUtils.ConvertToString(fieldValue);

                                        if (field.Equals("Side", StringComparison.OrdinalIgnoreCase))
                                        {
                                            value = ComboSourceUtil.Instance.FindEnum(value, "side");
                                        }
                                        else if (field.Equals("SettlType", StringComparison.OrdinalIgnoreCase))
                                        {
                                            value = ComboSourceUtil.Instance.FindEnum(value, "settletype");
                                        }
                                        else if (field.Equals("Price", StringComparison.OrdinalIgnoreCase))
                                        {
                                            value = value.ConvertToDecimal().ToString("0.00#");
                                        }
                                        else if (field.Equals("Qty", StringComparison.OrdinalIgnoreCase))
                                        {
                                            value = value.ConvertToDecimal().ToString("0");
                                        }
                                        else if (field.Equals("Status", StringComparison.OrdinalIgnoreCase))
                                        {
                                            value = ComboSourceUtil.Instance.FindEnum(value, "intentionstatus1");
                                        }
                                        else if (field.Equals("", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                long time = Common.Utils.CommonUtils.ConvertToLong(value.ToString()) / 1000;
                                                if (time <= 0)
                                                    value = "--";
                                                value = $"{time / 60}h {time % 60}min";
                                            }
                                            value = "--";
                                        }
                                        if (lineSB.Length > 0)
                                        {
                                            lineSB.Append(",");
                                        }

                                        lineSB.Append($"\"{value}\"");
                                    }
                                    sw.WriteLine(lineSB);
                                    successCount++;
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error(e);
                                    failCount++;
                                }
                                totalSize++;
                            }
                            if (reply.Datas.Count < request.pageSize)
                            {
                                flag = false;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                // 释放资源
                sw.Flush();
                sw.Close();
                Common.View.Component.CustomMessageBox.Info($"《{fileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
            }
            catch(Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                ExportEnabled_History = true;
            }
        }

        #endregion

        #region 过滤
        string interest = string.Empty;
        List<string> bondCategoryConditions_Interest = new List<string>();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType(System.Collections.IList selectedItems)
        {
            interest = string.Empty;
            bondCategoryConditions_Interest.Clear();
            if (InterestRateDebtList != null)
            {
                foreach (FilterListBoxItemData lbi in InterestRateDebtList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        bondCategoryConditions_Interest.Clear();
                        interest = "1";
                        break;
                    }
                    bondCategoryConditions_Interest.Add(lbi.itemText);
                }
            }
            FilterDatas();
        }

        string credit = string.Empty;
        List<string> bondCategoryConditions_Credit = new List<string>();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType()
        {
            credit = string.Empty;
            bondCategoryConditions_Credit.Clear();
            if (CreditBondList != null)
            {
                foreach (FilterListBoxItemData lbi in CreditBondList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        bondCategoryConditions_Credit.Clear();
                        credit = "1";
                        break;
                    }
                   
                    if (lbi.itemText.ToString() == "其他")
                    {
                        bondCategoryConditions_Credit.Add("");
                    }
                    else
                    {
                        bondCategoryConditions_Credit.Add(lbi.itemText);
                    }
                }
            }
            FilterDatas();
        }

        List<string> policyFinancialBondConditions = new List<string>();
        /// <summary>
        /// 政策性金融债
        /// </summary>
        public void FilterPolicyFinancialBond()
        {
            policyFinancialBondConditions.Clear();
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
                        policyFinancialBondConditions.Clear();
                        break;
                    }

                    if (lbi.itemText == "国开")
                    {
                        policyFinancialBondConditions.Add("100034");
                    }
                    else if (lbi.itemText == "农发")
                    {
                        policyFinancialBondConditions.Add("101915");
                    }
                    else if (lbi.itemText == "口行")
                    {
                        policyFinancialBondConditions.Add("100017");
                    }

                }
            }
            this.FilterDatas();
        }

        List<int[]> RemainingTimeConditions = new List<int[]>();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime()
        {
            RemainingTimeConditions.Clear();
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
                        RemainingTimeConditions.Clear();
                        break;
                    }


                    if (lbi.itemText == "0~1Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 0, 1 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 1, 3 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 3, 5 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 5, 7 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 7, 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.FilterDatas();
        }

        List<string> rateOptions = new List<string>() { "AAA+", "AAA", "AA+", "AA", "AA-", "A+" };
        List<string> ratingConditions = new List<string>();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating()
        {
            ratingConditions.Clear();
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

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        ratingConditions.Clear();
                        break;
                    }

                    ratingConditions.Add(lbi.itemText);
                    //switch (lbi.itemText)
                    //{
                    //    case "AAA+":
                    //    case "AAA":
                    //    case "AA+":
                    //    case "AA":
                    //    case "AA-":
                    //    case "A+":
                    //        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    //        break;
                    //    case "其他":
                    //        notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                    //        break;
                    //}
                }
            }
            this.FilterDatas();
        }

        List<string> settlTypeConditions = new List<string>();
        /// <summary>
        /// 清算速度过滤条件
        /// </summary>
        public void FilterSettlType()
        {
            settlTypeConditions.Clear();
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
                        settlTypeConditions.Clear();
                        break;
                    }

                    //if (settlTypeFilter.Length > 0)
                    //{
                    //    settlTypeFilter.Append(" or ");
                    //}

                    if (lbi.itemText == "T+1")
                    {
                        settlTypeConditions.Add("2");
                        settlTypeConditions.Add("3");
                        // settlTypeFilter.Append($"({MMListBoxViewModel.SETTLE_TYPE_FIELD} = '2' or {MMListBoxViewModel.SETTLE_TYPE_FIELD} = '3')");
                    }
                    else if (lbi.itemText == "T+0")
                    {
                        settlTypeConditions.Add("1");
                        //settlTypeFilter.Append($"{MMListBoxViewModel.SETTLE_TYPE_FIELD} = '1'");
                    }
                }
            }
            this.FilterDatas();
        }
        #endregion


        #region 历史过滤
        string interest_history = string.Empty;
        StringBuilder bondCategoryConditions_Interest_History = new StringBuilder();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType_History(System.Collections.IList selectedItems)
        {
            interest_history = string.Empty;
            bondCategoryConditions_Interest_History.Clear();
            if (InterestRateDebtList_History != null)
            {
                foreach (FilterListBoxItemData lbi in InterestRateDebtList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        bondCategoryConditions_Interest_History.Clear();
                        interest_history = "1";
                        break;
                    }

                    if (bondCategoryConditions_Interest_History.Length > 0)
                    {
                        bondCategoryConditions_Interest_History.Append(",");
                    }

                    string option = lbi.itemText.ToString();

                    switch (option)
                    {
                        case "国债":
                            bondCategoryConditions_Interest_History.Append("1");
                            break;
                        case "政策性金融债":
                            bondCategoryConditions_Interest_History.Append("2");
                            break;
                        case "地方政府债":
                            bondCategoryConditions_Interest_History.Append("3");
                            break;
                        case "央债":
                            bondCategoryConditions_Interest_History.Append("4");
                            break;
                    }
                }
            }
            UpdateHistoryIntentions();
        }

        string credit_history = string.Empty;
        StringBuilder bondCategoryConditions_Credit_history = new StringBuilder();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType_history()
        {
            credit_history = string.Empty;
            bondCategoryConditions_Credit_history.Clear();
            if (CreditBondList_History != null)
            {
                foreach (FilterListBoxItemData lbi in CreditBondList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        bondCategoryConditions_Credit_history.Clear();
                        credit_history = "1";
                        break;
                    }

                    if (bondCategoryConditions_Credit_history.Length > 0)
                    {
                        bondCategoryConditions_Credit_history.Append(",");
                    }

                    string option = lbi.itemText.ToString();

                    switch(option)
                    {
                        case "政府支持机构债":
                            bondCategoryConditions_Credit_history.Append("5");
                            break;
                        case "公司债":
                            bondCategoryConditions_Credit_history.Append("6");
                            break;
                        case "金融债":
                            bondCategoryConditions_Credit_history.Append("7");
                            break;
                        case "NCD":
                            bondCategoryConditions_Credit_history.Append("8");
                            break;
                        case "ABS":
                            bondCategoryConditions_Credit_history.Append("9");
                            break;
                        case "其他":
                            bondCategoryConditions_Credit_history.Append("10");
                            break;
                    }
                }
            }
            UpdateHistoryIntentions();
        }

        StringBuilder policyFinancialBondConditions_history = new StringBuilder();
        /// <summary>
        /// 政策性金融债
        /// </summary>
        public void FilterPolicyFinancialBond_History()
        {
            policyFinancialBondConditions_history.Clear();
            if (PolicyFinancialBondList_History != null)
            {
                foreach (FilterListBoxItemData lbi in PolicyFinancialBondList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        policyFinancialBondConditions_history.Clear();
                        break;
                    }

                    if (policyFinancialBondConditions_history.Length > 0)
                    {
                        policyFinancialBondConditions_history.Append(",");
                    }

                    if (lbi.itemText == "国开")
                    {
                        policyFinancialBondConditions_history.Append("100034");
                    }
                    else if (lbi.itemText == "农发")
                    {
                        policyFinancialBondConditions_history.Append("101915");
                    }
                    else if (lbi.itemText == "口行")
                    {
                        policyFinancialBondConditions_history.Append("100017");
                    }

                }
            }
            this.UpdateHistoryIntentions();
        }

        StringBuilder RemainingTimeConditions_history = new StringBuilder();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime_History()
        {
            RemainingTimeConditions_history.Clear();
            if (RemaingingTimeList_History != null)
            {
                foreach (FilterListBoxItemData lbi in RemaingingTimeList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        RemainingTimeConditions_history.Clear();
                        break;
                    }

                    if (RemainingTimeConditions_history.Length > 0)
                        RemainingTimeConditions_history.Append(",");


                    if (lbi.itemText == "0~1Y")
                    {
                        RemainingTimeConditions_history.Append("0-1");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        RemainingTimeConditions_history.Append("1-3");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        RemainingTimeConditions_history.Append("3-5");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        RemainingTimeConditions_history.Append("5-7");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        RemainingTimeConditions_history.Append("7-10");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        RemainingTimeConditions_history.Append("10-100");
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.UpdateHistoryIntentions();
        }

        StringBuilder ratingConditions_history = new StringBuilder();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating_History()
        {
            ratingConditions_history.Clear();
            if (RatingList_History != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (FilterListBoxItemData lbi in RatingList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        ratingConditions_history.Clear();
                        break;
                    }

                    if (ratingConditions_history.Length > 0)
                    {
                        ratingConditions_history.Append(",");
                    }

                    ratingConditions_history.Append(lbi.itemText);
                    //switch (lbi.itemText)
                    //{
                    //    case "AAA+":
                    //    case "AAA":
                    //    case "AA+":
                    //    case "AA":
                    //    case "AA-":
                    //    case "A+":
                    //        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    //        break;
                    //    case "其他":
                    //        notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                    //        break;
                    //}
                }
            }
            this.UpdateHistoryIntentions();
        }

        StringBuilder settlTypeConditions_history = new StringBuilder();
        /// <summary>
        /// 清算速度过滤条件
        /// </summary>
        public void FilterSettlType_History()
        {
            settlTypeConditions_history.Clear();
            if (SettlTypeList_History != null)
            {
                foreach (FilterListBoxItemData lbi in SettlTypeList_History)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        settlTypeConditions_history.Clear();
                        break;
                    }

                    if (settlTypeConditions_history.Length > 0)
                    {
                        settlTypeConditions_history.Append(",");
                    }

                    if (lbi.itemText == "T+1")
                    {
                        settlTypeConditions_history.Append("2,3");
                    }
                    else if (lbi.itemText == "T+0")
                    {
                        settlTypeConditions_history.Append("1");
                    }
                }
            }
            this.UpdateHistoryIntentions();
        }
        #endregion

        /// <summary>
        /// 打开历史查询界面
        /// </summary>
        public void OpenHistoryPage(bool isExecution)
        {
            // 使用插件形式打开历史页面
            int tradeBookPluginKey = (int)PluginKeyEnum.HistoryPlugin;

            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
            if (plugin != null)
            {
                bool isExist = TabPageHelper.IsExistTabItem(plugin.Description);
                if (isExist == false)
                {
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                }


                if (AppUtils.Application.Panel.SelectedItem != null
                && AppUtils.Application.Panel.SelectedItem is TabItem)
                {
                    TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
                    //if (tabItem.Content is HistoryView)
                    //{

                    //    HistoryView historyView = tabItem.Content as HistoryView;
                    //    if (isExecution)
                    //    {
                    //        historyView.SelectDealView();
                    //    }
                    //    else
                    //    {
                    //        historyView.SelectOrderView();
                    //    }
                    //}
                    try
                    {
                        if (tabItem.Content != null)
                        {
                            if (isExecution)
                            {
                                MethodInfo mi = tabItem.Content.GetType().GetMethod("SelectDealView");
                                if (mi != null)
                                {
                                    mi.Invoke(tabItem.Content, null);
                                }
                            }
                            else
                            {
                                MethodInfo mi = tabItem.Content.GetType().GetMethod("SelectOrderView");
                                if (mi != null)
                                {
                                    mi.Invoke(tabItem.Content, null);
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }
            }
        }
    }
}
