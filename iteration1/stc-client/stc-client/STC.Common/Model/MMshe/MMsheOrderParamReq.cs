using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MMshe
{
    /// <summary>
    /// 下单参数定义 stc.mm.mm.bond.updateParas
    /// </summary>
    [Serializable]
    public class MMsheOrderParamReq
    {
        public string venuetypegw;//交易平台
        public string venues;//交易执行渠道
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
        //成交后关闭策略，关闭时，基于行情进行报价，无行情时使用中债进行兜底1-开启（成交后关闭策略）；0-关闭（成交后开启策略）
        public string dealClose;
        
        public string bidCDCDefenNetPrice;//bid中债估值计算净价
        public string ofrCDCDefenNetPrice;//ofr中债估值计算净价

        public void InitData()
        {
            this.bidCalDetail = string.Empty;
            this.ofrCalDetail = string.Empty;
            this.circulSize = string.Empty;
            this.riskMinQtySize = string.Empty;
            this.riskMaxQtyRatio = string.Empty;
            this.securityid = string.Empty;
            this.symbol = string.Empty;
            this.bidBasePriority = "1,2";
            this.bidDefenCalPrice = string.Empty;
            this.bidAlgoCalPrice = string.Empty;
            this.algoMidPrice = string.Empty;
            this.bidQuoteModel = "Defend";
            this.defaultManualSpread = "25";
            this.bidBaseQty = "1";
            this.bidBasePrice = string.Empty;

            this.bidBaseReSendValue = "0";
            this.bidBaseWidenValue = "5";
            this.BaseERValue = "0";
            this.BaseERReSendType = "1";
            this.baseSettleType = "2";
            this.ofrBasePriority = "1,2";
            this.ofrDefenCalPrice = string.Empty;
            this.ofrAlgoCalPrice = string.Empty;
            this.ofrQuoteModel = "Defend";
            this.ofrBaseQty = "1";

            this.ofrBasePrice = string.Empty;
            this.ofrBaseReSendValue = "0";
            this.ofrBaseWidenValue = "5";
            this.bidAlgoDefenValue = "5";
            this.bidAlgoSafeValue = "0";
            this.bidAlgoTrigValue = "0";
            this.ofrAlgoDefenValue = "5";
            this.ofrAlgoSafeValue = "0";
            this.ofrAlgoTrigValue = "0";
            this.algoEnable = "0";

            this.algoBaseEnable = "0";
            this.algoBaseSecurityid = string.Empty;
            this.algoMidType = "2";
            this.algoMidDetail = "0";
            this.algoMidDetailDefined = "0";
            this.algoMidOffsetValue = "0";
            this.algoValue = "30";
            this.algoCDCEnable = "0";
            this.algoCDCDetail = "0";
            this.bidAlgoCDCValue = "1";

            this.ofrAlgoCDCValue = "1";
            this.posLmtEnable = "0";
            this.bidPosValue = "0";
            this.ofrPosValue = "0";
            this.posFakEnable = "0";
            this.posFakSpread = "0";
            this.dealWidenLowerTime = "30";
            this.dealWidenUpperTime = "30";
            this.globalWallBid = "0";
            this.globalWallOfr = "0";
            this.bidStrategyType = "Defend";

            this.ofrStrategyType = "Defend";
            this.apiBid = string.Empty;
            this.apiOfr = string.Empty;
            this.apiBidQty = string.Empty;
            this.apiOfrQty = string.Empty;
            this.apiLastTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.manualBid = "0";
            this.manualOfr = "0";
            this.isQuoteSuccess = "0";
            this.lastOptTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.stepLength = "0.001";
            //this.maxFloorEnable = "0";
            this.maxFloor = string.Empty;
            this.openQDM2Enable = "0";
            this.defenCDCEnable = "1";
            this.bidDefenCDCValue = "14.5";
            this.ofrDefenCDCValue = "14.5";
            this.defenCDCDetail = "0";
            this.bidDefenReferMarketType = "1";
            this.bidDefenReferLevel = "1";
            this.ofrDefenReferMarketType = "1";
            this.ofrDefenReferLevel = "1";
            this.wallEnable = "0";
            this.cDCValueType = string.Empty;
            this.exeManualBid = "0";
            this.exeManualOfr = "0";
            this.extAlgoMidDetailDefined = "0";
            this.exeBondPre = "0";
            this.exeBondFollow = "0";
            this.dealClose = "0";
            this.bidCDCDefenNetPrice = string.Empty;
            this.ofrCDCDefenNetPrice = string.Empty;
        }
    }
}
