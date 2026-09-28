using STC.Common.Model;
using STC.Common.View.Component;
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

namespace STC.Common.View.UI
{
    /// <summary>
    /// WhiteSecuritySettingView.xaml 的交互逻辑
    /// </summary>
    public partial class WhiteSecuritySettingView : UserControl, ISetting
    {
        private WhiteSecuritySettingViewModel _whiteSecuritySettingViewModel;
        public WhiteSecuritySettingView()
        {
            InitializeComponent();
            _whiteSecuritySettingViewModel = new WhiteSecuritySettingViewModel();
            this.DataContext = _whiteSecuritySettingViewModel;
            this.Loaded += WhiteSecuritySettingView_Loaded;
        }

        private void WhiteSecuritySettingView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= WhiteSecuritySettingView_Loaded;
            if (_whiteSecuritySettingViewModel != null)
                _whiteSecuritySettingViewModel.Init();
        }

        public string GetName()
        {
            return "黑名单审批";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {

        }

        private void SettingSearchTextBox_AfterSearch(Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null)
            {
               (this.DataContext as WhiteSecuritySettingViewModel).RefreshSecurityid(obj.Bond_ID);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;
            if ((tBox.Text.Length > 200) && (e.Key != Key.Delete) && (e.Key != Key.Back))
            { 
                e.Handled = true;
                return;
            }
        }
    }

}
