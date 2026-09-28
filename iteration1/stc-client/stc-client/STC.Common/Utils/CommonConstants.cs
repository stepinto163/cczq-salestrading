using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class CommonConstants
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
        public const string SSE = "上新债";
        public const string SSF = "上固收";
        public const string SZE = "深匹配";
        public const string SZF = "深点击";

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

        // merge STC.Frame.Utils.CommonConstants
        public const string SIDE_BID_ONE = "1";
        public const string SIDE_OFR_TWO = "2";
        public const string SIDE_TRD_THREE = "3";

        public const string OrderEnd = "X";

        public const string AllBroker = "全部";
        public const string ALL_EN = "ALL";
        public const string CNEX = "国际";
        public const string TPSC = "国利";
        public const string CTTJ = "信唐";
        public const string PTCN = "平安";
        public const string CCTB = "中诚";
        public const string UEDA = "上田";
        public const string ALL = "全部";
        public const string ALL_PARTY = "全";
        public const string QDM = "RFQ";
        public const string NDM = "协商";

        public static char[] COMMA_CHAR = new char[] { ',' };
        public static char[] PLUS_CHAR = new char[] { '+' };
        public static char[] MID_LINE_CHAR = new char[] { '-' };
        public static char[] COLON_CHAR = new char[] { ':' };
        public static char[] SEMICOLON_CHAR = new char[] { ';' };
        public static char[] BAR_CHAR = new char[] { '|' };
        public static char[] SPACE_CHAR = new char[] { ' ' };
        public const int pageCount = 1000;

        public const string T2_T3 = "2,3";
        public const string T2_3_CN = "T+1(不限)";


        public const string T1_ONE = "1";
        public const string T2_TWO = "2";
        public const string T3_THREE = "3";
        public const string T4_FOUR = "4";

        public const string NO_PRICE = "- -";
        public const string NO_PERMISSION = "**";

        public const string SIDE_X = "X";//ofr
        public const string SIDE_Y = "Y";//bid
        public const string SIDE_Z = "Z";//trd



        // product 品种
        public const string PRODUCT_FIELD = "期限";
        public const string FR007_FIELD = "FR007";
        public const string Shibor3M_FIELD = "Shibor3M";
        public const string LPR1Y_FIELD = "LPR1Y";
        public const string LPR5Y_FIELD = "LPR5Y";
        public const string FDR007_FIELD = "FDR007";
        public const string GB10_FIELD = "GB10";
        public const string CDB10_FIELD = "CDB10";
        public const string D10G1_FIELD = "D10/G10";
        public const string AAA3D3_FIELD = "AAA3/D3";
        public const string ShiborON_FIELD = "ShiborO/N";
        public const string FDR001_FIELD = "FDR001";
        public const string S3MR07_FIELD = "S3M/R07";

        public const string TERM_1M_FIELD = "1M";
        public const string TERM_3M_FIELD = "3M";
        public const string TERM_6M_FIELD = "6M";
        public const string TERM_9M_FIELD = "9M";
        public const string TERM_1Y_FIELD = "1Y";
        public const string TERM_2Y_FIELD = "2Y";
        public const string TERM_3Y_FIELD = "3Y";
        public const string TERM_4Y_FIELD = "4Y";
        public const string TERM_5Y_FIELD = "5Y";
        public const string TERM_7Y_FIELD = "7Y";
        public const string TERM_10Y_FIELD = "10Y";
        public const string TERM_3M6M_FIELD = "3M*6M";
        public const string TERM_6M9M_FIELD = "6M*9M";
        public const string TERM_6M1Y_FIELD = "6M*1Y";
        public const string TERM_9M1Y_FIELD = "9M*1Y";
        public const string TERM_1Y2Y_FIELD = "1Y*2Y";
        public const string TERM_1Y5Y_FIELD = "1Y*5Y";
        public const string TERM_2Y5Y_FIELD = "2Y*5Y";

        public const string CDCValueTxt = "中债估值:";

        //BDB
        public const string BrokerNumEnum = "1,2,3,4,5,6";
        public const string EXECTYPE = "ExecType";//0:列表， 1:图形
        public const string NO_BROKER = "***";//✲❊
        public const string ToTop = "回到顶部";
        public const string RefreshTxt = "出现新数据，点击刷新";
        public const string SIDE_FUTURE_FOUR = "4";
        public const string SIDE_FUTURE_FIVE = "5";
        public const string FWD_CN = "远期";
        public const string FWD_EN = "FWD";
        public const string QUANT_Type = "Quant";
        public const string HOME_Type = "Home";
        public const string INNER_Type = "INNER";
        public const string MM_Type = "MM";
        public const string MMSHE_Type = "MMSHE";
        public const string QUANT_Type_Msg = "策略";
        public const string HOME_Type_Msg = "主页";
        public const string INNER_Type_Msg = "内部看板";
        public const string MM_Type_Msg = "双边做市";
        public const string MMSHE_Type_Msg = "做市工作台";

        //查看机构内部权限
        public const string MODELID_5000 = "5000";
        public const string CODE_5002 = "5002";

        //// FQ消息默认前缀
        //public const string FQ_PREFIX = "有关这个报价:";
    }
}
