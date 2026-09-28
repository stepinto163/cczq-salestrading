using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.quant.plugin.Utils;
using System;
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

namespace STC.quant.plugin.View.WorkTable
{
    /// <summary>
    /// ElectronicChannelView.xaml 的交互逻辑
    /// </summary>
    public partial class ElectronicChannelView : UserControl
    {
        /// <summary>
        /// 利率:BNC,信用:BCO
        /// </summary>
        public string SecurityType
        {
            get { return (string)GetValue(SecurityTypeProperty); }
            set { SetValue(SecurityTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecurityType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecurityTypeProperty =
            DependencyProperty.Register("SecurityType", typeof(string), typeof(ElectronicChannelView), new FrameworkPropertyMetadata(string.Empty, OnSecurityTypeChanged));


        private static void OnSecurityTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string securityType = e.NewValue.ToString();
            var control = d as ElectronicChannelView;
            if (control != null)
            {
                List<ListBoxItemData> itemsSource = VenueToListBoxItemData(SystemParamHandler.Instance.GetListVenueByCondition(securityType, string.Empty));
                control.lb.ItemsSource = itemsSource;
                SetItemParam(itemsSource, securityType, "3", control);
            }
        }

        /// <summary>
        /// 选择的指定渠道
        /// </summary>
        public string SelfSelectVenues
        {
            get { return (string)GetValue(SelfSelectVenuesProperty); }
            set { SetValue(SelfSelectVenuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfSelectVenues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelfSelectVenuesProperty =
            DependencyProperty.Register("SelfSelectVenues", typeof(string), typeof(ElectronicChannelView));

        public static readonly RoutedEvent CheckValueChangedEvent = EventManager.RegisterRoutedEvent("CheckValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ElectronicChannelView));

        public event RoutedEventHandler CheckValueChanged
        {
            add { AddHandler(CheckValueChangedEvent, value); }
            remove { RemoveHandler(CheckValueChangedEvent, value); }
        }

        private void LbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelfSelectVenues = GetSelectItem();
            RaiseEvent(new RoutedEventArgs { RoutedEvent = CheckValueChangedEvent });
        }

        public ElectronicChannelView()
        {
            InitializeComponent();
        }

        private static List<ListBoxItemData> VenueToListBoxItemData(List<Venue> venues)
        {
            List<ListBoxItemData> result = null;

            if (venues != null && venues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (Venue uv in venues)
                {
                    ListBoxItemData itemData = new ListBoxItemData();
                    itemData.Code = uv.venue;
                    itemData.Name = uv.name;
                    itemData.Seq = uv.sn;
                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        static SolidColorBrush greenColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
        private static void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType, ElectronicChannelView control)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = greenColor;

                    bool isContain = false;
                    List<UserVenue> userItemDatas = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, operationType);
                    if (userItemDatas != null && userItemDatas.Count > 0)
                    {
                        foreach (UserVenue uv in userItemDatas)
                        {
                            if (!string.IsNullOrEmpty(uv.selfselectvenues) && uv.selfselectvenues.Contains(item.Code))
                            {
                                item.Checked = true;
                                isContain = true;
                                break;
                            }
                        }
                    }
                    if (!isContain)
                        item.Checked = false;
                }
            }
        }

        public string GetSelectItem()
        {
            string result = string.Empty;

            foreach (ListBoxItemData itemData in this.lb.SelectedItems)
            {
                if (itemData != null)
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }
    }
}
