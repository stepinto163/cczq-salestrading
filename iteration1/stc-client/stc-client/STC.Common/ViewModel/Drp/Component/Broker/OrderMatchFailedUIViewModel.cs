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
using System.Windows.Media;

namespace STC.Common.ViewModel.Drp.Component.Broker
{
    public class OrderMatchFailedUIViewModel : ViewModelBase
    {
        #region
        public OrderMatchFailedUI ParentWindow;
        private string _OrderID;
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
        private string _Msg;
        private string _Count;
        private string _RejectText;
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
        public string Msg
        {
            get
            {
                return _Msg;
            }
            set
            {
                _Msg = value;
                this.OnPropertyChanged();
            }
        }
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
        public string RejectText
        {
            get
            {
                return _RejectText;
            }
            set
            {
                _RejectText = value;
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
        public ICommand DetailCommand { get; set; }
        #endregion
        public void BindingData(ExecutionReport po)
        {
            HandlInst = po.HandlInst;
            OrderID = po.StrategyOrderId;
            SecurityID = po.SecurityID;
            Symbol = po.Symbol;
            UpdateTime = po.UpdateTime;
            TraderName = po.PartySubName;
            Side = po.Side.Equals("1") ? "2" : "1";
            BGBackground = Side == "2" ? (Brush)ParentWindow.FindResource("OfrLinearGradientBrush") : (Brush)ParentWindow.FindResource("BidLinearGradientBrush");
            CounterPartyInstitutionName = po.CounterPartyInstitutionName;
            CounterPartyTraderName = po.CounterPartyTraderName;
            Price = po.Price;
            OrderQty = po.OrderQty;
            SettlType = po.SettlType;
            Msg = "没有量";
            Count = po.Info2;
            if (!string.IsNullOrWhiteSpace(po.OrdRejReason))
            {
                string[] strs = po.OrdRejReason.Split(',');
                if (strs != null && strs.Length > 0)
                {
                    RejectText = ConstCode.Instance.getMsg(strs[0],po.RejectText);
                }
            }
        }
        public OrderMatchFailedUIViewModel() 
        {
            this.DetailCommand = new ComRelayCommand(DetailClick);
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
    }
}
