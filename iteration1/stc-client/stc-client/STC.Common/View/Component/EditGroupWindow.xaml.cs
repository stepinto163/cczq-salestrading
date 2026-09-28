using Newtonsoft.Json;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Component;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.Common.View.Component
{
    /// <summary>
    /// FilterGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditGroupWindow : WindowBase
    {
        private EditGroupWindowViewModel _filterGroupWinViewModel;
        public Action EditAction; 
        public EditGroupWindow(ObservableCollection<FilterGroupItem> groupList, string groupid)
        {
            InitializeComponent();
            _filterGroupWinViewModel = new EditGroupWindowViewModel(groupList, groupid);
            this.DataContext = _filterGroupWinViewModel;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMessageBox.Question("是否保存组管理信息？","组管理"))
            {
                _filterGroupWinViewModel.Save(this);
            }
        }

        private void NewGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            _filterGroupWinViewModel.NewOne();
            //DataGridRow row = (DataGridRow)this.GroupList.ItemContainerGenerator.ContainerFromItem(_filterGroupWinViewModel.SelectedItem);
            ////Get the cell from first row
            //var cell = DataGrid.GetCell(0, 2);
            ////Focus on the cell for editing
            //cell.Focus();
            //Start editing the cell
            this.GroupList.BeginEdit();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GroupList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if(e.Row.Item is FilterGroupItem)
            {
                FilterGroupItem item = e.Row.Item as FilterGroupItem;
                if (item.Inf1 == "BOND")
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void GroupList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is FilterGroupItem)
            {
                FilterGroupItem item = e.Row.Item as FilterGroupItem;
                if (string.IsNullOrEmpty(item.GroupName_Edit))
                {
                    item.GroupName_Edit = item.GroupName;
                    return;
                }
            }
        }

        private void GroupList_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }

        private void GroupList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            _filterGroupWinViewModel.SetCurrentGroupInf2();
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_filterGroupWinViewModel.IsNeedUpdate())
            {
                if (!CustomMessageBox.Question("组管理信息有数据变化，是否退出编辑?","组管理"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            _filterGroupWinViewModel.UnsubscribeAppMsg();
            _filterGroupWinViewModel.clearGroup();
        }
    }
}
