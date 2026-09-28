using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Speech.Plugin
{
    class SpeechConvert
    {

        public static StringBuilder stringBuilder;


       /// <summary>
       /// side == 2
       /// 播报债券代码，方向，利率，时间
       /// </summary>
       /// <param name="bond"></param>
       /// <param name="side"></param>
       /// <param name="rate"></param>
       /// <param name="updateTime"></param>
       /// <returns></returns>
        public static string formatStr(string bondCode, string side, string rate)
        {
            if (!bondCode.Contains("."))
            {
                return "bondCode input error";
            }
            int idx = bondCode.IndexOf(".");
            string bondStr = bondCode.Substring(0, idx);
            string sideStr = numConvertSide(side);
            string rateStr = numConvertRate(rate);
            stringBuilder = new StringBuilder();
            stringBuilder.Append(bondStr+" ").Append(sideStr+" ").Append(rateStr);
            return stringBuilder.ToString();
        }

          /// <summary>
          /// side == 1
          /// 播报债券名称，方向，完整价格，期限和时间
          /// </summary>
          /// <param name="bond"></param>
          /// <param name="side"></param>
          /// <param name="rate"></param>
          /// <param name="term"></param>
          /// <param name="updateTime"></param>
          /// <returns></returns>
        public static string formatStr(string bondName, string side, string price, string term)
        {
           
            string bondNameStr = numConverBondName(bondName);
            string sideStr = numConvertSide(side);
            string termStr = numConverTerm(term);
           stringBuilder = new StringBuilder().Append(termStr+" ").Append(bondNameStr+" ").Append(sideStr+" ").Append(price);
            return stringBuilder.ToString();
        }


        /// <summary>
        /// 转换买卖方向
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static string numConvertSide(String side)
        {
            if (side.Equals("1"))
            {
                return "given";
            }
            else if (side.Equals("2"))
            {
                return "taken";
            }
            return null;
        }

        /// <summary>
        ///  转换债券名称
        ///  数字加中文字符串 转换为纯中文字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string numConverBondName(string str)
        {
            stringBuilder = new StringBuilder();
            foreach (char s in str)
            {
                //在 ASCII码表中，英文的范围是0 - 127，而汉字则是大于127
                if ((int)s > 127)
                {
                    stringBuilder.Append(s);
                }
                else if (Char.IsNumber(s))
                {
                    stringBuilder.Append(numConvertChinese(s.ToString()));
                }
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// 转换利率，获取小数点后面的位数
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static string numConvertRate(string rate)
        {
            stringBuilder = new StringBuilder();

            //小数点后不足两位补足
            rate = decimalPlaces(rate);
            int idx = rate.IndexOf(".");
            string str = rate.Substring(idx + 1);

            foreach (char s in str)
            {
                stringBuilder.Append(numConvertChinese(s.ToString()));
            }
            return stringBuilder.ToString();
        }


        public static string decimalPlaces(string price)
        {
            int idx = price.IndexOf(".");
            if (!price.Contains("."))
            {
                price = price + ".00";
            }
            else if (price.Contains("."))
            {

                if (price.Length - (idx + 1) < 2)
                {
                    price = price + "0";
                }
            }
            return price;
        }


        /// <summary>
        /// 转换期限
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static string numConverTerm(string year)
        {
            stringBuilder = new StringBuilder();
            if (null == year || "" == year)
            {
                return null;
            }
            if (year.Contains("Y") || year.Contains("y"))
            {
                string str = Regex.Replace(year, "Y", "", RegexOptions.IgnoreCase);
                foreach (char s in str)
                {
                    stringBuilder.Append(numConvertChinese(s.ToString()));
                }
                return stringBuilder.Append("年").ToString();
            }
            if (year.Contains("D") || year.Contains("d"))
            {
                if (year.Contains("."))
                {
                    return "Must be an integer";
                }
                int str = int.Parse(Regex.Replace(year, "D", "", RegexOptions.IgnoreCase));

                return stringBuilder.Append(converChinese(str)).Append("天").ToString();
            }
            if (year.Contains("M") || year.Contains("m"))
            {
                if (year.Contains("."))
                {
                    return "Must be an integer";
                }
                int str = int.Parse(Regex.Replace(year, "M", "", RegexOptions.IgnoreCase));
                return stringBuilder.Append(converChinese(str)).Append("月").ToString();
            }
            return null;
        }



        /// <summary>
        /// 数字转中文
        /// </summary>
        /// <param name="number">eg: 22</param>
        /// <returns></returns>
        public static string converChinese(int number)
        {
            string res = string.Empty;
            string str = number.ToString();
            string schar = str.Substring(0, 1);
            switch (schar)
            {
                case "1":
                    res = "一";
                    break;
                case "2":
                    res = "二";
                    break;
                case "3":
                    res = "三";
                    break;
                case "4":
                    res = "四";
                    break;
                case "5":
                    res = "五";
                    break;
                case "6":
                    res = "六";
                    break;
                case "7":
                    res = "七";
                    break;
                case "8":
                    res = "八";
                    break;
                case "9":
                    res = "九";
                    break;
                default:
                    res = "零";
                    break;
            }
            if (str.Length > 1)
            {
                switch (str.Length)
                {
                    case 2:
                    case 6:
                        res += "十";
                        break;
                    case 3:
                    case 7:
                        res += "百";
                        break;
                    case 4:
                        res += "千";
                        break;
                    case 5:
                        res += "万";
                        break;
                    default:
                        res += "";
                        break;
                }
                res += converChinese(int.Parse(str.Substring(1, str.Length - 1)));
            }
            return res;
        }

        /// <summary>
        /// 单个数字转中文
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string numConvertChinese(string str)
        {
            if (str.Equals("0"))
            {
                return "零";
            }
            else if (str.Equals("1"))
            {
                return "一";
            }
            else if (str.Equals("2"))
            {
                return "二";
            }
            else if (str.Equals("3"))
            {
                return "三";
            }
            else if (str.Equals("4"))
            {
                return "四";
            }
            else if (str.Equals("5"))
            {
                return "五";
            }
            else if (str.Equals("6"))
            {
                return "六";
            }
            else if (str.Equals("7"))
            {
                return "七";
            }
            else if (str.Equals("8"))
            {
                return "八";
            }
            else if (str.Equals("9"))
            {
                return "九";
            }
            else if (str.Equals("."))
            {
                return "点";
            }
            return null;
        }

        //// <summary>
        ///// 提取字符串中的数字字符串
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static bool IsNumber(String str)
        //{
        //    bool result = false;
        //    for (int i = 0; i < str.Length; i++)
        //    {
        //        if (Char.IsNumber(str, i))
        //        {
        //            result = false;
        //        }
        //        else
        //        {
        //            result = true;
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// 判断是否是数字还是自负串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {

            foreach (char c in str)
            {
                if (!Char.IsNumber(c))
                {
                    return false;
                }
            }
            return true;
        }




    }
}
