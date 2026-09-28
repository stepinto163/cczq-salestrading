using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 我的订单
    /// </summary>
    public class MyOrdersItem : ViewModelBase
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

        private bool _isChecked = false;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;
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

        private decimal _NetPrice = 0;

        public decimal NetPrice
        {
            get
            {
                return this._NetPrice;
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

        private decimal _Price = 0;

        public decimal Price
        {
            get
            {
                return this._Price;
            }

            set
            {
                if (this._Price != value)
                {
                    this._Price = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _Qty = 0;

        public long Qty
        {
            get
            {
                return this._Qty;
            }

            set
            {
                if (this._Qty != value)
                {
                    this._Qty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _DealingQty = 0;

        public long DealingQty
        {
            get
            {
                return this._DealingQty;
            }

            set
            {
                if (this._DealingQty != value)
                {
                    this._DealingQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _ExecQty = 0;

        public long ExecQty
        {
            get
            {
                return this._ExecQty;
            }

            set
            {
                if (this._ExecQty != value)
                {
                    this._ExecQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _MyDealer = string.Empty;

        public string MyDealer
        {
            get
            {
                return this._MyDealer;
            }

            set
            {
                if (this._MyDealer != value)
                {
                    this._MyDealer = value;
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
                    this._SettleType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _OrdType = string.Empty;

        public string OrdType
        {
            get
            {
                return this._OrdType;
            }

            set
            {
                if (this._OrdType != value)
                {
                    this._OrdType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _STCCode;

        public string STCCode
        {
            get
            {
                return this._STCCode;
            }

            set
            {
                if (this._STCCode != value)
                {
                    this._STCCode = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _OrderStatus;

        public string OrderStatus
        {
            get
            {
                return this._OrderStatus;
            }

            set
            {
                if (this._OrderStatus != value)
                {
                    this._OrderStatus = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Veneues;

        public string Veneues
        {
            get
            {
                return this._Veneues;
            }

            set
            {
                if (this._Veneues != value)
                {
                    this._Veneues = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _TransactTime = string.Empty;

        public string TransactTime
        {
            get
            {
                return this._TransactTime;
            }

            set
            {
                if (this._TransactTime != value)
                {
                    this._TransactTime = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _AlgoName;
        public string AlgoName
        {
            get
            {
                return _AlgoName;
            }

            set
            {
                if (this._AlgoName != value)
                {
                    this._AlgoName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ShowDeleteBtn = Visibility.Collapsed;
        public Visibility ShowDeleteBtn
        {
            get
            {
                return _ShowDeleteBtn;
            }

            set
            {
                if (this._ShowDeleteBtn != value)
                {
                    this._ShowDeleteBtn = value;
                    this.OnPropertyChanged();
                }
            }
        }

        


    }
}
