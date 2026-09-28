using STC.Common.Model;
using STC.Common.ViewModel.UI;
using STC.PP.Plugin.Handler;
using STC.PP.Plugin.ViewModel;
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

namespace STC.PP.Plugin.View
{
    /// <summary>
    /// WhiteSecuritySettingView.xaml 的交互逻辑
    /// </summary>
    public partial class PPColumnsSetView : UserControl
    {
        private PPColumnsSetViewModel _msgAlertSettingViewModel;

        public Window myWindow { get; set; }

        public PPColumnsSetView()
        {
            InitializeComponent();
            _msgAlertSettingViewModel = new PPColumnsSetViewModel(this);
            this.DataContext = _msgAlertSettingViewModel;
        }

        public void Close()
        {
            this.myWindow?.Close();
        }

        public void SetOwnerWin(Window win)
        {
            this.myWindow = win;
        }

        public void SetSetGridDatas(MsgAlertDataModel dgc)
        {
            _msgAlertSettingViewModel.FullDataGridConfig = dgc;

            if (_msgAlertSettingViewModel.GridDatas.Count == 0)
                _msgAlertSettingViewModel.SetGridDatas(dgc);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //if (sender != null)
            //{
            //    CheckBox chk = sender as CheckBox;
            //    if (chk.DataContext is MsgAlertModel)
            //    {
            //        if (!(this.DataContext as MsgAlertSettingViewModel).IsDisposed)
            //            (this.DataContext as MsgAlertSettingViewModel).UpdateGridDatas(chk.DataContext as MsgAlertModel, chk.IsChecked ?? false);
            //    }
            //}
        }
    }
}
