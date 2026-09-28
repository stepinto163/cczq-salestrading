using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 时间处理类
    /// </summary>
    public class TimeUtils
    {
        private static TimeUtils timeUtils = new TimeUtils();
        public static TimeUtils Instance { get { return timeUtils; } }

        public bool TimeLengthMoreTwoYear(DateTime begin, DateTime end)
        {
            DateTime dtNew = begin.AddMonths(24);
            if (dtNew >= end)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断开始时间到结束时间是否超过6个月
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool TimeLengthMoreThanMonth(DateTime begin, DateTime end)
        {
            DateTime dtNew = begin.AddMonths(6);
            if (dtNew >= end)
            {
                return false;
            }
            return true;
        }

        public bool TimeLengthMoreThanDays(DateTime begin, DateTime end,int limitDays)
        {
            DateTime dtNew = begin.AddDays(limitDays);
            if (dtNew >= end)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 字符串格式化成日期
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ToDate(string time)
        {
            DateTime dt = Convert.ToDateTime(time);
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 字符串格式化成时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ToTime(string time)
        {
            if (time == null || "".Equals(time))
                return "";
            DateTime dt = Convert.ToDateTime(time);
            return dt.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 时间戳格式化成日期
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ToDatesTamp(string time)
        {
            long timestamp;
            if (long.TryParse(time, out timestamp))
            {
                DateTime datetime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                return datetime.ToString("yyyy-MM-dd");
            }
            return time;
        }
        /// <summary>
        /// 时间戳格式化成时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ToTimesTamp(string value)
        {
            long timestamp;
            if (long.TryParse(value, out timestamp))
            {
                DateTime datetime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                return datetime.ToString("HH:mm:ss");
            }
            return value;
        }

    }
}
