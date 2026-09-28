namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>多档行情行（详情面板 C5 多档报价表：来源Bid/Vol.Bid/Bid/Ofr/Vol.Ofr/来源Ofr）。</summary>
    public class InstQuoteRow
    {
        /// <summary>Bid 来源机构。</summary>
        public string InstBid { get; set; }

        /// <summary>Bid 报量，单位万。</summary>
        public decimal? VolBid { get; set; }

        /// <summary>Bid 价。</summary>
        public decimal? Bid { get; set; }

        /// <summary>Ofr 价。</summary>
        public decimal? Ofr { get; set; }

        /// <summary>Ofr 报量，单位万。</summary>
        public decimal? VolOfr { get; set; }

        /// <summary>Ofr 来源机构。</summary>
        public string InstOfr { get; set; }

        /// <summary>Bid 侧渠道码（Venues，供渠道筛选/配色）。</summary>
        public string VenueBid { get; set; }

        /// <summary>Ofr 侧渠道码（Venues）。</summary>
        public string VenueOfr { get; set; }
    }
}
