using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    public class SettleParseHelper
    {
        #region 清算速度逻辑
        //selectSettle:T+0:0, T+1:1
        public static bool IsSpotSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime dtSelect = new DateTime(selectDate.Value.Year, selectDate.Value.Month, selectDate.Value.Day);

                int days = (dtSelect - dtNow).Days;

                if ((days == 0 && (selectSettle == 0 || selectSettle == 1)) || (days == 1 && selectSettle == 0))
                    result = true;
            }

            return result;
        }

        public static bool IsToday(DateTime? selectDate)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime dtSelect = new DateTime(selectDate.Value.Year, selectDate.Value.Month, selectDate.Value.Day);

                int days = (dtSelect - dtNow).Days;

                if (days == 0)
                    result = true;
            }

            return result;
        }

        public static bool IsTomorrow(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                string tem = DateTime.Now.ToString("yyyyMMdd");

                //明天+0=T+1
                if ((now.CompareTo(select) == 0 && selectSettle == 0) || (tem.CompareTo(select) == 0 && selectSettle == 1))
                    result = true;

            }

            return result;
        }

        public static bool IsFwdSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                TimeSpan ts = selectDate.Value - DateTime.Now;

                int days = ts.Days;

                if ((days == 1 || ts.TotalMinutes > 0) && selectSettle == 1)
                {
                    result = true;
                }
                else if ((days > 1) || (days == 1 && ts.TotalMinutes > 0))
                    result = true;
            }

            return result;
        }

        #endregion


    }
}
