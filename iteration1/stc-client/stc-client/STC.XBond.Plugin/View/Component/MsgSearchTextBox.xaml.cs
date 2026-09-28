using STC.Common.Utils;
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

namespace STC.XBond.Plugin.View.Component
{
    /// <summary>
    /// MsgSearchTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class MsgSearchTextBox : UserControl
    {
        private bool TextChangeFlag = true;
        private bool IsInit = true;
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string),
            typeof(MsgSearchTextBox), new PropertyMetadata("", new PropertyChangedCallback(OnSearchTextPropertyChange)));
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MsgSearchTextBox));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set
            {
                SetValue(SearchTextProperty, value);
                RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
            }
        }
        static void OnSearchTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MsgSearchTextBox source = (MsgSearchTextBox)sender;

            string newValue = args.NewValue.ToString();
            if (source.SearchText != newValue)
            {
                source.SearchText = newValue;
            }
            if (source.SearchTermTextBox.Text != newValue)
            {
                source.SearchTermTextBox.Text = newValue;
            }
        }
        public MsgSearchTextBox()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            IsInit = true;
            if (this.SearchText == string.Empty)
            {
                this.SearchTermTextBox.Text = string.Empty;
            }
            else
            {
                this.SearchText = string.Empty;
            }
            SearchTermTextBox.Focus();
            IsInit = false;
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }

            if (!TextChangeFlag)
                return;
            if (IsInit)
                return;
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private bool isFromEnter = false;
        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                this.SearchText = SearchTermTextBox.Text;
            }
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            IsInit = false;
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
        }

        private void tbTraderAInfoChange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = true;
        }

        private void tbTraderAInfoChange_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            TextChangeFlag = false;
        }
    }
}
