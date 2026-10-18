using System;
using System.Globalization;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Quote;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;
using QT = STC.SalesTrading.Plugin.Gateway.Contracts.Quote;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 协议 DTO → UI 行模型映射。全站编码转换的唯一入口（方案 §2.4 五个陷阱）：
    /// side "1"/"2"=Bid/Ofr；quoteType "1"/"2"/"3"=收益率/净价/全价（与 STC PriceType 编码相反）；
    /// yieldType "1"/"2"=到期/行权（与 STC ExerciseFlag 编码相反）；settlType "1".."4"=T+0..T+3；
    /// revision 十进制字符串（不可按字典序比较）。
    /// </summary>
    internal static class GatewayModelMapper
    {
        /// <summary>后端统一报价 → UI 报价行。reference 可空（缺失字段由服务层 EnsureReferenceAsync 过渡补取）。</summary>
        public static QuoteRow ToQuoteRow(QT.QuoteRecord record, QuoteReference reference)
        {
            if (record == null) return null;
            var row = new QuoteRow
            {
                QuoteId = record.QuoteId,
                Revision = record.Revision,
                Status = ParseStatus(record.Status),
                ClientQuoteId = record.ClientQuoteId,
                SourceMessageId = record.SourceMessageId,
                SourceType = ParseSourceType(record.Source),
                SecurityId = record.SecurityId,
                InstitutionId = record.InstitutionId,
                InstitutionName = record.InstitutionName,
                ContactId = record.ContactId,
                ContactName = record.ContactName,
                OwnerUserId = record.OwnerUserId,
                Side = ParseSide(record.Side),
                QuoteType = ParseQuoteType(record.QuoteType),
                YieldType = ParseYieldType(record.YieldType),
                Price = ParseDecimal(record.Price),
                Quantity = ParseDecimal(record.Quantity),
                TradeDate = ParseDate(record.TradeDate),
                SettlType = ParseSettlType(record.SettlType),
                Urgent = record.Urgent,
                Recommended = record.Recommended,
                Whole = record.Whole,
                Oco = record.Oco,
                Packing = record.Packing,
                Remark = record.Remark,
                CreateTime = record.CreateTime,
                UpdateTime = record.UpdateTime,
                QuoteTime = record.QuoteTime,
                ConfirmTime = record.ConfirmTime,
            };

            if (reference != null)
            {
                row.SecurityName = reference.SecurityName;
                row.Maturity = reference.RemainingYears;
                row.BondLibrary = reference.BondSubtype;
                row.IssuerRating = reference.IssuerRating;
                row.BondRating = reference.BondRating;
                row.NzYtm = ParseDecimal(reference.ValuationYield);
                row.NzNetPrice = ParseDecimal(reference.ValuationCleanPrice);
                row.CsYtm = ParseDecimal(reference.CsiValuationYield);
                row.CsNetPrice = ParseDecimal(reference.CsiValuationCleanPrice);
            }
            return row;
        }

        #region 编码转换（陷阱集中处，勿在其他地方散写）

        /// <summary>后端 side："1"=Bid "2"=Ofr（STC MDEntryType 是 "0"=Bid，勿混用）。</summary>
        public static QuoteDirection ParseSide(string code)
        {
            return code == "2" ? QuoteDirection.Ofr : QuoteDirection.Bid;
        }

        /// <summary>后端 quoteType："1"=收益率 "2"=净价 "3"=全价（STC PriceType "3"=收益率，编码相反）。</summary>
        public static PriceType ParseQuoteType(string code)
        {
            switch (code)
            {
                case "2": return PriceType.CleanPrice;
                case "3": return PriceType.DirtyPrice;
                default: return PriceType.YieldToMaturity;
            }
        }

        /// <summary>后端 yieldType："1"=到期 "2"=行权（STC ExerciseFlag "0"=行权，编码相反）；空为 null。</summary>
        public static YieldType? ParseYieldType(string code)
        {
            if (code == "2") return YieldType.ToExercise;
            if (code == "1") return YieldType.ToMaturity;
            return null;
        }

        /// <summary>后端 settlType："1"/"2"/"3"/"4" = T+0/T+1/T+2/T+3（已确认口径，见方案 §2.4）。</summary>
        public static SettlementType ParseSettlType(string code)
        {
            switch (code)
            {
                case "1": return SettlementType.T0;
                case "2": return SettlementType.T1;
                case "3": return SettlementType.T2;
                case "4": return SettlementType.T3;
                default: return SettlementType.Forward;
            }
        }

        public static QuoteStatus ParseStatus(string code)
        {
            switch (code)
            {
                case "ACTIVE": return QuoteStatus.Active;
                case "REFERRED": return QuoteStatus.Referred;
                case "DELETED": return QuoteStatus.Deleted;
                default: return QuoteStatus.PendingConfirm;
            }
        }

        public static SourceType ParseSourceType(QT.QuoteSource source)
        {
            if (source == null) return SourceType.Manual;
            switch (source.Type)
            {
                case "QT": return SourceType.Qt;
                case "IDEAL": return SourceType.Ideal;
                default: return SourceType.Manual;
            }
        }

        public static string StatusToCode(QuoteStatus status)
        {
            switch (status)
            {
                case QuoteStatus.Active: return "ACTIVE";
                case QuoteStatus.Referred: return "REFERRED";
                case QuoteStatus.Deleted: return "DELETED";
                default: return "PENDING_CONFIRM";
            }
        }

        /// <summary>枚举 → 后端 quoteType 编码（提交方向）。</summary>
        public static string QuoteTypeToCode(PriceType type)
        {
            switch (type)
            {
                case PriceType.CleanPrice: return "2";
                case PriceType.DirtyPrice: return "3";
                default: return "1";
            }
        }

        public static string YieldTypeToCode(YieldType? type)
        {
            return type == YieldType.ToExercise ? "2" : "1";
        }

        public static string SettlTypeToCode(SettlementType type)
        {
            switch (type)
            {
                case SettlementType.T0: return "1";
                case SettlementType.T1: return "2";
                case SettlementType.T2: return "3";
                case SettlementType.T3: return "4";
                default: return "2";
            }
        }

        public static string SideToCode(QuoteDirection side)
        {
            return side == QuoteDirection.Ofr ? "2" : "1";
        }

        /// <summary>十进制字符串 → decimal?（协议传输统一字符串，空/坏值返回 null 而非抛异常）。</summary>
        public static decimal? ParseDecimal(string text)
        {
            decimal value;
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                ? value : (decimal?)null;
        }

        /// <summary>yyyy-MM-dd → DateTime?。</summary>
        public static DateTime? ParseDate(string text)
        {
            DateTime value;
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
                ? value : (DateTime?)null;
        }

        #endregion
    }
}
