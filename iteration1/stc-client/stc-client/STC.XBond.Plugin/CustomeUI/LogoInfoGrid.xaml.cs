using Atf.MQIII;
using STC.Common.Api;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace STC.XBond.Plugin.CustomUI
{
    /// <summary>
    /// LogoInfoGrid.xaml 的交互逻辑
    /// </summary>
    public partial class LogoInfoGrid : UserControl
    {
        private readonly string ErrorTxt = "个连线中断，请立即处理。";
        private readonly string OpenTxt = "开市中";
        private readonly string CloseTxt = "闭市中";
        private readonly string ConStatusNormal = "连接正常";
        private readonly string ConStatusError = "连接中断";

        private object _lockObj = new object();

        public LogoInfoGrid()
        {
            InitializeComponent();
            
        }

        

        public event Action<object, RoutedEventArgs> OnMinClick;
        public event Action<object, RoutedEventArgs> OnMaxClick;
        public event Action<object, RoutedEventArgs> OnCloseClick;

        public void SubscribeConStatus()
        {
            InitGridUI();

            TBookHandler.Instance.SubscribeConStatus(Instance_OnConStatus);

            Atf.MQIII.MQIIIManager.Instance.MQConnectionEvent += Instance_MQConnectionEvent;
            
        }

        private void InitGridUI()
        {
            CustomStatusShow(ApiConsts.Trade, gridTrader);
            CustomStatusShow(ApiConsts.Price, gridPrice);
            CustomStatusShow(ApiConsts.Inner, gridInner);
        }

        private void CustomStatusShow(string targetSessionId, Grid grid)
        {
            int columnIndex = 0;
            int rowIndex = 0;
            List<ServerStatus> serverStatus = LocalEnv.ConfInfo.ServerStatus.Where(delegate (ServerStatus ss)
              {
                  if (ss.TargetSessionId.ToUpper().Equals(targetSessionId.ToUpper()))
                      return true;
                  else
                      return false;
              }).ToList();

            int rowTotal = (serverStatus.Count % 3) == 0 ? serverStatus.Count % 3 : serverStatus.Count % 3 + 1;

            for (int i = 0; i < rowTotal; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }



            foreach (ServerStatus ss in serverStatus)
            {
                ServerStatusUI statusUI = new ServerStatusUI();
                statusUI.Tag = ss;
                statusUI.SetShowTxt(ss.Name);
                statusUI.SetShowBg((Brush)this.FindResource("ColorBrush.Red7"), (Brush)this.FindResource("ColorBrush.Red7"));

                grid.Children.Add(statusUI);

                Grid.SetColumn(statusUI, columnIndex);
                Grid.SetRow(statusUI, rowIndex);

                columnIndex++;

                if (columnIndex == 3)
                {
                    rowIndex++;
                    columnIndex = 0;
                }
            }
            tbError.Text = string.Format("{0}{1}", serverStatus.Count, ErrorTxt);
        }


        private void Instance_MQConnectionEvent(string appName, Atf.MQIII.MQIIIEventStatus status)
        {

            this.Dispatcher.Invoke(() =>
            {
                lock (_lockObj)
                {
                    switch (status)
                    {
                        case MQIIIEventStatus.CONNECTION_CLOSED:

                            bdSTC.Background = (Brush)this.FindResource("ColorBrush.Red7");
                            tbSTC.Foreground = (Brush)this.FindResource("ColorBrush.Red7");
                            int count = 1;
                            foreach (UIElement ui in gridPrice.Children)
                            {
                                ServerStatusUI ss = ui as ServerStatusUI;
                                if (ss != null)
                                {
                                    ss.SetShowBg(bdSTC.Background, tbSTC.Foreground);
                                    count++;
                                }
                            }

                            foreach (UIElement ui in gridTrader.Children)
                            {
                                ServerStatusUI ss = ui as ServerStatusUI;
                                if (ss != null)
                                {
                                    ss.SetShowBg(bdSTC.Background, tbSTC.Foreground);
                                    count++;
                                }
                            }

                            //popMsg.Height = 158;
                            tbError.Text = string.Format("{0}{1}", count, ErrorTxt);
                            border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red11");
                            grid.RowDefinitions[1].Height = new GridLength(65);
                            tbError.Visibility = Visibility.Visible;

                            tbConStatusTxt.Text = this.ConStatusError;

                            tbConStatusTxt.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");

                            break;
                        case MQIIIEventStatus.CONNECTION_LOGIN_SUCCESS:
                            bdSTC.Background = (Brush)this.FindResource("ColorBrush.Green14");
                            tbSTC.Foreground = (Brush)this.FindResource("ColorBrush.While");
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        #region 链接状态更新

        private Brush getShowBG(string status)
        {
            Brush result = null;
            if (status.Equals("1"))
                result = (Brush)this.FindResource("ColorBrush.Green14");
            else
            {
                result = (Brush)this.FindResource("ColorBrush.Red7");
            }
            return result;
        }

        private Brush getShowForground(string status)
        {
            Brush result = null;
            if (status.Equals("1"))
                result = (Brush)this.FindResource("ColorBrush.While");
            else
                result = (Brush)this.FindResource("ColorBrush.Red7");

            return result;
        }

        private ServerStatusUI GetServerStatusUI(SessionStatus sessionStatus)
        {
            ServerStatusUI result = null;

            Grid grid = null;
            if (sessionStatus.TargetSessionId.ToUpper().Equals(ApiConsts.Trade.ToUpper()))
                grid = gridTrader;
            else
                grid = gridPrice;

            foreach (UIElement ui in grid.Children)
            {
                ServerStatusUI ss = ui as ServerStatusUI;
                if (ss != null)
                {
                    ServerStatus serverStatus = ss.Tag as ServerStatus;
                    if (serverStatus != null && serverStatus.TargetSessionId.ToUpper().Equals(sessionStatus.TargetSessionId.ToUpper())
                        && serverStatus.VenueTypeGW.ToUpper().Equals(sessionStatus.VenueTypeGW.ToUpper()))
                    {
                        result = ss;
                        break;
                    }
                }
            }

            return result;
        }

        private void Instance_OnConStatus(List<SessionStatus> sessionStatus)
        {
            if (sessionStatus != null && sessionStatus.Count > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    lock (_lockObj)
                    {
                        int count = 0;
                        bool needRefresh = false;
                        foreach (SessionStatus ss in sessionStatus)
                        {
                            Brush brushBg = getShowBG(ss.NetStatus);
                            Brush brushForbg = getShowForground(ss.NetStatus);

                            ServerStatusUI serverStatusUI = GetServerStatusUI(ss);

                            if (serverStatusUI != null)
                            {
                                serverStatusUI.SetShowBg(brushBg, brushForbg);

                                needRefresh = true;
                            }
                        }


                        if (needRefresh)
                        {
                            Brush brush = (Brush)this.FindResource("ColorBrush.Red7");

                            foreach (UIElement ui in gridTrader.Children)
                            {
                                ServerStatusUI ss = ui as ServerStatusUI;
                                if (ss != null)
                                {
                                    if (ss.CheckStatus(brush))
                                        count++;
                                }
                            }

                            foreach (UIElement ui in gridPrice.Children)
                            {
                                ServerStatusUI ss = ui as ServerStatusUI;
                                if (ss != null)
                                {
                                    if (ss.CheckStatus(brush))
                                        count++;
                                }
                            }

                            if (count > 0)
                            {
                                tbError.Text = string.Format("{0}{1}", count, ErrorTxt);
                                tbConStatusTxt.Text = this.ConStatusError;
                                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Red11");
                                tbError.Visibility = Visibility.Visible;
                                grid.RowDefinitions[1].Height = new GridLength(65);
                                tbConStatusTxt.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Red7");
                            }
                            else
                            {
                                border.BorderBrush = (Brush)this.FindResource("ColorBrush.Gray14");
                                tbError.Visibility = Visibility.Collapsed;
                                grid.RowDefinitions[1].Height = new GridLength(30);
                                tbConStatusTxt.Text = this.ConStatusNormal;
                                tbConStatusTxt.Background = bdStatus.Background = (Brush)this.FindResource("ColorBrush.Green13");
                            }
                        }
                    }
                });
            }
        }
        #endregion

        public void setUserName(string txt)
        {
            this.Dispatcher.Invoke(() => 
            {
                tbUserName.Text = txt;
            });
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            OnMinClick?.Invoke(sender, e);
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            OnMaxClick?.Invoke(sender, e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseClick?.Invoke(sender, e);
        }

        private void btnStatus_MouseEnter(object sender, MouseEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (!ifLeft)
            {
                popMsg.HorizontalOffset = (popMsg.Width - bdStatus.Width)*(-1);
            }

            popMsg.IsOpen = true;
        }

        private void btnStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            //Console.WriteLine("popMsg_MouseLeave, x:" + p.X + ", Y:" + p.Y);

            if (p.X > bdStatus.Width || p.X < 0 || p.Y > bdStatus.Height || p.Y < 0)
            {
                popMsg.IsOpen = false;
            }

        }
        
    }
}
