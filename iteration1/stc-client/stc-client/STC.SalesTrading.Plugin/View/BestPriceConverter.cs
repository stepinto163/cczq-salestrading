using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.View
{
    internal sealed class BestPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JObject book = value as JObject;
            JArray entries = book?["marketData"]?["NoMDEntries"] as JArray;
            JObject entry = entries?.OfType<JObject>().FirstOrDefault(row => (string)row["MDEntryType"] == (string)parameter);
            return entry == null ? "—" : (string)entry["MDEntryPx"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
