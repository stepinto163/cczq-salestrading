using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix
{
    /// <summary>
    /// Fix类型转换
    /// </summary>
    public class DataTypeConverter
    {

        public static string ToString(object o)
        {
            string str = null;
            if(o!=null)
            {
                str = o + "";
            }
            return str;
        }
        public static int ToInt(object o)
        {
            int i;
            string str = o + "";
            int.TryParse(str, out i);
            return i;
        }
        public static float ToFloat(object o)
        {
            float f;
            string str = o + "";
            float.TryParse(str, out f);
            return f;
        }

        public static double ToDouble(object o)
        {
            double d;
            string str = o + "";
            double.TryParse(str, out d);
            return d;
        }
        public static char ToChar(object o)
        {
            char c;
            string str = o + "";
            char.TryParse(str, out c);
            return c;
        }
        public static bool ToBool(object o)
        {
            bool b;
            string str = o + "";
            bool.TryParse(str, out b);
            return b;
        }
        public static decimal ToDecimal(object o)
        {
            decimal d;
            string str = o + "";
            decimal.TryParse(str, out d);
            return d;
        }
        public static short ToShort(object o)
        {
            short s;
            string str = o + "";
            short.TryParse(str, out s);
            return s;
        }
        public static long ToLong(object o)
        {
            long l;
            string str = o + "";
            long.TryParse(str, out l);
            return l;
        }
        public static byte ToByte(object o)
        {
            byte b;
            string str = o + "";
            byte.TryParse(str, out b);
            return b;
        }
        
    }
}
