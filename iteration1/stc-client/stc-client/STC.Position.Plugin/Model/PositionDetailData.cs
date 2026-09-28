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
    public class PositionDetailData : ViewModelBase
    {
        public int _recid = 0;
        /// <summary>
        /// 组合ID
        /// </summary>
        public int recid
        {
            get
            {
                return _recid;
            }
            set
            {
                if (_recid != value)
                {
                    _recid = value;
                    this.OnPropertyChanged(nameof(recid));
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

        private string _bookname = string.Empty;

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname
        {
            get
            {
                return _bookname;
            }
            set
            {
                if (_bookname != value)
                {
                    _bookname = value;
                    this.OnPropertyChanged(nameof(bookname));
                }
            }
        }



        private string _key = string.Empty;
        /// <summary>
        /// key   BookId + "#" + Securityid
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


        private string _securityid = string.Empty;
        /// <summary>
        /// 代码
        /// </summary>
        public string securityid
        {
            get
            {
                return _securityid;
            }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.OnPropertyChanged(nameof(securityid));
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

        private decimal _netavgprice = 0;

        /// <summary>
        /// 均价(净)
        /// </summary>
        public decimal netavgprice
        {
            get
            {
                return _netavgprice;
            }
            set
            {
                if (_netavgprice != value)
                {
                    _netavgprice = value;
                    this.OnPropertyChanged(nameof(netavgprice));
                }
            }
        }

        private decimal _netavgyield = 0;

        /// <summary>
        /// 均价(%) 收益率
        /// </summary>
        public decimal netavgyield
        {
            get
            {
                return _netavgyield;
            }
            set
            {
                if (_netavgyield != value)
                {
                    _netavgyield = value;
                    this.OnPropertyChanged(nameof(netavgyield));
                }
            }
        }

        private string _netpricesource = string.Empty;
        /// <summary>
        /// 净价来源 净价来源 1-中债  2-成交
        /// </summary>
        public string netpricesource
        {
            get
            {
                return _netpricesource;
            }
            set
            {
                if (_netpricesource != value)
                {
                    _netpricesource = value;
                    this.OnPropertyChanged(nameof(netpricesource));
                }
            }
        }

        private decimal _longposition = 0;
        /// <summary>
        /// 多头持仓
        /// </summary>
        public decimal longposition
        {
            get
            {
                return _longposition;
            }
            set
            {
                if (_longposition != value)
                {
                    _longposition = value;
                    this.OnPropertyChanged(nameof(longposition));
                }
            }
        }


        private decimal _longpriceTotal = 0;
        /// <summary>
        /// 多头总价
        /// </summary>
        public decimal longpriceTotal
        {
            get
            {
                return _longpriceTotal;
            }
            set
            {
                if (_longpriceTotal != value)
                {
                    _longpriceTotal = value;
                    this.OnPropertyChanged(nameof(longpriceTotal));
                }
            }
        }


        private decimal _longavgprice = 0;
        /// <summary>
        /// 多头均价
        /// </summary>
        public decimal longavgprice
        {
            get
            {
                return _longavgprice;
            }
            set
            {
                if (_longavgprice != value)
                {
                    _longavgprice = value;
                    this.OnPropertyChanged(nameof(longavgprice));
                }
            }
        }


        private decimal _shortposition = 0;
        /// <summary>
        /// 空头持仓
        /// </summary>
        public decimal shortposition
        {
            get
            {
                return _shortposition;
            }
            set
            {
                if (_shortposition != value)
                {
                    _shortposition = value;
                    this.OnPropertyChanged(nameof(shortposition));
                }
            }
        }

        private decimal _shortpricetotal = 0;
        /// <summary>
        /// 空头总价
        /// </summary>
        public decimal shortpricetotal
        {
            get
            {
                return _shortpricetotal;
            }
            set
            {
                if (_shortpricetotal != value)
                {
                    _shortpricetotal = value;
                    this.OnPropertyChanged(nameof(shortpricetotal));
                }
            }
        }

        private decimal _shortavgprice = 0;
        /// <summary>
        /// 空头均价
        /// </summary>
        public decimal shortavgprice
        {
            get
            {
                return _shortavgprice;
            }
            set
            {
                if (_shortavgprice != value)
                {
                    _shortavgprice = value;
                    this.OnPropertyChanged(nameof(shortavgprice));
                }
            }
        }


        private decimal _netposition = 0;
        /// <summary>
        /// 持仓
        /// </summary>
        public decimal netposition
        {
            get
            {
                return _netposition;
            }
            set
            {
                if (_netposition != value)
                {
                    _netposition = value;
                    this.OnPropertyChanged(nameof(netposition));
                }
            }
        }


        private decimal _totalprofitandloss = 0;
        /// <summary>
        /// 总盈亏（结转计算）
        /// </summary>
        public decimal totalprofitandloss
        {
            get
            {
                return _totalprofitandloss;
            }
            set
            {
                if (_totalprofitandloss != value)
                {
                    _totalprofitandloss = value;
                    this.OnPropertyChanged(nameof(totalprofitandloss));
                }
            }
        }


        private decimal _ondayprofitandloss = 0;
        /// <summary>
        /// 当日盈亏
        /// </summary>
        public decimal ondayprofitandloss
        {
            get
            {
                return _ondayprofitandloss;
            }
            set
            {
                if (_ondayprofitandloss != value)
                {
                    _ondayprofitandloss = value;
                    this.OnPropertyChanged(nameof(ondayprofitandloss));
                }
            }
        }


        private decimal _closingprofitandloss = 0;
        /// <summary>
        /// 平仓盈亏（结转计算）
        /// </summary>
        public decimal closingprofitandloss
        {
            get
            {
                return _closingprofitandloss;
            }
            set
            {
                if (_closingprofitandloss != value)
                {
                    _closingprofitandloss = value;
                    this.OnPropertyChanged(nameof(closingprofitandloss));
                }
            }
        }


        private decimal _positionprofitandloss = 0;
        /// <summary>
        /// 持仓盈亏（结转计算,需要前台更新）
        /// </summary>
        public decimal positionprofitandloss
        {
            get
            {
                return _positionprofitandloss;
            }
            set
            {
                if (_positionprofitandloss != value)
                {
                    _positionprofitandloss = value;
                    this.OnPropertyChanged(nameof(positionprofitandloss));
                }
            }
        }


        private decimal _lastnetprice = 0;

        /// <summary>
        /// 最新价(净)
        /// </summary>
        public decimal lastnetprice
        {
            get
            {
                return _lastnetprice;
            }
            set
            {
                if (_lastnetprice != value)
                {
                    _lastnetprice = value;
                    this.OnPropertyChanged(nameof(lastnetprice));
                }
            }
        }

        private decimal _lastprice = 0;
        /// <summary>
        /// 最新价(%) 收益率
        /// </summary>
        public decimal lastprice
        {
            get
            {
                return _lastprice;
            }
            set
            {
                if (_lastprice != value)
                {
                    _lastprice = value;
                    this.OnPropertyChanged(nameof(lastprice));
                }
            }
        }


        private decimal _positionmarket = 0;
        /// <summary>
        /// 持仓市值 positionmarket
        /// </summary>
        public decimal positionmarket
        {
            get
            {
                return _positionmarket;
            }
            set
            {
                if (_positionmarket != value)
                {
                    _positionmarket = value;
                    this.OnPropertyChanged(nameof(positionmarket));
                }
            }
        }


        private string _createtime = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime
        {
            get
            {
                return _createtime;
            }
            set
            {
                if (_createtime != value)
                {
                    _createtime = value;
                    this.OnPropertyChanged(nameof(createtime));
                }
            }
        }



        private string _updatetime = string.Empty;
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime
        {
            get
            {
                return _updatetime;
            }
            set
            {
                if (_updatetime != value)
                {
                    _updatetime = value;
                    this.OnPropertyChanged(nameof(updatetime));
                }
            }
        }

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


        private string _side = string.Empty;
        /// <summary>
        /// 方向 >0多 <0空
        /// </summary>
        public string side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged(nameof(side));
                }
            }
        }

        private decimal _todaylongposition = 0;
        /// <summary>
        /// 今持仓量(多)
        /// </summary>
        public decimal todaylongposition
        {
            get
            {
                return _todaylongposition;
            }
            set
            {
                if (_todaylongposition != value)
                {
                    _todaylongposition = value;
                    this.OnPropertyChanged(nameof(todaylongposition));
                }
            }
        }

        private decimal _todayshortposition = 0;
        /// <summary>
        /// 今持仓量(空)
        /// </summary>
        public decimal todayshortposition
        {
            get
            {
                return _todayshortposition;
            }
            set
            {
                if (_todayshortposition != value)
                {
                    _todayshortposition = value;
                    this.OnPropertyChanged(nameof(todayshortposition));
                }
            }
        }

        private decimal _ytdlongposition = 0;
        /// <summary>
        /// 昨持仓量(多)
        /// </summary>
        public decimal ytdlongposition
        {
            get
            {
                return _ytdlongposition;
            }
            set
            {
                if (_ytdlongposition != value)
                {
                    _ytdlongposition = value;
                    this.OnPropertyChanged(nameof(ytdlongposition));
                }
            }
        }

        private decimal _ytdshortposition = 0;
        /// <summary>
        /// 昨持仓量(空)
        /// </summary>
        public decimal ytdshortposition
        {
            get
            {
                return _ytdshortposition;
            }
            set
            {
                if (_ytdshortposition != value)
                {
                    _ytdshortposition = value;
                    this.OnPropertyChanged(nameof(ytdshortposition));
                }
            }
        }

        private decimal _ytdsettlementprice = 0;
        /// <summary>
        /// 昨日结算价
        /// </summary>
        public decimal ytdsettlementprice
        {
            get
            {
                return _ytdsettlementprice;
            }
            set
            {
                if (_ytdsettlementprice != value)
                {
                    _ytdsettlementprice = value;
                    this.OnPropertyChanged(nameof(ytdsettlementprice));
                }
            }
        }

        private decimal _positionmarketvalue = 0;
        /// <summary>
        /// 持仓市值
        /// </summary>
        public decimal positionmarketvalue
        {
            get
            {
                return _positionmarketvalue;
            }
            set
            {
                if (_positionmarketvalue != value)
                {
                    _positionmarketvalue = value;
                    this.OnPropertyChanged(nameof(positionmarketvalue));
                }
            }
        }

        private string _bondcategory = string.Empty;

        /// <summary>
        /// 债券子类型
        /// </summary>
        public string bondcategory
        {
            get
            {
                return _bondcategory;
            }
            set
            {
                if (_bondcategory != value)
                {
                    _bondcategory = value;
                    this.OnPropertyChanged(nameof(bondcategory));
                }
            }
        }

        #region 新增字段
        private decimal _dv01;
        public decimal dv01
        {
            get
            {
                return _dv01;
            }
            set
            {
                if (_dv01 != value)
                {
                    _dv01 = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _dailyNetPosition;
        /// <summary>
        /// 今日净持仓  <今日净持仓>=<今日多仓>-<今日空仓>  可以为负值，负值表示持空仓。
        /// </summary>
        public decimal dailyNetPosition
        {
            get
            {
                return _dailyNetPosition;
            }
            set
            {
                if (_dailyNetPosition != value)
                {
                    _dailyNetPosition = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal _initialPosition;
        /// <summary>
        /// 日初头寸 initialPosition = initialPosition + dailyNetPosition
        /// </summary>
        public decimal initialPosition
        {
            get
            {
                return _initialPosition;
            }
            set
            {
                if (_initialPosition != value)
                {
                    _initialPosition = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _settlDate;
        /// <summary>
        /// 起息日 = 交割日
        /// </summary>
        public string settlDate
        {
            get
            {
                return _settlDate;
            }
            set
            {
                if (_settlDate != value)
                {
                    _settlDate = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _settlType;
        /// <summary>
        /// 清算速度
        /// </summary>
        public string settlType
        {
            get
            {
                return _settlType;
            }
            set
            {
                if (_settlType != value)
                {
                    _settlType = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal _availablePosition;
        /// <summary>
        /// 可用持仓
        /// </summary>
        public decimal availablePosition
        {
            get
            {
                return _availablePosition;
            }
            set
            {
                if (_availablePosition != value)
                {
                    _availablePosition = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal _cumAvaliablePosition;
        /// <summary>
        /// 累计可用持仓
        /// </summary>
        public decimal cumAvaliablePosition
        {
            get
            {
                return _cumAvaliablePosition;
            }
            set
            {
                if (_cumAvaliablePosition != value)
                {
                    _cumAvaliablePosition = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal _frozenAmount;
        /// <summary>
        /// 冻结额度
        /// </summary>
        public decimal frozenAmount
        {
            get
            {
                return _frozenAmount;
            }
            set
            {
                if (_frozenAmount != value)
                {
                    _frozenAmount = value;
                    this.OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 删除标记  "1" 删除
        /// </summary>

        private string _delflag = string.Empty;
        /// <summary>
        /// 清算速度
        /// </summary>
        public string delflag
        {
            get
            {
                return _delflag;
            }
            set
            {
                if (_delflag != value)
                {
                    _delflag = value;
                    this.OnPropertyChanged();
                }
            }
        }



        private string _traderdate;
        /// <summary>
        /// 持仓日期
        /// </summary>
        public string traderdate
        {
            get
            {
                return _traderdate;
            }
            set
            {
                if (_traderdate != value)
                {
                    _traderdate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal _netprice = 0;
        /// <summary>
        /// 结算价
        /// </summary>
        public decimal netprice
        {
            get
            {
                return _netprice;
            }
            set
            {
                if (_netprice != value)
                {
                    _netprice = value;
                    this.OnPropertyChanged(nameof(netprice));
                }
            }
        }

        #endregion
    }
}
