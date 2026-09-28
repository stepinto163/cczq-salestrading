using STC.Common.Handler.Admin;
using STC.Common.Model;
using STC.Common.Utils;
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

namespace STC.InnerBoard.Plugin.View
{
    /// <summary>
    /// SalesSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SalesSettingView : UserControl, ISetting
    {
        private SalesSettingViewModel vm = new SalesSettingViewModel();

        public SalesSettingView()
        {
            InitializeComponent();
            this.DataContext = vm;
            if (LocalEnv.ConfInfo.IsGroupFQNotEnter)
            {
                rbGroupEnter.IsEnabled = false;
                rbGroupNotEnter.IsEnabled = false;
                rbGroupNotEnter.IsChecked = true;
            }

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                InnerBoard_Part.Visibility = Visibility.Collapsed;
                intention_bp_part.Visibility = Visibility.Collapsed;
                FQ_Part.Visibility = Visibility.Collapsed;
                Client_Part.Visibility = Visibility.Collapsed;
            }

            bool isIntentionPermission = AdminHandler.Instance.CheckPermisionById("4000", "4008");
            bool isPermission = AdminHandler.Instance.CheckPermisionById("4000", "4013");
            if (isPermission == false && isIntentionPermission)
            {
                this.Copy_Part.Visibility = Visibility.Visible;
            }
        }

        public string GetName()
        {
            return "需求管理";
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
