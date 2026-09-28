using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    /// <summary>
    /// 下单参数定义 stc.mm.mm.bond.updateParas
    /// </summary>
    [Serializable]
    public class MMOrderParamReq
    {
        //1.债券余额
        public string circulSize;

        //2.债券代码
        public string securityid;

        //3.债券简称
        public string symbol;

        //4.Bid价格优先级：1防守，2算法
        public string bidBasePriority;

        //5.bid防守报价
        public string bidDefenCalPrice;

        //6.bid算法报价
        public string bidAlgoCalPrice;

        //算法参考中间价
        public string algoMidPrice;

        //7.bid方向报价模式1:手动、2:自动 废除
        // new Bid策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),
        public string bidQuoteModel;

        //8.手动报价默认拉宽或者收窄点差
        public string defaultManualSpread = "25";

        //9.Bid单笔报量
        public string bidBaseQty;

        //10.bid单笔报价
        public string bidBasePrice;

        //11.Bid重发阈值
        public string bidBaseReSendValue;

        //12.Bid拉宽值
        public string bidBaseWidenValue;

        //13.日成交限额
        public string BaseERValue;

        //14.成交拉宽,1: 按单笔报量拉宽后重发,2: 按剩余挂单量拉宽后重发
        public string BaseERReSendType;

        //15.清算速度
        public string baseSettleType;

        //16.ofr价格优先级：1防守，2算法
        public string ofrBasePriority;

        //17.ofr防守报价
        public string ofrDefenCalPrice;

        //18.ofr算法报价
        public string ofrAlgoCalPrice;

        //19.ofr方向报价模式1:手动、2:自动 废除
        //new Algorithm(算法) Manual(人工) API("接口"),
        public string ofrQuoteModel;

        //20.ofr单笔报量
        public string ofrBaseQty;

        //21.ofr单笔报价
        public string ofrBasePrice;

        //22.ofr重发阈值
        public string ofrBaseReSendValue;

        //23.ofr拉宽值
        public string ofrBaseWidenValue;

        //24.bid算法防守点
        public string bidAlgoDefenValue;

        //25.Bid算法安全点
        public string bidAlgoSafeValue;

        //26.Bid算法安全触发点
        public string bidAlgoTrigValue;

        //27.bid算法防守点
        public string ofrAlgoDefenValue;

        //28.Bid算法安全点
        public string ofrAlgoSafeValue;

        //29.Bid算法安全触发点
        public string ofrAlgoTrigValue;

        //30.启用算法计算
        public string algoEnable;

        //31.是否使用基准债券0：否、1：是
        public string algoBaseEnable;

        //32.使用基准债券
        public string algoBaseSecurityid;

        //33.参考中间价:1 双边市场中间价，2：中债估值，3：自定义
        public string algoMidType;

        //34.参考中间价详情
        public string algoMidDetail;

        //35.参考中间价—自定义详情
        public string algoMidDetailDefined;

        //36.中间价偏离值
        public string algoMidOffsetValue;

        //37.算法点差
        public string algoValue;

        //38.启用中债估值
        public string algoCDCEnable;

        //39.中债估值详情
        public string algoCDCDetail;

        //40.Bid中债估值点差
        public string bidAlgoCDCValue;

        //41.Ofr中债估值点差
        public string ofrAlgoCDCValue;

        //42.启用积极报价
        public string posLmtEnable;

        //43.Bid积极报价值
        public string bidPosValue;

        //44.Ofr积极报价值
        public string ofrPosValue;

        //45.本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
        public string posFakEnable;
        //点价点差
        public string posFakSpread;

        public bool posFakSpreadEnable;

        /**46.
        * 成交拉宽时长下限
        * 不填写默认为 5  单位：分钟
        * 债券 * 成交拉宽时长下限设置错误，可选范围： [ 1, 99]
        */
        //@Length(min = 1, max = 99, message = "债券 * 成交拉宽时长上限设置错误，可选范围:[1, 99]")
        public string dealWidenLowerTime;

        /**47.
        * 成交拉宽时长上限,单位：分钟
        * 不填写默认为 5
        * 债券 * 成交拉宽时长上限设置错误，可选范围： [ 5, 99]
        */
        //@Length(min = 5,max = 99,message = "债券 * 成交拉宽时长上限设置错误，可选范围:[ 5, 99]")
        //@Max(value = 99)
        public string dealWidenUpperTime;

        //48.全局强墙Bid
        public string globalWallBid;

        //49.全局强墙Ofr
        public string globalWallOfr;

        //50.Bid策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),
        public string bidStrategyType;

        //51.Ofr策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),
        public string ofrStrategyType;

        //52.接口bid 价格
        public string apiBid;

        //53.接口ofr价格
        public string apiOfr;

        //54.接口Bid报量
        public string apiBidQty;

        //55.接口Ofr报量
        public string apiOfrQty;

        //56.只读显示，展示接收到接口数据的最新时间戳
        public string apiLastTime;

        //57.bid手动报价
        public string manualBid;

        //58.ofr手动报价
        public string manualOfr;

        //59.挂单成功标志（1成功，0失败）
        public string isQuoteSuccess;

        //60.最后操作时间
        public string lastOptTime;

        //61.外面显示的报量（策略小窗)
        public string displayBidQty;

        //62.外面显示的报量（策略小窗)
        public string displayOfrQty;

        //63.风控报量下限
        public string riskMinQtySize;
        //64.风控报量上线比例
        public string riskMaxQtyRatio;
        public string circulRatioSize;
        //步长
        public string stepLength;

        //是否使用冰山量
        public string maxFloorEnable;
        //冰山量
        public string maxFloor;

        public bool maxFloorIsEnable;

        //是否使用冰山量
        public string openQDM2Enable;

        //防守无可报价格时锚定中债估值0：否、1：是
        public string defenCDCEnable;

        //防守无可报价格时锚定中债估值false:不可编辑；true:可编辑
        public bool defenCDCIsEnable;
        //Bid中债估值偏离[-1000, 1000] bp
        public string bidDefenCDCValue;
        //Ofr中债估值偏离[-1000, 1000]bp
        public string ofrDefenCDCValue;

        //防守中债估值详情
        public string defenCDCDetail;



        //Bid防守参考市场价类型 1-双边 2-XBOND 3-全市场
        public string bidDefenReferMarketType;

        //Bid防守参考市场价参考档位
        public string bidDefenReferLevel;

        //ofr防守参考市场价类型 1-双边 2-XBOND 3-全市场
        public string ofrDefenReferMarketType;

        //ofr防守参考市场价参考档位
        public string ofrDefenReferLevel;

        //是否启用墙0：否、1：是
        public string wallEnable;
        //是否启用墙
        public bool IswallEnable;
        //整组标志
        public string isUnifyGroup;

        //中债估值类型1-行权；2-到期（或者空）
        public string cDCValueType;

        //含权债不能启用编辑墙1-可编辑0-不可编辑
        public string isWallEable;

        //当前策略bid价格计算详情
        public string bidCalDetail;

        //当前策略ofr价格计算详情
        public string ofrCalDetail;

        public string exeManualBid;//行权人工Bid价格
        public string exeManualOfr; //行权人工Ofr价格
        public string extAlgoMidDetailDefined;//行权参考中间价—自定义详情
        public string exeBondPre;//含权债偏好 0-行权 1-到期
        public string exeBondFollow;//含权债跟随 0-行权 1-到期
        public string bidYieldPrice;//到期收益率
        public string ofrYieldPrice;//到期收益率
        public string bidYieldToExecutionPrice;//行权收益率
        public string ofrYieldToExecutionPrice;//行权收益率

        
        public string algoSafeEnable;//防守安全点是否启用0：否、1：是
        public string secondSafeEnable;//同向防守安全点是否启用0：否、1：是
        public string qdmespSafeEnable;//双边防守安全点是否启用0：否、1：是
        public string qdmespSafeLevel;//双边防守安全点档位1-第一档；2-第二档；3-第三档；4-第四档；5-第五档
        public string bidQdmespSafeValue;//Bid双边防守安全点
        public string ofrQdmespSafeValue;//ofr双边防守安全点

        
        public MMOrderParamReq Clone() 
        {
            MMOrderParamReq req = new MMOrderParamReq();
            req.circulSize = this.circulSize;
            req.securityid = this.securityid;
            req.symbol = this.symbol;
            req.bidBasePriority = this.bidBasePriority;
            req.bidDefenCalPrice = this.bidDefenCalPrice;
            req.bidAlgoCalPrice = this.bidAlgoCalPrice;
            req.algoMidPrice = this.algoMidPrice;
            req.bidQuoteModel = this.bidQuoteModel;
            req.defaultManualSpread = this.defaultManualSpread;
            req.bidBaseQty = this.bidBaseQty;
            req.bidBasePrice = this.bidBasePrice;
            req.bidBaseReSendValue = this.bidBaseReSendValue;
            req.bidBaseWidenValue = this.bidBaseWidenValue;
            req.BaseERValue = this.BaseERValue;
            req.BaseERReSendType = this.BaseERReSendType;
            req.baseSettleType = this.baseSettleType;
            req.ofrBasePriority = this.ofrBasePriority;
            req.ofrDefenCalPrice = this.ofrDefenCalPrice;
            req.ofrAlgoCalPrice = this.ofrAlgoCalPrice;
            req.ofrQuoteModel = this.ofrQuoteModel;
            req.ofrBaseQty = this.ofrBaseQty;
            req.ofrBasePrice = this.ofrBasePrice;
            req.ofrBaseReSendValue = this.ofrBaseReSendValue;
            req.ofrBaseWidenValue = this.ofrBaseWidenValue;
            req.bidAlgoDefenValue = this.bidAlgoDefenValue;
            req.bidAlgoSafeValue = this.bidAlgoSafeValue;
            req.bidAlgoTrigValue = this.bidAlgoTrigValue;
            req.ofrAlgoDefenValue = this.ofrAlgoDefenValue;
            req.ofrAlgoSafeValue = this.ofrAlgoSafeValue;
            req.ofrAlgoTrigValue = this.ofrAlgoTrigValue;
            req.algoEnable = this.algoEnable;
            req.algoBaseEnable = this.algoBaseEnable;
            req.algoBaseSecurityid = this.algoBaseSecurityid;
            req.algoMidType = this.algoMidType;
            req.algoMidDetail = this.algoMidDetail;
            req.algoMidDetailDefined = this.algoMidDetailDefined;
            req.algoMidOffsetValue = this.algoMidOffsetValue;
            req.algoValue = this.algoValue;
            req.algoCDCEnable = this.algoCDCEnable;
            req.algoCDCDetail = this.algoCDCDetail;
            req.bidAlgoCDCValue = this.bidAlgoCDCValue;
            req.ofrAlgoCDCValue = this.ofrAlgoCDCValue;
            req.posLmtEnable = this.posLmtEnable;
            req.bidPosValue = this.bidPosValue;
            req.ofrPosValue = this.ofrPosValue;
            req.posFakEnable = this.posFakEnable;
            req.posFakSpread = this.posFakSpread;
            req.posFakSpreadEnable = this.posFakSpreadEnable;
            req.dealWidenLowerTime = this.dealWidenLowerTime;
            req.dealWidenUpperTime = this.dealWidenUpperTime;
            req.globalWallBid = this.globalWallBid;
            req.globalWallOfr = this.globalWallOfr;
            req.bidStrategyType = this.bidStrategyType;
            req.ofrStrategyType = this.ofrStrategyType;
            req.apiBid = this.apiBid;
            req.apiOfr = this.apiOfr;
            req.apiBidQty = this.apiBidQty;
            req.apiOfrQty = this.apiOfrQty;
            req.apiLastTime = this.apiLastTime;
            req.manualBid = this.manualBid;
            req.manualOfr = this.manualOfr;
            req.isQuoteSuccess = this.isQuoteSuccess;
            req.lastOptTime = this.lastOptTime;
            req.displayBidQty = this.displayBidQty;
            req.displayOfrQty = this.displayOfrQty;
            req.riskMinQtySize = this.riskMinQtySize;
            req.riskMaxQtyRatio = this.riskMaxQtyRatio;
            req.circulRatioSize = this.circulRatioSize;
            req.stepLength = this.stepLength;
            req.maxFloorEnable = this.maxFloorEnable;
            req.maxFloor = this.maxFloor;
            req.maxFloorIsEnable = this.maxFloorIsEnable;
            req.openQDM2Enable = this.openQDM2Enable;
            req.defenCDCEnable = this.defenCDCEnable;
            req.defenCDCIsEnable = this.defenCDCIsEnable;
            req.bidDefenCDCValue = this.bidDefenCDCValue;
            req.ofrDefenCDCValue = this.ofrDefenCDCValue;
            req.defenCDCDetail = this.defenCDCDetail;
            req.bidDefenReferMarketType = this.bidDefenReferMarketType;
            req.bidDefenReferLevel = this.bidDefenReferLevel;
            req.ofrDefenReferMarketType = this.ofrDefenReferMarketType;
            req.ofrDefenReferLevel = this.ofrDefenReferLevel;
            req.wallEnable = this.wallEnable;
            req.IswallEnable = this.IswallEnable;
            req.isUnifyGroup = this.isUnifyGroup;
            req.cDCValueType = this.cDCValueType;
            req.isWallEable = this.isWallEable;
            req.bidCalDetail = this.bidCalDetail;
            req.ofrCalDetail = this.ofrCalDetail;
            req.exeManualBid = this.exeManualBid;
            req.exeManualOfr = this.exeManualOfr;
            req.extAlgoMidDetailDefined = this.extAlgoMidDetailDefined;
            req.exeBondPre = this.exeBondPre;
            req.exeBondFollow = this.exeBondFollow;
            req.bidYieldPrice = this.bidYieldPrice;
            req.ofrYieldPrice = this.ofrYieldPrice;
            req.bidYieldToExecutionPrice = this.bidYieldToExecutionPrice;
            req.ofrYieldToExecutionPrice = this.ofrYieldToExecutionPrice;
            req.algoSafeEnable = this.algoSafeEnable;
            req.secondSafeEnable = this.secondSafeEnable;
            req.qdmespSafeEnable = this.qdmespSafeEnable;
            req.qdmespSafeLevel = this.qdmespSafeLevel;
            req.bidQdmespSafeValue = this.bidQdmespSafeValue;
            req.ofrQdmespSafeValue = this.ofrQdmespSafeValue;
            return req;
        }
    }
}
