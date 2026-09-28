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
    /// CardSettleFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class CardSettleFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(CardSettleFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CardSettleFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public CardSettleFilterView()
        {
            InitializeComponent();
        }

        private void LbSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (!CommonConstants.ALL_PARTY.Equals(lbi.Content) && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[3];// 这个是AllBroker按钮
                if (isTrueCount == 3)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 3)
                {
                    tmp.IsSelected = false;
                }

                FilterDic["Settle"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
