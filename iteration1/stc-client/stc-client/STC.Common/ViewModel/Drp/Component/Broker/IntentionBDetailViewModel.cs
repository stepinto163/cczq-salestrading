using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class IntentionBDetailViewModel : ViewModelBase
    {
        #region
        public IntentionBUI ParentWindow;
        private string _OrderID;
        private string _ExecID;
        private string _SecurityID;
        private string _Symbol;
        private string _UpdateTime;
        private string _TraderName;
        private string _Side;
        private string _CounterPartyInstitutionName;
        private string _CounterPartyTraderName;
        private string _Price;
        private string _OrderQty;
        private string _LastQty;
        private decimal _PointOrderQty;
        private string _SettlType;
        private string _Comment;
        private string _OCOVisible;
        private string _PackageVisible;
        private string _SwitchVisible;
        private string _XVisible;
        private string _XXVisible;
        private string _QQMsg;
        public string OrderID
        {
            get
            {
                return _OrderID;
            }
            set
            {
                _OrderID = value;
                this.OnPropertyChanged();
            }
        }
        public string ExecID
        {
            get
            {
                return _ExecID;
            }
            set
            {
                _ExecID = value;
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

        public string QQMsg
        {
            get
            {
                return _QQMsg;
            }
            set
            {
                _QQMsg = value;
                this.OnPropertyChanged();
            }
        }
        public decimal PointOrderQty
        {
            get
            {
                return _PointOrderQty;
            }
            set
            {
                _PointOrderQty = value;
                this.OnPropertyChanged();
            }
        }
        private decimal amount = decimal.Zero;
        public decimal Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                SetButtonEnable();
                this.OnPropertyChanged();
            }
        }
        private decimal maxAmount = decimal.MaxValue;
        public decimal MaxAmount
        {
            get { return maxAmount; }
            set
            {
                maxAmount = value;
                SetButtonEnable();
                this.OnPropertyChanged();
            }
        }
        private string _SubtractionEnabled = "1";
        public string SubtractionEnabled
        {
            get { return _SubtractionEnabled; }
            set
            {
                _SubtractionEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private string _AddEnabled = "1";
        public string AddEnabled
        {
            get { return _AddEnabled; }
            set
            {
                _AddEnabled = value;
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
        private decimal MinAmount = decimal.Zero;
        public decimal AmountUnit { get { return 1000; } }

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

        //private string _SendQQErrorMsg;
        //public string SendQQErrorMsg
        //{
        //    get
        //    {
        //        return _SendQQErrorMsg;
        //    }
        //    set
        //    {
        //        _SendQQErrorMsg = value;
        //        this.OnPropertyChanged();
        //    }
        //}
        private string _AutoEnable = "1";
        public string AutoEnable
        {
            get
            {
                return _AutoEnable;
            }
            set
            {
                _AutoEnable = value;
                this.OnPropertyChanged();
            }
        }
        private string _ManualEnable = "1";
        public string ManualEnable
        {
            get
            {
                return _ManualEnable;
            }
            set
            {
                _ManualEnable = value;
                this.OnPropertyChanged();
            }
        }
        private string _PartySubID;
        public string PartySubID
        {
            get
            {
                return _PartySubID;
            }
            set
            {
                _PartySubID = value;
                this.OnPropertyChanged();
            }
        }
        private string _PartyTraderId;
        public string PartyTraderId
        {
            get
            {
                return _PartyTraderId;
            }
            set
            {
                _PartyTraderId = value;
                this.OnPropertyChanged();
            }
        }

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
        private string _ButtonIsEnable;
        public string ButtonIsEnable
        {
            get
            {
                return _ButtonIsEnable;
            }
            set
            {
                _ButtonIsEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _RejectVisible;
        public string RejectVisible
        {
            get
            {
                return _RejectVisible;
            }
            set
            {
                _RejectVisible = value;
                this.OnPropertyChanged();
            }
        }
        private string _StatusMsg;
        public string StatusMsg
        {
            get
            {
                return _StatusMsg;
            }
            set
            {
                _StatusMsg = value;
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
        private string _DayHandlInst;
        public string DayHandlInst
        {
            get
            {
                return _DayHandlInst;
            }
            set
            {
                _DayHandlInst = value;
                this.OnPropertyChanged();
            }
        }
        private string _TraderBQQ;
        public string TraderBQQ
        {
            get
            {
                return _TraderBQQ;
            }
            set
            {
                _TraderBQQ = value;
                this.OnPropertyChanged();
            }
        }
        private string _TraderBName;
        public string TraderBName
        {
            get
            {
                return _TraderBName;
            }
            set
            {
                _TraderBName = value;
                this.OnPropertyChanged();
            }
        }
        public ICommand SubtractionCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand RejectCommand { get; set; }

        public ICommand OkCommand { get; set; }
        public ICommand ManualCommand { get; set; }
        public ICommand AutoCommand { get; set; }
        #endregion
        public void BindingData(ExecutionReport po)
        {
            HandlInst = po.HandlInst;
            QQMsg = po.SendQQMsgTem;
            string sendqqmsgflag = po.SendQQMsgFlag;
            ManualEnable = "1";
            if ("1".Equals(sendqqmsgflag))
            {
                AutoEnable = "0";
            }
            else
            {
                AutoEnable = "1";
            }
            PartySubID = po.CounterPartySubID;
            PartyTraderId = po.CounterPartyTraderId;
            ExecID = po.ExecID;
            OrderID = po.StrategyOrderId;
            SecurityID = po.SecurityID;
            Symbol = po.Symbol;
            UpdateTime = po.UpdateTime;
            TraderName = po.PartySubName;
            Side = po.Side.Equals("1") ? "2" : "1";
            string counterTraderName = po.CounterPartyTraderName;
            if (!string.IsNullOrEmpty(counterTraderName))
            {
                CounterPartyInstitutionName = po.CounterPartyInstitutionName + string.Format("({0})", counterTraderName);
            }
            else
            {
                CounterPartyInstitutionName = po.CounterPartyInstitutionName;
            }
            CounterPartyTraderName = po.CounterPartySubName;
            if (PartySubID.Equals(po.PartySubID))
            {
                BrokerAIsMe = "1";
            }


            Price = po.Price;
            OrderQty = po.OrderQty;
            SettlType = po.SettlType;
            Comment = po.Remark;
            LastQty = po.LastQty;
            PointOrderQty = CommonUtils.ConvertToLong(po.LastQty) / 1000;
            Amount = CommonUtils.ConvertToDecimal(po.LastQty);
            MaxAmount = Amount;
            if (!string.IsNullOrEmpty(po.Tag1))
            {
                PopupERPo er = getDayOrderInfo(po.Tag1);
                DayPrice = er.Price;
                DayOrderQty = er.OrderQty;
                DaySettlType = er.SettlType;
                DayComment = er.Text;
                DayHandlInst = er.HandlInst;
                string OCOFlag = er.OCOFlag;
                string ExchangeSignFlag = er.ExchangeSignFlag;
                string BargainFlag = er.BargainFlag;
                string wholeFlag = er.WholeFlag;
                if ("1".Equals(wholeFlag))
                {
                    DayComment = "整量 " + DayComment;
                }
                if ("1".Equals(OCOFlag))
                {
                    OCOVisible = "0";
                }
                else
                {
                    OCOVisible = "1";
                }
                if ("2".Equals(OCOFlag))
                {
                    PackageVisible = "0";
                }
                else
                {
                    PackageVisible = "1";
                }
                if ("1".Equals(ExchangeSignFlag))
                {
                    SwitchVisible = "0";
                }
                else
                {
                    SwitchVisible = "1";
                }
                if ("1".Equals(BargainFlag))
                {
                    XVisible = "0";
                    XXVisible = "1";
                }
                else if ("2".Equals(BargainFlag))
                {
                    XVisible = "1";
                    XXVisible = "0";
                }
                else
                {
                    XVisible = "1";
                    XXVisible = "1";
                }

                if (po.OrdStatus.Equals("2"))
                {
                    ButtonIsEnable = "1";
                }
                else
                {
                    ButtonIsEnable = "0";
                }
                TraderBQQ = er.counterpartyTraderQQ;
            }
            RejectVisible = "1";
            if (po.OrdStatus.Equals("H"))
            {
                StatusMsg = "已拒绝";
                RejectVisible = "0";
            }
        }
        public IntentionBDetailViewModel()
        {
            this.SubtractionCommand = new ComRelayCommand(SubtractionClick);
            this.AddCommand = new ComRelayCommand(AddClick);
            this.RejectCommand = new ComRelayCommand(RejectClick);
            this.OkCommand = new ComRelayCommand(OkClick);
            this.ManualCommand = new ComRelayCommand(ManualClick);
            this.AutoCommand = new ComRelayCommand(AutoClick);
        }
        private PopupERPo getDayOrderInfo(string tag1)
        {
            if (string.IsNullOrEmpty(tag1))
            {
                return new PopupERPo();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PopupERPo>(tag1);
        }
        private string getDayOrderInfo(string info7, int index)
        {
            string result = "";
            if (!string.IsNullOrEmpty(info7))
            {
                string[] info7Array = info7.Split('|');
                if (info7Array.Count() > (index + 1))
                {
                    result = "null".Equals(info7Array[index]) ? "" : info7Array[index];
                }
            }
            return result;
        }
        public void OkClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExecutionReport er = new ExecutionReport();
                    //er.OrderID = OrderID;
                    er.LastQty = CommonUtils.ConvertToString(decimal.Multiply(PointOrderQty, 1000));
                    er.OrdStatus = "E";
                    er.ExecID = ExecID;
                    if (CommonUtils.ConvertToDecimal(OrderQty).CompareTo(CommonUtils.ConvertToDecimal(er.LastQty)) != 0)
                    {
                        er.CommandType = "27";
                    }
                    else
                    {
                        er.CommandType = "26";
                    }
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //IntentionBUIViewModel parent = (IntentionBUIViewModel)ParentWindow.DataContext;
                        //if (parent != null) 
                        //{
                        //    parent.OnCloseSelf();
                        //}
                        //OnCloseSelf();
                    }
                });
            });
        }

        public void RejectClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExecutionReport er = new ExecutionReport();
                    //er.OrderID = OrderID;
                    er.OrdStatus = "F";
                    er.CommandType = "28";
                    er.ExecID = ExecID;
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //IntentionBUIViewModel parent = (IntentionBUIViewModel)ParentWindow.DataContext;
                        //if (parent != null)
                        //{
                        //    parent.OnCloseSelf();
                        //}
                    }
                });
            });
        }
        private void AutoClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SendQQMsgReq er = new SendQQMsgReq();
                    er.execId = ExecID;
                    er.partySubId = PartySubID;
                    er.traderId = PartyTraderId;
                    er.userName = LocalEnv.UserInfo.UserName;
                    er.message = QQMsg;
                    er.sendType = "1";
                    PResult reply = GRHandler.Instance.sendQQMsg(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //AutoEnable = "0";
                    }
                });
            });
        }
        private void ManualClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    
                    if (!string.IsNullOrEmpty(TraderBQQ))
                    {
                        Process pro = new Process();
                        Clipboard.SetDataObject(QQMsg, true);
                        string arguments = string.Format("tencent://message/?Menu=yes&amp;uin={0}", TraderBQQ);
                        Process.Start(arguments);
                        Thread.Sleep(1000);
                        //if (LocalEnv.ConfInfo.QQStartSleep.HasValue && LocalEnv.ConfInfo.QQStartSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQStartSleep.Value);
                        System.Windows.Forms.SendKeys.SendWait("^V");

                        //if (LocalEnv.ConfInfo.QQSendSleep.HasValue && LocalEnv.ConfInfo.QQSendSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQSendSleep.Value);

                        Thread.Sleep(1000);
                        System.Windows.Forms.SendKeys.SendWait("{Enter}");
                        Clipboard.SetDataObject(QQMsg, true);
                    }
                    else 
                    {
                        string errorMsg = string.Format("{0} QQ is null", TraderBName);
                        BrokerMessageBoxGUI msgBox = new BrokerMessageBoxGUI();
                        msgBox.setWarnMsg(errorMsg, false);
                        msgBox.Show();
                    }
                });
            });
        }

        public void SubtractionClick()
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                if (amount - AmountUnit > MinAmount)
                {
                    Amount -= AmountUnit;
                    PointOrderQty--;
                }
            }));
        }

        public void AddClick()
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                if (amount + AmountUnit <= MaxAmount)
                {
                    Amount += AmountUnit;
                    PointOrderQty++;
                }
            }));
        }
        private void SetButtonEnable()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                AddEnabled = decimal.Multiply(PointOrderQty, AmountUnit) + AmountUnit <= MaxAmount ? "0" : "1";
                SubtractionEnabled = decimal.Multiply(PointOrderQty, AmountUnit) - AmountUnit > MinAmount ? "0" : "1";
            }));
        }
    }
}
