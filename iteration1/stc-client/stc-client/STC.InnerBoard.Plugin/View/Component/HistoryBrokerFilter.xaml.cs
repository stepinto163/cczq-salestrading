using System;
using STC.Common.Utils;
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
using STC.InnerBoard.Plugin.Utils;

namespace STC.InnerBoard.Plugin.View.Component
{
    /// <summary>
    /// HistoryBrokerFilter.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryBrokerFilter : UserControl
    {
        //过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("RiskFilterDic", typeof(Dictionary<string, IList>),
            typeof(HistoryBrokerFilter), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> RiskFilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }
        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("RiskFilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HistoryBrokerFilter));

        public event RoutedEventHandler RiskFilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }
        public HistoryBrokerFilter()
        {
            InitializeComponent();

        }

     
        private void StyleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (e.AddedItems.Count > 0)
                {
                    foreach (object obj in e.AddedItems)
                    {
                        ListBoxItem item = obj as ListBoxItem;
                        if (item != null)
                        {
                            if (CommonConstants.ALL_EN.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (Constants.ADDED.Equals(lbi.Content)
                                        || Constants.UNADD.Equals(lbi.Content))
                                    {
                                        lbi.IsSelected = false;
                                        //ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                        //if (tmp.IsSelected)
                                        //    tmp.IsSelected = false;
                                    }
                                }
                            }
                            else if (Constants.ADDED.Equals(item.Content)
                                       || Constants.UNADD.Equals(item.Content))
                            {
                                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                if (tmp.IsSelected)
                                    tmp.IsSelected = false;
                            }
                        }
                    }
                }

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (listBox.SelectedItems.Count == 0)
                            listBox.SelectedItem = item;
                    }
                }
                RiskFilterDic["Status"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

      
    }
}
