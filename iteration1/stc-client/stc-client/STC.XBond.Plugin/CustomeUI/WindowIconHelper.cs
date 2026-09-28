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

namespace STC.XBond.Plugin.CustomUI
{
    public class WindowIconHelper
    {
        private static WindowIconHelper intance = new WindowIconHelper();

        public static WindowIconHelper Instance { get { return intance; } }

        private NotifyIcon notifyIcon;
        private DispatcherTimer icoTimer;
        private TaskbarItemInfo taskbarinfo;
        private string icoUrl = "pack://application:,,,/Images/stc_windows_icon_128.ico";
        private string icoHiddenUrl = "pack://application:,,,/Images/favicon.ico";
        private int i = 0;
        private Icon notifyImage;
        private Icon notifyHiddenImage;

        private WindowIconHelper()
        {
            taskbarinfo = new TaskbarItemInfo();
            icoTimer = new DispatcherTimer();
            icoTimer.Interval = TimeSpan.FromSeconds(0.3);
            icoTimer.Tick += IcoTimer_Tick;

            notifyIcon = new NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri(icoUrl)).Stream;
            notifyIcon.Icon = notifyImage = new System.Drawing.Icon(iconStream);

            Stream iconHiddenStream = System.Windows.Application.GetResourceStream(new Uri(icoHiddenUrl)).Stream;
            notifyHiddenImage = new System.Drawing.Icon(iconHiddenStream, 10, 10);

            notifyIcon.Visible = true;
            notifyIcon.Click += NotifyIcon_Click;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.MainWindow.Activate();

            Stop();
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

            FlashWindow.Start(System.Windows.Application.Current.MainWindow);
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
            notifyIcon.Visible = false;
        }
    }
}
