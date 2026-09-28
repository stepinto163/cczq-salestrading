using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.ViewModel.Data
{
    /// <summary>
    /// 市场成交
    /// </summary>
    public class MarketExecutionsItem : ViewModelBase
    {
        private string _MarketIndicator = string.Empty;

        public string MarketIndicator
        {
            get
            {
                return this._MarketIndicator;
            }

            set
            {
                if (this._MarketIndicator != value)
                {
                    this._MarketIndicator = value;
                }
            }
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

        private string _symbol;

        public string Symbol
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


        private string _InstitutionName = string.Empty;

        public string InstitutionName
        {
            get
            {
                return this._InstitutionName;
            }

            set
            {
                if (this._InstitutionName != value)
                {
                    this._InstitutionName = value;
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
        /// <summary>
        /// UpdateTime
        /// </summary>
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
                    if (!string.IsNullOrEmpty(value) && "1".Equals(value))
                    {
                        if (!SecurityID.EndsWith(".SH") && !SecurityID.EndsWith(".SZ"))
                        {
                            SettlTypeT0Vis = "true";
                        }
                    }
                    else
                    {
                        SettlTypeYuanVis = "false";
                    }
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

        private string _info10 = string.Empty;
        public string Info10
        {
            get
            {
                return _info10;
            }

            set
            {
                _info10 = value;
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

        private string _orderID;

        public string OrderID
        {
            get { return _orderID; }
            set 
            { 
                _orderID = value;
                this.OnPropertyChanged();
            }
        }


        private string _NetPrice_Display = string.Empty;

        public string NetPrice_Display
        {
            get
            {
                return this._NetPrice_Display;
            }

            set
            {
                if (this._NetPrice_Display != value)
                {
                    this._NetPrice_Display = value;
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
        private int _rowHeight = 24;

        public int RowHeight
        {
            get
            {
                return _rowHeight;
            }

            set
            {
                _rowHeight = value;
                this.OnPropertyChanged();

            }
        }

        private string _isNothingDown;

        public string IsNothingDown
        {
            get
            {
                return _isNothingDown;
            }

            set
            {
                _isNothingDown = value;
                this.OnPropertyChanged();

            }
        }

        //现手
        private string handNow;
        public string HandNow
        {
            get
            {
                return handNow;
            }

            set
            {
                handNow = value;
                this.OnPropertyChanged();

            }
        }

        //曾仓
        private string openInterest;
        public string OpenInterest
        {
            get
            {
                return openInterest;
            }

            set
            {
                openInterest = value;
                this.OnPropertyChanged();

            }
        }

        private Brush futureSideBrush;
        public Brush FutureSideBrush
        {
            get { return futureSideBrush; }
            set
            {
                futureSideBrush = value;
                this.OnPropertyChanged();
            }
        }

        //国债期货成交价格颜色显示枚举
        /// <summary>
        /// 1: 红色
        /// 2: 绿色
        /// 3: 白色
        /// </summary>
        public string priceBrushFlag;
        public string PriceBrushFlag
        {
            get
            {
                return priceBrushFlag;
            }

            set
            {
                priceBrushFlag = value;
                this.OnPropertyChanged();

            }
        }

        private int _showFontSize = 8;

        public int ShowFontSize
        {
            get
            {
                return _showFontSize;
            }

            set
            {
                _showFontSize = value;
                this.OnPropertyChanged();

            }
        }
        private string _SettlType_yuan;

        public string SettlType_yuan
        {
            get
            {
                return this._SettlType_yuan;
            }

            set
            {
                if (this._SettlType_yuan != value)
                {
                    this._SettlType_yuan = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        SettlTypeYuanVis = "false";
                    }
                    else 
                    {
                        SettlTypeYuanVis = "true";
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SettlTypeYuanVis;

        public string SettlTypeYuanVis
        {
            get
            {
                return this._SettlTypeYuanVis;
            }

            set
            {
                if (this._SettlTypeYuanVis != value)
                {
                    this._SettlTypeYuanVis = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _SettlTypeT0Vis;

        public string SettlTypeT0Vis
        {
            get
            {
                return this._SettlTypeT0Vis;
            }

            set
            {
                if (this._SettlTypeT0Vis != value)
                {
                    this._SettlTypeT0Vis = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _isOwnInst;

        public string IsOwnInst
        {
            get
            {
                return _isOwnInst;
            }

            set
            {
                _isOwnInst = value;
                this.OnPropertyChanged();

            }
        }
        private string _ownInstTip;

        public string OwnInstTip
        {
            get
            {
                return _ownInstTip;
            }

            set
            {
                _ownInstTip = value;
                this.OnPropertyChanged();

            }
        }

        private string _LastQty;

        public string LastQty
        {
            get
            {
                return _LastQty;
            }

            set
            {
                _LastQty = value;
                this.OnPropertyChanged();

            }
        }
        private string _NetPriceSpread;
        public string NetPriceSpread
        {
            get
            {
                return _NetPriceSpread;
            }

            set
            {
                _NetPriceSpread = value;
                this.OnPropertyChanged();
            }
        }
    }
}
