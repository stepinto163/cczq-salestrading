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
    /// MyOrdersFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MyOrdersFilterPanel : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MyOrdersFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        // 一键撤销按钮的Visibility属性
        public Visibility Visibility4CancelAllBtn
        {
            get { return (Visibility)GetValue(Visibility4CancelAllBtnPropertyProperty); }
            set { SetValue(Visibility4CancelAllBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4CancelAllBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4CancelAllBtnPropertyProperty =
            DependencyProperty.Register("Visibility4CancelAllBtn", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Collapsed, onVisibility4CancelAllBtn));

        // 恢复报价按钮的Visibility属性
        public Visibility Visibility4ResendBtn
        {
            get { return (Visibility)GetValue(Visibility4ResendBtnPropertyProperty); }
            set { SetValue(Visibility4ResendBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4ResendBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Visibility4ResendBtnPropertyProperty =
            DependencyProperty.Register("Visibility4ResendBtn", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Collapsed, onVisibility4ResendBtn));

        // 线下成交按钮的Visibility属性
        public Visibility VisibilityManualBtn
        {
            get { return (Visibility)GetValue(VisibilityManualBtnPropertyProperty); }
            set { SetValue(VisibilityManualBtnPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility4CancelAllBtnProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityManualBtnPropertyProperty =
            DependencyProperty.Register("VisibilityManualBtn", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Collapsed, onVisibilityManualBtn));

        //合约筛选的Visibility属性
        public Visibility ContractVisibility
        {
            get { return (Visibility)GetValue(ContractVisibilityProperty); }
            set { SetValue(ContractVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContractVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContractVisibilityProperty =
            DependencyProperty.Register("ContractVisibility", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Visible, onContractVisibility));

        // 来源筛选的Visibility属性
        public Visibility SourceVisibility
        {
            get { return (Visibility)GetValue(SourceVisibilityProperty); }
            set { SetValue(SourceVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceVisibilityProperty =
            DependencyProperty.Register("SourceVisibility", typeof(Visibility), typeof(MyOrdersFilterPanel),
                new PropertyMetadata(Visibility.Visible, onSourceVisibility));

        //一键撤单的文字
        public string CancelAllBtnText
        {
            get { return (string)GetValue(CancelAllBtnTextProperty); }
            set { SetValue(CancelAllBtnTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CancelAllBtnText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelAllBtnTextProperty =
            DependencyProperty.Register("CancelAllBtnText", typeof(string), typeof(MyOrdersFilterPanel)
                , new PropertyMetadata("一键撤单", onCancelAllBtnTextChanged));

        private static void onCancelAllBtnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.CancelAllBtn.Content = (string)args.NewValue;
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyOrdersFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public MyOrdersFilterPanel()
        {
            InitializeComponent();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["内部"] = "内部";
            dic["做市"] = "做市";
            this.VenuesMscb.ItemsSource = dic;
        }

        private void VenuesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                FilterDic["Venues"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void FirmFlagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                FilterDic["FirmFlag"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private static void onVisibility4CancelAllBtn(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.CancelAllBtn.Visibility = (Visibility)args.NewValue;
        }

        private static void onVisibility4ResendBtn(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.ResendBtn.Visibility = (Visibility)args.NewValue;
        }

        private static void onVisibilityManualBtn(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.ManualBtn.Visibility = (Visibility)args.NewValue;
            if (source.ManualBtn.Visibility == Visibility.Hidden)
                source.ManualBtn.Width = 0;
        }


        private static void onContractVisibility(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.ContractType.Visibility = (Visibility)args.NewValue;
            if (source.ContractType.Visibility == Visibility.Hidden)
                source.ContractType.Width = 0;

            source.ContractTypeListBox.Visibility = (Visibility)args.NewValue;
            if (source.ContractTypeListBox.Visibility == Visibility.Hidden)
                source.ContractTypeListBox.Width = 0;
        }

        private static void onSourceVisibility(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MyOrdersFilterPanel source = (MyOrdersFilterPanel)sender;
            source.SourceLbl.Visibility = (Visibility)args.NewValue;
            if (source.SourceLbl.Visibility == Visibility.Hidden)
                source.SourceLbl.Width = 0;

            source.VenuesMscb.Visibility = (Visibility)args.NewValue;
            if (source.VenuesMscb.Visibility == Visibility.Hidden)
                source.VenuesMscb.Width = 0;
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

        public event Action<object, RoutedEventArgs> OnCancelClick;

        private void CancelAllBtn_Click(object sender, RoutedEventArgs e)
        {
            OnCancelClick?.Invoke(sender, e);
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

        public event Action<object, RoutedEventArgs> OnManualClick;

        private void ManualBtn_Click(object sender, RoutedEventArgs e)
        {
            OnManualClick?.Invoke(sender, e);
        }

        private void VenuesMscb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.VenuesMscb.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            FilterDic["Venues"] = selectedList;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
