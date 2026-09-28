using Ats.Foundation.Message;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class STCHelper
    {
        public static T Copy<T>(T a)
        {
            string temp = MsgHelper.Serializer(a);
            return MsgHelper.Deserialize<T>(temp);
        }

        /// <summary>
        /// 计算剩余期限
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string CalcLeftTenor(string date)
        {
            try
            {
                string fmt = "yyyyMMdd";
                DateTime dtCur = DateTime.ParseExact(DateTimeEx.ServerTime.ToString(fmt), fmt, CultureInfo.CurrentCulture);
                DateTime dtDest = DateTime.ParseExact(date, fmt, CultureInfo.CurrentCulture);

                TimeSpan ts = dtDest.Subtract(dtCur);
                int days = ts.Days;

                int restYear = dtDest.Year - dtCur.Year;

                string curMMdd = string.Format("{0:yyyyMMdd}", dtCur).Substring(4, 4);
                string maturMMdd = date.Substring(4, 4);

                if (restYear == 0)
                {
                    if (maturMMdd.CompareTo(curMMdd) == 0)
                        return "0D";
                    else if (maturMMdd.CompareTo(curMMdd) > 0)
                        return days + "D";
                }
                else if (restYear > 0)
                {
                    if (maturMMdd.CompareTo(curMMdd) >= 0)
                        dtCur = dtCur.AddYears(restYear);
                    else
                    {
                        dtCur = dtCur.AddYears(restYear - 1);
                        restYear -= 1;
                    }

                    ts = dtDest.Subtract(dtCur);
                    days = ts.Days;

                    double d = (days / (DateTime.IsLeapYear(dtDest.Year) ? 366.0 : 365.0));

                    d = Math.Round(d, 2);

                    if (restYear == 0)
                        return days + "D";
                    else
                    {
                        d += restYear;
                        return d.ToString("#0.00") + "Y";
                    }
                }
            }
            catch (Exception e)
            { }

            return string.Empty;
        }

        public static string SetShowTenor(BondsInfoPo bond)
        {
            string fmt = "yyyyMMdd";
            string result = string.Empty;
            bool isOption = false;
            string optionDate = bond.Option_Date + "";

            //永续债
            if (!string.IsNullOrEmpty(bond.Option_Type) && bond.Option_Type.Equals("ETS"))
            {
                result = CalcTenor(string.Empty, (bond.Maturity_Date == null ? string.Empty : bond.Maturity_Date));

                result = string.Format("{0} + N", result);
            }
            else if (!string.IsNullOrEmpty(optionDate))
            {
                DateTime dtNow = DateTime.ParseExact(DateTimeEx.ServerTime.ToString(fmt), fmt, CultureInfo.CurrentCulture);
                string now = dtNow.ToString(fmt);
                isOption = now.CompareTo(optionDate) > 0 ? false : true;

                if (isOption)
                {
                    result = CalcTenor(string.Empty, optionDate);

                    result = string.Format("{0} + {1}", result, CalcTenor(optionDate, bond.Maturity_Date));
                }
                else
                    result = CalcTenor(string.Empty, bond.Maturity_Date);
            }
            else
                result = CalcTenor(string.Empty, (bond.Maturity_Date == null ? string.Empty : bond.Maturity_Date));


            return result;
        }

        public static string SetShowTenor(SimpleBondsInfoPo bond)
        {
            string fmt = "yyyyMMdd";
            string result = string.Empty;
            bool isOption = false;
            string optionDate = bond.Option_Date + "";

            //永续债
            if (!string.IsNullOrEmpty(bond.Option_Type) && bond.Option_Type.Equals("ETS"))
            {
                result = CalcTenor(string.Empty, (bond.Maturity_Date==null?string.Empty:bond.Maturity_Date));

                result = string.Format("{0} + N", result);
            }
            else if (!string.IsNullOrEmpty(optionDate))
            {
                DateTime dtNow = DateTime.ParseExact(DateTimeEx.ServerTime.ToString(fmt), fmt, CultureInfo.CurrentCulture);
                string now = dtNow.ToString(fmt);
                isOption = now.CompareTo(optionDate) > 0 ? false : true;

                if (isOption)
                {
                    result = CalcTenor(string.Empty, optionDate);

                    result = string.Format("{0} + {1}", result, CalcTenor(optionDate, bond.Maturity_Date));
                }
                else
                    result = CalcTenor(string.Empty, bond.Maturity_Date);
            }
            else
                result = CalcTenor(string.Empty, (bond.Maturity_Date == null ? string.Empty : bond.Maturity_Date));


            return result;
        }


        /// <summary>
        /// 计算剩余期限
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string CalcTenor(string start, string end)
        {
            try
            {
                string fmt = "yyyyMMdd";
                DateTime dtCur = new DateTime();
                if (!string.IsNullOrEmpty(start))
                    dtCur = DateTime.ParseExact(start, fmt, CultureInfo.CurrentCulture);
                else
                    dtCur = DateTime.ParseExact(DateTimeEx.ServerTime.ToString(fmt), fmt, CultureInfo.CurrentCulture);

                DateTime dtDest = DateTime.ParseExact(end, fmt, CultureInfo.CurrentCulture);

                TimeSpan ts = dtDest.Subtract(dtCur);
                int days = ts.Days;

                int restYear = dtDest.Year - dtCur.Year;

                string curMMdd = string.Format("{0:yyyyMMdd}", dtCur).Substring(4, 4);
                string maturMMdd = end.Substring(4, 4);

                if (restYear == 0)
                {
                    if (maturMMdd.CompareTo(curMMdd) == 0)
                        return "0D";
                    else if (maturMMdd.CompareTo(curMMdd) > 0)
                        return days + "D";
                }
                else if (restYear > 0)
                {
                    if (maturMMdd.CompareTo(curMMdd) >= 0)
                        dtCur = dtCur.AddYears(restYear);
                    else
                    {
                        dtCur = dtCur.AddYears(restYear - 1);
                        restYear -= 1;
                    }

                    ts = dtDest.Subtract(dtCur);
                    days = ts.Days;

                    double d = (days / (DateTime.IsLeapYear(dtDest.Year) ? 366.0 : 365.0));
                    
                    d = Math.Round(d, 2);

                    if (restYear == 0)
                        return days + "D";
                    else
                    {
                        if (d == 0)
                            return restYear + "Y";
                        else
                        {
                            d += restYear;
                            if ((d % 1) == 0)
                                return d + "Y";
                            else
                                return d.ToString("#0.00") + "Y";
                        }
                    }
                }
            }
            catch (Exception e)
            { }

            return string.Empty;
        }
    }
}
