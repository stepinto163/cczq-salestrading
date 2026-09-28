using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Position.Plugin.Model
{
    /// <summary>
    ///  持仓明细
    /// </summary>
    public class TradeStatisticsData : ViewModelBase
    {
        private int alwaysTop = 0;

        /// <summary>
        /// 置顶
        /// </summary>
        public int AlwaysTop
        {
            get
            {
                return alwaysTop;
            }
            set
            {
                if (alwaysTop != value)
                {
                    alwaysTop = value;
                    this.OnPropertyChanged(nameof(AlwaysTop));
                }
            }
        }

        private string _bookid = string.Empty;
        /// <summary>
        /// 组合ID
        /// </summary>
        public string bookid 
        {
            get
            {
                return _bookid;
            }
            set
            {
                if (_bookid != value)
                {
                    _bookid = value;
                    this.OnPropertyChanged(nameof(bookid));
                }
            }
        }

        private string _bookName = string.Empty;

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname
        {
            get
            {
                return _bookName;
            }
            set
            {
                if (_bookName != value)
                {
                    _bookName = value;
                    this.OnPropertyChanged(nameof(bookname));
                }
            }
        }

        private string _key = string.Empty;
        /// <summary>
        /// key   BookId + "#" + SecurityId
        /// </summary>
        public string key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    this.OnPropertyChanged(nameof(key));
                }
            }
        }

        private string _securityId = string.Empty;
        /// <summary>
        /// 代码
        /// </summary>
        public string securityid
        {
            get
            {
                return _securityId;
            }
            set
            {
                if (_securityId != value)
                {
                    _securityId = value;
                    this.OnPropertyChanged(nameof(securityid));
                }
            }
        }

        private string _lastUpdateTime = string.Empty;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string lastUpdateTime
        {
            get
            {
                return _lastUpdateTime;
            }
            set
            {
                if (_lastUpdateTime != value)
                {
                    _lastUpdateTime = value;
                    this.OnPropertyChanged(nameof(lastUpdateTime));
                }
            }
        }

        private string _symbol = string.Empty;
        /// <summary>
        /// 简称
        /// </summary>
        public string symbol
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
                    this.OnPropertyChanged(nameof(symbol));
                }
            }
        }

        private string _symboltype = string.Empty;

        /// <summary>
        /// 债券类型
        /// </summary>
        public string symboltype
        {
            get
            {
                return _symboltype;
            }
            set
            {
                if (_symboltype != value)
                {
                    _symboltype = value;
                    this.OnPropertyChanged(nameof(symboltype));
                }
            }
        }

        private string _marketindicator = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public string marketindicator
        {
            get
            {
                return _marketindicator;
            }
            set
            {
                if (_marketindicator != value)
                {
                    _marketindicator = value;
                    this.OnPropertyChanged(nameof(marketindicator));
                }
            }
        }

        private decimal _longLot = 0;

        /// <summary>
        /// 买成交量（手）
        /// </summary>
        public decimal longLot
        {
            get
            {
                return _longLot;
            }
            set
            {
                if (_longLot != value)
                {
                    _longLot = value;
                    this.OnPropertyChanged(nameof(longLot));
                }
            }
        }

        private decimal _shortLot = 0;

        /// <summary>
        /// 卖成交量（手）
        /// </summary>
        public decimal shortLot
        {
            get
            {
                return _shortLot;
            }
            set
            {
                if (_shortLot != value)
                {
                    _shortLot = value;
                    this.OnPropertyChanged(nameof(shortLot));
                }
            }
        }

        private decimal _netLot = 0;
        /// <summary>
        /// 净成交量（手）
        /// </summary>
        public decimal netLot
        {
            get
            {
                return _netLot;
            }
            set
            {
                if (_netLot != value)
                {
                    _netLot = value;
                    this.OnPropertyChanged(nameof(netLot));
                }
            }
        }

        private decimal _totalLot = 0;
        /// <summary>
        /// 总成交量（手）
        /// </summary>
        public decimal totalLot
        {
            get
            {
                return _totalLot;
            }
            set
            {
                if (_totalLot != value)
                {
                    _totalLot = value;
                    this.OnPropertyChanged(nameof(totalLot));
                }
            }
        }

        private decimal _longVolume = 0;
        /// <summary>
        /// 买成交量（元）
        /// </summary>
        public decimal longVolume
        {
            get
            {
                return _longVolume;
            }
            set
            {
                if (_longVolume != value)
                {
                    _longVolume = value;
                    this.OnPropertyChanged(nameof(longVolume));
                }
            }
        }
        private decimal _shortVolume = 0;
        /// <summary>
        /// 卖成交量（元）
        /// </summary>
        public decimal shortVolume
        {
            get
            {
                return _shortVolume;
            }
            set
            {
                if (_shortVolume != value)
                {
                    _shortVolume = value;
                    this.OnPropertyChanged(nameof(shortVolume));
                }
            }
        }

        private decimal _netVolume = 0;
        /// <summary>
        /// 净成交量（元）
        /// </summary>
        public decimal netVolume
        {
            get
            {
                return _netVolume;
            }
            set
            {
                if (_netVolume != value)
                {
                    _netVolume = value;
                    this.OnPropertyChanged(nameof(netVolume));
                }
            }
        }

        private decimal _totalVolume = 0;
        /// <summary>
        /// 总成交量（元）
        /// </summary>
        public decimal totalVolume
        {
            get
            {
                return _totalVolume;
            }
            set
            {
                if (_totalVolume != value)
                {
                    _totalVolume = value;
                    this.OnPropertyChanged(nameof(totalVolume));
                }
            }
        }

        private string _longAvgPrice = "0";
        /// <summary>
        /// 成交均价（买）
        /// </summary>
        public string longAvgPrice
        {
            get
            {
                return _longAvgPrice;
            }
            set
            {
                if (_longAvgPrice != value)
                {
                    _longAvgPrice = value;
                    this.OnPropertyChanged(nameof(longAvgPrice));
                }
            }
        }
        private string _shortAvgPrice = "0";
        /// <summary>
        /// 成交均价（卖）
        /// </summary>
        public string shortAvgPrice
        {
            get
            {
                return _shortAvgPrice;
            }
            set
            {
                if (_shortAvgPrice != value)
                {
                    _shortAvgPrice = value;
                    this.OnPropertyChanged(nameof(shortAvgPrice));
                }
            }
        }
        private decimal _totalAmt = 0;
        /// <summary>
        /// 总成交金额（元）
        /// </summary>
        public decimal totalAmt
        {
            get
            {
                return _totalAmt;
            }
            set
            {
                if (_totalAmt != value)
                {
                    _totalAmt = value;
                    this.OnPropertyChanged(nameof(totalAmt));
                }
            }
        }
    }
}
