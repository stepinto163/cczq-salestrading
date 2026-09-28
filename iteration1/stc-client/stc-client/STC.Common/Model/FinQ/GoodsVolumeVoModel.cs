using Ats.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.Model.FinQ
{
    public class GoodsVolumeVoModel : BaseViewModel
    {
        /// <summary>
        /// 方向
        /// 0 offer
        /// 1 bid
        /// </summary>
        /// 
        public bool _isbid = false;//public int type = 0;

        public bool IsBid
        {
            get
            {
                return _isbid;
            }
            set
            {
                _isbid = value;
                this.OnPropertyChanged();
            }
        }


        private bool _onlyShowSpot = true;
        /// <summary>
        /// 只show即期报价
        /// 是否只展示即期
        /// </summary>
        public bool onlyShowSpot
        {
            get
            {
                return _onlyShowSpot;
            }
            set
            {
                _onlyShowSpot = value;
                this.OnPropertyChanged();
            }
        }


        private string _bargainFlag;
        /// <summary>
        /// 议价标识 0:无星; 1: 1星; 2: 2星
        /// </summary>
        public string bargainFlag
        {
            get
            {
                return _bargainFlag;
            }
            set
            {
                _bargainFlag = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _bargainFlagVisibility1 = Visibility.Collapsed;
        /// <summary>
        /// 议价标识 0:无星; 1: 1星; 2: 2星
        /// </summary>
        public Visibility BargainFlagVisibility1
        {
            get
            {
                return _bargainFlagVisibility1;
            }
            set
            {
                _bargainFlagVisibility1 = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _bargainFlagVisibility2 = Visibility.Collapsed;
        /// <summary>
        /// 议价标识 0:无星; 1: 1星; 2: 2星
        /// </summary>
        public Visibility BargainFlagVisibility2
        {
            get
            {
                return _bargainFlagVisibility2;
            }
            set
            {
                _bargainFlagVisibility2 = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 议价标识复制展示
        /// </summary>
        public string BargainFlagShow
        {
            get
            {
                string show = string.Empty;
                if (bargainFlag == "1")
                {
                    show = "*";
                }
                else if (bargainFlag == "2")
                {
                    show = "**";
                }
                else
                {

                }

                return show;
            }
        }


        private bool _exchangesignFlag;

        /// <summary>
        /// 换券标识 true: 换券; false 不换券
        /// </summary>
        public bool ExchangeSignFlag
        {
            get
            {
                return _exchangesignFlag;
            }
            set
            {
                _exchangesignFlag = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _exchangesignFlagVisibility = Visibility.Collapsed;

        /// <summary>
        /// 换券标识 true: 换券; false 不换券
        /// </summary>
        public Visibility ExchangeSignFlagVisibility
        {
            get
            {
                return _exchangesignFlagVisibility;
            }
            set
            {
                _exchangesignFlagVisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 返费
        /// </summary>
        private string _fandian;

        public string FanDian
        {
            get
            {
                return _fandian;
            }
            set
            {
                _fandian = value;
                this.OnPropertyChanged();
            }
        }

        private string _FanDianFlag;

        public string FanDianFlag
        {
            get
            {
                return _FanDianFlag;
            }
            set
            {
                _FanDianFlag = value;
                this.OnPropertyChanged();
            }
        }



        /// <summary>
        /// 返费复制展示
        /// </summary>
        public string FanDianShow { get { return string.IsNullOrEmpty(FanDian) ? string.Empty : $"F{FanDian}"; } }


        /// <summary>
        /// 换券标识复制展示
        /// </summary>
        public string ExchangeSignFlagShow
        {
            get
            {
                return ExchangeSignFlag ? "换券" : string.Empty;
            }
        }

        /// <summary>
        /// oco flag 复制展示
        /// </summary>
        public string OcoFlagShow
        {
            get
            {
                string show = string.Empty;
                if (ocoFlag == "1")
                {
                    show = "oco";
                }
                else if (ocoFlag == "2")
                {
                    show = "打包";
                }
                else
                {

                }
                return show;
            }
        }

        private string _brokerID;

        /// <summary>
        /// 经纪商ID
        /// Broker唯一标识
        /// </summary>
        public string brokerID
        {
            get
            {
                return _brokerID;
            }
            set
            {
                _brokerID = value;

                IsMe = brokerID == LocalEnv.UserInfo.UserId;
                this.OnPropertyChanged();
            }
        }

        private string _brokerName;
        /// <summary>
        /// 经纪商名称
        /// </summary>
        public string brokerName
        {
            get
            {
                return _brokerName;
            }
            set
            {
                _brokerName = value;
                //_showbrokerName = _isMe ? $"{value}" : value;
                _showbrokerName = value;
                this.OnPropertyChanged();
            }
        }

        private string _showbrokerName;
        /// <summary>
        /// 经纪商名称,含有 (我) 的名字
        /// </summary>
        public string ShowBrokerName
        {
            get
            {
                return _showbrokerName;
            }
            set
            {
                _showbrokerName = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isMe = false;
        public bool IsMe
        {
            get
            {
                _isMe = brokerID == LocalEnv.UserInfo.UserId;
                return _isMe;
            }
            set
            {
                _isMe = value;
                this.OnPropertyChanged();
            }
        }


        private string _description;
        /// <summary>
        /// 描述
        /// </summary>
        public string description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                this.OnPropertyChanged();
            }
        }


        private string _description2;
        /// <summary>
        /// 描述2
        /// </summary>
        public string description2
        {
            get
            {
                return _description2;
            }
            set
            {
                _description2 = value;
                this.OnPropertyChanged();
            }
        }

        private string _id;
        /// <summary>
        /// 报价唯一标识
        /// </summary>
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }


        private string _institutionID;

        /// <summary>
        /// 机构ID
        /// </summary>
        public string institutionID
        {
            get
            {
                return _institutionID;
            }
            set
            {
                _institutionID = value;
                this.OnPropertyChanged();
            }
        }


        private string _institutionName;

        /// <summary>
        /// 机构名称
        /// </summary>
        public string institutionName
        {
            get
            {
                return _institutionName;
            }
            set
            {
                _institutionName = value;
                this.OnPropertyChanged();
            }
        }

        private string _ocoFlag;
        /// <summary>
        /// OCO标识
        /// </summary>
        public string ocoFlag
        {
            get
            {
                return _ocoFlag;
            }
            set
            {
                _ocoFlag = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ocoFlagVisibility1 = Visibility.Collapsed;
        /// <summary>
        /// OCO标识
        /// </summary>
        public Visibility OcoFlagVisibility1
        {
            get
            {
                return _ocoFlagVisibility1;
            }
            set
            {
                _ocoFlagVisibility1 = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ocoFlagVisibility2 = Visibility.Collapsed;
        /// <summary>
        /// OCO标识
        /// </summary>
        public Visibility OcoFlagVisibility2
        {
            get
            {
                return _ocoFlagVisibility2;
            }
            set
            {
                _ocoFlagVisibility2 = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _qty;

        /// <summary>
        /// 数量
        /// </summary>
        public decimal qty
        {
            get
            {
                return _qty;
            }
            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }

        private string _qtyDisplay;

        /// <summary>
        /// 数量
        /// </summary>
        public string QtyDisplay
        {
            get
            {
                return _qtyDisplay;
            }
            set
            {
                _qtyDisplay = value;
                this.OnPropertyChanged();
            }
        }

        private string[] _settlementType;
        /// <summary>
        /// 结算方式 数组
        /// </summary>
        public string[] settlementType
        {
            get
            {
                return _settlementType;
            }
            set
            {
                _settlementType = value;
                this.OnPropertyChanged();
            }
        }


        private string _time;
        /// <summary>
        /// 时间
        /// </summary>
        public string time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                this.OnPropertyChanged();
            }
        }


        private string _traderID;
        /// <summary>
        /// 交易员ID
        /// </summary>
        public string traderID
        {
            get
            {
                return _traderID;
            }
            set
            {
                _traderID = value;
                this.OnPropertyChanged();
            }
        }


        private string _traderName;
        /// <summary>
        /// 交易员名字
        /// </summary>
        public string traderName
        {
            get
            {
                return _traderName;
            }
            set
            {
                _traderName = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlag;

        /// <summary>
        /// 整量标志
        /// </summary>
        public string wholeFlag
        {
            get
            {
                return _wholeFlag;
            }
            set
            {
                _wholeFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _wholeFlagDisPlay;

        /// <summary>
        /// 整量标志显示
        /// </summary>
        public string WholeFlagDisPlay
        {
            get
            {
                return _wholeFlagDisPlay;
            }
            set
            {
                if (_wholeFlagDisPlay != value)
                {
                    _wholeFlagDisPlay = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _quoteInternalFlag;

        /// <summary>
        /// 内部报价标识
        /// 2：内部报价，1：正常报价
        /// </summary>
        public string quoteInternalFlag
        {
            get
            {
                return _quoteInternalFlag;
            }
            set
            {
                _quoteInternalFlag = value;
                this.OnPropertyChanged();
            }
        }

        private string _bondKeyListedMarket;

        /// <summary>
        /// 市场标识
        /// </summary>
        public string bondKeyListedMarket
        {
            get
            {
                return _bondKeyListedMarket;
            }
            set
            {
                _bondKeyListedMarket = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isCheck;
        /// <summary>
        /// 混排
        /// </summary>
        public bool isCheck
        {
            get
            {
                return _isCheck;
            }
            set
            {
                _isCheck = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isBestPrice;
        /// <summary>
        /// 是否是最优价
        /// </summary>
        public bool isBestPrice
        {
            get
            {
                return _isBestPrice;
            }
            set
            {
                _isBestPrice = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isFwdCheck;
        /// <summary>
        /// 详情（远期界面）
        /// </summary>
        public bool isFwdCheck
        {
            get
            {
                return _isFwdCheck;
            }
            set
            {
                _isFwdCheck = value;
                this.OnPropertyChanged();
            }
        }


        private string _goodCode;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string goodCode
        {
            get
            {
                return _goodCode;
            }
            set
            {
                _goodCode = value;
                this.OnPropertyChanged();
            }
        }

        private bool _flagUrgent;

        /// <summary>
        /// 紧急标志
        /// </summary>
        public bool flagUrgent
        {
            get
            {
                return _flagUrgent;
            }
            set
            {
                _flagUrgent = value;
                this.OnPropertyChanged();
            }
        }

        public string FwdId
        {
            get { return string.Format("{0}-{1}", "Fwd", id); }
        }


        public bool IsSpot
        {
            get
            {
                return settlementType.Contains("+0") || settlementType.Contains("+1") || settlementType.Contains("明天+0");
            }
        }

        private decimal _price;

        /// <summary>
        /// 价格
        /// </summary>
        public decimal price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                this.OnPropertyChanged();
            }
        }

        private string _showprice;

        /// <summary>
        /// 价格
        /// </summary>
        public string showprice
        {
            get
            {
                return _showprice;
            }
            set
            {
                _showprice = value;
                this.OnPropertyChanged();
            }
        }

        public string PriceFormat
        {
            get
            {
                return MathEx.PriceFormat(price);
            }
        }

        public string PriceInt
        {
            get
            {
                return PriceFormat.Substring(0, PriceFormat.IndexOf('.') + 1);
            }
        }

        public string PriceDecimal
        {
            get
            {
                return PriceFormat.Substring(PriceFormat.IndexOf('.') + 1);
            }
        }


        /// <summary>
        /// 报价类型，2线上，1线下
        /// </summary>

        private string _onLine;

        public string OnLine
        {
            get
            {
                return _onLine;
            }
            set
            {
                _onLine = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 报价类型，2线上，1线下
        /// </summary>

        private Visibility _onLineVisibility = Visibility.Collapsed;

        public Visibility OnLineVisibility
        {
            get
            {
                return _onLineVisibility;
            }
            set
            {
                _onLineVisibility = value;
                this.OnPropertyChanged();
            }
        }


        private string _settlementTypeShow;

        /// <summary>
        /// 结算展示 V2.10.0
        /// </summary>
        public string settlementTypeShow
        {
            get
            {
                return _settlementTypeShow;
            }
            set
            {
                _settlementTypeShow = value;
                this.OnPropertyChanged();
            }
        }


        private int _MDPriceLevel = 0;
        /// <summary>
        /// 优先顺序
        /// </summary>
        public int MDPriceLevel { get { return _MDPriceLevel; } set { _MDPriceLevel = value; this.OnPropertyChanged(); } }



        private string _CalFlag = "";
        /// <summary>
        /// 试算标记 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0-原始 
        /// </summary>
        public string CalFlag { get { return _CalFlag; } set { _CalFlag = value; this.OnPropertyChanged(); } }

        private string _ExerciseFlag = "";
        /// <summary>
        /// 0=行权收益率,1或空=到期收益率
        /// </summary>
        public string ExerciseFlag { get { return _ExerciseFlag; } set { _ExerciseFlag = value; this.OnPropertyChanged(); } }

        private string _VenueTypeGW = "";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string VenueTypeGW { get { return _VenueTypeGW; } set { _VenueTypeGW = value; this.OnPropertyChanged(); } }


        private string _CustomerSource = "";
        /// <summary>
        /// 自定义来源
        /// </summary>
        public string CustomerSource { get { return _CustomerSource; } set { _CustomerSource = value; this.OnPropertyChanged(); } }

        private string _Source = "";
        /// <summary>
        /// Venues
        /// </summary>
        public string Source { get { return _Source; } set { _Source = value; this.OnPropertyChanged(); } }

        private string _SourceId = "";
        /// <summary>
        /// 渠道ID
        /// </summary>
        public string SourceId { get { return _SourceId; } set { _SourceId = value; this.OnPropertyChanged(); } }

        private string _Key = "";
        public string Key { get { return _Key; } set { _Key = value; this.OnPropertyChanged(); } }

        private string _Side = "";
        /// <summary>
        /// 方向
        /// </summary>
        public string Side { get { return _Side; } set { _Side = value; this.OnPropertyChanged(); } }


        private string _DeleteFlag = "0";
        /// <summary>
        ///  1要删除 0 新纪录
        /// </summary>
        public string DeleteFlag { get { return _DeleteFlag; } set { _DeleteFlag = value; this.OnPropertyChanged(); } }

        //priceType '0-意向;1-净价;2- 全价;3-收益率;4-利差;8-经纪商未指定',  当 priceType == 1   price=MDEntryPx 保存的是净价 yield保存是收益率 netPrice净价
        private string _PriceType;
        public string PriceType { get { return _PriceType; } set { _PriceType = value; this.OnPropertyChanged(); } }

        private string _settlType;
        public string settlType
        {
            get
            {
                return _settlType;
            }
            set
            {
                _settlType = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlTypeDisplay;
        public string SettlTypeDisplay
        {
            get
            {
                return _settlTypeDisplay;
            }
            set
            {
                if (_settlTypeDisplay != value)
                {
                    _settlTypeDisplay = value;
                }
                this.OnPropertyChanged();
            }
        }



        private string _transactTime;
        public string TransactTime { get { return _transactTime; } set { _transactTime = value; this.OnPropertyChanged(); } }

        private string _MDEntryDate;
        public string MDEntryDate { get { return _MDEntryDate; } set { _MDEntryDate = value; this.OnPropertyChanged(); } }


        private string _HandlInst;
        /// <summary>
        ///  HandlInst":"2" 2明盘 1是暗盘
        /// </summary>
        public string HandlInst { get { return _HandlInst; } set { _HandlInst = value; this.OnPropertyChanged(); } }

        private string _firmflag;
        /// <summary>
        /// 1 firm 0 NoFrim
        /// </summary>
        public string FirmFlag
        {
            get
            {
                return _firmflag;
            }
            set
            {
                _firmflag = value; this.OnPropertyChanged();
            }
        }

        private Visibility _firmflagVisibility = Visibility.Collapsed;
        /// <summary>
        /// 1 firm 0 NoFrim
        /// </summary>
        public Visibility FirmFlagVisibility
        {
            get
            {
                return _firmflagVisibility;
            }
            set
            {
                _firmflagVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> dictSettleType = new Dictionary<string, string>();
        /// <summary>
        /// 清算速度列表
        /// </summary>
        public Dictionary<string, string> DictSettleType
        {
            get
            {
                return dictSettleType;
            }
            set
            {
                dictSettleType = value;
                this.OnPropertyChanged();
            }
        }


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

        private string _info9;
        /// <summary>
        /// 备注清算速度
        /// </summary>
        public string Info9
        {
            get
            {
                return _info9;
            }
            set
            {
                _info9 = value; this.OnPropertyChanged();
            }
        }

        private Visibility _info9Visibility = Visibility.Collapsed;
        /// <summary>
        /// 备注清算速度
        /// </summary>
        public Visibility Info9Visibility
        {
            get
            {
                return _info9Visibility;
            }
            set
            {
                _info9Visibility = value; this.OnPropertyChanged();
            }
        }

        private string _info10;
        /// <summary>
        /// 备注清算速度
        /// </summary>
        public string Info10
        {
            get
            {
                return _info10;
            }
            set
            {
                _info10 = value; this.OnPropertyChanged();
            }
        }

        private string _divided;
        /// <summary>
        /// 标识需 P*
        /// </summary>
        public string Divided
        {
            get
            {
                return _divided;
            }
            set
            {
                _divided = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _dividedVisibility = Visibility.Collapsed;
        /// <summary>
        /// 标识需 P*
        /// </summary>
        public Visibility DividedVisibility
        {
            get
            {
                return _dividedVisibility;
            }
            set
            {
                _dividedVisibility = value;
                this.OnPropertyChanged();
            }
        }



        private string _IsInfo9Visible = "0";
        /// <summary>
        ///  1 可见 0 不可见
        /// </summary>
        public string IsInfo9Visible { get { return _IsInfo9Visible; } set { _IsInfo9Visible = value; this.OnPropertyChanged(); } }


        ////private string _institutionNameAndTradename;

        /////// <summary>
        /////// 机构名称(交易员)
        /////// </summary>
        ////public string InstitutionNameAndTradeName
        ////{
        ////    get
        ////    {
        ////        if (CommonHelper.Instance.IsSameTeamTag("", brokerID))

        ////        {
        ////            return $"{this.institutionName}({this.traderName})";
        ////        }
        ////        else
        ////        {
        ////            return Constant.DoubleDash;
        ////        }
        ////    }
        ////}


        private int _SortNum = 0;
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNum
        {
            get
            {
                return _SortNum;
            }
            set
            {
                _SortNum = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// P*标识复制展示
        /// </summary>
        public string DividedFlagShow
        {
            get
            {
                string show = string.Empty;
                if (Divided == "true")
                {
                    show = "P*";
                }
                return show;
            }
        }

        private Brush _qtyForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAB854"));
        /// <summary>
        /// 报价量颜色显示
        /// </summary>
        public Brush QtyForeground
        {
            get
            {
                return _qtyForeground;
            }
            set
            {
                if (_qtyForeground != value)
                {
                    _qtyForeground = value;
                    OnPropertyChanged("QtyForeground");
                }
            }
        }

        private string _yield;

        /// <summary>
        /// 收益率
        /// </summary>
        public string Yield
        {
            get
            {
                return _yield;
            }
            set
            {
                _yield = value;
                this.OnPropertyChanged();
            }
        }


        private string _netprice;

        /// <summary>
        /// 净价
        /// </summary>
        public string NetPrice
        {
            get
            {
                return _netprice;
            }
            set
            {
                _netprice = value;
                this.OnPropertyChanged();
            }
        }

        private string _mdentrysize;

        /// <summary>
        /// 原始报量  可能为 --
        /// </summary>
        public string MDEntrySize
        {
            get
            {
                return _mdentrysize;
            }
            set
            {
                _mdentrysize = value;
                this.OnPropertyChanged();
            }
        }

        private string _mdentrypx;

        /// <summary>
        /// 原始报价 可能为 --
        /// </summary>
        public string MDEntryPx
        {
            get
            {
                return _mdentrypx;
            }
            set
            {
                _mdentrypx = value;
                this.OnPropertyChanged();
            }
        }

        private string _yieldtoexecution;
        /// <summary>
        /// 行权收益率
        /// </summary>
        public string YieldToExecution
        {
            get
            {
                return _yieldtoexecution;
            }

            set
            {
                _yieldtoexecution = value;
            }
        }
    }
}
