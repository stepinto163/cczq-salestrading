using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Broker.Plugin.Model
{
    public class TransactionOrdermModel : BaseViewModel
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

        private string _blocknum;

        public string Blocknum
        {
            get { return _blocknum; }
            set
            {
                if (_blocknum != value)
                {

                    _blocknum = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _execID;
        /// <summary>
        /// 执行单号
        /// </summary>
        public string ExecID
        {
            get { return _execID; }
            set
            {
                if (_execID != value)
                {

                    _execID = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _seq;
        /// <summary>
        /// 内码
        /// </summary>
        public string Seq
        {
            get { return _seq; }
            set
            {
                if (_seq != value)
                {

                    _seq = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _orderStatus;
        /// <summary>
        /// 订单状态 O：新意向 E&I&G:确认 F&H：拒绝 C：撤销
        /// </summary>
        public string OrderStatus
        {
            get { return _orderStatus; }
            set
            {
                if (_orderStatus != value)
                {

                    _orderStatus = value;
                    this.OnPropertyChanged();
                }
            }
        }





        private string _intentionId;

        public string IntentionID
        {
            get { return _intentionId; }
            set
            {
                if (_intentionId != value)
                {
                    _intentionId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _quoteid;

        public string QuoteId
        {
            get { return _quoteid; }
            set
            {
                if (_quoteid != value)
                {
                    _quoteid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _securityID;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityID
        {
            get { return _securityID; }
            set
            {

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
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged();
                }

            }
        }


        private string _sideDisplay;
        /// <summary>
        /// 方向展示
        /// </summary>
        public string SideDisplay
        {
            get { return _sideDisplay; }
            set
            {
                if (_sideDisplay != value)
                {
                    _sideDisplay = value;
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

        private string _settleType;

        public string SettleType
        {
            get { return _settleType; }
            set
            {
                if (_settleType != value)
                {
                    _settleType = value;
                    this.OnPropertyChanged();
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
        /// <summary>
        /// 价格
        /// </summary>
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

        private string _price_display;

        public string Price_Display
        {
            get { return _price_display; }
            set
            {
                if (_price_display != value)
                {
                    _price_display = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _netPrice;

        public decimal? NetPrice
        {
            get { return _netPrice; }
            set
            {
                if (_netPrice != value)
                {
                    _netPrice = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _qty;

        public decimal? Qty
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


        private decimal? qtyChange;
        public decimal? QtyChange
        {
            get { return qtyChange; }
            set
            {
                if (qtyChange != value)
                {
                    qtyChange = value;
                    OnPropertyChanged("QtyChange");
                }
            }
        }


        private string _qtyDisplay;
        /// <summary>
        /// 量展示
        /// </summary>
        public string QtyDisplay
        {
            get
            {
                return _qtyDisplay;
            }
            set
            {
                if (_qtyDisplay != value)
                {
                    _qtyDisplay = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _copyInfo;

        public string CopyInfo
        {
            get
            {
                return _copyInfo;
            }
            set
            {
                if (_copyInfo != value)
                {
                    _copyInfo = value;
                }
            }
        }

        private string _partySubId;

        public string PartySubId
        {
            get
            {
                return _partySubId;
            }
            set
            {
                if (_partySubId != value)
                {
                    _partySubId = value;
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


        private string traderAInfoDisplay;
        /// <summary>
        /// 交易员A
        /// </summary>
        public string TraderAInfoDisplay
        {
            get { return traderAInfoDisplay; }
            set
            {
                if (traderAInfoDisplay != value)
                {
                    traderAInfoDisplay = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string traderBInfoDisplay;
        /// <summary>
        /// 交易员b
        /// </summary>
        public string TraderBInfoDisplay
        {
            get { return traderBInfoDisplay; }
            set
            {
                if (traderBInfoDisplay != value)
                {
                    traderBInfoDisplay = value;
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

        private string _createTime;

        public string CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                if (_createTime != value)
                {
                    _createTime = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private DateTime _createTimeOrderBy;

        public DateTime CreateTimeOrderBy
        {
            get
            {
                return _createTimeOrderBy;
            }
            set
            {
                if (_createTimeOrderBy != value)
                {
                    _createTimeOrderBy = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _createTimeDisplay;
        /// <summary>
        /// 展示时间
        /// </summary>
        public string CreateTimeDisplay
        {
            get { return _createTimeDisplay; }
            set
            {
                if (_createTimeDisplay != value)
                {
                    _createTimeDisplay = value;
                    OnPropertyChanged("CreateTimeDisplay");
                }
            }
        }

        private bool _isDealHistDay;
        /// <summary>
        /// 是否历史
        /// </summary>
        public bool IsDealHistDay
        {
            get { return _isDealHistDay; }
            set
            {
                if (_isDealHistDay != value)
                {
                    _isDealHistDay = value;
                    OnPropertyChanged("IsDealHistDay");
                }
            }
        }

        private string _updateTime_Bid;

        public string UpdateTime_InnerBid
        {
            get
            {
                return _updateTime_Bid;
            }
            set
            {
                if (_updateTime_Bid != value)
                {
                    _updateTime_Bid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _updateTime_Ofr;

        public string UpdateTime_InnerOfr
        {
            get
            {
                return _updateTime_Ofr;
            }
            set
            {
                if (_updateTime_Ofr != value)
                {
                    _updateTime_Ofr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _wholeflag;

        public string WholeFlag
        {
            get
            {
                return _wholeflag;
            }
            set
            {
                if (_wholeflag != value)
                {
                    _wholeflag = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债
        /// </summary>
        private decimal? _ccdcvaluation;

        public decimal? CCDCValuation
        {
            get
            {
                return _ccdcvaluation;
            }
            set
            {
                if (_ccdcvaluation != value)
                {
                    _ccdcvaluation = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债估值偏离
        /// </summary>
        private decimal? _ccdcvaluation_bp;

        public decimal? CCDCValuation_BP
        {
            get
            {
                return _ccdcvaluation_bp;
            }
            set
            {
                if (_ccdcvaluation_bp != value)
                {
                    _ccdcvaluation_bp = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 中债日期
        /// </summary>
        private string _ccdcvaluation_date;

        public string CCDCValuation_Date
        {
            get
            {
                return _ccdcvaluation_date;
            }
            set
            {
                if (_ccdcvaluation_date != value)
                {
                    _ccdcvaluation_date = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _showlastworkday;
        /// <summary>
        /// 中债估值日期警告标记 !
        /// </summary>
        public string ShowLastWorkDay
        {
            get { return _showlastworkday; }
            set
            {
                _showlastworkday = value;
                this.OnPropertyChanged();
            }
        }

        private string _client;

        public string Client
        {
            get { return _client; }
            set
            {
                if (_client != value)
                {
                    _client = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _client_InnerBid;

        public string Client_InnerBid
        {
            get { return _client_InnerBid; }
            set
            {
                if (_client_InnerBid != value)
                {
                    _client_InnerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _client_InnerOfr;

        public string Client_InnerOfr
        {
            get { return _client_InnerOfr; }
            set
            {
                if (_client_InnerOfr != value)
                {
                    _client_InnerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _dqType;

        public string DQType
        {
            get { return _dqType; }
            set
            {
                if (_dqType != value)
                {
                    _dqType = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _dqType_InnerBid;

        public string DQType_InnerBid
        {
            get { return _dqType_InnerBid; }
            set
            {
                if (_dqType_InnerBid != value)
                {
                    _dqType_InnerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _dqType_InnerOfr;

        public string DQType_InnerOfr
        {
            get { return _dqType_InnerOfr; }
            set
            {
                if (_dqType_InnerOfr != value)
                {
                    _dqType_InnerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isMe;

        public bool IsMe
        {
            get { return _isMe; }
            set
            {
                if (_isMe != value)
                {
                    _isMe = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isMe_InnerBid;

        public bool IsMe_InnerBid
        {
            get { return _isMe_InnerBid; }
            set
            {
                if (_isMe_InnerBid != value)
                {
                    _isMe_InnerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isMe_InnerOfr;

        public bool IsMe_InnerOfr
        {
            get { return _isMe_InnerOfr; }
            set
            {
                if (_isMe_InnerOfr != value)
                {
                    _isMe_InnerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _best_bid;

        public decimal? Best_Bid
        {
            get
            {
                return _best_bid;
            }
            set
            {
                if (_best_bid != value)
                {
                    _best_bid = value;
                    this.OnPropertyChanged();
                    //BestBid_FC.BeginAnimation(SolidColorBrush.ColorProperty, BID_CA); // 颜色绘制使用动画绘制
                    bestbidFlashCount = 4;
                }
            }
        }

        public int bestbidFlashCount = 0;

        private double _best_Bid_Opacity = 1;

        public double Best_Bid_Opacity
        {
            get
            {
                return _best_Bid_Opacity;
            }
            set
            {
                _best_Bid_Opacity = value;
                this.OnPropertyChanged();
            }
        }

        //private static ColorAnimation BID_CA = new ColorAnimation()
        //{
        //    From = (Color)ColorConverter.ConvertFromString("#4CFB943C"),
        //    To = (Color)ColorConverter.ConvertFromString("#FB943C"),
        //    AutoReverse = false, // 正常播放
        //    RepeatBehavior = new RepeatBehavior(3),
        //    Duration = new Duration(TimeSpan.FromSeconds(1)) // 动画一次所用时间
        //};//动画效果
        //private SolidColorBrush _bestBid_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB943C"));

        //public SolidColorBrush BestBid_FC
        //{
        //    get
        //    {
        //        return _bestBid_FC;
        //    }
        //    set
        //    {
        //        if (_bestBid_FC != value)
        //        {
        //            _bestBid_FC = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        private decimal? _best_ofr;

        public decimal? Best_Ofr
        {
            get
            {
                return _best_ofr;
            }
            set
            {
                if (_best_ofr != value)
                {
                    _best_ofr = value;
                    this.OnPropertyChanged();
                    //BestOfr_FC.BeginAnimation(SolidColorBrush.ColorProperty, OFR_CA); // 颜色绘制使用动画绘制
                    bestofrFlashCount = 4;
                }
            }
        }

        public int bestofrFlashCount = 0;

        private double _best_Ofr_Opacity = 1;

        public double Best_Ofr_Opacity
        {
            get
            {
                return _best_Ofr_Opacity;
            }
            set
            {
                _best_Ofr_Opacity = value;
                this.OnPropertyChanged();
            }
        }


        //private static ColorAnimation OFR_CA = new ColorAnimation()
        //{
        //    From = (Color)ColorConverter.ConvertFromString("#4C58B3FF"),
        //    To = (Color)ColorConverter.ConvertFromString("#58B3FF"),
        //    AutoReverse = false, // 正常播放
        //    RepeatBehavior = new RepeatBehavior(3),
        //    Duration = new Duration(TimeSpan.FromSeconds(1)), // 动画一次所用时间
        //}; //动画效果
        //private SolidColorBrush _bestOfr_FC = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#58B3FF"));

        //public SolidColorBrush BestOfr_FC
        //{
        //    get
        //    {
        //        return _bestOfr_FC;
        //    }
        //    set
        //    {
        //        if (_bestOfr_FC != value)
        //        {
        //            _bestOfr_FC = value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        private string _wholeflag_innerBid;

        public string WholeFlag_InnerBid
        {
            get { return _wholeflag_innerBid; }
            set
            {
                if (_wholeflag_innerBid != value)
                {
                    _wholeflag_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _rolename_innerBid;

        public string RoleName_InnerBid
        {
            get { return _rolename_innerBid; }
            set
            {
                if (_rolename_innerBid != value)
                {
                    _rolename_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _firmflag_innerBid;

        public string FirmFlag_InnerBid
        {
            get { return _firmflag_innerBid; }
            set
            {
                if (_firmflag_innerBid != value)
                {
                    _firmflag_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _settleType_innerBid;

        public string SettleType_InnerBid
        {
            get { return _settleType_innerBid; }
            set
            {
                if (_settleType_innerBid != value)
                {
                    _settleType_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _roleid_innerBid;

        public string RoleId_InnerBid
        {
            get { return _roleid_innerBid; }
            set
            {
                if (_roleid_innerBid != value)
                {
                    _roleid_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _partySubName_innerBid;

        public string PartySubName_InnerBid
        {
            get { return _partySubName_innerBid; }
            set
            {
                if (_partySubName_innerBid != value)
                {
                    _partySubName_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _partySubName_tooltip_innerBid;

        public string PartySubName_Tooltip_InnerBid
        {
            get { return _partySubName_tooltip_innerBid; }
            set
            {
                if (_partySubName_tooltip_innerBid != value)
                {
                    _partySubName_tooltip_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long? _qty_innerBid;

        public long? Qty_InnerBid
        {
            get { return _qty_innerBid; }
            set
            {
                if (_qty_innerBid != value)
                {
                    _qty_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _copyInfo_innerBid;

        public string CopyInfo_InnerBid
        {
            get
            {
                return _copyInfo_innerBid;
            }
            set
            {
                if (_copyInfo_innerBid != value)
                {
                    _copyInfo_innerBid = value;
                }
            }
        }

        private string _qty_tooltip_innerBid;

        public string Qty_Tooltip_InnerBid
        {
            get { return _qty_tooltip_innerBid; }
            set
            {
                if (_qty_tooltip_innerBid != value)
                {
                    _qty_tooltip_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _count_innerBid;

        public int Count_InnerBid
        {
            get { return _count_innerBid; }
            set
            {
                if (_count_innerBid != value)
                {
                    _count_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _price_innerBid;

        public decimal? Price_InnerBid
        {
            get { return _price_innerBid; }
            set
            {
                if (_price_innerBid != value)
                {
                    _price_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _netprice_innerBid;

        public decimal? NetPrice_InnerBid
        {
            get { return _netprice_innerBid; }
            set
            {
                if (_netprice_innerBid != value)
                {
                    _netprice_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _intentionid_innerBid;

        public string IntentionId_InnerBid
        {
            get { return _intentionid_innerBid; }
            set
            {
                if (_intentionid_innerBid != value)
                {
                    _intentionid_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _quoteid_innerBid;

        public string QuoteId_innerBid
        {
            get { return _quoteid_innerBid; }
            set
            {
                if (_quoteid_innerBid != value)
                {
                    _quoteid_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isBest_innerBid = string.Empty;

        public string IsBest_InnerBid
        {
            get { return _isBest_innerBid; }
            set
            {
                if (_isBest_innerBid != value)
                {
                    _isBest_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isMarketBest_innerBid = string.Empty;

        public string IsMarketBest_InnerBid
        {
            get { return _isMarketBest_innerBid; }
            set
            {
                if (_isMarketBest_innerBid != value)
                {
                    _isMarketBest_innerBid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _quoteid_innerOfr;

        public string QuoteId_InnerOfr
        {
            get { return _quoteid_innerOfr; }
            set
            {
                if (_quoteid_innerOfr != value)
                {
                    _quoteid_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _intentionid_innerOfr;

        public string IntentionId_InnerOfr
        {
            get { return _intentionid_innerOfr; }
            set
            {
                if (_intentionid_innerOfr != value)
                {
                    _intentionid_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private decimal? _netprice_innerOfr;

        public decimal? NetPrice_InnerOfr
        {
            get { return _netprice_innerOfr; }
            set
            {
                if (_netprice_innerOfr != value)
                {
                    _netprice_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal? _price_innerOfr;

        public decimal? Price_InnerOfr
        {
            get { return _price_innerOfr; }
            set
            {
                if (_price_innerOfr != value)
                {
                    _price_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _count_innerOfr;

        public int Count_InnerOfr
        {
            get { return _count_innerOfr; }
            set
            {
                if (_count_innerOfr != value)
                {
                    _count_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long? _qty_innerOfr;

        public long? Qty_InnerOfr
        {
            get { return _qty_innerOfr; }
            set
            {
                if (_qty_innerOfr != value)
                {
                    _qty_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _copyInfo_innerOfr;

        public string CopyInfo_InnerOfr
        {
            get
            {
                return _copyInfo_innerOfr;
            }
            set
            {
                if (_copyInfo_innerOfr != value)
                {
                    _copyInfo_innerOfr = value;
                }
            }
        }

        private string _qty_tooltip_innerOfr;

        public string Qty_Tooltip_InnerOfr
        {
            get { return _qty_tooltip_innerOfr; }
            set
            {
                if (_qty_tooltip_innerOfr != value)
                {
                    _qty_tooltip_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _partySubName_innerOfr;

        public string PartySubName_InnerOfr
        {
            get { return _partySubName_innerOfr; }
            set
            {
                if (_partySubName_innerOfr != value)
                {
                    _partySubName_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _partySubName_tooltip_innerOfr;

        public string PartySubName_Tooltip_InnerOfr
        {
            get { return _partySubName_tooltip_innerOfr; }
            set
            {
                if (_partySubName_tooltip_innerOfr != value)
                {
                    _partySubName_tooltip_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _firmflag_innerOfr;

        public string FirmFlag_InnerOfr
        {
            get { return _firmflag_innerOfr; }
            set
            {
                if (_firmflag_innerOfr != value)
                {
                    _firmflag_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _settleType_innerOfr;

        public string SettleType_InnerOfr
        {
            get { return _settleType_innerOfr; }
            set
            {
                if (_settleType_innerOfr != value)
                {
                    _settleType_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _roleid_innerOfr;

        public string RoleId_InnerOfr
        {
            get { return _roleid_innerOfr; }
            set
            {
                if (_roleid_innerOfr != value)
                {
                    _roleid_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _wholeflag_innerOfr;

        public string WholeFlag_InnerOfr
        {
            get { return _wholeflag_innerOfr; }
            set
            {
                if (_wholeflag_innerOfr != value)
                {
                    _wholeflag_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _rolename_innerOfr;

        public string RoleName_InnerOfr
        {
            get { return _rolename_innerOfr; }
            set
            {
                if (_rolename_innerOfr != value)
                {
                    _rolename_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private decimal? _lastMarketPx;

        public decimal? LastMarketPx
        {
            get { return _lastMarketPx; }
            set
            {
                if (_lastMarketPx != value)
                {
                    _lastMarketPx = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isInnerBest = false;

        public bool IsInnerBest
        {
            get
            {
                return _isInnerBest;
            }
            set
            {
                if (_isInnerBest != value)
                {
                    _isInnerBest = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isBest_innerOfr = string.Empty;

        public string IsBest_InnerOfr
        {
            get { return _isBest_innerOfr; }
            set
            {
                if (_isBest_innerOfr != value)
                {
                    _isBest_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _isMarketBest_innerOfr = string.Empty;

        public string IsMarketBest_InnerOfr
        {
            get { return _isMarketBest_innerOfr; }
            set
            {
                if (_isMarketBest_innerOfr != value)
                {
                    _isMarketBest_innerOfr = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private long _sign;

        public long Sign
        {
            get { return _sign; }
            set
            {
                if (_sign != value || _signDic == null)
                {
                    _sign = value;
                    _signDic = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic;

        public Dictionary<string, string> SignDic
        {
            get { return _signDic; }
            set
            {
                if (_signDic != value)
                {
                    _signDic = value;
                }
            }
        }

        private long _sign_InnerBid;

        public long Sign_InnerBid
        {
            get { return _sign_InnerBid; }
            set
            {
                if (_sign_InnerBid != value || _signDic_InnerBid == null)
                {
                    _sign_InnerBid = value;
                    _signDic_InnerBid = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid;

        public Dictionary<string, string> SignDic_InnerBid
        {
            get { return _signDic_InnerBid; }
            set
            {
                if (_signDic_InnerBid != value)
                {
                    _signDic_InnerBid = value;
                }
            }
        }

        private long _sign_InnerOfr;

        public long Sign_InnerOfr
        {
            get { return _sign_InnerOfr; }
            set
            {
                if (_sign_InnerOfr != value || _signDic_InnerOfr == null)
                {
                    _sign_InnerOfr = value;
                    _signDic_InnerOfr = ParseSign("sign", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr;

        public Dictionary<string, string> SignDic_InnerOfr
        {
            get { return _signDic_InnerOfr; }
            set
            {
                if (_signDic_InnerOfr != value)
                {
                    _signDic_InnerOfr = value;
                }
            }
        }

        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, QuoteTag> quoteTagDic = BaseHandler.Instance.GetQuoteTagDic();
            string a = Utils.CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }

        #region sign1
        private long _sign1;

        public long Sign1
        {
            get { return _sign1; }
            set
            {
                if (_sign1 != value || _signDic1 == null)
                {
                    _sign1 = value;
                    _signDic1 = ParseSign("sign1", value);
                }
            }
        }

        private Dictionary<string, string> _signDic1;

        public Dictionary<string, string> SignDic1
        {
            get { return _signDic1; }
            set
            {
                if (_signDic1 != value)
                {
                    _signDic1 = value;
                }
            }
        }

        private long _sign_InnerBid1;

        public long Sign_InnerBid1
        {
            get { return _sign_InnerBid1; }
            set
            {
                if (_sign_InnerBid1 != value || _signDic_InnerBid1 == null)
                {
                    _sign_InnerBid1 = value;
                    _signDic_InnerBid1 = ParseSign("sign1", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid1;

        public Dictionary<string, string> SignDic_InnerBid1
        {
            get { return _signDic_InnerBid1; }
            set
            {
                if (_signDic_InnerBid1 != value)
                {
                    _signDic_InnerBid1 = value;
                }
            }
        }

        private long _sign_InnerOfr1;

        public long Sign_InnerOfr1
        {
            get { return _sign_InnerOfr1; }
            set
            {
                if (_sign_InnerOfr1 != value || _signDic_InnerOfr1 == null)
                {
                    _sign_InnerOfr1 = value;
                    _signDic_InnerOfr1 = ParseSign("sign1", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr1;

        public Dictionary<string, string> SignDic_InnerOfr1
        {
            get { return _signDic_InnerOfr1; }
            set
            {
                if (_signDic_InnerOfr1 != value)
                {
                    _signDic_InnerOfr1 = value;
                }
            }
        }
        #endregion

        #region
        private long _sign2;

        public long Sign2
        {
            get { return _sign2; }
            set
            {
                if (_sign2 != value || _signDic2 == null)
                {
                    _sign2 = value;
                    _signDic2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic2;

        public Dictionary<string, string> SignDic2
        {
            get { return _signDic2; }
            set
            {
                if (_signDic2 != value)
                {
                    _signDic2 = value;
                }
            }
        }

        private long _sign_InnerBid2;

        public long Sign_InnerBid2
        {
            get { return _sign_InnerBid2; }
            set
            {
                if (_sign_InnerBid2 != value || _signDic_InnerBid2 == null)
                {
                    _sign_InnerBid2 = value;
                    _signDic_InnerBid2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid2;

        public Dictionary<string, string> SignDic_InnerBid2
        {
            get { return _signDic_InnerBid2; }
            set
            {
                if (_signDic_InnerBid2 != value)
                {
                    _signDic_InnerBid2 = value;
                }
            }
        }

        private long _sign_InnerOfr2;

        public long Sign_InnerOfr2
        {
            get { return _sign_InnerOfr2; }
            set
            {
                if (_sign_InnerOfr2 != value || _signDic_InnerOfr2 == null)
                {
                    _sign_InnerOfr2 = value;
                    _signDic_InnerOfr2 = ParseSign("sign2", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr2;

        public Dictionary<string, string> SignDic_InnerOfr2
        {
            get { return _signDic_InnerOfr2; }
            set
            {
                if (_signDic_InnerOfr2 != value)
                {
                    _signDic_InnerOfr2 = value;
                }
            }
        }
        #endregion


        #region
        private long _sign3;

        public long Sign3
        {
            get { return _sign3; }
            set
            {
                if (_sign3 != value || _signDic3 == null)
                {
                    _sign3 = value;
                    _signDic3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic3;

        public Dictionary<string, string> SignDic3
        {
            get { return _signDic3; }
            set
            {
                if (_signDic3 != value)
                {
                    _signDic3 = value;
                }
            }
        }

        private long _sign_InnerBid3;

        public long Sign_InnerBid3
        {
            get { return _sign_InnerBid3; }
            set
            {
                if (_sign_InnerBid3 != value || _signDic_InnerBid3 == null)
                {
                    _sign_InnerBid3 = value;
                    _signDic_InnerBid3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerBid3;

        public Dictionary<string, string> SignDic_InnerBid3
        {
            get { return _signDic_InnerBid3; }
            set
            {
                if (_signDic_InnerBid3 != value)
                {
                    _signDic_InnerBid3 = value;
                }
            }
        }

        private long _sign_InnerOfr3;

        public long Sign_InnerOfr3
        {
            get { return _sign_InnerOfr3; }
            set
            {
                if (_sign_InnerOfr3 != value || _signDic_InnerOfr3 == null)
                {
                    _sign_InnerOfr3 = value;
                    _signDic_InnerOfr3 = ParseSign("sign3", value);
                }
            }
        }

        private Dictionary<string, string> _signDic_InnerOfr3;

        public Dictionary<string, string> SignDic_InnerOfr3
        {
            get { return _signDic_InnerOfr3; }
            set
            {
                if (_signDic_InnerOfr3 != value)
                {
                    _signDic_InnerOfr3 = value;
                }
            }
        }
        #endregion

        private string _Rating;

        public string Rating
        {
            get { return _Rating; }
            set
            {
                _Rating = value;
                this.OnPropertyChanged();
            }
        }

        private string _cdc_Implited_Rating;

        public string Cdc_Implited_Rating
        {
            get { return _cdc_Implited_Rating; }
            set
            {
                _cdc_Implited_Rating = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _termDays;
        public decimal? TermDays
        {
            get { return _termDays; }
            set { _termDays = value; this.OnPropertyChanged(); }
        }

        private string _remainingTime_new;
        public string RemainingTime_New
        {
            get { return _remainingTime_new; }
            set { _remainingTime_new = value; this.OnPropertyChanged(); }
        }

        private decimal? _couponRateCurrent;

        public decimal? CouponRateCurrent
        {
            get { return _couponRateCurrent; }
            set { _couponRateCurrent = value; this.OnPropertyChanged(); }
        }


        private string _issuerCode;
        public string IssuerCode
        {
            get { return _issuerCode; }
            set { _issuerCode = value; this.OnPropertyChanged(); }
        }

        private string _issuerName;
        public string IssuerName
        {
            get { return _issuerName; }
            set { _issuerName = value; this.OnPropertyChanged(); }
        }

        private string _warranterCode;

        public string WarranterCode
        {
            get { return _warranterCode; }
            set { _warranterCode = value; this.OnPropertyChanged(); }
        }

        private string _warranterName;

        public string WarranterName
        {
            get { return _warranterName; }
            set { _warranterName = value; this.OnPropertyChanged(); }
        }

        // 修正久期
        private decimal? _modifiedDuration;

        public decimal? ModifiedDuration
        {
            get { return _modifiedDuration; }
            set { _modifiedDuration = value; this.OnPropertyChanged(); }
        }

        // 中债估值
        private decimal? _chinaBondValuation;

        public decimal? ChinaBondValuation
        {
            get { return _chinaBondValuation; }
            set { _chinaBondValuation = value; this.OnPropertyChanged(); }
        }

        // 中债估值(复制)
        private string _chinaBondValuationFull;

        public string ChinaBondValuationFull
        {
            get { return _chinaBondValuationFull; }
            set { _chinaBondValuationFull = value; this.OnPropertyChanged(); }
        }

        // 中债行权
        private decimal? _chinaBondExercise;

        public decimal? ChinaBondExercise
        {
            get { return _chinaBondExercise; }
            set { _chinaBondExercise = value; this.OnPropertyChanged(); }
        }

        // 中债到期
        private decimal? _chinaBondDue;

        public decimal? ChinaBondDue
        {
            get { return _chinaBondDue; }
            set { _chinaBondDue = value; this.OnPropertyChanged(); }
        }

        // 中证估值
        private decimal? _csiBondValuation;

        public decimal? CsiBondValuation
        {
            get { return _csiBondValuation; }
            set { _csiBondValuation = value; this.OnPropertyChanged(); }
        }

        // 中证估值(复制)
        private string _csiBondValuationFull;

        public string CsiBondValuationFull
        {
            get { return _csiBondValuationFull; }
            set { _csiBondValuationFull = value; this.OnPropertyChanged(); }
        }

        // 中证行权
        private decimal? _csiBondValuationExercise;

        public decimal? CsiBondValuationExercise
        {
            get { return _csiBondValuationExercise; }
            set { _csiBondValuationExercise = value; this.OnPropertyChanged(); }
        }

        // 中证到期
        private decimal? _csiBondValuationDue;

        public decimal? CsiBondValuationDue
        {
            get { return _csiBondValuationDue; }
            set { _csiBondValuationDue = value; this.OnPropertyChanged(); }
        }

        //到期日距离下个工作日天数（休n/x）
        private string _dayOffStr;

        public string DayOffStr
        {
            get { return _dayOffStr; }
            set
            {
                _dayOffStr = value;
                this.OnPropertyChanged();
            }
        }

        private bool if_guarantee;

        public bool If_Guarantee
        {
            get { return if_guarantee; }
            set
            {
                if_guarantee = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _issueAmount;

        public decimal? IssueAmount
        {
            get { return _issueAmount; }
            set
            {
                _issueAmount = value;
                this.OnPropertyChanged();
            }
        }


        #region 基础数据
        private string _remainingTime;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private string _maturityDate;

        public string MaturtityDate
        {
            get { return _maturityDate; }
            set { _maturityDate = value; this.OnPropertyChanged(); }
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

        private string _bondSubType;

        public string BondSubType
        {
            get { return _bondSubType; }
            set { _bondSubType = value; }
        }

        private string _Coupon_Rate_Current;

        public string Coupon_Rate_Current
        {
            get
            {
                return _Coupon_Rate_Current;
            }
            set
            {
                _Coupon_Rate_Current = value;
                this.OnPropertyChanged();
            }
        }

        private string _level;

        public string Level
        {
            get { return _level; }
            set { _level = value; this.OnPropertyChanged(); }
        }


        private string _regionType;

        public string RegionType
        {
            get { return _regionType; }
            set { _regionType = value; this.OnPropertyChanged(); }
        }


        private string _enterpriseType;

        public string EnterpriseType
        {
            get { return _enterpriseType; }
            set { _enterpriseType = value; this.OnPropertyChanged(); }
        }


        private string _issueType;

        public string IssueType
        {
            get { return _issueType; }
            set { _issueType = value; this.OnPropertyChanged(); }
        }

        #endregion


        #region 废弃区
        private string text;
        /// <summary>
        /// 原始文本
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; this.OnPropertyChanged(); }
        }

        private string info1;
        /// <summary>
        /// 记忆内容
        /// </summary>
        public string Info1
        {
            get { return info1; }
            set { info1 = value; this.OnPropertyChanged(); }
        }

        private string _institutionid;
        /// <summary>
        /// 机构id
        /// </summary>
        public string InstitutionID
        {
            get
            {
                return _institutionid;
            }
            set
            {
                if (_institutionid != value)
                {
                    _institutionid = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _institutionName;
        /// <summary>
        /// 机构名称
        /// </summary>
        public string InstitutionName
        {
            get
            {
                return _institutionName;
            }
            set
            {
                if (_institutionName != value)
                {
                    _institutionName = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _institutionShowName;
        /// <summary>
        /// 机构展示名称
        /// </summary>
        public string InstitutionShowName
        {
            get
            {
                return _institutionShowName;
            }
            set
            {
                if (_institutionShowName != value)
                {
                    _institutionShowName = value;
                    this.OnPropertyChanged();
                }

            }
        }

        private string _traderid;
        /// <summary>
        /// 交易员id
        /// </summary>
        public string TraderID
        {
            get
            {
                return _traderid;
            }
            set
            {
                if (_traderid != value)
                {
                    _traderid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _counterPartyInstitutionId;
        /// <summary>
        /// 机构b
        /// </summary>
        public string CounterPartyInstitutionId
        {
            get
            {
                return _counterPartyInstitutionId;
            }
            set
            {
                if (_counterPartyInstitutionId != value)
                {
                    _counterPartyInstitutionId = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _counterPartyInstitutionName;
        /// <summary>
        /// 机构b
        /// </summary>
        public string CounterPartyInstitutionName
        {
            get
            {
                return _counterPartyInstitutionName;
            }
            set
            {
                if (_counterPartyInstitutionName != value)
                {
                    _counterPartyInstitutionName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _counterPartyInstitutionShowName;
        /// <summary>
        /// 机构b
        /// </summary>
        public string CounterPartyInstitutionShowName
        {
            get
            {
                return _counterPartyInstitutionShowName;
            }
            set
            {
                if (_counterPartyInstitutionShowName != value)
                {
                    _counterPartyInstitutionShowName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderidB;
        /// <summary>
        /// 交易员idB
        /// </summary>
        public string TraderIDB
        {
            get
            {
                return _traderidB;
            }
            set
            {
                if (_traderidB != value)
                {
                    _traderidB = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _tradername;
        /// <summary>
        /// 客户名
        /// </summary>
        public string TraderName
        {
            get
            {
                return _tradername;
            }
            set
            {
                if (_tradername != value)
                {
                    _tradername = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool isChecked;
        /// <summary>
        /// 选择
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private bool isSelected;
        /// <summary>
        /// 鼠标多选
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 意向报价状态
        /// </summary>
        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                this.OnPropertyChanged();
            }
        }
        private string trader;

        /// <summary>
        /// Trader 他人
        /// </summary>
        public string Trader
        {
            get
            {
                return trader;
            }
            set
            {
                trader = value;
                this.OnPropertyChanged();
            }
        }


        private string closeByName;

        /// <summary>
        /// 操作人（中文）
        /// </summary>
        public string CloseByName
        {
            get
            {
                return closeByName;
            }
            set
            {
                closeByName = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 视图区
        private string _bondCodeShow;
        /// <summary>
        /// 债券显示
        /// </summary>
        public string BondCodeShow
        {
            get { return _bondCodeShow; }
            set
            {
                _bondCodeShow = value;
                this.OnPropertyChanged();
            }
        }
        private Visibility _bondShowVisible = Visibility.Visible;
        /// <summary>
        /// 债券显示框是否显示
        /// </summary>
        public Visibility BondShowVisible
        {
            get { return _bondShowVisible; }
            set
            {
                _bondShowVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _bondShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// 债券显示框是否显示
        /// </summary>
        public Visibility BondShowNoEditVisible
        {
            get { return _bondShowNoEditVisible; }
            set
            {
                _bondShowNoEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _bondEditVisible = Visibility.Collapsed;
        /// <summary>
        /// 债券编辑框是否显示
        /// </summary>
        public Visibility BondEditVisible
        {
            get { return _bondEditVisible; }
            set
            {
                _bondEditVisible = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility _priceShowVisible = Visibility.Visible;
        /// <summary>
        /// 价格显示框是否显示
        /// </summary>
        public Visibility PriceShowVisible
        {
            get { return _priceShowVisible; }
            set
            {
                _priceShowVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _priceShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// 价格显示框是否显示
        /// </summary>
        public Visibility PriceShowNoEditVisible
        {
            get { return _priceShowNoEditVisible; }
            set
            {
                _priceShowNoEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _priceEditVisible = Visibility.Collapsed;
        /// <summary>
        /// 价格编辑框是否显示
        /// </summary>
        public Visibility PriceEditVisible
        {
            get { return _priceEditVisible; }
            set
            {
                _priceEditVisible = value;
                this.OnPropertyChanged();
            }
        }
        private Visibility _qtyShowVisible = Visibility.Visible;
        /// <summary>
        /// 数量显示框是否显示
        /// </summary>
        public Visibility QtyShowVisible
        {
            get { return _qtyShowVisible; }
            set
            {
                _qtyShowVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _qtyShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// 数量显示框是否显示
        /// </summary>
        public Visibility QtyShowNoEditVisible
        {
            get { return _qtyShowNoEditVisible; }
            set
            {
                _qtyShowNoEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _qtyEditVisible = Visibility.Collapsed;
        /// <summary>
        /// 数量编辑框是否显示
        /// </summary>
        public Visibility QtyEditVisible
        {
            get { return _qtyEditVisible; }
            set
            {
                _qtyEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string _brokerIdA;
        /// <summary>
        /// brokerA唯一标识
        /// </summary>
        public string BrokerIdA
        {
            get { return _brokerIdA; }
            set
            {
                if (_brokerIdA != value)
                {
                    _brokerIdA = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _brokerNameA;
        /// <summary>
        /// brokerA名称
        /// </summary>
        public string BrokerNameA
        {
            get { return _brokerNameA; }
            set
            {
                if (_brokerNameA != value)
                {
                    _brokerNameA = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private string _brokerIdB;
        /// <summary>
        /// brokerB唯一标识
        /// </summary>
        public string BrokerIdB
        {
            get { return _brokerIdB; }
            set
            {
                if (_brokerIdB != value)
                {
                    _brokerIdB = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _brokerNameB;
        /// <summary>
        /// brokerB名称
        /// </summary>
        public string BrokerNameB
        {
            get { return _brokerNameB; }
            set
            {
                if (_brokerNameB != value)
                {
                    _brokerNameB = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isInternalQuote;
        /// <summary>
        /// 是否内部报价
        /// </summary>
        public bool IsInternalQuote
        {
            get { return _isInternalQuote; }
            set
            {
                if (_isInternalQuote != value)
                {
                    _isInternalQuote = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _brokerAShowVisible = Visibility.Visible;
        /// <summary>
        /// BrokerA显示框是否显示
        /// </summary>
        public Visibility BrokerAShowVisible
        {
            get { return _brokerAShowVisible; }
            set
            {
                _brokerAShowVisible = value;
                this.OnPropertyChanged("BrokerAShowVisible");
            }
        }

        private Visibility _brokerAShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// BrokerA显示框是否显示
        /// </summary>
        public Visibility BrokerAShowNoEditVisible
        {
            get { return _brokerAShowNoEditVisible; }
            set
            {
                _brokerAShowNoEditVisible = value;
                this.OnPropertyChanged("BrokerAShowNoEditVisible");
            }
        }
        private Visibility _brokerAEditVisible = Visibility.Collapsed;
        /// <summary>
        /// BrokerA编辑框是否显示
        /// </summary>
        public Visibility BrokerAEditVisible
        {
            get { return _brokerAEditVisible; }
            set
            {
                _brokerAEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _brokerBShowVisible = Visibility.Visible;
        /// <summary>
        /// BrokerA显示框是否显示
        /// </summary>
        public Visibility BrokerBShowVisible
        {
            get { return _brokerBShowVisible; }
            set
            {
                _brokerBShowVisible = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility _brokerBShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// BrokerA显示框是否显示
        /// </summary>
        public Visibility BrokerBShowNoEditVisible
        {
            get { return _brokerBShowNoEditVisible; }
            set
            {
                _brokerBShowNoEditVisible = value;
                this.OnPropertyChanged("BrokerBShowNoEditVisible");
            }
        }


        private Visibility _brokerBEditVisible = Visibility.Collapsed;
        /// <summary>
        /// BrokerA编辑框是否显示
        /// </summary>
        public Visibility BrokerBEditVisible
        {
            get { return _brokerBEditVisible; }
            set
            {
                _brokerBEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _traderAShowVisible = Visibility.Visible;
        /// <summary>
        /// TraderA显示框是否显示
        /// </summary>
        public Visibility TraderAShowVisible
        {
            get { return _traderAShowVisible; }
            set
            {
                _traderAShowVisible = value;
                this.OnPropertyChanged("TraderAShowVisible");
            }
        }

        private Visibility _traderAShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// TraderA显示框是否显示
        /// </summary>
        public Visibility TraderAShowNoEditVisible
        {
            get { return _traderAShowNoEditVisible; }
            set
            {
                _traderAShowNoEditVisible = value;
                this.OnPropertyChanged("TraderAShowNoEditVisible");
            }
        }
        private Visibility _traderAEditVisible = Visibility.Collapsed;
        /// <summary>
        /// TraderA编辑框是否显示
        /// </summary>
        public Visibility TraderAEditVisible
        {
            get { return _traderAEditVisible; }
            set
            {
                _traderAEditVisible = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _traderBShowNoEditVisible = Visibility.Visible;
        /// <summary>
        /// TraderA显示框是否显示
        /// </summary>
        public Visibility TraderBShowNoEditVisible
        {
            get { return _traderBShowNoEditVisible; }
            set
            {
                _traderBShowNoEditVisible = value;
                this.OnPropertyChanged("TraderBShowNoEditVisible");
            }
        }

        private Visibility _traderBShowVisible = Visibility.Visible;
        /// <summary>
        /// TraderA显示框是否显示
        /// </summary>
        public Visibility TraderBShowVisible
        {
            get { return _traderBShowVisible; }
            set
            {
                _traderBShowVisible = value;
                this.OnPropertyChanged("TraderBShowVisible");
            }
        }

        private Visibility _traderBEditVisible = Visibility.Collapsed;
        /// <summary>
        /// TraderA编辑框是否显示
        /// </summary>
        public Visibility TraderBEditVisible
        {
            get { return _traderBEditVisible; }
            set
            {
                _traderBEditVisible = value;
                this.OnPropertyChanged();
            }
        }


        private string _traderIdB;
        /// <summary>
        /// brokerA名称
        /// </summary>
        public string TraderIdB
        {
            get { return _traderIdB; }
            set
            {
                if (_traderIdB != value)
                {
                    _traderIdB = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderNameB;
        /// <summary>
        /// TraderNameB
        /// </summary>
        public string TraderNameB
        {
            get { return _traderNameB; }
            set
            {
                if (_traderNameB != value)
                {
                    _traderNameB = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private List<ComboxBaseModel> _comboxBases;
        /// <summary>
        /// 下拉数据源
        /// </summary>
        public List<ComboxBaseModel> ChannelComboxModel
        {
            get
            {
                return _comboxBases;
            }
            set
            {
                if (_comboxBases != value)
                {
                    _comboxBases = value;
                    OnPropertyChanged("ChannelComboxModel");
                }
            }
        }

        private ComboxBaseModel _comboxBaseSelect;
        /// <summary>
        /// 选中渠道
        /// </summary>
        public ComboxBaseModel ComboxBaseSelect
        {
            get
            {
                return _comboxBaseSelect;
            }
            set
            {
                if (_comboxBaseSelect != value)
                {
                    _comboxBaseSelect = value;
                    OnPropertyChanged("ComboxBaseSelect");
                }
            }
        }

        private string issueTraderName;
        /// <summary>
        /// 发单人名称
        /// </summary>
        public string IssueTraderName
        {
            get { return issueTraderName; }
            set
            {
                if (issueTraderName != value)
                {
                    issueTraderName = value;
                    OnPropertyChanged("IssueTraderName");
                }
            }
        }

        private string _fandianDisplay;
        /// <summary>
        /// 返点展示
        /// </summary>
        public string FandianDisplay
        {
            get { return _fandianDisplay; }
            set
            {
                if (_fandianDisplay != value)
                {
                    _fandianDisplay = value;
                    OnPropertyChanged("FandianDisplay");
                }
            }
        }

        private string _fandianFlag;
        /// <summary>
        /// 返点标签 1 数字,2文本
        /// </summary>
        public string FandianFlag
        {
            get { return _fandianFlag; }
            set
            {
                if (_fandianFlag != value)
                {
                    _fandianFlag = value;
                    OnPropertyChanged("FandianFlag");
                }
            }
        }


        private string _fandianTag;
        /// <summary>
        /// 返点标签 true 科编辑,false不可编辑
        /// </summary>
        public string FandianTag
        {
            get { return _fandianTag; }
            set
            {
                if (_fandianTag != value)
                {
                    _fandianTag = value;
                    OnPropertyChanged("FandianTag");
                }
            }
        }

        private Visibility _fanDianShowVisible = Visibility.Visible;
        /// <summary>
        /// 返点显示框是否显示
        /// </summary>
        public Visibility FandianShowVisible
        {
            get { return _fanDianShowVisible; }
            set
            {
                if (_fanDianShowVisible != value)
                {
                    _fanDianShowVisible = value;
                    OnPropertyChanged("FandianShowVisible");
                }
            }
        }

        private Visibility _fanDianShowReadVisible = Visibility.Visible;
        /// <summary>
        /// 返点显示框是否显示
        /// </summary>
        public Visibility FandianShowReadVisible
        {
            get { return _fanDianShowReadVisible; }
            set
            {
                if (_fanDianShowReadVisible != value)
                {
                    _fanDianShowReadVisible = value;
                    OnPropertyChanged("FandianShowReadVisible");
                }
            }
        }

        private Visibility _fanDianEditVisible = Visibility.Collapsed;
        /// <summary>
        /// 返点编辑框是否显示
        /// </summary>
        public Visibility FanDianEditVisible
        {
            get { return _fanDianEditVisible; }
            set
            {
                _fanDianEditVisible = value;
                this.OnPropertyChanged("FanDianEditVisible");
            }
        }

        private string fandian;
        /// <summary>
        /// 返点费用
        /// </summary>
        public string Fandian
        {
            get { return fandian; }
            set
            {
                if (fandian != value)
                {
                    fandian = value;
                    OnPropertyChanged("Fandian");
                }
            }
        }

        private string fandianTxt;
        /// <summary>
        /// 返点费用
        /// </summary>
        public string FandianTxt
        {
            get { return fandianTxt; }
            set
            {
                if (fandianTxt != value)
                {
                    fandianTxt = value;
                    OnPropertyChanged("FandianTxt");
                }
            }
        }

        private string _settleDisplay;
        public string SettleAllDisplay
        {
            get
            {
                return _settleDisplay;
            }
            set
            {
                if (_settleDisplay != value)
                {
                    _settleDisplay = value;
                    OnPropertyChanged("SettleAllDisplay");
                }
            }
        }
        private const string DefaultSettle1 = "+1,明天+0";
        private const string DefaultSettle2 = "明天+0,+1";
        public string SettleDisplay
        {
            get
            {
                if (DefaultSettle2.Equals(settlementType) && settlementType.Contains(DefaultSettle2))
                {
                    return "";
                }
                return TransOrderHelper.Instance.GetSettleDisplay(settlementType, dealdate);
            }
        }

        public string SettleDisplay2
        {
            get
            {
                if (DefaultSettle2.Equals(settlementtype2) && settlementtype2.Contains(DefaultSettle2))
                {
                    return "";
                }
                return TransOrderHelper.Instance.GetSettleDisplay(settlementtype2, dealdate2);
            }
        }

        private string settlementtype2;
        /// <summary>
        /// ofr  settlementtype2是卖方的结算类型
        /// </summary>
        public string Settlementtype2
        {
            get { return settlementtype2; }
            set
            {
                if (settlementtype2 != value)
                {
                    settlementtype2 = value;
                    OnPropertyChanged("Settlementtype2");
                }
            }
        }

        private string settlementType;
        /// <summary>
        /// bid 注意默认结算类型是买方
        /// 结算方式
        /// </summary>
        public string SettlementType
        {
            get { return settlementType; }
            set
            {
                if (settlementType != value)
                {
                    settlementType = value;
                    OnPropertyChanged("SettlementType");
                }
            }
        }

        private string dealdate;
        /// <summary>
        /// BID 交易日
        /// </summary>
        public string Dealdate
        {
            get { return dealdate; }
            set
            {
                if (dealdate != value)
                {
                    dealdate = value;
                    OnPropertyChanged("Dealdate");
                }
            }
        }

        private string dealdate2;
        /// <summary>
        /// OFR 交易日2
        /// </summary>
        public string Dealdate2
        {
            get { return dealdate2; }
            set
            {
                if (dealdate2 != value)
                {
                    dealdate2 = value;
                    OnPropertyChanged("Dealdate2");
                }
            }
        }


        private string remarks;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks
        {
            get { return remarks; }
            set
            {
                if (remarks != value)
                {
                    remarks = value;
                    OnPropertyChanged("Remarks");
                }
            }
        }

        private string settlementdate;
        /// <summary>
        /// BID 结算日
        /// </summary>
        public string Settlementdate
        {
            get { return settlementdate; }
            set
            {
                if (settlementdate != value)
                {
                    settlementdate = value;
                    OnPropertyChanged("Settlementdate");
                }
            }
        }


        private string settlementdate2;
        /// <summary>
        /// OFR 结算日2
        /// </summary>
        public string Settlementdate2
        {
            get { return settlementdate2; }
            set
            {
                if (settlementdate2 != value)
                {
                    settlementdate2 = value;
                    OnPropertyChanged("Settlementdate2");
                }
            }
        }

        /// <summary>
        /// 成交列表使用
        /// 如果是默认结算类型，则不显示
        /// </summary>
        public string SettleDisplayForMain
        {
            get
            {
                string settleStr = settlementType;
                switch (settlementType)
                {
                    case "1":
                        settleStr = "+0";
                        break;
                    case "2":
                        settleStr = "+1";
                        break;
                    case "3":
                        settleStr = "明天+0";
                        break;
                }
                if (DefaultSettle1.Equals(settlementType) && settlementType.Contains(DefaultSettle1))
                {
                    return "";
                }
                return settleStr;
            }
        }
        /// <summary>
        /// 成交列表使用
        /// 如果是默认结算类型，则不显示
        /// </summary>
        public string SettleDisplayForMain2
        {
            get
            {
                string settleStr = settlementtype2;
                switch (settlementtype2)
                {
                    case "1":
                        settleStr = "+0";
                        break;
                    case "2":
                        settleStr = "+1";
                        break;
                    case "3":
                        settleStr = "明天+0";
                        break;
                }

                if (DefaultSettle2.Equals(settlementtype2) && settlementtype2.Contains(DefaultSettle2))
                {
                    return "";
                }
                return settleStr;
            }
        }


        private string bridgeType;//第一座桥的桥类型：默认方式:bid,ofr共有桥:B,O bid的桥:B offer的桥:O
        public string BridgeType
        {
            get { return bridgeType; }
            set
            {
                if (bridgeType != value)
                {
                    bridgeType = value;
                    OnPropertyChanged("BridgeType");
                }
            }
        }


        /// <summary>
        /// 双桥错期选择，默认是Ofr ；且只有订单结算类型是错期且添加双桥，此字段才有意义。
        /// O：Ofr方错期
        /// B：Bid方错期
        /// </summary>
        private string _doubleBridgeSettleSide;
        public string DoubleBridgeSettleSide
        {
            get { return _doubleBridgeSettleSide; }
            set
            {
                if (_doubleBridgeSettleSide != value)
                {
                    _doubleBridgeSettleSide = value;
                    OnPropertyChanged("DoubleBridgeSettleSide");
                }
            }
        }



        private string _onlineA;
        /// <summary>
        /// 1-线下
        /// 2-线上
        /// 3-线下录入
        /// </summary>
        public string OnlineA
        {
            get { return _onlineA; }
            set
            {
                if (_onlineA != value)
                {
                    _onlineA = value;
                    OnPropertyChanged("OnlineA");
                }
            }
        }

        private string _onlineB;
        /// <summary>
        /// 1-线下
        /// 2-线上
        /// 3-线下录入
        /// </summary>
        public string OnlineB
        {
            get { return _onlineB; }
            set
            {
                if (_onlineB != value)
                {
                    _onlineB = value;
                    OnPropertyChanged("OnlineB");
                }
            }
        }

        private bool isSelect = false;
        /// <summary>
        /// 订单是否选中
        /// </summary>
        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                if (isSelect != value)
                {
                    isSelect = value;
                    OnPropertyChanged("IsSelect");
                }
            }
        }

        private string _bridgeFlag;
        /// <summary>
        /// 是否加桥标记
        /// </summary>
        public string BridgeFlag
        {
            get { return _bridgeFlag; }
            set
            {
                if (_bridgeFlag != value)
                {
                    _bridgeFlag = value;
                    OnPropertyChanged("BridgeFlag");
                }
            }
        }


        private Visibility _bridgeSendVisible = Visibility.Collapsed;
        /// <summary>
        /// 桥发给是否显示
        /// </summary>
        public Visibility BridgeSendVisible
        {
            get { return _bridgeSendVisible; }
            set
            {
                _bridgeSendVisible = value;
                this.OnPropertyChanged("BridgeSendVisible");
            }
        }

        private Visibility _sendStandVisible = Visibility.Collapsed;
        /// <summary>
        /// 发给标准化是否显示
        /// </summary>
        public Visibility SendStandVisible
        {
            get { return _sendStandVisible; }
            set
            {
                _sendStandVisible = value;
                this.OnPropertyChanged("SendStandVisible");
            }
        }

        public bool IsPartlyConfirm
        {
            get
            {
                return !string.IsNullOrWhiteSpace(BrokerBConfirmQty)
                    && !string.IsNullOrWhiteSpace(MatchQTY)
                    && BrokerBConfirmQty != MatchQTY;
            }
        }


        private string _brokerBConfirmQty;
        /// <summary>
        /// Broker确认量
        /// </summary>
        public string BrokerBConfirmQty
        {
            get { return _brokerBConfirmQty; }
            set
            {
                if (_brokerBConfirmQty != value)
                {
                    _brokerBConfirmQty = value;
                    OnPropertyChanged("BrokerBConfirmQty");
                }
            }
        }

        private string _matchQTY;
        /// <summary>
        /// 报价量
        /// </summary>
        public string MatchQTY
        {
            get { return _matchQTY; }
            set
            {
                if (_matchQTY != value)
                {
                    _matchQTY = value;
                    OnPropertyChanged("MatchQTY");
                }
            }
        }


        private string _brokerAStatus;
        /// <summary>
        /// BrokerA状态 :0可操作确认 1待确认，2部分确认，3拒绝，4全部确认
        /// </summary>
        public string BrokerAStatus
        {
            get { return _brokerAStatus; }
            set
            {
                if (_brokerAStatus != value)
                {
                    _brokerAStatus = value;
                    OnPropertyChanged("BrokerAStatus");
                }
            }
        }

        private string _brokerBStatus;
        /// <summary>
        /// BrokerB状态
        /// </summary>
        public string BrokerBStatus
        {
            get { return _brokerBStatus; }
            set
            {
                if (_brokerBStatus != value)
                {
                    _brokerBStatus = value;
                    OnPropertyChanged("BrokerBStatus");
                }
            }
        }


        private Visibility isRefreshOrderShow = Visibility.Collapsed;
        /// <summary>
        /// 是否刷新
        /// </summary>
        public Visibility IsRefreshOrderShow
        {
            get
            {
                return isRefreshOrderShow;
            }
            set
            {
                if (isRefreshOrderShow != value)
                {
                    isRefreshOrderShow = value;
                    OnPropertyChanged("IsRefreshOrderShow");
                }
            }
        }


        private bool isRefreshOrderEnable = false;
        /// <summary>
        /// 是否刷新
        /// </summary>
        public bool IsRefreshOrderEnable
        {
            get
            {
                return isRefreshOrderEnable;
            }
            set
            {
                if (isRefreshOrderEnable != value)
                {
                    isRefreshOrderEnable = value;
                    OnPropertyChanged("IsRefreshOrderEnable");
                }
            }
        }

        private string _bidSendtoDisplay;
        /// <summary>
        /// bid方向桥发给
        /// </summary>
        public string BidSendtoDisplay
        {
            get { return _bidSendtoDisplay; }
            set
            {
                if (_bidSendtoDisplay != value)
                {
                    _bidSendtoDisplay = value;
                    OnPropertyChanged("BidSendtoDisplay");
                }
            }
        }


        private string _ofrSendtoDisplay;
        /// <summary>
        /// Ofr方向桥发给
        /// </summary>
        public string OfrSendtoDisplay
        {
            get { return _ofrSendtoDisplay; }
            set
            {
                if (_ofrSendtoDisplay != value)
                {
                    _ofrSendtoDisplay = value;
                    OnPropertyChanged("OfrSendtoDisplay");
                }
            }
        }


        private Visibility _lookButtonVisible = Visibility.Collapsed;
        /// <summary>
        /// 查看按钮是否显示
        /// </summary>
        public Visibility LookButtonVisible
        {
            get { return _lookButtonVisible; }
            set
            {
                if (_lookButtonVisible != value)
                {
                    _lookButtonVisible = value;
                    OnPropertyChanged("LookButtonVisible");
                }
            }
        }

        private string _bridgeCloseById;
        /// <summary>
        /// 关闭者IDB唯一标识
        /// </summary>
        public string BridgeCloseById
        {
            get { return _bridgeCloseById; }
            set
            {
                if (_bridgeCloseById != value)
                {
                    _bridgeCloseById = value;
                    OnPropertyChanged("BridgeCloseById");
                }
            }
        }

        private string _lookButtonStatus;
        /// <summary>
        /// 查看按钮状态 1可操作性，2不可操作
        /// </summary>
        public string LookButtonStatus
        {
            get { return _lookButtonStatus; }
            set
            {
                if (_lookButtonStatus != value)
                {
                    _lookButtonStatus = value;
                    OnPropertyChanged("LookButtonStatus");
                }
            }
        }

        private string _brokerAFree;
        /// <summary>
        /// BrokerageA是否免佣
        /// Y：免佣 N：不免佣
        /// </summary>
        public string BrokerAFree
        {
            get { return _brokerAFree; }
            set
            {
                if (_brokerAFree != value)
                {
                    _brokerAFree = value;
                    OnPropertyChanged("BrokerAFree");
                }
            }
        }

        private string _brokerBFree;
        /// <summary>
        /// BrokerageB是否免佣
        /// Y：免佣 N：不免佣
        /// </summary>
        public string BrokerBFree
        {
            get { return _brokerBFree; }
            set
            {
                if (_brokerBFree != value)
                {
                    _brokerBFree = value;
                    OnPropertyChanged("BrokerBFree");
                }
            }
        }


        private Visibility _customBtnVisibility;
        /// <summary>
        /// BrokerB标识显示
        /// </summary>
        public Visibility CustomBtnVisibility
        {
            get
            {
                return _customBtnVisibility;
            }
            set
            {
                if (_customBtnVisibility != value)
                {
                    _customBtnVisibility = value;
                    OnPropertyChanged("CustomBtnVisibility");
                }
            }
        }

        private bool _isPayFeeEnable = false;
        /// <summary>
        /// 代付费是否启用
        /// </summary>
        public bool IsPayFeeEnable
        {
            get { return _isPayFeeEnable; }
            set
            {
                if (_isPayFeeEnable != value)
                {
                    _isPayFeeEnable = value;
                    OnPropertyChanged("IsPayFeeEnable");
                }
            }
        }

        private bool _customBtnIsEnable;
        /// <summary>
        /// 发给标准环是否展示
        /// </summary>
        public bool CustomBtnIsEnable
        {
            get
            {
                return _customBtnIsEnable;
            }
            set
            {
                if (_customBtnIsEnable != value)
                {
                    _customBtnIsEnable = value;
                    OnPropertyChanged("CustomBtnIsEnable");
                }
            }
        }

        private bool _sendBtnIsEnable;
        /// <summary>
        /// 发给标准化是否可用
        /// </summary>
        public bool SendBtnIsEnable
        {
            get
            {
                return _sendBtnIsEnable;
            }
            set
            {
                if (_sendBtnIsEnable != value)
                {
                    _sendBtnIsEnable = value;
                    OnPropertyChanged("SendBtnIsEnable");
                }
            }
        }

        private string issueState;
        /// <summary>
        /// 发给标准化标识
        /// </summary>
        public string IssueState
        {
            get { return issueState; }
            set
            {
                if (issueState != value)
                {
                    issueState = value;
                    OnPropertyChanged("IssueState");
                }
            }
        }


        private string _issueDir;
        /// <summary>
        /// 发给方向 1：ofr发bid  2：bid发ofr
        /// </summary>
        public string IssueDir
        {
            get { return _issueDir; }
            set
            {
                if (_issueDir != value)
                {
                    _issueDir = value;
                    OnPropertyChanged("IssueDir");
                }
            }
        }

        private string _issueChannel;
        /// <summary>
        /// 发给渠道
        /// </summary>
        public string IssueChannel
        {
            get { return _issueChannel; }
            set
            {
                if (_issueChannel != value)
                {
                    _issueChannel = value;
                    OnPropertyChanged("IssueChannel");
                }
            }
        }
        private string _issueInstName;
        /// <summary>
        /// 发单机构名称
        /// </summary>
        public string IssueInstName
        {
            get { return _issueInstName; }
            set
            {
                if (_issueInstName != value)
                {
                    _issueInstName = value;
                    OnPropertyChanged("IssueInstName");
                }
            }
        }

        private string issueInstNameShort;
        /// <summary>
        /// 发单机构台子简称
        /// </summary>
        public string IssueInstNameShort
        {
            get { return issueInstNameShort; }
            set
            {
                if (issueInstNameShort != value)
                {
                    issueInstNameShort = value;
                    OnPropertyChanged("IssueInstNameShort");
                }
            }
        }
        private string _issueTraderId;
        /// <summary>
        /// 发单人id
        /// </summary>
        public string IssueTraderId
        {
            get { return _issueTraderId; }
            set
            {
                if (_issueTraderId != value)
                {
                    _issueTraderId = value;
                    OnPropertyChanged("IssueTraderId");
                }
            }
        }

        private string _issueRemark;
        /// <summary>
        /// 发给备注
        /// </summary>
        public string IssueRemark
        {
            get { return _issueRemark; }
            set
            {
                if (_issueRemark != value)
                {
                    _issueRemark = value;
                    OnPropertyChanged("IssueRemark");
                }
            }
        }

        private string _issueEditRemark;
        /// <summary>
        /// 发给备注
        /// </summary>
        public string IssueEditRemark
        {
            get { return _issueEditRemark; }
            set
            {
                if (_issueEditRemark != value)
                {
                    _issueEditRemark = value;
                    OnPropertyChanged("IssueEditRemark");
                }
            }
        }
        //private string _issueTraderName;
        ///// <summary>
        ///// 发单人名称
        ///// </summary>
        //public string IssueTraderName
        //{
        //    get { return _issueTraderName; }
        //    set
        //    {
        //        if (_issueTraderName != value)
        //        {
        //            _issueTraderName = value;
        //            OnPropertyChanged("IssueTraderName");
        //        }
        //    }
        //}

        private string _issueTraderShowNameDisplay;
        /// <summary>
        /// 发单人名称显示
        /// </summary>
        public string IssueTraderShowNameDisplay
        {
            get { return _issueTraderShowNameDisplay; }
            set
            {
                if (_issueTraderShowNameDisplay != value)
                {
                    _issueTraderShowNameDisplay = value;
                    OnPropertyChanged("IssueTraderShowNameDisplay");
                }
            }
        }

        private string _issueProxyInstId;
        /// <summary>
        /// 代付费机构id
        /// </summary>
        public string IssueProxyInstId
        {
            get { return _issueProxyInstId; }
            set
            {
                if (_issueProxyInstId != value)
                {
                    _issueProxyInstId = value;
                    OnPropertyChanged("IssueProxyInstId");
                }
            }
        }

        private string _issueProxyFee;
        /// <summary>
        /// 代付费用
        /// </summary>
        public string IssueProxyFee
        {
            get { return _issueProxyFee; }
            set
            {
                if (_issueProxyFee != value)
                {
                    _issueProxyFee = value;
                    OnPropertyChanged("IssueProxyFee");
                }
            }
        }
        private string _issueProxyFlag;
        /// <summary>
        /// 代付费标记0：未开启代付费  1：开启代付费
        /// </summary>
        public string IssueProxyFlag
        {
            get { return _issueProxyFlag; }
            set
            {
                if (_issueProxyFlag != value)
                {
                    _issueProxyFlag = value;
                    OnPropertyChanged("IssueProxyFlag");
                }
            }
        }

        private Visibility _brokerASelfFlag;
        /// <summary>
        /// BrokerA标识显示
        /// </summary>
        public Visibility BrokerASelfFlag
        {
            get
            {
                return _brokerASelfFlag;
            }
            set
            {
                if (_brokerASelfFlag != value)
                {
                    _brokerASelfFlag = value;
                    OnPropertyChanged("BrokerASelfFlag");
                }
            }
        }

        private Visibility _brokerBSelfFlag;
        /// <summary>
        /// BrokerB标识显示
        /// </summary>
        public Visibility BrokerBSelfFlag
        {
            get
            {
                return _brokerBSelfFlag;
            }
            set
            {
                if (_brokerBSelfFlag != value)
                {
                    _brokerBSelfFlag = value;
                    OnPropertyChanged("BrokerBSelfFlag");
                }
            }
        }

        private string _issueShowType;
        /// <summary>
        /// 交易员A唯一标识
        /// </summary>
        public string IssueShowType
        {
            get { return _issueShowType; }
            set
            {
                if (_issueShowType != value)
                {
                    _issueShowType = value;
                    OnPropertyChanged("IssueShowType");
                }
            }
        }


        private string dateExchangeShow;
        /// <summary>
        /// 互换交易员日期展示
        /// </summary>
        public string DateExchangeShow
        {
            get { return dateExchangeShow; }
            set
            {
                if (dateExchangeShow != value)
                {
                    dateExchangeShow = value;
                    OnPropertyChanged("DateExchangeShow");
                }
            }
        }

        private string traderExchangeShow;
        /// <summary>
        ///  互换交易员展示
        /// </summary>
        public string TraderExchangeShow
        {
            get { return traderExchangeShow; }
            set
            {
                if (traderExchangeShow != value)
                {
                    traderExchangeShow = value;
                    OnPropertyChanged("TraderExchangeShow");
                }
            }
        }

        private string traderExchangeID;
        /// <summary>
        ///  互换交易员展示
        /// </summary>
        public string TraderExchangeID
        {
            get { return traderExchangeID; }
            set
            {
                if (traderExchangeID != value)
                {
                    traderExchangeID = value;
                    OnPropertyChanged("TraderExchangeID");
                }
            }
        }

        /// <summary>
        /// 标准化内容是否显示
        /// </summary>
        private Visibility standSendtoValVisible = Visibility.Collapsed;
        public Visibility StandSendtoValVisible
        {
            get { return standSendtoValVisible; }
            set
            {
                if (standSendtoValVisible != value)
                {
                    standSendtoValVisible = value;
                    OnPropertyChanged("StandSendtoValVisible");
                }
            }
        }


        private Visibility payInstChangeVisible = Visibility.Collapsed;
        /// <summary>
        /// 代付机构修改框
        /// </summary>
        public Visibility PayInstChangeVisible
        {
            get { return payInstChangeVisible; }
            set
            {
                if (payInstChangeVisible != value)
                {
                    payInstChangeVisible = value;
                    OnPropertyChanged("PayInstChangeVisible");
                }
            }
        }

        private Visibility payInstTextVisible = Visibility.Visible;
        /// <summary>
        /// 代付机构文本
        /// </summary>
        public Visibility PayInstTextVisible
        {
            get { return payInstTextVisible; }
            set
            {
                if (payInstTextVisible != value)
                {
                    payInstTextVisible = value;
                    OnPropertyChanged("PayInstTextVisible");
                }
            }
        }
        private Visibility sendRightVisible = Visibility.Collapsed;
        /// <summary>
        /// 方向箭头
        /// </summary>
        public Visibility SendRightVisible
        {
            get { return sendRightVisible; }
            set
            {
                if (sendRightVisible != value)
                {
                    sendRightVisible = value; OnPropertyChanged("SendRightVisible");
                }
            }
        }

        private Visibility sendLeftVisible = Visibility.Collapsed;
        /// <summary>
        /// 方向箭头
        /// </summary>
        public Visibility SendLeftVisible
        {
            get { return sendLeftVisible; }
            set
            {
                if (sendLeftVisible != value)
                {
                    sendLeftVisible = value;
                    OnPropertyChanged("SendLeftVisible");
                }
            }
        }

        private Visibility channelVisible;

        public Visibility ChannelVisible
        {
            get { return channelVisible; }
            set
            {
                if (channelVisible != value)
                {
                    channelVisible = value;
                    OnPropertyChanged("ChannelVisible");
                }
            }
        }

        private Visibility orderDirVisibility;
        /// <summary>
        /// Broker标识显示
        /// </summary>
        public Visibility OrderDirVisibility
        {
            get
            {
                return orderDirVisibility;
            }
            set
            {
                if (orderDirVisibility != value)
                {
                    orderDirVisibility = value;
                    OnPropertyChanged("OrderDirVisibility");
                }
            }
        }


        private Visibility _bidSendtoShowVisibility;
        /// <summary>
        /// Broker标识显示
        /// </summary>
        public Visibility BidSendtoShowVisibility
        {
            get
            {
                return _bidSendtoShowVisibility;
            }
            set
            {
                if (_bidSendtoShowVisibility != value)
                {
                    _bidSendtoShowVisibility = value;
                    OnPropertyChanged("BidSendtoShowVisibility");
                }
            }
        }


        private Visibility _bidSendtoEditVisibility;
        /// <summary>
        /// Broker标识显示
        /// </summary>
        public Visibility BidSendtoEditVisibility
        {
            get
            {
                return _bidSendtoEditVisibility;
            }
            set
            {
                if (_bidSendtoEditVisibility != value)
                {
                    _bidSendtoEditVisibility = value;
                    OnPropertyChanged("BidSendtoEditVisibility");
                }
            }
        }


        private Visibility _ofrSendtoShowVisibility;
        /// <summary>
        /// Broker标识显示
        /// </summary>
        public Visibility OfrSendtoShowVisibility
        {
            get
            {
                return _ofrSendtoShowVisibility;
            }
            set
            {
                if (_ofrSendtoShowVisibility != value)
                {
                    _ofrSendtoShowVisibility = value;
                    OnPropertyChanged("OfrSendtoShowVisibility");
                }
            }
        }

        private Visibility _ofrSendtoEditVisibility;
        /// <summary>
        /// Broker标识显示
        /// </summary>
        public Visibility OfrSendtoEditVisibility
        {
            get
            {
                return _ofrSendtoEditVisibility;
            }
            set
            {
                if (_ofrSendtoEditVisibility != value)
                {
                    _ofrSendtoEditVisibility = value;
                    OnPropertyChanged("OfrSendtoEditVisibility");
                }
            }
        }


        private string issueDesc;
        /// <summary>
        /// 发给动作描述
        /// </summary>
        public string IssueDesc
        {
            get { return issueDesc; }
            set
            {
                if (issueDesc != value)
                {
                    issueDesc = value;
                    OnPropertyChanged("IssueDesc");
                }
            }
        }

        private bool isMouseover = false;
        /// <summary>
        /// 鼠标悬浮
        /// </summary>
        public bool IsMouseover
        {
            get { return isMouseover; }
            set
            {
                if (isMouseover != value)
                {
                    isMouseover = value;
                    OnPropertyChanged("IsMouseover");
                }
            }
        }


        private bool isPartConfirm = false;
        /// <summary>
        /// 部分成交标签
        /// </summary>
        public bool IsPartConfirm
        {
            get { return isPartConfirm; }
            set
            {
                if (isPartConfirm != value)
                {
                    isPartConfirm = value;
                    OnPropertyChanged("IsPartConfirm");
                }
            }
        }


        private string associatedFlag;
        /// <summary>
        /// 订单关联机构标志
        /// </summary>
        public string AssociatedFlag
        {
            get { return associatedFlag; }
            set
            {
                if (associatedFlag != value)
                {
                    associatedFlag = value;
                    OnPropertyChanged("AssociatedFlag");
                }
            }
        }


        private string sendToBackground;
        /// <summary>
        /// 发给输入框的颜色
        /// </summary>
        public string SendToBackground
        {
            get { return sendToBackground; }
            set
            {
                if (sendToBackground != value)
                {
                    sendToBackground = value;
                    OnPropertyChanged("SendToBackground");
                }
            }
        }

        private bool _neeSendMsgForB = false;
        /// <summary>
        /// brokerB需要点击发送按钮
        /// </summary>
        public bool NeeSendMsgForB
        {
            get { return _neeSendMsgForB; }
            set
            {
                if (_neeSendMsgForB != value)
                {
                    _neeSendMsgForB = value;
                    OnPropertyChanged("NeeSendMsgForB");
                }
            }
        }

        private bool _bridgeEnable;
        /// <summary>
        /// 桥是否可用
        /// </summary>
        public bool BridgeEnable
        {
            get { return _bridgeEnable; }
            set
            {
                if (_bridgeEnable != value)
                {
                    _bridgeEnable = value;
                    OnPropertyChanged("BridgeEnable");
                }
            }
        }


        private bool _brokerFeeEnable;
        /// <summary>
        /// 免用是否可用
        /// </summary>
        public bool BrokerFeeEnable
        {
            get { return _brokerFeeEnable; }
            set
            {
                if (_brokerFeeEnable != value)
                {
                    _brokerFeeEnable = value;
                    OnPropertyChanged("BrokerFeeEnable");
                }
            }
        }

        private bool _canUpdateOrder;
        /// <summary>
        /// 当前用户是否可以修改订单
        /// </summary>
        public bool CanUpdateOrder
        {
            get { return _canUpdateOrder; }
            set
            {
                if (_canUpdateOrder != value)
                {
                    _canUpdateOrder = value;
                    OnPropertyChanged("CanUpdateOrder");
                }
            }
        }

        private bool _canUpdateRemark;
        /// <summary>
        /// 当前用户是否可以修改备注
        /// </summary>
        public bool CanUpdateRemark
        {
            get { return _canUpdateRemark; }
            set
            {
                if (_canUpdateRemark != value)
                {
                    _canUpdateRemark = value;
                    OnPropertyChanged("CanUpdateRemark");
                }
            }
        }


        private Visibility _remarkEditVisibility = Visibility.Collapsed;
        /// <summary>
        /// 备注编辑
        /// </summary>
        public Visibility RemarkEditVisibility
        {
            get
            {
                return _remarkEditVisibility;
            }
            set
            {
                if (_remarkEditVisibility != value)
                {
                    _remarkEditVisibility = value;
                    OnPropertyChanged("RemarkEditVisibility");
                }
            }
        }

        private Visibility _remarkDisplayVisibility = Visibility.Visible;
        /// <summary>
        /// 备注展示
        /// </summary>
        public Visibility RemarkDisplayVisibility
        {
            get
            {
                return _remarkDisplayVisibility;
            }
            set
            {
                if (_remarkDisplayVisibility != value)
                {
                    _remarkDisplayVisibility = value;
                    OnPropertyChanged("RemarkDisplayVisibility");
                }
            }
        }

        private Visibility _remarkDisplayNoEditVisibility = Visibility.Visible;
        /// <summary>
        /// 备注展示
        /// </summary>
        public Visibility RemarkDisplayNoEditVisibility
        {
            get
            {
                return _remarkDisplayNoEditVisibility;
            }
            set
            {
                if (_remarkDisplayNoEditVisibility != value)
                {
                    _remarkDisplayNoEditVisibility = value;
                    OnPropertyChanged("RemarkDisplayNoEditVisibility");
                }
            }
        }


        private Visibility _remarkWaterVisibility = Visibility.Visible;
        /// <summary>
        /// 备注水印
        /// </summary>
        public Visibility RemarkWaterVisibility
        {
            get
            {
                return _remarkWaterVisibility;
            }
            set
            {
                if (_remarkWaterVisibility != value)
                {
                    _remarkWaterVisibility = value;
                    OnPropertyChanged("RemarkWaterVisibility");
                }
            }
        }


        private Visibility _remarkContentVisibility = Visibility.Visible;
        /// <summary>
        /// 备注内容
        /// </summary>
        public Visibility RemarkContentVisibility
        {
            get
            {
                return _remarkContentVisibility;
            }
            set
            {
                if (_remarkContentVisibility != value)
                {
                    _remarkContentVisibility = value;
                    OnPropertyChanged("RemarkContentVisibility");
                }
            }
        }

        private string timeToMaturity;
        /// <summary>
        /// 剩余期限
        /// </summary>
        public string TimeToMaturity
        {
            get
            {
                return timeToMaturity;
            }
            set
            {
                timeToMaturity = value;
                this.OnPropertyChanged();
            }
        }

        public string BrokerUnread { get; set; }
        public string BidSendto { get; set; }
        public string OfrSendto { get; set; }
        /// <summary>
        /// 发单机构id
        /// </summary>
        public string IssueInstId { get; set; }


        /// <summary>
        /// 发给渠道
        /// </summary>
        public ChannelEnum IssueChannelEnum
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IssueChannel))
                {
                    return (ChannelEnum)System.Enum.Parse(typeof(ChannelEnum), IssueChannel, true);
                }
                return ChannelEnum.Request;
            }

        }

        public bool IsGVN
        {
            get
            {
                return "2".Equals(Side);
            }
        }

        /// <summary>
        /// traderB发给方向显示
        /// </summary>
        public string TraderBSendSideDisplay
        {
            get
            {
                if (IsGVN)
                {
                    return "BID";
                }
                else
                {
                    return "OFR";
                }
            }
        }

        /// <summary>
        /// traderA发给方向显示
        /// </summary>
        public string TraderASendSideDisplay
        {
            get
            {
                if (IsGVN)
                {
                    return "OFR";
                }
                else
                {
                    return "BID";
                }
            }
        }

        #endregion
    }
}
