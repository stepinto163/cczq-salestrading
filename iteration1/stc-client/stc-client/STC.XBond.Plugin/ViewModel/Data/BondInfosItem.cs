using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 债券信息
    /// </summary>
    public class BondInfosItem : ViewModelBase
    {

        public BondInfosItem()
        {
            this.IsValidCase = true;
        }

        private string _Key = string.Empty;

        public string Key
        {
            get
            {
                return this._Key;
            }

            set
            {
                if (this._Key != value)
                {
                    this._Key = value;
                }
            }
        }

        private string _securityID = string.Empty;

        public string securityID
        {
            get
            {
                return this._securityID;
            }

            set
            {
                if (this._securityID != value)
                {
                    this._securityID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _symbol = string.Empty;

        public string symbol
        {
            get
            {
                return this._symbol;
            }

            set
            {
                if (this._symbol != value)
                {
                    this._symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _ExecValue = 0;

        public decimal ExecValue
        {
            get
            {
                return this._ExecValue;
            }

            set
            {
                if (this._ExecValue != value)
                {
                    this._ExecValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _OfrPrice = 0;

        public decimal OfrPrice
        {
            get
            {
                return this._OfrPrice;
            }

            set
            {
                if (this._OfrPrice != value)
                {
                    this._OfrPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _OfrQty = 0;

        public long OfrQty
        {
            get
            {
                return this._OfrQty;
            }

            set
            {
                if (this._OfrQty != value)
                {
                    this._OfrQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _BestOfrTrader = string.Empty;

        public string BestOfrTrader
        {
            get
            {
                return this._BestOfrTrader;
            }

            set
            {
                if (this._BestOfrTrader != value)
                {
                    this._BestOfrTrader = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _BestBidTrader = string.Empty;

        public string BestBidTrader
        {
            get
            {
                return this._BestBidTrader;
            }

            set
            {
                if (this._BestBidTrader != value)
                {
                    this._BestBidTrader = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _BidQty = 0;

        public long BidQty
        {
            get
            {
                return this._BidQty;
            }

            set
            {
                if (this._BidQty != value)
                {
                    this._BidQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _BestBidPrice = 0;

        public decimal BestBidPrice
        {
            get
            {
                return this._BestBidPrice;
            }

            set
            {
                if (this._BestBidPrice != value)
                {
                    this._BestBidPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _BestBidQty = 0;

        public long BestBidQty
        {
            get
            {
                return this._BestBidQty;
            }

            set
            {
                if (this._BestBidQty != value)
                {
                    this._BestBidQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _BestOfrPrice = 0;

        public decimal BestOfrPrice
        {
            get
            {
                return this._BestOfrPrice;
            }

            set
            {
                if (this._BestOfrPrice != value)
                {
                    this._BestOfrPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _BestOfrQty = 0;

        public long BestOfrQty
        {
            get
            {
                return this._BestOfrQty;
            }

            set
            {
                if (this._BestOfrQty != value)
                {
                    this._BestOfrQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _LastActionStatus = string.Empty;

        public string LastActionStatus
        {
            get
            {
                return this._LastActionStatus;
            }

            set
            {
                if (this._LastActionStatus != value)
                {
                    this._LastActionStatus = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _LastActionTime = string.Empty;

        public string LastActionTime
        {
            get
            {
                return this._LastActionTime;
            }

            set
            {
                if (this._LastActionTime != value)
                {
                    this._LastActionTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _CreateTime = string.Empty;

        public string CreateTime
        {
            get
            {
                return this._CreateTime;
            }

            set
            {
                if (this._CreateTime != value)
                {
                    this._CreateTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Venue = string.Empty;

        public string Venue
        {
            get
            {
                return this._Venue;
            }

            set
            {
                if (this._Venue != value)
                {
                    this._Venue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _VenueTypeGW = string.Empty;

        public string VenueTypeGW
        {
            get
            {
                return this._VenueTypeGW;
            }

            set
            {
                if (this._VenueTypeGW != value)
                {
                    this._VenueTypeGW = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _RemaingingTime;

        public string RemaingingTime
        {
            get
            {
                return this._RemaingingTime;
            }

            set
            {
                if (this._RemaingingTime != value)
                {
                    this._RemaingingTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool IsValidCase { get; set; }
        private bool _Switch = false;

        public bool Switch
        {
            get
            {
                return this._Switch;
            }

            set
            {
                //if (this._Switch != value)
                //{
                this._Switch = value;
                //if (IsValidCase)
                //{
                this.OnPropertyChanged();
                //}
                //this.IsValidCase = true;
                //}
            }
        }

        private decimal _BidPrice = 0;

        public decimal BidPrice
        {
            get
            {
                return this._BidPrice;
            }

            set
            {
                if (this._BidPrice != value)
                {
                    this._BidPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SettleType = string.Empty;

        public string SettleType
        {
            get
            {
                return this._SettleType;
            }

            set
            {
                if (this._SettleType != value)
                {
                    _SettleType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string _RatingCurrent = string.Empty;
        public string RatingCurrent
        {
            get
            {
                return this._RatingCurrent;
            }

            set
            {
                if (this._RatingCurrent != value)
                {
                    _RatingCurrent = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string _Info = string.Empty;
        public string Info
        {
            get
            {
                return this._Info;
            }

            set
            {
                if (this._Info != value)
                {
                    _Info = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ICommand _OpenCloseBondCommand;

        public ICommand OpenCloseBondCommand
        {
            get
            {
                return _OpenCloseBondCommand;
            }

            set
            {
                if (_OpenCloseBondCommand != value)
                {
                    _OpenCloseBondCommand = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _bidOrderTime = string.Empty;
        public string BidOrderTime
        {
            get
            {
                return _bidOrderTime;
            }

            set
            {
                if (_bidOrderTime != value)
                {
                    _bidOrderTime = value;
                    this.OnPropertyChanged();
                }
                
            }
        }

        private string _ofrOrderTime = string.Empty;
        public string OfrOrderTime
        {
            get
            {
                return _ofrOrderTime;
            }

            set
            {
                if (_ofrOrderTime != value)
                {
                    _ofrOrderTime = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _securityType = string.Empty;
        public string SecurityType
        {
            get
            {
                return _securityType;
            }

            set
            {
                if(_securityType != value)
                {
                    _securityType = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}
