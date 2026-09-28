using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PP.Plugin.Model
{
    public class StcBondPositionData : BaseViewModel
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value) 
                {
                    _isChecked = value; OnPropertyChanged(); 
                }
            }
        }

        private string _key;
        public string Key
        {
            get { return _key; }
            set { if (_key != value) { _key = value; OnPropertyChanged(); } }
        }


        private string _portfolioNo;
        /// <summary>
        /// 组合代码
        /// </summary>
        public string PortfolioNo
        {
            get { return _portfolioNo; }
            set { if (_portfolioNo != value) { _portfolioNo = value; OnPropertyChanged(); } }
        }


        private string _portfolioName;
        /// <summary>
        /// 组合名称
        /// </summary>
        public string PortfolioName
        {
            get { return _portfolioName; }
            set { if (_portfolioName != value) { _portfolioName = value; OnPropertyChanged(); } }
        }


        private string _tradeAccount;
        /// <summary>
        /// 交易账户代码
        /// </summary>
        public string TradeAccount
        {
            get { return _tradeAccount; }
            set { if (_tradeAccount != value) { _tradeAccount = value; OnPropertyChanged(); } }
        }


        private string _tradeName;
        /// <summary>
        /// 交易账户名称
        /// </summary>
        public string TradeName
        {
            get { return _tradeName; }
            set { if (_tradeName != value) { _tradeName = value; OnPropertyChanged(); } }
        }


        private string _fundAccount;
        /// <summary>
        /// 投资经理
        /// </summary>
        public string FundAccount
        {
            get { return _fundAccount; }
            set { if (_fundAccount != value) { _fundAccount = value; OnPropertyChanged(); } }
        }


        private string _remaingingTime;
        /// <summary>
        /// 剩余期限
        /// </summary>
        public string RemaingingTime
        {
            get { return _remaingingTime; }
            set { if (_remaingingTime != value) { _remaingingTime = value; OnPropertyChanged(); } }
        }


        private string _securityId;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityId
        {
            get { return _securityId; }
            set { if (_securityId != value) { _securityId = value; OnPropertyChanged(); } }
        }


        private string _symbolName;
        /// <summary>
        /// 债券名称
        /// </summary>
        public string SymbolName
        {
            get { return _symbolName; }
            set { if (_symbolName != value) { _symbolName = value; OnPropertyChanged(); } }
        }

        private string _issuer;
        /// <summary>
        /// 发行人
        /// </summary>
        public string Issuer
        {
            get { return _issuer; }
            set { if (_issuer != value) { _issuer = value; OnPropertyChanged(); } }
        }

        private decimal _availableposition;

        /// <summary>
        /// 可用持仓量(万)
        /// </summary>
        public decimal AvailablePosition
        {
            get { return _availableposition; }
            set { if (_availableposition != value) { _availableposition = value; OnPropertyChanged(); } }
        }

        private decimal _usedQty;

        /// <summary>
        /// 资金询价占用量
        /// </summary>
        public decimal UsedQty
        {
            get { return _usedQty; }
            set { if (_usedQty != value) { _usedQty = value; OnPropertyChanged(); } }
        }


        private decimal _stcInSalesVolume;
        /// <summary>
        /// STC在途卖量(万)
        /// </summary>
        public decimal StcInSalesVolume
        {
            get { return _stcInSalesVolume; }
            set { if (_stcInSalesVolume != value) { _stcInSalesVolume = value; OnPropertyChanged(); } }
        }

        private decimal _stcAvailablePosition;
        /// <summary>
        /// STC可用持仓量(万)
        /// </summary>
        public decimal StcAvailablePosition
        {
            get { return _stcAvailablePosition; }
            set { if (_stcAvailablePosition != value) { _stcAvailablePosition = value; OnPropertyChanged(); } }
        }

        /// <summary>
        ///  STC可用持仓量(万) 是否小于0
        /// </summary>
        public bool IsStcAvailableNegative
        {
            get { return _stcAvailablePosition < 0; }
        }
            

        private decimal _stcInBuyVolume;
        /// <summary>
        /// STC在途买量(万)
        /// </summary>
        public decimal StcInBuyVolume
        {
            get { return _stcInBuyVolume; }
            set { if (_stcInBuyVolume != value) { _stcInBuyVolume = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// STC在途买量(万) 是否小于0
        /// </summary>
        public bool IsStcInBuyVolumeNegative
        {
            get { return StcInBuyVolume < 0; }
        }

        private decimal _quotalimit;
        /// <summary>
        /// 主体额度(万)=>主体剩余额度(万)
        /// </summary>
        public decimal Quotalimit
        {
            get { return _quotalimit; }
            set { if (_quotalimit != value) { _quotalimit = value; OnPropertyChanged(); } }
        }

        private decimal _outstandingAmount;
        /// <summary>
        /// 单券余额(万)=>单券发行量(万)
        /// </summary>
        public decimal OutStandingAmount
        {
            get { return _outstandingAmount; }
            set { if (_outstandingAmount != value) { _outstandingAmount = value; OnPropertyChanged(); } }
        }
        

        private string _investmentratio;

        /// <summary>
        /// 可投比例(%)
        /// </summary>
        public string Investmentratio
        {
            get { return _investmentratio; }
            set { if (_investmentratio != value) { _investmentratio = value; OnPropertyChanged(); } }
        }


        private string _rate;
        /// <summary>
        /// 内部主体评级
        /// </summary>
        public string Rate
        {
            get { return _rate; }
            set 
            {
                if (_rate != value) 
                { 

                    _rate = value; OnPropertyChanged(); 
                } 
            }
        }


        private string _bonddrating;
        /// <summary>
        /// 内部债券评级
        /// </summary>
        public string Bonddrating
        {
            get { return _bonddrating; }
            set { if (_bonddrating != value) { _bonddrating = value; OnPropertyChanged(); } }
        }

        private decimal _netprice;

        /// <summary>
        /// 成本净价（元）
        /// </summary>
        public decimal Netprice
        {
            get { return _netprice; }
            set { if (_netprice != value) { _netprice = value; OnPropertyChanged(); } }
        }

        private string _valuationYield;

        /// <summary>
        /// 中债估值（元）
        /// </summary>
        public string ValuationYield
        {
            get { return _valuationYield; }
            set { if (_valuationYield != value) { _valuationYield = value; OnPropertyChanged(); } }
        }

        private string _valuationYield2;

        /// <summary>
        /// 中债估值（%）
        /// </summary>
        public string ValuationYield2
        {
            get { return _valuationYield2; }
            set { if (_valuationYield2 != value) { _valuationYield2 = value; OnPropertyChanged(); } }
        }

        private string _csiBondValuation;

        /// <summary>
        /// 中证估值（%）
        /// </summary>
        public string CsiBondValuation
        {
            get { return _csiBondValuation; }
            set { if (_csiBondValuation != value) { _csiBondValuation = value; OnPropertyChanged(); } }
        }

        private string _exClose;

        /// <summary>
        /// 交易所收盘价（元）
        /// </summary>
        public string ExClose
        {
            get { return _exClose; }
            set { if (_exClose != value) { _exClose = value; OnPropertyChanged(); } }
        }

        private string _exClose2;

        /// <summary>
        /// 交易所收盘价（%）
        /// </summary>
        public string ExClose2
        {
            get { return _exClose2; }
            set { if (_exClose2 != value) { _exClose2 = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// optim总持仓量(万)
        /// </summary>
        private decimal totalQty;

        public decimal TotalQty
        {
            get { return totalQty; }
            set { if (totalQty != value) { totalQty = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// STC可用额度(万)
        /// </summary>
        private decimal stcAvailableLimit;
        public decimal StcAvailableLimit
        {
            get { return stcAvailableLimit; }
            set { if (stcAvailableLimit != value) { stcAvailableLimit = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 单券剩余额度
        /// </summary>
        private decimal singleBondAmount;
        public decimal SingleBondAmount
        {
            get { return singleBondAmount; }
            set { if (singleBondAmount != value) { singleBondAmount = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 恒生基金编号
        /// </summary>
        private string hsProductId { get; set; }
        public string HsProductId
        {
            get { return hsProductId; }
            set { if (hsProductId != value) { hsProductId = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// STC可用额度(万) 是否小于0
        /// </summary>
        public bool IsStcAvailableLimitNegative
        {
            get { return StcAvailableLimit < 0; }
        }

        #region 内部主体评级到期日
        private string _rateenddate;
        /// <summary>
        /// 内部主体评级到期日
        /// </summary>
        public string RateEnddate
        {
            get { return _rateenddate; }
            set 
            { 
                if (_rateenddate != value) 
                { 
                    _rateenddate = value;
                    if (!string.IsNullOrWhiteSpace(_rateenddate))
                    {
                        ShowRateEnddate = $"过期日期{_rateenddate}";
                    }
                    else
                    {
                        ShowRateEnddate = "";
                    }
                    
                    OnPropertyChanged(); 
                } 
            }
        }

        private string _showrateenddatee;
        /// <summary>
        /// 内部主体评级到期日（显示ToolTip用）
        /// </summary>
        public string ShowRateEnddate
        {
            get { return _showrateenddatee; }
            set 
            {
                if (_showrateenddatee != value) { _showrateenddatee = value; OnPropertyChanged(); } 
            }
        }

        /// <summary>
        /// 内部主体评级已过期
        /// </summary>
        public bool IsRateEnddated
        {
            get
            {
                DateTime parsedDate;
                // 支持的日期格式（可根据需要扩展）
                string[] formats = new string[]
                {
                    "yyyyMMdd",
                    "yyyy-MM-dd",
                    "yyyy/MM/dd",
                    "MM/dd/yyyy",
                    "dd/MM/yyyy",
                    "yyyy-M-d",
                    "yyyy/M/d",
                    "M/d/yyyy",
                    "d/M/yyyy",
                    "dd-MM-yyyy",
                    "MM-dd-yyyy"
                };

                if (!string.IsNullOrWhiteSpace(_rateenddate))
                {
                    if (DateTime.TryParseExact(_rateenddate, formats,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None, out parsedDate))
                    {
                        // 只比较日期部分
                        return parsedDate.Date < DateTime.Now.Date;
                    }
                }
                else
                {
                    return false; // 格式不正确时返回 false
                }

                return false;
            }
        }

        #endregion

        #region  内部债券评级过期日

        private string _ratingExpirydate;
        /// <summary>
        /// 内部债券评级过期日
        /// </summary>
        public string RatingExpirydate
        {
            get { return _ratingExpirydate; }
            set
            {
                if (_ratingExpirydate != value) 
                { 
                    _ratingExpirydate = value;

                    if (!string.IsNullOrWhiteSpace(_ratingExpirydate))
                    {
                        ShowRatingExpiryDate = $"过期日期{_ratingExpirydate}";
                    }
                    else
                    {
                        ShowRatingExpiryDate = "";
                    }

                    OnPropertyChanged(); 
                }
            }
        }

        private string _showRatingExpirydate;
        /// <summary>
        /// 内部债券评级过期日 （显示ToolTip用）
        /// </summary>
        public string ShowRatingExpiryDate
        {
            get { return _showRatingExpirydate; }
            set
            {
                if (_showRatingExpirydate != value) 
                { 
                    _showRatingExpirydate = value; 
                    OnPropertyChanged(); 
                }
            }
        }

        private string _issueType;
        /// <summary>
        /// 募集类型
        /// </summary>
        public string IssueType
        {
            get { return _issueType; }
            set
            {
                if (_issueType != value)
                {
                    _issueType = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _guarantor;
        /// <summary>
        /// 担保人
        /// </summary>
        public string Guarantor
        {
            get { return _guarantor; }
            set
            {
                if (_guarantor != value)
                {
                    _guarantor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _guarantorRating;
        /// <summary>
        /// 担保人内部评级
        /// </summary>
        public string GuarantorRating
        {
            get { return _guarantorRating; }
            set
            {
                if (_guarantorRating != value)
                {
                    _guarantorRating = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 内部债券评级过已过期
        /// </summary>
        public bool IsRatingExpiryEnddated
        {
            get
            {
                DateTime parsedDate;
                // 支持的日期格式（可根据需要扩展）
                string[] formats = new string[]
                {
                    "yyyyMMdd",
                    "yyyy-MM-dd",
                    "yyyy/MM/dd",
                    "MM/dd/yyyy",
                    "dd/MM/yyyy",
                    "yyyy-M-d",
                    "yyyy/M/d",
                    "M/d/yyyy",
                    "d/M/yyyy",
                    "dd-MM-yyyy",
                    "MM-dd-yyyy"
                };

                if (!string.IsNullOrWhiteSpace(_ratingExpirydate))
                {
                    if (DateTime.TryParseExact(_ratingExpirydate, formats,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None, out parsedDate))
                    {
                        // 只比较日期部分
                        return parsedDate.Date < DateTime.Now.Date;
                    }
                }
                else
                {
                    return false; // 格式不正确时返回 false
                }

                return false;
            }
        }
        #endregion


        public StcBondPositionData()
        {
        }

    }
}
