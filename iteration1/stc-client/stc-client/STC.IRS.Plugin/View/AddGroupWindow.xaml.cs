using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
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

namespace STC.IRS.Plugin.View
{
    /// <summary>
    /// AddGroupView.xaml 的交互逻辑
    /// </summary>
    public partial class AddGroupWindow : WindowBase
    {

        private AddGroupViewModel _AddGroupViewModel;

        public Action<FilterGroupItem> AddAction;


        public AddGroupWindow(ObservableCollection<FilterGroupItem> groupList, string focusTags, string groupid,string startTermDY, string endTermDY, bool isAppTerm,
            string startPrice, string endPrice, bool isPrice, string focusType = "TAG")
        {
            InitializeComponent();
            this.newGroupRd.Checked += newGroupRd_Checked;
            this.updateGroupRd.Checked += updateGroupRd_Checked;
            this.groupNameTxt.TextChanged += TextBox_TextChanged;

            if (groupList != null && groupList.Count > 0 && !string.IsNullOrEmpty(groupid))
            {
                updateGroupRd.IsChecked = true;
            }

            _AddGroupViewModel = new AddGroupViewModel(groupList, focusTags, this, groupid, startTermDY, endTermDY, isAppTerm, startPrice, endPrice, isPrice, focusType);
            this.DataContext = _AddGroupViewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.newGroupRd.IsChecked.Value == true)
            {
                _AddGroupViewModel.NewOne();
            }
            else
            {
                _AddGroupViewModel.UpdateOne();
            }
        }

        private void CheckData()
        {
            if  (this.newGroupRd.IsChecked.Value)
            {
                this.btnOK.IsEnabled = !string.IsNullOrEmpty(this.groupNameTxt.Text); 
            }
            else
            {
                this.btnOK.IsEnabled = true;
            }
        }

        private void newGroupRd_Checked(object sender, RoutedEventArgs e)
        {
            CheckData();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckData();
        }

        private void updateGroupRd_Checked(object sender, RoutedEventArgs e)
        {
            CheckData();
        }
    }
}
