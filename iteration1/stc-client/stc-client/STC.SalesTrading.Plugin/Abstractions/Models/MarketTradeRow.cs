using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>成交行情行（详情面板 C5 成交行情表：方向/代码/简称/价/量/时间）。</summary>
    public class MarketTradeRow
    {
        /// <summary>成交方向：TRD(绿)/TKN(紫)/GVN(金)。</summary>
        public TradeDirection Direction { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>成交价。</summary>
        public decimal? Price { get; set; }

        /// <summary>成交量，单位万。</summary>
        public decimal? Volume { get; set; }

        /// <summary>成交时间。</summary>
        public DateTime? Time { get; set; }
    }
}
