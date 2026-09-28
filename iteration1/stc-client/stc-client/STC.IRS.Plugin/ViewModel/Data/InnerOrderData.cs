using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model;
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

namespace STC.IRS.Plugin.ViewModel.Data
{
    public class InnerOrderData : BaseViewModel
    {
        //private bool _isNeedFlash = false;

        //public bool IsNeedFlash
        //{
        //    get { return _isNeedFlash; }
        //    set
        //    {
        //        if (_isNeedFlash != value)
        //        {
        //            _isNeedFlash = value;
        //            this.OnPropertyChanged();
        //            //_isNeedFlash = false;
        //        }
        //    }
        //}

        private int _flashCount = 0;

        public int FlashCount
        {
            get
            {
                return _flashCount;
            }
            set
            {
                _flashCount = value;
                this.OnPropertyChanged();
            }
        }

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

        public string StrategyOrderId
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

        public string SecurityID
        {
            get { return _securityID; }
            set
            {

                if (_securityID != value)
                {
                    _securityID = value;

                    int index = value.IndexOf("_");
                    if (!string.IsNullOrEmpty(value) && index >= 0)
                    {
                        if (SecurityID.StartsWith("S3M/R07"))
                        {
                            this.Type = "Basis";
                        }
                        else if (SecurityID.Contains("*"))
                        {
                            this.Type = "Spread";
                        }
                        else
                        {
                            this.Type = "Outright";
                        }


                        this.Term = value.Substring(index + 1);

                        int posIndex = this.Term.IndexOf("*");
                        string termStr = this.Term;
                        if (posIndex > -1)
                        {
                            termStr = this.Term.Substring(0, posIndex);
                        }
                       

                        this.Term_Convert = Utils.CommonUtils.ConvertRemainingTime(termStr);
                        this.Product = value.Substring(0, index);
                        //string trem
                        string x = $"000000000000000000{GetIrsTerm(this.Term)}{GetIrsProduct(this.Product, this.Type)}{GetIrsType(this.Type)}00000000000000000000000000000000";

                        this.Sign_InnerBid3 = this.Sign_InnerOfr3 = Utils.CommonUtils.ToLong(x);
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        public string GetIrsType(string irsType)
        {
            if ("Outright" == irsType)
            {
                return "0001";
            }
            else if ("Spread" == irsType)
            {
                return "0010";
            }
            else if ("Basis" == irsType)
            {
                return "0011";
            }
            else
            {
                return "0000";
            }
        }

        public string GetIrsProduct(string irsProduct, string type)
        {
            string key = $"{type}_{irsProduct}";
            switch(key)
            {
                case "Basis_S3M/R07":
                    return "001110";
                case "Outright_FR007":
                    return "000001";
                case "Outright_Shibor3M":
                    return "000010";
                case "Outright_LPR1Y":
                    return "000011";
                case "Outright_LPR5Y":
                    return "000100";
                case "Outright_FDR007":
                    return "000101";
                case "Outright_GB10":
                    return "000110";
                case "Outright_CDB10":
                    return "000111";
                case "Outright_D10/G10":
                    return "001000";
                case "Outright_AAA3/D3":
                    return "001001";
                case "Outright_ShiborO/N":
                    return "001010";
                case "Outright_FDR001":
                    return "001011";
                case "Spread_FR007":
                    return "001100";
                case "Spread_Shibor3M":
                    return "001101";
            }
            return "000000";
        }

        public string GetIrsTerm(string term)
        {
            string termStr = term;
            int index = term.IndexOf("*");
            if (index > 0)
            {
                termStr = term.Substring(0, index);
            }
            switch (termStr)
            {
                case "1M":
                    return "0001";
                case "3M":
                    return "0010";
                case "6M":
                    return "0011";
                case "9M":
                    return "0100";
                case "1Y":
                    return "0101";
                case "2Y":
                    return "0110";
                case "3Y":
                    return "0111";
                case "4Y":
                    return "1000";
                case "5Y":
                    return "1001";
                case "7Y":
                    return "1010";
                case "10Y":
                    return "1011";
                
            }
            return "0000";

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

        private string _settleType;

        public string SettleType
        {
            get { return _settleType; }
            set {
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
            get { return _netPrice;  }
            set
            {
                if (_netPrice != value)
                {
                    _netPrice = value;
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

        private string _createTime_Bid;

        public string UpdateTime_InnerBid
        {
            get
            {
                return _createTime_Bid;
            }
            set
            {
                if (_createTime_Bid != value)
                {
                    _createTime_Bid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _createTime_Ofr;

        public string UpdateTime_InnerOfr
        {
            get
            {
                return _createTime_Ofr;
            }
            set
            {
                if (_createTime_Ofr != value)
                {
                    _createTime_Ofr = value;
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

        private static ColorAnimation BID_CA = new ColorAnimation()
        {
            From = (Color)ColorConverter.ConvertFromString("#4CFB943C"),
            To = (Color)ColorConverter.ConvertFromString("#FB943C"),
            AutoReverse = false, // 正常播放
            RepeatBehavior = new RepeatBehavior(3),
            Duration = new Duration(TimeSpan.FromSeconds(1)) // 动画一次所用时间
        };//动画效果
        private SolidColorBrush _bestBid_FC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB943C"));

        public SolidColorBrush BestBid_FC
        {
            get
            {
                return _bestBid_FC;
            }
            set
            {
                if (_bestBid_FC != value)
                {
                    _bestBid_FC = value;
                    this.OnPropertyChanged();
                }
            }
        }

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


        private static ColorAnimation OFR_CA = new ColorAnimation()
        {
            From = (Color)ColorConverter.ConvertFromString("#4C58B3FF"),
            To = (Color)ColorConverter.ConvertFromString("#58B3FF"),
            AutoReverse = false, // 正常播放
            RepeatBehavior = new RepeatBehavior(3),
            Duration = new Duration(TimeSpan.FromSeconds(1)), // 动画一次所用时间
        }; //动画效果
        private SolidColorBrush _bestOfr_FC = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#58B3FF"));

        public SolidColorBrush BestOfr_FC
        {
            get
            {
                return _bestOfr_FC;
            }
            set
            {
                if (_bestOfr_FC != value)
                {
                    _bestOfr_FC = value;
                    this.OnPropertyChanged();
                }
            }
        }

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
                if(_partySubName_innerBid != value)
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

        public string StrategyOrderId_innerBid
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

        public string StrategyOrderId_InnerOfr
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
                if (_sign != value)
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
                if (_sign_InnerBid != value)
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
                if (_sign_InnerOfr != value)
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
                if (_sign1 != value)
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
                if (_sign_InnerBid1 != value)
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
                if (_sign_InnerOfr1 != value)
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
                if (_sign2 != value)
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
                if (_sign_InnerBid2 != value)
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
                if (_sign_InnerOfr2 != value)
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
                if (_sign3 != value)
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
                if (_sign_InnerBid3 != value)
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
                if (_sign_InnerOfr3 != value)
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
            set {
                if_guarantee = value;
                this.OnPropertyChanged();
            }
        }

        //品种
        private string _product;

        public string Product
        {
            get { return _product; }
            set
            {
                _product = value;
                this.OnPropertyChanged();
            }
        }

        //类型
        private string _type;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.OnPropertyChanged();
            }
        }

        //期限
        private string _term;

        public string Term
        {
            get { return _term; }
            set
            {
                _term = value;

                if (_term.StartsWith("1Y") || _term.StartsWith("5Y"))
                {
                    Is1YOr5Y = "1";
                }
                else
                {
                    Is1YOr5Y = "0";
                }

                this.OnPropertyChanged();
            }
        }

        //期限转换
        private decimal _term_convert;

        public decimal Term_Convert
        {
            get { return _term_convert; }
            set
            {
                _term_convert = value;
                this.OnPropertyChanged();
            }
        }

        private string _is1YOr5Y;

        public string Is1YOr5Y
        {
            get { return _is1YOr5Y; }
            set {
                _is1YOr5Y = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isSpreadOrBasis = false;

        public bool IsSpreadOrBasis
        {
            get { return _isSpreadOrBasis; }
            set { _isSpreadOrBasis = value;
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


        private string _institutionname;
        /// <summary>
        /// 客户机构
        /// </summary>
        public string InstitutionName
        {
            get
            {
                return _institutionname;
            }
            set
            {
                if (_institutionname != value)
                {
                    _institutionname = value;
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


        private string vipcode;
        /// <summary>
        /// VIP："0001"（勾选）、"0010"(不勾选）
        /// </summary>
        public string vipCode
        {
            get
            {
                return vipcode;
            }
            set
            {
                vipcode = value;
                this.OnPropertyChanged();
            }
        }


        private string orgtype;
        /// <summary>
        /// 机构类型:"0001"（银行配置户）、"0010"（券商）、"0011"（基金）、"0100"（资管）、"0101"（保险）、"0110"（其他）、"0111"（银行交易户）、"1000"（理财子公司）、"1001"（期货机构）、"1010"（信托）
        /// </summary>
        public string orgType
        {
            get
            {
                return orgtype;
            }
            set
            {
                orgtype = value;
                this.OnPropertyChanged();
            }
        }


        private string ifsubbond;
        /// <summary>
        /// 订阅该券同向/反向/全选："BID/OFR"(同向/反向）,"ALL"（全选）
        /// </summary>
        public string ifSubBond
        {
            get
            {
                return ifsubbond;
            }
            set
            {
                ifsubbond = value;
                this.OnPropertyChanged();
            }
        }

        private string validuntiltime;

        /// <summary>
        /// 有效期
        /// </summary>
        public string validUntilTime
        {
            get
            {
                return validuntiltime;
            }
            set
            {
                validuntiltime = value;
                this.OnPropertyChanged();
            }
        }


        private string reftype;

        /// <summary>
        /// 撤销类型："1"人工，"2"成交，"3"自动
        /// </summary>
        public string RefType
        {
            get
            {
                return reftype;
            }
            set
            {
                reftype = value;
                this.OnPropertyChanged();
            }
        }
        #endregion
    }
}
