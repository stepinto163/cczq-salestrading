using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.ViewModel;
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
using System.Windows.Shapes;

namespace STC.Frame.UI
{
    /// <summary>
    /// NetworkSetting.xaml 的交互逻辑
    /// </summary>
    public partial class NetworkSetting : WindowBase
    {
        NetworkSettingViewModel networkSettingVM = new NetworkSettingViewModel();

        public NetworkSetting()
        {
            InitializeComponent();
            this.DataContext = networkSettingVM;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.networkSettingVM.SaveProxyInfo();
            this.Close();
        }
    }
}
