using BDB.Trade.Plugin.Handler;
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
    /// CustomSettleView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomSettleView : UserControl
    {
        private int originWith = 76;
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(CustomSettleView), new PropertyMetadata(new Dictionary<string, IList>()));

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


        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomSettleView));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        private bool _isMainSettle = false;

        public bool IsMainSettle { get { return _isMainSettle; } set { _isMainSettle = value; } }


        public CustomSettleView()
        {
            InitializeComponent();

            this.Unloaded += SettleFilterView_Unloaded;
        }

        private void SettleFilterView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SettleFilterView_Unloaded;

            if (IsMainSettle)
                BondChangeHandler.Instance.OnSelectSettleChange -= Instance_OnSelectSettleChange;
        }

        public void SetMainHandle()
        {
            IsMainSettle = true;
            BondChangeHandler.Instance.OnSelectSettleChange += Instance_OnSelectSettleChange;
        }

        private void Instance_OnSelectSettleChange(OperationType operationType, string groupId, object obj)
        {
            if (operationType == OperationType.Main)
            {
                List<string> allSettle = obj as List<string>;
                if (allSettle != null)
                {
                    _isChange = false;

                    foreach (ListBoxItem item in gdSettle.Items)
                    {
                        item.IsSelected = false;
                    }

                    if (allSettle.Count == 1)
                    {
                        foreach (ListBoxItem item in gdSettle.Items)
                        {
                            if (item.Content.ToString() == allSettle[0])
                                item.IsSelected = true;
                        }
                    }
                    else
                    {
                        //LbSettle_SelectionChanged(null, null);
                    }

                    if (FilterDic != null)
                        FilterDic.Clear();

                    _isChange = true;
                }
            }
        }


        private bool _isChange = true;

        public void SetSelectedIndex(int index)
        {
            _isChange = false;

            if (emptyTB != null)
            {
                if (gdSettle.SelectedIndex == -1)
                    emptyTB.Visibility = Visibility.Visible;
                else
                    emptyTB.Visibility = Visibility.Collapsed;
            }

            gdSettle.SelectedIndex = index;

            if (index < 0)
            {
                FilterDic.Clear();

                Console.WriteLine(string.Format("**********---------------Current _isMainSetle:{0}, dic count:{1}", _isMainSettle, FilterDic.Count));
            }

            _isChange = true;
        }

        private void gdSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (emptyTB != null)
            {
                if (gdSettle.SelectedIndex == -1)
                    emptyTB.Visibility = Visibility.Visible;
                else
                    emptyTB.Visibility = Visibility.Collapsed;
            }


            if (!_isChange)
                return;

            string content = string.Empty;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                content = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            }

            List<ListBoxItem> selectedItems = new List<ListBoxItem>();

            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = content;

            selectedItems.Add(listBoxItem);

            FilterDic.Clear();
            FilterDic[CommonConstants.SETTLE] = selectedItems;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }
    }
}
