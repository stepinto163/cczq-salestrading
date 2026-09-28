using com.gateway.log;
using STC.Common.Command;
using STC.Common.Handler.Drp;
using STC.Common.Handler.PT;
using STC.Common.Handler.TBook;
using STC.Common.Model;
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
    public class FeedBackAUIViewModel : ViewModelBase
    {
        #region 定义
        public FeedBackAUI ParentWindow;

        #region use
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

        private string _SecurityID;

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
        private string _Symbol;

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

        private string _UpdateTime;
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

        private string _Price;
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

        private string _Side;
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


        private string _OrderQty;

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

        private string _LastQty;
        public string LastQty
        {
            get
            {
                return _LastQty;
            }
            set
            {
                _LastQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _SettlType;
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

        private string _ConfirmMsg;
        public string ConfirmMsg
        {
            get
            {
                return _ConfirmMsg;
            }
            set
            {
                _ConfirmMsg = value;
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
        #endregion


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
        #region  Command
        public ICommand RejectCommand { get; set; }

        public ICommand OkCommand { get; set; }
        #endregion

        private object updlock = new object();
        #endregion

        public FeedBackAUIViewModel()
        {
            this.RejectCommand = new DelegateCommand(new Action<object>(OnRejectCommand));
            this.OkCommand = new DelegateCommand(new Action<object>(OnOkCommand));

            // View
            this.FeedBackQuotationsMV.Source = FeedBackQuotations;
            this.FeedBackQuotationsMV.IsLiveFilteringRequested = true;
            this.FeedBackQuotationsMV.IsLiveSortingRequested = true;
            this.FeedBackQuotationsMV.SortDescriptions.Add(new SortDescription("TransactTime", ListSortDirection.Descending));
        }

        private bool IsInitData = false;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="polist"></param>
        public void InitData(List<ExecutionReport> polist)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                ExecutionReport po = polist[0];
                if (!IsInitData)
                {
                    IsInitData = true;
                    SecurityID = po.SecurityID;
                    Symbol = po.Symbol;
                    UpdateTime = po.UpdateTime;
                    Side = po.Side.Equals("1") ? "2" : "1";
                    Price = po.Price;
                    HandlInst = po.HandlInst;
                    OrderQty = po.OrderQty;
                    SettlType = po.SettlType;
                    LastQty = po.LastQty ?? "0";

                    if (po.OrdStatus.Equals("F"))
                    {
                        BGBackground = (Brush)ParentWindow.FindResource("RejectLinearGradientBrush");
                    }
                    else
                    {
                        BGBackground = (Brush)ParentWindow.FindResource("ConfirmLinearGradientBrush");
                        if (LastQty.Equals(OrderQty))
                        {
                            ConfirmMsg = "全部确认";
                        }
                        else
                        {
                            ConfirmMsg = "部分确认";
                        }
                    }
                }

                UpdateData(polist);
            }));
        }


        private void UpdateConfirmData()
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                //更新最后更新时间
                GrFeedBackViewModel er = FeedBackQuotations?.OrderByDescending(p => p.UpdateTime).FirstOrDefault();
                if (er != null)
                {
                    UpdateTime = er.UpdateTime;
                    //未确认量必须等于 0
                    int notConfirmcount = FeedBackQuotations.Count(p => p.ButtonVisible == "1");
                    if (notConfirmcount == 0)
                    {
                        //终态 BrokerBCanceled("F"), BrokerACanceled("H"),
                        int sumcancelqty = FeedBackQuotations.Count(p => p.OrdStatus == "F" || p.OrdStatus == "H");
                        //BrokerBConfirm("E"),BrokerAConfirm("G"),
                        int sumfilledqty = FeedBackQuotations.Count(p => p.OrdStatus == "G" || p.OrdStatus == "1");
                        int countrecc = FeedBackQuotations.Count;

                        //当只有一条记录的时候 拒绝 关闭
                        if (sumcancelqty == countrecc && countrecc == 1)
                        {
                            if (FeedBackQuotations[0].OrdStatus == "1")
                                OnCloseSelf();
                        }

                        //只要有拒绝，就不关闭点价反馈
                        if (sumcancelqty > 0)
                        {
                            return;
                        }

                        if (!(sumcancelqty == countrecc))
                        {
                            if (countrecc == sumcancelqty + sumfilledqty)
                            {
                                OnCloseSelf();
                            }
                        }
                        else if (sumcancelqty + sumfilledqty == countrecc)
                        {
                            OnCloseSelf();
                        }
                        else if (sumfilledqty == countrecc)
                        {
                            OnCloseSelf();
                        }
                    }
                }
            }));
        }


        
        public void UpdateData(List<ExecutionReport> polist)
        {
            ThreadPool.QueueUserWorkItem(o => Application.Current.Dispatcher.Invoke(delegate
            {
                Application.Current.Dispatcher.Invoke(() =>
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
                                findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
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
                                    findDr.CounterPartyTraderName = po.CounterPartySubName;
                                    if (po.CounterPartySubID.Equals(po.PartySubID))
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

                                findDr.OrderID = po.OrderID;
                                findDr.TransactTime = po.TransactTime;
                                findDr.UpdateTime = po.UpdateTime;
                                findDr.OrdStatus = po.OrdStatus == "C" ? "D" : po.OrdStatus;
                                findDr.LastQty = po.LastQty;
                                findDr.OrderQty = po.OrderQty;
                                findDr.LeavesQty = po.LeavesQty;
                                findDr.ButtonVisible = "2";//Hide 

                                if ((po.OrdStatus.Equals("E")) && !po.LastQty.Equals(po.OrderQty))
                                {
                                    findDr.OrdStatus = "1";
                                    findDr.ButtonVisible = "1";
                                    findDr.PartLastQty = po.LastQty;
                                    BGBackground = (Brush)ParentWindow.FindResource("ConfirmLinearGradientBrush");
                                }
                                else if ((po.OrdStatus.Equals("G")) && !po.LastQty.Equals(po.OrderQty))
                                {
                                    findDr.OrdStatus = "1";
                                    findDr.ButtonVisible = "2";
                                    BGBackground = (Brush)ParentWindow.FindResource("ConfirmLinearGradientBrush");
                                }
                                if (findDr.OrdStatus.Equals("F") || findDr.OrdStatus.Equals("H"))
                                {
                                    BGBackground = (Brush)ParentWindow.FindResource("RejectLinearGradientBrush");
                                    findDr.ButtonVisible = "2";
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

                    UpdateConfirmData();
                });
            }));
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="inparam"></param>
        private void OnOkCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string key = (string)inparam;
                    ExecutionReport er = new ExecutionReport();
                    //er.LastQty = LastQty;
                    er.OrdStatus = "G";
                    er.CommandType = "24";
                    er.ExecID = key;
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);

                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        GrFeedBackViewModel findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
                        findDr.ButtonVisible = "2";
                        UpdateConfirmData();
                    }
                    else
                    {
                        GrFeedBackViewModel findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
                        findDr.OrdStatus = "X";
                        findDr.StatusMsg = $"错误 {reply.Msg}";
                    }
                });
            });
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="inparam"></param>
        private void OnRejectCommand(object inparam)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string key = (string)inparam;
                    ExecutionReport er = new ExecutionReport();
                    er.OrdStatus = "H";
                    er.CommandType = "25";
                    er.ExecID = key;
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        GrFeedBackViewModel findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
                        findDr.ButtonVisible = "2";
                        UpdateConfirmData();
                    }
                    else
                    {
                        GrFeedBackViewModel findDr = FeedBackQuotations.Where(a => a.ExecID == key).FirstOrDefault();
                        findDr.OrdStatus = "X";
                        findDr.StatusMsg = $"错误 {reply.Msg}";
                    }
                });
            });
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void OnCloseSelf()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ParentWindow != null)
                {
                    try
                    {
                        ParentWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error(ex);
                    }
                }
            });
        }

        #region Function
        private string getDayOrderInfo(string info7, int index)
        {
            string result = "";
            if (!string.IsNullOrEmpty(info7))
            {
                string[] info7Array = info7.Split('|');
                if (info7Array.Count() > (index + 1))
                {
                    result = "null".Equals(info7Array[index]) ? "": info7Array[index];
                }
            }
            return result;
        }
        #endregion
    }
}
