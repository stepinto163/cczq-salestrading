using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    /// <summary>
    /// 表格行背景色转换类
    /// </summary>
    public class AltBackgroundConverter : IValueConverter
    {
        Brush odd_ColorBrush;
        Brush even_ColorBrush;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AltBackgroundConverter()
        {
            var converter = new System.Windows.Media.BrushConverter();

            odd_ColorBrush = (Brush)converter.ConvertFromString("#0F0F0F");
            even_ColorBrush = (Brush)converter.ConvertFromString("#141414");
        }

        /// <summary>
        /// 隔行背景色转换逻辑
        /// </summary>
        /// <param name="value">当前行数据</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">区域性的信息</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            if (index % 2 == 0)
            {
                return odd_ColorBrush;
            }
            else
            {
                return even_ColorBrush;
            }
        }

        /// <summary>
        /// ignore
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
