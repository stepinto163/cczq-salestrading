using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    /// <summary>
    /// 策略列表接口 分组策略查询 结果
    /// </summary>
    public class RfqstgViewModel
    {
        /// <summary>
        /// 策略ID
        /// </summary>
        public string id;

        /// <summary>
        /// 0:查询,1:插入,2:更新,3:删除,4:导入,5:启动,6:停止
        /// </summary>
        public string action;

        /// <summary>
        /// 策略分组ID
        /// </summary>
        public string groupid;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid;

        /// <summary>
        /// 债券名称
        /// </summary>
        public string symbol;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string username;

        /// <summary>
        /// 类型
        /// to do...
        /// </summary>
        public string type;

        /// <summary>
        /// 开关标识
        /// </summary>
        public string enable;

        /// <summary>
        /// 终端
        /// </summary>
        public string terminal;

        /// <summary>
        /// ip
        /// </summary>
        public string ip;

        public string closeby;
        public string remark;
        public string createtime;
        public string updatetime;
        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;

        // 基础设置 --------------------------------------------------- //

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settltype;

        /// <summary>
        /// 有效方向:不限,Buy,Se11,无   (A11, Buy, Sel1, None)
        /// </summary>
        public string validSide = "";

        /// <summary>
        /// Bid策略类型 Defend(防守) Algorithm(算法) Manual(人工) API("接口"),     ==  bidStrategyType
        /// to do...
        /// </summary>
        public string strategyTypeBid;

        /// <summary>
        /// Defend(防守) Algorithm(算法) Manual(人工) API("接口"),   == ofrStrategyType 
        /// to do...
        /// </summary>
        public string strategyTypeOfr;

        /// <summary>
        /// Bid预设回量
        /// to do...
        /// </summary>
        public string quantityBid;

        /// <summary>
        /// Ofr预设回量
        /// to do...
        /// </summary>
        public string quantityOfr;

        /// <summary>
        /// BID方向有效日期
        /// </summary>
        public string validDateBid;

        /// <summary>
        /// OFR方向有效日期
        /// </summary>
        public string validDateOfr;

        /// <summary>
        /// BID方向回复有效时间
        /// </summary>
        public string quoteValidUntilTimeBid;

        /// <summary>
        /// OFR方向回复有效时间
        /// </summary>
        public string quoteValidUntilTimeOfr;

        /// <summary>
        /// BID方向重发阈值
        /// </summary>
        public string resendThresholdBid;

        /// <summary>
        /// OFR方向重发阈值
        /// </summary>
        public string resendThresholdOfr;

        /// <summary>
        /// BID方向回量上限
        /// </summary>
        public string maxQtyBid;

        /// <summary>
        /// OFR方向回量上限
        /// </summary>
        public string maxQtyOfr;

        // 一般报价设置 --------------------------------------------------- //

        /// <summary>
        /// 手动报价-BID
        /// </summary>
        public string manualBid;

        /// <summary>
        /// 手动报价-OFR
        /// </summary>
        public string manualOfr;

        /// <summary>
        /// 手动报价-调整步长
        /// to do ... 
        /// </summary>
        public string manualSpread = "0.25";

        // 防守设置 --------------------------------------------------- //
        
        /// <summary>
        /// bid防守报价 显示用
        /// </summary>
        public string bidDefenCalPrice;

        /// <summary>
        /// ofr防守报价 显示用
        /// </summary>
        public string ofrDefenCalPrice;

        /// <summary>
        /// 防守点报价-BID =  bidAlgoDefenValue
        /// </summary>
        public string defBid;

        /// <summary>
        /// 防守点报价-OFR = bidAlgoSafeValue
        /// </summary>
        public string defOfr;

        /// <summary>
        /// 全局强墙Bid --用于显示
        /// </summary>
        public string defWallPriceBid;

        /// <summary>
        /// 全局强墙Ofr --用于显示
        /// </summary>
        public string defWallPriceOfr;

        /// <summary>
        /// 防守报价-全场最优OFR    = MAX 算法安全点 bidAlgoSafeValue
        /// </summary>
        public string defMaxOfr;

        /// <summary>
        /// 防守报价-全场最优BID = Min 算法安全点 bidAlgoTrigValue
        /// </summary>
        public string defMaxBid;

        /// <summary>
        /// Bid触墙拉宽-BID墙   = bidAlgoTrigValue
        /// </summary>
        public string defWallBid;

        /// <summary>
        /// Ofr触墙拉宽-OFR墙  = ofrAlgoTrigValue
        /// </summary>
        public string defWallOfr;

        // 算法报价 --------------------------------------------------- //
        /// <summary>
        /// bid算法报价 显示用
        /// </summary>
        public string bidAlgoCalPrice;

        /// <summary>
        /// ofr算法报价 显示用
        /// </summary>
        public string ofrAlgoCalPrice;

        /// <summary>
        /// bid算法报价
        /// </summary>
        public string algoBidPrice;
        
        /// <summary>
        /// ofr算法报价
        /// </summary>
        public string algoOfrPrice;

        /// <summary>
        /// 算法报价-中间价类型  == algoMidType 1 双边市场中间价，2：中债估值，3：自定义 ""为null
        /// </summary>
        public string algoMidPriceType;

        /// <summary>
        /// 算法报价-中间价值
        /// to do...
        /// </summary>
        public string algoMidPrice;

        /// <summary>
        /// 是否使用基准债券0：否、1：是
        /// to do...
        /// </summary>
        public string algoBaseEnable;

        /// <summary>
        /// 算法报价-基准券
        /// </summary>
        public string algoBaseSecurityId;

        /// <summary>
        /// 参考中间价详情 显示用
        /// </summary>
        public string algoMidDetail;

        /// <summary>
        /// 中债估值详情 显示用
        /// </summary>
        public string algoCDCDetail;

        /// <summary>
        /// 算法报价-算法偏离  == algoMidOffsetValue
        /// </summary>
        public string algoDeviation;

        /// <summary>
        /// 算法报价-算法点差  == algoValue
        /// </summary>
        public string algoSpread;

        // 接口报价 --------------------------------------------------- //
        /// <summary>
        /// 接口报价-接口状态 == apiLastTime 显示用
        /// </summary>
        public string apiStatus;

        /// <summary>
        /// 接口报价-BID价格 显示用
        /// </summary>
        public string apiBid;

        /// <summary>
        /// 接口报价-OFR价格 显示用
        /// </summary>
        public string apiOfr;

        /// <summary>
        /// 接口报价-BID报量 显示用
        /// </summary>
        public string apiBidQty;

        /// <summary>
        /// 接口报价-OFR报量 显示用
        /// </summary>
        public string apiOfrQty;
 
        /// <summary>
        /// 成交达成后不再自动回复,0：不回复，1：回复
        /// </summary>
        public int autoReply;

        /// <summary>
        /// 触发ACT后是否撤单，0：不撤单，1：撤单
        /// </summary>
        public int actCancel;


        #region 中债估值缓存
        /// <summary>
        /// 中债估值
        /// </summary>
        public string Valuation;
        /// <summary>
        /// 中债估值日期 yyyyMMdd
        /// </summary>
        public string CCDCValuaDate;

        /// <summary>
        /// 中债估值日期是否等于最后的交易日 不等于显示 ！
        /// </summary>
        public string ShowLastWorkDay;
        #endregion
    }
}
