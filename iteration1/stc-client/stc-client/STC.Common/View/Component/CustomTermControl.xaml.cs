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

namespace STC.Common.View.Component
{
    /// <summary>
    /// CustomTermControl.xaml 的交互逻辑
    /// </summary>
    public partial class CustomTermControl : UserControl
    {
        /// <summary>
        /// 确认事件
        /// </summary>
        public static readonly RoutedEvent OKActionEvent = EventManager.RegisterRoutedEvent("OKAction", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomTermControl));

        public event RoutedEventHandler OKAction
        {
            add { AddHandler(OKActionEvent, value); }
            remove { RemoveHandler(OKActionEvent, value); }
        }

        public string StartTermDY
        {
            get { return (string)GetValue(StartTermDYProperty); }
            set { SetValue(StartTermDYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTermDYProperty =
            DependencyProperty.Register("StartTermDY", typeof(string), typeof(CustomTermControl), new PropertyMetadata(string.Empty, onStartTermDYChanged));

        private static void onStartTermDYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTermControl source = d as CustomTermControl;
            source.startTerm.Text = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }

        public string EndTermDY
        {
            get { return (string)GetValue(EndTermDYProperty); }
            set { SetValue(EndTermDYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTermDY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTermDYProperty =
            DependencyProperty.Register("EndTermDY", typeof(string), typeof(CustomTermControl), new PropertyMetadata(string.Empty, onEndTermDYChanged));

        private static void onEndTermDYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTermControl source = d as CustomTermControl;
            source.endTerm.Text = e.NewValue == null ? string.Empty : e.NewValue.ToString();
        }

        public CustomTermControl()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = null;
            regex = new Regex("^[0-9ydDY.]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            string text = tb.Text.ToLower();

            if ((text.Contains("y") && e.Text.Equals("y"))
                || (text.Contains("d") && e.Text.Equals("d"))
                || (text.Contains(".") && e.Text.Equals(".")))
            {
                e.Handled = true;
                return;
            }

            decimal d = 0;
            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            string temp = text.Replace("d", ",").Replace("y", ",");
            string[] sectionArr = text.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            foreach (string textSec in sectionArr)
            {
                string[] arr = textSec.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    if (decimal.TryParse(arr[1], out d) && arr[1].Length > 4)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void dyBtn_Click(object sender, RoutedEventArgs e)
        {
            dyBtn.Visibility = Visibility.Collapsed;
            startTerm.Visibility = Visibility.Collapsed;
            endTerm.Visibility = Visibility.Collapsed;
            startTerm.Text = string.Empty;
            endTerm.Text = string.Empty;

            calendarBtn.Visibility = Visibility.Visible;
            startDatePicker.Visibility = Visibility.Visible;
            endDatePicker.Visibility = Visibility.Visible;

        }

        private void calendarBtn_Click(object sender, RoutedEventArgs e)
        {
            dyBtn.Visibility = Visibility.Visible;
            startTerm.Visibility = Visibility.Visible;
            endTerm.Visibility = Visibility.Visible;

            calendarBtn.Visibility = Visibility.Collapsed;
            startDatePicker.Visibility = Visibility.Collapsed;
            endDatePicker.Visibility = Visibility.Collapsed;
            startDatePicker.SelectedDate = null;
            endDatePicker.SelectedDate = null;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            StartTermDY = this.startTerm.Text;
            EndTermDY = this.endTerm.Text;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = OKActionEvent });
        }

        public void ClearValue()
        {
            this.startTerm.Text = string.Empty;
            this.endTerm.Text = string.Empty;
        }

        private void Term_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //this.okBtn_Click(null, null);
                tb.Focus();
            }
        }

        private void startTerm_LostFocus(object sender, RoutedEventArgs e)
        {
            StartTermDY = this.startTerm.Text;
            EndTermDY = this.endTerm.Text;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = OKActionEvent });
        }

        private void endTerm_LostFocus(object sender, RoutedEventArgs e)
        {
            StartTermDY = this.startTerm.Text;
            EndTermDY = this.endTerm.Text;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = OKActionEvent });
        }
    }
}
