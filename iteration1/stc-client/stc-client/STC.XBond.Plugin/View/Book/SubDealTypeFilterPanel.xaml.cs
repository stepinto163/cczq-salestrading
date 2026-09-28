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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// SubBookTypeFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SubDealTypeFilterPanel : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(SubDealTypeFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SubDealTypeFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public SubDealTypeFilterPanel()
        {
            InitializeComponent();
        }

        private void SubBookTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                FilterDic["dealtype"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
