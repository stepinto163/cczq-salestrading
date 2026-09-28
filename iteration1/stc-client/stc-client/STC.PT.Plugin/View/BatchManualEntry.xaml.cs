using STC.Common.Handler.Position;
using STC.Common.Model.CRM;
using STC.Common.Model.Position;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// BatchManualEntry.xaml 的交互逻辑
    /// </summary>
    public partial class BatchManualEntry : WindowBase
    {
        private BatchManualEntryViewModel _manualEntryViewModel = new BatchManualEntryViewModel();
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }
        //public string symboltype;
        public bool isSales { get; set; }
        public static List<KeyValuePair<string, string>> QuotemethodItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("quotemethod_pt");
            }
        }
        public static List<KeyValuePair<string, string>> BusinessTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("businesstype");
            }
        }
        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();
        public BatchManualEntry()
        {
            InitializeComponent();
            this.DataContext = _manualEntryViewModel;
            //comBook.ItemsSource = PTExecOrderProcesser.Instance.BookDict;
            comWay.ItemsSource = QuotemethodItemSource;
            comBusinessType.ItemsSource = BusinessTypeItemSource;
            _manualEntryViewModel.manualEntry = this;

            this.Loaded += BatchManualEntry_Loaded;
        }

        private void BatchManualEntry_Loaded(object sender, RoutedEventArgs e)
        {
            if (isSales)
            {
                InsNametb.Visibility = Visibility.Visible;
                InsNameCb_edit.Visibility = Visibility.Visible;
                TrdNametb.Visibility = Visibility.Visible;
                TrdNameCb_edit.Visibility = Visibility.Visible;
            }
            else
            {
                InsNametb.Visibility = Visibility.Collapsed;
                InsNameCb_edit.Visibility = Visibility.Collapsed;
                TrdNametb.Visibility = Visibility.Collapsed;
                TrdNameCb_edit.Visibility = Visibility.Collapsed;
            }
        }
        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj == null)
                return;
            txSymbol.Text = obj.Short_Name;
            //symboltype = obj.SECURITY_TYPE;
            if (_manualEntryViewModel.IsBank)//true-交易所
            {
                comWay.Visibility = Visibility.Collapsed;
                txWay.Visibility = Visibility.Collapsed;
                Grid.SetColumn(tbCommnet, 0);
                Grid.SetColumn(txCommnet, 1);
                tbCommnet.Text = "约定号:";
                tbSeatNumber.Visibility = Visibility.Visible;
                txSeatNumber.Visibility = Visibility.Visible;
            }
            else
            {
                comWay.Visibility = Visibility.Visible;
                txWay.Visibility = Visibility.Visible;
                tbSeatNumber.Visibility = Visibility.Collapsed;
                txSeatNumber.Visibility = Visibility.Collapsed;
                tbCommnet.Text = "备注:";
            }
        }

        private void Search_TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "AmountTxt")
            {
                tag = "1";
            }
            else if ((tb.Name == "txPrice") || (tb.Name == "txNetPrice"))
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;

        }

        private void TxNetPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

            //if (tag == "1" || tag == "3" || tag == "4")//量
            //    regex = new Regex("^[0-9]+$");
            //else if (tag == "2")
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }

        private void TxNetPrice_PreviewKeyDown(object sender, KeyEventArgs e)
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
                    // viewModel.AddPriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    // viewModel.ReducePriceCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "AmountTxt")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    //viewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    //viewModel.ReduceAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
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

        private void TxQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = new Regex("^[0-9.-]+$");

            decimal max = 9999.99M;
            decimal min = -9999.99M;

            if (_manualEntryViewModel.IsIRS)//量
            {
                regex = new Regex("^[0-9]+$");

                max = 1000;
                min = 1;
            }
            else
                regex = new Regex("^[0-9.-]+$");

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".") && _manualEntryViewModel.IsIRS)
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
            else
            {
                e.Handled = true;
                return;
            }
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2 && !_manualEntryViewModel.IsIRS)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }

        private void InsNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(InsNameCb_edit.Text))
            {
                InsNameCb_edit.ExpandOptions();
            }
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                _manualEntryViewModel.FilterInstitution(args);
            }

        }

        private void TrdNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(TrdNameCb_edit.Text))
            {
                TrdNameCb_edit.ExpandOptions();
            }
        }

        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    _manualEntryViewModel.FilterTraderLocal(args);
                }
                else
                {
                    _manualEntryViewModel.FilterTrader(args);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
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

        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }
        public void setUpdateList(IList SelectedItemsList) 
        {
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                _manualEntryViewModel.SelectedItemsList = SelectedItemsList;
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                if("SideListBox".Equals(listBox.Name))
                    _manualEntryViewModel.SideIndex = -1;
                else if ("SettlTypeListBox".Equals(listBox.Name))
                    _manualEntryViewModel.SettlTypeIndex = -1;
            }
        }

        private void CounterInsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CounterTrdNameCb_edit.Text))
            {
                CounterInsNameCb_edit.ExpandOptions();
            }
        }

        private void CounterInsNameCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                _manualEntryViewModel.FilterCounterInstitution(args);
            }
        }

        private void CounterTrdNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CounterTrdNameCb_edit.Text))
            {
                CounterTrdNameCb_edit.ExpandOptions();
            }
        }

        private void CounterTrdNameCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_manualEntryViewModel != null)
            {
                if (LocalEnv.ConfInfo.IsLocalCacheCrm == "1")
                {
                    _manualEntryViewModel.FilterCounterTraderLocal(args);
                }
                else
                {
                    _manualEntryViewModel.FilterCounterTrader(args);
                }
            }
        }

        private void datePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                _manualEntryViewModel.TradeDate = null;
                e.Handled = true;
            }
        }
    }
}
