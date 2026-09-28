using STC.Common.Handler.Base;
using STC.Execute.Plugin.ViewModel.Execute;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace STC.Execute.Plugin.View.Execute
{
    /// <summary>
    /// AlterHistoryAll.xaml 的交互逻辑
    /// </summary>
    public partial class AlterHistoryAll : UserControl
    {
        private AlterHistoryViewModel _alterHistoryViewModel = null;
        public AlterHistoryAll()
        {
            InitializeComponent();
            _alterHistoryViewModel = new AlterHistoryViewModel();
            this.DataContext = _alterHistoryViewModel;
            BaseHandler.Instance.Init(_alterHistoryViewModel.Instance_OnClear);
        }

        public void OnOrder(DataRow dataRowView)
        {
            if (_alterHistoryViewModel != null)
                _alterHistoryViewModel.OnOrder(dataRowView);
        }

        private void btConfirm_Click(object sender, RoutedEventArgs e)
        {
            Feedpop.IsOpen = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Feedpop.IsOpen = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Feedpop.IsOpen = false;
        }
    }
}
