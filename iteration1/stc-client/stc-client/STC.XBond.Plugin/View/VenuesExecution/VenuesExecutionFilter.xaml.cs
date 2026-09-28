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
    /// VenuesExecutionFilter.xaml 的交互逻辑
    /// </summary>
    public partial class VenuesExecutionFilter : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(VenuesExecutionFilter), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VenuesExecutionFilter));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public event Action OnCopyClick;

        public VenuesExecutionFilter()
        {
            InitializeComponent();
        }



        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 
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
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 6)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }

                FilterDic["Venues"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void StatusListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者4时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((Constants.UNSOLVE.Equals(lbi.Content)
                    || Constants.SOLVEING.Equals(lbi.Content)
                    || Constants.EXECUTED.Equals(lbi.Content)
                    || Constants.CANCELED.Equals(lbi.Content)


                    || Constants.CANCELING.Equals(lbi.Content)
                    || Constants.WARICANCEL.Equals(lbi.Content)
                    || Constants.PARTEXECUTED.Equals(lbi.Content)
                    || Constants.EXCEPTION.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 5)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }
                
                FilterDic["Status"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void IsOCOListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者4时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((Constants.NOTOCO.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                //if (isTrueCount == 0 || isTrueCount == 1)
                //{
                //    tmp.IsSelected = true;
                //}
                //else
                //{
                //    tmp.IsSelected = false;
                //}

                FilterDic["IsOCO"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnCopyClick?.Invoke();
        }

        private void LbMyOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isTrueCount = 0;// 如果是true的个数是0或者4时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((Constants.MYOPERATION.Equals(lbi.Content)
                        || Constants.NOOPERATION.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0)// || isTrueCount == 2)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }



                FilterDic["MyOperation"] = listBox.SelectedItems;
                
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
