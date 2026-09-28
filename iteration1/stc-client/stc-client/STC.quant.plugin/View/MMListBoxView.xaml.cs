using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.quant.plugin.View.Component;
using STC.quant.plugin.ViewModel;

namespace STC.quant.plugin.View
{
    /// <summary>
    /// MMListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class MMListBoxView : UserControl
    {
        public MMListBoxViewModel viewmodel;
        public MMListBoxView()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            QuantListView.SelectedItem = ((Button)sender).DataContext;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            if (viewmodel != null && c != null)
                viewmodel.UpdateEnable(c.IsChecked.Value);
        }

        private void OperationGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMyOrderOperationGridMargin();
        }

        private void SetMyOrderOperationGridMargin()
        {
            //PART_HorizontalScrollBar
            ScrollBar hScrollBar = CommonUtils.GetVisualChild<ScrollBar>(QuantListView, dp => dp.Name == "PART_HorizontalScrollBar");

            if (hScrollBar != null)
            {
                if (hScrollBar.Visibility == Visibility.Visible)
                {
                    this.OperationGrid.Margin = new Thickness(0, 0, 0, 10);
                }
                else
                {
                    this.OperationGrid.Margin = new Thickness(0);
                }
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (this.DataContext != null && this.DataContext is MMListBoxViewModel)
                {
                    (this.DataContext as MMListBoxViewModel).CheckBoxAllValueChange(flag);
                }
            }
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb != null && cb.DataContext != null && cb.DataContext is DataRowView)
                {
                    DataRowView currentDRV = cb.DataContext as DataRowView;
                    bool IsSwitch = (bool)currentDRV[MMListBoxViewModel.SOURCE_FIELD];
                    if (this.QuantListView.DataContext != null && this.QuantListView.DataContext is MMListBoxViewModel)
                    {
                        MMListBoxViewModel mmListBoxViewModel = (this.QuantListView.DataContext as MMListBoxViewModel);
                        if (IsSwitch)
                            //mmListBoxViewModel.Stop();
                            mmListBoxViewModel.quantUpdateEnable(false, currentDRV);
                        else
                            // mmListBoxViewModel.Start();
                            mmListBoxViewModel.quantUpdateEnable(true, currentDRV);
                    }

                    //string securityID = CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.SECURITY_ID_FIELD]);
                    //string settltype = CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.SETTLE_TYPE_FIELD]);
                    //bool enable;
                    //bool.TryParse(CommonUtils.ConvertToString(currentDRV[MMListBoxViewModel.ENABLE_FIELD]), out enable);

                    //MMUpdateEnableReq req = new MMUpdateEnableReq();
                    //req.securityid = securityID;
                    //req.settltype = settltype;
                    //req.enable = enable ? "0" : "1";

                    //if (this.MMListView.DataContext != null && this.MMListView.DataContext is MMListBoxViewModel)
                    //{
                    //    MMListBoxViewModel mmListBoxViewModel = (this.MMListView.DataContext as MMListBoxViewModel);
                    //    mmListBoxViewModel.MMUpdateEnable(req);
                    //}
                }
            }
        }

        private void del_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender != null && sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb != null && cb.DataContext != null && cb.DataContext is DataRowView)
                {
                    DataRowView currentDRV = cb.DataContext as DataRowView;
                    bool IsSwitch = (bool)currentDRV[MMListBoxViewModel.SOURCE_FIELD];
                    if (this.QuantListView.DataContext != null && this.QuantListView.DataContext is MMListBoxViewModel)
                    {
                        MMListBoxViewModel mmListBoxViewModel = (this.QuantListView.DataContext as MMListBoxViewModel);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            mmListBoxViewModel.AdjustAllChkStatus();
                        }));
                    }
                }
            }
        }

    }
}
