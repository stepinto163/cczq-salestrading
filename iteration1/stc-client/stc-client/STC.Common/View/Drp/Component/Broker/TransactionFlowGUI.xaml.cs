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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// TransactionFlowGUI.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionFlowGUI : Window
    {
        /// <summary>
        /// 订单id
        /// </summary>
        public string orderid = null;
        /// <summary>
        /// 是否是brokerA的弹窗
        /// </summary>
        public bool isA = false;
        /// <summary>
        /// 是否被拖动过
        /// </summary>
        public bool IsLocationChanged = false;
        /// <summary>
        /// 是否在拖动中
        /// </summary>
        private bool isDrag = false;
        /// <summary>
        /// 鼠标当前屏幕坐标
        /// </summary>
        private Point anchorPoint;
        /// <summary>
        /// 是否捕获到鼠标
        /// </summary>
        private bool iscaptured = false;

        public TransactionFlowGUI()
        {
            InitializeComponent();
            this.GotFocus += TransactionFlowGUI_GotFocus;
            this.Closed += TransactionFlowGUI_Closed;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void TransactionFlowGUI_Closed(object sender, EventArgs e)
        {
            if (this.Owner != null)
                Owner.Focus();
        }

        private void TransactionFlowGUI_GotFocus(object sender, RoutedEventArgs e)
        {
            //Application.Current.MainWindow.Focus();
            //if (this.Owner != null)
            //    Owner.Focus();            
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            anchorPoint = PointToScreen(e.GetPosition(this));
            isDrag = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDrag)
            {
                if (!iscaptured)
                {
                    CaptureMouse();
                    iscaptured = true;
                }
                // e.GetPosition(this) 获取鼠标在当前窗口的坐标
                // PointToScreen(point) 将窗口中的坐标转换成屏幕的坐标
                Point currentPoint = PointToScreen(e.GetPosition(this));
                /*
                 WPF使用如下的计算去得出如何将逻辑单位转换为物理设备的象素。
                pus [Physical Unit Size]= [独立设备单位尺寸]×[系统DPI]
                ＝1/96 英寸 ×120 dpi
                ＝1.25 象素
                wpf使用ups为单位设计空间。
                使用pus是为了保证设计的ui在不同dpi的电脑上大小保持一致，即物理尺寸不会变化。
               
                注意鼠标移动是已像素为单位。
                假设currentPoint.X - anchorPoint.X=96 即鼠标移动了96像素，
                则控件this.Left = this.Left + 96，实际上这里的增加的单位物理无关单位，并不是像素。
                在DPI为96的电脑上，鼠标移动96，则窗口移动96*1=96；
                在DPI为120的电脑上，鼠标移动96，则窗口一移动96*1.25 = 120个像素。
                所以通过鼠标移动来重置窗口的位置，需要考虑电脑的DPI设置，不然会是窗口与鼠标错位。
                 */
                /*this.Left = this.Left + (currentPoint.X - anchorPoint.X) / PrimaryScreen.ScaleX;
                this.Top = this.Top + (currentPoint.Y - anchorPoint.Y) / PrimaryScreen.ScaleY;
                anchorPoint = currentPoint;*/

                IsLocationChanged = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
                iscaptured = false;
                ReleaseMouseCapture();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
            //IsLocationChanged = true;
        }
    }
}
