using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace STC.Common.View.Component
{
    /// <summary>
    /// DescriptionCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class DescriptionCtrl : UserControl
    {
        public ObservableCollection<DescriptionItem> ItemsSource
        {
            get { return (ObservableCollection<DescriptionItem>)GetValue(ItemsSourceProperty); }
            set { 
                
                SetValue(ItemsSourceProperty, value);

                if (value != null)
                {
                    value.CollectionChanged -= ItemsSource_CollectionChanged;
                    value.CollectionChanged += ItemsSource_CollectionChanged;
                }
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<DescriptionItem>), typeof(DescriptionCtrl), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            DescriptionCtrl control = d as DescriptionCtrl;
            ObservableCollection<DescriptionItem> list = e.NewValue as ObservableCollection<DescriptionItem>;

            if (list.Count == 0)
                control.mainGrid.Visibility = Visibility.Collapsed;
            else
                control.mainGrid.Visibility = Visibility.Visible;

            list.CollectionChanged -= control.ItemsSource_CollectionChanged;
            list.CollectionChanged += control.ItemsSource_CollectionChanged;

            if (list.Count <= 5)
            {
                control.descriptionList.ItemsSource = list.ToList();
            }
            else
            {
                ObservableCollection<DescriptionItem> newList = new ObservableCollection<DescriptionItem>();
                for (int i = 0; i < 5; i++)
                {
                    newList.Add(list[i]);
                }
                newList.Add(new DescriptionItem("...", "..."));

                control.descriptionList.ItemsSource = newList;
            }

            control.dgDescriptionList.ItemsSource = list;
        }

        BrushConverter converter = new System.Windows.Media.BrushConverter();
        Brush default_ColorBrush;
        Brush mouseOver_ColorBrush;

        public DescriptionCtrl()
        {
            InitializeComponent();

            default_ColorBrush = (Brush)converter.ConvertFromString("#1E1E1E");
            mouseOver_ColorBrush = (Brush)converter.ConvertFromString("#3C3C3C");

            //ObservableCollection<DescriptionItem> list = new ObservableCollection<DescriptionItem>();
            //DescriptionItem item = new DescriptionItem("银行","1");
            //list.Add(item);
            //DescriptionItem item1 = new DescriptionItem("口行", "2");
            //list.Add(item1);
            //DescriptionItem item2 = new DescriptionItem("公司债", "3");
            //list.Add(item2);
            //DescriptionItem item3 = new DescriptionItem("6-9M", "4");
            //list.Add(item3);
            //DescriptionItem item4 = new DescriptionItem("上海/北京/深圳/广州/内蒙古", "5");
            //list.Add(item4);
            //DescriptionItem item5 = new DescriptionItem("xxxx", "6");
            //list.Add(item5);


            //this.ItemsSource = list;
        }

        private void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource.Count == 0)
                this.mainGrid.Visibility = Visibility.Collapsed;
            else
                this.mainGrid.Visibility = Visibility.Visible;

            if (ItemsSource.Count <= 5)
            {
                descriptionList.ItemsSource = ItemsSource.ToList();
            }
            else
            {
                ObservableCollection<DescriptionItem> newList = new ObservableCollection<DescriptionItem>();
                for (int i = 0; i < 5; i++)
                {
                    newList.Add(ItemsSource[i]);
                }
                newList.Add(new DescriptionItem("...", "..."));

                descriptionList.ItemsSource = newList;
            }
        }

        private void out_border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!popDescriptionList.IsOpen)
            {
                popDescriptionList.IsOpen = true;
            }
        }

        private void descriptionList_MouseEnter(object sender, MouseEventArgs e)
        {
            foreach (DescriptionItem item in this.descriptionList.Items)
            {
                ContentPresenter xx = this.descriptionList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                Border b = xx.ContentTemplate.FindName("txtBorder", xx) as Border;
                b.Background = mouseOver_ColorBrush;
            }
        }


        private void descriptionList_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach(DescriptionItem item in this.descriptionList.Items)
            {
                ContentPresenter xx  = this.descriptionList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                Border b = xx.ContentTemplate.FindName("txtBorder", xx) as Border;
                b.Background = default_ColorBrush;
            }
        }

        private void popDescriptionList_Opened(object sender, EventArgs e)
        {
            double popupWidth = this.out_border.ActualWidth;
            foreach (DescriptionItem info in this.dgDescriptionList.Items)
            {
                if (info != null)
                {
                    //计算最大长度的文本
                    string maxLengthString = info.Title;

                    var formattedText = new FormattedText(maxLengthString, CultureInfo.CurrentUICulture,
                            FlowDirection.LeftToRight, new Typeface(new FontFamily("微软雅黑"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            12, Brushes.Black);

                    //获取换行文本的文本框宽度，即template里面的textbox或textblock的实际宽度
                    double textBoxWidth = formattedText.Width;
                    if (formattedText.Width > 300)
                    {
                        textBoxWidth = 300;
                    }
                    else
                    {
                        if (textBoxWidth < this.out_border.ActualWidth)
                        {
                            textBoxWidth = this.out_border.ActualWidth;
                        }
                    }

                    if (popupWidth < textBoxWidth)
                    {
                        popupWidth = textBoxWidth;
                    }
                }
                this.dgDescriptionList.Width = popupWidth;
            }
        }
    }

    public class DescriptionItem : ViewModelBase
    {
        private string _title;
        private string _key;
        private bool _isSelected;
        #region ctor
        public DescriptionItem(string title, string key)
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
