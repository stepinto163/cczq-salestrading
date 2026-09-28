using com.gateway.log;
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

namespace BDB.Trade.Plugin.Component
{
    /// <summary>
    /// BrokerFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerSettleFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(BrokerSettleFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrokerSettleFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public BrokerSettleFilterView()
        {
            InitializeComponent();

            this.Loaded += BrokerSettleFilterView_Loaded;
        }

        private int isTrueCount = 0;

        private void BrokerSettleFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BrokerSettleFilterView_Loaded;

            try
            {
                string[] brokerSortArr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokerSortArr != null)
                {
                    int index = 1;
                    foreach (string bs in brokerSortArr)
                    {
                        if (CommonConstants.BrokerNumEnum.Contains(bs))
                        {
                            string cn = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");
                            ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                            lb.Visibility = Visibility.Visible;

                            lb.Content = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");

                            index++;
                        }
                    }

                }

                //string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //if (arr != null)
                //{
                //    isTrueCount = arr.Length;

                //    int index = 1;
                //    foreach (string str in arr)
                //    {
                //        ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                //        lb.Visibility = Visibility.Visible;
                //        lb.Content = ComboSourceUtil.Instance.FindEnum(str, "venues_cn");

                //        index++;
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void LbBroker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectCount = 0;
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                
                // 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((CommonConstants.CNEX.Equals(lbi.Content)
                    || CommonConstants.TPSC.Equals(lbi.Content)
                    || CommonConstants.CTTJ.Equals(lbi.Content)
                    || CommonConstants.PTCN.Equals(lbi.Content)
                    || CommonConstants.CCTB.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        selectCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];// 这个是AllBroker按钮
                if (selectCount == isTrueCount)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 5)
                {
                    tmp.IsSelected = false;
                }

                FilterDic["Broker"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void LbSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectCount = 0;
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (!CommonConstants.ALL.Equals(lbi.Content) && lbi.IsSelected == true)
                    {
                        selectCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];// 这个是AllBroker按钮
                if (selectCount == 4)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 4)
                {
                    tmp.IsSelected = false;
                }

                FilterDic["Settle"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
