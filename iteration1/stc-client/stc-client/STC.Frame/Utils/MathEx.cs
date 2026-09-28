using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
   public class MathEx
    {
        public static int decimalCount(decimal dec)
        {
            string tmp = dec + "";
            if (tmp.IndexOf('.') >= 0)
            {
                return tmp.Length - tmp.IndexOf('.') - 1;
            }
            return 0;
        }

        public static string PriceFormat(decimal price)
        {
            int i = MathEx.decimalCount(price);
            if (i < 2)
            {
                return price.ToString("N2");
            }

            return price + "";
        }
    }
}
