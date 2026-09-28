using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Mmshe.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Convertor
{
    public class MMBasePriorityTextConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return string.Empty;
            }

            string text = string.Empty;

            string[] splitArr = value.ToString().Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            switch (parameter.ToString())
            {
                case "1":
                    
                    if (!string.IsNullOrEmpty(splitArr[0].ToString()))
                    {
                        if (splitArr[0].ToString() == "1")
                        {
                            text = "防守报价";
                        }
                        else
                        {
                            text = "算法报价";
                        }
                    }
                    else
                    {
                        text = "防守报价";
                    }
                    break;
                case "2":
                    if (!string.IsNullOrEmpty(splitArr[1].ToString()))
                    {
                        if (splitArr[1].ToString() == "1")
                        {
                            text = "防守报价";
                        }
                        else
                        {
                            text = "算法报价";
                        }
                    }
                    else
                    {
                        text = "算法报价";
                    }
                    break;
            }


            return text;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return string.Empty;
            }

            string text = string.Empty;

            switch (parameter.ToString())
            {
                case "1":

                    if (value.ToString()== "防守报价")
                    {
                        text = "1,2";
                    }
                    else
                    {
                        text = "2,1";
                    }

                    break;

                case "2":
                    if (value.ToString() == "防守报价")
                    {
                        text = "2,1";
                    }
                    else
                    {
                        text = "1,2";
                    }
                    break;
            }
            return text;
        }
    }
}
