using STC.Common.Model.MM;
using STC.Mm.Plugin.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Mm.Plugin.Utils;
using STC.Common.Utils;
using System.Threading;
using STC.Common.Model.Cal;
using STC.Common.Handler.Cal;
using System.Windows;
using com.gateway.log;

namespace STC.Mm.Plugin.ViewModel
{
    public class OrderParamProp : ViewModelBase, ICloneable
    {
        private MMOrderParamReq _MMOrderParamReq;

        public MMOrderParamReq MMOrderParamReq { get { return _MMOrderParamReq; } }

        public OrderParamProp(MMOrderParamReq param)
        {
            if (param == null)
                _MMOrderParamReq = new MMOrderParamReq();
            else
                _MMOrderParamReq = param;
        }

        //债券余额
        public string circulSize
        {
            get { return _MMOrderParamReq.circulSize; }
            set
            {
                _MMOrderParamReq.circulSize = value;
                if (!string.IsNullOrEmpty(_MMOrderParamReq.riskMaxQtyRatio)) 
                {
                    circulRatioSize = (_MMOrderParamReq.circulSize.ConvertToDecimal() * _MMOrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10).ToString("n2");
                }
                OnPropertyChanged();
            }
        }
        //债券余额
        public string circulRatioSize 
        { 
            get { return _MMOrderParamReq.circulRatioSize; } 
            set {
                _MMOrderParamReq.circulRatioSize = value;
                OnPropertyChanged();
            }
        }

        //风控报量下限
        public string riskMinQtySize
        {
            get { return _MMOrderParamReq.riskMinQtySize; }
            set
            {
                _MMOrderParamReq.riskMinQtySize = value;
                OnPropertyChanged();
            }
        }

        //风控报量上线比例
        public string riskMaxQtyRatio
        {
            get { return _MMOrderParamReq.riskMaxQtyRatio; }
            set
            {
                _MMOrderParamReq.riskMaxQtyRatio = value;
                if (!string.IsNullOrEmpty(_MMOrderParamReq.riskMaxQtyRatio))
                {
                    if (!string.IsNullOrEmpty(_MMOrderParamReq.circulSize))
                    {
                        circulRatioSize = (_MMOrderParamReq.circulSize.ConvertToDecimal() * _MMOrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10).ToString("n2");

                    }
                    else {
                        circulRatioSize = "";
                    }
                }
                OnPropertyChanged();
            }
        }

        public string securityid
        {
            get { return _MMOrderParamReq.securityid; }
            set
            {
                _MMOrderParamReq.securityid = value;
                this.OnPropertyChanged();
            }
        }

        public string symbol
        {
            get { return _MMOrderParamReq.symbol; }
            set
            {
                _MMOrderParamReq.symbol = value;
                this.OnPropertyChanged();
            }
        }
        public string algoMidPrice
        {
            get { return _MMOrderParamReq.algoMidPrice; }
            set
            {
                _MMOrderParamReq.algoMidPrice = value;
                this.OnPropertyChanged();
            }
        }

        //Bid价格优先级：1防守，2算法
        public string bidBasePriority
        {
            get
            {
                return _MMOrderParamReq.bidBasePriority;
            }

            set
            {
                _MMOrderParamReq.bidBasePriority = value;
                this.OnPropertyChanged();
            }

        }

