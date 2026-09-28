
using BDB.Price.Plugin.ViewModel.Trade;
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

namespace BDB.Price.Plugin.View.Trade
{
    /// <summary>
    /// MsgSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MsgSettingWindow : Window
    {

        MsgSettingViewModel msgSettingVM = new MsgSettingViewModel();

        public MsgSettingWindow()
        {
            InitializeComponent();

            this.DataContext = msgSettingVM;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Enter
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                if (msgSettingVM != null && msgSettingVM.OkCommand.CanExecute(null))
                    msgSettingVM.OkCommand.Execute(null);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tag = string.Empty;
            // tag 改变用途
            //if (tb.Name == "BestBp")
            //{
            //    tag = "1";
            //}
            //else 
            if ((tb.Name == "BestBidLimit") || (tb.Name == "BestOfrLimit") || (tb.Name == "BestBp"))
            {
                tag = "2";
            }
            else
            {
                tag = tb.Tag as string;
            }

            Regex regex = null;

            if (tag == "1" || tag == "3" || tag == "4")//量
                regex = new Regex("^[0-9]+$");
            else if (tag == "2")
                regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 1000;
            int min = -1;

            if (tag == "1")//量
            {
                max = 1000;
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
            //else if (tag == "4")//随机渠道
            //{
            //    max = viewModel.RandomMax;
            //    min = 1;
            //}

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

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

                if (tag == "1") //量
                {
                    if (text != d.ToString())
                    {
                        e.Handled = true;
                        return;
                    }

                }
            }

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


            e.Handled = false;
        }


        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tBox = sender as TextBox;

            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            if (!(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.KeyboardDevice.IsKeyDown(Key.Enter)))
            {
                //AmountTxt.Focus();
                //// 选择最后位数
                //int pos = AmountTxt.Text.Length;
                //if (pos > 0)
                //{
                //    int sellength = AmountTxt.Text.Length;
                //    AmountTxt.SelectionStart = 0;
                //    AmountTxt.SelectionLength = sellength;
                //}
                e.Handled = true;
                return;
            }
            // 判读是否是功能键
            // tag 改变用途
            string tag = string.Empty;


            // 功能键有效性检查
            string text = tBox.Text;

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                decimal d = 0;
                if (text.Length > 0)
                {
                    if (!decimal.TryParse(text, out d))
                    {
                        return;
                    }
                }

                // tag 改变用途
                //if (tBox.Name == "BestBp")
                //{
                //    tag = "1";
                //}
                //else 
                if ((tBox.Name == "BestBidLimit") || (tBox.Name == "BestOfrLimit") || (tBox.Name == "BestBp"))
                {
                    tag = "2";
                }
                else
                {
                    tag = tBox.Tag as string;
                }
                int max = 1000;
                int min = 0;

                if (tag == "1")//量
                {
                    max = 1000;
                    min = 0;
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

                if (decimal.TryParse(text, out d) && (d > max || d < min))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

    }
}
