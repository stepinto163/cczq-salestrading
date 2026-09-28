using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkBench.Plugin.ViewModel.Data
{
    public class TradeRequestData : ViewModelBase
    {
         private string _IntentionId;

        public string IntentionId
        {
            get { return _IntentionId; }
            set {
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
    }
}
