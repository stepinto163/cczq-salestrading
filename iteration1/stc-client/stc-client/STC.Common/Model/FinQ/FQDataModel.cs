using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;

namespace STC.Common.Model.FinQ
{
    /// <summary>
    /// 用户点击盘口，推送价格：
    /// </summary>
    public class FQDataModel
    {
        /// <summary>
        /// 示例: "Z0003002024GOVBGB18"
        /// </summary>
        [JsonProperty("bondkey")]
        public string bondkey { get; set; }

        /// <summary>
        /// 示例: "CIB"
        /// </summary>
        [JsonProperty("listedmarket")]
        public string listedmarket { get; set; }

        /// <summary>
        /// 示例: "zhongxin.chen"
        /// </summary>
        [JsonProperty("fromQB")]
        public string fromQB { get; set; }

        /// <summary>
        /// 示例: "c5cc87c1b75f49329c40b68d526ce861"
        /// </summary>
        [JsonProperty("fromQBAccountID")]
        public string fromQBAccountID { get; set; }

        /// <summary>
        /// 示例: true
        /// </summary>
        [JsonProperty("openChat")]
        public bool openChat { get; set; }

        /// <summary>
        /// 示例: "6.05Y   240018.IB   24附息国债18   1.71/1.70   5000+2000(需做市)/3000   国际   昨日中债 1.7000   10:08:32"
        /// </summary>
        [JsonProperty("content")]
        public string content { get; set; }

        /// <summary>
        /// 示例: 点击单家最优0; 数据类型，0或null：点击单家最优；1：点击综合最优；2：点击成交价
        /// </summary>
        [JsonProperty("dataType")]
        public int dataType { get; set; }

        [JsonProperty("bondinfo")]
        public BondInfo bondinfo { get; set; }

        [JsonProperty("data")]
        public Data data { get; set; }

        public string _uuid { get; set; }
    }

    /// <summary>
    /// change_bond 接口：客户端主动推送
    /// </summary>
    public class FQUpdateDataModel
    {
        /// <summary>
        /// 示例: "Z0003002024GOVBGB18"
        /// </summary>
        [JsonProperty("bondkey")]
        public string bondkey { get; set; }

        /// <summary>
        /// 示例: "CIB"
        /// </summary>
        [JsonProperty("listedmarket")]
        public string listedmarket { get; set; }

        /// <summary>
        /// 示例: "zhongxin.chen"
        /// </summary>
        [JsonProperty("fromQB")]
        public string fromQB { get; set; }

        /// <summary>
        /// 示例: "c5cc87c1b75f49329c40b68d526ce861"
        /// </summary>
        [JsonProperty("fromQBAccountID")]
        public string fromQBAccountID { get; set; }

        /// <summary>
        /// 示例: true
        /// </summary>
        [JsonProperty("openChat")]
        public bool openChat { get; set; }

        /// <summary>
        /// 示例: 多行字符串，包含多条债券信息
        /// </summary>
        [JsonProperty("content")]
        public string content { get; set; }

        /// <summary>
        /// 示例: 1
        /// </summary>
        [JsonProperty("dataType")]
        public int? dataType { get; set; }

        [JsonProperty("bondinfo")]
        public BondInfo bondinfo { get; set; }

        /// <summary>
        /// 示例: 多个报价对象
        /// </summary>
        [JsonProperty("data")]
        public List<Data> data { get; set; }


        public string _uuid { get; set; }
    }

    public class BondInfo
    {
        /// <summary>
        /// 债项评级
        /// </summary>
        /// <summary>示例: ""</summary>
        [JsonProperty("bond_rating_cur_code")]
        public string bond_rating_cur_code { get; set; }

        /// <summary>
        /// 债券子类型: "",金融机构债,资产支持证券,NCD,CRM,PPN,国债,其他,公司债,中票,地方债,可交换债,央票,可转债,铁道债,金融债,短融,企业
        /// </summary>
        /// <summary>示例: "国债"</summary>
        [JsonProperty("bond_typ2")]
        public string bond_typ2 { get; set; }

        /// <summary>
        /// 发行量
        /// </summary>
        /// <summary>示例: "27916000"</summary>
        [JsonProperty("iss_sz")]
        public string iss_sz { get; set; }

        /// <summary>
        /// 主体评级
        /// </summary>
        /// <summary>示例: ""</summary>
        [JsonProperty("issuer_rating")]
        public string issuer_rating { get; set; }

        /// <summary>
        /// 行权日
        /// </summary>
        /// <summary>示例: ""</summary>
        [JsonProperty("nxt_strike_dt")]
        public string nxt_strike_dt { get; set; }

        /// <summary>
        /// 存续量---------------债券余额
        /// </summary>
        /// <summary>示例: "27916000.0000"</summary>
        [JsonProperty("otstd_amnt")]
        public string otstd_amnt { get; set; }

        /// <summary>
        /// 债券简称拼音, 无法处理多音字, 例如长沙(zhang sha)
        /// </summary>
        /// <summary>示例: "24FXGZ18"</summary>
        [JsonProperty("pinyin_short")]
        public string pinyin_short { get; set; }

