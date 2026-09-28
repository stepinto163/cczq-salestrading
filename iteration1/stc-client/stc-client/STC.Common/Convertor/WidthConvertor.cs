using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 卡片模式item宽度转换器
    /// </summary>
    public class WidthConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if (parameter == null) return 1;
            try
            {
                double parentWidth = Math.Floor(double.Parse(value.ToString()));
                double width = 300 + 6; // item实际大小 + margin
                double scrollarWidth = 20;//滚动条宽度

                int perferCols = (int)((parentWidth - scrollarWidth) / width); 

                //int cols = int.Parse(parameter.ToString());
                do
                {

                   width  = double.Parse(((parentWidth - scrollarWidth) / perferCols).ToString());
                    perferCols--;
                }

                while (width < (300 + 6) && perferCols > 1);

                return width - 8;
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
