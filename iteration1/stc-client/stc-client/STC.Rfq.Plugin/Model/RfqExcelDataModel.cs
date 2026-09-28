using STC.Rfq.Plugin.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin.Model
{
    /// <summary>
    /// RFQ Excel Import Model
    /// </summary>
    public class RfqExcelDataModel : ViewModelBase
    {
        #region 中文字段
        private string _代码;
        public string 代码
        {
            get 
            { 
                return _代码; 
            }
            set 
            {
                _代码 = value;
                this.securityid = value;
                this.OnPropertyChanged();
            }
        }

        private string _简称;

        public string 简称
        {
            get
            {
                return _简称;
            }
            set
            {
                _简称 = value;
                this.symbol = value;
                this.OnPropertyChanged();
            }
        }
        
        private string _清算速度;

        public string 清算速度
        {
            get
            {
                return _清算速度;
            }
            set
            {
                _清算速度 = value;
                this.settltype = RfqGroupHandler.Instance.ReverseBaseSettleTypeList.ContainsKey(value) ? RfqGroupHandler.Instance.ReverseBaseSettleTypeList[value] : "2";
                this.OnPropertyChanged();
            }
        }

        private string _有效方向;

        public string 有效方向
        {
            get
            {
                return _有效方向;
            }
            set
            {
                _有效方向 = value;
                this.validSide = RfqGroupHandler.Instance.ReverseValidSideList.ContainsKey(value) ? RfqGroupHandler.Instance.ReverseValidSideList[value] : "All";
                this.OnPropertyChanged();
            }
        }

        private string _Bid策略类型;
        public string Bid策略类型
        {
            get
            {
                return _Bid策略类型;
            }
            set
            {
                _Bid策略类型 = value;
                this.strategyTypeBid = RfqGroupHandler.Instance.ReverseStrategyModeList.ContainsKey(value) ? RfqGroupHandler.Instance.ReverseStrategyModeList[value] : "Manual";
                this.OnPropertyChanged();
            }
        }

        private string _Bid预设回量;

        public string Bid预设回量
        {
            get
            {
                return _Bid预设回量;
            }
            set
            {
                _Bid预设回量 = value;
                this.quantityBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid有效日期;

        public string Bid有效日期
        {
            get
            {
                return _Bid有效日期;
            }
            set
            {
                _Bid有效日期 = value;
                this.validDateBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid有效时长;

        public string Bid有效时长
        {
            get
            {
                return _Bid有效时长;
            }
            set
            {
                _Bid有效时长 = value;
                this.quoteValidUntilTimeBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid重发阈值;
        public string Bid重发阈值
        {
            get
            {
                return _Bid重发阈值;
            }
            set
            {
                _Bid重发阈值 = value;
                this.resendThresholdBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid回量上限;
        public string Bid回量上限
        {
            get
            {
                return _Bid回量上限;
            }
            set
            {
                _Bid回量上限 = value;
                this.maxQtyBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid手动报价;
        public string Bid手动报价
        {
            get
            {
                return _Bid手动报价;
            }
            set
            {
                _Bid手动报价 = value;
                this.manualBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid防守点;
        public string Bid防守点
        {
            get
            {
                return _Bid防守点;
            }
            set
            {
                _Bid防守点 = value;
                this.defBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid全市场最优;
        public string Bid全市场最优
        {
            get
            {
                return _Bid全市场最优;
            }
            set
            {
                _Bid全市场最优 = value;
                this.defMaxBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Bid触墙拉宽;
        public string Bid触墙拉宽
        {
            get
            {
                return _Bid触墙拉宽;
            }
            set
            {
                _Bid触墙拉宽 = value;
                this.defWallBid = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr策略类型;
        public string Ofr策略类型
        {
            get
            {
                return _Ofr策略类型;
            }
            set
            {
                _Ofr策略类型 = value;
                this.strategyTypeOfr = RfqGroupHandler.Instance.ReverseStrategyModeList.ContainsKey(value) ? RfqGroupHandler.Instance.ReverseStrategyModeList[value] : "Manual";
                this.OnPropertyChanged();
            }
        }

        private string _Ofr预设回量;
        public string Ofr预设回量
        {
            get
            {
                return _Ofr预设回量;
            }
            set
            {
                _Ofr预设回量 = value;
                this.quantityOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr有效日期;
        public string Ofr有效日期
        {
            get
            {
                return _Ofr有效日期;
            }
            set
            {
                _Ofr有效日期 = value;
                this.validDateOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr有效时长;
        public string Ofr有效时长
        {
            get
            {
                return _Ofr有效时长;
            }
            set
            {
                _Ofr有效时长 = value;
                this.quoteValidUntilTimeOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr重发阈值;
        public string Ofr重发阈值
        {
            get
            {
                return _Ofr重发阈值;
            }
            set
            {
                _Ofr重发阈值 = value;
                this.resendThresholdOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr回量上限;
        public string Ofr回量上限
        {
            get
            {
                return _Ofr回量上限;
            }
            set
            {
                _Ofr回量上限 = value;
                this.maxQtyOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr手动报价;
        public string Ofr手动报价
        {
            get
            {
                return _Ofr手动报价;
            }
            set
            {
                _Ofr手动报价 = value;
                this.manualOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr防守点;
        public string Ofr防守点
        {
            get
            {
                return _Ofr防守点;
            }
            set
            {
                _Ofr防守点 = value;
                this.defOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr全市场最优;
        public string Ofr全市场最优
        {
            get
            {
                return _Ofr全市场最优;
            }
            set
            {
                _Ofr全市场最优 = value;
                this.defMaxOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _Ofr触墙拉宽;
        public string Ofr触墙拉宽
        {
            get
            {
                return _Ofr触墙拉宽;
            }
            set
            {
                _Ofr触墙拉宽 = value;
                this.defWallOfr = value;
                this.OnPropertyChanged();
            }
        }

        private string _参考中间价;
        public string 参考中间价
        {
            get
            {
                return _参考中间价;
            }
            set
            {
                _参考中间价 = value;
                this.algoMidPriceType = string.IsNullOrEmpty(value) ? null : RfqGroupHandler.Instance.ReverseAlgoMidTypeList.ContainsKey(value) ? RfqGroupHandler.Instance.ReverseAlgoMidTypeList[value] : null;
                this.OnPropertyChanged();
            }
        }

        private string _自定义;
        public string 自定义
        {
            get
            {
                return _自定义;
            }
            set
            {
                _自定义 = value;
                this.algoMidPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _是否使用基准券;
        public string 是否使用基准券
        {
            get
            {
                return _是否使用基准券;
            }
            set
            {
                _是否使用基准券 = value;
                this.algoBaseEnable = string.IsNullOrEmpty(value) ? "0" : value;
                this.OnPropertyChanged();
            }
        }

        private string _基准券;
        public string 基准券
        {
            get
            {
                return _基准券;
            }
            set
            {
                _基准券 = value;
                this.algoBaseSecurityId = value;
                this.OnPropertyChanged();
            }
        }

        private string _算法偏离;
        public string 算法偏离
        {
            get
            {
                return _算法偏离;
            }
            set
            {
                _算法偏离 = value;
                this.algoDeviation = value;
                this.OnPropertyChanged();
            }
        }

        private string _算法点差;
        public string 算法点差
        {
            get
            {
                return _算法点差;
            }
            set
            {
                _算法点差 = value;
                this.algoSpread = value;
                this.OnPropertyChanged();
            }
        }

        private int _成交达成后不再自动回复;
        public int 成交达成后不再自动回复
        {
            get
            {
                return _成交达成后不再自动回复;
            }
            set
            {
                _成交达成后不再自动回复 = value;
                this.autoReply = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 参数字段
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
        #endregion

        //newDR["代码"] = param.securityid;
        //        newDR["简称"] = param.symbol;
        //        newDR["清算速度"] = RfqGroupHandler.Instance.BaseSettleTypeList.ContainsKey(param.settltype)? RfqGroupHandler.Instance.BaseSettleTypeList[param.settltype] : "2";
        //newDR["有效方向"] = RfqGroupHandler.Instance.ValidSideList.ContainsKey(param.validSide)? RfqGroupHandler.Instance.ValidSideList[param.validSide] : "All";
        //newDR["Bid策略类型"] = RfqGroupHandler.Instance.StrategyModeList.ContainsKey(param.strategyTypeBid)? RfqGroupHandler.Instance.StrategyModeList[param.strategyTypeBid] : "Manual";
        //newDR["Bid预设回量"] = param.quantityBid;
        //        newDR["Bid有效日期"] = param.validDateBid;
        //        newDR["Bid有效时长"] = param.quoteValidUntilTimeBid;
        //        newDR["Bid重发阈值"] = param.resendThresholdBid;
        //        newDR["Bid回量上限"] = param.maxQtyBid;
        //        newDR["Bid手动报价"] = param.manualBid;
        //        newDR["Bid防守点"] = param.defBid;
        //        newDR["Bid全市场最优"] = param.defMaxBid;
        //        newDR["Bid触墙拉宽"] = param.defWallBid;

        //        newDR["Ofr策略类型"] = RfqGroupHandler.Instance.StrategyModeList.ContainsKey(param.strategyTypeOfr)? RfqGroupHandler.Instance.StrategyModeList[param.strategyTypeOfr] : "Manual";
        //newDR["Ofr预设回量"] = param.quantityOfr;
        //        newDR["Ofr有效日期"] = param.validDateOfr;
        //        newDR["Ofr有效时长"] = param.quoteValidUntilTimeOfr;
        //        newDR["Ofr重发阈值"] = param.resendThresholdOfr;
        //        newDR["Ofr回量上限"] = param.maxQtyOfr;
        //        newDR["Ofr手动报价"] = param.manualOfr;
        //        newDR["Ofr防守点"] = param.defOfr;
        //        newDR["Ofr全市场最优"] = param.defMaxOfr;
        //        newDR["Ofr触墙拉宽"] = param.defWallOfr;

        //        // algoMidType 1 全市场中间价，2：中债估值，3：自定义 ""为null  algoMidPrice
        //        newDR["参考中间价"] = string.IsNullOrEmpty(param.algoMidPriceType) ? null : RfqGroupHandler.Instance.AlgoMidTypeList.ContainsKey(param.algoMidPriceType)? RfqGroupHandler.Instance.AlgoMidTypeList[param.algoMidPriceType] : null;

        //newDR["自定义"] = param.algoMidPrice;
        //        //是否使用基准债券0：否、1：是
        //        newDR["是否使用基准券"] = string.IsNullOrEmpty(param.algoBaseEnable) ? "0" : param.algoBaseEnable;
        //        newDR["基准券"] = param.algoBaseSecurityId;
        //        newDR["算法偏离"] = param.algoDeviation;
        //        newDR["算法点差"] = param.algoSpread;
        //        //0：不回复，1：回复
        //        newDR["成交达成后不再自动回复"] = param.autoReply;

    }
}
