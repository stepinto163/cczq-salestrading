using STC.Common.Utils;
using STC.quant.plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
namespace STC.quant.plugin.Convertor
{
    class UpdateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                DateTime time = CommonUtils.ConvertToDateTime(value.ToString());
                return time.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