        //bid防守报价
        public string bidDefenCalPrice
        {
            get { return _MMOrderParamReq.bidDefenCalPrice; }
            set
            {
                _MMOrderParamReq.bidDefenCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //bid算法报价
        public string bidAlgoCalPrice
        {
            get { return _MMOrderParamReq.bidAlgoCalPrice; }
            set
            {
                _MMOrderParamReq.bidAlgoCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //bid方向报价模式1:手动、2:自动
        public string bidQuoteModel
        {
            get { return _MMOrderParamReq.bidQuoteModel; }
            set
            {
                _MMOrderParamReq.bidQuoteModel = value;
                this.OnPropertyChanged();

                bidBasePrice = bidBasePrice;
                manualBid = manualBid;
                bidAlgoDefenValue = bidAlgoDefenValue;
                bidAlgoSafeValue = bidAlgoSafeValue;
                bidAlgoTrigValue = bidAlgoTrigValue;
                algoMidType = algoMidType;
                algoBaseSecurityid = algoBaseSecurityid;
                algoMidDetailDefined = algoMidDetailDefined;
                algoMidOffsetValue = algoMidOffsetValue;
                algoValue = algoValue;

                OnPropertyChanged(bidBasePrice);
                OnPropertyChanged(manualBid);
                OnPropertyChanged(bidAlgoDefenValue);
                OnPropertyChanged(bidAlgoSafeValue);
                OnPropertyChanged(bidAlgoTrigValue);
                OnPropertyChanged(algoMidType);
                OnPropertyChanged(algoBaseSecurityid);
                OnPropertyChanged(algoMidDetailDefined);
                OnPropertyChanged(algoMidOffsetValue);
                OnPropertyChanged(algoValue);
                if (value.Equals("多")) 
                {
                    bidQuoteModelImageVis = "0";
                }
            }
        }

        //手动报价默认拉宽或者收窄点差
        public string defaultManualSpread
        {
            get { return _MMOrderParamReq.defaultManualSpread; }
            set
            {
                _MMOrderParamReq.defaultManualSpread = value;
                this.OnPropertyChanged();
            }
        }

        //Bid单笔报量
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBaseQty
        {
            get
            {
                return _MMOrderParamReq.bidBaseQty;
            }

            set
            {
                _MMOrderParamReq.bidBaseQty = value;
                this.OnPropertyChanged();
            }
        }

        //Bid单笔报价
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBasePrice
        {
            get { return _MMOrderParamReq.bidBasePrice; }
            set
            {
                _MMOrderParamReq.bidBasePrice = value;
                this.OnPropertyChanged();
            }
        }

        //Bid重发阈值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBaseReSendValue
        {
            get { return _MMOrderParamReq.bidBaseReSendValue; }

            set
            {
                _MMOrderParamReq.bidBaseReSendValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid拉宽值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBaseWidenValue
        {
            get { return _MMOrderParamReq.bidBaseWidenValue; }

            set
            {
                _MMOrderParamReq.bidBaseWidenValue = value;
                this.OnPropertyChanged();
            }
        }

        //日成交限额
        public string BaseERValue
        {
            get { return _MMOrderParamReq.BaseERValue; }

            set
            {
                _MMOrderParamReq.BaseERValue = value;
                OnPropertyChanged();
            }
        }

        //成交拉宽,1: 按单笔报量拉宽后重发,2: 按剩余挂量拉宽后重发
        public string BaseERReSendType
        {
            get { return _MMOrderParamReq.BaseERReSendType; }

            set
            {
                _MMOrderParamReq.BaseERReSendType = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    BaseERReSendTypeImageVis = "0";
                }
            }
        }

        //清算速度
        public string baseSettleType
        {
            get { return _MMOrderParamReq.baseSettleType; }

            set
            {
                _MMOrderParamReq.baseSettleType = value;
                this.OnPropertyChanged();
            }
        }


        //ofr价格优先级：1防守，2算法
        public string ofrBasePriority
        {
            get { return _MMOrderParamReq.ofrBasePriority; }

            set
            {
                _MMOrderParamReq.ofrBasePriority = value;
                this.OnPropertyChanged();
            }
        }

        //ofr防守报价
        public string ofrDefenCalPrice
        {
            get { return _MMOrderParamReq.ofrDefenCalPrice; }
            set
            {
                _MMOrderParamReq.ofrDefenCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr算法报价
        public string ofrAlgoCalPrice
        {
            get { return _MMOrderParamReq.ofrAlgoCalPrice; }
            set
            {
                _MMOrderParamReq.ofrAlgoCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr方向报价模式1:手动、2:自动
        public string ofrQuoteModel
        {
            get { return _MMOrderParamReq.ofrQuoteModel; }
            set
            {
                _MMOrderParamReq.ofrQuoteModel = value;
                this.OnPropertyChanged();

                ofrBasePrice = ofrBasePrice;
                manualOfr = manualOfr;
                ofrAlgoDefenValue = ofrAlgoDefenValue;
                ofrAlgoSafeValue = ofrAlgoSafeValue;
                ofrAlgoTrigValue = ofrAlgoTrigValue;
                algoMidType = algoMidType;
                algoBaseSecurityid = algoBaseSecurityid;
                algoMidDetailDefined = algoMidDetailDefined;
                algoMidOffsetValue = algoMidOffsetValue;
                algoValue = algoValue;

                OnPropertyChanged(ofrBasePrice);
                OnPropertyChanged(manualOfr);
                OnPropertyChanged(ofrAlgoDefenValue);
                OnPropertyChanged(ofrAlgoSafeValue);
                OnPropertyChanged(ofrAlgoTrigValue);
                OnPropertyChanged(algoMidType);
                OnPropertyChanged(algoBaseSecurityid);
                OnPropertyChanged(algoMidDetailDefined);
                OnPropertyChanged(algoMidOffsetValue);
                OnPropertyChanged(algoValue);
                if (value.Equals("多")) 
                {
                    ofrQuoteModelImageVis = "0";
                }
            }
        }

        //ofr单笔报量
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBaseQty
        {
            get { return _MMOrderParamReq.ofrBaseQty; }

            set
            {
                _MMOrderParamReq.ofrBaseQty = value;
                this.OnPropertyChanged();
            }
        }

        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBasePrice
        {
            get { return _MMOrderParamReq.ofrBasePrice; }
            set
            {
                _MMOrderParamReq.ofrBasePrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr重发阈值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBaseReSendValue
        {
            get { return _MMOrderParamReq.ofrBaseReSendValue; }

            set
            {
                _MMOrderParamReq.ofrBaseReSendValue = value;
                this.OnPropertyChanged();
            }
        }

        //ofr拉宽值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBaseWidenValue
        {
            get { return _MMOrderParamReq.ofrBaseWidenValue; }

            set
            {
                _MMOrderParamReq.ofrBaseWidenValue = value;
                this.OnPropertyChanged();
            }
        }

        //bid算法防守点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoDefenValue
        {
            get { return _MMOrderParamReq.bidAlgoDefenValue; }

            set
            {
                _MMOrderParamReq.bidAlgoDefenValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid算法安全点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoSafeValue
        {
            get { return _MMOrderParamReq.bidAlgoSafeValue; }

            set
            {
                _MMOrderParamReq.bidAlgoSafeValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid算法安全触发点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoTrigValue
        {
            get { return _MMOrderParamReq.bidAlgoTrigValue; }

            set
            {
                _MMOrderParamReq.bidAlgoTrigValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr算法防守点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoDefenValue
        {
            get { return _MMOrderParamReq.ofrAlgoDefenValue; }

            set
            {
                _MMOrderParamReq.ofrAlgoDefenValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr算法安全点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoSafeValue
        {
            get { return _MMOrderParamReq.ofrAlgoSafeValue; }

            set
            {
                _MMOrderParamReq.ofrAlgoSafeValue = value;
                this.OnPropertyChanged();

            }
        }

        //Ofr算法安全触发点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoTrigValue
        {
            get { return _MMOrderParamReq.ofrAlgoTrigValue; }

            set
            {
                _MMOrderParamReq.ofrAlgoTrigValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用算法计算
        public string algoEnable
        {
            get { return _MMOrderParamReq.algoEnable; }

            set
            {
                _MMOrderParamReq.algoEnable = value;
                this.OnPropertyChanged();
            }
        }

        //是否使用基准债券0：否、1：是
        public string algoBaseEnable
        {
            get { return _MMOrderParamReq.algoBaseEnable; }

            set
            {
                _MMOrderParamReq.algoBaseEnable = value;
                this.OnPropertyChanged();

                OnPropertyChanged(algoBaseSecurityid);
            }
        }

        //使用基准债券
        public string algoBaseSecurityid
        {
            get { return _MMOrderParamReq.algoBaseSecurityid; }
            set
            {
                _MMOrderParamReq.algoBaseSecurityid = value;
                this.OnPropertyChanged();
            }
        }

        //参考中间价:1 双边市场中间价，2：中债估值，3：自定义
        public string algoMidType
        {
            get { return _MMOrderParamReq.algoMidType; }

            set
            {
                _MMOrderParamReq.algoMidType = value;
                this.OnPropertyChanged();

                if ("多".Equals(value))
                {
                    algoMidTypeImageVis = "0";
                }
            }
        }

        //参考中间价详情
        public string algoMidDetail
        {
            get { return _MMOrderParamReq.algoMidDetail; }

            set
            {
                _MMOrderParamReq.algoMidDetail = value;
                this.OnPropertyChanged();
            }
        }

        //参考中间价—自定义详情
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoMidDetailDefined
        {
            get { return _MMOrderParamReq.algoMidDetailDefined; }

            set
            {
                _MMOrderParamReq.algoMidDetailDefined = value;
                this.OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    extAlgoMidDetailDefined = MMCommonUtils.CalBond(securityid, "4", baseSettleType, algoMidDetailDefined, "1");
                }
            }
        }

        //中间价偏离值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoMidOffsetValue
        {
            get { return _MMOrderParamReq.algoMidOffsetValue; }

            set
            {
                _MMOrderParamReq.algoMidOffsetValue = value;
                this.OnPropertyChanged();
            }
        }

        //算法点差
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoValue
        {
            get { return _MMOrderParamReq.algoValue; }

            set
            {
                _MMOrderParamReq.algoValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用中债估值
        public string algoCDCEnable
        {
            get { return _MMOrderParamReq.algoCDCEnable; }

            set
            {
                _MMOrderParamReq.algoCDCEnable = value;
                this.OnPropertyChanged();
            }
        }

        //中债估值详情
        public string algoCDCDetail
        {
            get { return _MMOrderParamReq.algoCDCDetail; }

            set
            {
                _MMOrderParamReq.algoCDCDetail = value;
                this.OnPropertyChanged();
            }
        }

        //Bid中债估值点差
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoCDCValue
        {
            get { return _MMOrderParamReq.bidAlgoCDCValue; }

            set
            {
                _MMOrderParamReq.bidAlgoCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr中债估值点差
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoCDCValue
        {
            get { return _MMOrderParamReq.ofrAlgoCDCValue; }

            set
            {
                _MMOrderParamReq.ofrAlgoCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用积极报价
        public string posLmtEnable
        {
            get { return _MMOrderParamReq.posLmtEnable; }

            set
            {
                _MMOrderParamReq.posLmtEnable = value;
                this.OnPropertyChanged();
            }
        }

        //Bid积极报价值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidPosValue
        {
            get { return _MMOrderParamReq.bidPosValue; }

            set
            {
                _MMOrderParamReq.bidPosValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr积极报价值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrPosValue
        {
            get { return _MMOrderParamReq.ofrPosValue; }

            set
            {
                _MMOrderParamReq.ofrPosValue = value;
                this.OnPropertyChanged();
            }
        }

        //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
        public string posFakEnable
        {
            get { return _MMOrderParamReq.posFakEnable; }

            set
            {
                _MMOrderParamReq.posFakEnable = value;
                if (!string.IsNullOrEmpty(_MMOrderParamReq.posFakEnable) && _MMOrderParamReq.posFakEnable.Equals("1"))
                {
                    posFakSpreadEnable = true;
                }
                else 
                {
                    posFakSpreadEnable = false;
                }
                this.OnPropertyChanged();
                OnPropertyChanged("posFakSpread");
                if ("多".Equals(value))
                {
                    posFakEnableImageVis = "0";
                }
            }
        }

        public bool posFakSpreadEnable
        {
            get { return _MMOrderParamReq.posFakSpreadEnable; }

            set
            {
                _MMOrderParamReq.posFakSpreadEnable = value;
                this.OnPropertyChanged();
            }
        }
        //点价点差
        public string posFakSpread
        {
            get { return _MMOrderParamReq.posFakSpread; }

            set
            {
                _MMOrderParamReq.posFakSpread = value;
                this.OnPropertyChanged();
            }
        }

        // 成交拉宽时长下限 不填写默认为 5  单位：分钟   [ 1, 99]
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string dealWidenLowerTime
        {
            get { return this._MMOrderParamReq.dealWidenLowerTime; }
            set
            {
                _MMOrderParamReq.dealWidenLowerTime = value;
                OnPropertyChanged();
            }
        }

        // 成交拉宽时长上限,单位：分钟 不填写默认为 5 [ 5, 99]
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string dealWidenUpperTime
        {
            get { return _MMOrderParamReq.dealWidenUpperTime; }
            set
            {
                _MMOrderParamReq.dealWidenUpperTime = value;
                OnPropertyChanged();
            }
        }

        //48.全局强墙Bid
        public string globalWallBid
        {
            get { return _MMOrderParamReq.globalWallBid; }
            set
            {
                _MMOrderParamReq.globalWallBid = value;
                OnPropertyChanged();
            }
        }

        //49.全局强墙Bid
        public string globalWallOfr
        {
            get { return _MMOrderParamReq.globalWallOfr; }
            set
            {
                _MMOrderParamReq.globalWallOfr = value;
                OnPropertyChanged();
            }
        }

        //50.Bid策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),
        public string bidStrategyType
        {
            get { return _MMOrderParamReq.bidStrategyType; }
            set
            {
                _MMOrderParamReq.bidStrategyType = value;
                OnPropertyChanged();
            }
        }

        //51.Ofr策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),
        public string ofrStrategyType
        {
            get { return _MMOrderParamReq.ofrStrategyType; }
            set
            {
                _MMOrderParamReq.ofrStrategyType = value;
                OnPropertyChanged();
            }
        }

        //52.接口bid 价格
        public string apiBid
        {
            get { return _MMOrderParamReq.apiBid; }
            set
            {
                _MMOrderParamReq.apiBid = value;
                OnPropertyChanged();
            }
        }

        //53.接口ofr价格
        public string apiOfr
        {
            get { return _MMOrderParamReq.apiOfr; }
            set
            {
                _MMOrderParamReq.apiOfr = value;
                OnPropertyChanged();
            }
        }

        //54.接口Bid报量
        public string apiBidQty
        {
            get { return _MMOrderParamReq.apiBidQty; }
            set
            {
                _MMOrderParamReq.apiBidQty = value;
                OnPropertyChanged();
            }
        }

        //55.接口Ofr报量
        public string apiOfrQty
        {
            get { return _MMOrderParamReq.apiOfrQty; }
            set
            {
                _MMOrderParamReq.apiOfrQty = value;
                OnPropertyChanged();
            }
        }

        //56.只读显示，展示接收到接口数据的最新时间戳
        public string apiLastTime
        {
            get { return _MMOrderParamReq.apiLastTime; }
            set
            {
                _MMOrderParamReq.apiLastTime = value;
                OnPropertyChanged();
            }
        }

        //57.bid手动报价
        public string manualBid
        {
            get { return _MMOrderParamReq.manualBid; }
            set
            {
                _MMOrderParamReq.manualBid = value;
                OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    exeManualBid = MMCommonUtils.CalBond(securityid, "4", baseSettleType, manualBid, "1");
                }
            }
        }

        //58.ofr手动报价
        public string manualOfr
        {
            get { return _MMOrderParamReq.manualOfr; }
            set
            {
                _MMOrderParamReq.manualOfr = value;
                OnPropertyChanged();

                if (("1").Equals(exeBondPre)) 
                {
                    exeManualOfr = MMCommonUtils.CalBond(securityid, "4", baseSettleType, manualOfr, "1");
                }
            }
        }

        //59.挂单成功标志（1成功，0失败）
        public string isQuoteSuccess
        {
            get { return _MMOrderParamReq.isQuoteSuccess; }
            set
            {
                _MMOrderParamReq.isQuoteSuccess = value;
                OnPropertyChanged();
            }
        }

        //60.最后操作时间
        public string lastOptTime
        {
            get { return _MMOrderParamReq.lastOptTime; }
            set
            {
                _MMOrderParamReq.lastOptTime = value;
                OnPropertyChanged();
            }
        }

        //61.外面显示的报量（策略小窗)
        public string displayBidQty
        {
            get { return _MMOrderParamReq.displayBidQty; }
            set
            {
                _MMOrderParamReq.displayBidQty = value;
                OnPropertyChanged();
            }
        }

        //62.外面显示的报量（策略小窗)
        public string displayOfrQty
        {
            get { return _MMOrderParamReq.displayOfrQty; }
            set
            {
                _MMOrderParamReq.displayOfrQty = value;
                OnPropertyChanged();
            }
        }

        public string stepLength
        {
            get { return _MMOrderParamReq.stepLength; }

            set
            {
                _MMOrderParamReq.stepLength = value;
                this.OnPropertyChanged();
            }
        }

        //是否使用冰山量
        public string maxFloorEnable
        {
            get { return _MMOrderParamReq.maxFloorEnable; }

            set
            {
                _MMOrderParamReq.maxFloorEnable = value;
                if (!string.IsNullOrEmpty(_MMOrderParamReq.maxFloorEnable) && _MMOrderParamReq.maxFloorEnable.Equals("1"))
                {
                    maxFloorIsEnable = true;
                }
                else
                {
                    maxFloorIsEnable = false;
                }
                this.OnPropertyChanged();
                OnPropertyChanged("maxFloor");
                if ("多".Equals(value)) 
                {
                    maxFloorEnableImageVis = "0";
                }
            }
        }
        //是否使用冰山量
        public bool maxFloorIsEnable
        {
            get { return _MMOrderParamReq.maxFloorIsEnable; }

            set
            {
                _MMOrderParamReq.maxFloorIsEnable = value;
                this.OnPropertyChanged();
            }
        }
        //冰山量
        public string maxFloor
        {
            get { return _MMOrderParamReq.maxFloor; }

            set
            {
                _MMOrderParamReq.maxFloor = value;
                this.OnPropertyChanged();
            }
        }
        //是否报指示性
        public string openQDM2Enable
        {
            get { return _MMOrderParamReq.openQDM2Enable; }

            set
            {
                _MMOrderParamReq.openQDM2Enable = value;
                this.OnPropertyChanged();
            }
        }


        //是否防守策略无可报价格时锚定中债估值
        public string defenCDCEnable
        {
            get { return _MMOrderParamReq.defenCDCEnable; }

            set
            {
                _MMOrderParamReq.defenCDCEnable = value;
                if (!string.IsNullOrEmpty(_MMOrderParamReq.defenCDCEnable) && _MMOrderParamReq.defenCDCEnable.Equals("1"))
                {
                    defenCDCIsEnable = true;
                }
                else
                {
                    defenCDCIsEnable = false;
                }
                this.OnPropertyChanged();
                OnPropertyChanged("bidDefenCDCValue");
                OnPropertyChanged("ofrDefenCDCValue");
                if ("多".Equals(value))
                {
                    defenCDCEnableImageVis = "0";
                }
            }
        }
        //是否防守策略无可报价格时锚定中债估值
        public bool defenCDCIsEnable
        {
            get { return _MMOrderParamReq.defenCDCIsEnable; }

            set
            {
                _MMOrderParamReq.defenCDCIsEnable = value;
                this.OnPropertyChanged();
            }
        }
        //估值拉宽Bid
        public string bidDefenCDCValue
        {
            get { return _MMOrderParamReq.bidDefenCDCValue; }

            set
            {
                _MMOrderParamReq.bidDefenCDCValue = value;
                this.OnPropertyChanged();
            }
        }
        //估值拉宽Ofr
        public string ofrDefenCDCValue
        {
            get { return _MMOrderParamReq.ofrDefenCDCValue; }

            set
            {
                _MMOrderParamReq.ofrDefenCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //中债估值详情
        public string defenCDCDetail
        {
            get { return _MMOrderParamReq.defenCDCDetail; }

            set
            {
                _MMOrderParamReq.defenCDCDetail = value;
                this.OnPropertyChanged();
            }
        }



        //bid防守参考市场价类型 1-双边 2-XBOND 3-全市场
        public string bidDefenReferMarketType
        {
            get { return _MMOrderParamReq.bidDefenReferMarketType; }

            set
            {
                _MMOrderParamReq.bidDefenReferMarketType = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    bidDefenReferImageVis = "0";
                }
            }
        }

        //bid防守参考市场价参考档位 1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
        public string bidDefenReferLevel
        {
            get { return _MMOrderParamReq.bidDefenReferLevel; }

            set
            {
                _MMOrderParamReq.bidDefenReferLevel = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    bidDefenReferImageVis = "0";
                }
            }
        }

        //ofr防守参考市场价类型 1-双边 2-XBOND 3-全市场
        public string ofrDefenReferMarketType
        {
            get { return _MMOrderParamReq.ofrDefenReferMarketType; }

            set
            {
                _MMOrderParamReq.ofrDefenReferMarketType = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    ofrDefenReferImageVis = "0";
                }
            }
        }

        //ofr防守参考市场价参考档位 1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
        public string ofrDefenReferLevel
        {
            get { return _MMOrderParamReq.ofrDefenReferLevel; }

            set
            {
                _MMOrderParamReq.ofrDefenReferLevel = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    ofrDefenReferImageVis = "0";
                }
            }
        }

        //是否启用墙0：否、1：是
        public string wallEnable
        {
            get { return _MMOrderParamReq.wallEnable; }

            set
            {
                _MMOrderParamReq.wallEnable = value;
                //if (!string.IsNullOrEmpty(_MMOrderParamReq.wallEnable) && _MMOrderParamReq.wallEnable.Equals("1"))
                //{
                //    IswallEnable = true;
                //}
                //else
                //{
                //    IswallEnable = false;
                //}
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    wallEnableImageVis = "0";
                }
            }
        }

        //是否启用墙
        public bool IswallEnable
        {
            get { return _MMOrderParamReq.IswallEnable; }

            set
            {
                _MMOrderParamReq.IswallEnable = value;
                this.OnPropertyChanged();
            }
        }

        //自定义
        public bool IsResverse
        {
            get
            {
                if (string.IsNullOrEmpty(bidBasePrice) || string.IsNullOrEmpty(ofrBasePrice))
                    return false;

                if (decimal.Parse(bidBasePrice) <= decimal.Parse(ofrBasePrice))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //自定义
        private Dictionary<string, string> _bidStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> bidStrategyModeList
        {
            get
            {
                _bidStrategyModeList.Clear();

                //if (!string.IsNullOrEmpty(bidBasePrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(manualBid, out value)) value = 0;
                    _bidStrategyModeList.Add("Manual", "手动" + ":" + (string.IsNullOrEmpty(manualBid) ?"--": MMCommonUtils.Format4Digit(manualBid)));
                }

                //if (!string.IsNullOrEmpty(bidDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(bidDefenCalPrice, out value)) value = 0;
                    _bidStrategyModeList.Add("Defend", "防守" + ":" + (string.IsNullOrEmpty(bidDefenCalPrice) ? "--" : MMCommonUtils.Format4Digit(bidDefenCalPrice)));
                }

                //if (!string.IsNullOrEmpty(bidAlgoCalPrice))
                {
                    decimal value = 0;
                    if (!decimal.TryParse(bidAlgoCalPrice, out value)) value = 0;
                    _bidStrategyModeList.Add("Algorithm", "算法" + ":" + (string.IsNullOrEmpty(bidAlgoCalPrice) ? "--" : MMCommonUtils.Format4Digit(bidAlgoCalPrice)));
                }

                if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                {
                    //decimal value = 0;
                    //decimal qty = 0;
                    //if (!decimal.TryParse(apiBid, out value)) value = 0;
                    //if (!decimal.TryParse(apiBidQty, out qty)) qty = 0;
                    _bidStrategyModeList.Add("API","接口 " + ":" + (string.IsNullOrEmpty(apiBid) ? "--": MMCommonUtils.Format4Digit(apiBid)) + "(" + (string.IsNullOrEmpty(apiBidQty) ? "--": apiBidQty) + "KW)");
                }

                return _bidStrategyModeList;
            }
        }

        //自定义
        private Dictionary<string, string> _ofrStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> ofrStrategyModeList
        {
            get
            {
                _ofrStrategyModeList.Clear();

                //if (!string.IsNullOrEmpty(ofrBasePrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(manualOfr, out value)) value = 0;
                    _ofrStrategyModeList.Add("Manual", "手动" + ":" + (string.IsNullOrEmpty(manualOfr) ? "--" : MMCommonUtils.Format4Digit(manualOfr)));
                }

                //if (!string.IsNullOrEmpty(ofrDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(ofrDefenCalPrice, out value)) value = 0;
                    _ofrStrategyModeList.Add("Defend", "防守" + ":" + (string.IsNullOrEmpty(ofrDefenCalPrice) ? "--" : MMCommonUtils.Format4Digit(ofrDefenCalPrice)));
                }

                //if (!string.IsNullOrEmpty(ofrAlgoCalPrice))
                {
                    //decimal value = 0;
                    //if (!decimal.TryParse(ofrAlgoCalPrice, out value)) value = 0;
                    _ofrStrategyModeList.Add("Algorithm", "算法" + ":" + (string.IsNullOrEmpty(ofrAlgoCalPrice) ? "--" : MMCommonUtils.Format4Digit(ofrAlgoCalPrice)));
                }
                if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                {
                    //decimal value = 0;
                    //decimal qty = 0;
                    //if (!decimal.TryParse(apiOfr, out value)) value = 0;
                    //if (!decimal.TryParse(apiOfrQty, out qty)) qty = 0;
                    _ofrStrategyModeList.Add("API", "接口" + ":" + (string.IsNullOrEmpty(apiOfr) ? "--" : MMCommonUtils.Format4Digit(apiOfr)) + "(" + (string.IsNullOrEmpty(apiOfrQty) ? "--" : apiOfrQty) + "KW)");
                }
                return _ofrStrategyModeList;
            }
        }

        //自定义 快捷调价按钮是否可用
        private bool _IsEnabledClick = true;
        public bool IsEnabledClick
        {
            get {
                if (string.IsNullOrEmpty(_MMOrderParamReq.cDCValueType))
                {
                    return true;
                }
                else
                {
                    if ("1".Equals(_MMOrderParamReq.cDCValueType))
                    {
                       return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            /*get
            {
                _MMOrderParamReq.IsEnabledClick = value;
                this.OnPropertyChanged();

                *//*if (bidQuoteModel == "Defend" || ofrQuoteModel == "Defend")
                {
                    if (string.IsNullOrEmpty(bidDefenCalPrice) || string.IsNullOrEmpty(ofrDefenCalPrice))
                    {
                        _IsEnabledClick = false;
                    }
                }

                if (bidQuoteModel == "Algorithm" || ofrQuoteModel == "Algorithm")
                {
                    if (string.IsNullOrEmpty(bidAlgoCalPrice) || string.IsNullOrEmpty(ofrAlgoCalPrice))
                    {
                        _IsEnabledClick = false;
                    }
                }

                if (bidQuoteModel == "API" || ofrQuoteModel == "API")
                {
                    decimal apiBidQty_d;
                    decimal apiOfrQty_d;
                    decimal.TryParse(apiBidQty, out apiBidQty_d);
                    decimal.TryParse(apiOfrQty, out apiOfrQty_d);

                    if (string.IsNullOrEmpty(apiBid) || string.IsNullOrEmpty(apiOfr) || apiBidQty_d <= 0 || apiOfrQty_d <= 0)
                    {
                        _IsEnabledClick = false;
                    }
                }*//*
            }*/
        }

        //自定义 (bid-ofr)* 100
        private string _bidOfrDiff = string.Empty;
        public string bidOfrDiff
        {
            get
            {
                string diff = string.Empty;

                if (!string.IsNullOrEmpty(bidBasePrice) && !string.IsNullOrEmpty(ofrBasePrice))
                {
                    decimal bidValue;
                    decimal ofrValue;
                    decimal diffValue;
                    if (decimal.TryParse(bidBasePrice,out bidValue) && decimal.TryParse(ofrBasePrice,out ofrValue))
                    {
                        diffValue = (bidValue - ofrValue) * 100;
                        diff = diffValue.ToString("0.##");
                    }
                }
                return diff;
            }
        }

        private string _bidOfrDiffToolTip = string.Empty;
        public string bidOfrDiffToolTip
        {
            get
            {
                string text = bidOfrDiff;

                if (string.IsNullOrEmpty(bidOfrDiff))
                {
                    text = "--";
                }
                else
                {
                    text = bidOfrDiff + "bp";
                }

                return text;
            }
        }

        private List<QuoteModel> _quoteModelList = new List<QuoteModel>();
        public List<QuoteModel> quoteModelList
        {
            get
            {
                if (_quoteModelList.Count == 0)
                {
                    _quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Manual"],
                        ofr = ofrStrategyModeList["Manual"],
                    });

                    _quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Defend"],
                        ofr = ofrStrategyModeList["Defend"],
                    });

                    _quoteModelList.Add(new QuoteModel()
                    {
                        bid = bidStrategyModeList["Algorithm"],
                        ofr = ofrStrategyModeList["Algorithm"],
                    });
                    if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                    {
                        _quoteModelList.Add(new QuoteModel()
                        {
                            bid = bidStrategyModeList["API"],
                            ofr = ofrStrategyModeList["API"],
                        });
                    }
                }
                return _quoteModelList;
            }
    }
        //中债估值类型1-行权；2-到期（或者空）
        public string CDCValueType
        {
            get { return _MMOrderParamReq.cDCValueType; }

            set
            {
                _MMOrderParamReq.cDCValueType = value;
                if ("1".Equals(_MMOrderParamReq.cDCValueType))
                {
                    isWallEable = "0";
                }
                else
                {
                    isWallEable = "1";
                }
                this.OnPropertyChanged();
            }
        }

        //含权债不能启用编辑墙1-可编辑0-不可编辑
        public string isWallEable
        {
            get { return _MMOrderParamReq.isWallEable; }

            set
            {
                _MMOrderParamReq.isWallEable = value;
                this.OnPropertyChanged();
            }
        }

        //当前策略bid价格计算详情
        public string bidCalDetail {

            get { return _MMOrderParamReq.bidCalDetail; }

            set
            {
                _MMOrderParamReq.bidCalDetail = value;
                this.OnPropertyChanged();
            }
        }

        //当前策略ofr价格计算详情
        public string ofrCalDetail
        {

            get { return _MMOrderParamReq.ofrCalDetail; }

            set
            {
                _MMOrderParamReq.ofrCalDetail = value;
                this.OnPropertyChanged();
            }
        }
        public string exeManualBid
        {
            get { return _MMOrderParamReq.exeManualBid; }

            set
            {
                _MMOrderParamReq.exeManualBid = value;
                this.OnPropertyChanged();
                if (("0").Equals(exeBondPre))
                {
                    manualBid = MMCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualBid, "1");
                }
            }
        }

        public string exeManualOfr
        {
            get { return _MMOrderParamReq.exeManualOfr; }

            set
            {
                _MMOrderParamReq.exeManualOfr = value;
                this.OnPropertyChanged();
                if (("0").Equals(exeBondPre))
                {
                    manualOfr = MMCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualOfr, "2");
                }
            }
        }
        public string extAlgoMidDetailDefined
        {
            get { return _MMOrderParamReq.extAlgoMidDetailDefined; }

            set
            {
                _MMOrderParamReq.extAlgoMidDetailDefined = value;
                this.OnPropertyChanged();
                if (("0").Equals(exeBondPre))
                {
                    algoMidDetailDefined = MMCommonUtils.CalBond(securityid, "7", baseSettleType, extAlgoMidDetailDefined, "2");
                }
            }
        }
        //含权债偏好
        public string exeBondPre
        {
            get { return _MMOrderParamReq.exeBondPre; }

            set
            {
                _MMOrderParamReq.exeBondPre = value;
                this.OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    exeManualBid = MMCommonUtils.CalBond(securityid, "4", baseSettleType, manualBid, "1");
                    exeManualOfr = MMCommonUtils.CalBond(securityid, "4", baseSettleType, manualOfr, "2");
                    //extAlgoMidDetailDefined = MMCommonUtils.CalBond(securityid, "4", baseSettleType, algoMidDetailDefined, "1");
                }
                else if (("0").Equals(exeBondPre))
                {
                    manualBid = MMCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualBid, "1");
                    manualOfr = MMCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualOfr, "2");
                    //algoMidDetailDefined = MMCommonUtils.CalBond(securityid, "7", baseSettleType, extAlgoMidDetailDefined, "1");
                }
            }
        }

        //含权债跟随
        public string exeBondFollow
        {
            get { return _MMOrderParamReq.exeBondFollow; }

            set
            {
                _MMOrderParamReq.exeBondFollow = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidQuoteModelImageVis = "1";
        public string bidQuoteModelImageVis 
        {
            get { return _bidQuoteModelImageVis; }
            set 
            { 
                _bidQuoteModelImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _ofrQuoteModelImageVis = "1";
        public string ofrQuoteModelImageVis
        {
            get { return _ofrQuoteModelImageVis; }
            set
            {
                _ofrQuoteModelImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _settlTypeImageVis = "1";
        public string settlTypeImageVis
        {
            get { return _settlTypeImageVis; }
            set
            {
                _settlTypeImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _maxFloorEnableImageVis = "1";
        public string maxFloorEnableImageVis
        {
            get { return _maxFloorEnableImageVis; }
            set
            {
                _maxFloorEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _bidDefenReferImageVis = "1";
        public string bidDefenReferImageVis
        {
            get { return _bidDefenReferImageVis; }
            set
            {
                _bidDefenReferImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _ofrDefenReferImageVis = "1";
        public string ofrDefenReferImageVis
        {
            get { return _ofrDefenReferImageVis; }
            set
            {
                _ofrDefenReferImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _wallEnableImageVis = "1";
        public string wallEnableImageVis
        {
            get { return _wallEnableImageVis; }
            set
            {
                _wallEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _defenCDCEnableImageVis = "1";
        public string defenCDCEnableImageVis
        {
            get { return _defenCDCEnableImageVis; }
            set
            {
                _defenCDCEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _algoMidTypeImageVis = "1";
        public string algoMidTypeImageVis
        {
            get { return _algoMidTypeImageVis; }
            set
            {
                _algoMidTypeImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _BaseERReSendTypeImageVis = "1";
        public string BaseERReSendTypeImageVis
        {
            get { return _BaseERReSendTypeImageVis; }
            set
            {
                _BaseERReSendTypeImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _posFakEnableImageVis = "1";
        public string posFakEnableImageVis
        {
            get { return _posFakEnableImageVis; }
            set
            {
                _posFakEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        private string _IsInterest;
        public string IsInterest
        {
            get { return _IsInterest; }

            set
            {
                _IsInterest = value;
                this.OnPropertyChanged();
            }
        }
        public string algoSafeEnable
        {
            get { return _MMOrderParamReq.algoSafeEnable; }

            set
            {
                _MMOrderParamReq.algoSafeEnable = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    algoSafeEnableImageVis = "0";
                }
            }
        }
        private string _algoSafeEnableImageVis = "1";
        public string algoSafeEnableImageVis
        {
            get { return _algoSafeEnableImageVis; }
            set
            {
                _algoSafeEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        public string secondSafeEnable
        {
            get { return _MMOrderParamReq.secondSafeEnable; }

            set
            {
                _MMOrderParamReq.secondSafeEnable = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    secondSafeEnableImageVis = "0";
                }
            }
        }
        private string _secondSafeEnableImageVis = "1";
        public string secondSafeEnableImageVis
        {
            get { return _secondSafeEnableImageVis; }
            set
            {
                _secondSafeEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        public string qdmespSafeEnable
        {
            get { return _MMOrderParamReq.qdmespSafeEnable; }

            set
            {
                _MMOrderParamReq.qdmespSafeEnable = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    qdmespSafeEnableImageVis = "0";
                }
            }
        }
        private string _qdmespSafeEnableImageVis = "1";
        public string qdmespSafeEnableImageVis
        {
            get { return _qdmespSafeEnableImageVis; }
            set
            {
                _qdmespSafeEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }
        public string qdmespSafeLevel
        {
            get { return _MMOrderParamReq.qdmespSafeLevel; }

            set
            {
                _MMOrderParamReq.qdmespSafeLevel = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    qdmespSafeLevelImageVis = "0";
                }
            }
        }
        private string _qdmespSafeLevelImageVis = "1";
        public string qdmespSafeLevelImageVis
        {
            get { return _qdmespSafeLevelImageVis; }
            set
            {
                _qdmespSafeLevelImageVis = value;
                this.OnPropertyChanged();
            }
        }
        public string bidQdmespSafeValue
        {
            get { return _MMOrderParamReq.bidQdmespSafeValue; }

            set
            {
                _MMOrderParamReq.bidQdmespSafeValue = value;
                this.OnPropertyChanged();
            }
        }
        public string ofrQdmespSafeValue
        {
            get { return _MMOrderParamReq.ofrQdmespSafeValue; }

            set
            {
                _MMOrderParamReq.ofrQdmespSafeValue = value;
                this.OnPropertyChanged();
            }
        }
        public object Clone()
        {
            OrderParamProp ret = new OrderParamProp(null);
            ret.circulSize = _MMOrderParamReq.circulSize;
            ret.riskMinQtySize = _MMOrderParamReq.riskMinQtySize;
            ret.riskMaxQtyRatio = _MMOrderParamReq.riskMaxQtyRatio;
            ret.securityid = this._MMOrderParamReq.securityid;
            ret.symbol = _MMOrderParamReq.symbol;
            ret.bidBasePriority = this._MMOrderParamReq.bidBasePriority;
            ret.bidDefenCalPrice = this._MMOrderParamReq.bidDefenCalPrice;
            ret.bidAlgoCalPrice = this._MMOrderParamReq.bidAlgoCalPrice;
            ret.bidQuoteModel = this._MMOrderParamReq.bidQuoteModel;
            ret.defaultManualSpread = this._MMOrderParamReq.defaultManualSpread;
            ret.bidBaseQty = this._MMOrderParamReq.bidBaseQty;
            ret.bidBasePrice = this._MMOrderParamReq.bidBasePrice;

            ret.bidBaseReSendValue = this._MMOrderParamReq.bidBaseReSendValue;
            ret.bidBaseWidenValue = this._MMOrderParamReq.bidBaseWidenValue;
            ret.BaseERValue = this._MMOrderParamReq.BaseERValue;
            ret.BaseERReSendType = this._MMOrderParamReq.BaseERReSendType;
            ret.baseSettleType = this._MMOrderParamReq.baseSettleType;
            ret.ofrBasePriority = this._MMOrderParamReq.ofrBasePriority;
            ret.ofrDefenCalPrice = this._MMOrderParamReq.ofrDefenCalPrice;
            ret.ofrAlgoCalPrice = this._MMOrderParamReq.ofrAlgoCalPrice;
            ret.ofrQuoteModel = this._MMOrderParamReq.ofrQuoteModel;
            ret.ofrBaseQty = this._MMOrderParamReq.ofrBaseQty;

            ret.ofrBasePrice = this._MMOrderParamReq.ofrBasePrice;
            ret.ofrBaseReSendValue = this._MMOrderParamReq.ofrBaseReSendValue;
            ret.ofrBaseWidenValue = this._MMOrderParamReq.ofrBaseWidenValue;
            ret.bidAlgoDefenValue = this._MMOrderParamReq.bidAlgoDefenValue;

            ret.algoSafeEnable = this._MMOrderParamReq.algoSafeEnable;
            ret.bidAlgoSafeValue = this._MMOrderParamReq.bidAlgoSafeValue;
            ret.ofrAlgoSafeValue = this._MMOrderParamReq.bidAlgoSafeValue;
            ret.secondSafeEnable = this._MMOrderParamReq.secondSafeEnable;
            ret.qdmespSafeEnable = this._MMOrderParamReq.qdmespSafeEnable;
            ret.qdmespSafeLevel = this._MMOrderParamReq.qdmespSafeLevel;
            ret.bidQdmespSafeValue = this._MMOrderParamReq.bidQdmespSafeValue;
            ret.ofrQdmespSafeValue = this._MMOrderParamReq.bidQdmespSafeValue;

            ret.bidAlgoTrigValue = this._MMOrderParamReq.bidAlgoTrigValue;
            ret.ofrAlgoDefenValue = this._MMOrderParamReq.ofrAlgoDefenValue;
            ret.ofrAlgoTrigValue = this._MMOrderParamReq.ofrAlgoTrigValue;
            ret.algoEnable = this._MMOrderParamReq.algoEnable;

            ret.algoBaseEnable = this._MMOrderParamReq.algoBaseEnable;
            ret.algoBaseSecurityid = this._MMOrderParamReq.algoBaseSecurityid;
            ret.algoMidType = this._MMOrderParamReq.algoMidType;
            ret.algoMidDetail = this._MMOrderParamReq.algoMidDetail;
            ret.algoMidDetailDefined = this._MMOrderParamReq.algoMidDetailDefined;
            ret.algoMidOffsetValue = this._MMOrderParamReq.algoMidOffsetValue;
            ret.algoValue = this._MMOrderParamReq.algoValue;
            ret.algoCDCEnable = this._MMOrderParamReq.algoCDCEnable;
            ret.algoCDCDetail = this._MMOrderParamReq.algoCDCDetail;
            ret.bidAlgoCDCValue = this._MMOrderParamReq.bidAlgoCDCValue;

            ret.ofrAlgoCDCValue = this._MMOrderParamReq.ofrAlgoCDCValue;
            ret.posLmtEnable = this._MMOrderParamReq.posLmtEnable;
            ret.bidPosValue = this._MMOrderParamReq.bidPosValue;
            ret.ofrPosValue = this._MMOrderParamReq.ofrPosValue;
            ret.posFakEnable = this._MMOrderParamReq.posFakEnable;
            ret.posFakSpread = this._MMOrderParamReq.posFakSpread;
            ret.dealWidenLowerTime = _MMOrderParamReq.dealWidenLowerTime;
            ret.dealWidenUpperTime = _MMOrderParamReq.dealWidenUpperTime;
            ret.globalWallBid = _MMOrderParamReq.globalWallBid;
            ret.globalWallOfr = _MMOrderParamReq.globalWallOfr;
            ret.bidStrategyType = _MMOrderParamReq.bidStrategyType;

            ret.ofrStrategyType = _MMOrderParamReq.ofrStrategyType;
            ret.apiBid = _MMOrderParamReq.apiBid;
            ret.apiOfr = _MMOrderParamReq.apiOfr;
            ret.apiBidQty = _MMOrderParamReq.apiBidQty;
            ret.apiOfrQty = _MMOrderParamReq.apiOfrQty;
            ret.apiLastTime = _MMOrderParamReq.apiLastTime;
            ret.manualBid = _MMOrderParamReq.manualBid;
            ret.manualOfr = _MMOrderParamReq.manualOfr;
            ret.isQuoteSuccess = _MMOrderParamReq.isQuoteSuccess;
            ret.lastOptTime = _MMOrderParamReq.lastOptTime;

            ret.displayBidQty = _MMOrderParamReq.displayBidQty;
            ret.displayOfrQty = _MMOrderParamReq.displayOfrQty;
            ret.stepLength = _MMOrderParamReq.stepLength;
            ret.maxFloorEnable = _MMOrderParamReq.maxFloorEnable;
            ret.maxFloor = _MMOrderParamReq.maxFloor;
            ret.openQDM2Enable = _MMOrderParamReq.openQDM2Enable;
            ret.defenCDCEnable = _MMOrderParamReq.defenCDCEnable;
            ret.bidDefenCDCValue = _MMOrderParamReq.bidDefenCDCValue;
            ret.ofrDefenCDCValue = _MMOrderParamReq.ofrDefenCDCValue;
            ret.bidDefenReferMarketType = _MMOrderParamReq.bidDefenReferMarketType;
            ret.bidDefenReferLevel = _MMOrderParamReq.bidDefenReferLevel;
            ret.ofrDefenReferMarketType = _MMOrderParamReq.ofrDefenReferMarketType;
            ret.ofrDefenReferLevel = _MMOrderParamReq.ofrDefenReferLevel;
            ret.wallEnable = _MMOrderParamReq.wallEnable;
            ret.CDCValueType = _MMOrderParamReq.cDCValueType;
            ret.bidCalDetail = _MMOrderParamReq.bidCalDetail;
            ret.ofrCalDetail = _MMOrderParamReq.ofrCalDetail;
            ret.exeManualBid = _MMOrderParamReq.exeManualBid;
            ret.exeManualOfr = _MMOrderParamReq.exeManualOfr;
            ret.extAlgoMidDetailDefined = _MMOrderParamReq.extAlgoMidDetailDefined;
            ret.exeBondPre = _MMOrderParamReq.exeBondPre;
            ret.exeBondFollow = _MMOrderParamReq.exeBondFollow;
            ret.IsInterest = IsInterest;
            return ret;
        }
    }

    public class QuoteModel: ViewModelBase
    {
        private string _bid = string.Empty;
        public string bid
        {
            get { return _bid; }
            set
            {
                _bid = value;
                OnPropertyChanged();
            }
        }

        private string _ofr = string.Empty;
        public string ofr
        {
            get { return _ofr; }
            set
            {
                _ofr = value;
                OnPropertyChanged();
            }
        }

        
    }
}
