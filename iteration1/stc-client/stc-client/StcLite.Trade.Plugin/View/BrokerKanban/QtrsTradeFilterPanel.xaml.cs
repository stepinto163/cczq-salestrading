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

namespace BDB.Trade.Plugin.View.BrokerKanban
{
    /// <summary>
    /// 已确认筛选面板
    /// </summary>
    public partial class QtrsTradeFilterPanel : UserControl
    {
        //过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(QtrsTradeFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }
        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(QtrsTradeFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public QtrsTradeFilterPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 渠道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                if (listBox.Items.Count < 5)
                {
                    FilterDic["Venues"] = listBox.SelectedItems;
                    return;
                }

                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((CommonConstants.CNEX.Equals(lbi.Content)
                    || CommonConstants.TPSC.Equals(lbi.Content)
                    || CommonConstants.CTTJ.Equals(lbi.Content)
                    || CommonConstants.PTCN.Equals(lbi.Content)
                    || CommonConstants.CCTB.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];// 这个是AllBroker按钮
                if (isTrueCount == 5)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 5)
                {
                    tmp.IsSelected = false;
                }

                // 此为不包含第一个全部的选中个数
                int isTrueCountVenues = 0;
                // 此为选项索引
                int index = 0;
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (index != 0 && lbi.IsSelected == true)
                    {
                        isTrueCountVenues++;
                    }
                    index++;
                }
                ListBoxItem tmpAll = (ListBoxItem)listBox.Items[0];
                // 如果是true的个数是0或者8或tradeX+bond+全部选中， all按钮被选中 (总渠道)
                if (isTrueCountVenues == 0 || isTrueCountVenues == 5 )
                {
                    tmpAll.IsSelected = true;
                }
                else
                {
                    tmpAll.IsSelected = false;
                }

                FilterDic["Venues"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        /// <summary>
        /// 债券类别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void typeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                if (isTrueCount == 0 || isTrueCount == 3)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }
                FilterDic["SecurityType"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
