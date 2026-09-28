using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class Consts
    {
        public static readonly string Topic_Clear = "STC_Clear";

        //利率
        public static readonly string BNC = "BNC";
        //NCD
        public static readonly string NCD = "NCD";
        //信用
        public static readonly string BCO = "BCO";
        //ABS
        public static readonly string ABS = "ABS";

        public const string BIDOFRPRICE = "对价";
        public const string LATESTPRICE = "最新价";
        public const string MARKETPRICE = "市价";

        public const string SPECULATE = "投机";
        public const string ARBITRAGE = "套利";
        public const string HEDGE = "套保";

        public const string SPECULATE_CODE = "1";
        public const string ARBITRAGE_CODE = "2";
        public const string HEDGE_CODE = "3";

        //开平仓标志代码
        public const string OPEN_CODE = "0";
        public const string CLOSE_CODE = "1";
        public const string CLOSE_TODAY_CODE = "2";
        public const string CLOSE_YESTERDAY_CODE = "3";
        public const string OPEN = "开仓";
        public const string CLOSE = "平仓";
        public const string CLOSE_TODAY = "平今";
        public const string CLOSE_YESTERDAY = "平昨";

        //债券
        public const string MARKETINDICATOR_BOND = "4";
        //国债期货
        public const string MARKETINDICATOR_FUTURE = "5";
        //
        public static readonly string Topic_RestartApp = "STC_RestartApp";

        //价格最小变动单位
        public const string PRICE_MIN_CHANGE_00001 = "0.0001";
        public const string PRICE_MIN_CHANGE_0002 = "0.002";
        public const string PRICE_MIN_CHANGE_0001 = "0.001";
        public const string PRICE_MIN_CHANGE_0005 = "0.005";
        public const string PRICE_MIN_CHANGE_002 = "0.02"; 
        public const string PRICE_MIN_CHANGE_005 = "0.05";
        public const string PRICE_MIN_CHANGE_01 = "0.1";
        public const string PRICE_MIN_CHANGE_02 = "0.2";
        public const string PRICE_MIN_CHANGE_05 = "0.5";
        public const string PRICE_MIN_CHANGE_1 = "1.0";
        public const string PRICE_MIN_CHANGE_2 = "2.0";
        public const string PRICE_MIN_CHANGE_5 = "5.0"; 
        public const string PRICE_MIN_CHANGE_10 = "10.0";

        public const decimal DEC_PRICE_MIN_CHANGE_0002 = 0.002m;
        public const decimal DEC_PRICE_MIN_CHANGE_0005 = 0.005m;
        public const decimal DEC_PRICE_MIN_CHANGE_002 = 0.02m;
        public const decimal DEC_PRICE_MIN_CHANGE_005 = 0.05m;
        public const decimal DEC_PRICE_MIN_CHANGE_01 = 0.1m;
        public const decimal DEC_PRICE_MIN_CHANGE_02 = 0.2m;
        public const decimal DEC_PRICE_MIN_CHANGE_05 = 0.5m;
        public const decimal DEC_PRICE_MIN_CHANGE_1 = 1.0m;
        public const decimal DEC_PRICE_MIN_CHANGE_2 = 2.0m;
        public const decimal DEC_PRICE_MIN_CHANGE_5 = 5.0m;
        public const decimal DEC_PRICE_MIN_CHANGE_10 = 10.0m;
        //期货交易所代码
        public const string CFFEX = "CFFEX";    //中金所
        public const string SHFE = "SHFE";      //上期所
        public const string CZCE = "CZCE";      //郑商所
        public const string DCE = "DCE";        //大商所
        public const string INE = "INE";        //上期能源
        public const string CTP = "CTP";        //CTP

        public const string BONDTYPE = "BOND_TYPE";//
        public const string TERM = "TERM";//
        public const string SIDE = "SIDE";
        public const string VIP = "VIP";
        public const string FIRM = "FIRM";
        public const string SETTLTYPE = "SETTL_TYPE";
        public const string ORGTYPE = "ORG_TYPE";
        public const string ROLETYPE = "ROLE_TYPE";
        public const string FINBONDTYPE = "FIN_BOND_TYPE";//
        public const string DQTYPE = "DQ_TYPE";//

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
        public const string BID_SUB_VAL_INF3 = "BID_SUB_VAL";//bid-中债
        public const string VAL_SUB_OFR_INF3 = "VAL_SUB_OFR";//中债-ofr

        public static string IF_EXERCISE = "IF_EXERCISE";// 是否已行权

        public static string IF_REPAY = "IF_REPAY";//还本

        public static string IF_PLEDGED = "IF_PLEDGED";//可质押


        public static string IF_MARKET_MAKING = "IF_MARKET_MAKING";//是否做市
        public static string IF_DEFAULT = "IF_DEFAULT";//是否违约
        public static string INNOVATION_TYPE = "INNOVATION_TYPE";//创新债类型
        public static string IF_TIER2_PERP = "IF_TIER2_PERP";//是否二级资本永续债
        public static string BOND_SUB_TYPE2 = "BOND_SUB_TYPE2";//BOND_SUB_TYPE2

        public static string IF_TAX_FREE = "IF_TAX_FREE";// 是否免税

        public static char[] COMMA_CHAR = new char[] { ',' };

        public static Dictionary<string, string> TagDic = new Dictionary<string, string>
        {
            { "Home", "主页" },
            { "Quant", "策略"},
            { "INNER", "内部看板"},
            { "MM", "双边做市"},
            { "MMSHE", "做市工作台"}
        };

        // 平安养老账号类型
        public static string IR = "ir";//利率债
        public static string CFP = "cfp";//商金二永
        public static string IB = "ib";//银行间
        public static string CP = "cp";//公司债
    }
}
