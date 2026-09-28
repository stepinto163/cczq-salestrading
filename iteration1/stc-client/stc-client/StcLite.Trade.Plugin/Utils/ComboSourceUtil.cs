using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StcLite.Trade.Plugin.Utils
{
    public class ComboSourceUtil : IComboSourceUtil
    {
        private static readonly Dictionary<string, List<KeyValuePair<string, string>>> BindSourceCache = new Dictionary<string, List<KeyValuePair<string, string>>>();
        private static IComboSourceUtil instance = new ComboSourceUtil();

        public static IComboSourceUtil Instance
        {
            get
            {
                return instance;
            }
        }

        public void ClearComboSource()
        {
            BindSourceCache.Clear();
        }

        /// <summary>
        /// 找出枚举对应值
        /// </summary>
        /// <param name="key">枚举键值</param>
        /// <param name="param">枚举类型</param>
        /// <returns></returns>
        public string FindEnum(string key, string param)
        {
            List<KeyValuePair<string, string>> comboList = GetComboSource(param);
            if (comboList != null && comboList.Count > 0)
            {
                foreach (KeyValuePair<string, string> pair in comboList)
                {
                    if (pair.Key == key)
                    {
                        return pair.Value;
                    }
                }
            }
            //if (string.IsNullOrEmpty(key) == false && param == "settletype")
            //{
            //    return "远期";
            //}

            if (!string.IsNullOrEmpty(key) && param == "settletype_broker")
                return "远期";

            return key;
        }

        public List<KeyValuePair<string, string>> GetComboSource(string typeName)
        {
            return CreateBindSource(typeName);
        }

        private List<KeyValuePair<string, string>> CreateBindSource(string typeName)
        {
            List<KeyValuePair<string, string>> source = null;
            if (BindSourceCache.TryGetValue(typeName, out source))
            {
                return source;
            }
            else
            {
                source = InitByEnum(typeName);
                if (source == null)
                {
                    return null;
                }
                else
                {
                    return BindSourceCache[typeName] = source;
                }
            }
        }

        private static List<KeyValuePair<string, string>> InitByEnum(string typeName)
        {
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(typeName) == false)
            {
                switch (typeName.ToLower())
                {
                    case "venues_cn":
                        arg.Add(new KeyValuePair<string, string>("1", "国利"));
                        arg.Add(new KeyValuePair<string, string>("2", "国际"));
                        arg.Add(new KeyValuePair<string, string>("3", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("4", "平安"));
                        arg.Add(new KeyValuePair<string, string>("5", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("6", "上田"));
                        return arg;
                    case "venues_en":
                        arg.Add(new KeyValuePair<string, string>("1", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("2", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("3", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("4", "PTCN"));
                        arg.Add(new KeyValuePair<string, string>("5", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("6", "UEDA"));
                        return arg;
                    case "venues_en_tonumber":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "1"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "2"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "3"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "4"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "5"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "6"));
                        return arg;
                    case "side":
                        arg.Add(new KeyValuePair<string, string>("1", "Bid"));
                        arg.Add(new KeyValuePair<string, string>("2", "Ofr"));
                        arg.Add(new KeyValuePair<string, string>("X", "GVN"));
                        arg.Add(new KeyValuePair<string, string>("Y", "TKN"));
                        arg.Add(new KeyValuePair<string, string>("Z", "TRD"));

                        arg.Add(new KeyValuePair<string, string>("9", "多换"));
                        arg.Add(new KeyValuePair<string, string>("10", "空换"));
                        arg.Add(new KeyValuePair<string, string>("3", "双开"));
                        arg.Add(new KeyValuePair<string, string>("4", "双平"));
                        arg.Add(new KeyValuePair<string, string>("5", "多开"));
                        arg.Add(new KeyValuePair<string, string>("6", "空开"));
                        arg.Add(new KeyValuePair<string, string>("7", "多平"));
                        arg.Add(new KeyValuePair<string, string>("8", "空平"));

                        return arg;
                    case "settletype_myorder":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1(今+1)"));
                        arg.Add(new KeyValuePair<string, string>("3", "T+1(明+0)"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        arg.Add(new KeyValuePair<string, string>("2,3", "T+1(不限)"));
                        return arg;
                    case "settletype":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1"));
                        arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        return arg;
                    case "settletype_tooltip":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1"));
                        arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        return arg;
                    case "settletype_broker":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1"));
                        arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        return arg;
                    case "settle_to_en":
                        arg.Add(new KeyValuePair<string, string>("1", "+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "+1"));
                        arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        return arg;
                    case "venues":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "人工"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
                        arg.Add(new KeyValuePair<string, string>("CTP", "中金"));
                        return arg;
                    case "venues_exe":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "TradeBook"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "TradeBook"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "TradeBook"));
                        arg.Add(new KeyValuePair<string, string>("中金", "CTP"));
                        return arg;
                    case "ordstatus":
                        arg.Add(new KeyValuePair<string, string>("N", "新增"));
                        arg.Add(new KeyValuePair<string, string>("0", "正常"));
                        arg.Add(new KeyValuePair<string, string>("4", "已撤销"));
                        arg.Add(new KeyValuePair<string, string>("8", "已拒绝"));
                        arg.Add(new KeyValuePair<string, string>("1", "部分成交"));
                        arg.Add(new KeyValuePair<string, string>("2", "完全成交"));
                        arg.Add(new KeyValuePair<string, string>("S", "报价中"));
                        arg.Add(new KeyValuePair<string, string>("6", "撤销中"));
                        arg.Add(new KeyValuePair<string, string>("C", "异常"));
                        return arg;
                    case "firmflag":
                        arg.Add(new KeyValuePair<string, string>("0", "nonfirm"));
                        arg.Add(new KeyValuePair<string, string>("1", "firm"));
                        return arg;
                    case "bondtype":
                        arg.Add(new KeyValuePair<string, string>("100001", "国债"));
                        arg.Add(new KeyValuePair<string, string>("100003", "政策性金融债"));
                        arg.Add(new KeyValuePair<string, string>("100050", "定向工具"));
                        arg.Add(new KeyValuePair<string, string>("100002", "央行票据"));
                        arg.Add(new KeyValuePair<string, string>("100004", "企业债"));
                        arg.Add(new KeyValuePair<string, string>("100005", "次级债"));
                        arg.Add(new KeyValuePair<string, string>("100009", "混合资本债"));
                        arg.Add(new KeyValuePair<string, string>("100023", "证券公司债"));
                        arg.Add(new KeyValuePair<string, string>("100027", "政府支持机构债券"));
                        arg.Add(new KeyValuePair<string, string>("100008", "国际开发机构债"));
                        arg.Add(new KeyValuePair<string, string>("100006", "短期融资券"));
                        arg.Add(new KeyValuePair<string, string>("100011", "地方政府债"));
                        arg.Add(new KeyValuePair<string, string>("100054", "非新发关键期限国债"));
                        arg.Add(new KeyValuePair<string, string>("100010", "中期票据"));
                        arg.Add(new KeyValuePair<string, string>("100052", "政府支持债券"));
                        arg.Add(new KeyValuePair<string, string>("100007", "商业银行普通金融债"));
                        arg.Add(new KeyValuePair<string, string>("100022", "财务公司债2"));
                        arg.Add(new KeyValuePair<string, string>("100028", "汽车金融公司金融债"));
                        arg.Add(new KeyValuePair<string, string>("100026", "金融租赁公司金融债"));
                        arg.Add(new KeyValuePair<string, string>("100056", "非政府信用债券"));
                        arg.Add(new KeyValuePair<string, string>("100057", "SCP债券"));
                        arg.Add(new KeyValuePair<string, string>("100082", "项目收益票据"));
                        arg.Add(new KeyValuePair<string, string>("100029", "超短期融资券"));
                        arg.Add(new KeyValuePair<string, string>("100024", "证券公司短期融资券"));
                        arg.Add(new KeyValuePair<string, string>("100053", "新发关键期限国债"));
                        arg.Add(new KeyValuePair<string, string>("100061", "外国主权政府人民币债券"));
                        arg.Add(new KeyValuePair<string, string>("100062", "外国地方政府人民币债券"));
                        arg.Add(new KeyValuePair<string, string>("100041", "同业存单"));
                        arg.Add(new KeyValuePair<string, string>("100055", "政府类开发金融机构债券"));
                        arg.Add(new KeyValuePair<string, string>("100073", "绿色债务融资工具"));

                        return arg;
                    case "crossvenues":
                        arg.Add(new KeyValuePair<string, string>("1", "所有"));
                        arg.Add(new KeyValuePair<string, string>("2", "内部"));
                        arg.Add(new KeyValuePair<string, string>("3", "内部+XBOND"));
                        return arg;
                    case "wholeflag":
                        arg.Add(new KeyValuePair<string, string>("1", "整量"));
                        arg.Add(new KeyValuePair<string, string>("0", string.Empty));
                        return arg;
                    case "synchflag":
                        arg.Add(new KeyValuePair<string, string>("Y","已发送"));
                        arg.Add(new KeyValuePair<string, string>("S","待处理"));
                        arg.Add(new KeyValuePair<string, string>("C","待对方处理"));
                        return arg;
                    case "level":
                        arg.Add(new KeyValuePair<string, string>("C", "紧急"));
                        arg.Add(new KeyValuePair<string, string>("A", "重要"));
                        arg.Add(new KeyValuePair<string, string>("N", "一般"));
                        return arg;
                    case "risktype":
                        arg.Add(new KeyValuePair<string, string>("1", "警示"));
                        arg.Add(new KeyValuePair<string, string>("2", "禁止"));
                        arg.Add(new KeyValuePair<string, string>("3", "2次警示"));
                        return arg;
                    case "contenttype":
                        arg.Add(new KeyValuePair<string, string>("ER", "成交回报"));
                        arg.Add(new KeyValuePair<string, string>("PM", "价格匹配"));
                        arg.Add(new KeyValuePair<string, string>("ST", "系统消息"));
                        arg.Add(new KeyValuePair<string, string>("OD", "订单"));
                        arg.Add(new KeyValuePair<string, string>("IT", "点价待确认"));
                        return arg;
                    case "timeinforce":
                        arg.Add(new KeyValuePair<string, string>("0", "限价单"));
                        arg.Add(new KeyValuePair<string, string>("3", "点价单"));
                        return arg;

                    case "favoriteicon":
                        arg.Add(new KeyValuePair<string, string>("0", "/StcLite.Trade.Plugin;component/Images/stc_icon_favorite_normal.png"));
                        arg.Add(new KeyValuePair<string, string>("1", "/StcLite.Trade.Plugin;component/Images/stc_icon_favorite_seleted.png"));
                        return arg;

                    case "syncflag":
                        arg.Add(new KeyValuePair<string, string>("20", "发送中"));
                        arg.Add(new KeyValuePair<string, string>("21", "待发送"));
                        arg.Add(new KeyValuePair<string, string>("22", "已发送"));
                        arg.Add(new KeyValuePair<string, string>("23", "发送失败"));
                        return arg;
                    case "exectype":
                        arg.Add(new KeyValuePair<string, string>("21", "更新中"));
                        arg.Add(new KeyValuePair<string, string>("22", "已完成"));
                        arg.Add(new KeyValuePair<string, string>("23", "异常"));
                        arg.Add(new KeyValuePair<string, string>("24", "已删除"));
                        arg.Add(new KeyValuePair<string, string>("25", "更新中"));
                        return arg;
                    case "terminal":
                        arg.Add(new KeyValuePair<string, string>("0", "线上执行"));
                        arg.Add(new KeyValuePair<string, string>("1", "渠道执行"));
                        arg.Add(new KeyValuePair<string, string>("2", "线下补录"));
                        return arg;
                    case "dealtype":
                        arg.Add(new KeyValuePair<string, string>("1","内部划转"));
                        arg.Add(new KeyValuePair<string, string>("2", "成交"));
                        return arg;
                    case "trademethod":
                        arg.Add(new KeyValuePair<string, string>("1", "发对话"));
                        arg.Add(new KeyValuePair<string, string>("4", "发请求"));
                        arg.Add(new KeyValuePair<string, string>("30", "收对话"));
                        arg.Add(new KeyValuePair<string, string>("50", "收请求"));
                        arg.Add(new KeyValuePair<string, string>("60", "待定"));
                        return arg;
                    case "trademethod1":
                        arg.Add(new KeyValuePair<string, string>("发对话", "1"));
                        arg.Add(new KeyValuePair<string, string>("发请求", "4"));
                        arg.Add(new KeyValuePair<string, string>("收对话", "30"));
                        arg.Add(new KeyValuePair<string, string>("收请求", "50"));
                        arg.Add(new KeyValuePair<string, string>("待定", "60"));
                        return arg;
                    case "ispoint":
                        arg.Add(new KeyValuePair<string, string>("0", "需求"));
                        arg.Add(new KeyValuePair<string, string>("1", "意向"));
                        return arg;
                    case "venuesorder":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
                        arg.Add(new KeyValuePair<string, string>("SOR-TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("SOR-PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("INNER-TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("INNER-PTCN", "平安"));
                        return arg;
                    case "tovenues":
                        arg.Add(new KeyValuePair<string, string>("国利","TPSC"));
                        arg.Add(new KeyValuePair<string, string>("信唐","CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("国际","CNEX"));
                        arg.Add(new KeyValuePair<string, string>("中诚","CCTB"));
                        arg.Add(new KeyValuePair<string, string>("上田", "平安"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
                        arg.Add(new KeyValuePair<string, string>("1", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("2", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("3", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("4", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("5", "PTCN"));
                        arg.Add(new KeyValuePair<string, string>("TPSC", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "PTCN"));
                        arg.Add(new KeyValuePair<string, string>("SOR-TPSC", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CTTJ", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CNEX", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("SOR-CCTB", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("SOR-PTCN", "PTCN"));
                        arg.Add(new KeyValuePair<string, string>("INNER-TPSC", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CTTJ", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CNEX", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("INNER-CCTB", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("INNER-PTCN", "PTCN"));
                        return arg;
                }

            }
            return null;
        }
    }  
}
