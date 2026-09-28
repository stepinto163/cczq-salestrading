using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Utils
{
    public class TimeUtils
    {
        private static TimeUtils timeUtils = new TimeUtils();
        public static TimeUtils Instance { get { return timeUtils; } }
        public bool TimeLengthMoreThanMonth(DateTime begin, DateTime end)
        {
            DateTime dtNew = begin.AddMonths(6);
            if (dtNew >= end)
            {
                return false;
            }
            return true;
        }
        public bool TimeLengthMoreTwoYear(DateTime begin, DateTime end)
        {
            DateTime dtNew = begin.AddMonths(24);
            if (dtNew >= end)
            {
                return false;
            }
            return true;
        }

        public string ToDate(string time)
        {
            DateTime dt = Convert.ToDateTime(time);
            return dt.ToString("yyyy-MM-dd");
        }
        public string ToTime(string time)
        {
            if (time == null || "".Equals(time))
                return "";
            DateTime dt = Convert.ToDateTime(time);
            return dt.ToString("HH:mm:ss");
        }
    }
}
