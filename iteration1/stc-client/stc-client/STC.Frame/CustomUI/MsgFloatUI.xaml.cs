using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Frame.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// MsgFloatUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgFloatUI : UserControl
    {
        private MsgPo msgPo;
        private Storyboard sb = null;
        public MsgFloatUI()
        {
            InitializeComponent();
        }

        public void Init(MsgPo msgPo)
        {
            this.msgPo = msgPo;

            sb = (Storyboard)this.FindResource("sbFloat");
            ColorAnimation ca = new ColorAnimation();
            ca.Duration = new Duration(TimeSpan.FromSeconds(5));
            sb.Children.Clear();
            sb.Children.Add(ca);

            tbContentType.Foreground = msgPo.ContentTypeBrush;
            tbContentType.Text = msgPo.ContentType;
            tbStatus.Text = msgPo.Status;
            tbStatus.Foreground = msgPo.StatusBrush;
            tbValuetime.Text = msgPo.ValueTime;
            tbValuetime.Foreground = msgPo.ValueTimeBrush;
            tbCloseBy.Text = msgPo.CloseBy;
            tbCloseBy.Foreground = msgPo.CloseByBrush;
            tbContent.Text = msgPo.Content;
            tbContent.Foreground = msgPo.ContentBrush;
            tbContentType.Text = msgPo.ContentType;
            tbContentType.Foreground = msgPo.ContentTypeBrush;
            tbUpdatetime.Text = msgPo.UpdateTime;
            tbUpdatetime.Foreground = msgPo.UpdateTimeBrush;
            
            border.Background = msgPo.ShowBG;
            border.BorderBrush = new SolidColorBrush(Colors.Transparent);
            
            if (msgPo.Level.Equals("1")) //紧急
            {
                ca.From = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50E63034");
                ca.To = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#20E63034");
                
            }
            else if (msgPo.Level.Equals("2")) //警告
            {
                ca.From = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
                ca.To = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");
            }
            else if (msgPo.Level.Equals("3")) //普通
            {
                ca.From = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
                ca.To = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");
            }

            if (!string.IsNullOrEmpty(msgPo.CloseBy))
            {
                this.Height = 88;
            }
            else
                this.Height = 75;

            if (msgPo.MsgType.Equals("1"))
                btnPopDeal.Visibility = Visibility.Collapsed;
            else
                btnPopDeal.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(msgPo.readFlag) && msgPo.readFlag.Contains(LocalEnv.UserInfo.UserId))
            {
                btnPopKnow.Visibility = Visibility.Collapsed;
            }
            else
                btnPopKnow.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(msgPo.CloseBy))
            {
                this.grid.RowDefinitions[1].Height = new GridLength(0);
                tbCloseBy.Visibility = Visibility.Collapsed;
            }

            sb.Begin();
        }

        public void FloatAction(bool isRun)
        {
            if (sb != null)
            {
                if (isRun)
                    sb.Begin();
                else
                    sb.Stop();
            }
        }

        public event Action<bool> OnStoryboardComplete;
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            OnStoryboardComplete?.Invoke(true);

            sb = null;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (OnStoryboardComplete != null)
            //    OnStoryboardComplete = null;
        }

        private void btnPopDeal_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPopKnow_Click(object sender, RoutedEventArgs e)
        {
            if (msgPo != null)
                this.KnowMsg(msgPo);
        }

        private void KnowMsg(MsgPo msgPo)
        {
            if (msgPo != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        AdminHandler.Instance.UpdateSysMsg(ObjectConvert.MsgPoToSystemMessageReq(msgPo, msgPo.MsgType));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                });
            }
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (msgPo != null)
            {
                border.BorderBrush = msgPo.ShowBorderBrush;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //border.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
    }
}
