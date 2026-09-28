using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.ViewModel.Data
{
    public class TradeRequestData : ViewModelBase
    {
        private bool isChecked;

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool _enable = false;

        public bool enable
        {
            get
            {
                return _enable;
            }
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _IntentionId;

        public string IntentionId
        {
            get { return _IntentionId; }
            set
            {
                _IntentionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityId;

        public string SecurityId
        {
            get { return _securityId; }
            set
            {
                _securityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                this.OnPropertyChanged();
            }
        }

        private string _side;

        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price;

        public decimal? Price
        {
            get { return _price; }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _netPrice;
        public decimal? NetPrice
        {
            get { return _netPrice; }
            set
            {
                _netPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty;

        public decimal? Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
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

        private long? _displayQty;

        public long? DisplayQty
        {
            get { return _displayQty; }
            set
            {
                _displayQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionId;

        public string InstitutionId
        {
            get { return _institutionId; }
            set
            {
                _institutionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName;

        public string InstitutionName
        {
            get { return _institutionName; }
            set
            {
                _institutionName = value;
                this.OnPropertyChanged();
            }
        }


        private string _traderId;

        public string TraderId
        {
            get { return _traderId; }
            set
            {
                _traderId = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName;

        public string TraderName
        {
            get { return _traderName; }
            set
            {
                _traderName = value;
                this.OnPropertyChanged();
            }
        }

        private string _qqNumber;

        public string QqNumber
        {
            get { return _qqNumber; }
            set
            {
                _qqNumber = value;
                this.OnPropertyChanged();
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                this.OnPropertyChanged();
            }
        }


        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if(value == "2")
                {
                    this.enable = true;
                }
                else
                {
                    this.enable = false;
                }
                this.OnPropertyChanged();
            }
        }

        private string _settlType;

        public string SettlType
        {
            get { return _settlType; }
            set
            {
                _settlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set
            {
                _createTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                this.OnPropertyChanged();

                IsMe = _userName == LocalEnv.UserInfo.UserName;
            }
        }

        private string _nikeName;

        public string NikeName
        {
            get
            {
                return _nikeName;
            }
            set
            {
                _nikeName = value;
                this.OnPropertyChanged();
            }
        }

        private double _completeRate;

        public double CompleteRate
        {
            get
            {
                return _completeRate;
            }
            set
            {
                _completeRate = value;
                this.OnPropertyChanged();
            }
        }
        
        private decimal? _executionPrice;

        public decimal? ExecutionPrice
        {
            get
            {
                return _executionPrice;
            }
            set
            {
                _executionPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _executionQty;

        public decimal? ExecutionQty
        {
            get
            {
                return _executionQty;
            }
            set
            {
                _executionQty = value;
                this.OnPropertyChanged();
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
                _PNL = value;
                this.OnPropertyChanged();
            }
        }

        private long _runningTime;

        public long RunningTime
        {
            get
            {
                return _runningTime;
            }
            set
            {
                _runningTime = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsMe;
        public bool IsMe
        {
            get
            {
                return _IsMe;
            }
            set
            {
                _IsMe = value;
                this.OnPropertyChanged();
            }
        }

        #region 基础数据
        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private string _securityType;
        public string SecurityType
        {
            get { return _securityType; }
            set { _securityType = value; this.OnPropertyChanged(); }
        }


        private string _bondCategory;
        public string BondCategory
        {
            get { return _bondCategory; }
            set { _bondCategory = value; this.OnPropertyChanged(); }
        }

        private string _issuer;
        public string Issuer
        {
            get { return _issuer; }
            set { _issuer = value; this.OnPropertyChanged(); }
        }

        private decimal _remaingingTime_convert;
        public decimal RemaingingTime_convert
        {
            get { return _remaingingTime_convert; }
            set { _remaingingTime_convert = value; this.OnPropertyChanged(); }
        }

        private string _issuerRatingCurrentNPY;
        public string IssuerRatingCurrentNPY
        {
            get { return _issuerRatingCurrentNPY; }
            set { _issuerRatingCurrentNPY = value; this.OnPropertyChanged(); }
        }
        #endregion

        public ConfirmIntentionQuoteRequest Request { get; set; }

        public TradeRequestData Clone()
        {
            TradeRequestData newData = new TradeRequestData();
            newData.IntentionId = this.IntentionId;
            newData.SecurityId = this.SecurityId;
            newData.Symbol = this.Symbol;
            newData.Side = this.Side;
            newData.NetPrice = this.NetPrice;
            newData.Price = this.Price;
            newData.BookQty = this.BookQty;
            newData.LeftQty = this.LeftQty;
            newData.Qty = this.Qty;
            newData.DisplayQty = this.DisplayQty;
            newData.InstitutionId = this.InstitutionId;
            newData.InstitutionName = this.InstitutionName;
            newData.TraderId = this.TraderId;
            newData.TraderName = this.TraderName;
            newData.QqNumber = this.QqNumber;
            newData.Text = this.Text;
            newData.Status = this.Status;
            newData.SettlType = this.SettlType;
            newData.CreateTime = this.CreateTime;
            newData.UpdateTime = this.UpdateTime;
            newData.UserName = this.UserName;
            newData.NikeName = this.NikeName;
            newData.CompleteRate = this.CompleteRate;
            newData.ExecutionPrice = this.ExecutionPrice;
            newData.PNL = this.PNL;
            newData.RunningTime = this.RunningTime;
            newData.SecurityType = this.SecurityType;
            newData.BondCategory = this.BondCategory;
            newData.Issuer = this.Issuer;
            newData.RemaingingTime_convert = this.RemaingingTime_convert;
            newData.IssuerRatingCurrentNPY = this.IssuerRatingCurrentNPY;

            return newData;
        }
    }
}
