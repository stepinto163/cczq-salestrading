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

namespace BDB.Trade.Plugin.View
{
    /// <summary>
    /// HistoryEMSFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryEMSFilterPanel : UserControl
    {
        //过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(HistoryEMSFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public bool IsClick
        {
            get
            {
                return isClick;
            }

            set
            {
                isClick = value;
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HistoryEMSFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }
        public HistoryEMSFilterPanel()
        {
            InitializeComponent();
            dpFrom.Text = DateTime.Now.AddDays(-1).ToShortDateString();
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
            if(dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            FilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
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
            FilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
        }

        private int isTrueCount = 0;
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox comboBox = sender as ListBox;

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (comboBox.Items.Count == 0)
                            comboBox.SelectedItem = item;
                    }
                }
                IList list = new ArrayList();
                list.Add(comboBox.SelectedItem);
                FilterDic["OrderStatus"] = list;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
        }

        private void lbPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                //int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (lbi.IsSelected)
                    {
                        isTrueCount++;
                    }
                }

                FilterDic.Clear();
                FilterDic["IsPoint"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
        }


        private bool isClick = true;
        public void setPro()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => 
            {
                if (orderStatusComBox.SelectedIndex != 0)
                {
                    IsClick = false;
                    orderStatusComBox.SelectedIndex = 0;
                }
                
                if (FilterDic.ContainsKey("TransactDate"))
                {
                    IList dateList = FilterDic["TransactDate"];
                    if (dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd") != dateList[0].ToString())
                    {
                        IsClick = false;
                        dpFrom.Text = dateList[0].ToString();
                    }
                    if (dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd") != dateList[1].ToString())
                    {
                        IsClick = false;
                        dpTo.Text = dateList[1].ToString();
                    }
                }
            }));
        }
    }
}
