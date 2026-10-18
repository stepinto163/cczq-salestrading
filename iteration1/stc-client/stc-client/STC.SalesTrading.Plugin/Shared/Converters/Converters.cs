using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Shared.Converters
{
    /// <summary>转换器画刷解析：优先取主题资源（可换肤），缺失时用原型色值兜底（Freeze 后只读共享）。
    /// 结果按 key 缓存——转换器是按单元格求值的，22 列宽表滚动时原先每个单元格都查一次资源树。</summary>
    internal static class ConverterBrushes
    {
        private static readonly System.Collections.Generic.Dictionary<string, SolidColorBrush> Cache =
            new System.Collections.Generic.Dictionary<string, SolidColorBrush>(StringComparer.Ordinal);

        private static readonly object Gate = new object();

        internal static SolidColorBrush Resolve(string key, string fallbackHex)
        {
            var cacheKey = (key ?? string.Empty) + "|" + fallbackHex;
            lock (Gate)
            {
                SolidColorBrush cached;
                if (Cache.TryGetValue(cacheKey, out cached)) return cached;
            }

            // key 为空 = 调用方只想要兜底色，不能进 TryFindResource
            // ——Application 资源表对 null 键会抛 ArgumentNullException（键不能为 null）
            var brush = string.IsNullOrEmpty(key) ? null : Application.Current?.TryFindResource(key) as Brush;
            var result = brush as SolidColorBrush;
            if (result == null)
            {
                var color = (Color)ColorConverter.ConvertFromString(fallbackHex);
                result = new SolidColorBrush(color);
                result.Freeze();
            }

            lock (Gate)
            {
                Cache[cacheKey] = result;
            }
            return result;
        }

        /// <summary>换肤后清空缓存（ThemeManager 应用主题时调用）。</summary>
        internal static void Invalidate()
        {
            lock (Gate)
            {
                Cache.Clear();
            }
        }
    }

    /// <summary>bool → Visibility。默认 true=Visible；ConverterParameter="invert" 时反转。</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;
            if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase)) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>null → Collapsed，否则 Visible。</summary>
    public class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>null/空白 → "—"（表格空值占位，原型惯例）。</summary>
    public class EmptyToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "—";
            var text = value.ToString();
            return string.IsNullOrWhiteSpace(text) ? "—" : text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>int &lt;= 0 → Collapsed（角标/计数徽标）。</summary>
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value is int i ? i : 0;
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>报价方向 → 画刷：Bid 琥珀橙 / Ofr 蓝（全站统一语义）。</summary>
    public class DirectionBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuoteDirection direction)
                return direction == QuoteDirection.Bid
                    ? ConverterBrushes.Resolve("BidBrush", "#F59E0B")
                    : ConverterBrushes.Resolve("OfrBrush", "#3B82F6");

            // 兼容字符串值（后续解析器/枚举扩展可能传字符串）
            if (string.Equals(value as string, "Bid", StringComparison.OrdinalIgnoreCase))
                return ConverterBrushes.Resolve("BidBrush", "#F59E0B");
            if (string.Equals(value as string, "Ofr", StringComparison.OrdinalIgnoreCase))
                return ConverterBrushes.Resolve("OfrBrush", "#3B82F6");
            return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>涨跌 → 画刷（中国习惯，与 A 股相反）：涨/最高红 #ef4444，跌/最低绿 #3fb950。</summary>
    public class UpDownBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isUp = value is bool b && b;
            return isUp
                ? ConverterBrushes.Resolve("UpBrush", "#EF4444")
                : ConverterBrushes.Resolve("DownBrush", "#3FB950");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Toast 级别 → 强调画刷。</summary>
    public class ToastKindBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ToastKind kind)) return ConverterBrushes.Resolve("AccentBrush", "#79C0FF");
            switch (kind)
            {
                case ToastKind.Success: return ConverterBrushes.Resolve("SuccessBrush", "#3FB950");
                case ToastKind.Warning: return ConverterBrushes.Resolve("BidDeepBrush", "#D29922");
                case ToastKind.Error: return ConverterBrushes.Resolve("DangerBrush", "#EF4444");
                default: return ConverterBrushes.Resolve("AccentBrush", "#79C0FF");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>提醒类型 → 画刷：关注债券(绿)/筛选分组(蓝)/对价提醒(金)/我的最优(紫)。</summary>
    public class AlertTypeBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is AlertType type)) return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
            switch (type)
            {
                case AlertType.FavoriteBond: return ConverterBrushes.Resolve("SuccessBrush", "#3FB950");
                case AlertType.FilterGroup: return ConverterBrushes.Resolve("OfrBrush", "#3B82F6");
                case AlertType.PriceMatch: return ConverterBrushes.Resolve("BidDeepBrush", "#D29922");
                case AlertType.MyBestQuote: return ConverterBrushes.Resolve("PurpleDeepBrush", "#A855F7");
                default: return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>成交方向 → 画刷：TRD 绿 / TKN 紫 #a371f7 / GVN 金 #d29922（原型语义）。</summary>
    public class TradeDirectionBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TradeDirection direction)) return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
            switch (direction)
            {
                case TradeDirection.Trd: return ConverterBrushes.Resolve("SuccessBrush", "#3FB950");
                case TradeDirection.Tkn: return ConverterBrushes.Resolve("TknBrush", "#A371F7");
                case TradeDirection.Gvn: return ConverterBrushes.Resolve("BidDeepBrush", "#D29922");
                default: return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>报价状态 → 中文（原型 mgmt 状态列：有效/已撤销，普通文本）。</summary>
    public class QuoteStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as QuoteStatus?)
            {
                case QuoteStatus.Active: return "有效";
                case QuoteStatus.Referred: return "已撤销";
                case QuoteStatus.PendingConfirm: return "待确认";
                case QuoteStatus.Deleted: return "已删除";
                default: return "—";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>报价来源 → 中文（原型「报价方式」列 tag：QT / ideal / 手工录入）。</summary>
    public class SourceTypeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as SourceType?)
            {
                case SourceType.Qt: return "QT";
                case SourceType.Ideal: return "ideal";
                case SourceType.Manual: return "手工录入";
                default: return "—";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>报价状态 → 画刷：撤销灰 / 有效正文色（原型语义）。</summary>
    public class StatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuoteStatus s)
            {
                switch (s)
                {
                    case QuoteStatus.Referred: return ConverterBrushes.Resolve("TextDisabledBrush", "#7D8590");
                    case QuoteStatus.PendingConfirm: return ConverterBrushes.Resolve("WarnBrush", "#F59E0B");
                    case QuoteStatus.Deleted: return ConverterBrushes.Resolve("TextDisabledBrush", "#7D8590");
                }
            }
            return ConverterBrushes.Resolve("TextSecondaryBrush", "#C9D1D9");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>枚举值 == ConverterParameter(同名串) → bool；反向 true → 解析为参数枚举。用于互斥 ToggleButton。</summary>
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null && targetType != null && targetType.IsEnum)
            {
                try { return Enum.Parse(targetType, parameter.ToString(), true); }
                catch (Exception) { return Binding.DoNothing; }
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>字符串值 == ConverterParameter(串) → bool；反向 true → 参数串。用于互斥 ToggleButton。</summary>
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value as string, parameter as string, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null) return parameter.ToString();
            return Binding.DoNothing;
        }
    }

    /// <summary>机构来源名称 → 原型机构徽标颜色。</summary>
    public class InstitutionBadgeBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((value as string) ?? string.Empty)
            {
                case "国利": return ConverterBrushes.Resolve("OfrBrush", "#1E40AF");
                case "国际": return ConverterBrushes.Resolve("DangerBrush", "#EF4444");
                case "平安": return ConverterBrushes.Resolve("BidBrush", "#F97316");
                case "中诚": return ConverterBrushes.Resolve("SuccessBrush", "#22C55E");
                case "上田": return ConverterBrushes.Resolve("VolumeBrush", "#06B6D4");
                case "信唐": return ConverterBrushes.Resolve("PurpleDeepBrush", "#9333EA");
                case "CFETS": return ConverterBrushes.Resolve("BidDeepBrush", "#A16207");
                default: return ConverterBrushes.Resolve("TextDisabledBrush", "#6B7280");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>value.ToString() == parameter.ToString() → Visible，否则 Collapsed。
    /// 视图常驻：多个预实例视图按 Key（字符串/枚举）重叠摆放，仅当前项 Visible。</summary>
    public class KeyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return Visibility.Visible;
            if (value == null || parameter == null) return Visibility.Collapsed;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>int &gt; 1 → Visible（原型最优页「同档报价笔数」按钮：仅多笔时显示）。</summary>
    public class MoreThanOneToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value is int i ? i : 0;
            return count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>收益率/估值 4 位小数格式化（null → "—"）。</summary>
    public class Price4Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "—";
            if (value is decimal d) return d.ToString("0.0000", CultureInfo.InvariantCulture);
            if (decimal.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                return parsed.ToString("0.0000", CultureInfo.InvariantCulture);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 转换器静态实例定位器。
    /// Binding 的 Converter 属性不是依赖属性，不能放 DynamicResource；而模板内的 StaticResource
    /// 作用域不可靠（实测 DataTemplate 中解析不到跨字典资源）。x:Static 引用静态属性在解析期直接
    /// 取值、零资源依赖，是 Binding.Converter 的最稳写法。用法：
    ///   xmlns:conv="clr-namespace:STC.SalesTrading.Plugin.Shared.Converters"
    ///   Converter="{x:Static conv:ConverterLocator.IntToVis}"
    /// </summary>
    public static class ConverterLocator
    {
        public static readonly BoolToVisibilityConverter BoolToVis = new BoolToVisibilityConverter();
        public static readonly NullToCollapsedConverter NullToCollapsed = new NullToCollapsedConverter();
        public static readonly EmptyToDashConverter EmptyToDash = new EmptyToDashConverter();
        public static readonly IntToVisibilityConverter IntToVis = new IntToVisibilityConverter();
        public static readonly MoreThanOneToVisibilityConverter MoreThanOneToVis = new MoreThanOneToVisibilityConverter();
        public static readonly QuoteStatusTextConverter QuoteStatusText = new QuoteStatusTextConverter();
        public static readonly SourceTypeTextConverter SourceTypeText = new SourceTypeTextConverter();
        public static readonly DirectionBrushConverter DirBrush = new DirectionBrushConverter();
        public static readonly UpDownBrushConverter UpDownBrush = new UpDownBrushConverter();
        public static readonly ToastKindBrushConverter ToastKindBrush = new ToastKindBrushConverter();
        public static readonly Price4Converter Price4 = new Price4Converter();
        public static readonly AlertTypeBrushConverter AlertTypeBrush = new AlertTypeBrushConverter();
        public static readonly TradeDirectionBrushConverter TradeDirectionBrush = new TradeDirectionBrushConverter();
        public static readonly StatusBrushConverter StatusBrush = new StatusBrushConverter();
        public static readonly EnumEqualsConverter EnumEquals = new EnumEqualsConverter();
        public static readonly StringEqualsConverter StringEquals = new StringEqualsConverter();
        public static readonly InstitutionBadgeBrushConverter InstitutionBadgeBrush = new InstitutionBadgeBrushConverter();
        public static readonly KeyToVisibilityConverter KeyToVis = new KeyToVisibilityConverter();
    }
}
