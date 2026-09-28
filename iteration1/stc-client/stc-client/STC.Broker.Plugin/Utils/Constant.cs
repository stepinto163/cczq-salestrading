using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public static class Constant
    {
        /// <summary>
        /// 地方债标志
        /// </summary>
        public static readonly HashSet<string> LocalBondFlags = new HashSet<string> { "TLB", "LLB" };

        /// <summary>
        /// string Comma ","
        /// </summary>
        public const string Comma = ",";

        /// <summary>
        /// +
        /// </summary>
        public const string Plus = "+";

        /// <summary>
        /// --
        /// </summary>
        public const string DoubleDash = "--";

        /// <summary>
        /// 四个空格
        /// </summary>
        public const string FourSpace = "    ";

        #region 客户端系统参数

        /// <summary>
        /// 其他人成交单是否按照机构交易员拆分
        /// </summary>
        public const string SplitShowOtherOrders = "splitshowotherorders";

        /// <summary>
        /// 地方债带简称
        /// </summary>
        public const string WithShortNameLGB = "withshortnamelgb";

        /// <summary>
        /// 带剩余量信息
        /// </summary>
        public const string WithResidualQuantity = "withbalanceinfo";

        /// <summary>
        /// 明细含公司名称展示
        /// </summary>
        public const string WithCompanyName = "withcompanyname";

        /// <summary>
        ///  qq发送带要素变化
        /// </summary>
        public const string WithQQElementChange = "withqqelementchange";

        /// <summary>
        ///  通过broker发送qq
        /// </summary>
        public const string WithAssistantQQSendMsg = "withassistantqqsendmsg";

        /// <summary>
        ///  带私募债类型
        /// </summary>
        public const string WithpPivatebondtype = "withprivatebondtype";



        #endregion

        /// <summary>
        /// 买入
        /// </summary>
        public const string Buy = "买入";

        /// <summary>
        /// 卖出
        /// </summary>
        public const string Sell = "卖出";

        /// <summary>
        /// 其他人成交单
        /// </summary>
        public const string OtherOrder = "其他人成交单";


        /// <summary>
        /// 无机构成交单
        /// </summary>
        public const string NoInstOrder = "无机构成交单";

        /// <summary>
        /// 经纪商标识
        /// </summary>
        public static readonly HashSet<string> BrokerFlags = new HashSet<string>() { "BGC" };

        /// <summary>
        /// 经纪商名称
        /// </summary>
        public static readonly Dictionary<string, string> DicBrokerNames = new Dictionary<string, string>() {
            {"BGC","中诚" }, {"TP","国利" }, {"PATR","平安" }, {"TJXT","信唐" }, {"UEDA","上田" }
        };

        /// <summary>
        /// 台子id 配置
        /// </summary>
        public const string SubCompanyId = "subcompanyid";

        /// <summary>
        /// OrderDetail
        /// </summary>
        public const string OrderDetail = "OrderDetail";

        /// <summary>
        /// OrderBridge
        /// </summary>
        public const string OrderBridge = "OrderBridge";

        /// <summary>
        /// PricingTab
        /// </summary>
        public const string PricingTab = "PricingTab";


        /// <summary>
        /// 桥状态默认展示
        /// </summary>
        public const string BridgeStatusShow = "...";
    }
}
