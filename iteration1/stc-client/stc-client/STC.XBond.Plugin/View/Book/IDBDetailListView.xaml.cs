using STC.Common.Model.Admin;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// IDBDetailListView.xaml 的交互逻辑
    /// </summary>
    public partial class IDBDetailListView : WindowBase
    {
        IDBDetailListViewModel _IDBDetailListViewModel = new IDBDetailListViewModel();

        public IDBDetailListView(DataTable targetDatas, BookEditViewModel bookEditViewModel, bool isSelf, StcTbookUsers subID)
        {
            InitializeComponent();
            _IDBDetailListViewModel.TargetDatas = targetDatas;
            _IDBDetailListViewModel.bookEditViewModel = bookEditViewModel;
            _IDBDetailListViewModel.IsSelf = isSelf;
            _IDBDetailListViewModel.SecurityID = bookEditViewModel.SecurityID;
            if (isSelf)
            {
                _IDBDetailListViewModel.Side = bookEditViewModel.Side;
            }
            else
            {
                _IDBDetailListViewModel.Side = bookEditViewModel.Side == "2" ? "1" : "2";
            }
            
            _IDBDetailListViewModel.PartySubID = subID;
            this.DataContext = _IDBDetailListViewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_IDBDetailListViewModel != null)
            {
                _IDBDetailListViewModel.Dispose();
            }
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_IDBDetailListViewModel != null)
            {
                if (_IDBDetailListViewModel.Finish())
                {
                    _IDBDetailListViewModel.Dispose();
                    this.Close();
                }
            }
        }

        private void Selected_Chk_Checked(object sender, RoutedEventArgs e)
        {
            _IDBDetailListViewModel.SelectedQty++;
        }

        private void Selected_Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            _IDBDetailListViewModel.SelectedQty--;
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            _IDBDetailListViewModel.Dispose();
        }
    }
}
