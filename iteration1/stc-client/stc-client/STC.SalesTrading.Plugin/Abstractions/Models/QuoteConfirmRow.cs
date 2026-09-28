using System;

namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// 待确认报价行（原型 quote-confirm-table，18 列）。纯 DTO，不实现 INPC；
    /// 行内编辑所需的 UI 状态包装在模块 ViewModel 层。
    /// 字段名对齐原型 canonical 命名（见 docs/PROTOTYPE_ANALYSIS.md §4.4）。
    /// </summary>
    public class QuoteConfirmRow
    {
        public long Revision { get; set; }
        public string InstitutionId { get; set; }
        public string ContactId { get; set; }
        public string SettlementCode { get; set; }
        public string ClientQuoteId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>识别结果主键（recognitionId）。</summary>
        public string RecognitionId { get; set; }

        /// <summary>所属批次 Id。</summary>
        public string BatchId { get; set; }

        /// <summary>批次状态（原型 batchStatus：待确认/部分识别/识别失败/部分确认）。</summary>
        public BatchStatus BatchStatus { get; set; }

        /// <summary>到报时间（批次头展示）。</summary>
        public DateTime? ReceivedAt { get; set; }

        /// <summary>批次机构名（batchCustomerName，分组头展示；缺省回退行机构）。</summary>
        public string BatchInstitution { get; set; }

        /// <summary>批次交易员名（batchCustomerTraderName，缺省回退行交易员）。</summary>
        public string BatchTrader { get; set; }

        /// <summary>来源渠道：IDEAL/QT（sourceType）。</summary>
        public string SourceType { get; set; }

        /// <summary>剩余期限（m），如 "7.73Y"。</summary>
        public string Maturity { get; set; }

        /// <summary>债券代码（c）。</summary>
        public string Code { get; set; }

        /// <summary>债券简称（n）。</summary>
        public string Name { get; set; }

        /// <summary>方向（d）。</summary>
        public QuoteDirection Direction { get; set; }

        /// <summary>报价（bp/op）。</summary>
        public decimal? Price { get; set; }

        /// <summary>价格类型（pt）。</summary>
        public PriceType PriceType { get; set; }

        /// <summary>报量，单位万（bv）。</summary>
        public decimal? Volume { get; set; }

        /// <summary>报价机构（inst）。</summary>
        public string Institution { get; set; }

        /// <summary>报价交易员（tr）。</summary>
        public string Trader { get; set; }

        /// <summary>交易日期。</summary>
        public DateTime? TradeDate { get; set; }

        /// <summary>清算速度（cs）。</summary>
        public SettlementType Settlement { get; set; }

        /// <summary>中债估值。</summary>
        public decimal? NzValuation { get; set; }

        /// <summary>中债净价（nzNet）。</summary>
        public decimal? NzNetPrice { get; set; }

        /// <summary>中债偏离（nzDev，BP 口径/净价口径价差）。</summary>
        public decimal? NzDeviation { get; set; }

        /// <summary>中证估值口径（行权/到期）。</summary>
        public string CsPriceBasis { get; set; }

        /// <summary>原始文本内容（tooltip 展示）。</summary>
        public string TextContent { get; set; }

        /// <summary>旗标组合（急/荐/整/OCO/打包）。</summary>
        public QuoteFlag Flags { get; set; }
    }
}
