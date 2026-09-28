using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mm.Plugin.Utils
{
    /// <summary>
    /// 常量类
    /// </summary>
    public class MMConstants
    {
        public const string NOW = "现";
        public const string FWD = "远";
        public const string FWD_NUM = "5";
        public const string T1_EN = "+0";
        public const string T2_EN = "+1";
        public const string T1 = "T+0";
        public const string T2 = "T+1";
        public const string T3 = "明天+0";
        public const string T4 = "明天+1";


        public const string T2_PART = "T+1(今+1)";
        public const string T3_PART = "T+1(明+0)";
        public const string T4_PART = "T+1(不限)";

        public const string GVN = "GVN";
        public const string TKN = "TKN";
        public const string TRD = "TRD";

        public const string UP = "▲";
        public const string DOWN = "▼";
        public const string NO_UP_DOWN = "━";

        public const string BROKER = "Broker";
        public const string SETTLE = "Settle";
        public const string BOND_TYPE = "BondType";
        public const string REMAINING_TIME = "RemainingTime";
        public const string SECURITYID = "SecurityId";

        public const string BNC = "BNC";
        public const string BCO = "BCO";

        public const string MMStatus_UpStandardToolTip = "最低做市义务已达到";
        public const string MMStatus_CanStandardToolTip = "最低做市义务未达到，且近日还能30分钟以上buffer可安全达到";
        public const string MMStatus_DangerToolTip = "最低做市义务未达到，今日还能达到，但是情况危险";
        public const string MMStatus_FailureToolTip = "最低做市义务未达到，且今日无法达到";
        public const string MMStatus_PauseToolTip = "做市债券若此刻没有进行双边报价，则后加入&quot;暂停&quot;标志 ";

        #region 批量导入导出字段
        public const string SecurityID_ExcelField = "债券代码";
        public const string BidQuoteModel_ExcelField = "策略类型Bid";
        public const string OfrQuoteModel_ExcelField = "策略类型Ofr";
        public const string BidBaseQty_ExcelField = "报量Bid(KW)";
        public const string OfrBaseQty_ExcelField = "报量Ofr(KW)";
        public const string ManualBid_ExcelField = "手动报价Bid(%)";
        public const string ManualOfr_ExcelField = "手动报价Ofr(%)";
        public const string StepLength_ExcelField = "调整步长";
        //public const string BidBasePriority_ExcelField = "报价优先级Bid";
        //public const string OfrBasePriority_ExcelField = "报价优先级Ofr";
        public const string BidBaseReSendValue_ExcelField = "重发阈值Bid(bp)";
        public const string OfrBaseReSendValue_ExcelField = "重发阈值Ofr(bp)";
        public const string BidBaseWidenValue_ExcelField = "成交拉宽Bid(bp)";
        public const string OfrBaseWidenValue_ExcelField = "成交拉宽Ofr(bp)";
        public const string BaseERReSendType_ExcelField = "部分成交后报量";
        public const string DealWidenLowerTime_ExcelField = "成交拉宽时长下限";
        public const string DealWidenUpperTime_ExcelField = "成交拉宽时长上限";
        public const string BaseSettleType_ExcelField = "清算速度";
        public const string BidAlgoDefenValue_ExcelField = "防守点差Bid";
        public const string OfrAlgoDefenValue_ExcelField = "防守点差Ofr";
        public const string AlgoSafeEnable_ExcelField = "是否启用全市场反向最优安全点";
        public const string BidAlgoSafeValue_ExcelField = "全市场反向最优安全点差";
        public const string SecondSafeEnable_ExcelField = "是否启用全市场同向次最优安全点";
        public const string QdmespSafeEnable_ExcelField = "是否启用双边同向安全点";
        public const string QdmespSafeLevel_ExcelField = "双边同向安全点档位";
        public const string BidQdmespSafeValue_ExcelField = "双边同向安全点差";
        //public const string OfrAlgoSafeValue_ExcelField = "安全点差Ofr";
        public const string BidAlgoTrigValue_ExcelField = "触发安全点报价点差Bid";
        public const string OfrAlgoTrigValue_ExcelField = "触发安全点报价点差Ofr";
        //public const string AlgoEnable_ExcelField = "是否启用算法计算";
        public const string AlgoBaseEnable_ExcelField = "是否使用基准券";
        public const string AlgoBaseSecurityid_ExcelField = "基准券";
        public const string AlgoMidType_ExcelField = "参考中间价";
        public const string AlgoMidDetailDefined_ExcelField = "自定义中间价";
        public const string AlgoMidOffsetValue_ExcelField = "中间价偏离(bp)";
        public const string AlgoValue_ExcelField = "算法点差";
        //public const string AlgoCDCEnable_ExcelField = "策略无报价，是否采用中债估值报价";
        //public const string _ExcelField = "估值偏离(bp)";
        //public const string _ExcelField = "估值点差";
        public const string PosLmtEnable_ExcelField = "是否允许报价更激进";
        public const string BidPosValue_ExcelField = "更优报价点差Bid";
        public const string OfrPosValue_ExcelField = "更优报价点差Ofr";
        public const string PosFakEnable_ExcelField = "是否报反向内部单";
        public const string PosFakSpread_ExcelField = "内部单点差";

        public const string MaxFloorEnable_ExcelField = "是否使用冰山订单";
        public const string MaxFloor_ExcelField = "冰山量(KW)";
        public const string DefenCDCEnable_ExcelField = "是否防守策略无可报价格时锚定中债估值";
        public const string BidDefenCDCValue_ExcelField = "估值拉宽Bid";
        public const string OfrDefenCDCValue_ExcelField = "估值拉宽Ofr";
        public const string BidDefenReferMarketType_ExcelField = "Bid防守参考市场价类型";
        public const string BidDefenReferLevel_ExcelField = "Bid防守参考市场价档位";
        public const string OfrDefenReferMarketType_ExcelField = "Ofr防守参考市场价类型";
        public const string OfrDefenReferLevel_ExcelField = "Ofr防守参考市场价档位";
        public const string QuoteModel_ExcelField = "策略类型";

        public const string WallEnable_ExcelField = "是否使用墙";
        public const string ExeBondFollow_ExcelField = "债券跟随类型";
        #endregion

        #region 做市详情批量导出字段
        public const string AnsDetail_DateField = "日期";
        public const string AnsDetail_SecurityIdField = "债券代码";
        public const string AnsDetail_SymbolField = "债券简称";
        public const string AnsDetail_SymbolTypeField = "债券种类";
        public const string AnsDetail_UsernameField = "报价人";
        public const string AnsDetail_SecuritySuperTypeField = "债券类型";
        public const string AnsDetail_TimetomaturityField = "剩余期限";
        public const string AnsDetail_TimetomaturityGroupField = "剩余期限分组";
        public const string AnsDetail_QuoteTimeLengthField = "有效报价时长";
        public const string AnsDetail_ExecutionCountField = "成交笔数";
        public const string AnsDetail_ExecutionQtyField = "总成交量(万)";
        public const string AnsDetail_AvgBPField = "报价点差(bp)";
        public const string AnsDetail_QuoteBeginTimeField = "报价开始时间";
        public const string AnsDetail_QuoteEndTimeField = "报价结束时间";
        #endregion

        #region 做市概要批量导出字段
        public const string AnsOutline_DateField = "日期";
        public const string AnsOutline_BncSecurityCountField = "利率债有效报价券数";
        public const string AnsOutline_BncSecurityTypeCountField = "利率债品种分布";
        public const string AnsOutline_BncTimetomaturityCountField = "剩余期限分布";
        public const string AnsOutline_BncNDCountField = "国债数量";
        public const string AnsOutline_BncNewNDCountField = "新发国债期限分布";
        public const string AnsOutline_BcoSecurityCountField = "信用债有效报价券数";
        public const string AnsOutline_BcoSecurityTypeCountField = "信用债品种分布";
        public const string AnsOutline_BcoIssuerCountField = "发行人分布";
        #endregion

        #region 导出属性
        public const string MANUAL_QUOTEMODEL = "手动";
        public const string DEFEND_QUOTEMODEL = "防守";
        public const string ALGORITHM_QUOTEMODEL = "算法";
        public const string API_QUOTEMODEL = "接口";
        public const string LEAVE_BASEERRESENDTYPE = "按剩余挂量拉宽后重发";
        public const string QTY_BASEERRESENDTYPE = "按单笔报量拉宽后重发";
        public const string Mid_ALGOMIDTYPE = "双边市场中间价";
        public const string Val_ALGOMIDTYPE = "中债估值";
        public const string Cus_ALGOMIDTYPE = "自定义";
        public const string T0_SETTLTYPE = "T+0";
        public const string T1_SETTLTYPE = "T+1";
        public const string YES_VALUE = "是";
        public const string NO_VALUE = "否";
        public const string ZERO_VALUE = "0";
        public const string DEALWIDEN_VALUE = "5";

        public const string QDMESP_DEFENREFERMARKETTYPE = "双边";
        public const string XBOND_DEFENREFERMARKETTYPE = "X-Bond";
        public const string MARKET_DEFENREFERMARKETTYPE = "全市场";
        public const string CFETS_DEFENREFERMARKETTYPE = "CFETS";

        public const string LEVEL1 = "第一档";
        public const string LEVEL2 = "第二档";
        public const string LEVEL3 = "第三档";
        public const string LEVEL4 = "第四档";
        public const string LEVEL5 = "第五档";
        #endregion

        #region
        public const string SECUTIRY_MMExcelField = "代码*";
        public const string SYMBOL_MMExcelField = "名称*";
        public const string MAXFLOOR_MMExcelField = "冰山券面（万元）";
        public const string SENDTO_MMExcelField = "发送对象*";
        public const string SETTL_MMExcelField = "清算速度*";
        public const string SETTLTYPE_MMExcelField = "清算类型*";
        public const string CLEARINGMETHOD_MMExcelField = "结算方式*";
        public const string BIDNETPRICE_MMExcelField = "买入净价（元）*";
        public const string BIDQTY_MMExcelField = "买入券面（万元）*";
        public const string BIDYEILD_MMExcelField = "买入到期（%）*";
        public const string BIDXQ_MMExcelField = "买入行权（%）*";
        public const string BIDINTEREST_MMExcelField = "买入应计利息（元）*";
        public const string BIDLASTQTY_MMExcelField = "买入成交（万元）";
        public const string OFRNETPRICE_MMExcelField = "卖出净价（元）*";
        public const string OFRQTY_MMExcelField = "卖出券面（万元）*";
        public const string OFRYEILD_MMExcelField = "卖出到期（%）*";
        public const string OFRXQ_MMExcelField = "卖出行权（%）*";
        public const string OFRINTEREST_MMExcelField = "卖出应计利息（元）*";
        public const string OFRLASTQTY_MMExcelField = "卖出成交（万元）";
        public const string PARYTID_MMExcelField = "本方交易员";
        public const string MYMID_MMExcelField = "本方中间价";
        public const string CFETSMID_MMExcelField = "双边市场中间价";
        public const string VALUATION_MMExcelField = "中债估值";
        public const string VALUATIONDATE_MMExcelField = "中债估值日期";
        public const string SPREAD_MMExcelField = "点差";
        #endregion

        public static string MM_GROPE_TYPE_NORMAL = "NORMAL";
        public static string MM_GROPE_TYPE_UNIFY = "UNIFY";
    }
}
