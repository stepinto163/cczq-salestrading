using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Inner;
using STC.Common.Model.Admin;
using STC.Common.Model.Inner;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
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

namespace STC.XBond.Plugin.CustomUI
{
    /// <summary>
    /// MsgFloatUI.xaml 的交互逻辑
    /// </summary>
    public partial class MsgFloatUI : UserControl
    {
        private readonly string IDB = "IDB";
        private MsgPo msgPo;
        private Storyboard sb = null;

        private Color _redBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50E63034");
        private Color _redSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#20E63034");

        private Color _orangeBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
        private Color _orangeSmall = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");

        private Color _redBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034");
        private Color _orangeBest = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00");

        public MsgFloatUI()
        {
            InitializeComponent();
        }

        public void Init(MsgPo msgPo)
        {
            this.msgPo = msgPo;

            sb = (Storyboard)this.FindResource("sbFloat");
            ColorAnimation ca = new ColorAnimation();
            ca.Duration = new Duration(TimeSpan.FromSeconds(2));
            sb.Children.Clear();
            sb.Children.Add(ca);

            tbContentType.Foreground = msgPo.ContentTypeBrush;
            tbContentType.Text = msgPo.HeaderTitle;
            tbStatus.Text = msgPo.Status;
            tbStatus.Foreground = msgPo.StatusBrush;
            tbValuetime.Text = msgPo.ValueTime;
            tbValuetime.Foreground = msgPo.ValueTimeBrush;
            tbCloseBy.Text = msgPo.CloseBy;
            tbCloseBy.Foreground = msgPo.CloseByBrush;
            tbContent.Text = msgPo.Content;
            tbContent.Foreground = msgPo.ContentBrush;
            tbContentType.Text = msgPo.HeaderTitle;
            tbContentType.Foreground = msgPo.ContentTypeBrush;
            tbUpdatetime.Text = msgPo.UpdateTime;
            tbUpdatetime.Foreground = msgPo.UpdateTimeBrush;

            border.Background = msgPo.ShowBG;
            border.BorderBrush = new SolidColorBrush(Colors.Transparent);

            if (msgPo.Level.Equals("1")) //紧急
            {
                ca.From = _redBig;//(Color)System.Windows.Media.ColorConverter.ConvertFromString("#50E63034");
                ca.To = _redSmall;//(Color)System.Windows.Media.ColorConverter.ConvertFromString("#20E63034");

            }
            else if (msgPo.Level.Equals("2")) //警告
            {
                ca.From = _orangeBig;// (Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
                ca.To = _orangeSmall;// (Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");
            }
            else if (msgPo.Level.Equals("3")) //普通
            {
                ca.From = _orangeBig;//(Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828");
                ca.To = _orangeSmall;//(Color)System.Windows.Media.ColorConverter.ConvertFromString("#50282828");
            }

            //if (!string.IsNullOrEmpty(msgPo.CloseBy))
            //{
            //    this.Height = 88;
            //}
            //else
                this.Height = 75;

            if (msgPo.MsgType.Equals("1"))
            {
                btnPopDeal.Visibility = Visibility.Collapsed;


                if (msgPo.Level.Equals("1"))
                {
                    tbStatus.Text = "请处理";
                    tbStatus.Width = 50;
                    tbStatus.Foreground = (Brush)this.FindResource("ColorBrush.Red7");// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
                    tbStatus.Visibility = Visibility.Visible;
                }
                else if (msgPo.Level.Equals("2"))
                {
                    tbStatus.Text = "需人工干预";
                    tbStatus.Width = 80;
                    tbStatus.Foreground = (Brush)this.FindResource("ColorBrush.Orange5");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));
                    tbStatus.Visibility = Visibility.Visible;
                }
                if (msgPo.HeaderTitle.Contains("连接中断"))
                    tbStatus.Text = "请处理";
                else if (msgPo.HeaderTitle.Contains("下单失败"))
                    tbStatus.Text = "请知悉";
                else if (msgPo.HeaderTitle.Contains("撤单失败"))
                    tbStatus.Text = "需人工干预";
                else if (msgPo.HeaderTitle.Contains("订单超时"))
                    tbStatus.Text = "需人工干预";
            }
            else
            {
                btnPopDeal.Visibility = Visibility.Visible;

                tbStatus.Text = "请确认firm量";
                tbStatus.Foreground = (Brush)this.FindResource("ColorBrush.Red7");//new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
                tbStatus.Visibility = Visibility.Visible;
                tbStatus.Width = 80;
            }

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

            PriceMatchDetail priceMatchDetail = null;
            if (msgPo.Detail != null)
            {
                priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);

                if (!string.IsNullOrEmpty(msgPo.Venue) && msgPo.Venue.Equals(IDB))
                {
                    btnPopDeal.Visibility = Visibility.Collapsed;
                    btnLook.Visibility = Visibility.Visible;
                }
                else
                {
                    btnLook.Visibility = Visibility.Collapsed;
                    btnPopDeal.Visibility = Visibility.Visible;
                }
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
            if (msgPo != null)
            {
                if (msgPo.ContentType.Equals(AdminConsts.Admin_ER))
                {
                    ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
                    executeReplyView.Show();
                }
                else if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    PriceMatchDetail priceMatch = null;
                    if (!string.IsNullOrEmpty(msgPo.Detail))
                    {
                        priceMatch = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);
                    }

                    if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetExecBackWinKey(priceMatch != null && priceMatch.iocOrder != null ? priceMatch.iocOrder.OrderId : string.Empty)))
                    {
                        NofirmView nofirmView = new NofirmView();
                        nofirmView.Init(priceMatch.iocOrder, priceMatch.order);

                        WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetExecBackWinKey(priceMatch != null && priceMatch.iocOrder != null ? priceMatch.iocOrder.OrderId : string.Empty), nofirmView);
                    }
                }
                else
                {
                    PriceMatchView priceMatchView = new PriceMatchView(msgPo);
                    priceMatchView.Show();
                }
            }
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
                if (msgPo.ContentType.Equals(AdminConsts.Admin_IT))
                {
                    PriceMatchDetail priceMatch = null;
                    if (!string.IsNullOrEmpty(msgPo.Detail))
                    {
                        priceMatch = MsgHelper.Deserialize<PriceMatchDetail>(msgPo.Detail);
                    }

                    if (priceMatch != null && priceMatch.iocOrder != null)
                    {

                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                InnerOrderReq innerOrderReq = new InnerOrderReq();
                                innerOrderReq.OrderID = priceMatch.iocOrder.OrderId;

                                InnerHandler.Instance.FakCancelOrder(innerOrderReq);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }

                        });
                    }
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            TBatchMsgReq batchMsgReq = new TBatchMsgReq();
                            batchMsgReq.delflag = "1";
                            batchMsgReq.readflag = "1";
                            batchMsgReq.id = new List<string>();

                            batchMsgReq.id.Add(msgPo.Id);

                            AdminHandler.Instance.BatchUpdateSysMsg(batchMsgReq);


                        //AdminHandler.Instance.UpdateSysMsg(ObjectConvert.MsgPoToSystemMessageReq(msgPo, msgPo.MsgType));
                    }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }

                    });
                }
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

        private void btnLook_Click(object sender, RoutedEventArgs e)
        {
            ExecuteReplyView executeReplyView = new ExecuteReplyView(msgPo);
            executeReplyView.Show();
        }
    }
}
