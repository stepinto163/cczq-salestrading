using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 报价行（后端 Quote 统一模型）。纯 DTO，不实现 INPC——行内编辑所需的 UI 状态
    /// 包装在各模块 ViewModel 层。
    /// <para>本模型合并了原 <c>MgmtQuoteRow</c>（报价管理）与 <c>QuoteConfirmRow</c>（报价确认）：
    /// 后端两者是同一条 Quote，只是查询的 status 范围不同。属性名对齐后端字段，
    /// 便于对照规范排查；仅有 UI 语义需要处做枚举化。</para>
    /// </summary>
    public class QuoteRow
    {
        #region 标识与来源

        /// <summary>服务端报价 Id，操作寻址主键（后端 quoteId）。</summary>
        public string QuoteId { get; set; }

        /// <summary>
        /// 报价版本。后端 revision 是十进制整数字符串，故此处也用 string 以避免
        /// 数值化后与后端语义偏差；不可按字典序比较大小。
        /// 写操作时作 expectedRevision 提交。
        /// </summary>
        public string Revision { get; set; }

        /// <summary>生命周期状态（后端 status）。</summary>
        public QuoteStatus Status { get; set; }

        /// <summary>
        /// 客户端新建本地行时生成的唯一标识，创建时提交；创建应答与推送通过它
        /// 回绑本地行。同一创建重试保持不变（后端 clientQuoteId）。
        /// </summary>
        public string ClientQuoteId { get; set; }

        /// <summary>来源消息 Id。报价确认页按它分组；纯手工录入行为空（后端 sourceMessageId）。</summary>
        public string SourceMessageId { get; set; }

        /// <summary>来源类型 MANUAL/QT/IDEAL（后端 source.type）。</summary>
        public SourceType SourceType { get; set; }

        #endregion

        #region 业务字段

        /// <summary>债券代码，带市场后缀（后端 securityId，如 250001.IB）。</summary>
        public string SecurityId { get; set; }

        /// <summary>债券简称（reference 补充，或识别结果 symbol）。</summary>
        public string SecurityName { get; set; }

        /// <summary>方向（后端 side："1"/"2"）。</summary>
        public QuoteDirection Side { get; set; }

        /// <summary>计价方式（后端 quoteType："1"/"2"/"3"）。</summary>
        public PriceType QuoteType { get; set; }

        /// <summary>
        /// 收益率类型（后端 yieldType："1"/"2"）。quoteType 为净价/全价时必须为 null。
        /// </summary>
        public YieldType? YieldType { get; set; }

        /// <summary>价格或收益率（后端 price）。收益率为百分数，最多 4 位小数。</summary>
        public decimal? Price { get; set; }

        /// <summary>报量，单位万（后端 quantity，须 &gt; 0）。</summary>
        public decimal? Quantity { get; set; }

        /// <summary>交易日期（后端 tradeDate）。</summary>
        public DateTime? TradeDate { get; set; }

        /// <summary>清算速度 STC 代码 1/2/3/4 = T+0/T+1/T+2/T+3（后端 settlType）。</summary>
        public SettlementType SettlType { get; set; }

        /// <summary>报价机构 CRM Id（后端 institutionId）。确认时必填。</summary>
        public string InstitutionId { get; set; }

        /// <summary>报价机构名称。仅有名称而无 Id 时确认会被后端拒绝。</summary>
        public string InstitutionName { get; set; }

        /// <summary>报价联系人 CRM Id（后端 contactId），须属于 InstitutionId。</summary>
        public string ContactId { get; set; }

        /// <summary>报价联系人名称。</summary>
        public string ContactName { get; set; }

        /// <summary>报价归属用户（后端 ownerUserId）。长城交易员列空时回退显示它。</summary>
        public string OwnerUserId { get; set; }

        /// <summary>
        /// 报价归属用户显示名（长城交易员列）。预留字段：后端当前未提供显示名，
        /// 为空时 UI 显示 <see cref="OwnerUserId"/>。
        /// </summary>
        public string OwnerUserName { get; set; }

        /// <summary>急标记。</summary>
        public bool Urgent { get; set; }

        /// <summary>荐标记。</summary>
        public bool Recommended { get; set; }

        /// <summary>整量标记。</summary>
        public bool Whole { get; set; }

        /// <summary>OCO 标记。与 Packing 互斥，不能同时为 true。</summary>
        public bool Oco { get; set; }

        /// <summary>打包标记。与 Oco 互斥。</summary>
        public bool Packing { get; set; }

        /// <summary>备注，可显式清空（后端 remark）。</summary>
        public string Remark { get; set; }

        #endregion

        #region reference（债券资料，服务端补充；决策 #9/#10）

        /// <summary>剩余期限，如 "7.73Y"（reference.remainingYears + "Y"）。</summary>
        public string Maturity { get; set; }

        /// <summary>债券库（reference.bondSubtype）。</summary>
        public string BondLibrary { get; set; }

        /// <summary>主体评级（reference.issuerRating）。</summary>
        public string IssuerRating { get; set; }

        /// <summary>债项评级（reference.bondRating）。</summary>
        public string BondRating { get; set; }

        /// <summary>中债估值 YTM，%（reference.valuationYield）。</summary>
        public decimal? NzYtm { get; set; }

        /// <summary>中债净价（reference.valuationCleanPrice）。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中债偏离，BP。买方取 bidValuationSpread、卖方取 offerValuationSpread。</summary>
        public decimal? NzDeviation { get; set; }

        /// <summary>中证估值 YTM，%（reference.csiValuationYield）。</summary>
        public decimal? CsYtm { get; set; }

        /// <summary>中证净价（reference.csiValuationCleanPrice）。</summary>
        public decimal? CsNetPrice { get; set; }

        /// <summary>中证全价（看板列工厂统一插列；计算源暂缺，恒空占位）。</summary>
        public decimal? CsFullPrice { get; set; }

        #endregion

        #region 时间（均为服务端维护，带时区 ISO 8601）

        /// <summary>首次创建时间，永不改写。</summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>最近一次成功变更时间。后端列表固定按它降序 + quoteId 升序。</summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>最近一次报价发布/刷新时间。首次确认前为 null。</summary>
        public DateTime? QuoteTime { get; set; }

        /// <summary>首次人工确认时间；未确认为 null，Unrefer 不覆盖。</summary>
        public DateTime? ConfirmTime { get; set; }

        #endregion

        #region 提示与历史扩展

        /// <summary>后端提示的缺失/错误字段列表，供页面定位（后端 validationIssues）。</summary>
        public IReadOnlyList<string> ValidationIssues { get; set; }

        /// <summary>历史动作 CREATE/UPDATE/CONFIRM/REFER/UNREFER/DELETE；仅历史页使用。</summary>
        public string Action { get; set; }

        #endregion

        #region 显示辅助（UI 用，不提交）

        /// <summary>来源徽标文本（QT/ideal/MANUAL）。</summary>
        public string SourceTypeText
        {
            get
            {
                switch (SourceType)
                {
                    case Models.SourceType.Qt: return "QT";
                    case Models.SourceType.Ideal: return "ideal";
                    default: return "手工";
                }
            }
        }

        /// <summary>旗标显示文本（急/荐/整/OCO/打包，空格分隔；无则空）。</summary>
        public string FlagsText
        {
            get
            {
                var parts = new List<string>();
                if (Urgent) parts.Add("急");
                if (Recommended) parts.Add("荐");
                if (Whole) parts.Add("整");
                if (Oco) parts.Add("OCO");
                if (Packing) parts.Add("打包");
                return string.Join(" ", parts);
            }
        }

        /// <summary>待确认行是否有缺失项（提交前校验提示）。</summary>
        public bool HasValidationIssues
        {
            get { return ValidationIssues != null && ValidationIssues.Count > 0; }
        }

        #endregion
    }
}
