using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mm.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mm.Plugin.Convertor
{
    public class MMBasePriorityValueConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || parameter == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(values[0].ToString()) || string.IsNullOrEmpty(values[1].ToString()) || string.IsNullOrEmpty(values[2].ToString()))
            {
                return string.Empty;
            }

            //价格优先级：1防守，2算法
            string basePriority = values[0].ToString();                
            string bidDefenCalPrice = values[1].ToString();
            string bidAlgoCalPrice = values[2].ToString();

            string text = string.Empty;

            string[] splitArr = basePriority.ToString().Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            switch (parameter.ToString())
            {
                case "1":
                    if (!string.IsNullOrEmpty(splitArr[0].ToString()))
                    {
                        if (splitArr[0].ToString() == "1")
                        {
                            text = bidDefenCalPrice;
                        }
                        else
                        {
                            text = bidAlgoCalPrice;
                        }
                    }
                    else
                    {
                        text = bidDefenCalPrice;
                    }
                    break;

                case "2":
                    if (!string.IsNullOrEmpty(splitArr[1].ToString()))
                    {
                        if (splitArr[1].ToString() == "1")
                        {
                            text = bidDefenCalPrice;
                        }
                        else
                        {
                            text = bidAlgoCalPrice;
                        }
                    }
                    else
                    {
                        text = bidAlgoCalPrice;
                    }
                    break;
            }

            return text;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
