using STC.Common.Utils;
using STC.XBond.Plugin.ViewModel;
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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// UserSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class UserSettingView : Window
    {
        private UserSettingModel _userSettingModel;

        public UserSettingView()
        {
            InitializeComponent();

            this.Icon = LocalEnv.ServerInfo.AppImage;

            _userSettingModel = new UserSettingModel();
            this.DataContext = _userSettingModel;
            _userSettingModel.Init(this);
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private UpdateUserPassUI lastUI = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (lastUI == null)
            {
                lastUI = new UpdateUserPassUI();
                lastUI.Closed += lastUIClosed;

                lastUI.Show();
            }
            else
                lastUI.Activate();
            //if (lastUI != null && lastUI.Activate())
            //{
            //    return;
            //}
            //UpdateUserPassUI ui = new UpdateUserPassUI();
            //lastUI = ui;
            //ui.Show();
            //DeleteBoxView deleteBoxView = new DeleteBoxView();
            //deleteBoxView.Title = "提示";
            //deleteBoxView.setWarnMsg("该操作请联系管理员");
            //deleteBoxView.Show();
        }

        private void lastUIClosed(object sender, EventArgs e)
        {
            lastUI.Closed -= lastUIClosed;
            lastUI = null;
        }

        private bool isBNCFirst = true;
        private void lbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isBNCFirst)
            {
                isBNCFirst = false;
                return;
            }

            //if (_userSettingModel.DirectionChecked)
            //    _userSettingModel.UpdateData();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;
            Regex regex = null;

            if (tag == "1")
                regex = new Regex("^[0-9]+$");
            else if (tag == "2" || tag == "4")
                regex = new Regex("^[0-9.-]+$");
            else if (tag == "3" || tag == "5" || tag == "6")//过期时间
                regex = new Regex("^[0-9]+$");


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
            else if (tag == "3")//过期时间
            {
                max = 120;
                min = 1;
            }
            else if (tag == "2")//BNC随机渠道
            {
                if (_userSettingModel.DefaultSecondItemData != null)
                    max = _userSettingModel.DefaultSecondItemData.Count;
                else
                    max = 0;

                min = 0;
            }
            else if (tag == "4")//BCO随机渠道
            {
                if (_userSettingModel.DefaultSecondItemDataBCO != null)
                    max = _userSettingModel.DefaultSecondItemDataBCO.Count;
                else
                    max = 0;

                min = 0;
            }
            else if (tag == "5")//NonFirm过期时间
            {
                max = 300;
                min = 1;
            }
            else if (tag == "6")//NonFirm点价窗口
            {
                max = 300;
                min = 1;
            }

            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;

            decimal d = 0;
            if (selectTxt.Length == text.Length )//&& (tag != "4" && tag != "2"))
            {
                if (decimal.TryParse(e.Text, out d) && (d > max || d < min))// && tag == "3")
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
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8)
                {
                    e.Handled = true;
                    return;
                }
            }
        }


        private bool isBCOFirst = true;
        private void lbBCODirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isBCOFirst)
            {
                isBCOFirst = false;
                return;
            }

            //if (_userSettingModel.DirectionCheckedBCO)
            //    _userSettingModel.UpdateData();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();


                }
                catch { }
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
    }
}
