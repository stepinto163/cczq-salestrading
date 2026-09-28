using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 行情数据
    /// </summary>
    public class QuotationItem : ViewModelBase
    {
        private double _TotalQty = 0;

        public double TotalQty
        {
            get
            {
                return this._TotalQty;
            }

            set
            {
                if (this._TotalQty != value)
                {
                    this._TotalQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

 

        private string _Time = string.Empty;

        public string Time
        {
            get
            {
                return this._Time;
            }

            set
            {
                if (this._Time != value)
                {
                    this._Time = value;
                    this.OnPropertyChanged();
                }
            }
        }

        

        private string _Source = string.Empty;

        public string Source
        {
            get
            {
                return this._Source;
            }

            set
            {
                if (this._Source != value)
                {
                    this._Source = value;
                    this.OnPropertyChanged();
                }
            }
        }

       
        private int _Qty = 0;

        public int Qty
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

        private bool _IsBidBest = false;

        public bool IsBidBest
        {
            get
            {
                return this._IsBidBest;
            }

            set
            {
                if (this._IsBidBest != value)
                {
                    this._IsBidBest = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _IsOfrBest = false;

        public bool IsOfrBest
        {
            get
            {
                return this._IsOfrBest;
            }

            set
            {
                if (this._IsOfrBest != value)
                {
                    this._IsOfrBest = value;
                    this.OnPropertyChanged();
                }
            }
        }



        private string _FlagType = string.Empty;

        public string FlagType
        {
            get
            {
                return this._FlagType;
            }

            set
            {
                if (this._FlagType != value)
                {
                    this._FlagType = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}
