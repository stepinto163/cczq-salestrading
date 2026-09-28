using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 日志公共类
    /// </summary>
    public class LogHandler
    {
        public static void InitLog(string logPath, string logLevel)
        {
            LoggerFactory.Instance().ConfigureClassic(logPath, logLevel);
        }

        public static void Debug(object msg)
        {
            LogHelper.Debug(msg);
        }
        public static void Debug(string format, params object[] msgs)
        {
            LogHelper.Debug(format, msgs);
        }
        public static void Error(object msg, Exception ex)
        {
            LogHelper.Error(msg, ex);
        }
        public static void Error(object msg)
        {
            LogHelper.Error(msg);
        }
        public static void Warn(object msg)
        {
            LogHelper.Warn(msg);
        }
        public static void Warn(object msg, Exception ex)
        {
            LogHelper.Warn(msg, ex);
        }
        public static void Info(object msg)
        {
            LogHelper.Info(msg);
        }
        public static void Info(string format, params object[] ps)
        {
            LogHelper.Info(format, ps);

        }
    }
}
