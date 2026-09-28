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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// TextBoxBlock.xaml 的交互逻辑
    /// </summary>
    public partial class TextBoxBlock : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBoxBlock), new PropertyMetadata(string.Empty, OnTextChangedCallback));


        public TextBoxBlock()
        {
            InitializeComponent();
        }

        public static void OnTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxBlock && e.NewValue is string)
            {
                TextBoxBlock pbox = (TextBoxBlock)d;
                string str = (string)e.NewValue;
                pbox.Text = str.Trim();
            }
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    TB_Name.Visibility = Visibility.Collapsed;
                    TBox_Name.Visibility = Visibility.Visible;
                    //Utils.wpf.DragDrop.CanDrop = false;
                }
                else
                {
                    TB_Name.Visibility = Visibility.Visible;
                    TBox_Name.Visibility = Visibility.Collapsed;
                    Utils.wpf.DragDrop.CanDrop = true;
                }

                SetValue(TextProperty, value);
                TB_Name.Text = Text;
                TBox_Name.Text = Text;
            }
        }


        private void TBoxName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TBoxName_KeyDown(sender, null);
        }

        private void TBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            int tt;
            if (!string.IsNullOrEmpty(TBox_Name.Text.Trim()) && int.TryParse(TBox_Name.Text.Trim(), out tt))
            {
                if (e == null || e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(TBox_Name.Text))
                    {
                        this.Focus();
                    }

                    TB_Name.Visibility = Visibility.Visible;
                    TBox_Name.Visibility = Visibility.Collapsed;
                    Utils.wpf.DragDrop.CanDrop = true;

                    Text = TBox_Name.Text.Trim();
                    //var win = Window.GetWindow(this);
                    //if (win != null)
                    //    win.Focus();
                }
            }
            else if (!string.IsNullOrEmpty(TBox_Name.Text.Trim()))
            {
               TBox_Name.Focus();
            }
        }

        private void TBoxName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TBox_Name.Text = string.Empty;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                TB_Name.Visibility = Visibility.Collapsed;
                TBox_Name.Visibility = Visibility.Visible;
                Utils.wpf.DragDrop.CanDrop = false;
                TBox_Name.Text = TB_Name.Text.Trim();

                TBox_Name.SelectAll();
                TBox_Name.Focus();
            }
        }

        /// <summary>
        /// 处理非法字符 只支持 1 - 99999
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBox_Name_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(TBox_Name.Text) && TBox_Name.Text.Length == 5 && string.IsNullOrEmpty(TBox_Name.SelectedText))
            {
                e.Handled = true;
            }
            //else if (!string.IsNullOrEmpty(TBox_Name.Text) && e.Text.IsNumber())
            //{

            //}
            //else if (string.IsNullOrEmpty(TBox_Name.Text) && e.Text.IsNumberWithoutZero())
            //{

            //}
            //else
            //{
            //    e.Handled = true;
            //}
        }

        private void TBox_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number;
            if (!string.IsNullOrEmpty(TBox_Name.Text) && int.TryParse(TBox_Name.Text, out number) && number <= 99999 && number > 0)
            {
                //Utils.wpf.DragDrop.CanDrop = false;
            }
            else
            {
                TBox_Name.Text = string.Empty;
            }
        }
    }
}
