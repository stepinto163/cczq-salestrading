
using BDB.Trade.Plugin.Utils;
using STC.Common.Utils;
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

namespace BDB.Trade.Plugin.View
{
    /// <summary>
    /// HistoryMsgFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryMsgFilterPanel : UserControl
    {
        //过滤属性
        public static readonly DependencyProperty MsgFilterDicProperty = DependencyProperty.Register("MsgFilterDic", typeof(Dictionary<string, IList>),
            typeof(HistoryMsgFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> MsgFilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(MsgFilterDicProperty); }
            set { SetValue(MsgFilterDicProperty, value); }
        }
        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent MsgFilterDicChangedEvent = EventManager.RegisterRoutedEvent("MsgFilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HistoryMsgFilterPanel));

        public event RoutedEventHandler MsgFilterDicChanged
        {
            add { AddHandler(MsgFilterDicChangedEvent, value); }
            remove { RemoveHandler(MsgFilterDicChangedEvent, value); }
        }
        public HistoryMsgFilterPanel()
        {
            InitializeComponent();
            dpFrom.Text = DateTime.Now.ToShortDateString();
            dpTo.Text = DateTime.Now.ToShortDateString();
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
            MsgFilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = MsgFilterDicChangedEvent });
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
            MsgFilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = MsgFilterDicChangedEvent });
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                                    if (lbi.IsSelected &&
                                        (CommonConstants.URGENT.Equals(lbi.Content)
                                        || CommonConstants.MAJOR.Equals(lbi.Content)
                                        || CommonConstants.GENERAL.Equals(lbi.Content)))
                                        lbi.IsSelected = false;
                                }
                            }
                            else if (CommonConstants.URGENT.Equals(item.Content)
                                        || CommonConstants.MAJOR.Equals(item.Content)
                                        || CommonConstants.GENERAL.Equals(item.Content))
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
                MsgFilterDic["ContentType"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = MsgFilterDicChangedEvent });
        }
    }
}
