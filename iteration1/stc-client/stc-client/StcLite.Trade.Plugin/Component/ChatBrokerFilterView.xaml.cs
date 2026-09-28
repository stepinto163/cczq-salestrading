using com.gateway.log;
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
    /// BrokerFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class ChatBrokerFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(ChatBrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChatBrokerFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public string DefaultSelectItem
        {
            get { return (string)GetValue(DefaultSelectItemProperty); }
            set { SetValue(DefaultSelectItemProperty, value); }
        }

        public bool LockOtherNotSelectItem
        {
            get { return (bool)GetValue(LockOtherNotSelectItemProperty); }
            set { SetValue(LockOtherNotSelectItemProperty, value); }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        public static readonly DependencyProperty DefaultSelectItemProperty = DependencyProperty.Register("DefaultSelectItem", typeof(string), typeof(ChatBrokerFilterView), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LockOtherNotSelectItemProperty = DependencyProperty.Register("LockOtherNotSelectItem", typeof(bool), typeof(ChatBrokerFilterView), new PropertyMetadata(false));


        public ChatBrokerFilterView()
        {
            InitializeComponent();

            this.Loaded += BrokerFilterView_Loaded;
        }

        private int isTrueCount = 0;
        private void BrokerFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BrokerFilterView_Loaded;

            try
            {
                //// 增加下单权限,修改 IdcTradePermissions 下单权限判断方法，只有0，1
                //if (LocalEnv.UserInfo.IdcTradePermissions != null)
                //{
                //    if (LocalEnv.UserInfo.IdcTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0] == "1")
                //    {
                //        IdcLst = LocalEnv.UserInfo.IDCPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //    }
                //}

                if ((string.IsNullOrEmpty(LocalEnv.UserInfo.BondTradePermissions)) || (string.IsNullOrEmpty(LocalEnv.UserInfo.IDCPermissions)))
                {
                    for (int i = 0; i < lbBroker.Items.Count; i++)
                    {
                        ListBoxItem lb = lbBroker.Items[i] as ListBoxItem;
                       lb.Visibility = Visibility.Collapsed;
                    }
                    return;
                }

                List<string> LstBondTrade = LocalEnv.UserInfo.BondTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                Dictionary<string, string> dicp = ComboSourceUtil.Instance.GetComboSource("venues_en").ToDictionary(x => x.Key, x => x.Value);
                List<string> IdcPermissions = new List<string>();
                for (int i = 0;i<LstBondTrade.Count;i++)
                {
                    if (dicp.ContainsKey(LstBondTrade[i]))
                    {
                        IdcPermissions.Add(dicp[LstBondTrade[i]]);
                    }
                }

                for (int i = 0; i < IdcPermissions.Count; i++)
                {
                    ListBoxItem lb = lbBroker.Items[i] as ListBoxItem;
                    if (!IdcPermissions.Contains(lb.Tag.ToString()))
                    {
                        lb.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 根据Tag 设置小红点
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="v"></param>
        public void SetEllipseVisibility(string broker,string v)
        {
            foreach (ListBoxItem item in lbBroker.Items)
            {
                if (item.Tag.ToString() == broker)
                {
                    item.Uid = v;
                    break;
                }
            }
        }

        /// <summary>
        /// 清空所有的小红点
        /// </summary>
        public void SetAllEllipseCollapsed()
        {
            foreach (ListBoxItem item in lbBroker.Items)
            {
                item.Uid = "0";
            }
        }
        private void LbBroker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                FilterDic.Clear();
                FilterDic["Broker"] = listBox.SelectedItems;
                (listBox.SelectedItems[0] as ListBoxItem).Uid = "0";
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        /// <summary>
        /// 根据 Tag 选择Settle
        /// </summary>
        /// <returns></returns>
        public void SetItemSelect(string content)
        {
            foreach (ListBoxItem item in lbBroker.Items)
            {
                if (item.Tag.ToString() == content)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }
    }
}
