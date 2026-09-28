using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// VenuesExecutionCopyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VenuesExecutionCopyWindow : Window
    {
        public VenuesExecutionCopyWindow()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            //Storyboard std = this.Resources["Storyboard_load"] as Storyboard;
            //std.Completed += delegate { this.Close(); };

            //std.Begin();

            ThreadPool.QueueUserWorkItem(o => {
                Thread.Sleep(2000);

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, 
                    new Action(() => {
                        this.Close();
                    }));
            });
        }
    }
}
