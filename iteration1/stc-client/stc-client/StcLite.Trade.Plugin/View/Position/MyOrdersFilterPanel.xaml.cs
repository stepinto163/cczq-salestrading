using BDB.Trade.Plugin.Utils;
using System;
using System.Collections;
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

namespace BDB.Trade.Plugin.View.Position
{
    /// <summary>
    /// MyOrdersFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MyOrdersFilterPanel : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MyOrdersFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        // 一键撤销按钮的Visibility属性
        public Visibility Visibility4CancelAllBtn
        {
            get { return (Visibility)GetValue(Visibility4CancelAllBtnPropertyProperty); }
            set { SetValue(Visibility4CancelAllBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4CancelAllBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4CancelAllBtnPropertyProperty =
            DependencyProperty.Register("Visibility4CancelAllBtn", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Collapsed, onVisibility4CancelAllBtn));

        // 恢复报价按钮的Visibility属性
        public Visibility Visibility4ResendBtn
        {
            get { return (Visibility)GetValue(Visibility4ResendBtnPropertyProperty); }
            set { SetValue(Visibility4ResendBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4ResendBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4ResendBtnPropertyProperty =
            DependencyProperty.Register("Visibility4ResendBtn", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Collapsed, onVisibility4ResendBtn));

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyOrdersFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public MyOrdersFilterPanel()
        {
            InitializeComponent();
        }

        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isTrueCount = 0;// 如果是true的个数是0或者3时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (listBox.Items.IndexOf(lbi) != 0
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 3)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }

                FilterDic["Venues"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void FirmFlagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者2时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (listBox.Items.IndexOf(lbi) != 0
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 2)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }

                FilterDic["FirmFlag"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private static void onVisibility4CancelAllBtn(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.CancelAll_CBC.Visibility = (Visibility)args.NewValue;
        }

        private static void onVisibility4ResendBtn(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.ResendBtn.Visibility = (Visibility)args.NewValue;
        }

        private void LbCodeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者2时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (listBox.Items.IndexOf(lbi) != 0
                        && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 2)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }
                FilterDic["CodeType"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
