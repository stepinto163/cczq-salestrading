using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// HistoryRiskFilter.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryRiskFilter : UserControl
    { //过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("RiskFilterDic", typeof(Dictionary<string, IList>),
            typeof(HistoryRiskFilter), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> RiskFilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }
        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("RiskFilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HistoryRiskFilter));

        public event RoutedEventHandler RiskFilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }
        public HistoryRiskFilter()
        {
            InitializeComponent();
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                dpFrom.Text = DateTime.Now.AddDays(-1).ToShortDateString();
                dpTo.Text = DateTime.Now.ToShortDateString();
                checkBox.IsChecked = true;
                IList list = new ArrayList();
                list.Add(checkBox);
                RiskFilterDic["Status"] = list;
            }));
        }

        private void dpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpTo.SelectedDate = dpFrom.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            RiskFilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void dpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpFrom.SelectedDate = dpTo.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            RiskFilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void RiskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                            if (CommonConstants.ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (Constants.ONEWARN.Equals(lbi.Content)
                                        || Constants.TWOWARN.Equals(lbi.Content)
                                        || Constants.FORBIDWARN.Equals(lbi.Content))
                                    {
                                        lbi.IsSelected = false;
                                        //ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                        //if (tmp.IsSelected)
                                        //    tmp.IsSelected = false;
                                    }
                                }
                            }
                            else if (Constants.ONEWARN.Equals(item.Content)
                                       || Constants.TWOWARN.Equals(item.Content)
                                       || Constants.FORBIDWARN.Equals(item.Content))
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
                RiskFilterDic["Types"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
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
                            if (CommonConstants.ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (Constants.MANUAL.Equals(lbi.Content)
                                        || Constants.IDB.Equals(lbi.Content)
                                        || Constants.QUANT.Equals(lbi.Content)
                                        || Constants.INNER.Equals(lbi.Content))
                                    {
                                        lbi.IsSelected = false;
                                        //ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                        //if (tmp.IsSelected)
                                        //    tmp.IsSelected = false;
                                    }
                                }
                            }
                            else if (Constants.MANUAL.Equals(item.Content)
                                        || Constants.IDB.Equals(item.Content)
                                        || Constants.QUANT.Equals(item.Content)
                                        || Constants.INNER.Equals(item.Content))
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
                RiskFilterDic["Venues"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void QuoteTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                            if (CommonConstants.ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (Constants.FIRM.Equals(lbi.Content)
                                        || Constants.NONFIRM.Equals(lbi.Content))
                                    {
                                        lbi.IsSelected = false;
                                        //ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                        //if (tmp.IsSelected)
                                        //    tmp.IsSelected = false;
                                    }
                                }
                            }
                            else if (Constants.FIRM.Equals(item.Content)
                                       || Constants.NONFIRM.Equals(item.Content))
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
                RiskFilterDic["Firmflag"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox clickedBox = sender as CheckBox;

                IList list = new ArrayList();
                list.Add(clickedBox);
                RiskFilterDic["Status"] = list;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
