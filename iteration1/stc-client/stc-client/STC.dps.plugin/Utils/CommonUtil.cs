using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.dps.plugin.Utils
{
    public class CommonUtil
    {
        public static int ConvertToInt(string obj)
        {
            try
            {
                int temp = Convert.ToInt32(Convert.ToDouble(obj));
                return temp;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public static double ConvertToDouble(string obj)
        {
            try
            {
                double temp = 0;
                double.TryParse(obj, out temp);
                return temp;
            }
            catch (Exception e)
            {

            }
            return 0;
        }
    }
}
