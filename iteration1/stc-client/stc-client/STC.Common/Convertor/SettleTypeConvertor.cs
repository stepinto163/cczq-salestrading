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
    /// <summary>
    /// 清算速度显示转换类
    /// </summary>
    public class SettleTypeConvertor : IMultiValueConverter
    {
        public const string MARKETINDICATOR_FUTURE = "5";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                object marketindicator = values[0];
                object settletype = values[1];
                
                if (settletype == null)
                {
                    return "--";
                }
                string key = settletype.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();
                //国债期货/交易所债券 清算速度不显示
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator) || (values.Length > 2 && !CommonUtils.IsBrokerSecurity(values[2].ToString())))
                {
                    return "--";
                }
                if (param.Equals("settletype_myOrder"))
                {
                    return ComboSourceUtil.Instance.FindEnum(key.Trim(), param);
                }
                else if (param.Equals("settletype") || param.Equals("settletype_tooltip"))//清算速度的ToolTip提示
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

                return ComboSourceUtil.Instance.FindEnum(settletype.ToString().Trim(), "settletype_myOrder");
            }
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
