using STC.Common.Model;
using STC.PT.Plugin.ViewModel;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// PTInnerSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class PTInnerSettingView : UserControl, ISetting
    {
        private PTInnerSettingViewModel vm = PTInnerSettingViewModel.Instance;
        public PTInnerSettingView()
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        public string GetName()
        {
            return "成交管理";
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
