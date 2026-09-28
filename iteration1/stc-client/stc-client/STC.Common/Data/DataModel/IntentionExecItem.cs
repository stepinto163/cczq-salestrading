using STC.Common.Model.Table;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data.DataModel
{
    public class IntentionExecItem : ViewModelBase
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _ExecId;

        public string ExecId
        {
            get
            {
                return _ExecId;
            }
            set
            {
                if (_ExecId != value)
                {
                    _ExecId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _IntentionId;

        public string IntentionId
        {
            get { return _IntentionId; }
            set {
                if (_IntentionId != value)
                {
                    _IntentionId = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _RelationFlag;

        public string RelationFlag
        {
            get { return _RelationFlag; }
            set
            {
                if (_RelationFlag != value)
                {
                    _RelationFlag = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SecurityId;

        public string SecurityId
        {
            get { return _SecurityId; }
            set {
                if (_SecurityId != value)
                {
                    _SecurityId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Symbol;

        public string Symbol
        {
            get { return _Symbol; }
            set
            {
                if (_Symbol != value)
                {
                    _Symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _Price;

        public decimal? Price
        {
            get { return _Price; }
            set {
                if (_Price != value)
                {
                    _Price = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _bookQty;

        public decimal? BookQty
        {
            get { return _bookQty; }
            set
            {
                _bookQty = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _leftQty;

        public decimal? LeftQty
        {
            get { return _leftQty; }
            set
            {
                _leftQty = value;
                this.OnPropertyChanged();
            }
        }


        private decimal? _remainQty;

        public decimal? RemainQty
        {
            get { return _remainQty; }
            set
            {
                _remainQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _side;

        public string Side
        {
            get { return _side; }
            set {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _Venues;

        public string Venues
        {
            get { return _Venues; }
            set { if (_Venues != value)
                {
                    _Venues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _settleType;

        public string SettlType
        {
            get { return _settleType; }
            set { 
                if (_settleType != value)
                {
                    _settleType = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _institutionId;

        public string InstitutionId
        {
            get { return _institutionId; }
            set
            {
                if (_institutionId != value)
                {
                    _institutionId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _institutionName;

        public string InstitutionName
        {
            get { return _institutionName; }
            set
            {
                if (_institutionName != value)
                {
                    _institutionName = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _traderId;

        public string TraderId
        {
            get { return _traderId; }
            set
            {
                if (_traderId != value)
                {
                    _traderId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderName;

        public string TraderName
        {
            get { return _traderName; }
            set
            {
                if (_traderName != value)
                {
                    _traderName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _counterPartyName;

        public string CounterPartyName
        {
            get { return _counterPartyName; }
            set {
                if (_counterPartyName != value)
                {
                    _counterPartyName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _counterPartySubName;

        public string CounterPartySubName
        {
            get { return _counterPartySubName; }
            set {
                if (_counterPartySubName != value)
                {
                    _counterPartySubName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _userName;

        public string UserName
        {
            get { return _counterPartySubName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _nikeName;

        public string NikeName
        {
            get { return _counterPartySubName; }
            set
            {
                if (_nikeName != value)
                {
                    _nikeName = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal? _PNL;

        public decimal? PNL
        {
            get
            {
                return _PNL;
            }
            set
            {
                if (_PNL != value)
                {
                    _PNL = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _tradeDate;
        public string TradeDate
        {
            get
            {
                return _tradeDate;
            }
            set
            {
                if (_tradeDate != value)
                {
                    _tradeDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _status;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TransactTime;
        public string TransactTime
        {
            get
            {
                return _TransactTime;
            }
            set
            {
                if (_TransactTime != value)
                {
                    _TransactTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _UpdateTime;

        public string UpdateTime
        {
            get { return _UpdateTime; }
            set {
                if (_UpdateTime != value)
                {
                    _UpdateTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _remark;

        public string Remark
        {
            get { return _remark; }
            set
            {
                if (_remark != value)
                {
                    _remark = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public IntentionExecInfo Info { get; set; }


        public IntentionExecItem Clone()
        {
            IntentionExecItem newData = new IntentionExecItem();
            newData.ExecId = this.ExecId;
            newData.RelationFlag = this.RelationFlag;
            newData.IntentionId = this.IntentionId;
            newData.SecurityId = this.SecurityId;
            newData.Symbol = this.Symbol;
            newData.Side = this.Side;
            newData.Venues = this.Venues;
            //newData.NetPrice = this.NetPrice;
            newData.Price = this.Price;
            newData.BookQty = this.BookQty;
            newData.LeftQty = this.LeftQty;
            //newData.Qty = this.Qty;
            //newData.DisplayQty = this.DisplayQty;
            newData.InstitutionId = this.InstitutionId;
            newData.InstitutionName = this.InstitutionName;
            newData.TraderId = this.TraderId;
            newData.TraderName = this.TraderName;
            newData.CounterPartyName = this.CounterPartyName;
            newData.CounterPartySubName = this.CounterPartySubName;
            //newData.QqNumber = this.QqNumber;
            //newData.Text = this.Text;
            newData.Status = this.Status;
            newData.SettlType = this.SettlType;
            //newData.CreateTime = this.CreateTime;
            newData.TradeDate = this.TradeDate;
            newData.UpdateTime = this.UpdateTime;
            newData.TransactTime = this.TransactTime;
            newData.UserName = this.UserName;
            newData.NikeName = this.NikeName;
            //newData.CompleteRate = this.CompleteRate;
            //newData.ExecutionPrice = this.ExecutionPrice;
            newData.PNL = this.PNL;
            //newData.RunningTime = this.RunningTime;
            //newData.SecurityType = this.SecurityType;
            newData.Remark = this.Remark;
            newData.Info = this.Info;

            return newData;
        }
    }
}
