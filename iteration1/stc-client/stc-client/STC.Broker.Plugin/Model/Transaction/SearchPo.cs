using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class SearchPo
    {
        /// <summary>
        /// wxp 2000225 查询缺桥单标记 需求定位2.3-3.2 用于搜索缺桥的明细
        /// 非空查询，空不查询
        /// </summary>
        public string SeachBridgeFlag { get; set; }
        /// <summary>
        /// 2.4需求 20200331 用于过滤删除的单子
        /// </summary>
        public string SeachDelFlag { get; set; }
        /// <summary>
        /// wxp 2000225 单号查询 需求定位2.3-3.6  用于单号查询明细
        /// </summary>
        private decimal orderseq = decimal.Zero;
        public decimal Seq
        {
            get { return orderseq; }
            set { orderseq = value; }
        }
        public string BondCode { get; set; }
        /// <summary>
        /// 债券名称
        /// </summary>
        public string BondName { get; set; }
        public string Inst { get; set; }

        /// <summary>
        /// 机构id
        /// </summary>
        public string InstID { get; set; }
        public string TraderInst { get; set; }

        /// <summary>
        /// 机构交易员名称
        /// </summary>
        public string InstTraderName { get; set; }

        public string Trader { get; set; }
        public Ofr Ofr { get; set; }
        public Bid Bid { get; set; }


        public decimal Price { get; set; }
        public string Priceformat { get; set; }
        public string TradeDate { get; set; }

        /// <summary>
        /// 日期显示
        /// </summary>
        public string TradeDateDisplay { get; set; }
        public string OfrMerge { get; set; }
        public string BidMerge { get; set; }
        public bool IsSort { get; set; }
        public bool IsOfr { get; set; }
        public bool IsBid { get; set; }
        public string ExcId { get; set; }

        /// <summary>
        /// 是否显示历史
        /// </summary>
        public bool IsShowHist { get; set; }

        /// <summary>
        /// 是否期限排序
        /// </summary>
        public bool IsMaturityOrder { get; set; }

        //*************v2.6 增加债券信息***********
        /// <summary>
        /// 债券类型
        /// </summary>
        public List<string> BondTypes { get; set; }

        /// <summary>
        /// 交易场所
        /// </summary>
        public List<string> TradingPlaces { get; set; }

        /// <summary>
        /// 订单类型 意向单、成交单
        /// </summary>
        public List<string> OrderTypes { get; set; } = new List<string>();


        /// <summary>
        /// broker列表
        /// </summary>
        public List<string> brokerNames { get; set; } = new List<string>();

        /// <summary>
        /// 交易场所选择项字符串
        /// </summary>
        public static readonly string TradingPlacesStr = "银行间、上交所、深交所";

        /// <summary>
        /// 债券类型选择项字符串
        /// </summary>
        public static readonly string BondTypesStr = "国债、国开、进出口、农发、地方债、短融/超短融、中票、企业/公司债、次级债、PPN、其他";

        /// <summary>
        /// 成交明细 意向筛选框选择项字符串
        /// </summary>
        public static readonly Dictionary<string, string> OrderTypeDic = new Dictionary<string, string> { { "1", "二级" }, { "0", "意向" } };//{ { "1", "二级" }, { "2", "意向" } };

        const string 国债Str = "BGB、EGB、SGB";

        static public List<string> 国债List = new List<string>(国债Str.Split(new char[] { '、' }));
        /// <summary>
        /// 企业债：CCB，COB，RAB以及当CEB和LEB且Ent_Cor=ENT
        /// 公司债：SCV，CVB以及当CEB和LEB且Ent_Cor=COP
        /// 暂时不考虑Ent_Cor
        /// </summary>
        const string 企业公司债Str = "CCB、COB、RAB、CEB、LEB、SCV、CVB";
        static public List<string> 企业公司债List = new List<string>(企业公司债Str.Split(new char[] { '、' }));
        const string 短融超短融Str = "CCP、LCP、SSB、LSP、CSP";
        static public List<string> 短融超短融List = new List<string>(短融超短融Str.Split(new char[] { '、' }));
        const string 中票Str = "CMN、LMN";
        static public List<string> 中票List = new List<string>(中票Str.Split(new char[] { '、' }));
        const string 次级债Str = "CSB、NCB、PSB、SES、SPS";
        static public List<string> 次级债List = new List<string>(次级债Str.Split(new char[] { '、' }));
        const string 国开Str = "PBB、PSB";
        static public List<string> 国开List = new List<string>(国开Str.Split(new char[] { '、' }));
        const string 进出口Str = "PBB、PSB";
        static public List<string> 进出口List = new List<string>(进出口Str.Split(new char[] { '、' }));
        const string 农发Str = "PBB、PSB";
        static public List<string> 农发List = new List<string>(农发Str.Split(new char[] { '、' }));
        const string 地方债Str = "TLB、LLB";
        static public List<string> 地方债List = new List<string>(地方债Str.Split(new char[] { '、' }));
        static public List<string> 全部债券类型List = 国债List.Union(企业公司债List).Union(短融超短融List).Union(中票List).Union(次级债List).Union(国开List).Union(进出口List).Union(农发List).Union(地方债List).ToList();
    }
    public class Ofr
    {
        public string Insta { get; set; }
        public string BondCode { get; set; }
        public decimal Price { get; set; }
        public string Dealdate { get; set; }
        public string Settlementdate { get; set; }
        public string bankAgentName { get; set; }
    }

    public class Bid
    {
        public string Instb { get; set; }
        public string BondCode { get; set; }
        public decimal Price { get; set; }
        public string Dealdate { get; set; }
        public string Settlementdate { get; set; }
        public string bankAgentName { get; set; }
    }
}
