using STC.Mm.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Mm.Plugin.ViewModel.Data
{
    /// <summary>
    /// 市场成交
    /// </summary>
    public class MarketExecutionsItem : ViewModelBase
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

        private decimal _ExecutePricePercent = 0;

        public decimal ExecutePricePercent
        {
            get
            {
                return this._ExecutePricePercent;
            }

            set
            {
                if (this._ExecutePricePercent != value)
                {
                    this._ExecutePricePercent = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _ExecuteAmount = 0;

        public decimal ExecuteAmount
        {
            get
            {
                return this._ExecuteAmount;
            }

            set
            {
                this._ExecuteAmount = value;
                this.OnPropertyChanged();
            }
        }

        private string _ExecutePricePercent_Display = string.Empty;

        public string ExecutePricePercent_Display
        {
            get
            {
                return this._ExecutePricePercent_Display;
            }

            set
            {
                if (this._ExecutePricePercent_Display != value)
                {
                    this._ExecutePricePercent_Display = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SourceTrader = string.Empty;

        public string SourceTrader
        {
            get
            {
                return this._SourceTrader;
            }

            set
            {
                if (this._SourceTrader != value)
                {
                    this._SourceTrader = value;
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

        private string _TimeStamp = string.Empty;

        public string TimeStamp
        {
            get
            {
                return this._TimeStamp;
            }

            set
            {
                if (this._TimeStamp != value)
                {
                    this._TimeStamp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _ShowStar;

        public Visibility ShowStar
        {
            get
            {
                return _ShowStar;
            }
            set
            {
                if (this._ShowStar != value)
                {
                    this._ShowStar = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SettlType = string.Empty;
        public string SettlType
        {
            get
            {
                return _SettlType;
            }

            set
            {
                if (_SettlType != value)
                {
                    _SettlType = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _info1 = string.Empty;
        public string Info1
        {
            get
            {
                return _info1;
            }

            set
            {
                _info1 = value;
                this.OnPropertyChanged();

            }
        }

        private string _info2 = string.Empty;
        public string Info2
        {
            get
            {
                return _info2;
            }

            set
            {
                _info2 = value;
                this.OnPropertyChanged();

            }
        }

        private string _downUp = string.Empty;
        public string DownUp
        {
            get
            {
                return _downUp;
            }

            set
            {
                _downUp = value;
                this.OnPropertyChanged();

            }
        }

        //0:- 1：up 2:down
        private int _downUpSort = 0;
        public int DownUpSort
        {
            get
            {
                return _downUpSort;
            }

            set
            {
                _downUpSort = value;
                this.OnPropertyChanged();

            }
        }

        private string _downUpBrush;
        public string DownUpBrush
        {
            get
            {
                return _downUpBrush;
            }

            set
            {
                _downUpBrush = value;
                this.OnPropertyChanged();

            }
        }

        private string _Bp;
        public string Bp
        {
            get
            {
                return _Bp;
            }

            set
            {
                _Bp = value;
                this.OnPropertyChanged();

            }
        }

        private bool _isFirst;
        public bool IsFirst
        {
            get
            {
                return _isFirst;
            }

            set
            {
                _isFirst = value;
                this.OnPropertyChanged();

            }
        }

        private string _isValid;

        public string IsValid
        {
            get
            {
                return _isValid;
            }

            set
            {
                _isValid = value;
                this.OnPropertyChanged();

            }
        }

        private string _venueTypeGW;

        public string VenueTypeGW
        {
            get
            {
                return _venueTypeGW;
            }

            set
            {
                _venueTypeGW = value;
                this.OnPropertyChanged();

            }
        }
        private decimal _volume = 0;

        public decimal Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                _volume = value;
                this.OnPropertyChanged();
            }
        }

        private string _execID;

        public string ExecID
        {
            get
            {
                return _execID;
            }

            set
            {
                _execID = value;
                this.OnPropertyChanged();

            }
        }
    }
}
