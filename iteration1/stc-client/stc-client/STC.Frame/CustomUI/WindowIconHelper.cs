using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;

namespace STC.Frame.CustomUI
{
    public class WindowIconHelper
    {
        private static WindowIconHelper intance = new WindowIconHelper();

        public static WindowIconHelper Instance { get { return intance; } }

        private NotifyIcon notifyIcon;
        private DispatcherTimer icoTimer;
        private TaskbarItemInfo taskbarinfo;
        private string icoUrl = "";
        private string icoHiddenUrl = "pack://application:,,,/STC.Frame;component/Images/stc_image_transparentbg_64x64.ico";
        private int i = 0;
        private Icon notifyImage;
        private Icon notifyHiddenImage;

        private WindowIconHelper()
        {
            taskbarinfo = new TaskbarItemInfo();
            icoTimer = new DispatcherTimer();
            icoTimer.Interval = TimeSpan.FromSeconds(0.5);
            icoTimer.Tick += IcoTimer_Tick;

            notifyIcon = new NotifyIcon();

            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {

                case "dmart":
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/dmart_windows_ico_64.ico";
                    break;
                case "gtja":
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/gtja_windows_ico_64.ico";
                    break;
                case "idbpro":
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/idbpro_windows_icon_64.ico";
                    break;
                case "bdb":
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/bdb_windows_icon_128.ico";
                    break;
                case "idcclient":
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/bdb_windows_icon_128.ico";
                    break;
                default:
                    icoUrl = "pack://application:,,,/STC.Common;component/Images/stc_windows_icon_64.ico";
                    break;
            }

            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri(icoUrl)).Stream;
            notifyIcon.Icon = notifyImage = new System.Drawing.Icon(iconStream);

            Stream iconHiddenStream = System.Windows.Application.GetResourceStream(new Uri(icoHiddenUrl)).Stream;
            notifyHiddenImage = new System.Drawing.Icon(iconHiddenStream, 16, 16);

            notifyIcon.Visible = true;
            notifyIcon.Click += NotifyIcon_Click;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.Windows.Application.Current != null
                && System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
                    System.Windows.Application.Current.MainWindow.Activate();
                }

                Stop();
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
            
        }

        private void IcoTimer_Tick(object sender, EventArgs e)
        {
            i = i + 1;
            if (i % 2 != 0)
            {
                notifyIcon.Icon = notifyImage;
            }
            else
            {
                notifyIcon.Icon = notifyHiddenImage;
            }
            
        }

        public void Start()
        {
            icoTimer.Start();

            FlashWindow.Flash(System.Windows.Application.Current.MainWindow, 3);
        }

        public void Stop()
        {
            FlashWindow.Stop(System.Windows.Application.Current.MainWindow);
            icoTimer.Stop();
            notifyIcon.Icon = notifyImage;
            taskbarinfo.ProgressState = TaskbarItemProgressState.None;
        }

        public void Clear()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                FlashWindow.Stop(System.Windows.Application.Current.MainWindow);

            icoTimer.Stop();

            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
        }

        /// <summary>
        /// 显示提醒消息（线程安全，可在线程里调用）
        /// </summary>
        public void ShowNotifyMessage(string title, string message, int durationMs = 3000)
        {
            //s确保在 UI 线程调用
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var balloon = new BalloonWindow(title, message);
                balloon.Show();
            });
        }
    }
}
