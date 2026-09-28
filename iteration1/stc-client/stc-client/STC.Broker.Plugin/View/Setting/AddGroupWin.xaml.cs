using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.View.Component;
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

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// AddGroupWin.xaml 的交互逻辑
    /// </summary>
    public partial class AddGroupWin : WindowBase
    {
        private InstSettingWinViewModel _settingWinViewModel;
        public AddGroupWin(InstSettingWinViewModel instSettingWinView)
        {
            InitializeComponent();
            _settingWinViewModel = instSettingWinView;
        }

        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int priority = 1;
            if (_settingWinViewModel != null
                && _settingWinViewModel.IssueGroupViewModels != null)
            {
                if (_settingWinViewModel.IssueGroupViewModels.Count > 0)
                {
                    foreach (IssueGroupViewModel item in _settingWinViewModel.IssueGroupViewModels)
                    {
                        if (item.Priority > priority)
                        {
                            priority = item.Priority;
                        }
                    }
                    priority = priority + 1;
                }
                bool isSuccess = await _settingWinViewModel.AddGroup(this.tb_GroupName.Text.Trim(), priority);
                if (isSuccess)
                {
                    this.Close();
                }
            }
        }

        private void btnCloseWin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void tbGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tb_GroupName.Text.Trim()))
            {
                this.tb_GroupName.ValidationResult = false;
                this.tb_GroupName.ErrorMessage = "分组名称不允许为空!";
                this.btnOk.IsEnabled = false;
                return;
            }

            if (_settingWinViewModel.IssueGroupViewModels.Where(o => o.GroupName == this.tb_GroupName.Text.Trim()).Any())
            {
                this.tb_GroupName.ValidationResult = false;
                this.tb_GroupName.ErrorMessage = "已经存在相同分组名称!";
                this.btnOk.IsEnabled = false;
            }
            else
            {
                this.tb_GroupName.ValidationResult = true;
                this.tb_GroupName.ErrorMessage = "";
                this.btnOk.IsEnabled = true;
            }
        }
    }
}

