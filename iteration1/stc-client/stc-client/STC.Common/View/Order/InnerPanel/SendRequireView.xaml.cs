using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Order.InnerPanel;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.Order.InnerPanel
{
    /// <summary>
    /// SendRequireView.xaml 的交互逻辑
    /// </summary>
    public partial class SendRequireView : Window
    {

        private SendRequireViewModel viewModel = null;
        private RequireParseViewModel requireParseViewModel = null;

        //执行权限
        private readonly string MODULEID_INNERTYPE = "6001";
        private readonly string MM = "600101";//做市
        private readonly string SALER = "600102";//销交

        /// <summary>
        /// 1:做市 2:销交
        /// </summary>
        private string _innerType = "1";

        public bool isMM()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INNERTYPE, MM);
        }

        public bool isSALER()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_INNERTYPE, SALER);
        }

        public SendRequireView(PriceMatchParam priceMatchParam)
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);

            requireParseViewModel = this.parseView.DataContext as RequireParseViewModel;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (priceMatchParam != null)
                {
                    customerPricePart.Visibility = Visibility.Collapsed;
                    if (!string.IsNullOrEmpty(priceMatchParam.managerName) && !string.IsNullOrEmpty(priceMatchParam.buyerTraderName))
                    {
                        //this.parseView.InvestorBd.IsEnabled = false;
                        this.parseView.TraderBd.IsEnabled = false;
                    }
                    if (!string.IsNullOrEmpty(priceMatchParam.IntentionID))
                    {
                        btnBid.IsEnabled = false;
                        btnOfr.IsEnabled = false;
                        Search_TextBox.IsEnabled = false;
                    }
                }
                this.parseView.cbInvestor.PatternChanged += cbInvestor_PatternChanged;
                this.parseView.cbTrader.PatternChanged += cbTrader_PatternChanged;
            }

            if (CommonUtils.IsBDBAppName())
            {
                Border_Left7.Visibility = Visibility.Collapsed;
            }

            if (isSALER())
            {
                _innerType = "2";
            }

            if (priceMatchParam != null && priceMatchParam.IsIntention)
            {
                OkBtn.Content = "发个需";
            }

            viewModel = new SendRequireViewModel();

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            this.DataContext = viewModel;

            if (priceMatchParam.IsIntention)
            {
                detailConditionView.SetPriceConditionVisility(Visibility.Collapsed);
                //detailConditionView.SetSubscribVisiblity(Visibility.Hidden);
            }
            

            //请输入债券代码
            viewModel.Init(this, priceMatchParam);

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
            {
                SettlTypeCombox.Items.Clear();
                SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "不限" });
                SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "T+1" });
                SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "明+0" });

                PA_GroupAccount_Part.Visibility = Visibility.Visible;
                Dictionary<string, object> dic = new Dictionary<string, object>();
                List<StcTbookUsers> investors = CommonUtils.GetInvestors();
                if (investors != null && investors.Count > 0)
                    foreach (StcTbookUsers investor in investors)
                    {
                        dic[investor.Username] = investor.Name;
                    }

                OtherInvestors.ItemsSource = dic;

                this.OtherInvestors.SelectedValues = priceMatchParam.relatedUsers;
                _accountID = priceMatchParam.AccountID;

                firm_Part.Visibility = Visibility.Collapsed;
            }
        }

        private void AmountTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                OkBtn.Focus();
            }
        }

        public SendRequireView(string security, bool isBid, bool isReadClipboard = false )
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);

            requireParseViewModel = this.parseView.DataContext as RequireParseViewModel;

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    SettlTypeCombox.Items.Clear();
                    SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "不限" });
                    SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "T+1" });
                    SettlTypeCombox.Items.Add(new ComboBoxItem() { Content = "明+0" });

                    PA_GroupAccount_Part.Visibility = Visibility.Visible;
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    List<StcTbookUsers> investors = CommonUtils.GetInvestors();
                    if (investors != null && investors.Count > 0)
                        foreach (StcTbookUsers investor in investors)
                        {
                            dic[investor.Username] = investor.Name;
                        }

                    OtherInvestors.ItemsSource = dic;
                }
                this.parseView.cbInvestor.PatternChanged += cbInvestor_PatternChanged;
                this.parseView.cbTrader.PatternChanged += cbTrader_PatternChanged;
            }

            if (CommonUtils.IsBDBAppName())
            {
                Border_Left7.Visibility = Visibility.Collapsed;
            }

            if (isSALER())
            {
                _innerType = "2";
            }

            viewModel = new SendRequireViewModel();

            datePicker.BlackoutDates.AddDatesInPast();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            this.DataContext = viewModel;
            if (security.ToLower() == "tradebook")
            {
                security = "";
                Search_TextBox.HintText = "请输入债券代码";
            }

            //请输入债券代码
            viewModel.Init(this, security, isBid, isReadClipboard);

        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                viewModel.clear();
                viewModel.Dispose();
            });
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbSettle.IsDropDownOpen = !cbSettle.IsDropDownOpen;
        }

        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettlTypeCombox.IsDropDownOpen = !SettlTypeCombox.IsDropDownOpen;
            if (viewModel != null)
            {
                viewModel.SettleBoxIndex = -1;
                viewModel.AddOneOpenedTag = "1";
            }
        }

        private void bdTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
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
            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                if (sender == this.PriceTxt)
                {
                    netPrice.Focus();
                }
                else if (sender == this.netPrice)
                {
                    qtyView.AmountTxt.Focus();
                }
                
                //AmountTxt.Focus();
                //// 选择最后位数
                //int pos = AmountTxt.Text.Length;
                //if (pos > 0)
                //{
                //    int sellength = AmountTxt.Text.Length;
                //    AmountTxt.SelectionStart = 0;
                //    AmountTxt.SelectionLength = sellength;
                //}
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
                //else if (tag == "4")//随机渠道
                //{
                //    max = viewModel.RandomMax;
                //    min = 1;
                //}

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
            //else if (tag == "4")//随机渠道
            //{
            //    max = viewModel.RandomMax;
            //    min = 1;
            //}

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

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (!CommonUtils.IsBrokerSecurity(viewModel.SendPriceItem.Security) && tb.Name == "netPrice")
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 3)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }


            e.Handled = false;
        }

        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }
        }

        private void Search_TextBox_AfterSearch(Model.Base.SimpleBondsInfoPo obj)
        {
            if (string.IsNullOrEmpty(Search_TextBox.SearchText))
            {
                Search_TextBox.SearchText = string.Empty;
                viewModel.SearchText = string.Empty;
                return;
            }

            viewModel.SearchText = Search_TextBox.SearchText;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                PriceMatchParam priceMatchParam = this.viewModel.GetPriceMatchParam();
                if (priceMatchParam != null && priceMatchParam.IsIntention
                    && !string.IsNullOrEmpty(priceMatchParam.IntentionID))
                {
                    SetPriceFocus();
                }
                else
                {
                    viewModel.onWindowActivated(2);
                    if (_innerType == "2")
                    {
                        SetRequireParseViewFocus();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.Search_TextBox.ShowText))
                        {
                            SetPriceFocus();
                        }
                        else
                        {
                            this.Search_TextBox.SelectAll();
                        }
                    }
                }
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.viewModel != null)
            {
                viewModel.onWindowActivated(1);
            }
        }


        public void SetRequireParseViewFocus()
        {
            (parseView as RequireParseView).SetContentFocus();
        }

        public void SetRequireParseViewText()
        {
            (parseView as RequireParseView).SetContentText(Clipboard.GetText().Trim());
            parseView.SetContentFocus();
        }

        private void dgPrice_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Card.PriceAndOrderView po = (Card.PriceAndOrderView)dgPrice;
                po.CloseViewModel();
            })); 
        }

        private void tbBestBid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.CustomPrice = tbBestBidInt.Text;
        }

        private void tbBestOfrInt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.CustomPrice = tbBestOfrInt.Text;
        }

        private void tbBestBidInt_MouseEnter(object sender, MouseEventArgs e)
        {
            if (viewModel.IsBid)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void tbBestBidInt_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void tbBestOfr_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!viewModel.IsBid)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void SetPriceFocus()
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

        private void Window_Activated_1(object sender, EventArgs e)
        {

        }

        private void AccountUserCbc_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void AccountUserCbc_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void OtherInvestors_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                MultiSelectComboBox ccb = sender as MultiSelectComboBox;
                this.viewModel.RelatedUsers = ccb.SelectedValues;
            }
        }

        private string _accountID;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_accountID))
            {
                this.cmbMultiTree.SetSelection(_accountID);
            }
        }

        private void cbInvestor_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                List <StcTbookUsers> list = new List<StcTbookUsers>();
                List<StcTbookUsers> investors = CommonUtils.GetInvestors();

                if (requireParseViewModel.SelectTraderItem != null && LocalEnv.ConfInfo.IsI2T)
                {
                    string type = CommonUtils.ConvertToPAType(this.viewModel.SendPriceItem.Security);
                    List<string> managerList = AdminHandler.Instance.GetManagerList(requireParseViewModel.SelectTraderItem.Userid, type);
                    if (managerList != null)
                    {
                        list = investors.Where(d => managerList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                    }
                }
                else
                {
                    if (investors != null)
                    {
                        if (!string.IsNullOrEmpty(args.Pattern))
                        {
                            list = investors.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                            || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                        }
                        else
                        {
                            list = investors.ToList();
                        }
                    }
                }
                args.DataSource = list;
            }
            

           
        }

        private void cbTrader_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (requireParseViewModel != null)
            {
                List<StcTbookUsers> list = new List<StcTbookUsers>();
                List<StcTbookUsers> traders = CommonUtils.GetTraders();

                if (requireParseViewModel.SelectInvestorItem != null && LocalEnv.ConfInfo.IsI2T)
                {
                    string type = CommonUtils.ConvertToPAType(this.viewModel.SendPriceItem.Security);
                    List<string> traderList = AdminHandler.Instance.GetTraderList(requireParseViewModel.SelectInvestorItem.Userid, type);
                    if (traderList != null)
                    {
                        list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                    }
                }
                else
                {
                    if (traders != null)
                    {
                        if (!string.IsNullOrEmpty(args.Pattern))
                        {
                            list = traders.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                            || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                        }
                        else
                        {
                            list = traders.ToList();
                        }
                    }
                }

                args.DataSource = list;
            }
        }
    }
}
