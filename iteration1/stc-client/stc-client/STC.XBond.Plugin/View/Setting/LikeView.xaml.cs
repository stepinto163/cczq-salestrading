using STC.Common.Model;
using STC.Common.Utils;
using STC.XBond.Plugin.ViewModel.Setting;
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

namespace STC.XBond.Plugin.View.Setting
{
    /// <summary>
    /// LikeView.xaml 的交互逻辑
    /// </summary>
    public partial class LikeView : UserControl, ISetting
    {
        private LikeViewModel _likeViewModel = null;

        public LikeView()
        {
            InitializeComponent();

            if (LocalEnv.ConfInfo.IsAccountUser != "1")
            {
                DefaultGroup4Orders.Visibility = Visibility.Collapsed;
            }

            this.Loaded += LikeView_Loaded;
            _likeViewModel = new LikeViewModel();

            this.DataContext = _likeViewModel;
        }

        private void LikeView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= LikeView_Loaded;

            if (_likeViewModel != null)
                _likeViewModel.Init(this);
        }

        public void OnSave()
        {
            if (_likeViewModel != null)
                _likeViewModel.OnOKClick();
        }

        public string GetName()
        {
            return "偏好设置";
        }

        public bool NeedSaveButton()
        {
            return true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            string tag = tb.Tag as string;
            Regex regex = null;

            if (tag == "1" || tag == "hour" || tag == "minute" || tag == "second")
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
                if (_likeViewModel.DefaultSecondItemData != null)
                    max = _likeViewModel.DefaultSecondItemData.Count;
                else
                    max = 0;

                min = 0;
            }
            else if (tag == "4")//BCO随机渠道
            {
                if (_likeViewModel.DefaultSecondItemDataBCO != null)
                    max = _likeViewModel.DefaultSecondItemDataBCO.Count;
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
            else if (tag == "hour")
            {
                max = 23;
                min = 0;
            }
            else if (tag == "minute" || tag == "second")
            {
                max = 59;
                min = 0;
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
            if (selectTxt.Length == text.Length)//&& (tag != "4" && tag != "2"))
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

            if (decimal.TryParse(text, out d))
            {
                if (text != d.ToString())
                {
                    e.Handled = true;
                    return;
                }
            }

            if (decimal.TryParse(text, out d) && (tag == "hour" || tag == "minute" || tag == "second") && d > 99)
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                            && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;

                return;
            }
        }

        #region 有效时长
        /// <summary>
        /// 禁止粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 检查输入范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidMinute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            Regex regex = null;
            regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;

            string tag = tb.Tag as string;
            int max = 500;
            int min = (tag == "0") ? 0 : 1;

            if (tag == "0")// [0-60)
            {
                max = 59;
            }
            else if (tag == "1") // [0-500]
            {
                min = 0;
                max = 500;
            }
            else if (tag == "2") // (0-24]
            {
                max = 24;
            }
            else if (tag == "3") // [0-60)
            {
                min = 0;
                max = 59;
            }
            else if (tag == "4") // [0-120]
            {
                min = 0;
                max = 120;
            }
            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals(".")) || (text.Contains("-") && e.Text.Equals("-")))
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
        }

        private void bdTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbSettle2.IsDropDownOpen = !cbSettle2.IsDropDownOpen;
        }


        #endregion

        private void AccountUserCbc_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void AccountUserCbc_DropDownOpened(object sender, EventArgs e)
        {

        }

    }
}
