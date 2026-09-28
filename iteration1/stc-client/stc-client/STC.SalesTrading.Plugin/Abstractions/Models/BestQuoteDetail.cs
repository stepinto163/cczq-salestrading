using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 最优页「同档最优报价」明细行（原型 0910 bestQuoteLayerHtml 的 7 列表格）：
    /// 价格 / 折算收益率 / 量 / 报价机构 / 报价交易员 / 长城交易员 / 报价时间。
    /// 权限规则同原型：仅本人报价（IsMine）可点击跳转对应市场「全部」页；
    /// 非本人报价的机构与交易员字段留空显示 `--`。
    /// </summary>
    public class BestQuoteDetail
    {
        /// <summary>报价（价格 + 价格类型短标识，如「2.9150 YTM」）。</summary>
        public string Price { get; set; }

        /// <summary>折算收益率（收益率口径直接取报价；净价/全价口径待中债估值换算，暂显示 —）。</summary>
        public string Yield { get; set; }

        /// <summary>报量，单位万。</summary>
        public decimal? Volume { get; set; }

        /// <summary>报价机构（非本人报价为空，界面显示 --）。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员（非本人报价为空，界面显示 --）。</summary>
        public string Trader { get; set; }

        /// <summary>长城交易员（本方归属）。</summary>
        public string OwnerTrader { get; set; }

        /// <summary>报价时间显示文本（HH:mm:ss）。</summary>
        public string Time { get; set; }

        /// <summary>是否本人报价：决定该行是否可点击跳转。</summary>
        public bool IsMine { get; set; }

        /// <summary>来源报价 Id（跳转「全部」页定位用）。</summary>
        public string QuoteId { get; set; }
    }
}
