using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model
{
   public class OrderTimeModel
    {
        /// <summary>
        /// 报价选择过期类型  默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}"
        /// </summary>
        public string ValidTimeType;

        /// <summary>
        /// 过期时间  yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string ValidUntilTime;


        public string IF_SUB_BOND;
        public string VIP;
        public string ORG_TYPE;

        public string MorethanMarketBPCheck; //与市场最优倒挂 0 -未勾选；1-勾选
        public string MorethanMarketBP; //与市场最优倒挂BP数

        /// <summary>
        /// 过滤信息
        /// </summary>
        public string FocusTags;

        // 是我
        public string isMe;
        //动态期限字符串 5Y30D,10Y60D
        public string termRangeStr;
        //动态期限的天数 3650,7300
        public string termDays;
        // 添加动态票面订阅 xxx,xxx
        public string couponRangeStr;
        // 报价动态订阅
        public string priceRangeStr;
        // 估值动态订阅
        public string valuationRangeStr;

        /// <summary> 
        /// QuoteType virtual 模糊需求 require 需求 order 现券 irs 利率互换 tbookirs  恢复需求unrefrequire   恢复模糊需求unrefvirtual
        /// </summary>
        public string QuoteType;

        /// <summary>
        /// 原文
        /// </summary>
        public string Text;

        /// <summary>
        /// 使用的发送接口 1:TableSvr 0 TradeBook
        /// </summary>
        public string IsFromTableSvrOrTradeBook;

        /// <summary>
        /// 当为虚拟债券时，下单途径  irs or innerboard
        /// </summary>
        public string IsFromIRSorInner;

        /// <summary>
        /// 原始记录的ID
        /// </summary>
        public string IntentionID;

        /// <summary>
        /// 他人操作员 Trader
        /// </summary>
        public string UserName;

        /// <summary>
        /// 【平安养老】 恒生基金编号
        /// </summary>
        public string HsProductId;
    }
}
