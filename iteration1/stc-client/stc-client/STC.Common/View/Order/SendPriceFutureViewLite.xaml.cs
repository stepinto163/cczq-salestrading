using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Position;
using STC.Common.Handler.TBook;
using STC.Common.Model.Position;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Order;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace STC.Common.View
{
    /// <summary>
    /// SendPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class SendPriceFutureViewLite : Window
    {
        private Thickness BORDER_THICKNESS_NONE = new Thickness(0, 0, 0, 0);
        //提示框粗细
        private Thickness BORDER_THICKNESS_TIP = new Thickness(2, 2, 2, 2);
        private SendPriceFutureViewModelLite viewModel = null;

        public SendPriceFutureViewLite()
        {
            InitializeComponent();

        }

        public SendPriceFutureViewLite(PriceMatchParam priceMatchParam)
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;


            viewModel = new SendPriceFutureViewModelLite();
            viewModel.Init(this, priceMatchParam);

            this.DataContext = viewModel;
            this.Activated += SendPriceFutureViewLite_Activated;
        }

        private void SendPriceFutureViewLite_Activated(object sender, EventArgs e)
        {
            SetPriceFocus();
        }

        public void SetPriceFocus()
        {
            // 选择最后位数
            int pos = PriceTxt.Text.IndexOf('.');
            if (pos > 0)
            {
                int sellength = PriceTxt.Text.Length - pos + 1;
                PriceTxt.Focus();
                PriceTxt.SelectionStart = pos + 1;
                PriceTxt.SelectionLength = sellength;
            }
            else
            {
                PriceTxt.Focus();
            }
        }

        public SendPriceFutureViewLite(string security, bool isBid)
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            viewModel = new SendPriceFutureViewModelLite();
            if (security.ToLower() == "tradebook")
            {
                security = "";
                Search_TextBox.HintText = "请输入债券代码";
            }

            //请输入债券代码
            viewModel.Init(this, security, isBid);
            this.DataContext = viewModel;

            this.Activated += SendPriceFutureViewLite_Activated;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PART_Button_Click(object sender, RoutedEventArgs e)
        {
            bool ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                Button btn = (Button)sender;
                Grid g = btn.Parent as Grid;
                if (g != null)
                {
                    Popup popup = g.FindName("PART_Popup") as Popup;
                    popup.HorizontalOffset = 103;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "PriceTxt") || (tb.Name == "netPrice"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            else if (tag == "4")//随机渠道
            {
                max = viewModel.RandomMax;
                min = 1;
            }

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
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

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }
            decimal price = d;
            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                //小数点后面不能输入4位小数
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 3)
                {
                    e.Handled = true;
                    return;
                }
                //输入数值不是最小价格变动单位的倍数
                decimal remainder = price % 0.005m;
                if (remainder != 0)
                {
                    e.Handled = true;
                    return;
                }
            }

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void AmountTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            // 增加 ↑↓来改变价格
            if (tBox.Name.ToString() == "PriceTxt")
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddPriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReducePriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "AmountTxt")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    viewModel.ReduceAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                AmountTxt.Focus();
                // 选择最后位数
                int pos = AmountTxt.Text.Length;
                if (pos > 0)
                {
                    int sellength = AmountTxt.Text.Length;
                    AmountTxt.SelectionStart = 0;
                    AmountTxt.SelectionLength = sellength;
                }
                e.Handled = true;
                return;
            }
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }


                // tag 改变用途
                if (tBox.Name == "AmountTxt")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "PriceTxt") || (tBox.Name == "netPrice"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
                }
                else if (tag == "2")//价格
                {
                    max = 9999;
                    min = -100;
                }
                else if (tag == "3")//过期时间
                {
                    max = 120;
                    min = 1;
                }
                else if (tag == "4")//随机渠道
                {
                    max = viewModel.RandomMax;
                    min = 1;
                }

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }


        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbAmount = sender as TextBox;
            // tag 改变用途
            string tag = "1";
            int intAmount = 0;
            int.TryParse(tbAmount.Text, out intAmount);

            int max = 1000;
            int min = 0;

            if (tag == "1")//量
            {
                max = 1000;
                min = 0;
            }

            if (intAmount > max || intAmount < min)
            {
                viewModel.SendPriceEnable = false;
            }
            else
            {
                viewModel.SendPriceEnable = true;
            }

            viewModel.SendPriceItem.Amount = intAmount == 0 ? tbAmount.Text : intAmount.ToString();
            viewModel.SetAmountTxtSelectIndex();
            //if (intAmount % 1000 != 0)
            //{
            //    viewModel.SendPriceEnable = false;
            //}

            ////TextBox tbAmount = sender as TextBox;
            ////string tag = tbAmount.Tag as string;
            ////int intAmount = 0;
            ////int.TryParse(tbAmount.Text, out intAmount);
            viewModel.ResetQtyWarn(tag, intAmount.ToString());

        }

        private void LbFollowingPrice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //使用跟盘价CheckBox
            if (viewModel.FollowingPriceCheck)
            {
                if (sender != null && sender is ListBox)
                {
                    ListBox listBox = sender as ListBox;
                    ListBoxItem lbi = listBox.SelectedItem as ListBoxItem;
                    //对价
                    if (Consts.BIDOFRPRICE.Equals(lbi.Content))
                    {
                        viewModel.SendPriceItem.CustomPrice = viewModel.IsBid ? viewModel.SendPriceItem.BestOfrStr : viewModel.SendPriceItem.BestBidStr;
                        viewModel.SendPriceItem.PriceType = viewModel.IsBid ? 1 : 4;
                    }
                    //最新价
                    else if (Consts.LATESTPRICE.Equals(lbi.Content))
                    {
                        viewModel.SendPriceItem.CustomPrice = viewModel.SendPriceItem.LastPriceStr;
                        viewModel.SendPriceItem.PriceType = viewModel.IsBid ? 2 : 5;
                    }
                    //市价
                    else if (Consts.MARKETPRICE.Equals(lbi.Content))
                    {
                        viewModel.SendPriceItem.CustomPrice = viewModel.IsBid ? viewModel.SendPriceItem.UpperLimitPriceStr : viewModel.SendPriceItem.LowerLimitPriceStr;
                        viewModel.SendPriceItem.PriceType = viewModel.IsBid ? 3 : 6;
                    }
                }
            }
        }

        private void bdCapAccountComboBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel != null)
            {
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //判断用户的按键是否为Enter
                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    if (viewModel != null && viewModel.OkCommand.CanExecute(null))
                        viewModel.OkCommand.Execute(null);

                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SendPriceFutureViewModelLite viewModel = (SendPriceFutureViewModelLite)this.DataContext;
            viewModel.Dispose();
        }

        private void dgPrice_Unloaded(object sender, RoutedEventArgs e)
        {
            //Card.PriceAndOrderView po = (Card.PriceAndOrderView)dgPrice;
            //po.CloseViewModel();
            GC.Collect();
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            SendPriceFutureViewModelLite viewModel = (SendPriceFutureViewModelLite)this.DataContext;
            if (string.IsNullOrEmpty(Search_TextBox.SearchText))
            {
                Search_TextBox.SearchText = string.Empty;
                return;
            }

            string newSecurityId = Search_TextBox.SearchText;
            viewModel.SearchText = newSecurityId;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPriceFutureViewModelLite))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
                //LineSplit.Stroke = (Brush)viewModel.myWindow.FindResource("LostFocusBorderBrush");
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if ((this.viewModel != null) && (viewModel is SendPriceFutureViewModelLite))
            {
                viewModel.BorderBrush = (Brush)viewModel.myWindow.FindResource("GetFocusBorderBrush");
            }
        }

        public void SetButtonEnable(string btnname, bool isenable)
        {
            switch (btnname.ToLower())
            {
                case "bidbtn":
                    this.BidBtn.IsEnabled = isenable;
                    break;
                case "ofrbtn":
                    this.OfrBtn.IsEnabled = isenable;
                    break;
                case "search_textbox":
                    this.Search_TextBox.IsEnabled = isenable;
                    break;
                case "btnxbond":
                    //this.btnXBond.IsEnabled = isenable;
                    break;
            }

        }

        private void bdTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbOrderType.IsDropDownOpen = !cbOrderType.IsDropDownOpen;
        }

        //跟盘价 选中
        private void followingPrice_Checked(object sender, RoutedEventArgs e)
        {
            FollowingPrice1.IsSelected = true;
            FollowingPrice5.IsSelected = false;
            FollowingPrice10.IsSelected = false;
            ListBox listBox = viewModel.myWindow.FindName("lbFollowingPrice") as ListBox;
            ListBoxItem lbi = listBox.SelectedItem as ListBoxItem;
            if (Consts.BIDOFRPRICE.Equals(lbi.Content))
            {
                //viewModel.CustomPrice = viewModel.IsBid? string.Format("{0}{1}", viewModel.SendPriceItem.BestOfrInt, viewModel.SendPriceItem.BestOfrFloat) : string.Format("{0}{1}", viewModel.SendPriceItem.BestBidInt, viewModel.SendPriceItem.BestBidFloat);
                viewModel.SendPriceItem.CustomPrice = viewModel.IsBid ? viewModel.SendPriceItem.BestOfrStr : viewModel.SendPriceItem.BestBidStr;
            }
            viewModel.SendPriceItem.FollowingPrice = true;
            viewModel.SendPriceItem.PriceType = viewModel.IsBid ? 1 : 4;
        }

        //跟盘价 未选中
        private void followingPrice_UnChecked(object sender, RoutedEventArgs e)
        {
            FollowingPrice1.IsSelected = false;
            FollowingPrice5.IsSelected = false;
            FollowingPrice10.IsSelected = false;
            viewModel.SendPriceItem.FollowingPrice = false;
            viewModel.SendPriceItem.PriceType = viewModel.IsBid ? 1 : 4;
        }


        private void bdUpperLimitPriceTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SendPriceItem.CustomPrice = viewModel.SendPriceItem.UpperLimitPriceStr;
        }

        private void bdLowerLimitPriceTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SendPriceItem.CustomPrice = viewModel.SendPriceItem.LowerLimitPriceStr;
        }

        private void bdBestBidPriceTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SendPriceItem.CustomPrice = viewModel.SendPriceItem.BestBidStr;
        }

        private void bdBestOfrPriceTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SendPriceItem.CustomPrice = viewModel.SendPriceItem.BestOfrStr;
        }

        private void lbOpenClose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.ResetQtyWarn();
        }

        //选中自动开平仓时，获取当前持仓
        private void lbOpenClose_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                // ListBox item clicked - do some things here
                string name = item.Name;
                if ("OpenCloseAuto".Equals(name))
                {
                    if (cbBookId.SelectedIndex == -1)
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        if (IsActive)
                        {
                            msgBox.Owner = this;
                        }
                        msgBox.setWarnMsg("选择自动开平仓请先选择持仓组合.");
                        msgBox.ShowDialog();
                        return;
                    }
                    string bookid = (string)cbBookId.SelectedValue;
                    QueryPositionReq req = new QueryPositionReq();
                    req.bookid = bookid;
                    req.securityid = viewModel.SendPriceItem.Security;
                    QueryPositionResult reply = GetPositionDetailData(req);
                    if (reply != null)
                    {
                        if (reply.Datas != null && reply.Datas.Count > 0)
                        {
                            foreach (PositionDetailModel pdm in reply.Datas)
                            {
                                decimal position = pdm.netposition;
                                if (position.CompareTo(decimal.Zero) > 0)
                                {
                                    viewModel.OffsetFlagAuto = "1"; //有持仓自动为平仓
                                }
                            }
                        }
                        else
                        {
                            viewModel.OffsetFlagAuto = "0";
                        }
                    }
                }
            }
        }

        public OrderMassCancelReplyPo CancelOrderBatch(OrderMassCancelRequest req)
        {
            OrderMassCancelReplyPo reply = null;
            reply = TBookHandler.Instance.CancelOrderBatch(req);
            return reply;
        }

        public QueryPositionResult GetPositionDetailData(QueryPositionReq req)
        {
            QueryPositionResult reply = null;
            reply = PositionHandler.Instance.QueryPositionDetail(req);
            return reply;
        }

        private void btnTen_OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            Button chb = CommonUtils.GetVisualChild<Button>(DepthDataGrid, v => v.Name == "btnTen");
            if (null != chb)
            {
                chb.IsEnabled = true;
            }
        }

        private void btnTen_Click(object sender, RoutedEventArgs e)
        {
            Popup pop = CommonUtils.GetVisualChild<Popup>(DepthDataGrid, v => v.Name == "popupTenMenu");
            if (null != pop)
            {
                pop.IsOpen = true;
            }
            Button btn = CommonUtils.GetVisualChild<Button>(DepthDataGrid, v => v.Name == "popupTenMenu");
            if (null != btn)
            {
                btn.IsEnabled = false;
            }
        }

        private void btnTen_Click1(object sender, RoutedEventArgs e)
        {
            Popup pop = CommonUtils.GetVisualChild<Popup>(DepthDataGrid, v => v.Name == "popupTenMenu");
            if (null != pop)
            {
                pop.IsOpen = false;
            }
            if (viewModel != null)
            {
                //10档状态下忽略10档盘口按钮点击
                if (viewModel.DisplayTenTwenty == 10)
                {
                    return;
                }
                viewModel.DisplayTenTwenty = 10;
                Border_Left31.Visibility = Visibility.Visible;
                Border_Left4.Visibility = Visibility.Visible;
                Border_Left6.Visibility = Visibility.Visible;
                //btn_Expand.Content = ">>";
                if (gridDepth != null)
                {
                    gridDepth.Height -= 166 + 150;
                }
                //btn_Expand.Tag = "Visible";
                viewModel.CommonRowNumber = 6;
                viewModel.CommonRowSpan = 1;
                Button btnTen = CommonUtils.GetVisualChild<Button>(DepthDataGrid, v => v.Name == "btnTen");
                if (btnTen != null)
                {
                    btnTen.Content = "10";
                }
                MakeCenter();
            }
        }

        private void btnTen_Click2(object sender, RoutedEventArgs e)
        {
            Popup pop = CommonUtils.GetVisualChild<Popup>(DepthDataGrid, v => v.Name == "popupTenMenu");
            if (null != pop)
            {
                pop.IsOpen = false;
            }
            if (viewModel != null)
            {
                //20档状态下忽略20档盘口按钮点击
                if (viewModel.DisplayTenTwenty == 20)
                {
                    return;
                }
                viewModel.DisplayTenTwenty = 20;
                Border_Left31.Visibility = Visibility.Collapsed;
                Border_Left4.Visibility = Visibility.Collapsed;
                Border_Left6.Visibility = Visibility.Collapsed;
                //btn_Expand.Content = "<<";
                if (gridDepth != null)
                {
                    gridDepth.Height += 166 + 150;
                }
                //btn_Expand.Tag = "Collapsed";
                viewModel.InitPriceView();
                viewModel.CommonRowNumber = 3;
                viewModel.CommonRowSpan = 4;
                viewModel.DepthMv.View.Refresh();
                Button btnTen = CommonUtils.GetVisualChild<Button>(DepthDataGrid, v => v.Name == "btnTen");
                if (btnTen != null)
                {
                    btnTen.Content = "20";
                }

                MakeCenter();
            }
        }

        private void MakeCenter()
        {
            if (viewModel != null)
            {
                if (!viewModel.SendPriceItem.BestBid5.HasValue || !viewModel.SendPriceItem.BestBid.HasValue)
                {
                    return;
                }

                //获取当前第一行的对象和其价格
                var border1 = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                if (border1 != null)
                {
                    var scroll = border1.Child as ScrollViewer;
                    if (scroll != null)
                    {
                        viewModel.FirstVisibleRowIndex = (int)scroll.VerticalOffset;
                        DepthEntry entry = (DepthEntry)DepthDataGrid.Items.GetItemAt(viewModel.FirstVisibleRowIndex);
                        if (entry != null)
                        {
                            viewModel.FirstVisibleRowPrice = entry.PriceDecimal;
                            //LogHandler.Info("居中： 第一行价格={0}，卖一价={1}，原滚动条位置={2}", entry.Price, viewModel.SendPriceItem.BestOfr, scroll.VerticalOffset);
                        }
                    }
                }
                //Fifth模式
                if (viewModel.IsFifthDepth)
                {
                    MakeCenterFifth();
                }
                //Tick模式
                else
                {
                    MakeCenterTick();
                }
            }
        }

        //Fifth模式下居中操作
        private void MakeCenterFifth()
        {
            //有买价行情且只有一档
            if (viewModel.SendPriceItem.BestBid.HasValue && viewModel.SendPriceItem.BestBid5.Value == viewModel.SendPriceItem.BestBid.Value)
            {
                //可见的第一行价格小于买一价， 第一行是买价(在买一价下面）
                if (viewModel.SendPriceItem.BestBid.Value >= viewModel.FirstVisibleRowPrice)
                {
                    decimal delta = viewModel.SendPriceItem.BestBid.Value - viewModel.FirstVisibleRowPrice;
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int add = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Add(deltaHeight, add * 1);
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset - offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            //LogHandler.Info("Fifth模式居中： 第一行价格={0}，买一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}", viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestBid, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
            }

            //有买价或卖价行情且只有一档
            if (viewModel.SendPriceItem.BestOfr.HasValue && viewModel.SendPriceItem.BestOfr5.Value == viewModel.SendPriceItem.BestOfr.Value)
            {
                //可见的第一行价格大于卖一价， 第一行是卖价(在卖一价上面）
                if (viewModel.SendPriceItem.BestOfr.Value.CompareTo(viewModel.FirstVisibleRowPrice) <= 0)
                {
                    decimal delta = viewModel.FirstVisibleRowPrice - viewModel.SendPriceItem.BestOfr.Value;
                    //ScrollUnit=Item，表示按列表项滚动
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int sub = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Subtract(deltaHeight, sub * 1);
                    //需要向上滚动的Item个数
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset + offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            //LogHandler.Info("Fifth模式居中： 第一行价格={0}，卖一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}", viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestOfr, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
            }
            /*
            //不止一档行情的时候 暂时无法测试。
            else
            {
                //可见的第一行价格大于卖5价(最上面的行情卖价)， 第一行是卖价(在卖5价上面）
                if (viewModel.SendPriceItem.BestOfr5.Value.CompareTo(viewModel.FirstVisibleRowPrice) <= 0)
                {
                    decimal delta = viewModel.FirstVisibleRowPrice - viewModel.SendPriceItem.BestOfr5.Value;
                    //ScrollUnit=Item，表示按列表项滚动
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int sub = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Subtract(deltaHeight, 0 * 1);
                    //需要向上滚动的Item个数
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset + offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            LogHandler.Info("居中： 第一行价格={0}，卖一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}",
                                viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestOfr5, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
                //可见的第一行价格小于买5价(最下面的行情买价)， 第一行是买价(在买5价下面）
                else if (viewModel.SendPriceItem.BestBid.Value.CompareTo(viewModel.FirstVisibleRowPrice) >= 0)
                {
                    decimal delta = viewModel.SendPriceItem.BestBid.Value - viewModel.FirstVisibleRowPrice;
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int add = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Add(deltaHeight, 0 * 1);
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset - offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            LogHandler.Info("居中： 第一行价格={0}，买一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}",
                                viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestBid, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
            }
            */
        }

        //Tick模式下居中操作
        private void MakeCenterTick()
        {
            //有买价行情且只有一档
            if (viewModel.SendPriceItem.BestBid.HasValue && viewModel.SendPriceItem.BestBid5.Value == viewModel.SendPriceItem.BestBid.Value)
            {
                //可见的第一行价格小于买一价， 第一行是买价(在买一价下面）
                if (viewModel.SendPriceItem.BestBid.Value >= viewModel.FirstVisibleRowPrice)
                {
                    decimal centerprice = viewModel.SendPriceItem.BestBid.Value == viewModel.SendPriceItem.LastPrice.Value ? viewModel.SendPriceItem.BestBid.Value : viewModel.SendPriceItem.BestOfr.Value;
                    if(viewModel.SendPriceItem.BestOfr.HasValue)
                    {
                        int num = (int)((viewModel.SendPriceItem.BestOfr.Value - viewModel.SendPriceItem.BestBid.Value) / 0.005m) / 2;
                        //最新价大于买一价（基本等于卖一价）
                        if (centerprice > viewModel.SendPriceItem.BestBid.Value)
                        {
                            //居中价格为买一卖一价的中间价格
                            centerprice = centerprice - num * 0.005m;
                        }
                        else
                        {
                            centerprice = centerprice + num * 0.005m;
                        }
                    }
                    //卖一价为空，居中价格直接为买一价
                    else
                    {
                        //
                    }
                    decimal delta = centerprice - viewModel.FirstVisibleRowPrice;
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int add = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Add(deltaHeight, add * 1);
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset - offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            //LogHandler.Info("Tick模式居中： 第一行价格={0}，买一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}", viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestBid, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
            }

            //有买价或卖价行情且只有一档
            if (viewModel.SendPriceItem.BestOfr.HasValue && viewModel.SendPriceItem.BestOfr5.Value == viewModel.SendPriceItem.BestOfr.Value)
            {
                //可见的第一行价格大于卖一价， 第一行是卖价(在卖一价上面）
                if (viewModel.SendPriceItem.BestOfr.Value.CompareTo(viewModel.FirstVisibleRowPrice) <= 0)
                {
                    decimal centerprice = viewModel.SendPriceItem.BestOfr.Value == viewModel.SendPriceItem.LastPrice.Value ? viewModel.SendPriceItem.BestOfr.Value : viewModel.SendPriceItem.BestBid.Value;
                    if (viewModel.SendPriceItem.BestBid.HasValue)
                    {
                        int num = (int)((viewModel.SendPriceItem.BestOfr.Value - viewModel.SendPriceItem.BestBid.Value) / 0.005m) / 2;
                        //最新价大于买一价（基本等于卖一价）
                        if (centerprice > viewModel.SendPriceItem.BestBid.Value)
                        {
                            //居中价格为买一卖一价的中间价格
                            centerprice = centerprice - num * 0.005m;
                        }
                        else
                        {
                            centerprice = centerprice + num * 0.005m;
                        }
                    }
                    //买一价为空，居中价格直接为卖一价
                    else
                    {
                        //
                    }
                    decimal delta = viewModel.FirstVisibleRowPrice - centerprice;
                    //ScrollUnit=Item，表示按列表项滚动
                    decimal deltaHeight = decimal.Multiply(decimal.Divide(delta, 0.005m), 1);
                    int sub = viewModel.DisplayTenTwenty == 10 ? 4 : 9;
                    decimal height = decimal.Subtract(deltaHeight, sub * 1);
                    //需要向上滚动的Item个数
                    double offset = CommonUtils.ConvertToDouble(height.ToString());
                    var border = VisualTreeHelper.GetChild(DepthDataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            offset = scroll.VerticalOffset + offset;
                            offset = Math.Max(Math.Min(offset, scroll.ExtentHeight), 0);
                            //LogHandler.Info("Tick模式居中： 第一行价格={0}，卖一价={1}，delta={4},滚动距离={2}，原滚动条位置={3}", viewModel.FirstVisibleRowPrice, viewModel.SendPriceItem.BestOfr, height, scroll.VerticalOffset, deltaHeight);
                            scroll.ScrollToVerticalOffset(offset);
                        }
                    }
                }
            }
        }

        //居中显示按钮
        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            MakeCenter();
        }

        private void cbBookId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ComboBox)
            {
                ComboBox combo = sender as ComboBox;
                if (combo.SelectedIndex != -1)
                {
                    string bookid = combo.SelectedValue.ToString();
                    QueryPositionReq req = new QueryPositionReq();
                    req.bookid = bookid;
                    req.securityid = viewModel.SendPriceItem.Security;
                    QueryPositionResult reply = GetPositionDetailData(req);
                    if (reply != null)
                    {
                        if (reply.Datas != null && reply.Datas.Count > 0)
                        {
                            foreach (PositionDetailModel pdm in reply.Datas)
                            {
                                decimal longavgprice = pdm.longavgprice;
                                decimal shortavgprice = pdm.shortavgprice;
                                viewModel.SendPriceItem.LongAvgPrice = longavgprice;
                                viewModel.SendPriceItem.ShortAvgPrice = shortavgprice;
                                viewModel.SendPriceItem.LongAvgPriceStr = longavgprice.ToString("N4");
                                viewModel.SendPriceItem.ShortAvgPriceStr = shortavgprice.ToString("N4");
                            }
                        }
                        else
                        {
                            viewModel.SendPriceItem.LongAvgPrice = 0m;
                            viewModel.SendPriceItem.ShortAvgPrice = 0m;
                            viewModel.SendPriceItem.LongAvgPriceStr = "--";
                            viewModel.SendPriceItem.ShortAvgPriceStr = "--";
                        }
                    }
                }
            }
        }

        private void depthGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (DataGridCellInfo info in e.AddedCells)
            {
                DataGridRow row = (DataGridRow)DepthDataGrid.ItemContainerGenerator.ContainerFromItem(info.Item);
                if (null != row)
                {
                    row.IsSelected = false;
                }
            }
        }

        //买量列双击下单
        private void buyQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            if (!viewModel.CheckParam())
            {
                string reason = string.Empty;
                decimal tempD = 0;
                if (string.IsNullOrEmpty(viewModel.SendPriceItem.Security))
                    reason = "未选择债券";
                else if (!decimal.TryParse(viewModel.SendPriceItem.Amount, out tempD) || tempD == 0)
                    reason = "未填写量或量为0";
                if (!viewModel.OpenSelected && !viewModel.CloseSelected && !viewModel.OpenCloseAutoSelected)  //开平仓未选择
                {
                    reason = "开平仓未选择";
                }
                if (!viewModel.SpeculateSelected && !viewModel.HedgeSelected && !viewModel.ArbitrageSelected)  //投机/套保/套利未选择
                {
                    reason = "投机/套保/套利未选择";
                }
                MessageBoxGUI msgBox = new MessageBoxGUI();
                if (IsActive)
                {
                    msgBox.Owner = this;
                }
                msgBox.setWarnMsg(reason);
                msgBox.ShowDialog();
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                viewModel.DoubleClickOrder(depthEntry.Price, "1");
            }
            return;
        }

        //卖量列双击下单
        private void sellQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            if (!viewModel.CheckParam())
            {
                string reason = string.Empty;
                decimal tempD = 0;
                if (string.IsNullOrEmpty(viewModel.SendPriceItem.Security))
                    reason = "未选择债券";
                else if (!decimal.TryParse(viewModel.SendPriceItem.Amount, out tempD) || tempD == 0)
                    reason = "未填写量或量为0";
                if (!viewModel.OpenSelected && !viewModel.CloseSelected && !viewModel.OpenCloseAutoSelected)  //开平仓未选择
                {
                    reason = "开平仓未选择";
                }
                if (!viewModel.SpeculateSelected && !viewModel.HedgeSelected && !viewModel.ArbitrageSelected)  //投机/套保/套利未选择
                {
                    reason = "投机/套保/套利未选择";
                }
                MessageBoxGUI msgBox = new MessageBoxGUI();
                if (IsActive)
                {
                    msgBox.Owner = this;
                }
                msgBox.setWarnMsg(reason);
                msgBox.ShowDialog();
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                viewModel.DoubleClickOrder(depthEntry.Price, "2");
            }
            return;
        }

        //切换Tick模式
        private void btnTick_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                //初始化阶梯下单板数据源
                viewModel.DepthMv.Source = viewModel.FullTickEntries;
                viewModel.IsFifthDepth = false;
                viewModel.UpdateQtySum();
                viewModel.DepthMv.View.Refresh();
                MakeCenter();
            }
        }

        //切换10档模式
        private void btnFifth_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                //初始化阶梯下单板数据源
                viewModel.DepthMv.Source = viewModel.FullDepthEntries;
                viewModel.IsFifthDepth = true;
                viewModel.UpdateQtySum();
                viewModel.DepthMv.View.Refresh();
                MakeCenter();
            }
        }

        private void DepthMv_Filter(object sender, FilterEventArgs e)
        {
            DepthEntry de = e.Item as DepthEntry;
            if (de != null)
            {
                e.Accepted = de.Show;
            }
            else
            {
                e.Accepted = false;
            }
        }

        //撤销指定价格策略买单
        private void strategyBuyQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                CancelOrders(depthEntry, 5);
            }
        }

        //撤销指定价格手工买单
        private void manualBuyQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                CancelOrders(depthEntry, 4);
            }
        }

        //撤销指定价格策略卖单
        private void strategySellQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                CancelOrders(depthEntry, 5);
            }
        }

        //撤销指定价格手工卖单
        private void manualSellQtyCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            DataGridCell dataGridCell = (DataGridCell)sender;
            DepthEntry depthEntry = dataGridCell.DataContext as DepthEntry;
            if (depthEntry != null)
            {
                CancelOrders(depthEntry, 4);
            }
        }

        //撤销全部挂单
        private void btnCancelAll_Click(object sender, RoutedEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            CancelOrders(null, 1);
        }

        //撤销策略挂单
        private void btnCancelStrategy_Click(object sender, RoutedEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            CancelOrders(null, 2);
        }

        //撤销手工挂单
        private void btnCancelManual_Click(object sender, RoutedEventArgs e)
        {
            if (null == viewModel)
            {
                return;
            }
            CancelOrders(null, 3);
        }

        //根据cancelType确定操作： 1=撤销全部挂单，2=撤销策略挂单，3=撤销手工挂单， 4=撤销特定价位手工单，5=撤销特定价位策略单
        private void CancelOrders(DepthEntry depthEntry, int cancelType)
        {
            OrderMassCancelRequest req = new OrderMassCancelRequest();
            req.UserName = LocalEnv.UserInfo.UserName;
            req.SecurityID = viewModel.SecurityID;
            req.MarketIndicator = Consts.MARKETINDICATOR_FUTURE;
            if (null == depthEntry)
            {
                switch (cancelType)
                {
                    case 1:
                        req.MassCancelRequestType = OrderMassCancelRequest.CANCEL_ALL_ORDERS;
                        break;
                    case 2:
                        req.MassCancelRequestType = OrderMassCancelRequest.CANCEL_STRATEGY_ORDERS;
                        break;
                    case 3:
                        req.MassCancelRequestType = OrderMassCancelRequest.CANCEL_MANUAL_ORDERS;
                        break;
                }
            }
            else
            {
                string price = depthEntry.Price;
                string side = "BUY".Equals(depthEntry.BuySell) ? "1" : "2";
                req.Price = price;
                req.Side = side;
                switch (cancelType)
                {
                    case 4:
                        req.MassCancelRequestType = OrderMassCancelRequest.CANCEL_MANUAL_ORDERS;
                        break;
                    case 5:
                        req.MassCancelRequestType = OrderMassCancelRequest.CANCEL_STRATEGY_ORDERS;
                        break;
                }
            }
            OrderMassCancelReplyPo reply = CancelOrderBatch(req);
            if (reply != null)
            {
                if (reply.Code == ApiConsts.CODE_SUCCESS)
                {
                    CustomMessageBox.Info(string.Format($"撤销订单成功."), "撤销订单");
                }
                else
                {
                    CustomMessageBox.Info(string.Format($"撤销订单失败, 原因:{reply.Msg}."), "撤销订单");
                }
            }
        }

        //取消，退出
        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.CloseClick();
            }
        }

        private void DepthDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //Tick模式不处理行加载事件,因为分隔线只有一条
            if (viewModel != null && !viewModel.IsFifthDepth)
            {
                return;
            }
            DepthEntry rowContext = e.Row.DataContext as DepthEntry;
            if (rowContext != null)
            {
                e.Row.BorderThickness = BORDER_THICKNESS_NONE;
                if (rowContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                {
                    e.Row.BorderThickness = new Thickness(0, 2, 0, 0);
                    e.Row.BorderBrush = Brushes.Silver;
                }
            }
            //LogHandler.Info("Row{0}，ThickNess={1}", rowContext.Price, e.Row.BorderThickness.ToString());

        }

        private void strategyBuyQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 1, 0, 0);
                    dataGridCell.BorderBrush = Brushes.Green;
                }
            }
        }

        private void manualBuyQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 1, 0, 0);
                    dataGridCell.BorderBrush = Brushes.Green;
                }
            }
        }

        private void buyQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 1, 0, 0);
                    dataGridCell.BorderBrush = Brushes.Green;
                }
            }
        }

        private void priceCell_Load(object sender, RoutedEventArgs e)
        {
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                //Fifth模式下，价格单元格加载只处理价格字体颜色粗细
                if (viewModel != null && viewModel.IsFifthDepth)
                {
                    return;
                }
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestOfr) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 0, 0, 1);
                    dataGridCell.BorderBrush = viewModel.IsFifthDepth ? Brushes.Silver : Brushes.Red;
                }
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 1, 0, 0);
                    dataGridCell.BorderBrush = viewModel.IsFifthDepth ? Brushes.Silver : Brushes.Green;
                }
            }
        }

        private void sellQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestOfr) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 0, 0, 1);
                    dataGridCell.BorderBrush = Brushes.Red;
                }
            }
        }

        private void strategySellQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestOfr) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 0, 0, 1);
                    dataGridCell.BorderBrush = Brushes.Red;
                }
            }
        }

        private void manualSellQtyCell_Load(object sender, RoutedEventArgs e)
        {
            if (viewModel != null && viewModel.IsFifthDepth)
            {
                return;
            }
            DataGridCell dataGridCell = sender as DataGridCell;
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestOfr) == 0)
                {
                    dataGridCell.BorderThickness = new Thickness(0, 0, 0, 1);
                    dataGridCell.BorderBrush = Brushes.Red;
                }
            }
        }

        //鼠标进入单元格前的值
        private string FormerText = string.Empty;
        private BrushConverter brushConverter = new BrushConverter();
        //BuySellQty:   1=买, 2=卖
        //QtyCell:      1=策略买, 2=手工买, 3=买量, 4=卖量, 5=手工卖, 6=策略卖
        private void ContentPresenter_MouseEnterBuyQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 1, "+", 3);
            MouseEnterQty((TextBlock)sender, true, "+", 3);
        }

        private void ContentPresenter_MouseLeaveBuyQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, true, "+");
        }

        private void ContentPresenter_MouseEnterSellQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 2, "+", 4);
            MouseEnterQty((TextBlock)sender, false, "+", 4);
        }

        private void ContentPresenter_MouseLeaveSellQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, false, "+");
        }

        private void ContentPresenter_MouseEnterStrategyBuyQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 1, "-", 1);
            MouseEnterQty((TextBlock)sender, true, "-", 1);
        }

        private void ContentPresenter_MouseLeaveStrategyBuyQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, true, "-");
        }

        private void ContentPresenter_MouseEnterManualBuyQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 1, "-", 2);
            MouseEnterQty((TextBlock)sender, true, "-", 2);
        }

        private void ContentPresenter_MouseLeaveManualBuyQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, true, "-");
        }

        private void ContentPresenter_MouseEnterManualSellQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 2, "-", 5);
            MouseEnterQty((TextBlock)sender, false, "-", 5);
        }

        private void ContentPresenter_MouseLeaveManualSellQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, false, "-");
        }

        private void ContentPresenter_MouseEnterStrategySellQty(object sender, MouseEventArgs e)
        {
            //ShowBuySellTipWindow(sender, 2, "-", 6);
            MouseEnterQty((TextBlock)sender, false, "-", 6);
        }

        private void ContentPresenter_MouseLeaveStrategySellQty(object sender, MouseEventArgs e)
        {
            MouseLeaveQty((TextBlock)sender, false, "-");
        }

        //cell： 1=策略买, 2=手工买, 3=买量, 4=卖量, 5=手工卖, 6=策略卖
        private void MouseEnterQty(TextBlock sender, bool isBuy, string sign, int cell)
        {
            DataGridCell dataGridCell = ((sender.TemplatedParent as ContentPresenter).Parent as DataGridCell);
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                //策略买，手工买，策略卖，手工卖没有报量的时候不显示撤单提示
                switch (cell)
                {
                    case 1:
                        if (cellContext.StrategyBuyQty <= 0)
                        {
                            return;
                        }
                        break;
                    case 2:
                        if (cellContext.ManualBuyQty <= 0)
                        {
                            return;
                        }
                        break;
                    case 3:
                    case 4:
                        break;
                    case 5:
                        if (cellContext.ManualSellQty <= 0)
                        {
                            return;
                        }
                        break;
                    case 6:
                        if (cellContext.StrategySellQty <= 0)
                        {
                            return;
                        }
                        break;
                }

                Brush brush = null;
                if (isBuy)
                {
                    //橙色
                    brush = (Brush)brushConverter.ConvertFromString("#FB943C");
                }
                else
                {
                    //蓝色
                    brush = (Brush)brushConverter.ConvertFromString("#58B3FF");
                }
                dataGridCell.BorderThickness = BORDER_THICKNESS_TIP;
                dataGridCell.Foreground = brush;
                dataGridCell.FontWeight = FontWeights.Bold;
                dataGridCell.BorderBrush = brush;
                if ("+".Equals(sign))
                {
                    FormerText = sender.Text;
                    sender.Text = "+" + viewModel.SendPriceItem.Amount;
                }
                else if ("-".Equals(sign))
                {
                    sender.Text = "-" + sender.Text;
                }
            }
        }

        private void MouseLeaveQty(TextBlock sender, bool isBuy, string sign)
        {
            DataGridCell dataGridCell = ((sender.TemplatedParent as ContentPresenter).Parent as DataGridCell);
            DepthEntry cellContext = dataGridCell.DataContext as DepthEntry;
            if (cellContext != null)
            {
                Brush brush = (Brush)brushConverter.ConvertFromString("#FFFFFF");
                dataGridCell.BorderThickness = BORDER_THICKNESS_NONE;
                dataGridCell.Foreground = brush;
                dataGridCell.FontWeight = FontWeights.Regular;
                if ("+".Equals(sign))
                {
                    sender.Text = FormerText;
                }
                else if ("-".Equals(sign))
                {
                    sender.Text = sender.Text.Replace("-", string.Empty);
                }
                //如果是最优BidOfr价位，需要还原红绿分隔线
                if(isBuy)
                {
                    if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestBid) == 0)
                    {
                        dataGridCell.BorderThickness = new Thickness(0, 1, 0, 0);
                        dataGridCell.BorderBrush = Brushes.Green;
                    }
                }
                else
                {
                    if (cellContext.PriceDecimal.CompareTo(viewModel.SendPriceItem.BestOfr) == 0)
                    {
                        dataGridCell.BorderThickness = new Thickness(0, 0, 0, 1);
                        dataGridCell.BorderBrush = Brushes.Red;
                    }
                }                
            }
        }
    }
}
