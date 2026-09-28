using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
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
        private static List<KeyValuePair<string, string>> InitByEnum(string typeName)
        {
            List<KeyValuePair<string, string>> arg = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(typeName) == false)
            {
                switch (typeName.ToLower())
                {
                    case "side":
                        arg.Add(new KeyValuePair<string, string>("1", "Bid"));
                        arg.Add(new KeyValuePair<string, string>("2", "Ofr"));
                        arg.Add(new KeyValuePair<string, string>("X", "TKN"));
                        arg.Add(new KeyValuePair<string, string>("Y", "GVN"));
                        arg.Add(new KeyValuePair<string, string>("Z", "TRD"));
                        return arg;
                    case "settletype_myorder":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1(今+1)"));
                        arg.Add(new KeyValuePair<string, string>("3", "T+1(明+0)"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        arg.Add(new KeyValuePair<string, string>("2,3", "T+1(不限)"));
                        return arg;
                    case "settletype_revert":
                        arg.Add(new KeyValuePair<string, string>("T+0","1"));
                        arg.Add(new KeyValuePair<string, string>("T+1", "2"));
                        arg.Add(new KeyValuePair<string, string>("T+1(今+1)","2"));
                        arg.Add(new KeyValuePair<string, string>("明+0", "3"));
                        arg.Add(new KeyValuePair<string, string>("T+1(明+0)","3"));
                        arg.Add(new KeyValuePair<string, string>("明天+1","4"));
                        arg.Add(new KeyValuePair<string, string>("T+1(不限)", "2,3"));
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
                    case "venues":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "XB"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "内部"));//"人工"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
                        return arg;
                    case "venues_cn":
                        arg.Add(new KeyValuePair<string, string>("国利", "TPSC"));
                        arg.Add(new KeyValuePair<string, string>("信唐", "CTTJ"));
                        arg.Add(new KeyValuePair<string, string>("国际", "CNEX"));
                        arg.Add(new KeyValuePair<string, string>("中诚", "CCTB"));
                        arg.Add(new KeyValuePair<string, string>("平安", "PTCN"));
                        arg.Add(new KeyValuePair<string, string>("XB", "ODM"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
                        arg.Add(new KeyValuePair<string, string>("RFQ", "QDM"));
                        arg.Add(new KeyValuePair<string, string>("RFQ", "HTQDM"));
                        arg.Add(new KeyValuePair<string, string>("内部", "INNER"));
                        arg.Add(new KeyValuePair<string, string>("双边", "QDMESP"));// 做市"));
                        arg.Add(new KeyValuePair<string, string>("内部", "MANUAL"));//"人工"));
                        arg.Add(new KeyValuePair<string, string>("量化", "QUANT"));
                        return arg;
                    case "venues_exe":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "TradeBook"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "TradeBook"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "TradeBook"));
                        return arg;
                    case "venuesxb":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "XBOND"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "IDB"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边"));// 做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "内部"));//"人工"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
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
                        arg.Add(new KeyValuePair<string, string>("0", "/STC.XBond.Plugin;component/Images/stc_icon_favorite_normal.png"));
                        arg.Add(new KeyValuePair<string, string>("1", "/STC.XBond.Plugin;component/Images/stc_icon_favorite_seleted.png"));
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
                    case "side2":
                        arg.Add(new KeyValuePair<string, string>("1", "GVN"));
                        arg.Add(new KeyValuePair<string, string>("2", "TKN"));
                        arg.Add(new KeyValuePair<string, string>("3", "TRD"));
                        arg.Add(new KeyValuePair<string, string>("X", "TKN"));
                        arg.Add(new KeyValuePair<string, string>("Y", "GVN"));
                        arg.Add(new KeyValuePair<string, string>("Z", "TRD"));
                        return arg;
                    case "mmstatus":
                        arg.Add(new KeyValuePair<string, string>("1", "危险"));
                        arg.Add(new KeyValuePair<string, string>("2", "可达标"));
                        arg.Add(new KeyValuePair<string, string>("3", "已达标"));
                        arg.Add(new KeyValuePair<string, string>("4", "失败"));
                        return arg;
                }
            }
            return null;
        }
    }  
}
