using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 常量类
    /// </summary>
    public class Constants
    {
        public const string RATE = "利率";
        public const string CREDIT = "信用";

        public const string VENUES_CUSTOME = "venues";

        public const string CNEX_EN = "CNEX";
        public const string TPSC_EN = "TPSC";
        public const string CTTJ_EN = "CTTJ";
        public const string PTCN_EN = "PTCN";
        public const string CCTB_EN = "CCTB";
        public const string UEDA_EN = "UEDA";

        public const string INNER = "内部";
        public const string MANUAL = "人工";
        public const string IDB = "IDB";
        public const string QUANT = "量化";
        public const string QDMESP = "做市";

        public const string XBond = "X-Bond";
        public const string TBook = "Tradebook";

        public const string FIRM = "firm";
        public const string NONFIRM = "nonfirm";

        public const string SEND = "已发送";
        public const string SOLVE = "待处理";

        public const string UNSOLVE = "待处理";
        public const string SOLVEING = "处理中";
        public const string EXECUTED = "已成交";
        public const string CANCELED = "已撤销";

        public const string CANCELING = "撤销中";
        public const string WARICANCEL = "待撤销";
        public const string PARTEXECUTED = "部分成交";
        public const string EXCEPTION = "异常";

        public const string ISOCO = "独家Firm";//"OCO订单";
        public const string NOTOCO = "非独家Firm";//"非OCO订单";

        public const string MYOPERATION = "我的";
        public const string NOOPERATION = "未指定";

        public const string VALID = "未完成";
        public const string INVALID = "完成";
        public const string NEWPENDING = "新建中";
        public const string NORMAL = "正常";
        public const string PartiallyFilled = "部分成交";
        public const string Filled = "完全成交";
        public const string Cancelled = "已撤销";
        public const string Cancelling = "撤销中";
        public const string Rejected = "已拒绝";
        public const string Exception = "异常";


        public const string URGENT = "紧急";
        public const string MAJOR = "重要";
        public const string GENERAL = "一般";
        public const string ONEWARN = "提醒";
        public const string TWOWARN = "2次提醒";
        public const string FORBIDWARN = "禁止";
        public const string MANUAL1 = "手工";

        public const string HISOrders = "HisOrders";
        public const string HISEMSOrders = "HisEMSOrders";
        public const string HISDeals = "HisDeals";
        public const string HISRisks = "HisRisks";
        public const string HISMsgs = "HisMsgs";
        public const string HISAgens = "HisAgens";

        public const string ONLINE = "线上执行";
        public const string VENUES = "渠道执行";
        public const string UNDERLINE = "线下补录";

        public const string TRANSFERINTERNAL = "内部划转";
        public const string EXCHANGE = "成交";

        // 线下补录面板类型
        public const string IDB_PART = "idb";
        public const string BROKER_PART = "broker";
        public const string SELF_PART = "self";
        public const string MARKET_PART = "market";
        public const string IDB_UNDERLINE_PART = "idb_underline";
        public const string BROKER_UNDERLINE_PART = "broker_underline";
        public const string SELF_UNDERLINE_PART = "self_underline";
        public const string MARKET_UNDERLINE_PART = "market_underline";

        public const string PUBLIC_ACCOUNT = "公账";

        public const string YYYYMMDDHHMMSSFFF_DATEFORMAT = "yyyy-MM-dd HH:mm:dd.fff";


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

        public const string SHOW_QUICK_EXEC = "是否录入快速线下成交?";
        public const string SHOW_QUICK_EXEC_WARN = "该操作将撤销本笔订单，并打开线下录入窗口";

        public const string SHANXIZHENGQUAN = "山西证券";

        public const string SIDE = "side";
        public const string SIDE_BID = "0";
        public const string SIDE_OFR = "1";

        public const string UP = "▲";
        public const string DOWN = "▼";
        public const string NO_UP_DOWN = "━";


        public const string NA = "N/A";

        public const string BROKER = "Broker";
        public const string SETTLE = "Settle";
        public const string BOND_TYPE = "BondType";
        public const string REMAINING_TIME = "RemainingTime";
        public const string SECURITYID = "SecurityId";

        public const string GVN = "GVN";
        public const string TKN = "TKN";
        public const string TRD = "TRD";

        public const string PointOrder = "需求";
        public const string PriceOrder = "意向";

        public const string BNC = "BNC";
        public const string BCO = "BCO";

        public const string WaitConfirm = "待确认";
        public const string Confirmed = "已确认";

        public const string BOND = "债券";
        public const string FUTURE = "期货";
        public const string IRS = "IRS";

        #region
        //public const string BONDTYPE = "BOND_TYPE";//
        //public const string TERM = "TERM";//
        //public const string SIDE = "SIDE";
        //public const string VIP = "VIP";
        //public const string FIRM = "FIRM";
        public const string SETTLTYPE = "SETTL_TYPE";
        //public const string ORGTYPE = "ORG_TYPE";
        public const string ROLETYPE = "ROLE_TYPE";
        //public const string FINBONDTYPE = "FIN_BOND_TYPE";//
        //public const string DQTYPE = "DQ_TYPE";//

        public const string LOCAL_GOV_BOND_TYPE = "LOCAL_GOV_BOND_TYPE";
        public const string CAL_INTER_TYPE = "CAL_INTER_TYPE";
        public const string PERPETUAL_TYPE = "PERPETUAL_TYPE";//永续类型
        public const string BOND_SUB_TYPE = "BOND_SUB_TYPE";//信用类型
        public const string ISSUE_TYPE = "ISSUE_TYPE";//发行类型
        public const string RANKING_TYPE = "RANKING_TYPE";//评级分类
        public const string IF_MUNICIPAL = "IF_MUNICIPAL";//城投
        public const string IF_GUARANTEE = "IF_GUARANTEE";//担保类型
        public const string CUM_TYPE = "CUM_TYPE";//含权类型\
        public const string ENTERPRISE_TYPE = "ENTERPRISE_TYPE";//企业类型
        public const string IF_PLATFORM = "IF_PLATFORM";//平台债类型
        public const string HOLIDAY_TYPE = "HOLIDAY_TYPE";//节假/工作日类型

        public const string INDUSTRY_TYPE = "INDUSTRY_TYPE";//行业
        public const string LISTED_MARKET = "LISTED_MARKET";//流通场所
        public const string PRINCIPAL_TYPE = "PRINCIPAL_TYPE";//主体类型/银行类型
        public const string ISSUE_YEAR = "ISSUE_YEAR";//年份
        public const string REGION_TYPE = "REGION_TYPE";//地区

        public const string RATING_CURRENT = "RATING_CURRENT";//债项评级
        public const string CDC_IMPLITED_RATING = "CDC_IMPLITED_RATING";//中债隐含评级
        public const string ISSUER_RATING_CURRENT_CBR = "ISSUER_RATING_CURRENT_CBR";//中债资信评级
        public const string CSI_IMPLIED_RATING = "CSI_IMPLIED_RATING";//中证隐含
        public const string NCD_TYPE = "NCD_TYPE";//NCD类型

        public const string ISSUER = "ISSUER"; // 发行人
        public const string GUARANTOR = "GUARANTOR"; //担保人

        public const string FIN_INST_BOND = "FIN_INST_BOND";//金融机构债
        public const string ADMIN_LEVEL = "ADMIN_LEVEL";//行政级别


        public const string TERM_INF3 = "TERM";//inf3回显
        public const string COUPON_INF3 = "COUPON";//inf3回显
        public const string PRICE_INF3 = "PRICE";//inf3回显
        public const string CHINABONDVALUE_INF3 = "VALUATION";//inf3回显

        public static string IF_EXERCISE = "IF_EXERCISE";// 是否已行权

        public static string IF_REPAY = "IF_REPAY";//还本

        public static string IF_PLEDGED = "IF_PLEDGED";//可质押
        #endregion
    }
}
