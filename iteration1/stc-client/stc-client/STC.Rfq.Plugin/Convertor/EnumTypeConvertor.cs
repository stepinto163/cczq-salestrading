using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Rfq.Plugin.Convertor
{
    /// <summary>
    /// 枚举与对应值转换类
    /// </summary>
    public class EnumTypeConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value != null)
            {
                string key = value.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();

                if (param.Equals("settletype_myOrder"))
                {
                    return ComboSourceUtil.Instance.FindEnum(key.Trim(), param);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string item in keyArr)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(CommonConstants.COMMA_CHAR);
                        }
                        sb.Append(ComboSourceUtil.Instance.FindEnum(item.Trim(), param));
                    }
                    return sb.ToString();
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
