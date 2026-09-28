using STC.Execute.Plugin.Utils;
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

namespace STC.Execute.Plugin.View.Execute
{
    /// <summary>
    /// BrokerFilter.xaml 的交互逻辑
    /// </summary>
    public partial class BrokerFilter : UserControl
    {

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(BrokerFilter), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrokerFilter));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public BrokerFilter()
        {
            InitializeComponent();
        }


        private int isTrueCount = 0;
        private void LbBroker_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                            if (Constants.ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (lbi.IsSelected &&
                                        (Constants.CNEX.Equals(lbi.Content)
                                            || Constants.TPSC.Equals(lbi.Content)
                                            || Constants.CTTJ.Equals(lbi.Content)
                                            || Constants.PTCN.Equals(lbi.Content)
                                            || Constants.CCTB.Equals(lbi.Content)
                                            || Constants.UEDA.Equals(lbi.Content)))
                                        lbi.IsSelected = false;
                                }
                            }
                            else if (Constants.CNEX.Equals(item.Content)
                                        || Constants.TPSC.Equals(item.Content)
                                        || Constants.CTTJ.Equals(item.Content)
                                        || Constants.PTCN.Equals(item.Content)
                                        || Constants.CCTB.Equals(item.Content)
                                        || Constants.UEDA.Equals(item.Content))
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
                FilterDic["Broker"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
