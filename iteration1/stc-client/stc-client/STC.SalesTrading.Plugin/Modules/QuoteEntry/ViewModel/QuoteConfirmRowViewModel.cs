using CommunityToolkit.Mvvm.ComponentModel;
using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using PriceTypeEnum = STC.SalesTrading.Plugin.Abstractions.Models.PriceType;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 报价确认行 VM：包装 QuoteConfirmRow DTO，承载行内编辑状态（原型 18 列）。
    /// 可编辑列：方向/报价/价格类型/报量/机构/交易员/交易日期/清算速度 + 5 个旗标；
    /// 其余列（期限/代码/简称/估值/偏离/文本内容）只读直通 DTO。
    /// </summary>
    public partial class QuoteConfirmRowViewModel : ObservableObject
    {
        public QuoteConfirmRowViewModel(QuoteConfirmRow row)
        {
            Row = row ?? new QuoteConfirmRow();

            _direction = Row.Direction == QuoteDirection.Bid ? "Bid" : Row.Direction == QuoteDirection.Ofr ? "Ofr" : string.Empty;
            _price = Row.Price?.ToString("0.####") ?? string.Empty;
            _priceType = PriceTypeDisplay(Row.PriceType);
            _volume = Row.Volume?.ToString("0.#") ?? string.Empty;
            _institution = Row.Institution ?? string.Empty;
            _trader = Row.Trader ?? string.Empty;
            _tradeDate = Row.TradeDate?.ToString("MM-dd") ?? string.Empty;
            _settlement = SettlementDisplay(Row.Settlement);
            _isOco = Row.Flags.HasFlag(QuoteFlag.Oco);
            _isPacking = Row.Flags.HasFlag(QuoteFlag.Packing);
            _isUrgent = Row.Flags.HasFlag(QuoteFlag.Urgent);
            _isRec = Row.Flags.HasFlag(QuoteFlag.Recommend);
            _isWhole = Row.Flags.HasFlag(QuoteFlag.Whole);
        }

        /// <summary>原始 DTO（提交时回写）。</summary>
        public QuoteConfirmRow Row { get; }

        /// <summary>识别主键（recognitionId）。</summary>
        public string RecognitionId { get { return Row.RecognitionId; } }

        #region 只读显示列

        public string Maturity { get { return Row.Maturity ?? string.Empty; } }
        public string Code { get { return Row.Code ?? string.Empty; } }
        public string Name { get { return Row.Name ?? string.Empty; } }
        public string NzValuation { get { return Format(Row.NzValuation, "0.0000"); } }
        public string NzNetPrice { get { return Format(Row.NzNetPrice, "0.0000"); } }
        public string NzDeviation { get { return Format(Row.NzDeviation, "0.00"); } }
        public string CsPriceBasis { get { return Row.CsPriceBasis ?? "—"; } }
        public string TextContent { get { return Row.TextContent ?? string.Empty; } }
        public string SourceType { get { return Row.SourceType ?? string.Empty; } }

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
            Row.Direction = _direction == "Ofr" ? QuoteDirection.Ofr
                : _direction == "Bid" ? QuoteDirection.Bid
                : Row.Direction;

            decimal parsed;
            if (decimal.TryParse(_price, out parsed)) Row.Price = parsed;

            Row.PriceType = ParsePriceType(_priceType);
            if (decimal.TryParse(_volume, out parsed)) Row.Volume = parsed;

            if (Row.Institution != _institution)
            {
                Row.InstitutionId = null;
            }
            if (Row.Trader != _trader)
            {
                Row.ContactId = null;
            }
            Row.Institution = _institution;
            Row.Trader = _trader;

            DateTime date;
            if (DateTime.TryParse(_tradeDate, out date)) Row.TradeDate = date;

            Row.Settlement = ParseSettlement(_settlement);

            Row.Flags = QuoteFlag.None
                | (_isOco ? QuoteFlag.Oco : QuoteFlag.None)
                | (_isPacking ? QuoteFlag.Packing : QuoteFlag.None)
                | (_isUrgent ? QuoteFlag.Urgent : QuoteFlag.None)
                | (_isRec ? QuoteFlag.Recommend : QuoteFlag.None)
                | (_isWhole ? QuoteFlag.Whole : QuoteFlag.None);
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
