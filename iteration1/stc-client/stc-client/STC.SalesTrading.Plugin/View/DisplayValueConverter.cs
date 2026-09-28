using System;
using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.View
{
    internal sealed class DisplayValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString() ?? "";
            string field = (string)parameter;
            if (field == "status")
            {
                switch (text)
                {
                    case "PENDING_CONFIRM": return "待确认";
                    case "ACTIVE": return "有效";
                    case "REFERRED": return "已撤销";
                    case "DELETED": return "已删除";
                }
            }
            if (field == "side")
            {
                return text == "1" ? "Bid 买入" : text == "2" ? "Ofr 卖出" : text;
            }
            if (field == "SettlType" || field == "settlType")
            {
                switch (text)
                {
                    case "1": return "T+0";
                    case "2": return "T+1";
                    case "3": return "T+2";
                    case "4": return "T+3";
                    case "5": return "T+4";
                    case "9": return "T+5";
                    case "10": return "T+6";
                    case "11": return "T+7";
                    case "12": return "T+8";
                    case "13": return "T+9";
                    case "14": return "T+10";
                    case "15": return "T+11";
                    case "16": return "T+12";
                }
            }
            if (field == "quoteType")
            {
                return text == "1" ? "收益率" : text == "2" ? "净价" : text == "3" ? "全价" : text;
            }
            if (field.EndsWith("Time", StringComparison.Ordinal) && text.Length > 0
                && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset timestamp))
            {
                return timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
