using CommunityToolkit.Mvvm.ComponentModel;
using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using PriceTypeEnum = STC.SalesTrading.Plugin.Abstractions.Models.PriceType;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 报价确认行 VM：包装 QuoteRow DTO，承载行内编辑状态（原型 18 列）。
    /// 可编辑列：方向/报价/价格类型/报量/机构/交易员/交易日期/清算速度 + 5 个旗标；
    /// 其余列（期限/代码/简称/估值/偏离/文本内容）只读直通 DTO。
    /// </summary>
    public partial class QuoteConfirmRowViewModel : ObservableObject
    {
        public QuoteConfirmRowViewModel(QuoteRow row)
        {
            Row = row ?? new QuoteRow();

            // 未识别方向在预览表里显示为空串；DTO 默认值按后端约定落在 Bid。
            _direction = Row.Side == QuoteDirection.Ofr ? "Ofr"
                : Row.Side == QuoteDirection.Bid && Row.QuoteId != null ? "Bid"
                : string.Empty;
            _price = Row.Price?.ToString("0.####") ?? string.Empty;
            _priceType = PriceTypeDisplay(Row.QuoteType);
            _volume = Row.Quantity?.ToString("0.#") ?? string.Empty;
            _institution = Row.InstitutionName ?? string.Empty;
            _trader = Row.ContactName ?? string.Empty;
            _tradeDate = Row.TradeDate?.ToString("MM-dd") ?? string.Empty;
            _settlement = SettlementDisplay(Row.SettlType);
            _isOco = Row.Oco;
            _isPacking = Row.Packing;
            _isUrgent = Row.Urgent;
            _isRec = Row.Recommended;
            _isWhole = Row.Whole;
        }

        /// <summary>原始 DTO（提交时回写）。</summary>
        public QuoteRow Row { get; }

        /// <summary>报价 Id。识别来源行在提交前用它关联候选。</summary>
        public string QuoteId { get { return Row.QuoteId; } }

        /// <summary>
        /// CRM 机构与联系人是否都已选定。后端 confirm 只认 ID——只有名称没有 ID 时
        /// 会被 UNRESOLVED_COUNTERPARTY 拒绝，故提交前先据此拦截并提示。
        /// </summary>
        public bool HasCounterpartyId
        {
            // 只有名称、没有 Id 才算未选定；两者都空（后端也允许无对手的报价）不算。
            get { return (string.IsNullOrWhiteSpace(Row.InstitutionName) || !string.IsNullOrWhiteSpace(Row.InstitutionId))
                && (string.IsNullOrWhiteSpace(Row.ContactName) || !string.IsNullOrWhiteSpace(Row.ContactId)); }
        }

        #region 只读显示列

        public string Maturity { get { return Row.Maturity ?? string.Empty; } }
        public string Code { get { return Row.SecurityId ?? string.Empty; } }
        public string Name { get { return Row.SecurityName ?? string.Empty; } }
        public string NzYtm { get { return Format(Row.NzYtm, "0.0000"); } }
        public string NzNetPrice { get { return Format(Row.NzNetPrice, "0.0000"); } }
        public string NzDeviation { get { return Format(Row.NzDeviation, "0.00"); } }
        public string CsYtm { get { return Format(Row.CsYtm, "0.0000"); } }
        public string Remark { get { return Row.Remark ?? string.Empty; } }
        public string SourceType { get { return Row.SourceTypeText; } }

        private static string Format(decimal? value, string fmt)
        {
            return value?.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture) ?? "—";
        }

        #endregion

        #region 可编辑列

        [ObservableProperty]
        private bool _isSelected;

        /// <summary>行类型（精确报价页首列：新增/更新/撤销）。</summary>
        [ObservableProperty]
        private string _rowTypeLabel = "新增";

        /// <summary>方向（"" 未识别 / Bid / Ofr）。</summary>
        [ObservableProperty]
        private string _direction;

        /// <summary>报价（文本编辑，提交时解析）。</summary>
        [ObservableProperty]
        private string _price;

        /// <summary>价格类型（展示文案）。</summary>
        [ObservableProperty]
        private string _priceType;

        /// <summary>报量，单位万（文本编辑）。</summary>
        [ObservableProperty]
        private string _volume;

        /// <summary>报价机构。</summary>
        [ObservableProperty]
        private string _institution;

        /// <summary>报价交易员。</summary>
        [ObservableProperty]
        private string _trader;

        /// <summary>交易日期（MM-dd 文本编辑，原型为 date 输入）。</summary>
        [ObservableProperty]
        private string _tradeDate;

        /// <summary>清算速度（展示文案）。</summary>
        [ObservableProperty]
        private string _settlement;

        [ObservableProperty]
        private bool _isOco;

        [ObservableProperty]
        private bool _isPacking;

        [ObservableProperty]
        private bool _isUrgent;

        [ObservableProperty]
        private bool _isRec;

        [ObservableProperty]
        private bool _isWhole;

        #endregion

        #region 提交回写

        /// <summary>把编辑状态回写到 DTO。</summary>
        public void Commit()
        {
            Row.Side = Direction == "Ofr" ? QuoteDirection.Ofr : QuoteDirection.Bid;

            decimal parsed;
            if (decimal.TryParse(Price, out parsed)) Row.Price = parsed;

            Row.QuoteType = ParsePriceType(PriceType);
            if (decimal.TryParse(Volume, out parsed)) Row.Quantity = parsed;

            Row.InstitutionName = string.IsNullOrWhiteSpace(Institution) ? null : Institution.Trim();
            Row.ContactName = string.IsNullOrWhiteSpace(Trader) ? null : Trader.Trim();

            DateTime date;
            if (DateTime.TryParse(TradeDate, out date)) Row.TradeDate = date;

            Row.SettlType = ParseSettlement(Settlement);

            Row.Oco = IsOco;
            Row.Packing = IsPacking;
            Row.Urgent = IsUrgent;
            Row.Recommended = IsRec;
            Row.Whole = IsWhole;
        }

        #endregion

        #region 枚举 ↔ 展示文案

        public static string[] DirectionOptions { get { return new[] { string.Empty, "Bid", "Ofr" }; } }

        public static string[] PriceTypeOptions
        {
            get { return new[] { string.Empty, "到期收益率", "行权收益率", "净价", "全价" }; }
        }

        public static string[] SettlementOptions
        {
            get { return new[] { string.Empty, "T+0", "T+1", "T+2", "T+3", "远期" }; }
        }

        public static string PriceTypeDisplay(PriceTypeEnum value)
        {
            switch (value)
            {
                case PriceTypeEnum.YieldToMaturity: return "到期收益率";
                case PriceTypeEnum.YieldToExercise: return "行权收益率";
                case PriceTypeEnum.CleanPrice: return "净价";
                case PriceTypeEnum.DirtyPrice: return "全价";
                default: return string.Empty;
            }
        }

        public static PriceTypeEnum ParsePriceType(string display)
        {
            switch (display)
            {
                case "到期收益率": return PriceTypeEnum.YieldToMaturity;
                case "行权收益率": return PriceTypeEnum.YieldToExercise;
                case "净价": return PriceTypeEnum.CleanPrice;
                case "全价": return PriceTypeEnum.DirtyPrice;
                default: return PriceTypeEnum.YieldToMaturity;
            }
        }

        public static string SettlementDisplay(SettlementType value)
        {
            switch (value)
            {
                case SettlementType.T0: return "T+0";
                case SettlementType.T1: return "T+1";
                case SettlementType.T2: return "T+2";
                case SettlementType.T3: return "T+3";
                case SettlementType.Forward: return "远期";
                default: return string.Empty;
            }
        }

        public static SettlementType ParseSettlement(string display)
        {
            switch (display)
            {
                case "T+0": return SettlementType.T0;
                case "T+1": return SettlementType.T1;
                case "T+2": return SettlementType.T2;
                case "T+3": return SettlementType.T3;
                case "远期": return SettlementType.Forward;
                default: return SettlementType.T1;
            }
        }

        #endregion
    }
}
