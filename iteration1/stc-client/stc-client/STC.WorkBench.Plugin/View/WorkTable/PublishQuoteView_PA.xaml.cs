using Newtonsoft.Json;
using STC.Common.View.Component;
using STC.Common.ViewModel;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// PublishQuoteView_PA.xaml 的交互逻辑
    /// </summary>
    public partial class PublishQuoteView_PA : WindowBase
    {
        public PublishQuoteView_PA()
        {
            InitializeComponent();
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Tags_Click(object sender, RoutedEventArgs e)
        {

        }

        PublishQuoteViewModel_PA vm;

        private void WindowBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            vm = e.NewValue as PublishQuoteViewModel_PA;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void group_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetQQGroupList(ccb.SelectedValues);
        }

        private void contacts_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetContactsList(ccb.SelectedValues);
        }

        private void counterparties_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetCounterPartiesList(ccb.SelectedValues);
        }

        private void broker_ncd_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetNcdList(ccb.SelectedValues);
        }

        private void broker_bco_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetBcoList(ccb.SelectedValues);
        }

        private void broker_local_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetLocalList(ccb.SelectedValues);
        }

        private void broker_bnc_box_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SetBncList(ccb.SelectedValues);
        }
    }
}
