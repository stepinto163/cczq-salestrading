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
    public class VenuesColorConvertor : IValueConverter
    {
        Brush TPSC_ColorBrush;
        Brush CTTJ_ColorBrush;
        Brush CNEX_ColorBrush;
        Brush CCTB_ColorBrush;
        Brush PTCN_ColorBrush;
        Brush XBond_ColorBrush;
        Brush QDMESP_ColorBrush;
        Brush RFQ_ColorBrush;
        Brush DEFAULT_Foreground_Colorbrush;
        Brush BEN_ColorBrush;
        Brush UEDA_ColorBrush;
        Brush SSE_ColorBrush;//上新债
        Brush SSF_ColorBrush;//上固收
        Brush SZE_ColorBrush;//深固收
        public VenuesColorConvertor()
        {
            var converter = new System.Windows.Media.BrushConverter();

            TPSC_ColorBrush = (Brush)converter.ConvertFromString("#2D5D96");
            CTTJ_ColorBrush = (Brush)converter.ConvertFromString("#62358A");
            CNEX_ColorBrush = (Brush)converter.ConvertFromString("#B03131");
            CCTB_ColorBrush = (Brush)converter.ConvertFromString("#5B9357");
            PTCN_ColorBrush = (Brush)converter.ConvertFromString("#FB943C");
            XBond_ColorBrush = (Brush)converter.ConvertFromString("#3C1DDB");
            QDMESP_ColorBrush = (Brush)converter.ConvertFromString("#2B3BFF");
            RFQ_ColorBrush = (Brush)converter.ConvertFromString("#00A74F");
            DEFAULT_Foreground_Colorbrush = (Brush)converter.ConvertFromString("#853629");
            BEN_ColorBrush = (Brush)converter.ConvertFromString("#853629");
            UEDA_ColorBrush = (Brush)converter.ConvertFromString("#B59E00");
            SZE_ColorBrush = (Brush)converter.ConvertFromString("#5F4E44");
            SSF_ColorBrush = (Brush)converter.ConvertFromString("#9E584E");
            SSE_ColorBrush = (Brush)converter.ConvertFromString("#72478F");
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value != null)
            {
                string venues = value.ToString();
                switch (venues)
                {
                    case "TPSC":
                        return TPSC_ColorBrush;
                    case "CTTJ":
                        return CTTJ_ColorBrush;
                    case "CNEX":
                        return CNEX_ColorBrush;
                    case "CCTB":
                        return CCTB_ColorBrush;
                    case "PTCN":
                        return PTCN_ColorBrush;
                    case "UEDA":
                        return UEDA_ColorBrush;
                    case "SSF":
                        return SSF_ColorBrush;
                    case "CTP":
                    case "SSE":
                        return SSE_ColorBrush;
                    case "ODM":
                        return XBond_ColorBrush;
                    case "SZE":
                        return SZE_ColorBrush;
                    case "QDM2":
                    case "BBO":
                        return XBond_ColorBrush;
                    case "QDMESP":
                        return QDMESP_ColorBrush;
                    case "QDM":
                    case "HTQDM":
                        return RFQ_ColorBrush;
                    case "IDB":
                    case "QUANT":
                    case "MANUAL":
                        return BEN_ColorBrush;

                }
            }
            return DEFAULT_Foreground_Colorbrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
