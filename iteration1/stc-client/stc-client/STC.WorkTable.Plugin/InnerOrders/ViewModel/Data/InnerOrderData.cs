using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.InnerOrders.ViewModel.Data
{
    public class InnerOrderData : BaseViewModel
    {
        private string _orderID;

        public string OrderID
        {
            get { return _orderID; }
            set
            {
                if (_orderID != value)
                {

                    _orderID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _securityID;

        public string SecurityID
        {
            get { return _securityID; }
            set {

                if (_securityID != value)
                {
                    _securityID = value;
                    this.OnPropertyChanged();
                }
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

        private string _firmflag;

        public string FirmFlag
        {
            get
            {
                return _firmflag;
            }
            set
            {
                if (_firmflag != value)
                {
                    _firmflag = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _symbol;
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _remaingingTime;

        public string RemaingingTime
        {
            get
            {
                return _remaingingTime;
            }
            set
            {
                if (_remaingingTime != value)
                {
                    _remaingingTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _settleType;

        public string SettleType
        {
            get { return _settleType; }
            set {
                if (_settleType != value)
                {
                    _settleType = value;
                    this.OnPropertyChanged();

                    string newSettleType;
                    string settleDate = CommonUtils.CalSettlDate(DateTimeEx.ServerTime.ToString("yyyyMMdd"), value, out newSettleType);
                    if (!value.Contains("+"))
                        settleDate += value;
                    this.SettlDate = settleDate;
                }
            }
        }

        private string _settlDate;

        public string SettlDate
        {
            get { return _settlDate; }
            set
            {
                if (_settlDate != value)
                {
                    _settlDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _roleId;

        public string RoleId
        {
            get { return _roleId; }
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _roleName;

        public string RoleName
        {
            get { return _roleName; }
            set
            {
                if (_roleName != value)
                {
                    _roleName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _price;

        public decimal? Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long? _qty;

        public long? Qty
        {
            get
            {
                return _qty;
            }
            set
            {
                if (_qty != value)
                {
                    _qty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _partySubName;

        public string PartySubName
        {
            get
            {
                return _partySubName;
            }
            set
            {
                if (_partySubName != value)
                {
                    _partySubName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get
            {
                return _updateTime;
            }
            set
            {
                if (_updateTime != value)
                {
                    _updateTime = value;
                    this.OnPropertyChanged();
                }
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

    }
}
