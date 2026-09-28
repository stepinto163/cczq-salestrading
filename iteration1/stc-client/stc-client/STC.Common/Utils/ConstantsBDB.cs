using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class ConstantsBDB
    {
        public const string RATE = "利率";
        public const string CREDIT = "信用";

        public const string VENUES_CUSTOME = "venues";

        public const string AllBroker = "全部";
        public const string ALL_EN = "All";
        public const string CNEX = "国际";
        public const string TPSC = "国利";
        public const string CTTJ = "信唐";
        public const string PTCN = "平安";
        public const string CCTB = "中诚";
        public const string UEDA = "上田";

        public const string CNEX_EN = "CNEX";
        public const string TPSC_EN = "TPSC";
        public const string CTTJ_EN = "CTTJ";
        public const string PTCN_EN = "PTCN";
        public const string CCTB_EN = "CCTB";
        public const string UEDA_EN = "UEDA";

        public const string ALL = "全部";
        public const string ALL_PARTY = "全";

        public const string INNER = "内部";
        public const string MANUAL = "人工";
        public const string IDB = "IDB";
        public const string QUANT = "量化";

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

        public static char[] COMMA_CHAR = new char[] { ',' };
        public static char[] PLUS_CHAR = new char[] { '+' };
        public static char[] MID_LINE_CHAR = new char[] { '-' };
        public const int pageCount = 1000;

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

        public const string T2_T3 = "2,3";
        public const string T2_3_CN = "T+1(不限)";

        public const string NOW = "现";
        public const string FWD = "远";
        public const string FWD_CN = "远期";
        public const string FWD_EN = "FWD";
        public const string FWD_NUM = "5";
        public const string T1_EN = "+0";
        public const string T2_EN = "+1";
        public const string T1 = "T+0";
        public const string T2 = "T+1";
        public const string T3 = "明天+0";
        public const string T4 = "明天+1";

        public const string T1_ONE = "1";
        public const string T2_TWO = "2";
        public const string T3_THREE = "3";
        public const string T4_FOUR = "4";

        public const string T2_PART = "T+1(今+1)";
        public const string T3_PART = "T+1(明+0)";
        public const string T4_PART = "T+1(不限)";

        public const string SHOW_QUICK_EXEC = "是否录入快速线下成交?";
        public const string SHOW_QUICK_EXEC_WARN = "该操作将撤销本笔订单，并打开线下录入窗口";

        public const string SHANXIZHENGQUAN = "山西证券";

        public const string SIDE = "side";
        public const string SIDE_BID = "0";
        public const string SIDE_OFR = "1";

        public const string SIDE_BID_ONE = "1";
        public const string SIDE_OFR_TWO = "2";

        public const string SIDE_TRD_THREE = "3";

        public const string SIDE_FUTURE_FOUR = "4";
        public const string SIDE_FUTURE_FIVE = "5";

        public const string SIDE_X = "X";//ofr
        public const string SIDE_Y = "Y";//bid
        public const string SIDE_Z = "Z";//trd


        public const string UP = "▲";
        public const string DOWN = "▼";
        public const string NO_UP_DOWN = "━";

        public const string NO_PRICE = "- -";

        public const string NO_BROKER = "***";//✲❊

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

        public const string ToTop = "回到顶部";
        public const string RefreshTxt = "出现新数据，点击刷新";








        public static readonly string TOPIC_SIMULATE_ORDER = "stclite.s.o";


        public const string TBookSvr = "TBookSvr";
        public const string Topic_stc_tbook_placeOrder = "BDB.tbook.o.n";
        public const string Topic_stc_tbook_replaceOrder = "BDB.tbook.o.r";
        public const string Topic_stc_tbook_cancelOrder = "BDB.tbook.o.c";
        public const string Topic_stc_tbook_orderQuery = "orderQuery";

        public const string BaseSvr = "BaseSvr";
        public const string Topic_stc_base_queryAccount = "queryAccount";
        public const string Topic_stc_base_mqAccount = "BDB.base.mq.account";
        public const string Topic_stc_base_clear = "BDB.base.clear";

        //预埋单激活或撤销
        public const string Topic_pre_activation = "BDB.event.publish";
        public static string Topic_stc_tbook_prefix = "BDB.tbook.s.";

        public static int delayTime = 5;

        public const string CODE_7000 = "7000";
        public const string MSG_7000 = "发送失败，请登录QM";
        public const string CODE_7001 = "7001";
        public const string MSG_7001 = "发送失败，未找到对应Broker";

        public static Dictionary<string, string> CompIDHt = new Dictionary<string, string>()
        {
            { "TPSC", "1" },
            { "CNEX", "2" },
            { "CCTB", "3" },
            { "PTCN", "4" },
            { "CTTJ", "5" }
        };

        public static Dictionary<string, string> CompVenueHt = new Dictionary<string, string>()
        {
            { "1", "TPSC" },
            { "2", "CNEX" },
            { "3", "CCTB" },
            { "4", "PTCN" },
            { "5", "CTTJ" }
        };

        public static string STC_Price = "PriceSvr";
        public static string STC_Trade = "TradeSvr";
        //查询口
        public static string queryPrice = "priceQueryHandler";
        public static string queryDeal = "dealQueryHandler";
        public static string queryOrder = "queryExecutionReport";
        //订阅口
        public static string priceTopic = "dbdd.broker.price.";
        public static string dealTopic = "dbdd.broker.deal.";
        public static string orderTopic = "dbdd.broker.order.";
        public static string orderCancelRejectTopic = "dbdd.broker.cancel.reject.";

        public const string Topic_dps_qtr = "BDB.qtr.";
        public const string Topic_dps_qtr_netStatus = "BDB.qtr.t.";
        public const string Topic_dps_qtr_quote = "BDB.qtr.p.";
        public const string Topic_dps_qtr_order = "BDB.qtr.order.";
        public const string Topic_dps_qtr_trade = "BDB.qtr.o.";

        public const int pageSize = 1000;

        public const string AdminSvr = "AdminSvr";
        public const string Topic_stc_admin_systemPara = "BDB.admin.systemPara";

        public const string BrokerNumEnum = "1,2,3,4,5,6";


    }
}
