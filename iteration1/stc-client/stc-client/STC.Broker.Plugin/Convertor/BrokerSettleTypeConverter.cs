using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Broker.Plugin.Convertor
{
    public class BrokerSettleTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string instr = (value.ToString());
            string[] arrstr = instr.Split(',');
            if (arrstr.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string settl in arrstr)
            {
                if (settl.Equals("1"))
                {
                    sb.AppendLine("+0");
                }
                else if (settl.Equals("2"))
                {
                    sb.AppendLine("+1");
                }
                else if (settl.Equals("3"))
                {
                    sb.AppendLine("明+0");
                }
                else
                {
                    sb.AppendLine(settl);
                }
            }

            string[] lines = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string newster = string.Join(",", lines);
            if (newster.EndsWith(","))
            {
                newster = newster.Substring(0, newster.Length - 1);
            }    
            return newster;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
