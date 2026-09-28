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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// MyExecutionsFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MyExecutionsFilterPanel : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MyExecutionsFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyExecutionsFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public MyExecutionsFilterPanel()
        {
            InitializeComponent();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["Tradebook"] = "Tradebook";
            dic["X-Bond"] = "X-Bond";
            dic["双边"] = "双边";
            dic["国际"] = "国际";
            dic["国利"] = "国利";
            dic["平安"] = "平安";
            dic["信唐"] = "信唐";
            dic["中诚"] = "中诚";
            dic["上田"] = "上田";
            this.VenuesMscb.ItemsSource = dic;

            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            dic1["已发送"] = "已发送";
            dic1["待处理"] = "待处理";
            this.SyncStatusMscb.ItemsSource = dic1;
        }
        
        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                
                if (listBox.Items.Count < 9)
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
                    || CommonConstants.CCTB.Equals(lbi.Content)
                    || CommonConstants.UEDA.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[4];// 这个是AllBroker按钮
                if (isTrueCount == 6)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount >0 && isTrueCount < 6)
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
                if (isTrueCountVenues == 0 || isTrueCountVenues == 8 || 
                    (((ListBoxItem)listBox.Items[1]).IsSelected == true
                    && ((ListBoxItem)listBox.Items[2]).IsSelected == true
                    && ((ListBoxItem)listBox.Items[3]).IsSelected == true))
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

        private void SyncStatusBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                FilterDic["SyncStatus"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
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

        private void ContractTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                FilterDic["ContractType"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void VenuesMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.VenuesMscb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            FilterDic["Venues"] = selectedList;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void SyncStatusMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.SyncStatusMscb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            FilterDic["SyncStatus"] = selectedList;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
