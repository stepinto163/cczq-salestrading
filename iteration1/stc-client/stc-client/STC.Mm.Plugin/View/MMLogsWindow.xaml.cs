using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.MM;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Mm.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// MMLogs.xaml 的交互逻辑
    /// </summary>
    public partial class MMLogsWindow : Window
    {
        public Window myWindow;
        public MMLogsWindow()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            myWindow = this;
            this.Loaded += PopupNoBorder_Loaded;
            this.Unloaded += PopupNoBorder_Unloaded;
            //this.Topmost = true;
            CommonUtils.SetIcon(TitleIcon);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            btnClose_Click(null, null);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                this.Close();
            }
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var window = Window.GetWindow(this);//获取当前主窗口
                    var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                    System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                    Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                    PresentationSource source = PresentationSource.FromVisual(window);

                    Matrix m = source.CompositionTarget.TransformFromDevice;

                    double widthRatio = m.M11;

                    double heightRatio = m.M22;

                    if (this.Width != rect.Width * widthRatio && this.Height != rect.Height * heightRatio)
                    {
                        this.MaxWidth = screen.WorkingArea.Width;
                        this.MaxHeight = screen.WorkingArea.Height;
                        this.Width = rect.Width * widthRatio;
                        this.Height = rect.Height * heightRatio;
                        this.Left = rect.Left == 0 ? 0 : rect.Left * widthRatio;
                        this.Top = rect.Top == 0 ? 0 : rect.Top * heightRatio;

                    }
                    else
                    {
                        this.Top = (screen.WorkingArea.Height - this.Height) / 2 + rect.Top * heightRatio;
                        this.Left = (rect.Width * widthRatio - this.Width) / 2 + rect.Left * widthRatio;
                    }
                }
                catch { }
            }));
        }

        private void PopupNoBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PopupNoBorder_Loaded;
            this.Unloaded -= PopupNoBorder_Unloaded;
            GC.Collect();
        }

        private void PopupNoBorder_Loaded(object sender, RoutedEventArgs e)
        {
            AddWindowsEvent();
        }

        public void AddWindowsEvent()
        {
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        public void RemoveWindowsEvent()
        {
            this.MouseLeftButtonDown -= Window_MouseLeftButtonDown;
        }

        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    MMLogsWindow po = (MMLogsWindow)sender;
                    var p = e.GetPosition(po);
                    if (p.Y > 0 && p.Y <= 30)
                    {
                        this.DragMove();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                catch { }
            }));
        }


        public async void SetCurrentMM(DataRowView drv)
        {
            this.fd.Blocks.Clear();
            if (drv != null)
            {
                string securtityID = drv[MMListBoxViewModel.SECURITY_ID_FIELD].ConvertToString();
                string ansStatus = drv[MMListBoxViewModel.ANS_MM_STATUS_FIELD].ConvertToString();
                string venue = drv[MMListBoxViewModel.VENUES_FIELD].ConvertToString();
                string venuetypegw = drv[MMListBoxViewModel.VENUETYPEGW_FIELD].ConvertToString();
                string username = drv[MMListBoxViewModel.USERNAME_FIELD].ConvertToString();

                this.TextBlockSecurity.Text = securtityID;
                if (ansStatus == "4")
                {
                    this.TextBlockStatus.Foreground = (Brush)this.FindResource("failure");
                    this.TextBlockStatus.Text = "(失败)";
                }
                else if (ansStatus == "3")
                {
                    this.TextBlockStatus.Foreground = (Brush)this.FindResource("standard");
                    this.TextBlockStatus.Text = "(已达标)";
                }
                else if (ansStatus == "2")
                {
                    this.TextBlockStatus.Foreground = (Brush)this.FindResource("opportunity");
                    this.TextBlockStatus.Text = "(可达标)";
                }
                else if (ansStatus == "1")
                {
                    this.TextBlockStatus.Foreground = (Brush)this.FindResource("danger");
                    this.TextBlockStatus.Text = "(危险)";
                }
                else
                {
                    this.TextBlockStatus.Text = ansStatus;
                }

                QueryMMSecurityidHistRequest reqeust = new QueryMMSecurityidHistRequest();
                reqeust.venuetypegw = venuetypegw;
                reqeust.venues = venue;
                reqeust.username = username;
                reqeust.securityid = securtityID;

                QueryMMSecurityidHist(reqeust);

               
            }
        }

        private QueryMMSecurityidHistReplyPo QueryMMSecurityidHist(QueryMMSecurityidHistRequest reqeust)
        {
            QueryMMSecurityidHistReplyPo reply = null;
            ThreadPool.QueueUserWorkItem(o =>
            {
                reply = MMHandler.Instance.QueryMMSecurityidHist(reqeust);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (MMSecurityidHist mmSH in reply.Datas)
                        {
                            Paragraph p = new Paragraph();

                            string actiontime = mmSH.actiontime.Substring(11, 8);

                            Run ctRun = new Run(actiontime);
                            ctRun.Foreground = (Brush)this.FindResource("white");
                            p.Inlines.Add(ctRun);

                            Run operationRun = new Run(mmSH.operation);
                            Run sperationRun1 = new Run("    ");
                            Run detailRun;
                            if (mmSH.operation == "正常报价")
                            {
                                string settleType = ComboSourceUtil.Instance.FindEnum(mmSH.beforeSettleType, "settletype_drp");
                                detailRun = new Run($"{mmSH.beforeBidPrice}/{mmSH.beforeOfrPrice} {mmSH.displayBidQty}kw/{mmSH.displayOfrQty}kw {settleType}");
                            }
                            else if (mmSH.operation == "触墙拉宽"
                                || mmSH.operation == "成交拉宽"
                                || mmSH.operation == "中债拉宽")
                            {
                                string settleType = ComboSourceUtil.Instance.FindEnum(mmSH.beforeSettleType, "settletype_drp");
                                detailRun = new Run($"{mmSH.beforeBidPrice}/{mmSH.beforeOfrPrice} {mmSH.displayBidQty}kw/{mmSH.displayOfrQty}kw {settleType}");
                            }
                            else
                            {
                                detailRun = new Run(mmSH.detail);
                            }

                            Run sperationRun2 = new Run("    ");
                            if (mmSH.operation == "正常报价")
                            {

                                operationRun.Foreground = (Brush)this.FindResource("green");
                                detailRun.Foreground = (Brush)this.FindResource("green");
                            }
                            else if (mmSH.operation == "手动开启"
                                || mmSH.operation == "监控市场")
                            {
                                operationRun.Foreground = (Brush)this.FindResource("white");
                                detailRun.Foreground = (Brush)this.FindResource("white");
                            }
                            else if (mmSH.operation == "自动关闭"
                                || mmSH.operation == "手动关闭")
                            {
                                operationRun.Foreground = (Brush)this.FindResource("gray");
                                detailRun.Foreground = (Brush)this.FindResource("gray");
                            }
                            else if (mmSH.operation == "触墙拉宽"
                                || mmSH.operation == "成交拉宽"
                                || mmSH.operation == "主动点价"
                                || mmSH.operation == "中债拉宽")
                            {
                                if (mmSH.detail.Contains("Ofr"))
                                {
                                    operationRun.Foreground = (Brush)this.FindResource("blue");
                                    detailRun.Foreground = (Brush)this.FindResource("blue");
                                }
                                else
                                {
                                    operationRun.Foreground = (Brush)this.FindResource("orange");
                                    detailRun.Foreground = (Brush)this.FindResource("orange");
                                }

                            }
                            else if (mmSH.operation == "报价中")
                            {
                                operationRun.Foreground = (Brush)this.FindResource("red");
                                detailRun.Foreground = (Brush)this.FindResource("red");
                            }

                            p.Inlines.Add(sperationRun1);
                            p.Inlines.Add(operationRun);
                            p.Inlines.Add(sperationRun2);
                            p.Inlines.Add(detailRun);
                            this.fd.Blocks.Add(p);
                        }
                    }));
                }
            });
            return reply;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
    }
}
