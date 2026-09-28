using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Mmshe.Plugin.Utils;

namespace STC.Mmshe.Plugin.Convertor
{
    public class MMStatusToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            switch (value.ToString())
            {
                //case "已达标":
                //    return MMConstants.MMStatus_UpStandardToolTip;
                //case "可达标":
                //    return MMConstants.MMStatus_CanStandardToolTip;
                //case "危险":
                //    return MMConstants.MMStatus_DangerToolTip;
                //case "失败":
                //    return MMConstants.MMStatus_FailureToolTip;
                //case "暂停":
                //    return MMConstants.MMStatus_PauseToolTip;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
