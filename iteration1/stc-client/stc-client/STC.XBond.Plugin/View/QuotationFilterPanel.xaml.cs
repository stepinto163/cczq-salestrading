using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// QuotationFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class QuotationFilterPanel : UserControl
    {
        

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(QuotationFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(QuotationFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        #region FilterVisisble
        public Visibility FilterVisisble
        {
            get { return (Visibility)GetValue(FilterVisisbleProperty); }
            set
            {
                SetValue(FilterVisisbleProperty, value);
            }
        }

        public static readonly DependencyProperty FilterVisisbleProperty = DependencyProperty.Register("FilterVisisble", typeof(Visibility),
       typeof(QuotationFilterPanel), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnFilterVisisbletPropertyChange)));

        static void OnFilterVisisbletPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            QuotationFilterPanel source = (QuotationFilterPanel)sender;

            var newValue = args.NewValue;
            source.FilterVisisble = (Visibility)newValue;
            source.SourceLbl.Visibility = (Visibility)newValue;
            source.VenuesListBox.Visibility = (Visibility)newValue;
            source.SettleLbl.Visibility = (Visibility)newValue;
            source.SettlTypeListBox.Visibility = (Visibility)newValue;
        }
        #endregion

        public QuotationFilterPanel()
        {
            InitializeComponent();
        }


        private bool isNonmanualCheckShowAll = false;
        private bool isCheckShowAll = false;
        private Dictionary<string, string> venuesDic = new Dictionary<string, string>();
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }

            isCheckShowAll = true;
            if (this.FirmTypeListBox != null)
            {
                foreach (ListBoxItem item in this.FirmTypeListBox.Items)
                {
                    item.IsSelected = false;
                }
            }
            
            //if (this.VenuesListBox != null)
            //{
            //    foreach (ListBoxItem item in this.VenuesListBox.Items)
            //    {
            //        item.IsSelected = false;
            //    }
            //}

            if (this.SettlTypeListBox != null)
            {
                foreach (ListBoxItem item in this.SettlTypeListBox.Items)
                {
                    item.IsSelected = false;
                }
            }
            if (this.FirmTypeListBox != null)
            {
                FilterDic["FirmFlag"] = this.FirmTypeListBox.SelectedItems;
            }
            if (this.VenuesListBox != null)
            {
                venuesDic.Clear();

                //模拟选中
                string content = string.Empty;
                if (this.VenuesListBox.SelectedItem != null)
                {
                    content = this.VenuesListBox.SelectedItem.ToString();
                }

                List<ListBoxItem> selectedItems = new List<ListBoxItem>();
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = content;
                selectedItems.Add(listBoxItem);


                FilterDic["Venues"] = selectedItems;
                foreach (ListBoxItem box in selectedItems)
                {
                    venuesDic.Add(box.Content.ToString(), box.Content.ToString());
                }
            }
            if (this.SettlTypeListBox != null)
            {
                FilterDic["SettlType"] = this.SettlTypeListBox.SelectedItems;
            }
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            isCheckShowAll = false;
        }


        private void ShowAll_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }
        }

        private void FirmTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.FirmTypeListBox != null)
            {
                FilterDic["FirmFlag"] = this.FirmTypeListBox.SelectedItems;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            venuesDic.Clear();

            //模拟选中
            string content = string.Empty;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                content = (e.AddedItems[0] as ComboBoxItem).Content.ToString();

                List<ListBoxItem> selectedItems = new List<ListBoxItem>();
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = content;
                selectedItems.Add(listBoxItem);


                FilterDic["Venues"] = selectedItems;
                foreach (ListBoxItem box in selectedItems)
                {
                    venuesDic.Add(box.Content.ToString(), box.Content.ToString());
                }

                this.AdjustShowAll_ToggleButtonCheck();

                RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            }
        }

        private void VenuesListBox_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            //if (isCheckShowAll == true)
            //    return;

            //if (sender != null && sender is ListBox)
            //{
            //    ListBox listBox = sender as ListBox;

            //    int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 
            //    int isInnerCount = 0;// 如果是true的个数是0或者3时， allinner按钮被选中 
            //    foreach (ListBoxItem lbi in listBox.Items)
            //    {
            //        if ((CommonConstants.CNEX.Equals(lbi.Content)
            //        || CommonConstants.TPSC.Equals(lbi.Content)
            //        || CommonConstants.CTTJ.Equals(lbi.Content)
            //        || CommonConstants.PTCN.Equals(lbi.Content)
            //        || CommonConstants.CCTB.Equals(lbi.Content)
            //        || CommonConstants.UEDA.Equals(lbi.Content))
            //        && lbi.IsSelected == true)
            //        {
            //            isTrueCount++;
            //        }

            //        if ((Constants.MANUAL.Equals(lbi.Content)
            //        || Constants.IDB.Equals(lbi.Content)
            //        || Constants.QUANT.Equals(lbi.Content))
            //        && lbi.IsSelected == true)
            //        {
            //            isInnerCount++;
            //        }
            //    }

            //    //ListBoxItem tmpInner = (ListBoxItem)listBox.Items[0];// 这个是allinner按钮
            //    //if (isInnerCount == 3)
            //    //{
            //    //    tmpInner.IsSelected = true;
            //    //}
            //    //else if (isInnerCount > 0 && isInnerCount < 3)
            //    //{
            //    //    tmpInner.IsSelected = false;
            //    //}

            //    ListBoxItem tmp = (ListBoxItem)listBox.Items[6];// 这个是AllBroker按钮
            //    if (isTrueCount == 6)
            //    {
            //        tmp.IsSelected = true;
            //    }
            //    else if (isTrueCount > 0 && isTrueCount < 5)
            //    {
            //        tmp.IsSelected = false;
            //    }
            //}
 
            //if (this.VenuesListBox != null)
            //{
            //    venuesDic.Clear();
            //    FilterDic["Venues"] = this.VenuesListBox.SelectedItems;
            //    foreach (ListBoxItem box in this.VenuesListBox.SelectedItems)
            //    {
            //        venuesDic.Add(box.Content.ToString(),box.Content.ToString());
            //    }
            //}

            //this.AdjustShowAll_ToggleButtonCheck();

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void SettlTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.SettlTypeListBox != null)
            {
                FilterDic["SettlType"] = this.SettlTypeListBox.SelectedItems;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;
            foreach(KeyValuePair<string, IList> pair in this.FilterDic)
            {
                if (pair.Key == "Venues")
                {
                   
                    string v = (pair.Value[0] as ListBoxItem).Content.ToString();
                    if (v != "全部")
                    {
                        isChecked = false;
                    }
                    //// 此为不包含第一个全部的选中个数
                    //int isTrueCountVenues = pair.Value[0].ToString();

                    ////如果是true的个数是0或者10或(idb + quant + manual + xbond + 全部)选中， all按钮被选中(总渠道)
                    //if (!(isTrueCountVenues == 0 || isTrueCountVenues == 11 ||(
                    //    (isTrueCountVenues == 7|| isTrueCountVenues == 3|| isTrueCountVenues == 8)&& 
                    //    (venuesDic.Keys.Contains(Constants.INNER) && venuesDic.Keys.Contains(Constants.XBond) && venuesDic.Keys.Contains(CommonConstants.AllBroker)))))
                    //{
                    //    isChecked = false;
                    //    break;
                    //}
                }
                else if (pair.Key == "FirmFlag")
                {
                    int firmFlagCount = 0;
                    foreach(ListBoxItem item in pair.Value)
                    {
                        if (item.IsSelected == true)
                        {
                            firmFlagCount++;
                        }
                    }
                    if (!(firmFlagCount == 0 || firmFlagCount == 2))
                    {
                        isChecked = false;
                        break;
                    }
                }
                else if (pair.Key == "SettlType")
                {
                    int settlTypeCount = 0;
                    foreach(ListBoxItem item in pair.Value)
                    {
                        if (item.IsSelected == true)
                        {
                            settlTypeCount++;
                        }
                    }
                    if (!(settlTypeCount == 0 || settlTypeCount == 3))
                    {
                        isChecked = false;
                        break;
                    }
                }
                //if (pair.Value.Count > 0)
                //{
                //    isChecked = false;
                //    break;
                //}
            }
            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked;
            isNonmanualCheckShowAll = false;
        }

        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.FirmTypeListBox.SelectedIndex = -1;
            this.VenuesListBox.SelectedIndex = 0;
            this.SettlTypeListBox.SelectedIndex = -1;
        }

        private void VenuesListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //ListBoxItem listBoxItem = e.Source as ListBoxItem;
            //if (listBoxItem != null)
            //{
            //    string venue = listBoxItem.Content.ToString();
            //    IList selectItems = VenuesListBox.SelectedItems;
            //    if (venue == CommonConstants.XBond)
            //    {
            //        foreach (ListBoxItem lb in selectItems)
            //        {
            //            string content = lb.Content.ToString();
            //            if (content == CommonConstants.SHE || content == CommonConstants.SHF || content == CommonConstants.SZE)
            //            {
            //                e.Handled = true;
            //                return;
            //            }
            //        }
            //    }
            //    else if (venue == CommonConstants.SHE || venue == CommonConstants.SHF || venue == CommonConstants.SZE)
            //    {
            //        foreach (ListBoxItem lb in selectItems)
            //        {
            //            string content = lb.Content.ToString();
            //            if (content == CommonConstants.XBond)
            //            {
            //                e.Handled = true;
            //                return;
            //            }
            //        }
            //    }
            //}
        }
    }
}
