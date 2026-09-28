using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.ViewModel.Setting;
using STC.InnerBoard.Plugin.ViewModel;
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

namespace STC.Common.View.Setting
{
    /// <summary>
    /// InnerSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class InnerSettingView : UserControl, ISetting
    {
        private InnerSettingViewModel vm = new InnerSettingViewModel();

        public InnerSettingView()
        {
            InitializeComponent();
            if (CommonUtils.isOnlyCredit)
            {
                this.rateSP.Visibility = Visibility.Collapsed;
            }

            if (CommonUtils.isOnlyRate)
            {
                this.creditSP.Visibility = Visibility.Collapsed;
            }
            this.DataContext = vm;
        }

        public string GetName()
        {
            return "行情复制";
        }

        public bool NeedSaveButton()
        {
            return true;
        }

        public void OnSave()
        {
            if (vm != null)
                vm.SaveParam();
        }
    }
}
