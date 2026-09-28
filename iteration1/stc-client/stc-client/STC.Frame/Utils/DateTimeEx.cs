using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 服务器时间扩展类
    /// </summary>
    public class DateTimeEx
    {
        public static DateTime GetDatetime(long timestamp)
        {
            DateTime time = DateTime.Now;

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
                return new DateTime(DateTime.Now.Ticks + LocalEnv.ServerInfo.Add_ServerTicks);
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

    }
}
