using STC.Common.Model.MMshe;
using STC.Mmshe.Plugin.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Mmshe.Plugin.Utils;
using STC.Common.Utils;

namespace STC.Mmshe.Plugin.ViewModel
{
    public class OrderParamProp : ViewModelBase, ICloneable
    {
        private MMsheOrderParamReq _MMsheOrderParamReq;

        public MMsheOrderParamReq MMsheOrderParamReq { get { return _MMsheOrderParamReq; } }

        public OrderParamProp(MMsheOrderParamReq param)
        {
            if (param == null)
                _MMsheOrderParamReq = new MMsheOrderParamReq();
            else
                _MMsheOrderParamReq = param;
        }

        //债券余额
        public string circulSize
        {
            get { return _MMsheOrderParamReq.circulSize; }
            set
            {
                _MMsheOrderParamReq.circulSize = value;
                if (!string.IsNullOrEmpty(_MMsheOrderParamReq.riskMaxQtyRatio))
                {
                    circulRatioSize = (_MMsheOrderParamReq.circulSize.ConvertToDecimal() * _MMsheOrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10).ToString("n2");
                }
                OnPropertyChanged();
            }
        }
        //债券余额
        public string circulRatioSize
        {
            get { return _MMsheOrderParamReq.circulRatioSize; }
            set
            {
                _MMsheOrderParamReq.circulRatioSize = value;
                OnPropertyChanged();
            }
        }

        //风控报量下限
        public string riskMinQtySize
        {
            get { return _MMsheOrderParamReq.riskMinQtySize; }
            set
            {
                _MMsheOrderParamReq.riskMinQtySize = value;
                OnPropertyChanged();
            }
        }

        //风控报量上线比例
        public string riskMaxQtyRatio
        {
            get { return _MMsheOrderParamReq.riskMaxQtyRatio; }
            set
            {
                _MMsheOrderParamReq.riskMaxQtyRatio = value;
                if (!string.IsNullOrEmpty(_MMsheOrderParamReq.riskMaxQtyRatio))
                {
                    if (!string.IsNullOrEmpty(_MMsheOrderParamReq.circulSize))
                    {
                        circulRatioSize = (_MMsheOrderParamReq.circulSize.ConvertToDecimal() * _MMsheOrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10).ToString("n2");

                    }
                    else
                    {
                        circulRatioSize = "";
                    }
                }
                OnPropertyChanged();
            }
        }

        public string securityid
        {
            get { return _MMsheOrderParamReq.securityid; }
            set
            {
                _MMsheOrderParamReq.securityid = value;
                this.OnPropertyChanged();
            }
        }

        public string symbol
        {
            get { return _MMsheOrderParamReq.symbol; }
            set
            {
                _MMsheOrderParamReq.symbol = value;
                this.OnPropertyChanged();
            }
        }
        public string algoMidPrice
        {
            get { return _MMsheOrderParamReq.algoMidPrice; }
            set
            {
                _MMsheOrderParamReq.algoMidPrice = value;
                this.OnPropertyChanged();
            }
        }

        //Bid价格优先级：1跟随，2算法
        public string bidBasePriority
        {
            get
            {
                return _MMsheOrderParamReq.bidBasePriority;
            }

            set
            {
                _MMsheOrderParamReq.bidBasePriority = value;
                this.OnPropertyChanged();
            }

        }

