using STC.Common.Model.MM;
using STC.Rfq.Plugin.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Rfq.Plugin.Utils;
using STC.Common.Utils;
using STC.Common.Model.RFQ;

namespace STC.Rfq.Plugin.Model
{
    /// <summary>
    /// RFQ策略列表
    /// </summary>
    public class RfqstgViewDataModel : ViewModelBase
    {
        /// <summary>
        /// 策略ID
        /// </summary>
        public string id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    this.OnPropertyChanged(nameof(id));
                }
            }
        }

        private string _id;


        /// <summary>
        /// 策略分组ID
        /// </summary>
        public string groupid
        {
            get { return _groupid; }
            set
            {
                if (_groupid != value)
                {
                    _groupid = value;
                    this.OnPropertyChanged(nameof(groupid));
                }
            }
        }

        private string _groupid;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid
        {
            get { return _securityid; }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.OnPropertyChanged(nameof(securityid));
                }
            }
        }

        private string _securityid;

        /// <summary>
        /// 债券名称
        /// </summary>
        public string symbol
        {
            get { return _symbol; }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged(nameof(symbol));
                }
            }
        }

        private string _symbol;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    this.OnPropertyChanged(nameof(username));
                }
            }
        }

        private string _username;


        /// <summary>
        /// 开关状态 
        /// </summary>
        public string enable
        {
            get { return _enable; }
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    this.OnPropertyChanged(nameof(enable));
                }
            }
        }

        private string _enable;



        // 基础设置 --------------------------------------------------- //

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settltype
        {
            get { return _settltype; }
            set
            {
                if (_settltype != value)
                {
                    _settltype = value;
                    this.OnPropertyChanged(nameof(settltype));
                }
            }
        }

        private string _settltype;

        /// <summary>
        /// 有效方向:不限,Buy,Se11,无   (A11, Buy, Sel1, None)
        /// </summary>
        public string validSide
        {
            get { return _validSide; }
            set
            {
                if (_validSide != value)
                {
                    _validSide = value;
                    this.OnPropertyChanged(nameof(validSide));
                }
            }
        }

        private string _validSide = "";

        /// <summary>
        /// Bid策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),     ==  bidStrategyType
        /// to do...
        /// </summary>
        public string strategyTypeBid
        {
            get { return _strategyTypeBid; }
            set
            {
                if (_strategyTypeBid != value)
                {
                    _strategyTypeBid = value;
                    this.OnPropertyChanged(nameof(strategyTypeBid));
                }
            }
        }

        private string _strategyTypeBid;

        /// <summary>
        /// Defend(防守) Algorithm(算法) Manual(人工) API("接口"),   == ofrStrategyType 
        /// to do...
        /// </summary>
        public string strategyTypeOfr
        {
            get { return _strategyTypeOfr; }
            set
            {
                if (_strategyTypeOfr != value)
                {
                    _strategyTypeOfr = value;
                    this.OnPropertyChanged(nameof(strategyTypeOfr));
                }
            }
        }

        private string _strategyTypeOfr;

        /// <summary>
        /// Bid预设回量
        /// to do...
        /// </summary>
        public string quantityBid
        {
            get { return _quantityBid; }
            set
            {
                if (_quantityBid != value)
                {
                    _quantityBid = value;
                    this.OnPropertyChanged(nameof(quantityBid));
                }
            }
        }

        private string _quantityBid;

        /// <summary>
        /// Ofr预设回量
        /// to do...
        /// </summary>
        public string quantityOfr
        {
            get { return _quantityOfr; }
            set
            {
                if (_quantityOfr != value)
                {
                    _quantityOfr = value;
                    this.OnPropertyChanged(nameof(quantityOfr));
                }
            }
        }

        private string _quantityOfr;

        /// <summary>
        /// BID方向有效日期
        /// </summary>
        public string validDateBid
        {
            get { return _validDateBid; }
            set
            {
                if (_validDateBid != value)
                {
                    _validDateBid = value;
                    this.OnPropertyChanged(nameof(validDateBid));
                }
            }
        }

        private string _validDateBid;

        private DateTime ? startdateBid;
        /// <summary>
        /// Bid UI 有效日期
        /// </summary>
        public DateTime ? StartDateBid
        {
            get { return startdateBid; }
            set
            {

                if (startdateBid != value)
                {
                    startdateBid = value;
                    this.OnPropertyChanged(nameof(StartDateBid));
                   this.validDateBid = startdateBid?.ToString("yyyy-MM-dd");
                }
            }
        }

        /// <summary>
        /// OFR方向有效日期
        /// </summary>
        public string validDateOfr
        {
            get { return _validDateOfr; }
            set
            {
                if (_validDateOfr != value)
                {
                    _validDateOfr = value;
                    this.OnPropertyChanged(nameof(validDateOfr));
                }
            }
        }

        private string _validDateOfr;

        private DateTime? startdateOfr;
        /// <summary>
        /// Ofr UI 有效日期
        /// </summary>
        public DateTime? StartdateOfr
        {
            get { return startdateOfr; }
            set
            {

                if (startdateOfr != value)
                {
                    startdateOfr = value;
                    this.OnPropertyChanged(nameof(StartdateOfr));
                    this.validDateOfr = startdateOfr?.ToString("yyyy-MM-dd");
                }
            }
        }
        /// <summary>
        /// BID方向回复有效时长
        /// </summary>
        public string quoteValidUntilTimeBid
        {
            get { return _quoteValidUntilTimeBid; }
            set
            {
                if (_quoteValidUntilTimeBid != value)
                {
                    _quoteValidUntilTimeBid = value;
                    this.OnPropertyChanged(nameof(quoteValidUntilTimeBid));
                }
            }
        }

        private string _quoteValidUntilTimeBid;

        /// <summary>
        /// OFR方向回复有效时长
        /// </summary>
        public string quoteValidUntilTimeOfr
        {
            get { return _quoteValidUntilTimeOfr; }
            set
            {
                if (_quoteValidUntilTimeOfr != value)
                {
                    _quoteValidUntilTimeOfr = value;
                    this.OnPropertyChanged(nameof(quoteValidUntilTimeOfr));
                }
            }
        }

        private string _quoteValidUntilTimeOfr;

        /// <summary>
        /// BID方向重发阈值
        /// </summary>
        public string resendThresholdBid
        {
            get { return _resendThresholdBid; }
            set
            {
                if (_resendThresholdBid != value)
                {
                    _resendThresholdBid = value;
                    this.OnPropertyChanged(nameof(resendThresholdBid));
                }
            }
        }

        private string _resendThresholdBid;

        /// <summary>
        /// OFR方向重发阈值
        /// </summary>
        public string resendThresholdOfr
        {
            get { return _resendThresholdOfr; }
            set
            {
                if (_resendThresholdOfr != value)
                {
                    _resendThresholdOfr = value;
                    this.OnPropertyChanged(nameof(resendThresholdOfr));
                }
            }
        }

        private string _resendThresholdOfr;

        /// <summary>
        /// BID方向回量上限
        /// </summary>
        public string maxQtyBid
        {
            get { return _maxQtyBid; }
            set
            {
                if (_maxQtyBid != value)
                {
                    _maxQtyBid = value;
                    this.OnPropertyChanged(nameof(maxQtyBid));
                }
            }
        }

        private string _maxQtyBid;

        /// <summary>
        /// OFR方向回量上限
        /// </summary>
        public string maxQtyOfr
        {
            get { return _maxQtyOfr; }
            set
            {
                if (_maxQtyOfr != value)
                {
                    _maxQtyOfr = value;
                    this.OnPropertyChanged(nameof(maxQtyOfr));
                }
            }
        }

        private string _maxQtyOfr;

        // 一般报价设置 --------------------------------------------------- //

        /// <summary>
        /// 手动报价-BID
        /// </summary>
        public string manualBid
        {
            get { return _manualBid; }
            set
            {
                if (_manualBid != value)
                {
                    _manualBid = value;
                    this.OnPropertyChanged(nameof(manualBid));
                }
            }
        }

        private string _manualBid;

        /// <summary>
        /// 手动报价-OFR
        /// </summary>
        public string manualOfr
        {
            get { return _manualOfr; }
            set
            {
                if (_manualOfr != value)
                {
                    _manualOfr = value;
                    this.OnPropertyChanged(nameof(manualOfr));
                }
            }
        }

        private string _manualOfr;

        /// <summary>
        /// 手动报价-调整步长
        /// to do ... 
        /// </summary>
        public string manualSpread
        {
            get { return _manualSpread; }
            set
            {
                if (_manualSpread != value)
                {
                    _manualSpread = value;
                    this.OnPropertyChanged(nameof(manualSpread));
                }
            }
        }

        private string _manualSpread = "0.25";

        // 防守设置 --------------------------------------------------- //

        /// <summary>
        /// bid防守报价 显示用
        /// </summary>
        public string bidDefenCalPrice
        {
            get { return _bidDefenCalPrice; }
            set
            {
                if (_bidDefenCalPrice != value)
                {
                    _bidDefenCalPrice = value;
                    this.OnPropertyChanged(nameof(bidDefenCalPrice));
                }
            }
        }

        private string _bidDefenCalPrice;

        /// <summary>
        /// ofr防守报价 显示用
        /// </summary>
        public string ofrDefenCalPrice
        {
            get { return _ofrDefenCalPrice; }
            set
            {
                if (_ofrDefenCalPrice != value)
                {
                    _ofrDefenCalPrice = value;
                    this.OnPropertyChanged(nameof(ofrDefenCalPrice));
                }
            }
        }

        private string _ofrDefenCalPrice;

        /// <summary>
        /// 防守点报价 +BID =  bidAlgoDefenValue
        /// </summary>
        public string defBid
        {
            get { return _defBid; }
            set
            {
                if (_defBid != value)
                {
                    _defBid = value;
                    this.OnPropertyChanged(nameof(defBid));
                }
            }
        }

        private string _defBid;

        /// <summary>
        /// 防守点报价- OFR = bidAlgoSafeValue
        /// </summary>
        public string defOfr
        {
            get { return _defOfr; }
            set
            {
                if (_defOfr != value)
                {
                    _defOfr = value;
                    this.OnPropertyChanged(nameof(defOfr));
                }
            }
        }

        private string _defOfr;

        /// <summary>
        /// 全局强墙 +Bid --用于显示
        /// </summary>
        public string defWallPriceBid
        {
            get { return _defWallPriceBid; }
            set
            {
                if (_defWallPriceBid != value)
                {
                    _defWallPriceBid = value;
                    this.OnPropertyChanged(nameof(defWallPriceBid));
                }
            }
        }

        private string _defWallPriceBid;

        /// <summary>
        /// 全局强墙-Ofr --用于显示
        /// </summary>
        public string defWallPriceOfr
        {
            get { return _defWallPriceOfr; }
            set
            {
                if (_defWallPriceOfr != value)
                {
                    _defWallPriceOfr = value;
                    this.OnPropertyChanged(nameof(defWallPriceOfr));
                }
            }
        }

        private string _defWallPriceOfr;

        /// <summary>
        /// 防守报价 +全场最优BID = Min 算法安全点 bidAlgoTrigValue
        /// </summary>
        public string defMaxBid
        {
            get { return _defMaxBid; }
            set
            {
                if (_defMaxBid != value)
                {
                    _defMaxBid = value;
                    this.OnPropertyChanged(nameof(defMaxBid));
                }
            }
        }

        private string _defMaxBid;

        /// <summary>
        /// 防守报价 -全场最优OFR    = MAX 算法安全点 bidAlgoSafeValue
        /// </summary>
        public string defMaxOfr
        {
            get { return _defMaxOfr; }
            set
            {
                if (_defMaxOfr != value)
                {
                    _defMaxOfr = value;
                    this.OnPropertyChanged(nameof(defMaxOfr));
                }
            }
        }

        private string _defMaxOfr;



        /// <summary>
        /// Bid触墙拉宽-BID墙   = bidAlgoTrigValue
        /// </summary>
        public string defWallBid
        {
            get { return _defWallBid; }
            set
            {
                if (_defWallBid != value)
                {
                    _defWallBid = value;
                    this.OnPropertyChanged(nameof(defWallBid));
                }
            }
        }
        private string _defWallBid;

        /// <summary>
        /// Ofr触墙拉宽-OFR墙  = ofrAlgoTrigValue
        /// </summary>
        public string defWallOfr
        {
            get { return _defWallOfr; }
            set
            {
                if (_defWallOfr != value)
                {
                    _defWallOfr = value;
                    this.OnPropertyChanged(nameof(defWallOfr));
                }
            }
        }

        private string _defWallOfr;

        // 算法报价 --------------------------------------------------- //
        /// <summary>
        /// bid算法报价 显示用
        /// </summary>
        public string bidAlgoCalPrice
        {
            get { return _bidAlgoCalPrice; }
            set
            {
                if (_bidAlgoCalPrice != value)
                {
                    _bidAlgoCalPrice = value;
                    this.OnPropertyChanged(nameof(bidAlgoCalPrice));
                }
            }
        }

        private string _bidAlgoCalPrice;


        /// <summary>
        /// bid算法报价返回值  algoBidPrice        algoOfrPrice
        /// </summary>
        public string algoBidPrice
        {
            get { return _algoBidPrice; }
            set
            {
                if (_algoBidPrice != value)
                {
                    _algoBidPrice = value;
                    this.OnPropertyChanged(nameof(algoBidPrice));
                }
            }
        }

        private string _algoBidPrice;


        /// <summary>
        /// ofr算法报价 显示用
        /// </summary>
        public string ofrAlgoCalPrice
        {
            get { return _ofrAlgoCalPrice; }
            set
            {
                if (_ofrAlgoCalPrice != value)
                {
                    _ofrAlgoCalPrice = value;
                    this.OnPropertyChanged(nameof(ofrAlgoCalPrice));
                }
            }
        }

        private string _ofrAlgoCalPrice;



        /// <summary>
        /// ofr算法报价返回值 algoOfrPrice
        /// </summary>
        public string algoOfrPrice
        {
            get { return _algoOfrPrice; }
            set
            {
                if (_algoOfrPrice != value)
                {
                    _algoOfrPrice = value;
                    this.OnPropertyChanged(nameof(algoOfrPrice));
                }
            }
        }

        private string _algoOfrPrice;

        /// <summary>
        /// 算法报价-中间价类型  == algoMidType 1 双边市场中间价，2：中债估值，3：自定义 ""为null
        /// </summary>
        public string algoMidPriceType
        {
            get { return _algoMidPriceType; }
            set
            {
                if (_algoMidPriceType != value)
                {
                    _algoMidPriceType = value;
                    this.OnPropertyChanged(nameof(algoMidPriceType));
                }
            }
        }

        private string _algoMidPriceType;

        /// <summary>
        /// 算法报价-自定义
        /// </summary>
        public string algoMidPrice
        {
            get { return _algoMidPrice; }
            set
            {
                if (_algoMidPrice != value)
                {
                    _algoMidPrice = value;
                    this.OnPropertyChanged(nameof(algoMidPrice));
                }
            }
        }


        private string _algoMidPrice;

        /// <summary>
        /// 是否使用基准债券0：否、1：是
        /// to do...
        /// </summary>
        public string algoBaseEnable
        {
            get { return _algoBaseEnable; }
            set
            {
                if (_algoBaseEnable != value)
                {
                    _algoBaseEnable = value;
                    this.OnPropertyChanged(nameof(algoBaseEnable));
                }
            }
        }

        private string _algoBaseEnable;

        /// <summary>
        /// 算法报价-基准券
        /// </summary>
        public string algoBaseSecurityId
        {
            get { return _algoBaseSecurityId; }
            set
            {
                if (_algoBaseSecurityId != value)
                {
                    _algoBaseSecurityId = value;
                    this.OnPropertyChanged(nameof(algoBaseSecurityId));
                }
            }
        }

        private string _algoBaseSecurityId;

        /// <summary>
        /// 参考中间价详情 显示用
        /// </summary>
        public string algoMidDetail
        {
            get { return _algoMidDetail; }
            set
            {
                if (_algoMidDetail != value)
                {
                    _algoMidDetail = value;
                    this.OnPropertyChanged(nameof(algoMidDetail));
                }
            }
        }

        private string _algoMidDetail;

        /// <summary>
        /// 中债估值详情 显示用
        /// </summary>
        public string algoCDCDetail
        {
            get { return _algoCDCDetail; }
            set
            {
                if (_algoCDCDetail != value)
                {
                    _algoCDCDetail = value;
                    this.OnPropertyChanged(nameof(algoCDCDetail));
                }
            }
        }

        private string _algoCDCDetail;

        /// <summary>
        /// 算法报价-中债估值详情是否可见
        /// </summary>
        public bool algoCDCDetailVisible
        {
            get { return _algoCDCDetailVisible; }
            set
            {
                if (_algoCDCDetailVisible != value)
                {
                    _algoCDCDetailVisible = value;
                    this.OnPropertyChanged(nameof(algoCDCDetailVisible));
                }
            }
        }
        private bool _algoCDCDetailVisible = false;

        /// <summary>
        /// 算法报价-算法偏离  == algoMidOffsetValue
        /// </summary>
        public string algoDeviation
        {
            get { return _algoDeviation; }
            set
            {
                if (_algoDeviation != value)
                {
                    _algoDeviation = value;
                    this.OnPropertyChanged(nameof(algoDeviation));
                }
            }
        }

        private string _algoDeviation;

        /// <summary>
        /// 算法报价-算法点差  == algoValue
        /// </summary>
        public string algoSpread
        {
            get { return _algoSpread; }
            set
            {
                if (_algoSpread != value)
                {
                    _algoSpread = value;
                    this.OnPropertyChanged(nameof(algoSpread));
                }
            }
        }

        private string _algoSpread;

        // 接口报价 --------------------------------------------------- //
        /// <summary>
        /// 接口报价-接口状态 == apiLastTime 显示用
        /// </summary>
        public string apiStatus
        {
            get { return _apiStatus; }
            set
            {
                if (_apiStatus != value)
                {
                    _apiStatus = value;
                    this.OnPropertyChanged(nameof(apiStatus));
                }
            }
        }

        private string _apiStatus;

        /// <summary>
        /// 接口报价-BID价格 显示用
        /// </summary>
        public string apiBid
        {
            get { return _apiBid; }
            set
            {
                if (_apiBid != value)
                {
                    _apiBid = value;
                    this.OnPropertyChanged(nameof(apiBid));
                }
            }
        }

        private string _apiBid;

        /// <summary>
        /// 接口报价-OFR价格 显示用
        /// </summary>
        public string apiOfr
        {
            get { return _apiOfr; }
            set
            {
                if (_apiOfr != value)
                {
                    _apiOfr = value;
                    this.OnPropertyChanged(nameof(apiOfr));
                }
            }
        }

        private string _apiOfr;

        /// <summary>
        /// 接口报价-BID报量 显示用
        /// </summary>
        public string apiBidQty
        {
            get { return _apiBidQty; }
            set
            {
                if (_apiBidQty != value)
                {
                    _apiBidQty = value;
                    this.OnPropertyChanged(nameof(apiBidQty));
                }
            }
        }

        private string _apiBidQty;

        /// <summary>
        /// 接口报价-OFR报量 显示用
        /// </summary>
        public string apiOfrQty
        {
            get { return _apiOfrQty; }
            set
            {
                if (_apiOfrQty != value)
                {
                    _apiOfrQty = value;
                    this.OnPropertyChanged(nameof(apiOfrQty));
                }
            }
        }

        private string _apiOfrQty;

        /// <summary>
        /// 成交达成后不再自动回复,0：不回复，1：回复
        /// </summary>
        public int autoReply
        {
            get { return _autoReply; }
            set
            {
                if (_autoReply != value)
                {
                    _autoReply = value;
                    this.OnPropertyChanged(nameof(autoReply));
                }
            }
        }

        private int _autoReply;

        /// <summary>
        /// 触发ACT后是否撤单，0：不撤单，1：撤单
        /// </summary>
        public int actCancel
        {
            get { return _actCancel; }
            set
            {
                if (_actCancel != value)
                {
                    _actCancel = value;
                    this.OnPropertyChanged(nameof(actCancel));
                }
            }
        }

        private int _actCancel;

        /// <summary>
        /// 格式化后的参数数据
        /// </summary>
        public string inf1
        {
            get { return _inf1; }
            set
            {
                if (_inf1 != value)
                {
                    _inf1 = value;
                    this.OnPropertyChanged(nameof(inf1));
                }
            }
        }
        private string _inf1;

        private string _inf2;
        public string inf2
        {
            get { return _inf2; }
            set
            {
                if (_inf2 != value)
                {
                    _inf2 = value;
                    this.OnPropertyChanged(nameof(inf2));
                }
            }
        }

        private string _inf3;
        public string inf3
        {
            get { return _inf3; }
            set
            {
                if (_inf3 != value)
                {
                    _inf3 = value;
                    this.OnPropertyChanged(nameof(inf3));
                }
            }
        }
        private string _inf4;
        public string inf4
        {
            get { return _inf4; }
            set
            {
                if (_inf4 != value)
                {
                    _inf4 = value;
                    this.OnPropertyChanged(nameof(inf4));
                }
            }
        }
        public string inf5
        {
            get { return _inf5; }
            set
            {
                if (_inf5 != value)
                {
                    _inf5 = value;
                    this.OnPropertyChanged(nameof(inf5));
                }
            }
        }
        private string _inf5;

        

        /// <summary>
        /// 我的Bid
        /// </summary>
        public decimal MyBid
        {
            get { return _MyBid; }
            set
            {
                if (_MyBid != value)
                {
                    _MyBid = value;
                    this.OnPropertyChanged(nameof(MyBid));
                }
            }
        }

        private decimal _MyBid;

        /// <summary>
        /// Bid订单数量
        /// </summary>
        public decimal MyBidQty
        {
            get { return _MyBidQty; }
            set
            {
                if (_MyBidQty != value)
                {
                    _MyBidQty = value;
                    this.OnPropertyChanged(nameof(MyBidQty));
                }
            }
        }

        private decimal _MyBidQty;

        /// <summary>
        /// 我的Ofr
        /// </summary>
        public decimal MyOfr
        {
            get { return _MyOfr; }
            set
            {
                if (_MyOfr != value)
                {
                    _MyOfr = value;
                    this.OnPropertyChanged(nameof(MyOfr));
                }
            }
        }

        private decimal _MyOfr;

        /// <summary>
        /// Ofr订单数量
        /// </summary>
        public decimal MyOfrQty
        {
            get { return _MyOfrQty; }
            set
            {
                if (_MyOfrQty != value)
                {
                    _MyOfrQty = value;
                    this.OnPropertyChanged(nameof(MyOfrQty));
                }
            }
        }

        private decimal _MyOfrQty;

        #region 债券参数
        /// <summary>
        /// 剩余期限
        /// </summary>
        private string _RemainingTime;
        public string RemainingTime
        {
            get { return _RemainingTime; }
            set
            {
                if (_RemainingTime != value)
                {
                    _RemainingTime = value;
                    this.OnPropertyChanged(nameof(RemainingTime));
                }
            }
        }

        /// <summary>
        /// Source 剩余期限
        /// </summary>
        private int _RemaingingTime_convert;

        public int RemaingingTime_convert
        {
            get { return _RemaingingTime_convert; }
            set
            {
                if (_RemaingingTime_convert != value)
                {
                    _RemaingingTime_convert = value;
                    this.OnPropertyChanged(nameof(RemaingingTime_convert));
                }
            }
        }

        /// <summary>
        /// 债券类型
        /// </summary>
        private string _BondType;

        public string BondType
        {
            get { return _BondType; }
            set
            {
                if (_BondType != value)
                {
                    _BondType = value;
                    this.OnPropertyChanged(nameof(BondType));
                }
            }
        }

        /// <summary>
        /// 债券类型
        /// </summary>
        private string _SecurityDesc;

        public string SecurityDesc
        {
            get { return _SecurityDesc; }
            set
            {
                if (_SecurityDesc != value)
                {
                    _SecurityDesc = value;
                    this.OnPropertyChanged(nameof(SecurityDesc));
                }
            }
        }

        /// <summary>
        /// 债券大类
        /// </summary>
        private string _GoodsType;
        public string GoodsType
        {
            get { return _GoodsType; }
            set
            {
                if (_GoodsType != value)
                {
                    _GoodsType = value;
                    this.OnPropertyChanged(nameof(GoodsType));
                }
            }
        }

        /// <summary>
        /// 债券子类
        /// </summary>
        private string _BondSubType;
        public string BondSubType
        {
            get { return _BondSubType; }
            set
            {
                if (_BondSubType != value)
                {
                    _BondSubType = value;
                    this.OnPropertyChanged(nameof(BondSubType));
                }
            }
        }

        /// <summary>
        /// 债券评级
        /// </summary>
        private string _IssuerRatingCurrentNPY;
        public string IssuerRatingCurrentNPY
        {
            get { return _IssuerRatingCurrentNPY; }
            set
            {
                if (_IssuerRatingCurrentNPY != value)
                {
                    _IssuerRatingCurrentNPY = value;
                    this.OnPropertyChanged(nameof(IssuerRatingCurrentNPY));
                }
            }
        }

        /// <summary>
        /// 发行人
        /// </summary>
        private string _Publisher;
        public string Publisher
        {
            get { return _Publisher; }
            set
            {
                if (_Publisher != value)
                {
                    _Publisher = value;
                    this.OnPropertyChanged(nameof(Publisher));
                }
            }
        }
        #endregion

        #region 中债估值
        /// <summary>
        /// 中债估值
        /// </summary>
        private string _Valuation;
        public string Valuation
        {
            get { return _Valuation; }
            set
            {
                if (_Valuation != value)
                {
                    _Valuation = value;
                    this.OnPropertyChanged(nameof(Valuation));
                }
            }
        }
        /// <summary>
        /// 中债估值日期 yyyyMMdd
        /// </summary>
        private string _CCDCValuaDate;
        public string CCDCValuaDate
        {
            get { return _CCDCValuaDate; }
            set
            {
                if (_CCDCValuaDate != value)
                {
                    _CCDCValuaDate = value;
                    this.OnPropertyChanged(nameof(CCDCValuaDate));
                }
            }
        }

        /// <summary>
        /// 中债估值日期是否等于最后的交易日 不等于显示 ！
        /// </summary>
        private string _ShowLastWorkDay;
        public string ShowLastWorkDay
        {
            get { return _ShowLastWorkDay; }
            set
            {
                if (_ShowLastWorkDay != value)
                {
                    _ShowLastWorkDay = value;
                    this.OnPropertyChanged(nameof(ShowLastWorkDay));
                }
            }
        }
        #endregion

        public string closeby
        {
            get { return _closeby; }
            set
            {
                if (_closeby != value)
                {
                    _closeby = value;
                    this.OnPropertyChanged(nameof(closeby));
                }
            }
        }

        private string _closeby;

        public string remark
        {
            get { return _remark; }
            set
            {
                if (_remark != value)
                {
                    _remark = value;
                    this.OnPropertyChanged(nameof(remark));
                }
            }
        }
        private string _remark;
        public string createtime
        {
            get { return _createtime; }
            set
            {
                if (_createtime != value)
                {
                    _createtime = value;
                    this.OnPropertyChanged(nameof(createtime));
                }
            }
        }
        private string _createtime;

        public string updatetime
        {
            get { return _updatetime; }
            set
            {
                if (_updatetime != value)
                {
                    _updatetime = value;
                    this.OnPropertyChanged(nameof(updatetime));
                }
            }
        }

        private string _updatetime;

    }
}
