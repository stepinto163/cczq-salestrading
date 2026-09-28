
using System;
using log4net;


namespace STC.Common.Utils
{
    /// <summary>
    /// 日志工厂管理类
    /// </summary>
    public class LoggerFactory
    {       
        private static Object lockObject = new object();
        private static LoggerFactory _Hander = null;        
        public static LoggerFactory Instance()
        {
            if (_Hander == null)
            {
                lock (lockObject)
                {
                    if (_Hander == null)
                    {
                        _Hander = new LoggerFactory();
                    }
                }
            }
            return _Hander;
        }

        private LoggerFactory() { }

        /// <summary>
        /// 监控日志配置文件变化
        /// </summary>
        /// <param name="config"></param>
        public void ConfigureAndWatch(string config)
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(config));
        }

        /// <summary>
        /// 设置日志配置文件
        /// </summary>
        /// <param name="config"></param>
        public void Configure(string config)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(config));
        }

        /// <summary>
        /// 初始化log4net配置
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="logLevel"></param>
        public void ConfigureClassic(string logPath,string logLevel)
        {
            log4net.Appender.RollingFileAppender appender = new log4net.Appender.RollingFileAppender();            

            appender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Composite;
            appender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            appender.File = logPath;
            appender.AppendToFile = true;
            appender.DatePattern = "yyyyMMdd'.LOG'";
            appender.MaxFileSize = 1024*1024*10;
            appender.MaxSizeRollBackups = 10;
            appender.StaticLogFileName = false;
            appender.Layout = new log4net.Layout.PatternLayout("%-30d{yyyy/MM/dd HH:mm:ss fff} [%t] %level  %m %n");
            log4net.Filter.LevelRangeFilter filter = new log4net.Filter.LevelRangeFilter();
            filter.LevelMax = log4net.Core.Level.Fatal;
            if ("debug".Equals(logLevel,StringComparison.OrdinalIgnoreCase))
            {
                filter.LevelMin = log4net.Core.Level.Debug;
            }
            else if("info".Equals(logLevel,StringComparison.OrdinalIgnoreCase))
            {
                filter.LevelMin = log4net.Core.Level.Info;
            }
            else if ("warn".Equals(logLevel, StringComparison.OrdinalIgnoreCase))
            {
                filter.LevelMin = log4net.Core.Level.Warn;
            }
            else if ("error".Equals(logLevel, StringComparison.OrdinalIgnoreCase))
            {
                filter.LevelMin = log4net.Core.Level.Error;
            }
            else 
            {
                filter.LevelMin = log4net.Core.Level.Info;
            }

            appender.AddFilter(filter);

            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);//设置到配置文件
        }
                
        public Logger GetLogger(Type type)
        {
            return new Logger(log4net.LogManager.GetLogger(type));
        }
                      
        public Logger GetRootLogger()
        {
            return new Logger(log4net.LogManager.GetLogger("Root"));
        }
        
        public Logger GetLogger(String LogName)
        {
            return new Logger(log4net.LogManager.GetLogger(LogName));
        }
    }

    public class Logger
    {   
        public Logger(ILog iLog)
        {
            _ILog = iLog;
        }

        private ILog _ILog = null;

        #region ILog 成员

        public void Debug(object message, Exception exception)
        {
            _ILog.Debug(message, exception);
        }

        public void Debug(object message)
        {
            _ILog.Debug(message);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            _ILog.DebugFormat(provider, format, args);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            _ILog.DebugFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            _ILog.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0)
        {
            _ILog.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _ILog.DebugFormat(format, args);
        }

        public void Error(object message, Exception exception)
        {
            _ILog.Error(message, exception);
        }

        public void Error(object message)
        {
            _ILog.Error(message);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            _ILog.ErrorFormat(provider, format, args);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            _ILog.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            _ILog.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0)
        {
            _ILog.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _ILog.ErrorFormat(format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            _ILog.Fatal(message, exception);
        }

        public void Fatal(object message)
        {
            _ILog.Fatal(message);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            _ILog.FatalFormat(format, args);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            _ILog.FatalFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            _ILog.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0)
        {
            _ILog.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _ILog.FatalFormat(format, args);
        }

        public void Info(object message, Exception exception)
        {
            _ILog.Info(message, exception);
        }

        public void Info(object message)
        {
            _ILog.Info(message);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            _ILog.InfoFormat(provider, format, args);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            _ILog.InfoFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            _ILog.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0)
        {
            _ILog.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _ILog.InfoFormat(format, args);
        }

        public bool IsDebugEnabled
        {
            get { return _ILog.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _ILog.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get {return _ILog.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _ILog.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _ILog.IsWarnEnabled; }
        }

        public void Warn(object message, Exception exception)
        {
            _ILog.Warn(message, exception);
        }

        public void Warn(object message)
        {
            _ILog.Warn(message);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            _ILog.WarnFormat(provider, format, args);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            _ILog.WarnFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            _ILog.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0)
        {
            _ILog.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _ILog.WarnFormat(format, args);
        }

        #endregion

        #region ILoggerWrapper 成员

        public log4net.Core.ILogger Instance
        {
            get { return _ILog.Logger; }
        }

        #endregion
    }

}
