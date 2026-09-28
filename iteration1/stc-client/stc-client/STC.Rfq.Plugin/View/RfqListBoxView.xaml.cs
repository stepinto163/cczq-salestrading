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
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Frame.CustomUI.Component;
using STC.Rfq.Plugin.Model;
using STC.Rfq.Plugin.Utils.Data;
using STC.Rfq.Plugin.View.HomePage;
using STC.Rfq.Plugin.ViewModel;

namespace STC.Rfq.Plugin.View
{
    /// <summary>
    /// MMListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class RfqListBoxView : UserControl
    {
        public RfqListBoxView()
        {
            InitializeComponent();
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                var item = this.MMListView.ItemContainerGenerator.ContainerFromItem((sender as CheckBox).DataContext as RfqstgViewDataModel);
                (item as DataGridRow).IsSelected = true;
                e.Handled = true;

                if (this.MMListView.DataContext != null && this.MMListView.DataContext is RfqListBoxViewModel)
                {
                    RfqListBoxViewModel mmListBoxViewModel = (this.MMListView.DataContext as RfqListBoxViewModel);
                    mmListBoxViewModel.OnEnableCommand(mmListBoxViewModel.SelectedStg.id);
                }
            }
            }

        private void del_btn_Click(object sender, RoutedEventArgs e)
        {
            var item = this.MMListView.ItemContainerGenerator.ContainerFromItem((sender as Button).DataContext as RfqstgViewDataModel);
            (item as DataGridRow).IsSelected = true;
 
            if (this.MMListView.DataContext != null && this.MMListView.DataContext is RfqListBoxViewModel)
            {
                RfqListBoxViewModel mmListBoxViewModel = (this.MMListView.DataContext as RfqListBoxViewModel);
                string securityID = mmListBoxViewModel.SelectedStg.securityid;
                string symbol = mmListBoxViewModel.SelectedStg.securityid;
                string enable = mmListBoxViewModel.SelectedStg.securityid;

                if ("True" == enable)
                {
                    CustomMessageBox.Warning($"债券\"{securityID}\"未关闭策略，请确认关闭后再删除");
                    return;
                }

                string text = $"确定要删除债券\"{securityID}\"吗？";// " " + symbol + "？";
                string title = "删除债券";
                if (CustomMessageBox.Question(text, title) == true)
                {
                    if (this.MMListView.DataContext != null && this.MMListView.DataContext is RfqListBoxViewModel)
                    {
                        mmListBoxViewModel.DelBond();
                    }
                }
            }
        }
    }
}