        /// <summary>
        /// 剩余期限
        /// </summary>
        /// <summary>示例: "6.05Y"</summary>
        [JsonProperty("residual_maturity")]
        public string residual_maturity { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        /// <summary>示例: "240018.IB"</summary>
        [JsonProperty("sec_code_par")]
        public string sec_code_par { get; set; }

        /// <summary>
        /// 债券简称
        /// </summary>
        /// <summary>示例: "24附息国债18"</summary>
        [JsonProperty("sec_shrt_nm")]
        public string sec_shrt_nm { get; set; }

        /// <summary>
        /// 债券类型: 1=信用债 BCO; 2=利率债 BNC; 3=NCD
        /// </summary>
        /// <summary>示例: "BNC"</summary>
        [JsonProperty("securityType")]
        public string securityType { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// 中介ID，1 = TPSC 国利; 2 = CNEX 国际; 3 = CCTB 中诚; 4 = PTCN 平安; 5 = CTTJ 信唐; 6 = UEDA 上田
        /// </summary>
        /// <summary>示例: "2"</summary>
        [JsonProperty("comp_id")]
        public string comp_id { get; set; }

        /// <summary>示例: ""</summary>
        [JsonProperty("bestbroker")]
        public string bestbroker { get; set; }

        /// <summary>示例: ""</summary>
        [JsonProperty("broker_qq")]
        public string broker_qq { get; set; }

        /// <summary>
        /// 债券代码 示例: "240018.IB
        /// </summary>
        [JsonProperty("code")]
        public string code { get; set; }

        /// <summary>
        /// 债券类型，BNC=利率债 BCO=信用债 NCD
        /// </summary>
        [JsonProperty("bond_type")]
        public string bond_type { get; set; }

        /// <summary>
        /// 债券名称 示例: "24附息国债18"
        /// </summary> 
        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// 剩余期限
        /// </summary>
        /// <summary>示例: "6.05Y"</summary>
        [JsonProperty("residual_maturity")]
        public string residual_maturity { get; set; }

        /// <summary>
        /// 债项评级
        /// </summary>
        /// <summary>示例: ""</summary>
        [JsonProperty("bond_rating")]
        public string bond_rating { get; set; }

        /// <summary>
        /// 主体评级
        /// </summary>
        /// <summary>示例: ""</summary>
        [JsonProperty("issuer_rating")]
        public string issuer_rating { get; set; }

        /// <summary>
        /// 方向，即用户点击的是 bid 还是 ofr，取值: bid, ofr
        /// </summary>
        /// <summary>示例: "ofr"</summary>
        [JsonProperty("side")]
        public string side { get; set; }

        /// <summary>
        /// 价格，即用户点击的价格，可为实际价格，空，bid，ofr
        /// </summary>
        /// <summary>示例: "1.70"</summary>
        [JsonProperty("price")]
        public string price { get; set; }

        /// <summary>
        /// 价格类型，有具体价格时必传，1=净价；2=全价；3=收益率；4=利差
        /// </summary>
        /// <summary>示例: "3"</summary>
        [JsonProperty("price_type")]
        public string price_type { get; set; }

        /// <summary>
        /// 收益率类型，当price_type=3时必传， * EXERCISE=行权，MATURITY=到期
        /// </summary>
        /// <summary>示例: "MATURITY"</summary>
        [JsonProperty("yield_type")]
        public string yield_type { get; set; }

        /// <summary>示例: [["3000","2"]]</summary>
        [JsonProperty("vol_delivery")]
        public List<List<string>> vol_delivery { get; set; }

        /// <summary>示例: [["3000","2","08.26+1"]]</summary>
        [JsonProperty("vol_delivery_qb")]
        public List<List<string>> vol_delivery_qb { get; set; }

        /// <summary>
        /// 另一方向价格，格式同 price
        /// </summary>
        /// <summary>示例: "1.71"</summary>
        [JsonProperty("opp_price")]
        public string opp_price { get; set; }

        /// <summary>
        /// 另一方向价格类型,有具体价格时必传，1=净价；2=全价；3=收益率；4=利差
        /// </summary>
        /// <summary>示例: "3"</summary>
        [JsonProperty("opp_price_type")]
        public string opp_price_type { get; set; }

        /// <summary>
        /// 中债估值
        /// </summary>
        [JsonProperty("val_yield")]
        public string val_yield { get; set; }

        /// <summary>
        /// 另一方向收益率类型，当price_type=3时必传， * EXERCISE=行权，MATURITY=到期
        /// </summary>
        /// <summary>示例: "MATURITY"</summary> 
        [JsonProperty("opp_yield_type")]
        public string opp_yield_type { get; set; }

        /// <summary>示例: [["5000","2"],["2000","2"]]</summary>
        [JsonProperty("opp_vol_delivery")]
        public List<List<string>> opp_vol_delivery { get; set; }

        /// <summary>
        /// 扩展字段，调用方自定义字段，如ID或JSON字符串，会填充到后续意向管理数据推送的 customInfo 字段中
        /// </summary>
        /// <summary>示例: "c5cc87c1b75f49329c40b68d526ce861_350021675360.600040"
        [JsonProperty("ext")]
        public string ext { get; set; }
    }
}
