using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 我的成交
    /// </summary>
    public class MyExecutionsItem : ViewModelBase
    {
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

        private string _ExecID;

        public string ExecID
        {
            get
            {
                return _ExecID;
            }

            set
            {
                if (this._ExecID != value)
                {
                    this._ExecID = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _Side = string.Empty;

        public string Side
        {
            get
            {
                return this._Side;
            }

            set
            {
                if (this._Side != value)
                {
                    this._Side = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SecurityID;

        public string SecurityID
        {
            get
            {
                return this._SecurityID;
            }

            set
            {
                if (this._SecurityID != value)
                {
                    this._SecurityID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Symbol = string.Empty;

        public string Symbol
        {
            get
            {
                return this._Symbol;
            }

            set
            {
                if (this._Symbol != value)
                {
                    this._Symbol = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _LastPx;

        public decimal LastPx
        {
            get
            {
                return _LastPx;
            }

            set
            {
                if (this._LastPx != value)
                {
                    this._LastPx = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _NetPrice;

        public decimal NetPrice
        {
            get
            {
                return _NetPrice;
            }

            set
            {
                if (this._NetPrice != value)
                {
                    this._NetPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _DirtyPrice;

        public decimal DirtyPrice
        {
            get
            {
                return _DirtyPrice;
            }

            set
            {
                if (this._DirtyPrice != value)
                {
                    this._DirtyPrice = value;
                    this.OnPropertyChanged();
                }
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
                if (this._SettlType != value)
                {
                    this._SettlType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _CumQty;

        public long CumQty
        {
            get
            {
                return _CumQty;
            }

            set
            {
                if (this._CumQty != value)
                {
                    this._CumQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _LastQty;
        public long LastQty
        {
            get
            {
                return _LastQty;
            }

            set
            {
                if (this._LastQty != value)
                {
                    this._LastQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _DeliveryAmount;

        public decimal DeliveryAmount
        {
            get
            {
                return _DeliveryAmount;
            }

            set
            {
                if (this._DeliveryAmount != value)
                {
                    this._DeliveryAmount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _CounterPartyName;
        public string CounterPartyName
        {
            get
            {
                return _CounterPartyName;
            }

            set
            {
                if (this._CounterPartyName != value)
                {
                    this._CounterPartyName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _CounterPartySubName;
        public string CounterPartySubName
        {
            get
            {
                return _CounterPartySubName;
            }

            set
            {
                if (this._CounterPartySubName != value)
                {
                    this._CounterPartySubName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Venues;
        public string Venues
        {
            get
            {
                return _Venues;
            }

            set
            {
                if (this._Venues != value)
                {
                    this._Venues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _PartySubName;
        public string PartySubName
        {
            get
            {
                return _PartySubName;
            }

            set
            {
                if (this._PartySubName != value)
                {
                    this._PartySubName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _ExecutionTime;
        public string ExecutionTime
        {
            get
            {
                return _ExecutionTime;
            }

            set
            {
                if(this._ExecutionTime != value)
                {
                    this._ExecutionTime = value;
                    this.OnPropertyChanged();
                }
               
            }
        }

        private string _RefOrderID;

        public string RefOrderID
        {
            get
            {
                return _RefOrderID;
            }

            set
            {
                if (this._RefOrderID != value)
                {
                    this._RefOrderID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _OrderID;
        public string OrderID
        {
            get
            {
                return _OrderID;
            }

            set
            {
                if (this._OrderID != value)
                {
                    this._OrderID = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}
