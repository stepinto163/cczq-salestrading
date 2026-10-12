using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>盘口档位方向。</summary>
    public enum QuoteFeedSide
    {
        Bid,
        Ofr,
    }

    /// <summary>宿主行情快照的单档报价（已从 FIX GNoMDEntries 归一化）。</summary>
    public class QuoteFeedLevel
    {
        /// <summary>买/卖方向（FIX MDEntryType：0=Bid，1=Ofr）。</summary>
        public QuoteFeedSide Side { get; set; }

        /// <summary>报价（MDEntryPx，净价或收益率按 PriceType）。</summary>
        public decimal Price { get; set; }

        /// <summary>净价（NetPrice 字段，可空）。</summary>
        public decimal? NetPrice { get; set; }

        /// <summary>收益率%（Yield 字段，可空）。</summary>
        public decimal? Yield { get; set; }

        /// <summary>报量，单位万（MDEntrySize）。</summary>
        public decimal Volume { get; set; }

        /// <summary>报价机构（InstitutionName，缺省回退 PartyName；作 tooltip 全称）。</summary>
        public string Institution { get; set; }

        /// <summary>来源原文（GenerateSourceByVenues：内部单=“角色-交易员”，其余=Venues 渠道码）。</summary>
        public string Source { get; set; }

        /// <summary>来源展示标签（VenuesSimpleConvertor 同款映射：TPSC=国利/CTTJ=信唐/本人单=“我”…）。</summary>
        public string SourceLabel { get; set; }

        /// <summary>交易员（ShowTrader/TraderName）。</summary>
        public string Trader { get; set; }

        /// <summary>渠道码（Venues：TPSC=国利/CNEX=国际/PTCN=平安/CCTB=中诚/UEDA=上田/CTTJ=信唐/ODM=X-Bond/QDMESP=双边…），
        /// 用于渠道筛选与徽章配色（VenuesColorConvertor 按此取色）。</summary>
        public string VenueCode { get; set; }

        /// <summary>档位号（MDPriceLevel，0 = 未提供）。</summary>
        public int Level { get; set; }

        /// <summary>报价时间（MDEntryDate + MDEntryTime）。</summary>
        public DateTime? QuoteTime { get; set; }
    }

    /// <summary>最新成交（LastExecutionProcesser 回报归一化）。</summary>
    public class QuoteFeedLastExec
    {
        /// <summary>成交价（LastPx）。</summary>
        public decimal? Price { get; set; }

        /// <summary>成交量（LastQty）。</summary>
        public decimal? Volume { get; set; }

        /// <summary>方向（Side 原文：TRD/TKN/GVN 等）。</summary>
        public string Side { get; set; }

        /// <summary>成交收益率%（ExecutionReport.Yield，可空）。</summary>
        public decimal? Yield { get; set; }

        /// <summary>成交时间（TransactTime）。</summary>
        public DateTime? Time { get; set; }
    }

    /// <summary>债券主数据（BondsInfoPo 归一化子集）。</summary>
    public class QuoteFeedBondInfo
    {
        /// <summary>债券代码。</summary>
        public string Code { get; set; }

        /// <summary>简称（Short_Name）。</summary>
        public string ShortName { get; set; }

        /// <summary>剩余期限（RemaingingTime 原文）。</summary>
        public string RemainingTime { get; set; }

        /// <summary>当前票面%（Coupon_Rate_Current 原文）。</summary>
        public string CouponRate { get; set; }

        /// <summary>中债估值展示串（CCDCValua.CCDCValuationShow，形如 "净价/YTM"）。</summary>
        public string ValuationShow { get; set; }

        /// <summary>市场标识（MarketIndicator：4=现券等）。</summary>
        public string MarketIndicator { get; set; }
    }
}
