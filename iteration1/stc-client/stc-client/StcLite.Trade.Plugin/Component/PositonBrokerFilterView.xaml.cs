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
    public partial class PositionBrokerFilterView : UserControl
    {

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(PositionBrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }

        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PositionBrokerFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public PositionBrokerFilterView()
        {
            InitializeComponent();

            this.Loaded += BrokerFilterView_Loaded;
        }

        public void SetButtonProperty(int width, int height)
        {
            foreach (ListBoxItem item in this.lbBroker.Items)
            {
                item.Width = width;
                item.Height = height;
            }
        }

        private int isTrueCount = 0;
        private void BrokerFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BrokerFilterView_Loaded;

            try
            {
                if ((string.IsNullOrEmpty(LocalEnv.UserInfo.BondTradePermissions)) || (string.IsNullOrEmpty(LocalEnv.UserInfo.IDCPermissions)))
                {
                    for (int i = 0; i < lbBroker.Items.Count; i++)
                    {
                        ListBoxItem lb = lbBroker.Items[i] as ListBoxItem;
                        lb.Visibility = Visibility.Collapsed;
                    }
                    return;
                }
                
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

                Dictionary<string, string> dicp = ComboSourceUtil.Instance.GetComboSource("venues_en").ToDictionary(x => x.Key, x => x.Value);
                List<string> IdcPermissions = new List<string>();
                foreach (var k in dicp)
                {
                    if (IdcLst.Contains(k.Key))
                    {
                        IdcPermissions.Add(k.Value);
                    }
                }
                IdcPermissions.Add("ALL");

                List<String> cmbkey = ComboSourceUtil.Instance.GetComboSource("venues_cn_tocn").Select(x => x.Key).ToList();
                
                if (arr != null)
                {
                    List<string> listC = arr.ToList();//.Intersect(cmbkey).ToList();
                    listC.Sort();
                    isTrueCount = arr.Length;

                    int index = 0;
                    for (int i = listC.Count; i >= 0; i--)
                    {
                        ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                        
                        // 增加下单权限
                        if (IdcPermissions.Contains(lb.Tag.ToString()))
                        {
                            lb.Visibility = Visibility.Visible;
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

                //int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if ((CommonConstants.CNEX.Equals(lbi.Content)
                    || CommonConstants.TPSC.Equals(lbi.Content)
                    || CommonConstants.CTTJ.Equals(lbi.Content)
                    || CommonConstants.PTCN.Equals(lbi.Content)
                    || CommonConstants.CCTB.Equals(lbi.Content))
                    && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];// 这个是AllBroker按钮
                if (isTrueCount == 5)
                {
                    tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 5)
                {
                    tmp.IsSelected = false;
                }

                FilterDic.Clear();
                FilterDic["Broker"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
