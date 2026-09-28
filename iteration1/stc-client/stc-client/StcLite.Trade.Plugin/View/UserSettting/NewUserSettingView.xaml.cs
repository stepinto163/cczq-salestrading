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
using BDB.Trade.Plugin.ViewModel.UserSetting;
using System.Text.RegularExpressions;
using STC.Common.Model;

namespace BDB.Trade.Plugin.View.UserSetting
{
    /// <summary>
    /// UserSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class NewUserSettingView : UserControl, ISetting
    {
        private NewUserSettingViewModel _userSettingModel;
        public NewUserSettingView()
        {
            InitializeComponent();
            _userSettingModel = new NewUserSettingViewModel();
            this.DataContext = _userSettingModel;
            _userSettingModel.Init(this);
        }

        public string GetName()
        {
            return "布局设置";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {

        }

        #region min
        private void txt_qtyMin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            // 输入框允许输入范围[1000，1000000]
            int max = 10000000;
            int min = 100;

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
        }


        private void txt_qtyMin_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }
        #endregion
    }
}
