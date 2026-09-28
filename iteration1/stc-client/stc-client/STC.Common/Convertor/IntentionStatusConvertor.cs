using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Common.Convertor
{
    public class IntentionStatusConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                string type = values[0]?.ToString();
                string status = values[1]?.ToString();
                if (status == "1")
                {
                    if (type == "2")
                    {
                        return "已发布";//"已firm发布";
                    }
                    else // type可能是1, 也可能是null
                    {
                        return "已发布";//"已NF发布";
                    }
                }
                else
                {
                    return ComboSourceUtil.Instance.FindEnum(status, "intentionstatus");
                }
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
