using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 服务器时间扩展类
    /// </summary>
    public class DateTimeEx
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static DateTime GetNow()
        {
            try
            {
                if (LocalEnv.ConfInfo.IsOSVersionAfterWin7)
                {
                    long fileTime;
                    GetSystemTimePreciseAsFileTime(out fileTime);
                    return DateTimeOffset.FromFileTime(fileTime).DateTime;
                }
                else
                {
                    return DateTime.Now;
                }
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static DateTime GetDatetime(long timestamp)
        {
            DateTime time = GetNow();
            DateTime dt_1970 = new DateTime(1970, 1, 1);            
            long ticks_1970 = dt_1970.Ticks;
            long time_tricks = ticks_1970 + timestamp * 10000*1000;
            time = new DateTime(time_tricks).ToLocalTime() ;
                      
            return time;
        }

        public static DateTime ServerTime
        {
            get
            {
                return new DateTime(GetNow().Ticks + LocalEnv.ServerInfo.Add_ServerTicks);
            }
        }

        /// <summary>
        /// 本地时间
        /// </summary>
        public static DateTime ServerTimeNow
        {
            get
            {
                return GetNow();
            }
        }

        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static int GetTimeStamp(DateTime dt)
        {
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            int timeStamp = Convert.ToInt32((dt - dateStart).TotalSeconds);
            return timeStamp;
        }

        public static DateTime? ConvertDateTime(string dataTime)
        {
            DateTime dt;
            if (!DateTime.TryParse(dataTime, out dt))
            {
                return null;
            }
            return dt;
        }


        public static DateTime ConvertStringToDateTime(string datetime)
        {
            string[] format = { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss", "yyyyMMdd HH:mm:ss.fff", "yyyyMMdd HH:mm:ss" };
            DateTime dt2 = DateTime.MinValue;
            if (DateTime.TryParseExact(datetime, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt2))
            {
                return dt2;
            }

            return dt2;
        }

    }
}
