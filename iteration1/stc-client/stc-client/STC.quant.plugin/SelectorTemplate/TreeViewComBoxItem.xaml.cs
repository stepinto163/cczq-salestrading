using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.quant.plugin.Utils.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.quant.plugin.SelectorTemplate
{
    /// <summary>
    /// TreeViewComBoxItem.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewComBoxItem : UserControl
    {
        private Timer _interval;

        private const string ALL = "全部";

        //public List<ChildTag> ItemsSource
        //{
        //    get { return (List<ChildTag>)GetValue(ItemsSourceProperty); }
        //    set { SetValue(ItemsSourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ItemsSourceProperty =
        //    DependencyProperty.Register("ItemsSource", typeof(List<ChildTag>), typeof(TreeViewComBoxItem), new PropertyMetadata(null, OnItemsSourceChanged));

        //private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.NewValue != null)
        //    {
        //        (d as TreeViewComBoxItem).InitData(e.NewValue as List<ChildTag>);
        //    }
        //}

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(TreeViewComBoxItem), new PropertyMetadata(false, OnIsEditableChanged));

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewComBoxItem control = d as TreeViewComBoxItem;

            if (control != null && e.NewValue != null)
            {
                if ((bool)e.NewValue)
                {
                    if (control._interval == null)
                    {
                        control._interval = new Timer(200);
                        control._interval.AutoReset = true;
                        control._interval.Elapsed += new ElapsedEventHandler(control._interval_Elapsed);
                    }
                }
                else
                {
                    if (control._interval != null)
                    {
                        control._interval.Close();
                        control._interval = null;
                    }
                }
            }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(TreeViewComBoxItem), new PropertyMetadata("", OnDefaultChanged));

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                TreeViewComBoxItem control = (TreeViewComBoxItem)d;
                control.SelectedTexts = e.NewValue.ToString();
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewComBoxItem));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        /// <summary>
        /// 选择的文本列表 ,分割
        /// </summary>
        public string SelectedTexts
        {
            get { return (string)GetValue(SelectedTextsProperty); }
            set { SetValue(SelectedTextsProperty, value); }
        }

        public static readonly DependencyProperty SelectedTextsProperty =
            DependencyProperty.Register("SelectedTexts", typeof(string), typeof(TreeViewComBoxItem), new UIPropertyMetadata(""));

        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedIdProperty =
            DependencyProperty.Register("SelectedId", typeof(string), typeof(TreeViewComBoxItem),// new UIPropertyMetadata(""));
                 new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedIdChanged)));
        private static void OnSelectedIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            TreeViewComBoxItem control = (TreeViewComBoxItem)d;
            control.SetId(e.NewValue + "");

        }

        /// <summary>
        /// 选择的值列表 ,分割
        /// </summary>
        public string SelectedValues
        {
            get { return (string)GetValue(SelectedValuesProperty); }
            set
            {
                SetValue(SelectedValuesProperty, value);
                RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            }
        }

        public static readonly DependencyProperty SelectedValuesProperty =
         DependencyProperty.Register("SelectedValues", typeof(string), typeof(TreeViewComBoxItem),
             new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuesChanged)));


        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            TreeViewComBoxItem control = (TreeViewComBoxItem)d;
            StringBuilder valuesb = new StringBuilder();
            int selectedItemsCount = 0;
            //control.iterateor(control._collection, null, valuesb, ref selectedItemsCount);
            if (e.NewValue.ToString() != valuesb.ToString())
            {
                string selectedValues = e.NewValue.ToString();
                string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                //foreach (TBookBookTreeItemModel item in control._selectedOrgs)
                //{
                //    item.IsChecked = false;
                //}
                control._selectedOrgs.Clear();

                if (valuesArr != null && valuesArr.Length > 0 && control._collection != null && control._collection.Count > 0)
                {
                   // control.SetValue_Iterateor(control._collection, valuesArr);
                }

                control.SetText();
            }
        }




        public int SelectedCount
        {
            get { return (int)GetValue(SelectedCountProperty); }
            set { SetValue(SelectedCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCountProperty =
            DependencyProperty.Register("SelectedCount", typeof(int), typeof(TreeViewComBoxItem), new PropertyMetadata(0, OnSelectedCountChanged));

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            //TreeViewComBoxItem control = (TreeViewComBoxItem)d;
            //if (control != null)
            //{
            //    int count = e.NewValue.ConvertToInt();
            //    if (count > 0)
            //    {
            //        control.count_text.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        control.count_text.Visibility = Visibility.Collapsed;
            //    }
            //}
        }




        //public bool IsAll
        //{
        //    get
        //    {
        //        int selectedItemsCount = 0;
        //        iterateor(_collection, null, null, ref selectedItemsCount);
        //        int rootCount = _collection.Count(item => item.IsChecked == true);
        //        return selectedItemsCount == 0; //|| rootCount == _collection.Count;
        //    }
        //}
        /// <summary>
        /// 当前策略的用户
        /// </summary>
        public string QuantUser
        {
            get { return (string)GetValue(QuantUserProperty); }
            set { SetValue(QuantUserProperty, value); }
        }

        public static readonly DependencyProperty QuantUserProperty =
            DependencyProperty.Register("QuantUser", typeof(string), typeof(TreeViewComBoxItem), new UIPropertyMetadata(""));

        private TreeView _orgTree;
        private TreeView OrgTree
        {
            get
            {
                if (_orgTree == null)
                {
                    _orgTree = innerCCB.Template.FindName("orgTree", innerCCB) as TreeView;
                }
                return _orgTree;
            }
        }


        private TextBox _SearchTextBox;
        private TextBox SearchTextBox
        {
            get
            {
                if (_SearchTextBox == null)
                {
                    Popup p = innerCCB.Template.FindName("PART_Popup", innerCCB) as Popup;
                    if (p != null)
                    {
                        _SearchTextBox = p.FindName("SearchTextBox") as TextBox;
                    }
                }
                return _SearchTextBox;
            }
        }


        public TreeViewComBoxItem()
        {
            InitializeComponent();
            //this._orgTree.Loaded += orgTree_Loaded;
            //this.Loaded += MultiSelOrgTree_Loaded;
        }

        void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            // pause the timer
            this._interval.Stop();

            // show the drop down when user starts typing then stops
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                this.ViewSource.View?.Refresh();

                if (this.SearchTextBox == null || string.IsNullOrEmpty(this.SearchTextBox.Text))
                {
                    ExpandInternal(this.OrgTree, false);
                }
                else
                {
                    ExpandInternal(this.OrgTree, true);
                }
            },
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private bool _firstLoad = true;
        //private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TreeViewComBoxItem));
        private ObservableCollection<TBookBookTreeItemModel> _collection = new ObservableCollection<TBookBookTreeItemModel>();
        private ObservableCollection<TBookBookTreeItemModel> _selectedOrgs = new ObservableCollection<TBookBookTreeItemModel>();
        private bool _inited = false;

        public IList<TBookBookTreeItemModel> SelectedOrgs
        {
            get { return _selectedOrgs; }
        }

        private void MultiSelOrgTree_Loaded(object sender, RoutedEventArgs e)
        {
            if (_firstLoad)
            {
                _firstLoad = false;

            }
        }

        public void InitData()
        {
            //var list = SP.Get<Cache.ICacheService>().OrgCache.GetCameraOrgs();
            //var info = list.FirstOrDefault(p => p.PAR_ID == 0);

            //TBookBookTreeItemModel root = new TBookBookTreeItemModel();
            //root.Info = new ChildTag() { childTagName = ALL, childTagValue = ALL };
            //root.Name = ALL;

            //_collection.Add(root);
            Dictionary<string, ObservableCollection<TBookBookTreeItemModel>> dicBookUser = QuantGroupHandler.Instance.dicBookUser;
            if (QuantGroupHandler.Instance.dicBookUser.ContainsKey(QuantUser))
            {
                //this.ViewSource.Source = QuantGroupHandler.Instance.dicBookUser[QuantUser];
                //this.ViewSource.IsLiveFilteringRequested = true;
                //this.ViewSource.IsLiveSortingRequested = true;
                this.OrgTree.ItemsSource = QuantGroupHandler.Instance.dicBookUser[QuantUser];
            }
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                //this.ViewSource.View?.Refresh();
                ExpandInternal(this.OrgTree, true);
            });


            //this.ViewSource.View.Filter = x =>
            //{
            //    if (this.SearchTextBox != null)
            //    {
            //        TBookBookTreeItemModel data = x as TBookBookTreeItemModel;

            //        bool flag = false;
            //        if (data.Nodes != null && data.Nodes.Count > 0)
            //        {
            //            foreach (TBookBookTreeItemModel sub_data in data.Nodes)
            //            {
            //                if (sub_data.Name.Contains(this.SearchTextBox.Text) || (!string.IsNullOrEmpty(sub_data.PinYin) && sub_data.PinYin.Contains(this.SearchTextBox.Text)))
            //                {
            //                    flag = true;
            //                    break;
            //                }
            //            }
            //        }

            //        if (!string.IsNullOrEmpty(this.SearchTextBox.Text))
            //        {
            //            return data.Name.Contains(this.SearchTextBox.Text) || (!string.IsNullOrEmpty(data.PinYin) && data.PinYin.Contains(this.SearchTextBox.Text)) || flag;
            //        }
            //    }

            //    return true;
            //};
        }

        private void orgTree_Loaded(object sender, RoutedEventArgs e)
        {
            //ExpandInternal(this.orgTree);
        }

        //private void BuildTree(TBookBookTreeItemModel root, IList<ChildTag> orgs)
        //{
        //    if (orgs != null && orgs.Count > 0)
        //    {
        //        foreach (ChildTag item in orgs)
        //        {
        //            TBookBookTreeItemModel model = new TBookBookTreeItemModel();
        //            //model.Info = item;
        //            model.Name = item.childTagName;
        //            model.Id = item.childTagValue;
        //            //model.PinYin = item.pinyin;
        //            //model.IsChecked = false;
        //            model.Parent = root;
        //            if (root != null)
        //            {
        //                root.Nodes.Add(model);
        //            }
        //            BuildTree(model, item.childTags);
        //        }
        //    }
        //    else
        //    {
        //        //root.IsLeaf = true;
        //    }
        //}

        /// <summary>
        /// 展开树节点
        /// </summary>
        /// <param name="targetItemContainer"></param>
        private void ExpandInternal(System.Windows.Controls.ItemsControl targetItemContainer, bool isExpand)
        {
            try
            {
                if (targetItemContainer == null) return;
                if (targetItemContainer.Items == null) return;
                //foreach (Object item in targetItemContainer.Items)
                //{
                //    System.Windows.Controls.TreeViewItem treeItem = targetItemContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                //    //var info = item as TreeNode;
                //    if (treeItem == null || !treeItem.HasItems)
                //    {
                //        continue;
                //    }
                //    //if (info.Info == null)
                //    //{
                //    //    treeItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ad7fa"));
                //    //}
                //    treeItem.IsExpanded = isExpand;
                //    //ExpandInternal(treeItem as ItemsControl);
                //}

                //for (int i = 0; i < targetItemContainer.Items.Count; i++)
                //{
                //    System.Windows.Controls.TreeViewItem treeItem = targetItemContainer.Items[i] as System.Windows.Controls.TreeViewItem;

                //    if (treeItem == null) continue;

                //    if (!treeItem.HasItems) continue;

                //    treeItem.IsExpanded = true;

                //    ExpandInternal(treeItem, true);

                //}

                IEnumerable<TreeViewItem> treeViewItems = FindVisualChildren<TreeViewItem>(this.OrgTree);

                foreach (TreeViewItem treeItem in treeViewItems)
                {
                    if (treeItem == null) continue;
                    if (!treeItem.HasItems) continue;
                    treeItem.IsExpanded = true;
                    ExpandInternal(treeItem, true);
                }
            }
            catch (Exception ex)
            {
                //_log.ErrorFormat("ExpandInternal error", ex);
            }
        }

       

        private void cxb_Node_UnChecked(object sender, RoutedEventArgs e) { }

        private void cxb_Node_Checked(object sender, RoutedEventArgs e) { }

       

        public void ClearSelection()
        {
            _selectedOrgs.Clear();
            SetText();
            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.Text = string.Empty;
            }
        }
        //private void BtnClick(object sender, RoutedEventArgs e)
        //{
        //    popup.PlacementTarget = sender as Button;
        //    popup.Placement = PlacementMode.Bottom;
        //    popup.IsOpen = true;
        //}

        public void Select(List<string> orgIdList, ObservableCollection<TBookBookTreeItemModel> Nodes = null)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (!_inited)
                {
                    System.Threading.Thread.Sleep(100);
                }
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Nodes == null)
                    {
                        //foreach (TBookBookTreeItemModel item in _collection[0].Nodes)
                        //{
                        //    if (orgIdList.Exists(a => a == item.Info.childTagValue.ToString()))
                        //    {
                        //        item.IsChecked = true;
                        //        if (!_selectedOrgs.Contains(item))
                        //        {
                        //            _selectedOrgs.Add(item);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        item.IsChecked = false;
                        //        if (_selectedOrgs.Contains(item))
                        //        {
                        //            _selectedOrgs.Remove(item);
                        //        }
                        //    }
                        //    Select(orgIdList, item.Nodes);
                        //}
                    }
                    else
                    {
                        //foreach (TBookBookTreeItemModel item in Nodes)
                        //{
                        //    if (orgIdList.Exists(a => a == item.Info.childTagValue.ToString()))
                        //    {
                        //        item.IsChecked = true;
                        //        if (!_selectedOrgs.Contains(item))
                        //        {
                        //            _selectedOrgs.Add(item);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        item.IsChecked = false;
                        //        if (_selectedOrgs.Contains(item))
                        //        {
                        //            _selectedOrgs.Remove(item);
                        //        }
                        //    }
                        //    Select(orgIdList, item.Nodes);
                        //}


                    }
                    //ToggleButton.Tag = string.Join(",", _selectedOrgs.ToList().ConvertAll(a => a.Info.childTagName));
                    SetText();
                }));
            });
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ClearSelection();
        }

        private void SetText()
        {
            if (_selectedOrgs.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder valueSb = new StringBuilder();
                // iterateor(_collection, sb, valueSb, ref count);
                this.SelectedCount = _selectedOrgs.Count;
                this.SelectedId = _selectedOrgs[0].Id;
                this.SelectedTexts = _selectedOrgs[0].Name;
                this.SelectedValues = valueSb.ToString();
            }
            else
            {
                this.SelectedCount = 0;
                this.SelectedId = "";
                this.SelectedTexts = DefaultText;
                this.SelectedValues = string.Empty;
            }

            int _selectedCount = 0;
            //foreach (TBookBookTreeItemModel s in _collection)
            //{
            //    if (s.IsChecked && s.Name != ALL)
            //        _selectedCount++;
            //}
            if (_selectedCount != 0 && _selectedCount == _collection.Count)
            {
                //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = true;
                CheckBox chk = this.innerCCB.Template.FindName("allChk", innerCCB) as CheckBox;
                if (chk != null)
                {
                    chk.IsChecked = true;
                }
            }
            else if (_selectedCount != 0)
            {
                CheckBox chk = this.innerCCB.Template.FindName("allChk", innerCCB) as CheckBox;
                if (chk != null)
                {
                    chk.IsChecked = null;
                }
            }
            else
            {
                //_nodeList.FirstOrDefault(i => i.Title == ALL).IsSelected = false;
                CheckBox chk = this.innerCCB.Template.FindName("allChk", innerCCB) as CheckBox;
                if (chk != null)
                {
                    chk.IsChecked = false;
                }
            }
        }

        private void SetId(string id)
        {
            this.SelectedId = id;
            if (!string.IsNullOrEmpty(id))
            {
                TTbookBook d = AdminHandler.Instance.GetTTbookByID(id);
                this.SelectedTexts = d != null ? d.Bookname : ""; 
            }
        }



        private void innerCCB_Loaded(object sender, RoutedEventArgs e)
        {

            _inited = true;
            InitData();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void right_part_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (count_text.Visibility == Visibility.Collapsed)
            //{
            //    this.count_text.IsDropDownOpen = false;
            //}
        }

        private void MultiSelectCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this.innerCCB.IsDropDownOpen = true;
            ResetTimer();
        }

        protected void ResetTimer()
        {
            this._interval.Stop();
            this._interval.Start();
        }

        private void cleanBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.Text = string.Empty;
            }
        }

        private void orgTree_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TreeViewItem tree = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
                if (tree == null)
                    return;
                TBookBookTreeItemModel node = (TBookBookTreeItemModel)tree.Header;

                if (node != null && node.Nodes.Count == 0)
                {
                    _selectedOrgs = new ObservableCollection<TBookBookTreeItemModel>();
                    _selectedOrgs.Add(node);
                    //CheckChild(node, false);
                    //CheckParent(node);
                    SetText();
                }
            }
            catch (Exception ex)
            {

            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

    }
}
