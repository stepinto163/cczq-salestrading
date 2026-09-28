using STC.Broker.Plugin.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Broker.Plugin.Convertor
{

    [ValueConversion(typeof(string), typeof(string))]
    public class HisRecordTextConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            string data = value.ToString() + "";
            int index = data.IndexOf(HisRecordPo.Flag);
            if (index > 0)
                return data.Substring(0, index);
            else
                return "";

        }

        public object ConvertBack(object value, Type typeTarget, object param, CultureInfo culture)
        {
            return "";
        }
    }

}
