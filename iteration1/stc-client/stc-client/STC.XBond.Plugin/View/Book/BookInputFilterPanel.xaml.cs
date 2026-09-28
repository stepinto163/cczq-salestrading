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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// BookInputFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class BookInputFilterPanel : UserControl
    {
        public BookInputFilterPanel()
        {
            InitializeComponent();
        }

        public string FilterType
        {
            get { return (string)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterTypeProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterTypeProperty =
            DependencyProperty.Register("FilterType", typeof(string), typeof(BookInputFilterPanel), new PropertyMetadata("BlockNum", OnFilterTypePropertyChanged));

        private static void OnFilterTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            string newTag = e.NewValue.ToString();
            BookInputFilterPanel source = (BookInputFilterPanel)d;
            ComboBoxItem currentSelectedItem = source.bookComboBox.SelectedItem as ComboBoxItem;

            if (currentSelectedItem.Tag.ToString() != newTag)
            {
                foreach (ComboBoxItem item in source.bookComboBox.Items)
                {
                    if (item.Tag.ToString() == newTag)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }

        //public static readonly RoutedEvent FilterTypeChangedEvent = EventManager.RegisterRoutedEvent("FilterTypeChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BookInputFilterPanel));

        //public event RoutedEventHandler FilterTypeChanged
        //{
        //    add { AddHandler(FilterTypeChangedEvent, value); }
        //    remove { RemoveHandler(FilterTypeChangedEvent, value); }
        //}

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchTextProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(BookInputFilterPanel), new PropertyMetadata(string.Empty, OnSearchTextChanged));

        private static void OnSearchTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            string newTextBox = e.NewValue.ToString();
            BookInputFilterPanel source = (BookInputFilterPanel)sender;
            
            if (source.bookTextBox.Text != newTextBox)
            {
                source.bookTextBox.Text = newTextBox;
            }

            //source.SelectedItem = e.NewValue.ToString();
        }

        //public static readonly RoutedEvent SearchTextChangedEvent = EventManager.RegisterRoutedEvent("SearchTextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BookInputFilterPanel));

        //public event RoutedEventHandler SearchTextChanged
        //{
        //    add { AddHandler(SearchTextChangedEvent, value); }
        //    remove { RemoveHandler(SearchTextChangedEvent, value); }
        //}

        private void bookTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //FilterCondition();
                //pageControl.Page = 1;
                //_historyOrderViewModel.ProcessOrdersFilterCondition(this.historyOrderFilter.FilterDic);
                SearchText = this.bookTextBox.Text;

                //RaiseEvent(new RoutedEventArgs { RoutedEvent = SearchTextChangedEvent });
            }
        }

        private void bookComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterType = ((ComboBoxItem)this.bookComboBox.SelectedItem).Tag.ToString();
           // RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterTypeChangedEvent });
        }

        private void bookTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = this.bookTextBox.Text;
            //RaiseEvent(new RoutedEventArgs { RoutedEvent = SearchTextChangedEvent });
        }
    }
}
