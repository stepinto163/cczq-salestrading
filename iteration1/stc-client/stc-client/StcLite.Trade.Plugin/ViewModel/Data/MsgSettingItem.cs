using STC.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.ViewModel.Data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class MsgSettingItem : BaseViewModel
    {
        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set
            {
                _securityID = value;
                this.OnPropertyChanged();
            }
        }

        private string _isBidLimit;

        public string IsBidLimit
        {
            get { return _isBidLimit; }
            set { _isBidLimit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isBidLimit_Edit;
        [JsonIgnore]
        public string IsBidLimit_Edit
        {
            get { return _isBidLimit_Edit; }
            set
            {
                _isBidLimit_Edit = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("BestBidLimit_Edit");
            }
        }


        private string _bestBidLimmit;

        public string BestBidLimit
        {
            get { return _bestBidLimmit; }
            set
            {
                _bestBidLimmit = value;
                this.OnPropertyChanged();
            }
        }

        private string _bestBidLimmit_Edit;
        [JsonIgnore]
        public string BestBidLimit_Edit
        {
            get { return _bestBidLimmit_Edit; }
            set
            {
                _bestBidLimmit_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isOfrLimit;

        public string IsOfrLimit
        {
            get { return _isOfrLimit; }
            set
            {
                _isOfrLimit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isOfrLimit_Edit;

        [JsonIgnore]
        public string IsOfrLimit_Edit
        {
            get { return _isOfrLimit_Edit; }
            set
            {
                _isOfrLimit_Edit = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("BestOfrLimit_Edit");
            }
        }

        private string _bestOfrLimmit;

        public string BestOfrLimit
        {
            get { return _bestOfrLimmit; }
            set
            {
                _bestOfrLimmit = value;
                this.OnPropertyChanged();
            }
        }

        private string _bestOfrLimmit_Edit;

        [JsonIgnore]
        public string BestOfrLimit_Edit
        {
            get { return _bestOfrLimmit_Edit; }
            set
            {
                _bestOfrLimmit_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isBestBp;

        public string IsBestBp
        {
            get { return _isBestBp; }
            set { _isBestBp = value;
                this.OnPropertyChanged(); }
        }

        private string _isBestBp_Edit;

        [JsonIgnore]
        public string IsBestBp_Edit
        {
            get { return _isBestBp_Edit; }
            set
            {
                _isBestBp_Edit = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("BestBp_Edit");
            }
        }


        private string _bestBp;

        public string BestBp
        {
            get { return _bestBp; }
            set
            {
                _bestBp = value;
                this.OnPropertyChanged();
            }
        }

        private string _bestBp_Edit;

        [JsonIgnore]
        public string BestBp_Edit
        {
            get { return _bestBp_Edit; }
            set
            {
                _bestBp_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isbidPrice;

        public string IsBidPrice
        {
            get { return _isbidPrice; }
            set
            {
                _isbidPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _isbidPrice_Edit;

        [JsonIgnore]
        public string IsBidPrice_Edit
        {
            get { return _isbidPrice_Edit; }
            set
            {
                _isbidPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }


        private string _isOfrPrice;

        public string IsOfrPrice
        {
            get { return _isOfrPrice; }
            set
            {
                _isOfrPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _isOfrPrice_Edit;

        [JsonIgnore]
        public string IsOfrPrice_Edit
        {
            get { return _isOfrPrice_Edit; }
            set
            {
                _isOfrPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _isExecPrice;

        public string IsExecPrice
        {
            get
            {
                return _isExecPrice;
            }
            set
            {
                _isExecPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _isExecPrice_Edit;

        [JsonIgnore]
        public string IsExecPrice_Edit
        {
            get
            {
                return _isExecPrice_Edit;
            }
            set
            {
                _isExecPrice_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _status;

        [JsonIgnore]
        public string Status
        {
            get { return _status; }
            set { _status = value;
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

        public void Clear()
        {
            this.SecurityID = string.Empty;
            this.IsBidLimit = string.Empty;
            this.IsBidLimit_Edit = string.Empty;
            this.BestBidLimit = string.Empty;
            this.BestBidLimit_Edit = string.Empty;
            this.IsOfrLimit = string.Empty;
            this.IsOfrLimit_Edit = string.Empty;
            this.BestOfrLimit = string.Empty;
            this.BestOfrLimit_Edit = string.Empty;
            this.IsBestBp = string.Empty;
            this.IsBestBp_Edit = string.Empty;
            this.BestBp = string.Empty;
            this.BestBp_Edit = string.Empty;
            this.IsBidPrice = string.Empty;
            this.IsBidPrice_Edit = string.Empty;
            this.IsOfrPrice = string.Empty;
            this.IsOfrPrice_Edit = string.Empty;
            this.IsExecPrice = string.Empty;
            this.IsExecPrice_Edit = string.Empty;
            this.Status = string.Empty;
            this.UpdateTime = string.Empty;
        }
    }
}
