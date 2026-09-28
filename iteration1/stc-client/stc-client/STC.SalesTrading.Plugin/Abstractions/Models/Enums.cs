using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>报价方向。Bid = 买价（琥珀橙），Ofr = 卖价（蓝）——全站统一配色语义。</summary>
    public enum QuoteDirection
    {
        Bid,
        Ofr
    }

    /// <summary>价格类型。对应原型：到期收益率/行权收益率/净价/全价/利差/面值。</summary>
    public enum PriceType
    {
        YieldToMaturity,
        YieldToExercise,
        CleanPrice,
        DirtyPrice,
        Spread,
        Par
    }

    /// <summary>清算速度。对应原型：T+0/T+1/T+2/T+3/远期。</summary>
    public enum SettlementType
    {
        T0,
        T1,
        T2,
        T3,
        Forward
    }

    /// <summary>报价方式。对应原型：QT/ideal/手工录入。</summary>
    public enum QuoteMode
    {
        Qt,
        Ideal,
        Manual
    }

    /// <summary>报价状态。对应原型：active/revoked（有效/撤销）。</summary>
    public enum QuoteStatus
    {
        Active,
        Revoked
    }

    /// <summary>待确认批次状态。对应原型：待确认/部分识别失败/识别失败/部分确认。</summary>
    public enum BatchStatus
    {
        PendingReview,
        PartialFailed,
        RecognitionFailed,
        PartiallyConfirmed
    }

    /// <summary>模糊报价状态（原型 0910 由「意向报价」改名）。对应原型：有效/撤销。</summary>
    public enum IntentStatus
    {
        Active,
        Revoked
    }

    /// <summary>报价提醒类型。对应原型：关注债券/筛选分组/对价提醒/我的最优。</summary>
    public enum AlertType
    {
        FavoriteBond,
        FilterGroup,
        PriceMatch,
        MyBestQuote
    }

    /// <summary>成交方向。对应原型：TRD/TKN/GVN。</summary>
    public enum TradeDirection
    {
        Trd,
        Tkn,
        Gvn
    }

    /// <summary>债券市场。对应原型：信用/利率。</summary>
    public enum BondMarket
    {
        Credit,
        Rate
    }

    /// <summary>报价旗标（可组合）。对应原型批量标签：急/荐/整/OCO/打包。</summary>
    [Flags]
    public enum QuoteFlag
    {
        None = 0,
        Urgent = 1,     // 急 #ef4444
        Recommend = 2,  // 荐 #3fb950
        Whole = 4,      // 整 #ec4899
        Oco = 8,        // OCO #79c0ff
        Packing = 16    // 打包 #a78bfa
    }

    /// <summary>报价变更事件类型（推送通道预留）。对应原型 SSE：new_quote/edit_quote/revoke_quote。</summary>
    public enum QuoteEventKind
    {
        New,
        Edit,
        Revoke
    }

    /// <summary>
    /// 智能推荐结果范围。对应原型 rec-tabs：全部/本券/相似券/相似机构。
    /// 推荐行仅携带 ExactBond/SimilarBond/SimilarInstitution 三种路径；
    /// Combined 是"全部"切换态，表示不过滤（含三种路径）。
    /// </summary>
    public enum RecommendationScope
    {
        Combined,
        ExactBond,
        SimilarBond,
        SimilarInstitution
    }
}
