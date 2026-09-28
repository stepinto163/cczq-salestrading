using STC.Common.Utils;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Speech
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            string logLevel = ConfigurationManager.AppSettings["logLevel"].ToString();
            string logAddress = ConfigurationManager.AppSettings["logAddress"].ToString();
            LogHandler.InitLog(logAddress, logLevel);
            LogHandler.Info("application startup");
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            LogHandler.Info("application exit, code:{0}", e.ApplicationExitCode);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            LogHandler.Error("TaskScheduler_UnobservedTaskException", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                LogHandler.Error("CurrentDomain_UnhandledException, will exit", (Exception)e.ExceptionObject);
            }
            else
            {
                LogHandler.Error("CurrentDomain_UnhandledException," + e.ExceptionObject);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                LogHandler.Error("App_DispatcherUnhandledException", e.Exception);
            }
            catch (Exception ex)
            {
                LogHandler.Error("App_DispatcherUnhandledException", ex);
            }
        }
    }
}
