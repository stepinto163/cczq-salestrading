using com.gateway.log;
using STC.Common.Handler.Drp;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class IntentionUIViewModel : ViewModelBase
    {
        #region 定义
        public IntentionUI ParentWindow;
        private string _SecurityID;
        private string _Symbol;
        private string _UpdateTime;
        private string _TraderName;
        private string _Side;
        private string _CounterPartyInstitutionName;
        private string _CounterPartyTraderName;
        private string _Price;
        private string _OrderQty;
        private string _SettlType;
        private string _Comment;
        private string _OCOVisible;
        private string _PackageVisible;
        private string _SwitchVisible;
        private string _XVisible;
        private string _XXVisible;
        private string _DayPrice;
        private string _DayOrderQty;
        private string _DaySettlType;
        private string _DayComment;
        public string DayPrice
        {
            get
            {
                return _DayPrice;
            }
            set
            {
                _DayPrice = value;
                this.OnPropertyChanged();
            }
        }
        public string DayOrderQty
        {
            get
            {
                return _DayOrderQty;
            }
            set
            {
                _DayOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        public string DaySettlType
        {
            get
            {
                return _DaySettlType;
            }
            set
            {
                _DaySettlType = value;
                this.OnPropertyChanged();
            }
        }
        public string DayComment
        {
            get
            {
                return _DayComment;
            }
            set
            {
                _DayComment = value;
                this.OnPropertyChanged();
            }
        }
        public string SecurityID
        {
            get
            {
                return _SecurityID;
            }
            set
            {
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }
        public string Symbol
        {
            get
            {
                return _Symbol;
            }
            set
            {
                _Symbol = value;
                this.OnPropertyChanged();
            }
        }
        public string UpdateTime
        {
            get
            {
                return _UpdateTime;
            }
            set
            {
                _UpdateTime = value;
                this.OnPropertyChanged();
            }
        }
        public string TraderName
        {
            get
            {
                return _TraderName;
            }
            set
            {
                _TraderName = value;
                this.OnPropertyChanged();
            }
        }
        public string Side
        {
            get
            {
                return _Side;
            }
            set
            {
                _Side = value;
                this.OnPropertyChanged();
            }
        }
        public string CounterPartyInstitutionName
        {
            get
            {
                return _CounterPartyInstitutionName;
            }
            set
            {
                _CounterPartyInstitutionName = value;
                this.OnPropertyChanged();
            }
        }
        public string CounterPartyTraderName
        {
            get
            {
                return _CounterPartyTraderName;
            }
            set
            {
                _CounterPartyTraderName = value;
                this.OnPropertyChanged();
            }
        }
        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
                this.OnPropertyChanged();
            }
        }
        public string OrderQty
        {
            get
            {
                return _OrderQty;
            }
            set
            {
                _OrderQty = value;
                this.OnPropertyChanged();
            }
        }
        public string SettlType
        {
            get
            {
                return _SettlType;
            }
            set
            {
                _SettlType = value;
                this.OnPropertyChanged();
            }
        }
        public string Comment
        {
            get
            {
                return _Comment;
            }
            set
            {
                _Comment = value;
                this.OnPropertyChanged();
            }
        }

        public string OCOVisible
        {
            get
            {
                return _OCOVisible;
            }
            set
            {
                _OCOVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string PackageVisible
        {
            get
            {
                return _PackageVisible;
            }
            set
            {
                _PackageVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string SwitchVisible
        {
            get
            {
                return _SwitchVisible;
            }
            set
            {
                _SwitchVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string XVisible
        {
            get
            {
                return _XVisible;
            }
            set
            {
                _XVisible = value;
                this.OnPropertyChanged();
            }
        }
        public string XXVisible
        {
            get
            {
                return _XXVisible;
            }
            set
            {
                _XXVisible = value;
                this.OnPropertyChanged();
            }
        }
        private Brush bgbackground = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BGBackground
        {
            get { return bgbackground; }
            set
            {
                if (value != null)
                {
                    bgbackground = value;
                    this.OnPropertyChanged(nameof(BGBackground));
                }
            }
        }

        private string _IOCOrderQty;
        public string IOCOrderQty
        {
            get
            {
                return _IOCOrderQty;
            }
            set
            {
                _IOCOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        private string _LeaveOrderQty;
        public string LeaveOrderQty
        {
            get
            {
                return _LeaveOrderQty;
            }
            set
            {
                _LeaveOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        private string _IOCPrice;
        public string IOCPrice
        {
            get
            {
                return _IOCPrice;
            }
            set
            {
                _IOCPrice = value;
                this.OnPropertyChanged();
            }
        }
        private string _Count;
        public string Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
                this.OnPropertyChanged();
            }
        }

        private string _NoMatchVisible = "0";
        public string NoMatchVisible
        {
            get
            {
                return _NoMatchVisible;
            }
            set
            {
                _NoMatchVisible = value;
                this.OnPropertyChanged();
            }
        }
        public ICommand DetailCommand { get; set; }
        public string Info4;
        private string _BrokerAIsMe;
        public string BrokerAIsMe
        {
            get
            {
                return _BrokerAIsMe;
            }
            set
            {
                _BrokerAIsMe = value;
                this.OnPropertyChanged();
            }
        }
        private string _HandlInst;
        public string HandlInst
        {
            get
            {
                return _HandlInst;
            }
            set
            {
                _HandlInst = value;
                this.OnPropertyChanged();
            }
        }

        private string _MsgId;
        public string MsgId
        {
            get
            {
                return _MsgId;
            }
            set
            {
                _MsgId = value;
                this.OnPropertyChanged();
            }
        }

        private string _CumQty;
        public string CumQty
        {
            get
            {
                return _CumQty;
            }
            set
            {
                _CumQty = value;
                this.OnPropertyChanged();
            }
        }


        private ObservableCollection<GrFeedBackViewModel> _FeedBackQuotations = new ObservableCollection<GrFeedBackViewModel>();
        /// <summary>
        /// 列表
        /// </summary>
        public ObservableCollection<GrFeedBackViewModel> FeedBackQuotations
        {
            get
            {
                return _FeedBackQuotations;
            }
            set
            {
                _FeedBackQuotations = value;
                this.OnPropertyChanged();
            }
        }

        private CollectionViewSource feedbackquotationsmv = new CollectionViewSource();
        public CollectionViewSource FeedBackQuotationsMV
        {
            get { return feedbackquotationsmv; }
            set
            {
                feedbackquotationsmv = value;
                this.OnPropertyChanged(nameof(FeedBackQuotationsMV));
            }
        }

        private string _BlockNum;
        public string BlockNum
        {
            get
            {
                return _BlockNum;
            }
            set
            {
                _BlockNum = value;
                this.OnPropertyChanged();
            }
        }

        private object updlock = new object();
        #endregion

        public IntentionUIViewModel()
        {
            ThreadPoolHelper.InvokeThreadPool((P) =>
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                {
                    this.DetailCommand = new ComRelayCommand(DetailClick);
                    // View
                    this.FeedBackQuotationsMV.Source = FeedBackQuotations;
                    this.FeedBackQuotationsMV.IsLiveFilteringRequested = true;
                    this.FeedBackQuotationsMV.IsLiveSortingRequested = true;
                    this.FeedBackQuotationsMV.SortDescriptions.Add(new SortDescription("TransactTime", ListSortDirection.Descending));
                }));
            });
        }

        private bool IsInitData = false;
        /// <summary>
        /// 初始化，更新界面UI数据
        /// </summary>
        /// <param name="polist"></param>
        public void InitData(List<ExecutionReport> polist)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                ExecutionReport po = polist[0];
                if (!IsInitData)
                {
                    IsInitData = true;
                    string tag1 = po.Tag1;
                    GrTBookOrderModel grdr = CommonUtils.DeserializeObject<GrTBookOrderModel>(tag1);

                    HandlInst = po.HandlInst;
                    SecurityID = po.SecurityID;
                    Symbol = po.Symbol;
                    UpdateTime = po.UpdateTime;
                    TraderName = po.PartySubName;
                    Side = po.Side.Equals("1") ? "2" : "1";
                    BGBackground = Side == "2" ? (Brush)ParentWindow.FindResource("OfrLinearGradientBrush") : (Brush)ParentWindow.FindResource("BidLinearGradientBrush");
                    Price = po.Price;
                    SettlType = po.SettlType;
                    Comment = po.Remark;
                    UpdateTime = po.UpdateTime;
                    OrderQty = grdr.OrderQty;
                    CumQty = grdr.QuoteQty;
                    BlockNum = po.BlockNum ?? "";
                    //Count = po.Info2 ?? "0";
                }

                UpdateData(polist);
            }));
        }


        /// <summary>
        /// 更新明细数据
        /// </summary>
        /// <param name="polist"></param>
        public void UpdateData(List<ExecutionReport> polist)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                try
                {
                    foreach (ExecutionReport po in polist)
                    {
                        string key = po.ExecID;
                        bool isfound = false;
                        GrFeedBackViewModel findDr = null;
                        lock (updlock)
                        {
                            findDr = FeedBackQuotations.FirstOrDefault(a => a.ExecID == key);
                            if (findDr == null)
                            {
                                isfound = false;
                                findDr = new GrFeedBackViewModel();
                                findDr.HandlInst = po.HandlInst;
                                findDr.ExecID = po.ExecID;
                                findDr.TraderName = po.PartySubName;
                                findDr.Side = po.Side.Equals("1") ? "2" : "1";
                                string counterTraderName = po.CounterPartyTraderName;
                                if (!string.IsNullOrEmpty(counterTraderName))
                                {
                                    findDr.CounterPartyInstitutionName = po.CounterPartyInstitutionName + string.Format("({0})", counterTraderName);
                                }
                                else
                                {
                                    findDr.CounterPartyInstitutionName = po.CounterPartyInstitutionName;
                                }
                                findDr.CounterPartyTraderName = po.CounterPartySubName ?? "";
                                if (po.CounterPartySubID.Equals(po.PartySubID ?? ""))
                                {
                                    findDr.IsMe = true;
                                }


                                if (!string.IsNullOrEmpty(po.Tag1))
                                {
                                    ExecutionReport er = CommonUtils.DeserializeObject<ExecutionReport>(po.Tag1);
                                    findDr.DayPrice = er.Price;
                                    findDr.DayOrderQty = er.OrderQty;
                                    findDr.DaySettlType = er.SettlType;
                                    findDr.DayComment = er.Text;
                                    findDr.DayHandlInst = er.HandlInst;

                                    string bargainFlag = er.BargainFlag;

                                    // 换券标识 true: 换券; false 不换券
                                    findDr.ExchangeSignFlag = string.IsNullOrEmpty(er.ExchangeSignFlag) ? false : er.ExchangeSignFlag == "0" ? false : true;
                                    //议价标识 0:无星; 1: 1星; 2: 2星
                                    findDr.bargainFlag = er.BargainFlag;
                                    //OCO 打包 1:oco 2:打包
                                    findDr.ocoFlag = er.OCOFlag ?? "";

                                    findDr.wholeFlag = er.WholeFlag;

                                    if ("1".Equals(er.WholeFlag))
                                    {
                                        findDr.DayComment = "整量 " + findDr.DayComment;
                                    }
                                }
                            }
                            else
                            {
                                isfound = true;
                            }

                                
                            findDr.TransactTime = po.TransactTime;
                            findDr.UpdateTime = po.UpdateTime;

                            findDr.OrdStatus = po.OrdStatus == "C" ? "D" : po.OrdStatus;
                            findDr.LastQty = po.LastQty;
                            findDr.LeavesQty = po.LeavesQty;
                            findDr.Amount = CommonUtils.ConvertToDecimal(findDr.LastQty);
                            findDr.MaxAmount = findDr.Amount;
                            findDr.ButtonVisible = "2";//Hide 
                            findDr.OrderQty = po.OrderQty;

                            if (findDr.OrdStatus.Equals("2"))
                            {
                                findDr.StatusMsg = "待确认";
                            }

                            if ((po.OrdStatus.Equals("E") || po.OrdStatus.Equals("G")) && !po.LastQty.Equals(po.OrderQty))
                            {
                                findDr.OrdStatus = "1";
                                findDr.ButtonVisible = "1";
                                //BGBackground = (Brush)ParentWindow.FindResource("ConfirmLinearGradientBrush");
                            }

                            if (findDr.OrdStatus.Equals("F") || findDr.OrdStatus.Equals("H"))
                            {
                                //BGBackground = (Brush)ParentWindow.FindResource("RejectLinearGradientBrush");
                            }

                            if (!isfound)
                            {
                                FeedBackQuotations.Add(findDr);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                UpdateConfirmData(polist);
                
            }));
        }

        private int _OrgRecc = 0;
        public int OrgRecc
        {
            get
            {
                return _OrgRecc;
            }
            set
            {
                if (_OrgRecc == 0)
                {
                    _OrgRecc = value;
                }
                else
                { 
                    if (_OrgRecc != value)
                    {
                        _OrgRecc = value;
                        boolProcNoMatchVisible = false;
                    }
                }

                this.OnPropertyChanged();
            }
        }


        private bool boolProcNoMatchVisible = false;

        private void UpdateConfirmData(List<ExecutionReport> polist)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        OrgRecc =  polist.Count;

                        if (!boolProcNoMatchVisible)
                        {
                            //更新匹配量和成交量
                            List<string> tmps = polist.Select(p => p.Tag1).ToList();
                            
                            List<ExecutionReport> lstER = CommonUtils.DeserializeObject<List<ExecutionReport>>($"[{string.Join(",", tmps)}]");
                            decimal sumQty = CommonUtils.ConvertToDecimal(CumQty);
                            decimal sumMatchQTY = FeedBackQuotations.Sum(p => CommonUtils.ConvertToDecimal(p.OrderQty));
                            LeaveOrderQty = (sumQty - sumMatchQTY).ToString();
                            if (sumQty - sumMatchQTY > 0)
                            {
                                NoMatchVisible = "1";
                                IOCOrderQty = sumMatchQTY.ToString();
                                IOCPrice = polist[0].Price;

                                if (DrpHandler.Instance.DictTBookOrders.ContainsKey(polist[0].StrategyOrderId))
                                { 
                                    Count = DrpHandler.Instance.DictTBookOrders[polist[0].StrategyOrderId].Info2 ?? "0";
                                }

                                boolProcNoMatchVisible = true;
                            }
                            else
                            {
                                NoMatchVisible = "0";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                });
            }));
        }



        private void DetailClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //发布消息
                    Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "PopupBorkerQuotesMiniWindows", Info1 = SecurityID });
                });
            });
        }
        private string getDayOrderInfo(string info7,int index) 
        {
            string result = "";
            if (!string.IsNullOrEmpty(info7))
            {
                string[] info7Array = info7.Split('|');
                if (info7Array.Count() >= (index + 1))
                {
                    result = "null".Equals(info7Array[index]) ? "" : info7Array[index];
                }
            }
            return result;
        }
    }
}