        //bid跟随报价
        public string bidDefenCalPrice
        {
            get { return _MMsheOrderParamReq.bidDefenCalPrice; }
            set
            {
                _MMsheOrderParamReq.bidDefenCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //bid算法报价
        public string bidAlgoCalPrice
        {
            get { return _MMsheOrderParamReq.bidAlgoCalPrice; }
            set
            {
                _MMsheOrderParamReq.bidAlgoCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //bid方向报价模式1:手动、2:自动
        public string bidQuoteModel
        {
            get { return _MMsheOrderParamReq.bidQuoteModel; }
            set
            {
                _MMsheOrderParamReq.bidQuoteModel = value;
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
            get { return _MMsheOrderParamReq.defaultManualSpread; }
            set
            {
                _MMsheOrderParamReq.defaultManualSpread = value;
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
                return _MMsheOrderParamReq.bidBaseQty;
            }

            set
            {
                _MMsheOrderParamReq.bidBaseQty = value;
                this.OnPropertyChanged();
            }
        }

        //Bid单笔报价
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBasePrice
        {
            get { return _MMsheOrderParamReq.bidBasePrice; }
            set
            {
                _MMsheOrderParamReq.bidBasePrice = value;
                this.OnPropertyChanged();
            }
        }

        //Bid重发阈值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBaseReSendValue
        {
            get { return _MMsheOrderParamReq.bidBaseReSendValue; }

            set
            {
                _MMsheOrderParamReq.bidBaseReSendValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid拉宽值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidBaseWidenValue
        {
            get { return _MMsheOrderParamReq.bidBaseWidenValue; }

            set
            {
                _MMsheOrderParamReq.bidBaseWidenValue = value;
                this.OnPropertyChanged();
            }
        }

        //日成交限额
        public string BaseERValue
        {
            get { return _MMsheOrderParamReq.BaseERValue; }

            set
            {
                _MMsheOrderParamReq.BaseERValue = value;
                OnPropertyChanged();
            }
        }

        //成交拉宽,1: 按单笔报量拉宽后重发,2: 按剩余挂量拉宽后重发
        public string BaseERReSendType
        {
            get { return _MMsheOrderParamReq.BaseERReSendType; }

            set
            {
                _MMsheOrderParamReq.BaseERReSendType = value;
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
            get { return _MMsheOrderParamReq.baseSettleType; }

            set
            {
                _MMsheOrderParamReq.baseSettleType = value;
                this.OnPropertyChanged();
            }
        }


        //ofr价格优先级：1跟随，2算法
        public string ofrBasePriority
        {
            get { return _MMsheOrderParamReq.ofrBasePriority; }

            set
            {
                _MMsheOrderParamReq.ofrBasePriority = value;
                this.OnPropertyChanged();
            }
        }

        //ofr跟随报价
        public string ofrDefenCalPrice
        {
            get { return _MMsheOrderParamReq.ofrDefenCalPrice; }
            set
            {
                _MMsheOrderParamReq.ofrDefenCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr算法报价
        public string ofrAlgoCalPrice
        {
            get { return _MMsheOrderParamReq.ofrAlgoCalPrice; }
            set
            {
                _MMsheOrderParamReq.ofrAlgoCalPrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr方向报价模式1:手动、2:自动
        public string ofrQuoteModel
        {
            get { return _MMsheOrderParamReq.ofrQuoteModel; }
            set
            {
                _MMsheOrderParamReq.ofrQuoteModel = value;
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
            get { return _MMsheOrderParamReq.ofrBaseQty; }

            set
            {
                _MMsheOrderParamReq.ofrBaseQty = value;
                this.OnPropertyChanged();
            }
        }

        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBasePrice
        {
            get { return _MMsheOrderParamReq.ofrBasePrice; }
            set
            {
                _MMsheOrderParamReq.ofrBasePrice = value;
                this.OnPropertyChanged();
            }
        }

        //ofr重发阈值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBaseReSendValue
        {
            get { return _MMsheOrderParamReq.ofrBaseReSendValue; }

            set
            {
                _MMsheOrderParamReq.ofrBaseReSendValue = value;
                this.OnPropertyChanged();
            }
        }

        //ofr拉宽值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrBaseWidenValue
        {
            get { return _MMsheOrderParamReq.ofrBaseWidenValue; }

            set
            {
                _MMsheOrderParamReq.ofrBaseWidenValue = value;
                this.OnPropertyChanged();
            }
        }

        //bid算法跟随点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoDefenValue
        {
            get { return _MMsheOrderParamReq.bidAlgoDefenValue; }

            set
            {
                _MMsheOrderParamReq.bidAlgoDefenValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid算法安全点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoSafeValue
        {
            get { return _MMsheOrderParamReq.bidAlgoSafeValue; }

            set
            {
                _MMsheOrderParamReq.bidAlgoSafeValue = value;
                this.OnPropertyChanged();
            }
        }

        //Bid算法安全触发点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoTrigValue
        {
            get { return _MMsheOrderParamReq.bidAlgoTrigValue; }

            set
            {
                _MMsheOrderParamReq.bidAlgoTrigValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr算法跟随点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoDefenValue
        {
            get { return _MMsheOrderParamReq.ofrAlgoDefenValue; }

            set
            {
                _MMsheOrderParamReq.ofrAlgoDefenValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr算法安全点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoSafeValue
        {
            get { return _MMsheOrderParamReq.ofrAlgoSafeValue; }

            set
            {
                _MMsheOrderParamReq.ofrAlgoSafeValue = value;
                this.OnPropertyChanged();

            }
        }

        //Ofr算法安全触发点
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoTrigValue
        {
            get { return _MMsheOrderParamReq.ofrAlgoTrigValue; }

            set
            {
                _MMsheOrderParamReq.ofrAlgoTrigValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用算法计算
        public string algoEnable
        {
            get { return _MMsheOrderParamReq.algoEnable; }

            set
            {
                _MMsheOrderParamReq.algoEnable = value;
                this.OnPropertyChanged();
            }
        }

        //是否使用基准债券0：否、1：是
        public string algoBaseEnable
        {
            get { return _MMsheOrderParamReq.algoBaseEnable; }

            set
            {
                _MMsheOrderParamReq.algoBaseEnable = value;
                this.OnPropertyChanged();

                OnPropertyChanged(algoBaseSecurityid);
            }
        }

        //使用基准债券
        public string algoBaseSecurityid
        {
            get { return _MMsheOrderParamReq.algoBaseSecurityid; }
            set
            {
                _MMsheOrderParamReq.algoBaseSecurityid = value;
                this.OnPropertyChanged();
            }
        }

        //参考中间价:1 双边市场中间价，2：中债估值，3：自定义
        public string algoMidType
        {
            get { return _MMsheOrderParamReq.algoMidType; }

            set
            {
                _MMsheOrderParamReq.algoMidType = value;
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
            get { return _MMsheOrderParamReq.algoMidDetail; }

            set
            {
                _MMsheOrderParamReq.algoMidDetail = value;
                this.OnPropertyChanged();
            }
        }

        //参考中间价—自定义详情
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoMidDetailDefined
        {
            get { return _MMsheOrderParamReq.algoMidDetailDefined; }

            set
            {
                _MMsheOrderParamReq.algoMidDetailDefined = value;
                this.OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    extAlgoMidDetailDefined = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, algoMidDetailDefined, "1");
                }
            }
        }

        //中间价偏离值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoMidOffsetValue
        {
            get { return _MMsheOrderParamReq.algoMidOffsetValue; }

            set
            {
                _MMsheOrderParamReq.algoMidOffsetValue = value;
                this.OnPropertyChanged();
            }
        }

        //算法点差
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string algoValue
        {
            get { return _MMsheOrderParamReq.algoValue; }

            set
            {
                _MMsheOrderParamReq.algoValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用中债估值
        public string algoCDCEnable
        {
            get { return _MMsheOrderParamReq.algoCDCEnable; }

            set
            {
                _MMsheOrderParamReq.algoCDCEnable = value;
                this.OnPropertyChanged();
            }
        }

        //中债估值详情
        public string algoCDCDetail
        {
            get { return _MMsheOrderParamReq.algoCDCDetail; }

            set
            {
                _MMsheOrderParamReq.algoCDCDetail = value;
                this.OnPropertyChanged();
            }
        }

        //Bid中债估值点差
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidAlgoCDCValue
        {
            get { return _MMsheOrderParamReq.bidAlgoCDCValue; }

            set
            {
                _MMsheOrderParamReq.bidAlgoCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr中债估值点差
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrAlgoCDCValue
        {
            get { return _MMsheOrderParamReq.ofrAlgoCDCValue; }

            set
            {
                _MMsheOrderParamReq.ofrAlgoCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //启用积极报价
        public string posLmtEnable
        {
            get { return _MMsheOrderParamReq.posLmtEnable; }

            set
            {
                _MMsheOrderParamReq.posLmtEnable = value;
                this.OnPropertyChanged();
            }
        }

        //Bid积极报价值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string bidPosValue
        {
            get { return _MMsheOrderParamReq.bidPosValue; }

            set
            {
                _MMsheOrderParamReq.bidPosValue = value;
                this.OnPropertyChanged();
            }
        }

        //Ofr积极报价值
        //[Required]
        //[CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string ofrPosValue
        {
            get { return _MMsheOrderParamReq.ofrPosValue; }

            set
            {
                _MMsheOrderParamReq.ofrPosValue = value;
                this.OnPropertyChanged();
            }
        }

        //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
        public string posFakEnable
        {
            get { return _MMsheOrderParamReq.posFakEnable; }

            set
            {
                _MMsheOrderParamReq.posFakEnable = value;
                if (!string.IsNullOrEmpty(_MMsheOrderParamReq.posFakEnable) && _MMsheOrderParamReq.posFakEnable.Equals("1"))
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
            get { return _MMsheOrderParamReq.posFakSpreadEnable; }

            set
            {
                _MMsheOrderParamReq.posFakSpreadEnable = value;
                this.OnPropertyChanged();
            }
        }
        //点价点差
        public string posFakSpread
        {
            get { return _MMsheOrderParamReq.posFakSpread; }

            set
            {
                _MMsheOrderParamReq.posFakSpread = value;
                this.OnPropertyChanged();
            }
        }

        // 成交拉宽时长下限 不填写默认为 5  单位：分钟   [ 1, 99]
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string dealWidenLowerTime
        {
            get { return this._MMsheOrderParamReq.dealWidenLowerTime; }
            set
            {
                _MMsheOrderParamReq.dealWidenLowerTime = value;
                OnPropertyChanged();
            }
        }

        // 成交拉宽时长上限,单位：分钟 不填写默认为 5 [ 5, 99]
        //[Required]
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmptyIsDecimal")]
        public string dealWidenUpperTime
        {
            get { return _MMsheOrderParamReq.dealWidenUpperTime; }
            set
            {
                _MMsheOrderParamReq.dealWidenUpperTime = value;
                OnPropertyChanged();
            }
        }

        //48.全局强墙Bid
        public string globalWallBid
        {
            get { return _MMsheOrderParamReq.globalWallBid; }
            set
            {
                _MMsheOrderParamReq.globalWallBid = value;
                OnPropertyChanged();
            }
        }

        //49.全局强墙Bid
        public string globalWallOfr
        {
            get { return _MMsheOrderParamReq.globalWallOfr; }
            set
            {
                _MMsheOrderParamReq.globalWallOfr = value;
                OnPropertyChanged();
            }
        }

        //50.Bid策略类型 Defend(跟随) Algorithm(算法) Manual(人工) API("接口"),
        public string bidStrategyType
        {
            get { return _MMsheOrderParamReq.bidStrategyType; }
            set
            {
                _MMsheOrderParamReq.bidStrategyType = value;
                OnPropertyChanged();
            }
        }

        //51.Ofr策略类型 Defend(跟随) Algorithm(算法) Manual(人工) API("接口"),
        public string ofrStrategyType
        {
            get { return _MMsheOrderParamReq.ofrStrategyType; }
            set
            {
                _MMsheOrderParamReq.ofrStrategyType = value;
                OnPropertyChanged();
            }
        }

        //52.接口bid 价格
        public string apiBid
        {
            get { return _MMsheOrderParamReq.apiBid; }
            set
            {
                _MMsheOrderParamReq.apiBid = value;
                OnPropertyChanged();
            }
        }

        //53.接口ofr价格
        public string apiOfr
        {
            get { return _MMsheOrderParamReq.apiOfr; }
            set
            {
                _MMsheOrderParamReq.apiOfr = value;
                OnPropertyChanged();
            }
        }

        //54.接口Bid报量
        public string apiBidQty
        {
            get { return _MMsheOrderParamReq.apiBidQty; }
            set
            {
                _MMsheOrderParamReq.apiBidQty = value;
                OnPropertyChanged();
            }
        }

        //55.接口Ofr报量
        public string apiOfrQty
        {
            get { return _MMsheOrderParamReq.apiOfrQty; }
            set
            {
                _MMsheOrderParamReq.apiOfrQty = value;
                OnPropertyChanged();
            }
        }

        //56.只读显示，展示接收到接口数据的最新时间戳
        public string apiLastTime
        {
            get { return _MMsheOrderParamReq.apiLastTime; }
            set
            {
                _MMsheOrderParamReq.apiLastTime = value;
                OnPropertyChanged();
            }
        }

        //57.bid手动报价
        public string manualBid
        {
            get { return _MMsheOrderParamReq.manualBid; }
            set
            {
                _MMsheOrderParamReq.manualBid = value;
                OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    exeManualBid = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, manualBid, "1");
                }
            }
        }

        //58.ofr手动报价
        public string manualOfr
        {
            get { return _MMsheOrderParamReq.manualOfr; }
            set
            {
                _MMsheOrderParamReq.manualOfr = value;
                OnPropertyChanged();

                //if (("1").Equals(exeBondPre))
                //{
                //    exeManualOfr = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, manualOfr, "1");
                //}
            }
        }

        //59.挂单成功标志（1成功，0失败）
        public string isQuoteSuccess
        {
            get { return _MMsheOrderParamReq.isQuoteSuccess; }
            set
            {
                _MMsheOrderParamReq.isQuoteSuccess = value;
                OnPropertyChanged();
            }
        }

        //60.最后操作时间
        public string lastOptTime
        {
            get { return _MMsheOrderParamReq.lastOptTime; }
            set
            {
                _MMsheOrderParamReq.lastOptTime = value;
                OnPropertyChanged();
            }
        }

        //61.外面显示的报量（策略小窗)
        public string displayBidQty
        {
            get { return _MMsheOrderParamReq.displayBidQty; }
            set
            {
                _MMsheOrderParamReq.displayBidQty = value;
                OnPropertyChanged();
            }
        }

        //62.外面显示的报量（策略小窗)
        public string displayOfrQty
        {
            get { return _MMsheOrderParamReq.displayOfrQty; }
            set
            {
                _MMsheOrderParamReq.displayOfrQty = value;
                OnPropertyChanged();
            }
        }

        public string stepLength
        {
            get { return _MMsheOrderParamReq.stepLength; }

            set
            {
                _MMsheOrderParamReq.stepLength = value;
                this.OnPropertyChanged();
            }
        }

        //是否使用冰山量
        public string maxFloorEnable
        {
            get { return _MMsheOrderParamReq.maxFloorEnable; }

            set
            {
                _MMsheOrderParamReq.maxFloorEnable = value;
                if (!string.IsNullOrEmpty(_MMsheOrderParamReq.maxFloorEnable) && _MMsheOrderParamReq.maxFloorEnable.Equals("1"))
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
            get { return _MMsheOrderParamReq.maxFloorIsEnable; }

            set
            {
                _MMsheOrderParamReq.maxFloorIsEnable = value;
                this.OnPropertyChanged();
            }
        }
        //冰山量
        public string maxFloor
        {
            get { return _MMsheOrderParamReq.maxFloor; }

            set
            {
                _MMsheOrderParamReq.maxFloor = value;
                this.OnPropertyChanged();
            }
        }
        //是否报指示性
        public string openQDM2Enable
        {
            get { return _MMsheOrderParamReq.openQDM2Enable; }

            set
            {
                _MMsheOrderParamReq.openQDM2Enable = value;
                this.OnPropertyChanged();
            }
        }


        //是否跟随策略无可报价格时锚定中债估值
        public string defenCDCEnable
        {
            get { return _MMsheOrderParamReq.defenCDCEnable; }

            set
            {
                _MMsheOrderParamReq.defenCDCEnable = value;
                if (!string.IsNullOrEmpty(_MMsheOrderParamReq.defenCDCEnable) && _MMsheOrderParamReq.defenCDCEnable.Equals("1"))
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
        //是否跟随策略无可报价格时锚定中债估值
        public bool defenCDCIsEnable
        {
            get { return _MMsheOrderParamReq.defenCDCIsEnable; }

            set
            {
                _MMsheOrderParamReq.defenCDCIsEnable = value;
                this.OnPropertyChanged();
            }
        }
        //估值拉宽Bid
        public string bidDefenCDCValue
        {
            get { return _MMsheOrderParamReq.bidDefenCDCValue; }

            set
            {
                _MMsheOrderParamReq.bidDefenCDCValue = value;
                this.OnPropertyChanged();
            }
        }
        //估值拉宽Ofr
        public string ofrDefenCDCValue
        {
            get { return _MMsheOrderParamReq.ofrDefenCDCValue; }

            set
            {
                _MMsheOrderParamReq.ofrDefenCDCValue = value;
                this.OnPropertyChanged();
            }
        }

        //中债估值详情
        public string defenCDCDetail
        {
            get { return _MMsheOrderParamReq.defenCDCDetail; }

            set
            {
                _MMsheOrderParamReq.defenCDCDetail = value;
                this.OnPropertyChanged();
            }
        }



        //bid跟随参考市场价类型 1-双边 2-XBOND 3-全市场
        public string bidDefenReferMarketType
        {
            get { return _MMsheOrderParamReq.bidDefenReferMarketType; }

            set
            {
                _MMsheOrderParamReq.bidDefenReferMarketType = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    bidDefenReferImageVis = "0";
                }
            }
        }

        //bid跟随参考市场价参考档位 1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
        public string bidDefenReferLevel
        {
            get { return _MMsheOrderParamReq.bidDefenReferLevel; }

            set
            {
                _MMsheOrderParamReq.bidDefenReferLevel = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    bidDefenReferImageVis = "0";
                }
            }
        }

        //ofr跟随参考市场价类型 1-双边 2-XBOND 3-全市场
        public string ofrDefenReferMarketType
        {
            get { return _MMsheOrderParamReq.ofrDefenReferMarketType; }

            set
            {
                _MMsheOrderParamReq.ofrDefenReferMarketType = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    ofrDefenReferImageVis = "0";
                }
            }
        }

        //ofr跟随参考市场价参考档位 1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
        public string ofrDefenReferLevel
        {
            get { return _MMsheOrderParamReq.ofrDefenReferLevel; }

            set
            {
                _MMsheOrderParamReq.ofrDefenReferLevel = value;
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
            get { return _MMsheOrderParamReq.wallEnable; }

            set
            {
                _MMsheOrderParamReq.wallEnable = value;
                //if (!string.IsNullOrEmpty(_MMsheOrderParamReq.wallEnable) && _MMsheOrderParamReq.wallEnable.Equals("1"))
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
            get { return _MMsheOrderParamReq.IswallEnable; }

            set
            {
                _MMsheOrderParamReq.IswallEnable = value;
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

                if (decimal.Parse(bidBasePrice) >= decimal.Parse(ofrBasePrice))
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
                    _bidStrategyModeList.Add("Manual", "手动" + ":" + (string.IsNullOrEmpty(manualBid) ? "--" : MMsheCommonUtils.Format3Digit(manualBid)));
                }

                //if (!string.IsNullOrEmpty(bidDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(bidDefenCalPrice, out value)) value = 0;
                    _bidStrategyModeList.Add("Defend", "跟随" + ":" + (string.IsNullOrEmpty(bidDefenCalPrice) ? "--" : MMsheCommonUtils.Format3Digit(bidDefenCalPrice)));
                }

                //if (!string.IsNullOrEmpty(bidAlgoCalPrice))
                //{
                //    decimal value = 0;
                //    if (!decimal.TryParse(bidAlgoCalPrice, out value)) value = 0;
                //    _bidStrategyModeList.Add("Algorithm", "算法" + ":" + (string.IsNullOrEmpty(bidAlgoCalPrice) ? "--" : MMsheCommonUtils.Format4Digit(bidAlgoCalPrice)));
                //}

                //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                //{
                //    //decimal value = 0;
                //    //decimal qty = 0;
                //    //if (!decimal.TryParse(apiBid, out value)) value = 0;
                //    //if (!decimal.TryParse(apiBidQty, out qty)) qty = 0;
                //    _bidStrategyModeList.Add("API", "接口 " + ":" + (string.IsNullOrEmpty(apiBid) ? "--" : MMsheCommonUtils.Format4Digit(apiBid)) + "(" + (string.IsNullOrEmpty(apiBidQty) ? "--" : apiBidQty) + "KW)");
                //}

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
                    _ofrStrategyModeList.Add("Manual", "手动" + ":" + (string.IsNullOrEmpty(manualOfr) ? "--" : MMsheCommonUtils.Format3Digit(manualOfr)));
                }

                //if (!string.IsNullOrEmpty(ofrDefenCalPrice))
                {
                    //decimal value;
                    //if (!decimal.TryParse(ofrDefenCalPrice, out value)) value = 0;
                    _ofrStrategyModeList.Add("Defend", "跟随" + ":" + (string.IsNullOrEmpty(ofrDefenCalPrice) ? "--" : MMsheCommonUtils.Format3Digit(ofrDefenCalPrice)));
                }

                //if (!string.IsNullOrEmpty(ofrAlgoCalPrice))
                //{
                //    //decimal value = 0;
                //    //if (!decimal.TryParse(ofrAlgoCalPrice, out value)) value = 0;
                //    _ofrStrategyModeList.Add("Algorithm", "算法" + ":" + (string.IsNullOrEmpty(ofrAlgoCalPrice) ? "--" : MMsheCommonUtils.Format4Digit(ofrAlgoCalPrice)));
                //}
                //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                //{
                //    //decimal value = 0;
                //    //decimal qty = 0;
                //    //if (!decimal.TryParse(apiOfr, out value)) value = 0;
                //    //if (!decimal.TryParse(apiOfrQty, out qty)) qty = 0;
                //    _ofrStrategyModeList.Add("API", "接口" + ":" + (string.IsNullOrEmpty(apiOfr) ? "--" : MMsheCommonUtils.Format4Digit(apiOfr)) + "(" + (string.IsNullOrEmpty(apiOfrQty) ? "--" : apiOfrQty) + "KW)");
                //}
                return _ofrStrategyModeList;
            }
        }

        //自定义 快捷调价按钮是否可用
        private bool _IsEnabledClick = true;
        public bool IsEnabledClick
        {
            get
            {
                if (string.IsNullOrEmpty(_MMsheOrderParamReq.cDCValueType))
                {
                    return true;
                }
                else
                {
                    if ("1".Equals(_MMsheOrderParamReq.cDCValueType))
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
                _MMsheOrderParamReq.IsEnabledClick = value;
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
                    if (decimal.TryParse(bidBasePrice, out bidValue) && decimal.TryParse(ofrBasePrice, out ofrValue))
                    {
                        diffValue = (ofrValue - bidValue);
                        diff = MMsheCommonUtils.Format3Digit(diffValue.ToString());
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
                    text = bidOfrDiff;
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

                    //_quoteModelList.Add(new QuoteModel()
                    //{
                    //    bid = bidStrategyModeList["Algorithm"],
                    //    ofr = ofrStrategyModeList["Algorithm"],
                    //});
                    //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
                    //{
                    //    _quoteModelList.Add(new QuoteModel()
                    //    {
                    //        bid = bidStrategyModeList["API"],
                    //        ofr = ofrStrategyModeList["API"],
                    //    });
                    //}
                }
                return _quoteModelList;
            }
        }
        //中债估值类型1-行权；2-到期（或者空）
        public string CDCValueType
        {
            get { return _MMsheOrderParamReq.cDCValueType; }

            set
            {
                _MMsheOrderParamReq.cDCValueType = value;
                if ("1".Equals(_MMsheOrderParamReq.cDCValueType))
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
            get { return _MMsheOrderParamReq.isWallEable; }

            set
            {
                _MMsheOrderParamReq.isWallEable = value;
                this.OnPropertyChanged();
            }
        }

        //当前策略bid价格计算详情
        public string bidCalDetail
        {

            get { return _MMsheOrderParamReq.bidCalDetail; }

            set
            {
                _MMsheOrderParamReq.bidCalDetail = value;
                this.OnPropertyChanged();
            }
        }

        //当前策略ofr价格计算详情
        public string ofrCalDetail
        {

            get { return _MMsheOrderParamReq.ofrCalDetail; }

            set
            {
                _MMsheOrderParamReq.ofrCalDetail = value;
                this.OnPropertyChanged();
            }
        }
        public string exeManualBid
        {
            get { return _MMsheOrderParamReq.exeManualBid; }

            set
            {
                _MMsheOrderParamReq.exeManualBid = value;
                this.OnPropertyChanged();
                //if (("0").Equals(exeBondPre))
                //{
                //    manualBid = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualBid, "1");
                //}
            }
        }

        public string exeManualOfr
        {
            get { return _MMsheOrderParamReq.exeManualOfr; }

            set
            {
                _MMsheOrderParamReq.exeManualOfr = value;
                this.OnPropertyChanged();
                if (("0").Equals(exeBondPre))
                {
                    manualOfr = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualOfr, "2");
                }
            }
        }
        public string extAlgoMidDetailDefined
        {
            get { return _MMsheOrderParamReq.extAlgoMidDetailDefined; }

            set
            {
                _MMsheOrderParamReq.extAlgoMidDetailDefined = value;
                this.OnPropertyChanged();
                if (("0").Equals(exeBondPre))
                {
                    algoMidDetailDefined = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, extAlgoMidDetailDefined, "2");
                }
            }
        }
        //含权债偏好
        public string exeBondPre
        {
            get { return _MMsheOrderParamReq.exeBondPre; }

            set
            {
                _MMsheOrderParamReq.exeBondPre = value;
                this.OnPropertyChanged();
                if (("1").Equals(exeBondPre))
                {
                    exeManualBid = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, manualBid, "1");
                    exeManualOfr = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, manualOfr, "2");
                    //extAlgoMidDetailDefined = MMsheCommonUtils.CalBond(securityid, "4", baseSettleType, algoMidDetailDefined, "1");
                }
                else if (("0").Equals(exeBondPre))
                {
                    manualBid = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualBid, "1");
                    manualOfr = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, exeManualOfr, "2");
                    //algoMidDetailDefined = MMsheCommonUtils.CalBond(securityid, "7", baseSettleType, extAlgoMidDetailDefined, "1");
                }
            }
        }

        //含权债跟随
        public string exeBondFollow
        {
            get { return _MMsheOrderParamReq.exeBondFollow; }

            set
            {
                _MMsheOrderParamReq.exeBondFollow = value;
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

        private string _Venuetypegw;
        public string Venuetypegw
        {
            get { return _MMsheOrderParamReq.venuetypegw ; }

            set
            {
                _MMsheOrderParamReq.venuetypegw = value;
                this.OnPropertyChanged();
            }
        }

        private string _Venues;
        public string Venues
        {
            get { return _MMsheOrderParamReq.venues; }

            set
            {
                _MMsheOrderParamReq.venues = value;
                this.OnPropertyChanged();
            }
        }
        //成交后关闭策略，关闭时，基于行情进行报价，无行情时使用中债进行兜底1-开启（成交后关闭策略）；0-关闭（成交后开启策略）
        public string dealClose
        {
            get { return _MMsheOrderParamReq.dealClose; }

            set
            {
                _MMsheOrderParamReq.dealClose = value;
                this.OnPropertyChanged();
                if ("多".Equals(value))
                {
                    dealCloseEnableImageVis = "0";
                }
            }
        }
        private string _dealCloseEnableImageVis = "1";
        public string dealCloseEnableImageVis
        {
            get { return _dealCloseEnableImageVis; }
            set
            {
                _dealCloseEnableImageVis = value;
                this.OnPropertyChanged();
            }
        }

        //bid中债估值计算净价
        public string bidCDCDefenNetPrice
        {
            get { return _MMsheOrderParamReq.bidCDCDefenNetPrice; }
            set
            {
                _MMsheOrderParamReq.bidCDCDefenNetPrice = value;
                this.OnPropertyChanged();
            }
        }
        //ofr中债估值计算净价
        public string ofrCDCDefenNetPrice
        {
            get { return _MMsheOrderParamReq.ofrCDCDefenNetPrice; }
            set
            {
                _MMsheOrderParamReq.ofrCDCDefenNetPrice = value;
                this.OnPropertyChanged();
            }
        }
        public object Clone()
        {
            OrderParamProp ret = new OrderParamProp(null);
            ret.circulSize = _MMsheOrderParamReq.circulSize;
            ret.riskMinQtySize = _MMsheOrderParamReq.riskMinQtySize;
            ret.riskMaxQtyRatio = _MMsheOrderParamReq.riskMaxQtyRatio;
            ret.securityid = this._MMsheOrderParamReq.securityid;
            ret.symbol = _MMsheOrderParamReq.symbol;
            ret.bidBasePriority = this._MMsheOrderParamReq.bidBasePriority;
            ret.bidDefenCalPrice = this._MMsheOrderParamReq.bidDefenCalPrice;
            ret.bidAlgoCalPrice = this._MMsheOrderParamReq.bidAlgoCalPrice;
            ret.bidQuoteModel = this._MMsheOrderParamReq.bidQuoteModel;
            ret.defaultManualSpread = this._MMsheOrderParamReq.defaultManualSpread;
            ret.bidBaseQty = this._MMsheOrderParamReq.bidBaseQty;
            ret.bidBasePrice = this._MMsheOrderParamReq.bidBasePrice;

            ret.bidBaseReSendValue = this._MMsheOrderParamReq.bidBaseReSendValue;
            ret.bidBaseWidenValue = this._MMsheOrderParamReq.bidBaseWidenValue;
            ret.BaseERValue = this._MMsheOrderParamReq.BaseERValue;
            ret.BaseERReSendType = this._MMsheOrderParamReq.BaseERReSendType;
            ret.baseSettleType = this._MMsheOrderParamReq.baseSettleType;
            ret.ofrBasePriority = this._MMsheOrderParamReq.ofrBasePriority;
            ret.ofrDefenCalPrice = this._MMsheOrderParamReq.ofrDefenCalPrice;
            ret.ofrAlgoCalPrice = this._MMsheOrderParamReq.ofrAlgoCalPrice;
            ret.ofrQuoteModel = this._MMsheOrderParamReq.ofrQuoteModel;
            ret.ofrBaseQty = this._MMsheOrderParamReq.ofrBaseQty;

            ret.ofrBasePrice = this._MMsheOrderParamReq.ofrBasePrice;
            ret.ofrBaseReSendValue = this._MMsheOrderParamReq.ofrBaseReSendValue;
            ret.ofrBaseWidenValue = this._MMsheOrderParamReq.ofrBaseWidenValue;
            ret.bidAlgoDefenValue = this._MMsheOrderParamReq.bidAlgoDefenValue;
            ret.bidAlgoSafeValue = this._MMsheOrderParamReq.bidAlgoSafeValue;
            ret.bidAlgoTrigValue = this._MMsheOrderParamReq.bidAlgoTrigValue;
            ret.ofrAlgoDefenValue = this._MMsheOrderParamReq.ofrAlgoDefenValue;
            ret.ofrAlgoSafeValue = this._MMsheOrderParamReq.ofrAlgoSafeValue;
            ret.ofrAlgoTrigValue = this._MMsheOrderParamReq.ofrAlgoTrigValue;
            ret.algoEnable = this._MMsheOrderParamReq.algoEnable;

            ret.algoBaseEnable = this._MMsheOrderParamReq.algoBaseEnable;
            ret.algoBaseSecurityid = this._MMsheOrderParamReq.algoBaseSecurityid;
            ret.algoMidType = this._MMsheOrderParamReq.algoMidType;
            ret.algoMidDetail = this._MMsheOrderParamReq.algoMidDetail;
            ret.algoMidDetailDefined = this._MMsheOrderParamReq.algoMidDetailDefined;
            ret.algoMidOffsetValue = this._MMsheOrderParamReq.algoMidOffsetValue;
            ret.algoValue = this._MMsheOrderParamReq.algoValue;
            ret.algoCDCEnable = this._MMsheOrderParamReq.algoCDCEnable;
            ret.algoCDCDetail = this._MMsheOrderParamReq.algoCDCDetail;
            ret.bidAlgoCDCValue = this._MMsheOrderParamReq.bidAlgoCDCValue;

            ret.ofrAlgoCDCValue = this._MMsheOrderParamReq.ofrAlgoCDCValue;
            ret.posLmtEnable = this._MMsheOrderParamReq.posLmtEnable;
            ret.bidPosValue = this._MMsheOrderParamReq.bidPosValue;
            ret.ofrPosValue = this._MMsheOrderParamReq.ofrPosValue;
            ret.posFakEnable = this._MMsheOrderParamReq.posFakEnable;
            ret.posFakSpread = this._MMsheOrderParamReq.posFakSpread;
            ret.dealWidenLowerTime = _MMsheOrderParamReq.dealWidenLowerTime;
            ret.dealWidenUpperTime = _MMsheOrderParamReq.dealWidenUpperTime;
            ret.globalWallBid = _MMsheOrderParamReq.globalWallBid;
            ret.globalWallOfr = _MMsheOrderParamReq.globalWallOfr;
            ret.bidStrategyType = _MMsheOrderParamReq.bidStrategyType;

            ret.ofrStrategyType = _MMsheOrderParamReq.ofrStrategyType;
            ret.apiBid = _MMsheOrderParamReq.apiBid;
            ret.apiOfr = _MMsheOrderParamReq.apiOfr;
            ret.apiBidQty = _MMsheOrderParamReq.apiBidQty;
            ret.apiOfrQty = _MMsheOrderParamReq.apiOfrQty;
            ret.apiLastTime = _MMsheOrderParamReq.apiLastTime;
            ret.manualBid = _MMsheOrderParamReq.manualBid;
            ret.manualOfr = _MMsheOrderParamReq.manualOfr;
            ret.isQuoteSuccess = _MMsheOrderParamReq.isQuoteSuccess;
            ret.lastOptTime = _MMsheOrderParamReq.lastOptTime;

            ret.displayBidQty = _MMsheOrderParamReq.displayBidQty;
            ret.displayOfrQty = _MMsheOrderParamReq.displayOfrQty;
            ret.stepLength = _MMsheOrderParamReq.stepLength;
            //ret.maxFloorEnable = _MMsheOrderParamReq.maxFloorEnable;
            //ret.maxFloor = _MMsheOrderParamReq.maxFloor;
            ret.openQDM2Enable = _MMsheOrderParamReq.openQDM2Enable;
            ret.defenCDCEnable = _MMsheOrderParamReq.defenCDCEnable;
            ret.bidDefenCDCValue = _MMsheOrderParamReq.bidDefenCDCValue;
            ret.ofrDefenCDCValue = _MMsheOrderParamReq.ofrDefenCDCValue;
            ret.bidDefenReferMarketType = _MMsheOrderParamReq.bidDefenReferMarketType;
            ret.bidDefenReferLevel = _MMsheOrderParamReq.bidDefenReferLevel;
            ret.ofrDefenReferMarketType = _MMsheOrderParamReq.ofrDefenReferMarketType;
            ret.ofrDefenReferLevel = _MMsheOrderParamReq.ofrDefenReferLevel;
            ret.wallEnable = _MMsheOrderParamReq.wallEnable;
            ret.CDCValueType = _MMsheOrderParamReq.cDCValueType;
            ret.bidCalDetail = _MMsheOrderParamReq.bidCalDetail;
            ret.ofrCalDetail = _MMsheOrderParamReq.ofrCalDetail;
            ret.exeManualBid = _MMsheOrderParamReq.exeManualBid;
            ret.exeManualOfr = _MMsheOrderParamReq.exeManualOfr;
            ret.extAlgoMidDetailDefined = _MMsheOrderParamReq.extAlgoMidDetailDefined;
            //ret.exeBondPre = _MMsheOrderParamReq.exeBondPre;
            //ret.exeBondFollow = _MMsheOrderParamReq.exeBondFollow;
            ret.IsInterest = IsInterest;
            ret.Venues = _MMsheOrderParamReq.venues;
            ret.Venuetypegw = _MMsheOrderParamReq.venuetypegw;
            ret.dealClose = _MMsheOrderParamReq.dealClose;
            ret.bidCDCDefenNetPrice = _MMsheOrderParamReq.bidCDCDefenNetPrice;
            ret.ofrCDCDefenNetPrice = _MMsheOrderParamReq.ofrCDCDefenNetPrice;
            return ret;
        }
    }

    public class QuoteModel : ViewModelBase
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
