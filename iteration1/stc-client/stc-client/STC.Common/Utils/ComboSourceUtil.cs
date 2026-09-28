using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 枚举公共类
    /// </summary>
    public class ComboSourceUtil : IComboSourceUtil
    {
        /// <summary>
        /// 枚举字典
        /// </summary>
        private static readonly ConcurrentDictionary<string, List<KeyValuePair<string, string>>> BindSourceCache = new ConcurrentDictionary<string, List<KeyValuePair<string, string>>>();
        private Dictionary<string, Dictionary<string, string>> DictEnum = new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// ComboSourceUtil实例
        /// </summary>
        private static IComboSourceUtil instance = new ComboSourceUtil();

        public static IComboSourceUtil Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public void ClearComboSource()
        {
            BindSourceCache.Clear();
        }

        public void InitComboSourceUtil()
        {
            InitAllEnum();
        }

        /// <summary>
        /// 找出枚举对应值
        /// </summary>
        /// <param name="key">枚举键值</param>
        /// <param name="param">枚举类型 Key</param>
        /// <returns></returns>
        public string FindEnum(string key, string param)
        {
            ////List<KeyValuePair<string, string>> comboList = GetComboSource(param);
            ////if (comboList != null && comboList.Count > 0)
            ////{
            ////    foreach (KeyValuePair<string, string> pair in comboList)
            ////    {
            ////        if (pair.Key == key)
            ////        {
            ////            return pair.Value;
            ////        }
            ////    }
            ////}
            ////if (!string.IsNullOrEmpty(key) && param == "settletype_broker")
            ////    return "远期";

            ////return key;
            ///
            if (!string.IsNullOrEmpty(param) && !string.IsNullOrEmpty(key))
            {
                param = param.ToLower();

                if (DictEnum.ContainsKey(param))
                {
                    return DictEnum[param].ContainsKey(key) ? DictEnum[param][key] : key;
                }
                else
                {
                    if (!string.IsNullOrEmpty(key) && param == "settletype_broker")
                        return "远期";

                    return key;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据typeName获得枚举列表
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetComboSource(string typeName)
        {
            return CreateBindSource(typeName);
        }

        /// <summary>
        /// 创建枚举数据源
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 初始化枚举列表
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> InitByEnum(string typeName)
        {
            if (string.IsNullOrEmpty(typeName) == false)
            {
                if (BindSourceCache.ContainsKey(typeName.ToLower()))
                    return BindSourceCache[typeName.ToLower()];
            }

            return null;
        }

        private void InitAllEnum()
        {
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            //switch (typeName.ToLower())
            //{
            //    case "venues_cn_tocn":
            arg.Add(new KeyValuePair<string, string>("1", "国利"));
            arg.Add(new KeyValuePair<string, string>("2", "国际"));
            arg.Add(new KeyValuePair<string, string>("3", "中诚"));
            arg.Add(new KeyValuePair<string, string>("4", "平安"));
            arg.Add(new KeyValuePair<string, string>("5", "信唐"));
            arg.Add(new KeyValuePair<string, string>("6", "上田"));
            BindSourceCache.TryAdd("venues_cn_tocn", arg);
            //return arg;
            //case "venues_cn_tonumber":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("国利", "1"));
            arg.Add(new KeyValuePair<string, string>("国际", "2"));
            arg.Add(new KeyValuePair<string, string>("中诚", "3"));
            arg.Add(new KeyValuePair<string, string>("平安", "4"));
            arg.Add(new KeyValuePair<string, string>("信唐", "5"));
            arg.Add(new KeyValuePair<string, string>("上田", "6"));
            BindSourceCache.TryAdd("venues_cn_tonumber", arg);
            //return arg;
            //case "venues_en":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "TPSC"));
            arg.Add(new KeyValuePair<string, string>("2", "CNEX"));
            arg.Add(new KeyValuePair<string, string>("3", "CCTB"));
            arg.Add(new KeyValuePair<string, string>("4", "PTCN"));
            arg.Add(new KeyValuePair<string, string>("5", "CTTJ"));
            arg.Add(new KeyValuePair<string, string>("6", "UEDA"));
            BindSourceCache.TryAdd("venues_en", arg);
            //return arg;
            //case "venues_en_tonumber":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("TPSC", "1"));
            arg.Add(new KeyValuePair<string, string>("CNEX", "2"));
            arg.Add(new KeyValuePair<string, string>("CCTB", "3"));
            arg.Add(new KeyValuePair<string, string>("PTCN", "4"));
            arg.Add(new KeyValuePair<string, string>("CTTJ", "5"));
            arg.Add(new KeyValuePair<string, string>("UEDA", "6"));
            BindSourceCache.TryAdd("venues_en_tonumber", arg);
            //    return arg;
            //case "openclose":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "开仓"));
            arg.Add(new KeyValuePair<string, string>("1", "平仓"));
            arg.Add(new KeyValuePair<string, string>("2", "平今"));
            arg.Add(new KeyValuePair<string, string>("3", "平昨"));
            arg.Add(new KeyValuePair<string, string>("4", "止损"));
            BindSourceCache.TryAdd("openclose", arg);
            //    return arg;
            //case "side":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "Bid"));
            arg.Add(new KeyValuePair<string, string>("2", "Ofr"));
            arg.Add(new KeyValuePair<string, string>("X", "TKN"));
            arg.Add(new KeyValuePair<string, string>("Y", "GVN"));
            arg.Add(new KeyValuePair<string, string>("Z", "TRD"));
            arg.Add(new KeyValuePair<string, string>("Q", "TRD2"));
            arg.Add(new KeyValuePair<string, string>("9", "多换"));
            arg.Add(new KeyValuePair<string, string>("10", "空换"));
            arg.Add(new KeyValuePair<string, string>("3", "双开"));
            arg.Add(new KeyValuePair<string, string>("4", "双平"));
            arg.Add(new KeyValuePair<string, string>("5", "多开"));
            arg.Add(new KeyValuePair<string, string>("6", "空开"));
            arg.Add(new KeyValuePair<string, string>("7", "多平"));
            arg.Add(new KeyValuePair<string, string>("8", "空平"));
            BindSourceCache.TryAdd("side", arg);
            //    return arg;
            //case "settletype_myorder":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("2", "T+1(今+1)"));
            arg.Add(new KeyValuePair<string, string>("3", "T+1(明+0)"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            arg.Add(new KeyValuePair<string, string>("2,3", "T+1(不限)"));
            BindSourceCache.TryAdd("settletype_myorder", arg);
            //    return arg;
            //case "settletype_revert":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("T+0", "1"));
            arg.Add(new KeyValuePair<string, string>("+0", "1"));
            arg.Add(new KeyValuePair<string, string>("T+1", "2"));
            arg.Add(new KeyValuePair<string, string>("+1", "2"));
            arg.Add(new KeyValuePair<string, string>("T+1(今+1)", "2"));
            arg.Add(new KeyValuePair<string, string>("明+0", "3"));
            arg.Add(new KeyValuePair<string, string>("明天+0", "3"));
            arg.Add(new KeyValuePair<string, string>("T+1(明+0)", "3"));
            arg.Add(new KeyValuePair<string, string>("明天+1", "4"));
            arg.Add(new KeyValuePair<string, string>("T+1(不限)", "2,3"));
            BindSourceCache.TryAdd("settletype_revert", arg);
            //    return arg;
            //case "settletype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("2", "T+1"));
            arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            BindSourceCache.TryAdd("settletype", arg);
            //    return arg;
            //case "settletype_tooltip":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("2", "T+1"));
            arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            BindSourceCache.TryAdd("settletype_tooltip", arg);
            //    return arg;
            //case "settletype_broker":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("2", "T+1"));
            arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            BindSourceCache.TryAdd("settletype_broker", arg);
            //    return arg;
            //case "settletype_ptcopy":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("3", "明+0"));
            arg.Add(new KeyValuePair<string, string>("2", "今+1"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            BindSourceCache.TryAdd("settletype_ptcopy", arg);
            //    return arg;
            //case "settle_to_en":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "+0"));
            arg.Add(new KeyValuePair<string, string>("2", "+1"));
            arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
            arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
            BindSourceCache.TryAdd("settle_to_en", arg);
            //    return arg;
            //case "venues_cn_to_en":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("国利", "TPSC"));
            arg.Add(new KeyValuePair<string, string>("信唐", "CTTJ"));
            arg.Add(new KeyValuePair<string, string>("国际", "CNEX"));
            arg.Add(new KeyValuePair<string, string>("中诚", "CCTB"));
            arg.Add(new KeyValuePair<string, string>("平安", "PTCN"));
            arg.Add(new KeyValuePair<string, string>("上田", "UEDA"));
            BindSourceCache.TryAdd("venues_cn_to_en", arg);
            //    return arg;
            //case "venues":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
            arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
            arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
            arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
            arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
            arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
            arg.Add(new KeyValuePair<string, string>("ODM", "XB"));
            arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
            arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
            arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
            arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
            arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
            arg.Add(new KeyValuePair<string, string>("MANUAL", "内部"));//"人工"));
            arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
            arg.Add(new KeyValuePair<string, string>("SSE", "上新债"));
            arg.Add(new KeyValuePair<string, string>("CTP", "期货"));
            arg.Add(new KeyValuePair<string, string>("SZE", "深匹配"));
            arg.Add(new KeyValuePair<string, string>("SZF", "深点击"));
            arg.Add(new KeyValuePair<string, string>("QDM2", "指示"));
            arg.Add(new KeyValuePair<string, string>("BBO", "经纪商"));
            arg.Add(new KeyValuePair<string, string>("NDM", "协商"));
            arg.Add(new KeyValuePair<string, string>("Auction", "匿名拍卖"));
            arg.Add(new KeyValuePair<string, string>("SSF", "上固收"));
            //arg.Add(new KeyValuePair<string, string>("CTP", "中金"));
            BindSourceCache.TryAdd("venues", arg);
            //    return arg;
            //case "venues_cn":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("国利", "TPSC"));
            arg.Add(new KeyValuePair<string, string>("信唐", "CTTJ"));
            arg.Add(new KeyValuePair<string, string>("国际", "CNEX"));
            arg.Add(new KeyValuePair<string, string>("中诚", "CCTB"));
            arg.Add(new KeyValuePair<string, string>("平安", "PTCN"));
            arg.Add(new KeyValuePair<string, string>("上田", "UEDA"));
            arg.Add(new KeyValuePair<string, string>("XB", "ODM"));
            arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
            arg.Add(new KeyValuePair<string, string>("RFQ", "QDM"));
            //arg.Add(new KeyValuePair<string, string>("RFQ", "HTQDM"));
            arg.Add(new KeyValuePair<string, string>("内部", "INNER"));
            arg.Add(new KeyValuePair<string, string>("双边", "QDMESP"));// 做市"));
            //arg.Add(new KeyValuePair<string, string>("内部", "MANUAL"));//"人工"));
            arg.Add(new KeyValuePair<string, string>("量化", "QUANT"));
            arg.Add(new KeyValuePair<string, string>("上新债", "SSE"));
            arg.Add(new KeyValuePair<string, string>("期货", "CTP"));
            arg.Add(new KeyValuePair<string, string>("深匹配", "SZE"));
            arg.Add(new KeyValuePair<string, string>("深点击", "SZF"));
            arg.Add(new KeyValuePair<string, string>("指示", "QDM2"));
            arg.Add(new KeyValuePair<string, string>("经纪商", "BBO"));
            arg.Add(new KeyValuePair<string, string>("协商", "NDM"));
            arg.Add(new KeyValuePair<string, string>("匿名拍卖", "Auction"));
            arg.Add(new KeyValuePair<string, string>("上固收", "SSF"));
            BindSourceCache.TryAdd("venues_cn", arg);
            //    return arg;
            //case "venues_exe":
            arg = new List<KeyValuePair<string, string>>();
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
            arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
            arg.Add(new KeyValuePair<string, string>("MANUAL", "TradeBook"));
            arg.Add(new KeyValuePair<string, string>("QUANT", "TradeBook"));
            arg.Add(new KeyValuePair<string, string>("SSE", "上新债"));
            arg.Add(new KeyValuePair<string, string>("CTP", "期货"));
            arg.Add(new KeyValuePair<string, string>("SZE", "深匹配"));
            arg.Add(new KeyValuePair<string, string>("SZF", "深点击"));
            arg.Add(new KeyValuePair<string, string>("QDM2", "指示"));
            arg.Add(new KeyValuePair<string, string>("BBO", "经纪商"));
            arg.Add(new KeyValuePair<string, string>("NDM", "协商"));
            arg.Add(new KeyValuePair<string, string>("Auction", "匿名拍卖"));
            arg.Add(new KeyValuePair<string, string>("SSF", "上固收"));
            arg.Add(new KeyValuePair<string, string>("中金", "CTP"));
            BindSourceCache.TryAdd("venues_exe", arg);

            arg = new List<KeyValuePair<string, string>>();
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
            arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
            arg.Add(new KeyValuePair<string, string>("MANUAL", "自谈"));
            arg.Add(new KeyValuePair<string, string>("QUANT", "TradeBook"));
            arg.Add(new KeyValuePair<string, string>("SSE", "上新债"));
            arg.Add(new KeyValuePair<string, string>("CTP", "期货"));
            arg.Add(new KeyValuePair<string, string>("SZE", "深匹配"));
            arg.Add(new KeyValuePair<string, string>("SZF", "深点击"));
            arg.Add(new KeyValuePair<string, string>("QDM2", "指示"));
            arg.Add(new KeyValuePair<string, string>("BBO", "经纪商"));
            arg.Add(new KeyValuePair<string, string>("NDM", "协商"));
            arg.Add(new KeyValuePair<string, string>("Auction", "匿名拍卖"));
            arg.Add(new KeyValuePair<string, string>("SSF", "上固收"));
            arg.Add(new KeyValuePair<string, string>("中金", "CTP"));
            BindSourceCache.TryAdd("venues_exept", arg);
            //    return arg;
            //case "venuesxb":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
            arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
            arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
            arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
            arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
            arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
            arg.Add(new KeyValuePair<string, string>("ODM", "XBOND"));
            arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
            arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
            arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
            arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
            arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
            arg.Add(new KeyValuePair<string, string>("MANUAL", "内部"));//"人工"));
            arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
            arg.Add(new KeyValuePair<string, string>("SSE", "上新债"));
            arg.Add(new KeyValuePair<string, string>("CTP", "期货"));
            arg.Add(new KeyValuePair<string, string>("SZE", "深匹配"));
            arg.Add(new KeyValuePair<string, string>("SZF", "深点击"));
            arg.Add(new KeyValuePair<string, string>("QDM2", "指示"));
            arg.Add(new KeyValuePair<string, string>("BBO", "经纪商"));
            arg.Add(new KeyValuePair<string, string>("NDM", "协商"));
            arg.Add(new KeyValuePair<string, string>("Auction", "匿名拍卖"));
            arg.Add(new KeyValuePair<string, string>("SSF", "上固收"));
            BindSourceCache.TryAdd("venuesxb", arg);
            //    return arg;
            //case "ordstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("N", "新增"));
            arg.Add(new KeyValuePair<string, string>("0", "正常"));
            arg.Add(new KeyValuePair<string, string>("4", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("8", "已拒绝"));
            arg.Add(new KeyValuePair<string, string>("1", "部分成交"));
            arg.Add(new KeyValuePair<string, string>("2", "完全成交"));
            arg.Add(new KeyValuePair<string, string>("S", "报价中"));
            arg.Add(new KeyValuePair<string, string>("6", "撤销中"));
            arg.Add(new KeyValuePair<string, string>("C", "异常"));
            BindSourceCache.TryAdd("ordstatus", arg);
            //    return arg;
            //case "firmflag":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "nonfirm"));
            arg.Add(new KeyValuePair<string, string>("1", "firm"));
            BindSourceCache.TryAdd("firmflag", arg);
            //    return arg;
            //case "bondtype":
            arg = new List<KeyValuePair<string, string>>();
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
            BindSourceCache.TryAdd("bondtype", arg);
            //    return arg;
            //case "crossvenues":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "所有"));
            arg.Add(new KeyValuePair<string, string>("2", "内部"));
            arg.Add(new KeyValuePair<string, string>("3", "内部+XBOND"));
            arg.Add(new KeyValuePair<string, string>("4", "仅外部"));
            arg.Add(new KeyValuePair<string, string>("11", "CFFEX"));   //中金所
            arg.Add(new KeyValuePair<string, string>("12", "SHFE"));    //上期所
            arg.Add(new KeyValuePair<string, string>("13", "CZCE"));    //郑商所
            arg.Add(new KeyValuePair<string, string>("14", "DCE"));     //大商所
            arg.Add(new KeyValuePair<string, string>("15", "INE"));     //上期能源
            BindSourceCache.TryAdd("crossvenues", arg);
            //    return arg;
            //case "wholeflag":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "整量"));
            arg.Add(new KeyValuePair<string, string>("0", string.Empty));
            BindSourceCache.TryAdd("wholeflag", arg);
            //    return arg;
            //case "synchflag":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("Y", "已发送"));
            arg.Add(new KeyValuePair<string, string>("S", "待处理"));
            arg.Add(new KeyValuePair<string, string>("C", "待对方处理"));
            BindSourceCache.TryAdd("synchflag", arg);
            //    return arg;
            //case "level":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("C", "紧急"));
            arg.Add(new KeyValuePair<string, string>("A", "重要"));
            arg.Add(new KeyValuePair<string, string>("N", "一般"));
            BindSourceCache.TryAdd("level", arg);
            //    return arg;
            //case "risktype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "警示"));
            arg.Add(new KeyValuePair<string, string>("2", "禁止"));
            arg.Add(new KeyValuePair<string, string>("3", "2次警示"));
            BindSourceCache.TryAdd("risktype", arg);
            //    return arg;
            //case "contenttype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("ER", "成交回报"));
            arg.Add(new KeyValuePair<string, string>("PM", "价格匹配"));
            arg.Add(new KeyValuePair<string, string>("ST", "系统消息"));
            arg.Add(new KeyValuePair<string, string>("OD", "订单"));
            arg.Add(new KeyValuePair<string, string>("IT", "点价待确认"));
            arg.Add(new KeyValuePair<string, string>("EV", "信号"));
            arg.Add(new KeyValuePair<string, string>("Ans", "双边断报"));
            arg.Add(new KeyValuePair<string, string>("TableSvr_FOCUS_TAG", "订阅消息"));
            arg.Add(new KeyValuePair<string, string>("TableSvr_FOCUS_TAG_RW", "订阅消息"));
            BindSourceCache.TryAdd("contenttype", arg);
            //    return arg;
            //case "timeinforce":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "限价单"));
            arg.Add(new KeyValuePair<string, string>("3", "点价单"));
            BindSourceCache.TryAdd("timeinforce", arg);
            //    return arg;
            //case "favoriteicon":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "/STC.XBond.Plugin;component/Images/stc_icon_favorite_normal.png"));
            arg.Add(new KeyValuePair<string, string>("1", "/STC.XBond.Plugin;component/Images/stc_icon_favorite_seleted.png"));
            BindSourceCache.TryAdd("favoriteicon", arg);
            //    return arg;
            //case "syncflag":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("20", "发送中"));
            arg.Add(new KeyValuePair<string, string>("21", "待发送"));
            arg.Add(new KeyValuePair<string, string>("22", "已发送"));
            arg.Add(new KeyValuePair<string, string>("23", "发送失败"));
            BindSourceCache.TryAdd("syncflag", arg);
            //    return arg;
            //case "exectype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("21", "更新中"));
            arg.Add(new KeyValuePair<string, string>("22", "已完成"));
            arg.Add(new KeyValuePair<string, string>("23", "异常"));
            arg.Add(new KeyValuePair<string, string>("24", "已删除"));
            arg.Add(new KeyValuePair<string, string>("25", "更新中"));
            //    return arg;
            //case "terminal":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "线上执行"));
            arg.Add(new KeyValuePair<string, string>("1", "渠道执行"));
            arg.Add(new KeyValuePair<string, string>("2", "线下补录"));
            BindSourceCache.TryAdd("terminal", arg);
            //    return arg;
            //case "dealtype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "内部划转"));
            arg.Add(new KeyValuePair<string, string>("2", "成交"));
            BindSourceCache.TryAdd("dealtype", arg);
            //    return arg;
            //case "trademethod":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "发对话"));
            arg.Add(new KeyValuePair<string, string>("4", "发请求"));
            arg.Add(new KeyValuePair<string, string>("30", "收对话"));
            arg.Add(new KeyValuePair<string, string>("50", "收请求"));
            arg.Add(new KeyValuePair<string, string>("60", "待定"));
            BindSourceCache.TryAdd("trademethod", arg);
            //    return arg;
            //case "trademethod1":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("发对话", "1"));
            arg.Add(new KeyValuePair<string, string>("发请求", "4"));
            arg.Add(new KeyValuePair<string, string>("收对话", "30"));
            arg.Add(new KeyValuePair<string, string>("收请求", "50"));
            arg.Add(new KeyValuePair<string, string>("待定", "60"));
            BindSourceCache.TryAdd("trademethod1", arg);
            //     return arg;
            //case "ispoint":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "需求"));
            arg.Add(new KeyValuePair<string, string>("1", "意向"));
            BindSourceCache.TryAdd("ispoint", arg);
            //    return arg;
            //case "venuesorder":
            arg = new List<KeyValuePair<string, string>>();
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
            BindSourceCache.TryAdd("venuesorder", arg);
            //    return arg;
            //case "tovenues":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("国利", "TPSC"));
            arg.Add(new KeyValuePair<string, string>("信唐", "CTTJ"));
            arg.Add(new KeyValuePair<string, string>("国际", "CNEX"));
            arg.Add(new KeyValuePair<string, string>("中诚", "CCTB"));
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
            BindSourceCache.TryAdd("tovenues", arg);
            //    return arg;
            //case "side2":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "GVN"));
            arg.Add(new KeyValuePair<string, string>("2", "TKN"));
            arg.Add(new KeyValuePair<string, string>("3", "TRD"));
            arg.Add(new KeyValuePair<string, string>("X", "TKN"));
            arg.Add(new KeyValuePair<string, string>("Y", "GVN"));
            arg.Add(new KeyValuePair<string, string>("Z", "TRD"));
            BindSourceCache.TryAdd("side2", arg);
            //    return arg;
            //case "mmstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "危险"));
            arg.Add(new KeyValuePair<string, string>("2", "可达标"));
            arg.Add(new KeyValuePair<string, string>("3", "已达标"));
            arg.Add(new KeyValuePair<string, string>("4", "失败"));
            BindSourceCache.TryAdd("mmstatus", arg);
            //    return arg;
            //case "intentionstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "待发布"));
            arg.Add(new KeyValuePair<string, string>("1", "已发布"));
            arg.Add(new KeyValuePair<string, string>("2", "已完成"));
            arg.Add(new KeyValuePair<string, string>("3", "已REF"));//已删除
            arg.Add(new KeyValuePair<string, string>("4", "已撤销"));
            BindSourceCache.TryAdd("intentionstatus", arg);
            //return arg;
            //case "intentionstatus1":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "待处理"));
            arg.Add(new KeyValuePair<string, string>("1", "已确认"));
            arg.Add(new KeyValuePair<string, string>("2", "正常"));
            arg.Add(new KeyValuePair<string, string>("3", "已删除"));
            arg.Add(new KeyValuePair<string, string>("4", "暂停"));
            arg.Add(new KeyValuePair<string, string>("5", "完成"));
            BindSourceCache.TryAdd("intentionstatus1", arg);
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("80", "待处理"));
            arg.Add(new KeyValuePair<string, string>("81", "处理中"));
            arg.Add(new KeyValuePair<string, string>("82", "撤销中"));
            arg.Add(new KeyValuePair<string, string>("83", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("84", "已完成"));
            BindSourceCache.TryAdd("intentionstatus_buyer", arg);
            //    return arg;
            //case "clearingmethod":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("6", "净额清算"));
            arg.Add(new KeyValuePair<string, string>("13", "全额清算"));
            BindSourceCache.TryAdd("clearingmethod", arg);
            //    return arg;
            //case "rfqvenues":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("QDM", "请求报价"));
            BindSourceCache.TryAdd("rfqvenues", arg);
            //    return arg;
            //case "deliverytype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "券款对付"));
            arg.Add(new KeyValuePair<string, string>("4", "见券付款"));
            arg.Add(new KeyValuePair<string, string>("5", "见款付券"));
            BindSourceCache.TryAdd("deliverytype", arg);
            //    return arg;
            //case "rfqstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("30", "Taker发送请求"));
            arg.Add(new KeyValuePair<string, string>("31", "Maker锁定"));
            arg.Add(new KeyValuePair<string, string>("32", "Maker解锁"));
            arg.Add(new KeyValuePair<string, string>("33", "Maker确认报价"));
            arg.Add(new KeyValuePair<string, string>("34", "Taker确认报价"));
            arg.Add(new KeyValuePair<string, string>("35", "Maker接受报价"));
            arg.Add(new KeyValuePair<string, string>("36", "订单完成"));
            arg.Add(new KeyValuePair<string, string>("37", "Taker拒绝报价"));
            arg.Add(new KeyValuePair<string, string>("38", "Maker拒绝报价"));
            arg.Add(new KeyValuePair<string, string>("39", "请求超时"));
            arg.Add(new KeyValuePair<string, string>("40", "Maker撤回报价"));
            arg.Add(new KeyValuePair<string, string>("42", "回复过期"));
            arg.Add(new KeyValuePair<string, string>("44", "取消报价"));
            arg.Add(new KeyValuePair<string, string>("46", "报价中"));
            arg.Add(new KeyValuePair<string, string>("47", "撤单中"));
            arg.Add(new KeyValuePair<string, string>("48", "拒绝中"));
            arg.Add(new KeyValuePair<string, string>("5", "操作失败"));
            arg.Add(new KeyValuePair<string, string>("45", "对手方与其它机构成交"));
            BindSourceCache.TryAdd("rfqstatus", arg);
            //    return arg;
            //case "rfqnormalstatus":
            arg = new List<KeyValuePair<string, string>>();
            /// <summary>
            ///待回复1 ==>30,31,32,37,42,40,44
            ///已发送2 ==>33,5
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已过期5 ==>39,45
            arg.Add(new KeyValuePair<string, string>("1", "待回复"));
            arg.Add(new KeyValuePair<string, string>("2", "已发送"));
            arg.Add(new KeyValuePair<string, string>("3", "已成交"));
            arg.Add(new KeyValuePair<string, string>("4", "已拒绝"));
            arg.Add(new KeyValuePair<string, string>("5", "已失效"));
            BindSourceCache.TryAdd("rfqnormalstatus", arg);
            //    return arg;
            //case "settletype_rfq":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "T+0"));
            arg.Add(new KeyValuePair<string, string>("2", "T+1"));
            arg.Add(new KeyValuePair<string, string>("3", "T+2"));
            arg.Add(new KeyValuePair<string, string>("4", "T+3"));
            arg.Add(new KeyValuePair<string, string>("5", "T+4"));
            arg.Add(new KeyValuePair<string, string>("9", "T+5"));
            arg.Add(new KeyValuePair<string, string>("10", "T+6"));
            arg.Add(new KeyValuePair<string, string>("11", "T+7"));
            arg.Add(new KeyValuePair<string, string>("12", "T+8"));
            arg.Add(new KeyValuePair<string, string>("13", "T+9"));
            arg.Add(new KeyValuePair<string, string>("14", "T+10"));
            arg.Add(new KeyValuePair<string, string>("15", "T+11"));
            arg.Add(new KeyValuePair<string, string>("16", "T+12"));
            BindSourceCache.TryAdd("settletype_rfq", arg);
            //    return arg;
            //case "download_datatype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "分渠道最优"));
            arg.Add(new KeyValuePair<string, string>("2", "多档深度"));
            arg.Add(new KeyValuePair<string, string>("3", "XBOND深度"));
            arg.Add(new KeyValuePair<string, string>("4", "CFETS双边深度"));
            arg.Add(new KeyValuePair<string, string>("5", "经纪商成交"));
            arg.Add(new KeyValuePair<string, string>("6", "期货高频快照"));
            arg.Add(new KeyValuePair<string, string>("7", "历史K线"));
            arg.Add(new KeyValuePair<string, string>("8", "原始行情"));
            BindSourceCache.TryAdd("download_datatype", arg);
            //    return arg;
            //case "download_datatype_revert":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("分渠道最优", "1"));
            arg.Add(new KeyValuePair<string, string>("多档深度", "2"));
            arg.Add(new KeyValuePair<string, string>("XBOND深度", "3"));
            arg.Add(new KeyValuePair<string, string>("CFETS双边深度", "4"));
            arg.Add(new KeyValuePair<string, string>("经纪商成交", "5"));
            arg.Add(new KeyValuePair<string, string>("期货高频快照", "6"));
            arg.Add(new KeyValuePair<string, string>("历史K线", "7"));
            arg.Add(new KeyValuePair<string, string>("原始行情", "8"));
            BindSourceCache.TryAdd("download_datatype_revert", arg);
            //    return arg;
            //case "downloadfile_datatype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "bbo"));
            arg.Add(new KeyValuePair<string, string>("2", "depth"));
            arg.Add(new KeyValuePair<string, string>("3", "xbond"));
            arg.Add(new KeyValuePair<string, string>("4", "double"));
            arg.Add(new KeyValuePair<string, string>("5", "trade"));
            arg.Add(new KeyValuePair<string, string>("6", "tick"));
            arg.Add(new KeyValuePair<string, string>("7", "kLine"));
            arg.Add(new KeyValuePair<string, string>("8", "price"));
            BindSourceCache.TryAdd("downloadfile_datatype", arg);
            //    return arg;
            //case "enordstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("New", "正常"));
            arg.Add(new KeyValuePair<string, string>("Frozen", "冻结"));
            arg.Add(new KeyValuePair<string, string>("Pending_Newing", "新建中"));
            arg.Add(new KeyValuePair<string, string>("Cancelled", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("Rejectd", "已拒绝"));
            arg.Add(new KeyValuePair<string, string>("Partially_Filled", "部分成交"));
            arg.Add(new KeyValuePair<string, string>("Filled", "完全成交"));
            arg.Add(new KeyValuePair<string, string>("Cancelling", "撤销中"));
            arg.Add(new KeyValuePair<string, string>("Expired", "已过期"));
            BindSourceCache.TryAdd("enordstatus", arg);
            //    return arg;
            //case "ifsubscribe_menu":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("False", "订阅"));
            arg.Add(new KeyValuePair<string, string>("True", "取消订阅"));
            BindSourceCache.TryAdd("ifsubscribe_menu", arg);
            //    return arg;
            //case "dqmsgcode":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("10010", "报价已撤"));
            arg.Add(new KeyValuePair<string, string>("11011", "议价"));
            arg.Add(new KeyValuePair<string, string>("11012", "已拒绝"));
            arg.Add(new KeyValuePair<string, string>("11013", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("11015", "已超时"));
            arg.Add(new KeyValuePair<string, string>("11016", "成交"));
            BindSourceCache.TryAdd("dqmsgcode", arg);
            //    return arg;
            //case "settletype_drp":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "+0"));
            arg.Add(new KeyValuePair<string, string>("2", "+1"));
            arg.Add(new KeyValuePair<string, string>("3", "+1"));
            arg.Add(new KeyValuePair<string, string>("4", "+2"));
            arg.Add(new KeyValuePair<string, string>("2,3", "+1"));
            BindSourceCache.TryAdd("settletype_drp", arg);
            //    return arg;
            //case "actiontype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "议价"));
            arg.Add(new KeyValuePair<string, string>("2", "成交"));
            arg.Add(new KeyValuePair<string, string>("3", "已拒绝"));
            arg.Add(new KeyValuePair<string, string>("4", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("5", "报价已撤"));
            arg.Add(new KeyValuePair<string, string>("6", "已超时"));
            arg.Add(new KeyValuePair<string, string>("7", "报价"));
            BindSourceCache.TryAdd("actiontype", arg);
            //    return arg;
            //case "reftype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "手动撤销"));
            arg.Add(new KeyValuePair<string, string>("2", "成交撤销"));
            arg.Add(new KeyValuePair<string, string>("3", "系统撤销"));
            BindSourceCache.TryAdd("reftype", arg);
            //    return arg;
            //case "relatestatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "未关联"));
            arg.Add(new KeyValuePair<string, string>("1", "已关联"));
            BindSourceCache.TryAdd("relatestatus", arg);
            //    return arg;
            //case "sendstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "发失败"));
            arg.Add(new KeyValuePair<string, string>("1", "已下发"));
            arg.Add(new KeyValuePair<string, string>("2", "未下发"));
            BindSourceCache.TryAdd("sendstatus", arg);
            //    return arg;
            //case "tag1_pt":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "平盘"));
            arg.Add(new KeyValuePair<string, string>("2", "对冲"));
            arg.Add(new KeyValuePair<string, string>("3", "未平盘"));
            BindSourceCache.TryAdd("tag1_pt", arg);
            //    return arg;
            //case "venues_pt":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
            arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
            arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
            arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
            arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
            arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
            arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));
            arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
            arg.Add(new KeyValuePair<string, string>("MANUAL", "自谈"));
            arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
            //arg.Add(new KeyValuePair<string, string>("MANUAL", "TradeBook"));
            BindSourceCache.TryAdd("venues_pt", arg);
            //    return arg;
            //case "quotemethod_pt":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "待定"));
            arg.Add(new KeyValuePair<string, string>("1", "发请求"));
            arg.Add(new KeyValuePair<string, string>("2", "收请求"));
            arg.Add(new KeyValuePair<string, string>("3", "发对话"));
            arg.Add(new KeyValuePair<string, string>("4", "收对话"));
            BindSourceCache.TryAdd("quotemethod_pt", arg);
            //    return arg;
            //case "side_pt":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "买入"));
            arg.Add(new KeyValuePair<string, string>("2", "卖出"));
            BindSourceCache.TryAdd("side_pt", arg);
            //    return arg;
            //case "companyid_gr":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "TPSC")); //国利
            arg.Add(new KeyValuePair<string, string>("2", "CNEX")); //国际
            arg.Add(new KeyValuePair<string, string>("3", "BGC"));  //中诚 
            arg.Add(new KeyValuePair<string, string>("4", "PTCN")); //平安
            arg.Add(new KeyValuePair<string, string>("5", "TJXT")); //信唐
            arg.Add(new KeyValuePair<string, string>("6", "UEDA")); //上田
            BindSourceCache.TryAdd("companyid_gr", arg);
            //    return arg;
            //case "approvalstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "新建"));
            arg.Add(new KeyValuePair<string, string>("2", "审批中"));
            arg.Add(new KeyValuePair<string, string>("3", "审批拒绝"));
            arg.Add(new KeyValuePair<string, string>("4", "审批通过"));
            arg.Add(new KeyValuePair<string, string>("5", "交易执行中"));
            arg.Add(new KeyValuePair<string, string>("6", "部分撤销"));
            arg.Add(new KeyValuePair<string, string>("7", "已撤销"));
            arg.Add(new KeyValuePair<string, string>("8", "交易终止"));
            arg.Add(new KeyValuePair<string, string>("9", "成交确认"));
            BindSourceCache.TryAdd("approvalstatus", arg);
            //    return arg;
            //case "businesstype":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "客需自营"));
            arg.Add(new KeyValuePair<string, string>("2", "客需对客"));
            arg.Add(new KeyValuePair<string, string>("3", "客需投顾"));
            arg.Add(new KeyValuePair<string, string>("4", "销售交易"));
            arg.Add(new KeyValuePair<string, string>("5", "自营交易"));
            arg.Add(new KeyValuePair<string, string>("6", "自营做市"));
            BindSourceCache.TryAdd("businesstype", arg);
            //    return arg;
            //case "manualprocessingpositionstatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "未执行"));
            arg.Add(new KeyValuePair<string, string>("1", "执行中"));
            arg.Add(new KeyValuePair<string, string>("2", "执行成功"));
            arg.Add(new KeyValuePair<string, string>("3", "执行失败"));
            BindSourceCache.TryAdd("manualprocessingpositionstatus", arg);
            //    return arg;
            //case "limitavailablestatus":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("Y", "正常"));
            arg.Add(new KeyValuePair<string, string>("N", "异常"));
            arg.Add(new KeyValuePair<string, string>("D", "删除"));
            BindSourceCache.TryAdd("limitavailablestatus", arg);
            //    return arg;
            //case "ifrepay":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("01", "到期一次性还本"));
            arg.Add(new KeyValuePair<string, string>("10", "提前还本"));
            BindSourceCache.TryAdd("ifrepay", arg);
            //    return arg;
            //case "sidepositon":
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "多仓"));
            arg.Add(new KeyValuePair<string, string>("1", "空仓"));
            BindSourceCache.TryAdd("sidepositon", arg);

            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "手动"));
            arg.Add(new KeyValuePair<string, string>("2", "自动"));
            BindSourceCache.TryAdd("UpdateType", arg);

            //平安养老类型
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "质押"));
            arg.Add(new KeyValuePair<string, string>("2", "STC在途意向"));
            arg.Add(new KeyValuePair<string, string>("3", "STC在途成交"));
            BindSourceCache.TryAdd("patradetype", arg);

            //平安养老成交方向
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "正回购"));
            arg.Add(new KeyValuePair<string, string>("2", "Bid"));
            arg.Add(new KeyValuePair<string, string>("3", "Ofr"));
            arg.Add(new KeyValuePair<string, string>("4", "Bid"));
            arg.Add(new KeyValuePair<string, string>("5", "Ofr"));
            BindSourceCache.TryAdd("patradeside", arg);


            //询价状态
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "未询价"));
            arg.Add(new KeyValuePair<string, string>("1", "询价中"));
            BindSourceCache.TryAdd("inquirystatus", arg);


            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "有效"));
            arg.Add(new KeyValuePair<string, string>("1", "已删除"));
            BindSourceCache.TryAdd("delflag", arg);

            //风控检查标记
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("0", "不通过"));
            arg.Add(new KeyValuePair<string, string>("1", "通过"));
            BindSourceCache.TryAdd("riskflag", arg);

            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("5", "未匹配"));
            arg.Add(new KeyValuePair<string, string>("6", "部分匹配"));
            arg.Add(new KeyValuePair<string, string>("7", "全部匹配"));
            arg.Add(new KeyValuePair<string, string>("8", "超量匹配"));
            BindSourceCache.TryAdd("matchstatus", arg);

            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("9", "未下发"));
            arg.Add(new KeyValuePair<string, string>("10", "部分下发"));
            arg.Add(new KeyValuePair<string, string>("11", "全部下发"));
            BindSourceCache.TryAdd("execstatus", arg);
            arg = new List<KeyValuePair<string, string>>();

            arg.Add(new KeyValuePair<string, string>("1", "对话"));
            arg.Add(new KeyValuePair<string, string>("2", "请求"));
            arg.Add(new KeyValuePair<string, string>("6", "固收"));
            arg.Add(new KeyValuePair<string, string>("7", "竞价"));
            BindSourceCache.TryAdd("dealway", arg);


            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "我方发起"));
            arg.Add(new KeyValuePair<string, string>("2", "对方发起"));
            BindSourceCache.TryAdd("trdsubtype", arg);

            //询价方向
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", "Bid"));
            arg.Add(new KeyValuePair<string, string>("2", "Ofr"));
            arg.Add(new KeyValuePair<string, string>("3", "Tkn"));
            arg.Add(new KeyValuePair<string, string>("4", "Gvn"));
            BindSourceCache.TryAdd("side_inquiry", arg);

            //报价标准
            arg = new List<KeyValuePair<string, string>>();
            arg.Add(new KeyValuePair<string, string>("1", ""));
            arg.Add(new KeyValuePair<string, string>("2", "全价"));
            arg.Add(new KeyValuePair<string, string>("3", ""));
            arg.Add(new KeyValuePair<string, string>("7", "行权"));
            BindSourceCache.TryAdd("pricetype", arg);
            //return arg;
            //}
            //}
            foreach (string mainkey in BindSourceCache.Keys)
            {
                List<KeyValuePair<string, string>> source = null;

                if (BindSourceCache.TryGetValue(mainkey, out source))
                {
                    this.DictEnum.Add(mainkey, source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                }
            }

            // 转换为 ConcurrentDictionary<string, Dictionary<string, string>>
            //DictEnum = new Dictionary<string, Dictionary<string, string>>(BindSourceCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(innerKvp => innerKvp.Key, innerKvp => innerKvp.Value)));
        }
    }
}
