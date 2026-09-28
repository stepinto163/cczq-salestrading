using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.View.Component;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
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

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// PublishQuoteView.xaml 的交互逻辑
    /// </summary>
    public partial class PublishQuoteView : WindowBase
    {
        PublishQuoteViewModel _PublishQuoteViewModel = new PublishQuoteViewModel();
        public PublishQuoteView()
        {
            InitializeComponent();

            this.DataContext = _PublishQuoteViewModel;
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (_PublishQuoteViewModel != null)
                {
                    _PublishQuoteViewModel.CheckBoxAllValueChange(flag);
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_PublishQuoteViewModel != null)
            {
                _PublishQuoteViewModel.CheckBoxSingleClick();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Tags_Click(object sender, RoutedEventArgs e)
        {

        }

        private void QQ_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox QQCb = sender as AutoCompleteComboBox;
            if (string.IsNullOrEmpty(QQCb.Text))
            {
                QQCb.ExpandOptions();
            }
        }

        private void QQ_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_PublishQuoteViewModel != null)
            {
                _PublishQuoteViewModel.FilterToQQ(args);
            }
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (_PublishQuoteViewModel != null)
            {
                _PublishQuoteViewModel.Clear();
            }
        }

        private void SourceTrader_edit_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_PublishQuoteViewModel != null)
            {
                _PublishQuoteViewModel.FilterBroker(args, _PublishQuoteViewModel.SelectedItem.SourceTrader);
            }
        }

        private void SettlTypeCombox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ComboBox cb = sender as System.Windows.Controls.ComboBox;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if ((cb.DataContext as IntentionMarketItem).AddOneOpenedTag == "1")
                {
                    if (cb.DataContext != null && cb.DataContext is IntentionMarketItem)
                    {
                        (cb.DataContext as IntentionMarketItem).AddOneOpenedTag = "0";
                    }
                    e.Handled = true;
                }
            }
        }
    }
}
