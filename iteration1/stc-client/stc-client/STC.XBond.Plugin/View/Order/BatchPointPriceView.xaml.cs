using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.Order;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace STC.XBond.Plugin.View.Order
{
    /// <summary>
    /// BatchPointPriceView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchPointPriceView : Window
    {
        private BatchPointPriceViewModel _batchPointPriceViewModel = null;

        public BatchPointPriceView()
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;

            _batchPointPriceViewModel = new BatchPointPriceViewModel();
            this.DataContext = _batchPointPriceViewModel;

            (priceTableView.DataContext as PriceTableViewModel).OnTotalQtyChange += PriceTableView_OnTotalQtyChange;

            numUpDown.UpDownBackground = this.FindResource("ColorBrush.Gray15") as Brush;
            numUpDown.UpDownBorderBrush = this.FindResource("ColorBrush.Gray-23") as Brush;
            numUpDown.UpDownBorderThickness = new Thickness(0);

            this.Unloaded += BatchPointPriceView_Unloaded;
        }

        private void BatchPointPriceView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= BatchPointPriceView_Unloaded;

            if (_batchPointPriceViewModel != null)
                _batchPointPriceViewModel.Clear();
        }

        public void Init(string side, string securityId, string pointPrice)
        {
            if (_batchPointPriceViewModel != null)
            {
                _batchPointPriceViewModel.Init(this, side, securityId, pointPrice);
            }
        }

        private void PriceTableView_OnTotalQtyChange(object obj)
        {
            if (_batchPointPriceViewModel != null)
                _batchPointPriceViewModel.UpdateTotal((decimal)obj);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            if (tBox.Name.ToString() == "AmountTxt")
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    _batchPointPriceViewModel.AddAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {
                    _batchPointPriceViewModel.DownAmountCommand.Execute(this);
                    e.Handled = true;
                    return;
                }
            }

        }

        private void AmountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_batchPointPriceViewModel != null && !_batchPointPriceViewModel.UpdateFromTable)
                _batchPointPriceViewModel.ResetClickQty(AmountTxt.Text);
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedEventArgs e)
        {

        }

        private void bdSetleBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettlTypeCombox.IsDropDownOpen = !SettlTypeCombox.IsDropDownOpen;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < 3)
                _batchPointPriceViewModel.QuickQty((listBox.SelectedItem as ListBoxItem).Content.ToString());
            else if (listBox.SelectedIndex == 3)//all
            {
                _batchPointPriceViewModel.QuickAllQty();
            }

            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void LstSettleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            List<string> settleList = new List<string>();
            foreach (ListBoxItemData listBoxItem in listBox.SelectedItems)
            {
                string settle = listBoxItem.Name;
                if (!settleList.Contains(settle))
                {
                    settleList.Add(settle);
                }
            }

            if (_batchPointPriceViewModel != null)
                _batchPointPriceViewModel.Filter(settleList);
        }
    }
}
