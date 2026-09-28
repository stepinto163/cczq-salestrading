using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 枚举公共类
    /// </summary>
    public class CommonMapUtil
    {
        /// <summary>
        /// 枚举字典
        /// </summary>
        private static readonly Dictionary<string, List<KeyValuePair<string, string>>> BindSourceCache = new Dictionary<string, List<KeyValuePair<string, string>>>();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public static void ClearComboSource()
        {
            BindSourceCache.Clear();
        }

        /// <summary>
        /// 找出枚举对应值
        /// </summary>
        /// <param name="key">枚举键值</param>
        /// <param name="param">枚举类型</param>
        /// <returns></returns>
        public static string FindEnum(string key, string param)
        {
            List<KeyValuePair<string, string>> comboList = GetComboSource(param);
            foreach (KeyValuePair<string, string> pair in comboList)
            {
                if (pair.Key == key)
                {
                    return pair.Value;
                }
            }
            return key;
        }

        /// <summary>
        /// 根据typeName获得枚举列表
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetComboSource(string typeName)
        {
            return CreateBindSource(typeName);
        }

        /// <summary>
        /// 创建枚举数据源
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static List<KeyValuePair<string, string>> CreateBindSource(string typeName)
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
                    case "settletype":
                        arg.Add(new KeyValuePair<string, string>("1", "T+0"));
                        arg.Add(new KeyValuePair<string, string>("2", "T+1"));
                        arg.Add(new KeyValuePair<string, string>("3", "明天+0"));
                        arg.Add(new KeyValuePair<string, string>("4", "明天+1"));
                        arg.Add(new KeyValuePair<string, string>("2,3", "T+1(不限)"));
                        arg.Add(new KeyValuePair<string, string>("1,2,3", "T+0,T+1(不限)"));
                        return arg;
                    case "venues":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "销售"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "人工"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
                        arg.Add(new KeyValuePair<string, string>("UEDA", "上田"));

                        arg.Add(new KeyValuePair<string, string>("SSE", "上新债"));
                        arg.Add(new KeyValuePair<string, string>("SSF", "上固收"));
                        arg.Add(new KeyValuePair<string, string>("SZE", "深固收"));


                        return arg;

                    case "dealpermission":
                        arg.Add(new KeyValuePair<string, string>("TPSC", "200201"));
                        arg.Add(new KeyValuePair<string, string>("CTTJ", "200204"));
                        arg.Add(new KeyValuePair<string, string>("CNEX", "200202"));
                        arg.Add(new KeyValuePair<string, string>("CCTB", "200205"));
                        arg.Add(new KeyValuePair<string, string>("PTCN", "200203"));
                        arg.Add(new KeyValuePair<string, string>("ODM", "X-Bond"));
                        arg.Add(new KeyValuePair<string, string>("IDB", "销售"));
                        arg.Add(new KeyValuePair<string, string>("QDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("HTQDM", "RFQ"));
                        arg.Add(new KeyValuePair<string, string>("INNER", "内部"));
                        arg.Add(new KeyValuePair<string, string>("QDMESP", "双边做市"));
                        arg.Add(new KeyValuePair<string, string>("MANUAL", "人工"));
                        arg.Add(new KeyValuePair<string, string>("QUANT", "量化"));
                        return arg;

                    case "ordstatus":
                        arg.Add(new KeyValuePair<string, string>("N", "新增"));
                        arg.Add(new KeyValuePair<string, string>("0", "正常"));
                        arg.Add(new KeyValuePair<string, string>("4", "撤销"));
                        arg.Add(new KeyValuePair<string, string>("8", "拒绝"));
                        arg.Add(new KeyValuePair<string, string>("1", "部分成交"));
                        arg.Add(new KeyValuePair<string, string>("2", "全部成交"));
                        arg.Add(new KeyValuePair<string, string>("S", "报价中"));
                        arg.Add(new KeyValuePair<string, string>("6", "撤销中"));
                        arg.Add(new KeyValuePair<string, string>("C", "异常"));
                        return arg;
                    case "firmflag":
                        arg.Add(new KeyValuePair<string, string>("0", "nonfirm"));
                        arg.Add(new KeyValuePair<string, string>("1", "firm"));
                        return arg;
                    case "venueordstatus":
                        arg.Add(new KeyValuePair<string, string>("N", "新增"));
                        arg.Add(new KeyValuePair<string, string>("0", "待处理"));
                        arg.Add(new KeyValuePair<string, string>("3", "处理中"));
                        arg.Add(new KeyValuePair<string, string>("1", "部分成交"));
                        arg.Add(new KeyValuePair<string, string>("2", "已成交"));
                        arg.Add(new KeyValuePair<string, string>("4", "已撤销"));
                        arg.Add(new KeyValuePair<string, string>("6", "待撤销"));
                        arg.Add(new KeyValuePair<string, string>("7", "撤销中")); 
                        //arg.Add(new KeyValuePair<string, string>("8", "撤销失败"));
                        arg.Add(new KeyValuePair<string, string>("8", "已拒绝"));
                        return arg;
                    case "venuestatus":
                        arg.Add(new KeyValuePair<string, string>("新增", "N"));
                        arg.Add(new KeyValuePair<string, string>("待处理", "0"));
                        arg.Add(new KeyValuePair<string, string>("处理中", "3"));
                        arg.Add(new KeyValuePair<string, string>("部分成交", "1"));
                        arg.Add(new KeyValuePair<string, string>("已成交", "2"));
                        arg.Add(new KeyValuePair<string, string>("已撤销", "4"));
                        arg.Add(new KeyValuePair<string, string>("待撤销", "6"));
                        arg.Add(new KeyValuePair<string, string>("撤销中", "7"));
                        //arg.Add(new KeyValuePair<string, string>("撤销失败", "8"));
                        arg.Add(new KeyValuePair<string, string>("已拒绝", "8"));

                        return arg;
                    case "rejecterreason":
                        arg.Add(new KeyValuePair<string, string>("5", "10006"));//忽略
                        arg.Add(new KeyValuePair<string, string>("4", "10007"));//撤销成功
                        arg.Add(new KeyValuePair<string, string>("6", "10008"));//拒绝
                        arg.Add(new KeyValuePair<string, string>("7", "10009"));//失败
                        arg.Add(new KeyValuePair<string, string>("8", "10005"));//等待时间已到

                        return arg;
                }
            }
            return null;
        }
    }
}
