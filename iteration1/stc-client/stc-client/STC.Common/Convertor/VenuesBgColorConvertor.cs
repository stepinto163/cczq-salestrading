using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace STC.Common.Convertor
{
    public class VenuesBgColorConvertor : IValueConverter
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
        public VenuesBgColorConvertor()
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
            DEFAULT_Foreground_Colorbrush = (Brush)converter.ConvertFromString("#136C5E"); //(Brush)converter.ConvertFromString("#FFFFFF");
            BEN_ColorBrush = (Brush)converter.ConvertFromString("#853629");
            UEDA_ColorBrush = (Brush)converter.ConvertFromString("#B59E00");
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
                    case "CTP":
                    case "SSE":
                    case "ODM":
                    case "SZE":
                    case "SZF":
                    case "QDM2":
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
