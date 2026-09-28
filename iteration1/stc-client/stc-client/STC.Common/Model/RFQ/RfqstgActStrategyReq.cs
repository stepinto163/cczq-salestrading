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
    public class RfqstgActStrategyReq
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

        
        // 防守设置 --------------------------------------------------- //
        /// <summary>
        /// 防守点报价-BID =  bidAlgoDefenValue
        /// </summary>
        public string defBid;

        /// <summary>
        /// 防守点报价-OFR = bidAlgoSafeValue
        /// </summary>
        public string defOfr;

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
        /// 算法报价-算法偏离  == algoMidOffsetValue
        /// </summary>
        public string algoDeviation;

        /// <summary>
        /// 算法报价-算法点差  == algoValue
        /// </summary>
        public string algoSpread;

        // 接口报价 --------------------------------------------------- //
        /// <summary>
        /// 成交达成后不再自动回复,0：不回复，1：回复
        /// </summary>
        public int autoReply;

        /// <summary>
        /// 触发ACT后是否撤单，0：不撤单，1：撤单
        /// </summary>
        public int actCancel;
    }
}
