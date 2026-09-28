using STC.Common.Utils;
using STC.Frame.Utils;
using STC.Rfq.Plugin.Utils;
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

namespace STC.Rfq.Plugin.View.Component
{
    /// <summary>
    /// SettleFilterView.xaml 的交互逻辑
    /// </summary>
    public partial class SettleFilterView : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(SettleFilterView), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get
            {
                Dictionary<string, IList> dic = (Dictionary<string, IList>)GetValue(FilterDicProperty);
                
                return dic;
            }
            set
            {
                SetValue(FilterDicProperty, value);

                //SetSelectedStatus(this.CopyDictionary(value));
            }
        }


        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettleFilterView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        private bool _isMainSettle = false;

        public bool IsMainSettle { get { return _isMainSettle; } set { _isMainSettle = value; } }

        public SettleFilterView()
        {
            InitializeComponent();

            this.Unloaded += SettleFilterView_Unloaded;
        }

        public Dictionary<string, IList> CopyDictionary(Dictionary<string, IList> oldDic)
        {
            Dictionary<string, IList> copyDic = new Dictionary<string, IList>();

            foreach (KeyValuePair<string, IList> kv in oldDic)
            {
                string key = kv.Key;

                copyDic.Add(key, CopyList(kv.Value));
            }

            return copyDic;
        }

        public IList CopyList(IList list)
        {
            List<object> result = new List<object>();

            foreach (object obj in list)
            {
                result.Add(obj);
            }

            return result;
        }

        private void SettleFilterView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SettleFilterView_Unloaded;

            ////if (IsMainSettle)
            ////    BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnSelectSettleChange;
        }

        public void SetMainHandle()
        {
            IsMainSettle = true;
            ////BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnSelectSettleChange;
        }

        ////private void Instance_OnSelectSettleChange(OperationType operationType, string groupId, object obj)
        ////{
        ////    if (operationType == OperationType.Main)
        ////    {
        ////        List<string> allSettle = obj as List<string>;
        ////        if (allSettle != null)
        ////        {
        ////            _isChange = false;
                    
        ////            foreach (ListBoxItem item in lbSettle.Items)
        ////            {
        ////                item.IsSelected = false;
        ////            }
                    
        ////            if (allSettle.Count == 1)
        ////            {
        ////                foreach (ListBoxItem item in lbSettle.Items)
        ////                {
        ////                    if (item.Content.ToString() == allSettle[0])
        ////                        item.IsSelected = true;
        ////                }
        ////            }
        ////            else
        ////            {
        ////                //LbSettle_SelectionChanged(null, null);
        ////            }

        ////            if (FilterDic != null)
        ////                FilterDic.Clear();

        ////            _isChange = true;
        ////        }
        ////    }
        ////}

        private ListBoxItem GetListBoxItem(string content)
        {
            ListBoxItem result = null;
            foreach (ListBoxItem item in lbSettle.Items)
            {
                if (item.Content.ToString() == content)
                {
                    result = item;

                    break;
                }
            }

            if (result != null)
                result.IsSelected = true;

            return result;
        }

        private bool _isChange = true;
        public void SetItemSelectedStatus(Dictionary<string, IList> filterDic)
        {

            _isChange = false;

            foreach (ListBoxItem item in lbSettle.Items)
            {
                item.IsSelected = false;
            }

            _isChange = true;
            if (filterDic.ContainsKey(RfqConstants.SETTLE))
            {
                IList settlTypeList = filterDic[RfqConstants.SETTLE];

                if (settlTypeList.Count > 0)
                {
                    foreach (ListBoxItem settlType in settlTypeList)
                    {
                        GetListBoxItem(settlType.Content.ToString());
                    }
                }
            }
        }

        public void SetSelectedStatus(Dictionary<string, IList> filterDic)
        {

            _isChange = false;

            foreach (ListBoxItem item in lbSettle.Items)
            {
                item.IsSelected = false;
            }

            
            if (filterDic.ContainsKey(RfqConstants.SETTLE))
            {
                IList settlTypeList = filterDic[RfqConstants.SETTLE];

                if (settlTypeList.Count > 0)
                {
                    foreach (ListBoxItem settlType in settlTypeList)
                    {
                        GetListBoxItem(settlType.Content.ToString());
                    }
                }
            }

            _isChange = true;
        }

        public void SetButtonProperty(int width, int height)
        {
            foreach (ListBoxItem item in lbSettle.Items)
            {
                item.Width = width;
                item.Height = height;
            }
        }

        public void SetButtonVisibility(string content, Visibility visibility)
        {
            foreach (ListBoxItem item in lbSettle.Items)
            {
                if (item.Content.Equals(content))
                {
                    item.Visibility = visibility;

                    break;
                }
            }
        }

        public void SetSelectedIndex(int index)
        {
            _isChange = false;

            lbSettle.SelectedIndex = index;

            if (index < 0)
            {
                FilterDic.Clear();

                Console.WriteLine(string.Format("**********---------------Current _isMainSetle:{0}, dic count:{1}", _isMainSettle, FilterDic.Count));
            }

            _isChange = true;
        }

        private void LbSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isChange)
                return;

            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;

                int isTrueCount = 0;// 如果是true的个数是0或者5时， allbroker按钮被选中 (五家经纪商级别)
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (!CommonConstants.ALL_PARTY.Equals(lbi.Content) && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[3];// 这个是AllBroker按钮
                
                if (isTrueCount == 3)
                {
                    lbSettle.SelectionChanged -= LbSettle_SelectionChanged;
                    ((ListBoxItem)listBox.Items[0]).IsSelected = ((ListBoxItem)listBox.Items[1]).IsSelected = ((ListBoxItem)listBox.Items[2]).IsSelected = false;
                    lbSettle.SelectionChanged += LbSettle_SelectionChanged;
                    ((ListBoxItem)listBox.Items[3]).IsSelected  = true;
                }
                else if (isTrueCount > 0 && isTrueCount < 4)
                {
                    tmp.IsSelected = false;
                }

                if (FilterDic != null)
                    FilterDic.Clear();
                else
                    FilterDic = new Dictionary<string, IList>();

                FilterDic["Settle"] = listBox.SelectedItems;

                foreach (ListBoxItem ite in listBox.SelectedItems)
                {
                    if (ite.Content.ToString() == "远")
                    {
                        SetEllipseVisibility(Visibility.Collapsed);
                        break;
                    }
                }

                FilterDic = FilterDic;
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        /// <summary>
        /// 设置小红点是否可见
        /// </summary>
        /// <param name="visibility"></param>
        public void SetEllipseVisibility(Visibility visibility)
        {
            ListBoxItem selectedItem = lbSettle.SelectedItem as ListBoxItem;

            Ellipse obj = lbtFwd.Template.FindName("elli", lbtFwd) as Ellipse;

            if (obj != null)
            {
                if (selectedItem == lbtFwd)
                    obj.Visibility = Visibility.Collapsed;
                else
                    obj.Visibility = visibility;
            }
        }

        public List<string> GetSelectContent()
        {
            List<string> result = new List<string>();
            foreach (ListBoxItem lbi in lbSettle.Items)
            {
                if (lbi.IsSelected)
                    result.Add(lbi.Content.ToString());
            }

            return result;
        }

        /// <summary>
        /// 根据 Content 选择Settle
        /// </summary>
        /// <returns></returns>
        public void SetItemSelect(string content)
        {
            foreach (ListBoxItem item in lbSettle.Items)
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
    }
}
