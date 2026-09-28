using STC.Common.Utils;
using STC.Mm.Plugin.ViewModel;
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
using System.Windows.Threading;

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// BatchDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchDetailView : Window
    {
        public BatchDetailView()
        {
            InitializeComponent();
            this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void orderDetailView_Initialized(object sender, EventArgs e)
        {
            if (this.DataContext != null)
            {
                SetAlgoEnable();
                SetAlgoBaseEnable();
                SetAPIModel();
            }
            if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            {
                dealQuoteSetGrid.Visibility = Visibility.Visible;
            }
            else
            {
                dealQuoteSetGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void orderDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                SetAlgoEnable();
                SetAlgoBaseEnable();
                SetAPIModel();
            }
        }

        private void orderDetailView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void bidStrategyMode_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!popBidStrategyList.IsOpen)
            {
                popBidStrategyList.IsOpen = true;
                bidStrategyList_dg.SelectedIndex = -1;

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    (Action)(() => { Keyboard.Focus(popBidStrategyList); }));
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            vm.ResetWindowBorderBursh(tb.Name);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = tb.Tag.ToString();
            decimal max = 0;
            decimal min = 0;
            Regex regex = null;
            int digit = 2;

            if (tag == "bidBaseQty" || tag == "ofrBaseQty" || tag == "maxFloor")
            {
                regex = new Regex("^[0-9]+$");
                max = 999;
                min = 1;
            }
            else if (tag == "manualBid" || tag == "manualOfr")
            {
                regex = new Regex("^[0-9.-]+$");
                max = 9999m;
                min = -99.9999m;
                digit = 4;
            }
            else if (tag == "bidBaseReSendValue" || tag == "ofrBaseReSendValue" || tag == "bidBaseWidenValue" || tag == "ofrBaseWidenValue" ||
                tag == "bidAlgoDefenValue" || tag == "ofrAlgoDefenValue" ||
                tag == "bidAlgoTrigValue" || tag == "ofrAlgoTrigValue" || tag == "algoMidTypeValue" || tag == "bidPosValue" || tag == "ofrPosValue")
            {
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = 0;
            }
            else if (tag == "algoMidOffsetValue" || tag == "bidDefenCDCValue" || tag == "ofrDefenCDCValue"
                || tag == "bidAlgoSafeValue" || tag == "ofrAlgoSafeValue" || tag == "bidQdmespSafeValue" || tag == "ofrQdmespSafeValue")
            {
                regex = new Regex("^[0-9.-]+$");
                max = 1000.00m;
                min = -1000m;
            }
            else if (tag == "algoValue" || tag == "bidAlgoCDCValue" || tag == "ofrAlgoCDCValue")
            {
                regex = new Regex("^[0-9.]+$");
                max = 1000.00m;
                min = 0;
                //min = 0.01m; 需要判断是否大于0
            }
            else if (tag == "algoMidDetailDefined")
            {
                digit = 4;
                regex = new Regex("^[0-9.]+$");
                max = 1000.0000m;
                min = 0;
            }
            else if (tag == "dealWidenLowerTime")
            {
                regex = new Regex("^[0-9]+$");
                digit = 0;
                max = 99;
                min = 1;
            }
            else if (tag == "dealWidenUpperTime")
            {
                regex = new Regex("^[0-9]+$");
                digit = 0;
                max = 99;
                min = 1;
            }
            else if (tag == "stepLength" || tag == "posFakSpread")
            {
                regex = new Regex("^[0-9.]+$");
                max = 99.99m;
                min = 0;
            }

            if (regex == null) { return; }

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
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

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > digit)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }

        private void maxFloor_cb_Click(object sender, RoutedEventArgs e)
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            CheckBox chk = sender as CheckBox;

            if (chk.IsChecked == true)
            {
                maxFloor_tb.IsEnabled = true;
            }
            else
            {
                maxFloor_tb.IsEnabled = false;
            }
            vm.ResetWindowBorderBursh("maxFloor_tb");
        }

        private void defenReferMarketType_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void defenReferLevel_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ofrDefenReferMarketType_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ofrDefenReferLevel_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void wallEnable_cb_Click(object sender, RoutedEventArgs e)
        {

        }

        private void defenCDC_cb_Click(object sender, RoutedEventArgs e)
        {

            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            CheckBox chk = sender as CheckBox;

            if (chk.IsChecked == true)
            {
                bidDefenCDCValue_tb.IsEnabled = true;
                ofrDefenCDCValue_tb.IsEnabled = true;
            }
            else
            {
                bidDefenCDCValue_tb.IsEnabled = false;
                ofrDefenCDCValue_tb.IsEnabled = false;
            }
            vm.ResetWindowBorderBursh("bidDefenCDCValue_tb");
            vm.ResetWindowBorderBursh("ofrDefenCDCValue_tb");
        }

        private void algoMidType_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetAlgoEnable();
        }
        private void SetAlgoEnable()
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            algoMidType_cb.IsEnabled = true;
            algoMidOffsetValue_tb.IsEnabled = true;
            algoValue_tb.IsEnabled = true;

            if (vm.Data.algoMidType == "3")
            {
                algoMidDetailDefined_tbox.IsEnabled = true;
                this.algoMidDetailDefined_tbox.Visibility = Visibility.Visible;
                this.algoMidDetail_tb.Visibility = Visibility.Collapsed;
                this.algoCDCDetail_tb.Visibility = Visibility.Collapsed;
                this.algoMidOffsetValue_tb.IsEnabled = false;
                vm.ResetWindowBorderBursh("algoMidDetailDefined_tbox");
            }
            else if (vm.Data.algoMidType == "1")
            {
                this.algoMidOffsetValue_tb.IsEnabled = true;
                this.algoMidDetail_tb.Visibility = Visibility.Visible;
                this.algoCDCDetail_tb.Visibility = Visibility.Collapsed;
                this.algoMidDetailDefined_tbox.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.algoMidOffsetValue_tb.IsEnabled = true;
                this.algoCDCDetail_tb.Visibility = Visibility.Visible;
                this.algoMidDetail_tb.Visibility = Visibility.Collapsed;
                this.algoMidDetailDefined_tbox.Visibility = Visibility.Collapsed;
            }
        }

        private void SetAlgoBaseEnable()
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;

            if (vm.Data.algoBaseEnable == "1")
            {
                bond_txt.IsEnabled = true;
                vm.ResetWindowBorderBursh("bond_txt");
            }
            else
            {
                bond_txt.IsEnabled = false;
                bond_txt.ClearSerchTxt();
            }
        }
        private void posFak_cb_Click(object sender, RoutedEventArgs e)
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            CheckBox chk = sender as CheckBox;

            if (chk.IsChecked == true)
            {
                posFakSpread_tb.IsEnabled = true;
            }
            else
            {
                posFakSpread_tb.IsEnabled = false;
            }
            vm.ResetWindowBorderBursh("posFakSpread_tb");
        }

        private void bidStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (popBidStrategyList.IsOpen)
            {
                popBidStrategyList.IsOpen = false;
                if (bidStrategyList_dg.SelectedItem != null)
                {
                    KeyValuePair<string, string> current = (KeyValuePair<string, string>)bidStrategyList_dg.SelectedItem;
                    (this.DataContext as BatchDetailViewModel).Data.bidQuoteModel = current.Key;
                    (this.DataContext as BatchDetailViewModel).ResetWindowBorderBursh("algoValue_tb");
                    (this.DataContext as BatchDetailViewModel).ResetWindowBorderBursh("bond_txt");

                    SetAPIModel();
                }
            }
        }
        private void SetAPIModel()
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            vm.ResetWindowBorderBursh("bidBaseQty_tb");
            vm.ResetWindowBorderBursh("ofrBaseQty_tb");
        }

        private void bidStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popBidStrategyList.IsOpen)
            {
                popBidStrategyList.IsOpen = false;
            }
        }

        private void ofrStrategyList_dg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = false;
                if (ofrStrategyList_dg.SelectedItem != null)
                {
                    KeyValuePair<string, string> current = (KeyValuePair<string, string>)ofrStrategyList_dg.SelectedItem;
                    (this.DataContext as BatchDetailViewModel).Data.ofrQuoteModel = current.Key;
                    (this.DataContext as BatchDetailViewModel).ResetWindowBorderBursh("algoValue_tb");
                    (this.DataContext as BatchDetailViewModel).ResetWindowBorderBursh("bond_txt");

                    SetAPIModel();
                }
            }
        }

        private void ofrStrategyList_dg_LostFocus(object sender, RoutedEventArgs e)
        {
            if (popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = false;
            }
        }

        private void scroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (popBidStrategyList.IsOpen || popOfrStrategyList.IsOpen)
            {
                e.Handled = true;
            }
        }

        private void Rest_Click(object sender, RoutedEventArgs e)
        {
            Button tb = sender as Button;
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            vm.ResetWindowBorderBursh(tb.Name);
        }

        private void ofrStrategyMode_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!popOfrStrategyList.IsOpen)
            {
                popOfrStrategyList.IsOpen = true;
                ofrStrategyList_dg.SelectedIndex = -1;
            }
        }

        private void algoBaseEnable_chk_Click(object sender, RoutedEventArgs e)
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            CheckBox chk = sender as CheckBox;

            if (chk.IsChecked == true)
            {
                bond_txt.IsEnabled = true;
            }
            else
            {
                bond_txt.IsEnabled = false;
            }
            vm.ResetWindowBorderBursh("bond_txt");
        }

        private void bond_txt_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null)
            {
                BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
                vm.Data.algoBaseSecurityid = obj.Bond_ID;
                vm.UpdateSecurityCdc();

                this.bond_txt.ToolTip = obj.Bond_ID + " " + obj.Short_Name;

                bond_txt.BorderBrush = vm.DefaultBorderBrush;
            }
        }

        private void bond_txt_ValueChanged(object sender, RoutedEventArgs e)
        {
            BatchDetailViewModel vm = this.DataContext as BatchDetailViewModel;
            vm.ResetWindowBorderBursh("bond_txt");
            bond_txt.ToolTip = string.Empty;
            Console.WriteLine("=== bond_txt.IsPopUpShow = {0}", bond_txt.IsPopUpShow);
        }

        private void bond_txt_LostFocus(object sender, RoutedEventArgs e)
        {
            bond_txt.IsPopUpShow = false;
        }

        private void ofrStrategyMode_btn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void bidStrategyMode_btn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void secondSafeEnable_cb_Click(object sender, RoutedEventArgs e)
        {

        }

        private void qdmespSafeEnable_cb_Click(object sender, RoutedEventArgs e)
        {

        }

        private void qdmespSafeLevel_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void algoSafeEnable_cb_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
