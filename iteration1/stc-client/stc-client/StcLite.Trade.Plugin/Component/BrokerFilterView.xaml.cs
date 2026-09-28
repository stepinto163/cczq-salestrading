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
    public partial class BrokerFilterView : UserControl
    {

        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(BrokerFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }


        public static readonly DependencyProperty SelectIndexProperty = DependencyProperty.Register("SelectIndex", typeof(int),
            typeof(BrokerFilterView), new PropertyMetadata());

        public int SelectIndex
        {
            get { return (int)GetValue(SelectIndexProperty); }
            set { SetValue(SelectIndexProperty, value); }
        }


        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrokerFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }


        public BrokerFilterView()
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

        public void SetSelectModel(SelectionMode selectionMode)
        {
            lbBroker.SelectionMode = selectionMode;
        }

        public void SetAllButtonVisibility(Visibility visibility)
        {
            this.All.Visibility = Visibility;
        }

        private int isTrueCount = 0;
        private void BrokerFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            LogHelper.Info("----BrokerFilterView_Loaded");
            this.Loaded -= BrokerFilterView_Loaded;

            try
            {
                string[] brokerSortArr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (brokerSortArr != null)
                {
                    int index = 1;
                    foreach (string bs in brokerSortArr)
                    {
                        if (CommonConstants.BrokerNumEnum.Contains(bs))
                        {
                            LogHelper.Info(string.Format("index:{0}, broker:{1}", index, bs));
                            string cn = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");
                            ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                            lb.Visibility = Visibility.Visible;

                            lb.Content = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");

                            index++;
                        }
                    }

                }
                //string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //List<String> cmbkey = ComboSourceUtil.Instance.GetComboSource("venues_cn").Select(x => x.Key).ToList();
                //if (arr != null)
                //{
                //    List<string> listC = arr.ToList();//.Intersect(cmbkey).ToList();
                //    //listC.Sort();
                //    isTrueCount = arr.Length;

                //    int index = 1;
                //    for (int i = listC.Count - 1; i >= 0; i--)
                //    {
                //        string cn = ComboSourceUtil.Instance.FindEnum(listC[i], "venues_cn");
                //        if (cn != listC[i])
                //        {
                //            ListBoxItem lb = lbBroker.Items[index] as ListBoxItem;
                //            lb.Visibility = Visibility.Visible;

                //            lb.Content = ComboSourceUtil.Instance.FindEnum(listC[i], "venues_cn");

                //            index++;
                //        }

                //    }
                //}
                //else
                //{
                //    return;
                //}
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

                this.SelectIndex = lbBroker.SelectedIndex;

                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
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
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];// 这个是AllBroker按钮
                if (isTrueCount == 6)
                {
                    //tmp.IsSelected = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 5)
                {
                    //tmp.IsSelected = false;
                }

                FilterDic.Clear();
                FilterDic["Broker"] = listBox.SelectedItems;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        /// <summary>
        /// 根据 Content 选择Settle
        /// </summary>
        /// <returns></returns>
        public void SetItemSelect(string content)
        {
            foreach (ListBoxItem item in lbBroker.Items)
            {
                if (item.Content.ToString() == content)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //foreach (ListBoxItem item in lbBroker.Items)
            //{
            //    item.Width = (this.ActualWidth) / lbBroker.Items.Count;
            //}
        }
    }
}
