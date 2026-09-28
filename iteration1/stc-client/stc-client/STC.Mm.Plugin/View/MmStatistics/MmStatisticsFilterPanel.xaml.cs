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

namespace STC.Mm.Plugin.View
{
    /// <summary>
    /// MmStatisticsFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MmStatisticsFilterPanel : UserControl
    {
        //过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(MmStatisticsFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));
        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        //public static readonly DependencyProperty IsShowBondTypeProperty = DependencyProperty.Register("IsShowBondType", typeof(bool), typeof(MmStatisticsFilterPanel),
        //            new PropertyMetadata(false, OnIsShowBondType));

        //private static void OnIsShowBondType(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.NewValue == null) return;

        //    MmStatisticsFilterPanel source = (MmStatisticsFilterPanel)d;
        //    if ((bool)e.NewValue)
        //    {
        //        source.BondType.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        source.BondType.Visibility = Visibility.Collapsed;
        //    }
        //}

        //public bool IsShowBondType
        //{
        //    get { return (bool)GetValue(IsShowBondTypeProperty); }
        //    set
        //    {
        //        SetValue(IsShowBondTypeProperty, value);
        //    }
        //}

        //public static readonly DependencyProperty IsShowSearchSecurityIDProperty = DependencyProperty.Register("IsShowSearchSecurityID", typeof(bool), typeof(MmStatisticsFilterPanel),
        //        new PropertyMetadata(false, OnIsShowSearchSecurityID));

        //private static void OnIsShowSearchSecurityID(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.NewValue == null) return;

        //    MmStatisticsFilterPanel source = (MmStatisticsFilterPanel)d;

        //    if ((bool)e.NewValue)
        //    {
        //        source.bond_txt.Visibility = Visibility.Visible;
        //        source.search_btn.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        source.bond_txt.Visibility = Visibility.Collapsed;
        //        source.search_btn.Visibility = Visibility.Collapsed;
        //    }
        //}

        //public bool IsShowSearchSecurityID
        //{
        //    get { return (bool)GetValue(IsShowSearchSecurityIDProperty); }
        //    set
        //    {
        //        SetValue(IsShowSearchSecurityIDProperty, value);
        //    }
        //}

        private bool isClick = true;
        public bool IsClick
        {
            get
            {
                return isClick;
            }

            set
            {
                isClick = value;
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MmStatisticsFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        private string today = string.Empty;
        private string week = string.Empty;
        private string month = string.Empty;
        private string quarter = string.Empty;

        public MmStatisticsFilterPanel()
        {
            InitializeComponent();
            today = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            week = DateTimeEx.ServerTime.AddDays(-7).ToString("yyyy-MM-dd");
            month = DateTimeEx.ServerTime.AddDays(-30).ToString("yyyy-MM-dd");
            quarter = DateTimeEx.ServerTime.AddDays(-90).ToString("yyyy-MM-dd");

            this.dpFrom.SelectedDate = DateTimeEx.ServerTime;
            this.dpTo.SelectedDate = DateTimeEx.ServerTime;

            this.SetTimeSpanType(this.dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"), this.dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
        }

        public void SetTimeSpanType(string begin,string end)
        {
            ListBoxItem itemToday = this.TimeSpanType.Items[0] as ListBoxItem;
            ListBoxItem itemWeek = this.TimeSpanType.Items[1] as ListBoxItem;
            ListBoxItem itemMonth= this.TimeSpanType.Items[2] as ListBoxItem;
            ListBoxItem itemQuarter = this.TimeSpanType.Items[3] as ListBoxItem;

            itemToday.IsSelected = false;
            itemWeek.IsSelected = false;
            itemMonth.IsSelected = false;
            itemQuarter.IsSelected = false;

            if (end == today)
            {
                if (begin == end && end== today)
                {
                    itemToday.IsSelected = true;
                }
                else
                {

                    if (begin == week)
                    {
                        itemWeek.IsSelected = true;
                    }
                    else if (begin == month)
                    {
                        itemMonth.IsSelected = true;
                    }
                    else if (begin == quarter)
                    {
                        itemQuarter.IsSelected = true;
                    }
                }
            }
        }

        private void SetDpFromToDate(string text)
        {
            if (string.IsNullOrEmpty(today) || string.IsNullOrEmpty(week) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(quarter))
            {
                return;
            }

            switch (text)
            {
                case "今日":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(today);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;

                case "最近一周":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(week);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;

                case "最近一月":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(month);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;

                case "最近一季":
                    this.dpFrom.SelectedDate = Convert.ToDateTime(quarter);
                    this.dpTo.SelectedDate = Convert.ToDateTime(today);
                    break;
            }
        }

        private void TimeSpanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                ListBoxItem selected = listBox.SelectedItem as ListBoxItem;
                if (selected != null)
                { SetDpFromToDate(selected.Content.ToString()); }
            }

            //RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            //IsClick = true;
        }

        private void bond_txt_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
        }

        private void dpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpTo.SelectedDate = dpFrom.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            SetTimeSpanType(list[0].ToString(), list[1].ToString());
            FilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
        }

        private void dpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpFrom.SelectedDate = dpTo.SelectedDate.Value;
                }
            }
            IList list = new ArrayList();
            if (dpFrom.SelectedDate.HasValue)
                list.Add(dpFrom.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            if (dpTo.SelectedDate.HasValue)
                list.Add(dpTo.SelectedDate.Value.Date.ToString("yyyy-MM-dd"));
            else
                list.Add(DateTime.Now.Date.ToString("yyyy-MM-dd"));
            SetTimeSpanType(list[0].ToString(), list[1].ToString());
            FilterDic["TransactDate"] = list;
            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            IsClick = true;
        }

        private void BondType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void search_btn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void bond_txt_ValueChanged(object sender, RoutedEventArgs e)
        {
        }
    }
}
