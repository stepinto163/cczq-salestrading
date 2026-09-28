using STC.Common.View.Component;
using STC.WorkTable.Plugin.ViewModel;
using STC.WorkTable.Plugin.ViewModel.Data;
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

namespace STC.WorkTable.Plugin.View.WorkTable
{
    /// <summary>
    /// ConfirmIntentionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmIntentionWindow : WindowBase
    {
        private ConfirmIntentionWindowViewModel _viewModel = new ConfirmIntentionWindowViewModel();

        public ConfirmIntentionWindow(TradeRequestData data)
        {
            InitializeComponent();
            _viewModel.Orignal = data;
            this.DataContext = _viewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                if (this.replace_lb.SelectedIndex == 0)
                {
                    _viewModel.Confirm();
                }
                else
                {
                    _viewModel.DoReplaceCommand();
                }
            }
            Close();
        }

        private void SelectedInstitutionCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterInstitution(args);
            }
        }

        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (_viewModel != null)
            {
                _viewModel.FilterTrader(args);
            }
        }
    }
}
