using STC.Common.Handler.Base;
using STC.Common.Model.Execute;
using STC.Common.Model.Fix.Message;
using STC.Execute.Plugin.Handler.Process;
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
using System.Windows.Threading;

namespace STC.Execute.Plugin.View.Execute
{
    /// <summary>
    /// ExecuteOrderDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class ExecuteOrderDetailView : UserControl
    {
        private ExecuteOrderDetailViewModel _executeOrderDetailViewModel = null;
        ContextMenu contextMenu;
        public ExecuteOrderDetailView()
        {
            InitializeComponent();

            _executeOrderDetailViewModel = new ExecuteOrderDetailViewModel();
            this.DataContext = _executeOrderDetailViewModel;
            tBspare.Focus();
            BaseHandler.Instance.Init(_executeOrderDetailViewModel.Instance_OnClear);
            _executeOrderDetailViewModel.CancelRemarkClick();
        }

        public void OnOrder(DataRow dataRowView)
        {
            if (_executeOrderDetailViewModel != null)
                _executeOrderDetailViewModel.OnOrder(dataRowView);
        }
        
        public void RefreshUI()
        {
            if (_executeOrderDetailViewModel != null)
                _executeOrderDetailViewModel.ClearUI();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _executeOrderDetailViewModel.EditRemarkClick();
            TextBox tb = sender as TextBox;
            tb.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                 tBspare.Focus();
                _executeOrderDetailViewModel.SaveRemarkClick();
            }
        }

      
        private void SetContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = (ContextMenu)this.FindResource("CopyContextMenu");
            }
        }
        private void copyRow_Click(object sender, RoutedEventArgs e)
        {
            Common.Utils.ClipboardHelper.CopyToClipboard(txAllocText.SelectedText, true);
        }

        private void txAllocText_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetContextMenu();
            contextMenu.IsOpen = true;
        }

        public void setButtonVisibility(Visibility visibility)
        {
            btnFullScreen.Visibility = visibility;
        }
      
    }
}
