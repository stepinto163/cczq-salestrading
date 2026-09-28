using STC.Common.View.Component;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.ViewModel;
using STC.PT.Plugin.ViewModel.ExecManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.PT.Plugin.View.ExecManage
{
    /// <summary>
    /// ExecManualEntry.xaml 的交互逻辑
    /// </summary>
    public partial class ExecManualEntry : WindowBase
    {
        ExecManualEntryViewModel _viewModel = new ExecManualEntryViewModel();

        public PtExecordersData drv { get; set; }
        public bool isSales { get; set; }
        public bool IsAdd = true;
        public ExecManualEntry()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
            _viewModel.RequestClose += () => this.Close();
        }

        public ExecManualEntry(bool isAdd, PtExecordersData drv)
        {
            InitializeComponent();
            _viewModel.IsAdd = isAdd ? "0" : "1";
            _viewModel.ExecordersData = drv;
            _viewModel.myWindow = this;
            this.DataContext = _viewModel;
            _viewModel.RequestClose += () => this.Close();
        }

        private void tbContent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel != null)
            {
                if (Keyboard.IsKeyDown(Key.Enter) && !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    _viewModel.RecognizeExec(tbContent.Text);
                    e.Handled = true;
                }

                if (Keyboard.IsKeyDown(Key.Enter) && ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
                {
                    if (tbContent.Text.Length > 0)
                    {
                        try
                        {
                            int sourcepos = tbContent.CaretIndex;
                            if (tbContent.CaretIndex >= 0)
                            {
                                var xbContent = tbContent.Text.Substring(0, tbContent.CaretIndex);
                                var xblevase = tbContent.Text.Substring(tbContent.CaretIndex, tbContent.Text.Length - tbContent.CaretIndex);
                                tbContent.Text = xbContent + "\r\n" + xblevase;
                                tbContent.SelectionStart = sourcepos + 2;
                            }
                        }
                        catch { }
                    }

                    e.Handled = true;
                }

            }
        }

        private void Search_TextBox_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {

        }

        private void TxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            if (tb.Name == "txQty")
            {
                tag = "1";
            }
            else if ((tb.Name == "txPrice") || (tb.Name == "txExePrice") ||
                (tb.Name == "txFullPrice") || (tb.Name == "txPaymentNetPrice")
                || (tb.Name == "txPaymentYield"))
            {
                tag = "2";
            }
            else if (tb.Name == "txPaymentRate")
            {
                tag = "5";
            }
            else if (tb.Name == "txSettlAmount")
            {
                tag = "6";
            }
            else if ((tb.Name == "txNetPrice")) 
            {
                tag = "7";
            }
            else if(tb.Name == "txCouponrate")
            {
                tag = "8";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4" || tag == "5" || tag == "8")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2" || tag == "6" || tag == "7")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            decimal max = 1000;
            decimal min = -1;

            if (tag == "1")//量
            {
                max = 1000000;
                min = 1;
            }
            else if (tag == "2" || tag == "7")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            else if (tag == "5")
            {
                max = 100;
                min = 1;
            }
            else if (tag == "6")
            {
                max = 9999999999;
                min = (decimal)0.01;
            }
            else if(tag == "8")//量
            {
                max = 100;
                min = 1;
            }

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }
            decimal d = 0;
            string selectTxt = tb.SelectedText;

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

                if (tag == "1" || tag == "5" || tag == "8") //量
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
                if (!string.IsNullOrEmpty(Search_TextBox.SearchText) && Search_TextBox.SearchText.EndsWith(".SH")) 
                {
                    if ("7".Equals(tag) && decimal.TryParse(arr[1], out d) && arr[1].Length > 3)
                    {
                        e.Handled = true;
                        return;
                    }
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
            if (tBox.Name.ToString() == "txPrice" || tBox.Name.ToString() == "txNetPrice" || 
                tBox.Name.ToString() == "txFullPrice" || tBox.Name.ToString() == "txPaymentNetPrice"||
                (tBox.Name == "txPaymentYield"))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    e.Handled = true;
                    return;
                }
            }
            else if (tBox.Name.ToString() == "txQty")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
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
                if (tBox.Name == "txQty")
                {
                    tag = "1";
                }
                else if ((tBox.Name == "txPrice") || (tBox.Name == "txNetPrice"))
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
                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }

        }

        
        private void CounterInsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CounterInsNameCb_edit.Text))
            {
                CounterInsNameCb_edit.ExpandOptions();
            }
        }

        private void CounterInsNameCb_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterCounterInstitution(args,false);
            }
        }
        private void OrderSide_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                //_viewModel.FilterOrderSide(args);
            }
        }

        private void OrderCounter_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OrderCounter_edit.ExpandOptions();
        }

        private void OrderCounter_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterCounterInstitution(args,true);
            }
        }

        private void CounterPartySubName_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (string.IsNullOrEmpty(CounterPartySubName_edit.Text))
            {
                CounterPartySubName_edit.ExpandOptions();
            }*/
        }

        private void CounterPartySubName_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterCounterPartySubName(args);
            }
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PartySubName_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {

            if (_viewModel != null)
            {
                _viewModel.FilterPartySubName(args);
            }
        }

        private void PartySubName_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(PartySubName_edit.Text))
            {
                PartySubName_edit.ExpandOptions();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShenzhenPartySubName_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ShenzhenPartySubName_edit.Text))
            {
                ShenzhenPartySubName_edit.ExpandOptions();
            }
        }

        private void ShenzhenPartySubName_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterShenzhenPartySubName(args);
            }
        }

        private void ShanghaiInstCode_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ShanghaiInstCode_edit.Text))
            {
                ShanghaiInstCode_edit.ExpandOptions();
            }
        }

        private void ShanghaiInstCode_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterShanghaiInst(args, ".SH");
            }
        }

        private void ShenzhenInstCode_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ShenzhenInstCode_edit.Text))
            {
                ShenzhenInstCode_edit.ExpandOptions();
            }
        }

        private void ShenzhenInstCode_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterShanghaiInst(args, ".SZ");
            }
        }

        private void ShenzhenTradingPart_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ShenzhenTradingPart_edit.Text))
            {
                ShenzhenTradingPart_edit.ExpandOptions();
            }
        }

        private void ShenzhenTradingPart_edit_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterShanghaiInst(args, ".SZ");
            }
        }
    }
}
