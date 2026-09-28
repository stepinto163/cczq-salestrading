using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Shared.Messages
{
    /// <summary>
    /// 从「最优」页的同档报价浮层点击本人报价 → 跳转到对应市场「全部」子页并选中该报价
    /// （原型 0910 openBestQuoteInAll）。
    /// 由最优页 VM 发布，行情中心容器订阅后切换子页并在「全部」列表里定位该行。
    /// </summary>
    public class LocateQuoteInAllMessage
    {
        public LocateQuoteInAllMessage(BondMarket market, string quoteId)
        {
            Market = market;
            QuoteId = quoteId;
        }

        /// <summary>目标市场：信用 / 利率。</summary>
        public BondMarket Market { get; }

        /// <summary>要定位的报价 Id。</summary>
        public string QuoteId { get; }
    }
}
