using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp.Component.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class UnderlineExecTpicUIViewModel : ViewModelBase
    {
        #region
        public UnderlineExecTpicUI ParentWindow;
        private string _OrderID;
        private string _ExecID;
        private string _SecurityID;
        private string _Symbol;
        private string _UpdateTime;
        private string _TraderName;
        private string _InstitutionName;
        private string _Side;
        private string _CounterPartyInstitutionName;
        private string _CounterPartyTraderName;
        private string _Price;
        private string _OrderQty;
        private string _LastQty;
        private decimal _PointOrderQty;
        private string _SettlType;
        private string _SettleAllDisplay;
        private string _Comment;
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
        public string InstitutionName
        {
            get
            {
                return _InstitutionName;
            }
            set
            {
                _InstitutionName = value;
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

        public string SettleAllDisplay
        {
            get
            {
                return _SettleAllDisplay;
            }
            set
            {
                _SettleAllDisplay = value;
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
        #endregion
        public ICommand OkCommand { get; set; }
        public void BindingData(ExecutionReport po)
        {
            HandlInst = po.HandlInst;
            ExecID = po.ExecID;
            OrderID = po.StrategyOrderId;
            SecurityID = po.SecurityID;
            Symbol = po.Symbol;
            UpdateTime = po.UpdateTime;
            TraderName = po.PartySubName;
            string traderName = po.TraderName;
            if (!string.IsNullOrEmpty(traderName))
            {
                InstitutionName = po.InstitutionShowName + string.Format("({0})", traderName);
            }
            else
            {
                InstitutionName = po.InstitutionShowName;
            }
            Side = po.Side.Equals("1") ? "买入" : "出给";
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
            Price = po.Price;
            OrderQty = po.LastQty;
            //修改和交易处理一样的算法
            ////if (!string.IsNullOrEmpty(po.SettlType2) && !po.SettlType.Equals(po.SettlType2))
            ////{
            ////    SettlType = string.Format("{0}/{1}", CommonUtils.ConvertSettlType(po.SettlType, "popup"), CommonUtils.ConvertSettlType(po.SettlType2, "popup"));
            ////}
            ////else
            ////{
            ////    SettlType = po.SettlType;
            ////}
            
            if (CommonUtils.IsDealHistDay(po))
                SettleAllDisplay = CommonUtils.GetHistoryDisplaySettle(po);
            else
                SettleAllDisplay = CommonUtils.GetDisplaySettle(po);


            Comment = po.Remark;
            LastQty = po.LastQty;
        }



        public UnderlineExecTpicUIViewModel() 
        {
            this.OkCommand = new ComRelayCommand(OkClick);
        }

        private void OkClick()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ExecutionReport er = new ExecutionReport();
                    //er.StrategyOrderId = OrderID;
                    er.LastQty = LastQty;
                    er.OrdStatus = "I";
                    er.ExecID = ExecID;
                    er.CloseBy = LocalEnv.UserInfo.UserName;
                    er.CloseById = LocalEnv.UserInfo.UserId;
                    er.CommandType = "26";
                    PResult reply = GRHandler.Instance.ExecOrderUpdate(er);
                    if (reply != null && reply.Code.Equals("0000"))
                    {
                        //OnCloseSelf();
                    }
                });
            });
        }

        private void OnCloseSelf()
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
    }
}
