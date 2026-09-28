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

namespace STC.Mm.Plugin.Convertor
{
    /// <summary>
    /// 渠道颜色转换器
    /// </summary>
    public class VenuesSimpleConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null)
            {
                string venues = value.ToString().ToUpper();
                switch (venues)
                {
                    case "TPSC":  //国利
                        return "国利";
                    case "CTTJ":  //信唐
                        return "信唐";
                    case "CNEX":  //国际
                        return "国际";
                    case "CCTB":  //中诚
                        return "中诚";
                    case "PTCN":  //平安
                        return "平安";
                    case "UEDA":  //上田
                        return "上田";
                    case "ODM":  //X-Bond
                        return "XB";
                    case "IDB":
                    case "QUANT":
                    case "MANUAL":
                        return "本方";
                    case "QDMESP":
                        return "双边";
                    case "NDM":
                        return "请";
                    case "QDM":
                        return "对";
                    case "XSAWP":
                        return "X";
                    case "CTP":
                        return "证券";
                    case "SSE":
                        return "上新债";
                    case "SZE":
                        return "深固收";
                    case "SSF":
                        return "上固收";
                    case "QDM2":
                        return "指示";
                    case "BBO":
                        return "经纪";
                    default:
                        if (!string.IsNullOrEmpty(venues))
                        {
                            return value.ToString().Substring(0, 1);
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
