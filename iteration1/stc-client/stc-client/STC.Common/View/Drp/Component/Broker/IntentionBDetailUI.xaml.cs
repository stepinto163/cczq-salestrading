using STC.Common.Handler.PT;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.ViewModel.Drp.Component.Broker;
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

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// IntentionBDetailUI.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionBDetailUI : UserControl
    {
        public IntentionBDetailViewModel viewModel;
        public IntentionBDetailUI()
        {
            InitializeComponent();
            viewModel = new IntentionBDetailViewModel();
            this.DataContext = viewModel;
            SetQtyFocus();
        }


        public void SetShowData(ExecutionReport po, IntentionBUI ui)
        {
            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    viewModel.ParentWindow = ui;
                    viewModel.BindingData(po);

                });
            });
        }

        public void SetQtyFocus()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int sellength = tbAmount.Text.Length;
                tbAmount.Focus();
                tbAmount.SelectionStart = 0;
                tbAmount.SelectionLength = sellength;
            });
        }
        private void btnReject_KeyDown(object sender, KeyEventArgs e)
        {
            IntentionBUIViewModel viewModel = (IntentionBUIViewModel)this.DataContext;
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                //if (viewModel != null)
                //    viewModel.RejectClick();
            }
        }

        private void btnOk_KeyDown(object sender, KeyEventArgs e)
        {
            IntentionBUIViewModel viewModel = (IntentionBUIViewModel)this.DataContext;
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                //if (viewModel != null)
                //    viewModel.OkClick();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;


            Regex regex = new Regex("^[0-9.-]+$");

            decimal max = 2000M;
            decimal min = 0M;

            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;


            string selectTxt = tb.SelectedText;
            decimal d = 0;

            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max)
                {
                    e.Handled = true;
                    return;
                }

            }
            else
            {
                e.Handled = true;
                return;
            }

            if (decimal.TryParse(text, out d) && (d > max || d < min) && d != 0)
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }

        private void tbAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;
            decimal max = 2000.00M;
            decimal min = 0M;
            IntentionBDetailViewModel viewModel = (IntentionBDetailViewModel)this.DataContext;
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            if (tBox.Name.ToString() == "tbAmount")
            {

                if (e.KeyboardDevice.IsKeyDown(Key.Up))
                {
                    viewModel.AddClick();
                    e.Handled = true;
                    return;
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Down))
                {

                    viewModel.SubtractionClick();
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Escape))
            {
                //viewModel.OnCloseSelf();
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                viewModel.OkClick();
                e.Handled = true;
                return;
            }

            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
