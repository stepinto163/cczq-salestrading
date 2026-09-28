using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Utils;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 登录
    /// </summary>
    public class LoginHelper
    {
        public const string DefaultLogoutMessage = "账号已在其他地方登录，请重新登录！";
        private readonly object logoutMessageLock = new object();
        private string latestLogoutMessage;
        private DateTime latestLogoutMessageTime = DateTime.MinValue;
        private bool logoutStarted;

        private static LoginHelper loginHelper = new LoginHelper();
        public static LoginHelper Instance { get { return loginHelper; } }
        private LoginHelper()
        {

        }

        public void Logout2(string msg)
        {
            lock (logoutMessageLock)
            {
                if (logoutStarted)
                {
                    return;
                }
                logoutStarted = true;
            }
            ThreadPool.QueueUserWorkItem(o => Logout(ResolveLogoutMessage(msg)));
        }

        public void SetLogoutMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg) || DefaultLogoutMessage.Equals(msg))
            {
                return;
            }

            lock (logoutMessageLock)
            {
                latestLogoutMessage = msg;
                latestLogoutMessageTime = DateTime.Now;
            }
        }

        private string ResolveLogoutMessage(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg) && !DefaultLogoutMessage.Equals(msg))
            {
                SetLogoutMessage(msg);
                return msg;
            }

            string pendingMessage = GetLatestLogoutMessage();
            if (!string.IsNullOrWhiteSpace(pendingMessage))
            {
                return pendingMessage;
            }

            Thread.Sleep(300);
            pendingMessage = GetLatestLogoutMessage();
            return string.IsNullOrWhiteSpace(pendingMessage) ? DefaultLogoutMessage : pendingMessage;
        }

        private string GetLatestLogoutMessage()
        {
            lock (logoutMessageLock)
            {
                if (!string.IsNullOrWhiteSpace(latestLogoutMessage)
                    && DateTime.Now.Subtract(latestLogoutMessageTime).TotalSeconds <= 10)
                {
                    return latestLogoutMessage;
                }
            }

            return null;
        }

        private void Logout(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LogHelper.Warn(msg);
                //Atf.MQIII.MQIIIManager.Instance.Stop();
                if (LocalEnv.ConfInfo.ApiKey.Equals(ApiConsts.API_KEY_GW))
                    ApiManager.Instance.GetDefaultApi().DisConnect();

                LogHelper.Info("CEDA stoped");
                SysWarnGUI warn = new SysWarnGUI();
                warn.tbMsg.Text = msg;
                if (warn != Application.Current.MainWindow)
                {
                    try
                    {
                        warn.Owner = Application.Current.MainWindow;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warn(ex);
                    }

                }

                warn.Topmost = true;
                warn.ShowDialog();
                if (warn.IsOk)
                {
                    //Application.Current.Shutdown();
                    AppExit();
                }
            }));
        }

        public void AppExit()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    LogHelper.Info("Application exit.");

                    foreach (Window window in Application.Current.Windows)
                    {
                        window.Close();
                    }

                    WriteDBLogHelper.Instance.ExitApp();

                    Environment.Exit(0);
                    //Application.Current.Shutdown());
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message);
                }

            });
        }
    }
}
