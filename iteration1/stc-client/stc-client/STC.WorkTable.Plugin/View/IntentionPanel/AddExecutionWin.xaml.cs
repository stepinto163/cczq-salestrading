using STC.Common.View.Component;
using STC.WorkTable.Plugin.ViewModel.IntentionPanel;
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

namespace STC.WorkTable.Plugin.View.IntentionPanel
{
    /// <summary>
    /// AddExecutionWin.xaml 的交互逻辑
    /// </summary>
    public partial class AddExecutionWin : WindowBase
    {
        AddExecutionViewModel addExecVM = new AddExecutionViewModel();

        public AddExecutionWin()
        {
            InitializeComponent();
            this.DataContext = addExecVM;
        }

        bool ischeckAll = false;
        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            ischeckAll = true;
            addExecVM.CtrlCheckBox((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
            addExecVM.CheckExeQty();
            ischeckAll = false;
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (ischeckAll)
                return;
            addExecVM.SelectedRowCount++;
            addExecVM.IsAllChecked = addExecVM.isCheckAll();
            addExecVM.CheckExeQty();
        }

        private void Selected_Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ischeckAll)
                return;
            addExecVM.SelectedRowCount--;
            addExecVM.IsAllChecked = addExecVM.isCheckAll();
            addExecVM.CheckExeQty();
        }

        private void PriceNU_Edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void QuantityNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void ExeQtyNU_edit_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is NumericUpDown)
            {
                (sender as NumericUpDown).EnterEdit();
            }
        }

        private void ExeQtyNU_edit_ValueChanged(object sender, RoutedEventArgs e)
        {
            addExecVM.CheckExeQty();
        }


    }
}
