using STC.Common.Model;
using STC.Common.ViewModel.UI;
using System;
using System.Collections.Generic;
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

namespace BDB.Common.View.UI
{
    /// <summary>
    /// WhiteSecuritySettingView.xaml 的交互逻辑
    /// </summary>
    public partial class MsgAlertSettingView : UserControl, ISetting
    {
        private BDB.Trade.Plugin.ViewModel.UserSetting.MsgAlertSettingViewModel _msgAlertSettingViewModel;
        public MsgAlertSettingView()
        {
            InitializeComponent();
            _msgAlertSettingViewModel = new BDB.Trade.Plugin.ViewModel.UserSetting.MsgAlertSettingViewModel();
            this.DataContext = _msgAlertSettingViewModel;
        }

        public string GetName()
        {
            return "";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                CheckBox chk = sender as CheckBox;
                if (chk.DataContext is MsgAlertModel)
                {
                    if (!(this.DataContext as BDB.Trade.Plugin.ViewModel.UserSetting.MsgAlertSettingViewModel).IsDisposed)
                        (this.DataContext as BDB.Trade.Plugin.ViewModel.UserSetting.MsgAlertSettingViewModel).UpdateGridDatas(chk.DataContext as MsgAlertModel, chk.IsChecked ?? false);
                }
            }
        }
    }
}
