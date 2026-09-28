

using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace STC.Broker.Plugin.Utils
{
    public class IDCHelper
    {
        public static T Copy<T>(T a)
        {
            //string temp = MsgHelper.Serializer(a);
            //return MsgHelper.Deserialize<T>(temp);
            return a;
        }

        public static bool CheckStrEqual(string oldStr, string newStr)
        {
            if (oldStr == null)
                oldStr = "";
            if (newStr == null)
                newStr = "";

            return oldStr.Equals(newStr);
        }

        public static bool CheckStr(string oldStr, string newStr)
        {
            if (string.IsNullOrEmpty(oldStr) && !string.IsNullOrEmpty(newStr))
                return true;

            if (oldStr == null)
                oldStr = "";
            if (newStr == null)
                newStr = "";

            return oldStr.Equals(newStr);
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

        /// <summary>
        /// 获取授权人的名字（不是登录名）
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetAuthorizedUserName()
        {
            HashSet<string> result = new HashSet<string> { LocalEnv.UserInfo.FullName };
            SetNames(GRSettingsHandler.PowerSettingModel.UpdateOrders, result);
            SetNames(GRSettingsHandler.PowerSettingModel.QueryOrders, result);
            return result;
        }

        public static void SetNames(List<UserRoleSettingDetail> accountInfos, HashSet<string> hashSet)
        {
            if (accountInfos != null)
            {
                var accountInfo = accountInfos.FirstOrDefault(x => x.userName == LocalEnv.UserInfo.UserName);
                if (accountInfo != null && accountInfo.relatedUsers != null)
                {
                    foreach (var item in accountInfo.relatedUsers)
                    {
                        hashSet.Add(item.userName);
                    }
                }
            }
        }

        /// <summary>
        /// 剩余期限转成天数
        /// </summary>
        /// <param name="maturity">
        /// 剩余期限字符串，可能的格式有：
        /// xxY，xxD，xxY + xxD，xxY + N，xxD + N
        /// </param>
        /// <param name="maturityEndDate">债券到期日</param>
        /// <returns></returns>
        public static int GetMaturityDays(string maturity, string maturityEndDate = "")
        {
            int days = 0;
            if (string.IsNullOrWhiteSpace(maturity)) return days;
            bool leapYear = false;
            if (!string.IsNullOrWhiteSpace(maturityEndDate))
            {
                DateTime dateTime;
                DateTime.TryParse(maturityEndDate, out dateTime);
                leapYear = DateTime.IsLeapYear(dateTime.Year);
            }
            double yearDays = leapYear ? 366.0 : 365.0;
            var maturityArray = maturity.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ma in maturityArray)
            {
                if (ma.Trim().EndsWith("N")) continue;
                if (ma.Trim().EndsWith("Y"))
                {
                    double year;
                    double.TryParse(ma.Trim().Replace("Y", string.Empty), out year);
                    days += (int)Math.Ceiling(year * yearDays);
                }
                else if (ma.Trim().EndsWith("D"))
                {
                    double day;
                    double.TryParse(ma.Trim().Replace("D", string.Empty), out day);
                    days += (int)day;
                }
            }

            return days;
        }
    }
}
