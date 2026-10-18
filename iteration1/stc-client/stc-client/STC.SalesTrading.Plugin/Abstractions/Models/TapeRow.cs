using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 行情中心"全部"看板行（后端价格簿 PriceResult 展开）。
    /// <para>一行 = 一个 <c>NoMDEntries</c> 条目（即一笔报价），经 quoteRefs 关联报价身份。
    /// 纯 DTO；筛选/选中等 UI 状态由模块 ViewModel 层包装。</para>
    /// </summary>
    public class TapeRow
    {
        /// <summary>
        /// 所属市场。后端价格簿按 filter.market 查询域区分、行上无此字段，
        /// 由服务实现按查询填入（UI 右键分组菜单/存储过滤需要行级市场语义）。
        /// </summary>
        public BondMarket Market { get; set; }

        /// <summary>关联报价 Id。编辑报价时用它拉完整报价；零散行（无关联）为空。</summary>
        public string QuoteId { get; set; }

        /// <summary>
        /// 报价版本，取自 quoteRefs.quoteRevision。编辑报价时作 expectedRevision 兜底展示。
        /// </summary>
        public string Revision { get; set; }

        /// <summary>债券代码，带市场后缀（securityId）。</summary>
        public string SecurityId { get; set; }

        /// <summary>债券简称（reference.securityName）。</summary>
        public string SecurityName { get; set; }

        /// <summary>方向（STC MDEntryType "0"/"1" 映射，与后端 side 编码不同）。</summary>
        public QuoteDirection Side { get; set; }

        /// <summary>比较用收益率（STC MDEntryPx）。</summary>
        public decimal? Price { get; set; }

        /// <summary>报量，单位万（STC MDEntrySize）。</summary>
        public decimal? Quantity { get; set; }

        /// <summary>清算速度（STC SettlType）。</summary>
        public SettlementType SettlType { get; set; }

        /// <summary>报价机构名称。价格簿条目当前不带该字段，后端补齐后直接绑定。</summary>
        public string InstitutionName { get; set; }

        /// <summary>报价机构 Id（价格簿条目随行下发；新报价预填 CRM 选中值用）。</summary>
        public string InstitutionId { get; set; }

        /// <summary>报价联系人名称。同上。</summary>
        public string ContactName { get; set; }

        /// <summary>报价联系人 Id（同上）。</summary>
        public string ContactId { get; set; }

        /// <summary>报价归属用户（STC UserID）。</summary>
        public string OwnerUserId { get; set; }

        /// <summary>报价归属用户显示名。预留：后端未提供时 UI 显示 <see cref="OwnerUserId"/>。</summary>
        public string OwnerUserName { get; set; }

        /// <summary>条目更新时间。取自 STC MDEntryDate+MDEntryTime 或 books.updateTime。</summary>
        public DateTime? UpdateTime { get; set; }

        #region 旗标（看板"加急/推荐/整手"筛选的数据来源）

        /// <summary>加急。来源：STC 扩展旗标位或 quoteRefs 关联报价，后端确认前看板不显示该列。</summary>
        public bool Urgent { get; set; }

        /// <summary>推荐。同上。</summary>
        public bool Recommended { get; set; }

        /// <summary>整手。同上。</summary>
        public bool Whole { get; set; }

        #endregion

        #region reference（债券资料；决策 #9/#10）

        /// <summary>剩余期限。</summary>
        public string Maturity { get; set; }

        /// <summary>债券库。</summary>
        public string BondLibrary { get; set; }

        /// <summary>主体评级。</summary>
        public string IssuerRating { get; set; }

        /// <summary>债项评级。</summary>
        public string BondRating { get; set; }

        /// <summary>中债估值 YTM，%。</summary>
        public decimal? NzYtm { get; set; }

        /// <summary>中债净价。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中债偏离，BP。</summary>
        public decimal? NzDeviation { get; set; }

        /// <summary>中证估值 YTM，%。</summary>
        public decimal? CsYtm { get; set; }

        /// <summary>中证净价。</summary>
        public decimal? CsNetPrice { get; set; }

        /// <summary>中证全价（看板列工厂统一插列；计算源暂缺，恒空占位）。</summary>
        public decimal? CsFullPrice { get; set; }

        #endregion

        /// <summary>最新成交价。行情域无此数据源，保留字段暂不赋值。</summary>
        public decimal? LastPrice { get; set; }

        /// <summary>是否本人报价。由 <see cref="OwnerUserId"/> 与当前登录用户比较得出。</summary>
        public bool IsMine { get; set; }
    }
}
