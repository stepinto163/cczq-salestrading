using System;

namespace STC.Common.Utils
{
    /// <summary>
    /// 日志对外接口类
    /// </summary>
   public class LogHelper
    {
       private static Logger logger = LoggerFactory.Instance().GetRootLogger();
       public  static  void Debug(object msg)
       {
            logger.Debug(msg);
       }
        public static void Debug(string format, params object[] msgs)
        {
            if (msgs.Length > 0)
            {
                logger.DebugFormat(format, msgs);
            }
            else
            {
                logger.Debug(format);
            }
            
        }       
        public static void Error(object msg, Exception ex)
        {
            logger.Error(msg,ex);
        }
        public static void Error(object msg)
       {
            logger.Error(msg);
        }
       public static void Warn(object msg)
       {
            logger.Warn(msg);
       }
        public static void Warn(object msg,Exception ex)
        {
            logger.Warn(msg, ex);
        }
        public static void Info(object msg)
       {
            logger.Info(msg);            
       }
        public static void Info(string format, params object[] ps)
        {
            if (ps.Length >0)
            {
                logger.InfoFormat(format, ps);
            }
            else
            {
                logger.Info(format);
            }
            
        }

    }
}
