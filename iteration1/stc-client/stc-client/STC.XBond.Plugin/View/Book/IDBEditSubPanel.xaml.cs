using STC.Common.Utils;
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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// IDBEditSubPanel.xaml 的交互逻辑
    /// </summary>
    public partial class IDBEditSubPanel : UserControl
    {
        private const string HeaderPanelKey = "datePicker";
        public IDBEditSubPanel()
        {
            InitializeComponent();
            //DatePicker myDatePicker = GetTemplateChild(HeaderPanelKey) as DatePicker;
            //if (myDatePicker != null)
            //{
            //    myDatePicker.BlackoutDates.AddDatesInPast();
            //}
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            int max = 1000;
            int min = -1000;

            if (tag == "1")//量
            {
                max = 1000000;
                min = 1;
            }
            else if (tag == "2")//价格
            {
                max = 9999;
                min = -100;
            }
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            else if (tag == "4")//随机渠道
            {
                //max = viewModel.RandomMax;
                //min = 0;
            }

            string text = tb.Text;
            string oldText = text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            if (selectTxt.Length == text.Length)
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))
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

                if (decimal.TryParse(newNum, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
                return;
            }


            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);


            if (decimal.TryParse(text, out d) && (d > max || d < min))
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

        private void dg_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = CommonUtils.FindVisualChild<ScrollViewer>(dg);
            bool isAtBottom = CommonUtils.IsVerticalScrollBarAtButtom(scrollViewer);
            if ((isAtBottom && e.Delta < 0)  || (scrollViewer.VerticalOffset == 0 && e.Delta > 0) || scrollViewer.ExtentHeight < scrollViewer.ActualHeight)
            {
                MouseWheelEventArgs eventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArgs.RoutedEvent = UIElement.MouseWheelEvent;
                eventArgs.Source = sender;
                dg.RaiseEvent(eventArgs);
            }
        }

        private void WatermarkTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;

            Regex regex = new Regex("^[A-Za-z0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text;

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            
            if (text.Length > 10)
            {
                e.Handled = true;
                return;
            }
        }

        private void WatermarkTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                        && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }
    }
}
