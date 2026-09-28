using STC.Common.Handler.Admin;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
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

namespace STC.Common.View.WorkTable
{
    /// <summary>
    /// ElectronicChannelView.xaml 的交互逻辑
    /// </summary>
    public partial class ElectronicChannelView : UserControl
    {
        #region 渠道权限和交易权限
        private static readonly string XBOND = "XBOND";
        /// <summary>
        /// 渠道权限
        /// </summary>
        private static readonly string MODULEID = "1002";
        /// <summary>
        /// 所有权限
        /// </summary>
        private static readonly string CHECK_ALL_ACTIONID = "100203";
        /// <summary>
        /// 仅内部
        /// </summary>
        private static readonly string CHECK_INNER_ACTIONID = "100201";
        /// <summary>
        /// 内部，X-Bond
        /// </summary>
        private static readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        /// 是否有内部撮合权限
        public bool IsHaveVenueInnerPermission = false;

        /// <summary>
        /// 渠道权限-仅内部
        /// </summary>
        private static bool IsHaveInnerPermission = false;

        /// <summary>
        /// 渠道权限-内部，X-Bond
        /// </summary>
        private static bool IsHaveInnerXbondPermission = false;

        /// <summary>
        /// 渠道权限-所有权限
        /// </summary>
        private static bool IsHaveAllPermission = false;

        #endregion
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
                itemsSource.RemoveAll(p => p.Code == "QDMESP");
                // 获取渠道权限
                bool IsHaveInnerPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                bool IsHaveInnerXbondPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
                bool IsHaveAllPermission = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
                if (!(IsHaveAllPermission || IsHaveInnerXbondPermission))
                {
                    itemsSource.RemoveAll(p => p.Name == XBOND);
                }

                if ((!IsHaveInnerPermission) && (!IsHaveInnerXbondPermission) && (!IsHaveAllPermission))
                {
                    itemsSource.Clear();
                }

                if (IsHaveInnerPermission)
                {
                    itemsSource.Clear();
                }

                if (IsHaveInnerXbondPermission)
                {
                    itemsSource.RemoveAll(p => p.Name != XBOND);
                }

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
                    itemData.ItemEnable = true;
                    itemData.Seq = uv.sn;
                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        //static SolidColorBrush greenColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#136C5E"));
        private static void SetItemParam(List<ListBoxItemData> itemDatas, string goodsType, string operationType, ElectronicChannelView control)
        {
            if (itemDatas != null && itemDatas.Count > 0)
            {
                foreach (ListBoxItemData item in itemDatas)
                {
                    item.BgColor = Application.Current.MainWindow.FindResource("TabItem.Selected.Background") as SolidColorBrush;

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
