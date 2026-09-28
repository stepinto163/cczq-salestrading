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
    /// TextBoxBlockNoLimite.xaml 的交互逻辑
    /// </summary>
    public partial class TextBoxBlockNoLimite : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBoxBlockNoLimite), new PropertyMetadata(string.Empty, OnTextChangedCallback));

        public event Action<string> TextChanged;


        public TextBoxBlockNoLimite()
        {
            InitializeComponent();
        }

        public static void OnTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxBlockNoLimite && e.NewValue is string)
            {
                TextBoxBlockNoLimite pbox = d as TextBoxBlockNoLimite;
                string str = e.NewValue.ToString();
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
            //TBoxName_KeyDown(sender, null);
        }

        private void TBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(TBox_Name.Text))
                {
                    TBox_Name.Text = TB_Name.Text;
                    TB_Name.Visibility = Visibility.Visible;
                    TBox_Name.Visibility = Visibility.Collapsed;
                    bd_bor.Visibility = Visibility.Visible;
                    TBox_Name.Focus();
                    return;
                }

                Utils.wpf.DragDrop.CanDrop = true;

                TB_Name.Visibility = Visibility.Visible;
                TBox_Name.Visibility = Visibility.Collapsed;

                Text = TBox_Name.Text.Trim();

                TextChanged?.Invoke(Text);
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
                Utils.wpf.DragDrop.CanDrop = false;

                bd_bor.Visibility = Visibility.Collapsed;
                TB_Name.Visibility = Visibility.Collapsed;
                TBox_Name.Visibility = Visibility.Visible;
                TBox_Name.Text = TB_Name.Text.Trim();


                TBox_Name.SelectAll();
                TBox_Name.Focus();
                e.Handled = true;
            }
        }
    }
}

