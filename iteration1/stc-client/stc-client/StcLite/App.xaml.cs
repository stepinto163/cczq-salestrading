using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using STC.Common.Utils.Proxy;
using STC.Common.Model.Common;
using System.Net;
using common.netapi.Utils;
using STC.Common.View.UI;
using STC.Frame.UI;

namespace BDB
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        string temArgs = string.Empty;
        protected override void OnStartup(StartupEventArgs e)
        {
            Process me = Process.GetCurrentProcess();
            List<Process> processes = new List<Process>(Process.GetProcessesByName(me.ProcessName));
            if (processes.Count > 2)
            {
                var matches =
                        processes.FindAll((p) =>
                        {
                            return
                                string.Equals(p.ProcessName, me.ProcessName, StringComparison.InvariantCultureIgnoreCase) &&
                                (p.Id != me.Id);
                        });
                Console.WriteLine("Application is already running.");
                Application.Current.Shutdown();
                if (matches.Count == 1)
                {
                    Process prior = matches[0];
                    if (prior.MainWindowHandle != IntPtr.Zero)
                    {
                        //bool b = NativeMethods.ShowWindow(prior.MainWindowHandle, NativeMethods.SH_SHOW);
                        //Console.WriteLine("NativeMethods.ShowWindow: {0}", b);
                        STC.Common.View.Component.PopupEx.NativeMethods.SetForegroundWindow(prior.MainWindowHandle);
                    }
                }
                return;
            }

            // 6️⃣ 允许磁盘缓存，加速页面加载
            string cachePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "bin", "cache");
            try
            {
                var visitedLinksFile = System.IO.Path.Combine(cachePath, "Visited Links");
                if (System.IO.File.Exists(visitedLinksFile))
                {
                    System.IO.File.Delete(visitedLinksFile);
                }
            }
            catch
            {
                // 文件被锁定时忽略，下次启动再尝试
            }

            //base.OnStartup(e);
            temArgs = JsonConvert.SerializeObject(e);
            Console.WriteLine("OnStartup: {0}", System.AppDomain.CurrentDomain.BaseDirectory);
            Console.WriteLine("OnStartup: args count={0}", e.Args.Length);
            common.netapi.Utils.FileUtil.BaseDir = FileUtil.BaseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            foreach (string item in e.Args)
            {
                Console.WriteLine("args: {0}", item);
            }
            if (e.Args.Length >= 1 && "-N" == e.Args[0])
            {
                if (e.Args.Length > 2)
                    QbLoginWindow.ArgsList.Add(e.Args[2]);
                if (e.Args.Length > 4)
                {
                    QbLoginWindow.ArgsList.Add(e.Args[4]);
                    QbLoginWindow.ArgsList.Add("encrypt");
                }
            }
            else
            {
                QbLoginWindow.ArgsList.AddRange(e.Args);
            }

            //QbLoginWindow.ArgsList.Add("idb_trader07");
            //QbLoginWindow.ArgsList.Add("abc123");
            //QbLoginWindow.ArgsList.Add("encrypt");

            base.OnStartup(e);
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.Name = "main";
            string appName = string.Format("{0}-{1}", Environment.UserName, ConfigurationManager.AppSettings["AppName"]);
            bool isSingle = true;
            bool.TryParse(ConfigurationManager.AppSettings["IsSingle"], out isSingle);
            bool createNew;

            string openType = IniFileHelper.ReadValue("StartType", "OpenType");

            BaseMachineHelper._appEvent = new EventWaitHandle(false, EventResetMode.AutoReset, appName, out createNew);

            if (isSingle)
            {
                if (string.IsNullOrEmpty(openType))//启动方式判断
                {
                    if (!createNew)
                    {
                        BaseMachineHelper._appEvent.Set();

                        Current.Shutdown();

                        return;
                    }
                    bool flag = BaseMachineHelper.CheckProcessIsMultiple(appName);
                    if (flag)
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg("另一个窗口已在运行，不能重复运行");

                        msgBox.Show();

                        return;
                    }
                }
                else
                    IniFileHelper.WriteValue("StartType", "OpenType", string.Empty);
            }
            
            if (BaseMachineHelper.IsVirtualMachine())
                RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            BaseMachineHelper.PreventSleep();

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;



            LocalEnv.Load();
            Task.Factory.StartNew(() => { STC.Common.Utils.ComboSourceUtil.Instance.InitComboSourceUtil(); });
            //加载样式
            List<STC.Common.Skin> resources = System.Windows.Application.Current.Resources.MergedDictionaries.Where(d => d is STC.Common.Skin).Cast<STC.Common.Skin>().ToList();
            if (LocalEnv.ConfInfo.DefaultSkinStyle != null)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    int cnt = System.Windows.Application.Current.Resources.MergedDictionaries.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        System.Windows.ResourceDictionary rd = System.Windows.Application.Current.Resources.MergedDictionaries[i];
                        if (!(rd is STC.Common.Skin))
                        {
                            System.Windows.Application.Current.Resources.MergedDictionaries.Remove(rd);
                            cnt--;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    System.Windows.Application.Current.Resources.MergedDictionaries.Clear();

                    foreach (STC.Common.Skin d in resources)
                    {
                        System.Windows.ResourceDictionary dict1 = new System.Windows.ResourceDictionary();
                        string url = $"pack://application:,,,/STC.Common;component/Resources/{LocalEnv.ConfInfo.DefaultSkinStyle}/{d.Name}.xaml";
                        var skinSource = new Uri(url, UriKind.RelativeOrAbsolute);
                        try
                        {
                            dict1.Source = skinSource;
                            System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict1);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Error(ex.Message);
                        }
                    }

                    System.Windows.ResourceDictionary dict2 = new System.Windows.ResourceDictionary();
                    string url2 = $"pack://application:,,,/STC.Common;component/themes/Generic.xaml";
                    dict2.Source = new Uri(url2, UriKind.RelativeOrAbsolute);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict2);

                    System.Windows.ResourceDictionary baseDic = new System.Windows.ResourceDictionary();
                    string baseURL = $"pack://application:,,,/STC.Common;component/Resources/BaseSkin.xaml";
                    baseDic.Source = new Uri(baseURL, UriKind.RelativeOrAbsolute);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(baseDic);
                }
            }

            //com.adaptiveMQ2.utils.CLog.initLog(0);
            LogHandler.InitLog(LocalEnv.LogInfo.LogPath, LocalEnv.LogInfo.LogLevel);
            LogHandler.Info("application startup");

            LogHandler.Info("Args:" + temArgs);

            string version = ReflectionHelper.GetVersion(this.GetType().Assembly);
            //DateTime buildtime = System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location);
            DateTime buildtime = ReflectionHelper.GetPe32Time(this.GetType().Assembly);

            LogHandler.Info("BDB Client Version:{0}(build:{1:yyyyMMdd})", version, buildtime);

            if (QbLoginWindow.ArgsList.Count >= 3)
            {
                LocalEnv.ConfInfo.LoginType = "1";
                Application.Current.StartupUri = new Uri("/STC.Frame;component/UI/QbLoginWindow.xaml", UriKind.RelativeOrAbsolute);
            }
            else
            {
                LocalEnv.ConfInfo.LoginType = "2";
                Application.Current.StartupUri = new Uri("/STC.Frame;component/UI/LoginGUI.xaml", UriKind.RelativeOrAbsolute);
            }
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {
            BaseMachineHelper.AllowSleep();
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
            if (e.Exception.HResult == -2147221040 // Clipboard问题
                || e.Exception.Message.Contains("NewItemFactory")
                || e.Exception.Message.Contains("OpenClipboard")//OpenClipboard 失败 (异常来自 HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))
                || e.Exception.Message.Contains("所请求的剪贴板操作失败"))//System.Runtime.InteropServices.ExternalException (0x800401D0): 所请求的剪贴板操作失败。

            {
                e.Handled = true;
                return;
            }
            var st = new StackTrace();
            var sb = new System.Text.StringBuilder();

            if (st.FrameCount >= 2)
            {
                StackFrame sf = st.GetFrame(2);
                var declaringType = sf.GetMethod().DeclaringType;
                if (declaringType != null) sb.Append($"{declaringType.Name}.{sf.GetMethod().Name} - ");
            }

            sb.Append(e.Exception);

            string msg = string.Empty;
            try
            {
                e.Handled = true;
                LogHandler.Error($"App_DispatcherUnhandledException:\r\n {sb.ToString()}");
                msg = $"按确定后关闭或取消继续！\r\n{e.Exception.Message}";
            }
            catch (Exception ex)
            {
                LogHandler.Error("App_DispatcherUnhandledException", ex);
                msg = $"按确定后关闭或取消继续！\r\n{ex.Message}";
            }
            finally
            {
                ShowErrorMessage(msg);
            }
        }


        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                STC.Common.View.UI.MessageBoxGUI me = new STC.Common.View.UI.MessageBoxGUI();
                me.MOK += Me_MOK;
                //me.HideCancelButton();
                me.setWarnMsg(message);
                me.ShowDialog();
            }));
        }

        private void Me_MOK()
        {
            try
            {
                LogHandler.Info("确定关闭程序");
                Environment.Exit(0);
                //Application.Current.Shutdown();
            }
            catch
            {
                Environment.Exit(0);
            }
        }
    }
}
