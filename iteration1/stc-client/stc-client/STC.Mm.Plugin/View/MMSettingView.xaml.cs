using STC.Common.Model;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Mm.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// MMSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MMSettingView : UserControl, ISetting
    {
        private MMSettingViewModel _view = new MMSettingViewModel();
        public MMSettingView()
        {
            InitializeComponent();
            this.DataContext = _view;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_view != null)
            {
                _view.init();
            }
        }

        public string GetName()
        {
            return "双边做市";
        }

        public bool NeedSaveButton()
        {
            return true;
        }

        public void OnSave()
        {
            if (_view != null)
                _view.OnOKClick();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                            && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string maxAmount = tb.Tag as string;

            int max = 1000;

            int tem = 0;
            if (Int32.TryParse(maxAmount, out tem))
                max = tem;

            int min = 0;
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;

            if (selectTxt.Length == text.Length)
                return;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);

            decimal d = 0;
            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void bookHcbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
