using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Shared.Converters
{
    /// <summary>
    /// Venues 渠道码 → 来源徽章底色（0918 原型 inst-badge 色板；X-Bond/双边取实机 VenuesColorConvertor 青）。
    /// 用法：Background="{Binding VenueBid, Converter={StaticResource VenueBadge}}"。
    /// </summary>
    public class VenueBadgeBrushConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "TPSC", "#1E40AF" },    // 国利
            { "CNEX", "#EF4444" },    // 国际
            { "PTCN", "#F97316" },    // 平安
            { "CCTB", "#22C55E" },    // 中诚
            { "UEDA", "#06B6D4" },    // 上田
            { "CTTJ", "#9333EA" },    // 信唐
            { "ODM", "#229B88" },     // X-Bond
            { "QDMESP", "#229B88" },  // 双边
            { "IDB", "#853629" },     // 内部单
            { "QUANT", "#853629" },
            { "MANUAL", "#853629" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var code = value as string;
            if (string.IsNullOrEmpty(code)) return Brushes.Transparent;
            string hex;
            if (!Palette.TryGetValue(code, out hex)) hex = "#484F58";
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
