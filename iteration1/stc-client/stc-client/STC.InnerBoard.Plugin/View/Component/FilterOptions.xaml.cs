using STC.Common.Model.Table;
using STC.Common.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.InnerBoard.Plugin.View.Component
{
    /// <summary>
    /// FilterOptions.xaml 的交互逻辑
    /// </summary>
    public partial class FilterOptions : UserControl
    {
        private volatile bool isCheckShowAll = false;

        public List<string> TagsList = new List<string>();

        public ObservableCollection<OptionItem> ItemsSource
        {
            get { return (ObservableCollection<OptionItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<OptionItem>), typeof(FilterOptions), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterOptions control = d as FilterOptions;
            control.FilterListBox.ItemsSource = e.NewValue as ObservableCollection<OptionItem>;
        }

        public string TagKey
        {
            get { return (string)GetValue(TagKeyProperty); }
            set { SetValue(TagKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TagKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagKeyProperty =
            DependencyProperty.Register("TagKey", typeof(string), typeof(FilterOptions), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent TagsListChangedEvent = EventManager.RegisterRoutedEvent("TagsListChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterOptions));

        public event RoutedEventHandler TagsListChanged
        {
            add { AddHandler(TagsListChangedEvent, value); }
            remove { RemoveHandler(TagsListChangedEvent, value); }
        }



        public bool IsAll
        {
            get { return (bool)GetValue(IsAllProperty); }
            set { SetValue(IsAllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAllProperty =
            DependencyProperty.Register("IsAll", typeof(bool), typeof(FilterOptions), new PropertyMetadata(true, OnIsAllChanged));

        private static void OnIsAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FilterOptions control = d as FilterOptions;
            control.Fitler_All_ToggleButton.IsChecked = (bool)e.NewValue;
        }

        public FilterOptions()
        {
            InitializeComponent();
        }

        internal void InitData(List<string> childTags)
        {
            if (childTags.Count > 0)
            {
                foreach (OptionItem item in this.FilterListBox.Items)
                {
                    if (item.Key != null)
                        item.IsSelected = childTags.Contains(item.Key);
                    else
                        item.IsSelected = false;
                }
            }
            else
            {
                this.Fitler_All_ToggleButton.IsChecked = true;
            }
        }

        private void Fitler_All_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            isCheckShowAll = true;
            IsAll = true;
            if (this.FilterListBox != null)
            {
                foreach (OptionItem item in this.FilterListBox.Items)
                {
                    item.IsSelected = false;
                }

                SetFocusTagDic(this.TagKey, this.FilterListBox.SelectedItems);

                RaiseEvent(new RoutedEventArgs { RoutedEvent = TagsListChangedEvent });
            }
            

            isCheckShowAll = false;
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.FilterListBox != null)
            {
                AdjustListBox(this.FilterListBox, this.ItemsSource.Count, this.Fitler_All_ToggleButton);

                SetFocusTagDic(this.TagKey, this.FilterListBox.SelectedItems);

                RaiseEvent(new RoutedEventArgs { RoutedEvent = TagsListChangedEvent });
            }
        }

        private void SetFocusTagDic(string key, IList listBoxItems)
        {
            if (listBoxItems != null && !string.IsNullOrEmpty(key))
            {
                this.TagsList.Clear();
                foreach (OptionItem item in listBoxItems)
                {
                    this.TagsList.Add(item.Key);
                }
            }
        }


        private void AdjustListBox(ListBox listBox, int allCount, RadioButton rd)
        {
            int isTrueCount = 0;// 如果是true的个数是0或者4时， all按钮被选中 
            foreach (OptionItem lbi in listBox.Items)
            {
                if (lbi.IsSelected == true)
                {
                    isTrueCount++;
                }
            }

            //if (rd != null)
            //{
            //    if (isTrueCount == allCount)
            //    {
            //        rd.IsChecked = true;
            //    }
            //    else
            //    {
            //        rd.IsChecked = false;
            //    }
            //}
            if (isTrueCount == 0)
            {
                rd.IsChecked = true;
            }
            else
            {
                rd.IsChecked = false;
            }
        }

        private void Fitler_All_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAll = false;
        }
    }

    public class OptionItem : ViewModelBase
    {
        private string _title;
        private string _key;
        private bool _isSelected;
        #region ctor
        public OptionItem(string title, string key)
        {
            Title = title;
            Key = key;
        }
        #endregion

        #region Properties
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion
    }
}
