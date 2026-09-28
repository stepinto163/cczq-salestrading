using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Mmshe.Plugin.ViewModel;
using System;
using System.Collections.Generic;
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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// UpdateWidenSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWidenSettingView : WindowBase
    {
        private UpdateWidenSettingViewModel _updateWindenSettingViewModel = new UpdateWidenSettingViewModel();
        public UpdateWidenSettingView()
        {
            InitializeComponent();
            this.DataContext = _updateWindenSettingViewModel;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            decimal max = 0;
            decimal min = 0;
            Regex regex = null;


            regex = new Regex("^[0-9.-]+$");
            max = 1000.00m;
            min = -99.9999m;


            if (regex == null) { return; }

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (!CommonUtils.IsNumber(e.Text, 4, 2)
                    || (decimal.TryParse(e.Text, out d) && (d > max || d < min)))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (!CommonUtils.IsNumber(newNum, 4, 2)
                    || (decimal.TryParse(newNum, out d) && (d > max || d < min)))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (!CommonUtils.IsNumber(text, 4, 2)
                    || (decimal.TryParse(text, out d) && (d > max || d < min)))
            {
                e.Handled = true;
                return;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
         e.Command == ApplicationCommands.Cut ||
         e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
