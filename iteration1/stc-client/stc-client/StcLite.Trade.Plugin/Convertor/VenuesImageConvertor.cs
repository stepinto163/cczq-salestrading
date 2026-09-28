using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StcLite.Trade.Plugin.Convertor
{
    public class VenuesImageConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string venues = value.ToString();
                switch(venues)
                {
                    //case "TPSC":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_guoli.png";
                    //case "CTTJ":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_xintang.png";
                    //case "CNEX":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_guoji.png";
                    //case "CCTB":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_zhongcheng.png";
                    //case "PTCN":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_pingan.png";
                    //case "ODM":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_xbond.png";
                    //case "QDMESP":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_shuangbian.png";
                    //case "QDM":
                    //    return "/STC.XBond.Plugin;component/Images/stc_tag_brokers_rfq.png";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
