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
    public partial class PriceBrokerFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(PriceBrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PriceBrokerFilterView));

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
        public static readonly DependencyProperty DefaultSelectItemProperty = DependencyProperty.Register("DefaultSelectItem", typeof(string), typeof(PriceBrokerFilterView), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LockOtherNotSelectItemProperty = DependencyProperty.Register("LockOtherNotSelectItem", typeof(bool), typeof(PriceBrokerFilterView), new PropertyMetadata(false));


        public PriceBrokerFilterView()
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
                string[] arr = LocalEnv.UserInfo.BondTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                // 增加下单权限,修改 IdcTradePermissions 下单权限判断方法，只有0，1
                List<string> IdcLst = new List<string>();
                if (LocalEnv.UserInfo.IdcTradePermissions != null)
                {
                    if (LocalEnv.UserInfo.IdcTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0] == "1")
                    {
                        IdcLst = LocalEnv.UserInfo.IDCPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
                //List<string> IdcLst = LocalEnv.UserInfo.IdcTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Intersect(LocalEnv.UserInfo.IDCPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();

                Dictionary<string,string> dicp = ComboSourceUtil.Instance.GetComboSource("venues_en").ToDictionary(x => x.Key, x => x.Value);
                List<string> IdcPermissions = new List<string>();
                foreach (var k in dicp)
                {
                    if (IdcLst.Contains(k.Key))
                    {
                        IdcPermissions.Add(k.Value);
                    }
                }

                List<String> cmbkey = ComboSourceUtil.Instance.GetComboSource("venues_cn_tocn").Select(x => x.Key).ToList();
                if (arr != null)
                {
                    List<string> listC = arr.ToList();//.Intersect(cmbkey).ToList();
                    //listC.Sort();
                    isTrueCount = arr.Length;

                    int index = 0;
                    for (int i = listC.Count-1; i >= 0; i--)
                    {
                        ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                        lb.Visibility = Visibility.Visible;
                        lb.Content = ComboSourceUtil.Instance.FindEnum(listC[i], "venues_cn_tocn");
                        if (DefaultSelectItem != null)
                        {
                            if (lb.Tag.ToString() == DefaultSelectItem)
                            {
                                lb.IsSelected = true;
                            }
                            else
                            {
                                if (LockOtherNotSelectItem)
                                {
                                    lb.IsEnabled = false;
                                }
                            }
                            // 增加下单权限
                            if (!IdcPermissions.Contains(lb.Tag.ToString()))
                            {
                                lb.IsEnabled = false;
                            }
                        }
                        index++;
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void LbBroker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                FilterDic.Clear();
                FilterDic["Broker"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
