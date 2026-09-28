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

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// MMFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MMFilterPanel : UserControl
    {

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MMFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MMFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public MMFilterPanel()
        {
            InitializeComponent();
        }

        private bool isNonmanualCheckShowAll = false;
        private bool isCheckShowAll = false;

        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }

            isCheckShowAll = true;

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

        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.BondPeriodListBox.SelectedIndex = -1;
            this.BondTypeListBox.SelectedIndex = -1;
            this.NewBondPeriodListBox.SelectedIndex = -1;
            this.IsNewBondChk.IsChecked = false;

            FilterDic.Clear();
        }

        private void BondTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.BondTypeListBox.SelectedItems != null)
            {
                bool isCheckedAll = false;
                bool isCheckedGZ = false;
                bool isCheckedJRZ = false;
                bool isCheckedDFZ = false;

                foreach (ListBoxItem item in this.BondTypeListBox.SelectedItems)
                {
                    if (item.Content.ToString() == "全部")
                    {
                        isCheckedAll = true;
                        break;
                    }

                    if (item.Content.ToString() == "国债")
                    {
                        isCheckedGZ = true;
                    }

                    if (item.Content.ToString() == "金融债")
                    {
                        isCheckedJRZ = true;
                    }

                    if (item.Content.ToString() == "地方债")
                    {
                        isCheckedDFZ = true;
                    }
                }

                if (!isCheckedAll)
                {
                    FilterDic["BondTypeFlag"] = this.BondTypeListBox.SelectedItems;

                    if (isCheckedGZ && isCheckedJRZ && isCheckedDFZ)
                    {
                        ListBox listbox = sender as ListBox;
                        ListBoxItem allItem = (ListBoxItem)listbox.Items[0];
                        allItem.IsSelected = true;
                    }
                }
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void BondPeriodListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.BondPeriodListBox.SelectedItems != null)
            {
                bool isCheckedAll = false;
                bool isChecked0Y = false;
                bool isChecked1Y = false;
                bool isChecked3Y = false;
                bool isChecked5Y = false;
                bool isChecked7Y = false;
                bool isChecked10Y = false;

                foreach (ListBoxItem item in this.BondPeriodListBox.SelectedItems)
                {
                    if (item.Content.ToString() == "全部")
                    {
                        isCheckedAll = true;
                        break;
                    }

                    switch (item.Content.ToString())
                    {
                        case "0-1Y":
                            isChecked0Y = true;
                            break;
                        case "1-3Y":
                            isChecked1Y = true;
                            break;
                        case "3-5Y":
                            isChecked3Y = true;
                            break;
                        case "5-7Y":
                            isChecked5Y = true;
                            break;
                        case "7-10Y":
                            isChecked7Y = true;
                            break;
                        case ">=10Y":
                            isChecked10Y = true;
                            break;
                    }
                }

                if (!isCheckedAll)
                {
                    FilterDic["BondPeriodFlag"] = this.BondPeriodListBox.SelectedItems;

                    if (isChecked0Y && isChecked1Y && isChecked3Y && isChecked5Y && isChecked7Y && isChecked10Y)
                    {
                        ListBox listbox = sender as ListBox;
                        ListBoxItem allItem = (ListBoxItem)listbox.Items[0];
                        allItem.IsSelected = true;
                    }
                }
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void NewBondPeriodListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.NewBondPeriodListBox.SelectedItems != null && (bool)this.IsNewBondChk.IsChecked)
            {
                bool isCheckedAll = false;
                bool isChecked1Y = false;
                bool isChecked3Y = false;
                bool isChecked5Y = false;
                bool isChecked7Y = false;
                bool isChecked10Y = false;

                foreach (ListBoxItem item in this.NewBondPeriodListBox.SelectedItems)
                {
                    if (item.Content.ToString() == "全部")
                    {
                        isCheckedAll = true;
                        break;
                    }

                    switch (item.Content.ToString())
                    {
                        case "1-3Y":
                            isChecked1Y = true;
                            break;
                        case "3-5Y":
                            isChecked3Y = true;
                            break;
                        case "5-7Y":
                            isChecked5Y = true;
                            break;
                        case "7-10Y":
                            isChecked7Y = true;
                            break;
                        case ">=10Y":
                            isChecked10Y = true;
                            break;
                    }
                }

                //if (!isCheckedAll)
                {
                    FilterDic["NewBondPeriodFlag"] = this.NewBondPeriodListBox.SelectedItems;

                    if (isChecked1Y && isChecked3Y && isChecked5Y && isChecked7Y && isChecked10Y)
                    {
                        ListBox listbox = sender as ListBox;
                        ListBoxItem allItem = (ListBoxItem)listbox.Items[0];
                        allItem.IsSelected = true;
                    }
                }
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void IsNewBondChk_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)this.IsNewBondChk.IsChecked)
            {
                if (FilterDic.ContainsKey("NewBondPeriodFlag"))
                {
                    FilterDic.Remove("NewBondPeriodFlag");
                }

            }
            else
            {
                //if (FilterDic.ContainsKey("BondPeriodFlag"))
                //{
                //    FilterDic.Remove("BondPeriodFlag");
                //}

                //if (FilterDic.ContainsKey("BondTypeFlag"))
                //{
                //    FilterDic.Remove("BondTypeFlag");
                //}

                if (!FilterDic.ContainsKey("NewBondPeriodFlag") || FilterDic["NewBondPeriodFlag"] == null)
                {
                    ListBoxItem allItem = (ListBoxItem)this.NewBondPeriodListBox.Items[0];
                    allItem.IsSelected = true;
                    this.NewBondPeriodListBox_SelectionChanged(this.NewBondPeriodListBox,null);
                }

            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void IsNewBondChk_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void IsNewBondChk_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void AdjustShowAll_ToggleButtonCheck()
        {

        }
    }
}
