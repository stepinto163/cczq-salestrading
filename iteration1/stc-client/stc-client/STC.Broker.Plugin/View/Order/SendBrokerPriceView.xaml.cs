using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using STC.Common.ViewModel.Order;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Broker.Plugin.ViewModel.Order;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Timers;

namespace STC.Broker.Plugin.View.Order
{
    /// <summary>
    /// SendBrokerPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendBrokerPriceView : Window
    {
        private SendBrokerPriceViewModel viewModel = null;

        public SendBrokerPriceView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
            viewModel = new SendBrokerPriceViewModel();
            this.DataContext = viewModel;

            if (timer == null)
            {
                // 创建一个Timer对象，参数是间隔时间（毫秒）
                timer = new Timer(300);
                // 注册Elapsed事件
                timer.Elapsed += Timer4Refresh_Tick;
                // 启动定时器
                timer.Start();
            }
        }


        #region 刷新闪烁
        static Timer timer;
        private DispatcherTimer Timer4Refresh;
        private void Timer4Refresh_Tick(object sender, EventArgs e)
        {
            // 长时间运行的任务
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 执行操作
                foreach (GoodsVolumeVoModel gvv in viewModel.BestQuotations)
                {
                    if (gvv.FlashCount > 0)
                        gvv.FlashCount--;
                }
            });
        }

        #endregion

        public void Init(GoodsPo goodinfo, ObservableCollection<GoodsVolumeVoModel> lstgoodsvolume, bool isBid, decimal price, string quoteid, string selsettle, string id = null)
        {
            //拆分settle,判断是否含远期，+0 ，+1
            string security = goodinfo.GoodsCode;

            //获取清算速度列表
            ObservableCollection<GoodsVolumeVoModel> tm2p = new ObservableCollection<GoodsVolumeVoModel>(lstgoodsvolume.Where(o=>o.MDPriceLevel != 999 && o.DeleteFlag == "0" &&  o.PriceFormat == Frame.Utils.MathEx.PriceFormat(price)));
            List<string> listSettle = viewModel.GetSettleList(tm2p);

            foreach (GoodsVolumeVoModel gvv in lstgoodsvolume)
            {
                gvv.FlashCount = 0;
            }
            //如果清算速度选择+1，+0 则不显示远期，反之显示远期
            if (!(selsettle == "2,3" || selsettle == "1"))
            {
                // 判断是否是多远期
                Style s = this.FindResource("SelfFilterListBoxItemDodgerBlueStyle") as Style;
                Thickness margin = new Thickness(0, 0, 4, 4);
                FontFamily fn = new FontFamily("Microsoft YaHei");
                FontWeight fw = FontWeights.SemiBold;
                //如果含有远期，则自动添加远期选项
                foreach (string tmp in listSettle)
                {
                    if (tmp.Contains("."))
                    {
                        ListBoxItem yuanitem = new ListBoxItem() { Content = tmp, Style = s, Width = 60, Height = 24, Margin = margin, ToolTip = "REQ", FontFamily = fn, FontWeight = fw, FontSize = 14, Tag = tmp };
                        ToolTipService.SetIsEnabled(yuanitem, false);
                        yuanitem.SetBinding(ListBoxItem.IsEnabledProperty, new Binding("IsEnableConfirm") { Source = viewModel });
                        LstSettleBox.Items.Add(yuanitem);
                    }
                }
            }

            //获取选择的清算速度

            List<string> selsettlelist = selsettle.Split(',').ToList<string>();

            foreach (ListBoxItem li in LstSettleBox.Items)
            {
                if (selsettlelist.Contains(li.Tag.ToString()))
                {
                    li.IsSelected = true;
                }
            }

            if (selsettle == "1,2,3")
            {
                LstSettleBoxNoLimit.IsChecked = true;
            }

            viewModel.BestQuotations = lstgoodsvolume;

            ////viewModel.SelSettleList = selsettlelist;
            viewModel.Init(this, goodinfo, lstgoodsvolume, isBid, price, quoteid, selsettlelist, id);
            //this.Activated += SendPointPriceView_Activated;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

       
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                    viewModel.OkCommand.Execute(null);
            }

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                if (viewModel != null && viewModel.CancelCommand.CanExecute(null))
                    viewModel.CancelCommand.Execute(null);
            }
        }

        public void SetQtyFocus()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int sellength = AmountTxt.Text.Length;
                AmountTxt.Focus();
                AmountTxt.SelectionStart = 0;
                AmountTxt.SelectionLength = sellength;
                this.Show();
                this.Activated += Window_Activated;
            });
        }

        #region 修改获取焦点后和失去焦点边框色
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendBrokerPriceViewModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
            }
        }


        private void Window_Activated(object sender, EventArgs e)
        {
            this.Activated -= Window_Activated;
            if ((this.viewModel != null) && (viewModel is SendBrokerPriceViewModel))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
                SetQtyFocus();
            }
        }
        #endregion

        #region Qty
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;
            decimal max = 1000.00M;
            decimal min = 0M;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            if (tBox.Name.ToString() == "AmountTxt")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.OnAddQtyCommand(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.OnReduceQtyCommand(this);
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Escape))
            {
                viewModel.OnCancelCommand(this);
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                viewModel.OnOkCommand(this);
                e.Handled = true;
                return;
            }

            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex(@"^[0-9]\d*$");
            if (LocalEnv.UserInfo.CompanyId == "4")
            {
                regex = new Regex("^[0-9.-]+$");
            }
            
            decimal max = 1000M;
            decimal min = 0M;

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".") && viewModel.QtyTxt == viewModel.TXT_IRS) || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }


            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

            }
            else
            {
                e.Handled = true;
                return;
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min) && d != 0)
            {
                e.Handled = true;
                return;
            }

            if (LocalEnv.UserInfo.CompanyId == "4")
            {
                string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4 && viewModel.QtyTxt != viewModel.TXT_IRS)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }

            e.Handled = false;
        }

        #endregion

        #region 清算速度
        private void LstSettleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                List<string> listSettle = new List<string>();
                ListBox listBox = sender as ListBox;

                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (lbi.IsSelected)
                    {
                        if (lbi.Tag.ToString() == "1,2,3")
                        {
                            listSettle.Add("1");
                            listSettle.Add("2");
                            listSettle.Add("3");
                        }
                        else
                        {
                            listSettle.Add(lbi.Tag.ToString());
                        }
                    }

                    if (lbi.Tag.ToString() == "1,2,3")
                    {
                        LstSettleBoxNoLimit.IsChecked = lbi.IsSelected;
                    }
                }

                if (listSettle.Select(o => o.Contains("+")).Count() > 1)
                {
                    foreach (ListBoxItem lbi in listBox.Items)
                    {
                        if (lbi.IsSelected && lbi.Tag.ToString().Contains("+"))
                        {
                            if (lbi.Tag.ToString() == listSettle[1].ToString())
                            {
                                lbi.IsSelected = false;
                                break;
                            }
                        }
                    }
                }

                listSettle.Clear();
                foreach (ListBoxItem lbi in listBox.SelectedItems)
                {
                    if (lbi.Tag.ToString() == "1,2,3")
                    {
                        listSettle.Add("1");
                        listSettle.Add("2");
                        listSettle.Add("3");
                    }
                    else
                    {
                        listSettle.Add(lbi.Tag.ToString());
                    }
                }

                viewModel.SelSettleList = listSettle.Distinct().ToList();
            }
        }

        /// <summary>
        /// 不限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstSettleBoxNoLimit_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                LstSettleBox.SelectionChanged -= LstSettleBox_SelectionChanged;
                CheckBox chk = sender as CheckBox;
                bool ischeck = chk.IsChecked ?? false;
                if (ischeck)
                {
                    foreach (ListBoxItem lbi in LstSettleBox.Items)
                    {
                        if (lbi.Tag.ToString() == "1,2,3")
                        {
                            lbi.IsSelected = true;
                        }
                        else
                        {
                            lbi.IsSelected = false;
                        }
                    }
                }
                else
                {
                    foreach (ListBoxItem lbi in LstSettleBox.Items)
                    {
                        if (lbi.Tag.ToString() == "1,2,3")
                        {
                            lbi.IsSelected = false;
                            break;
                        }
                    }
                }

                if (ischeck)
                {
                    List<string> listSettle = new List<string>() { "1", "2", "3" };

                    if (LstSettleBox.Items.Count > 4)
                    {
                        foreach (ListBoxItem li in LstSettleBox.Items)
                        {
                            if (li.Tag.ToString() != "1,2,3" && li.Tag.ToString() != "1" && li.Tag.ToString() != "2" && li.Tag.ToString() != "3")
                            {
                                listSettle.Add(li.Tag.ToString());
                            }
                        }
                    }

                    viewModel.SelSettleList = listSettle.Distinct().ToList();
                }


                LstSettleBox.SelectionChanged += LstSettleBox_SelectionChanged;
            }
        }

        #endregion

       
    }
}
