using BDB.Trade.Plugin.Handler;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.CustomeUI
{
    /// <summary>
    /// CustomMsgView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomMsgView : Window
    {
        public CustomMsgView()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;
            this.ShowInTaskbar = false;

            this.Topmost = true;

            this.Loaded += CustomMsgView_Loaded;
        }

        private void CustomMsgView_Loaded(object sender, RoutedEventArgs e)
        {
            msgUI.Init(true);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
        }

        private void msgUI_OnDragMove(object obj)
        {
            this.DragMove();
        }

        private double maxWidth = 360;
        private double minWidth = 65;

        private void msgUI_OnSiseChange(double width, double height)
        {
            double left = this.Left;

            this.Width = width;
            this.Height = height;

            if (left > maxWidth)
            {
                if (this.Width > 100)
                {
                    maxWidth = this.Width;
                    this.Left = this.Left - maxWidth + minWidth;
                }
                else
                {
                    minWidth = this.Width;
                    this.Left += (maxWidth - minWidth);
                }
            }
            else
            {
                if (this.Width < 100)
                {
                    this.Left += (maxWidth - minWidth);
                }
            }
        }

        private void WinMsg_LocationChanged(object sender, EventArgs e)
        {
            msgUI.RefreshPopupPostion();
        }

        private void WinMsg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PermisionHandler._IsMoveMessage = false;
            
        }
        public void DealDoubleClick()
        {
            msgUI.DealDoubleClick();
        }
    }
}
