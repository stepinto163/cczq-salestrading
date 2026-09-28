using IDC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IDC.Frame
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            LocalEnv.Load();
            Atf.Common.LocalEnv.SystemEnv.PutEnvironment("DefaultMQServer", "CEDA");
            Atf.Common.LocalEnv.SystemEnv.PutEnvironment("ACSModel", "ACS2");
            Atf.Common.LocalEnv.SystemEnv.PutEnvironment("MQServerList", "CEDA");

            com.adaptiveMQ2.utils.CLog.initLog(0);
            LoggerFactory.Instance().ConfigureClassic(LocalEnv.LogInfo.LogPath, LocalEnv.LogInfo.LogLevel);
            LogHelper.Info("application startup");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            LogHelper.Error("TaskScheduler_UnobservedTaskException", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {           
            Console.WriteLine(e.ExceptionObject);
            if(e.ExceptionObject is Exception)
            {
                LogHelper.Error("CurrentDomain_UnhandledException, will exit", (Exception)e.ExceptionObject);
            }
            else
            {
                LogHelper.Error("CurrentDomain_UnhandledException," + e.ExceptionObject);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                LogHelper.Error("App_DispatcherUnhandledException",e.Exception);
            }catch(Exception ex)
            {
                LogHelper.Error("App_DispatcherUnhandledException",ex);
            }            
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            LogHelper.Info("application exit, code:{0}", e.ApplicationExitCode);
        }
        
    }
}
