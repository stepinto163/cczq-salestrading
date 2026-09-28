using System;
using System.Collections.Generic;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 渠道颜色转换器
    /// </summary>
    public class PartyNameConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null)
            {
                string venues = value.ToString().Trim();
                switch (venues)
                {
                    default:
                        if (!string.IsNullOrEmpty(venues))
                        {
                            if (venues.Length > 4)
                            {
                                return venues.ToString().Substring(0, 3) + "...";
                            }
                            else
                            {
                                return venues.ToString();
                            }
                        }
                        else
                            return "";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
