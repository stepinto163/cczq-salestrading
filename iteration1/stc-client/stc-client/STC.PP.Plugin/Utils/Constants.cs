using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.PP.Plugin.Utils
{
    public class Constants
    {
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

        public const string ALL = "ALL";
        public static string ADDED = "已加入";//已加入
        public static string UNADD = "未加入";//未加入
    }
}
