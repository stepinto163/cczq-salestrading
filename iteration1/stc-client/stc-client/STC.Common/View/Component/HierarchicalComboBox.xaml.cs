using STC.Common.Model.Table;
using STC.Common.Utils;
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

namespace STC.Common.View.Component
{
    /// <summary>
    /// HierarchicalComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class HierarchicalComboBox : UserControl
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
        //    DependencyProperty.Register("ItemsSource", typeof(List<ChildTag>), typeof(HierarchicalComboBox), new PropertyMetadata(null, OnItemsSourceChanged));

        //private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.NewValue != null)
        //    {
        //        (d as HierarchicalComboBox).InitData(e.NewValue as List<ChildTag>);
        //    }
        //}

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(HierarchicalComboBox), new PropertyMetadata(false, OnIsEditableChanged));

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HierarchicalComboBox control = d as HierarchicalComboBox;

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
            DependencyProperty.Register("DefaultText", typeof(string), typeof(HierarchicalComboBox), new PropertyMetadata("All", OnDefaultChanged));

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                HierarchicalComboBox control = (HierarchicalComboBox)d;
                control.SelectedTexts = e.NewValue.ToString();
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HierarchicalComboBox));

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
            DependencyProperty.Register("SelectedTexts", typeof(string), typeof(HierarchicalComboBox), new UIPropertyMetadata("All"));


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
         DependencyProperty.Register("SelectedValues", typeof(string), typeof(HierarchicalComboBox),
             new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuesChanged)));


        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            HierarchicalComboBox control = (HierarchicalComboBox)d;
            StringBuilder valuesb = new StringBuilder();
            int selectedItemsCount = 0;
            control.iterateor(control._collection, null, valuesb, ref selectedItemsCount);
            if (e.NewValue.ToString() != valuesb.ToString())
            {
                string selectedValues = e.NewValue.ToString();
                string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (HierarchicalTreeItemModel item in control._selectedOrgs)
                {
                    item.IsChecked = false;
                }
                control._selectedOrgs.Clear();

                if (valuesArr != null && valuesArr.Length > 0 && control._collection != null && control._collection.Count > 0)
                {
                    control.SetValue_Iterateor(control._collection, valuesArr);
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
            DependencyProperty.Register("SelectedCount", typeof(int), typeof(HierarchicalComboBox), new PropertyMetadata(0, OnSelectedCountChanged));

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            HierarchicalComboBox control = (HierarchicalComboBox)d;
            if (control != null)
            {
                int count = e.NewValue.ConvertToInt();
                if (count > 0)
                {
                    control.count_text.Visibility = Visibility.Visible;
                }
                else
                {
                    control.count_text.Visibility = Visibility.Collapsed;
                }
            }
        }




        public bool IsAll
        {
            get
            {
                int selectedItemsCount = 0;
                iterateor(_collection, null, null, ref selectedItemsCount);
                int rootCount = _collection.Count(item => item.IsChecked == true);
                return selectedItemsCount == 0; //|| rootCount == _collection.Count;
            }
        }

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


        public HierarchicalComboBox()
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
        //private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(HierarchicalComboBox));
        private ObservableCollection<HierarchicalTreeItemModel> _collection = new ObservableCollection<HierarchicalTreeItemModel>();
        private ObservableCollection<HierarchicalTreeItemModel> _selectedOrgs = new ObservableCollection<HierarchicalTreeItemModel>();
        private bool _inited = false;

        public IList<HierarchicalTreeItemModel> SelectedOrgs
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

        public void InitData(IList<ChildTag> list)
        {
            //var list = SP.Get<Cache.ICacheService>().OrgCache.GetCameraOrgs();
            //var info = list.FirstOrDefault(p => p.PAR_ID == 0);

            //HierarchicalTreeItemModel root = new HierarchicalTreeItemModel();
            //root.Info = new ChildTag() { childTagName = ALL, childTagValue = ALL };
            //root.Name = ALL;

            //_collection.Add(root);
            foreach (ChildTag item in list)
            {
                if (item.childTagName == "无" || item.childTagName == "其他")
                    continue;
                HierarchicalTreeItemModel model = new HierarchicalTreeItemModel();
                model.Info = item;
                model.Name = item.childTagName;
                model.PinYin = item.pinyin;
                _collection.Add(model);
                BuildTree(model, item.childTags);
                if (item.childTags == null || item.childTags.Count == 0)
                {
                    model.IsLeaf = true;
                }
            }

            this.ViewSource.Source = _collection;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.View.Filter = x =>
            {
                if (this.SearchTextBox != null)
                {
                    HierarchicalTreeItemModel data = x as HierarchicalTreeItemModel;

                    bool flag = false;
                    if (data.Nodes != null && data.Nodes.Count > 0)
                    {
                        foreach (HierarchicalTreeItemModel sub_data in data.Nodes)
                        {
                            if (sub_data.Name.Contains(this.SearchTextBox.Text) || (!string.IsNullOrEmpty(sub_data.PinYin) && sub_data.PinYin.Contains(this.SearchTextBox.Text)))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(this.SearchTextBox.Text))
                    {
                        return data.Name.Contains(this.SearchTextBox.Text) || (!string.IsNullOrEmpty(data.PinYin) && data.PinYin.Contains(this.SearchTextBox.Text)) || flag;
                    }
                }

                return true;
            };
        }

        private void orgTree_Loaded(object sender, RoutedEventArgs e)
        {
            //ExpandInternal(this.orgTree);
        }

        private void BuildTree(HierarchicalTreeItemModel root, IList<ChildTag> orgs)
        {
            if (orgs != null && orgs.Count > 0)
            {
                foreach (ChildTag item in orgs)
                {
                    HierarchicalTreeItemModel model = new HierarchicalTreeItemModel();
                    model.Info = item;
                    model.Name = item.childTagName;
                    model.PinYin = item.pinyin;
                    model.IsChecked = false;
                    model.Parent = root;
                    if (root != null)
                    {
                        root.Nodes.Add(model);
                    }
                    BuildTree(model, item.childTags);
                }
            }
            else
            {
                root.IsLeaf = true;
            }
        }

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
                foreach (Object item in targetItemContainer.Items)
                {
                    System.Windows.Controls.TreeViewItem treeItem = targetItemContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    //var info = item as TreeNode;
                    if (treeItem == null || !treeItem.HasItems)
                    {
                        continue;
                    }
                    //if (info.Info == null)
                    //{
                    //    treeItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ad7fa"));
                    //}
                    treeItem.IsExpanded = isExpand;
                    //ExpandInternal(treeItem as ItemsControl);
                }
            }
            catch (Exception ex)
            {
                //_log.ErrorFormat("ExpandInternal error", ex);
            }
        }

        private void cxb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            try
            {
                foreach (HierarchicalTreeItemModel subNode in this.ViewSource.View)
                {
                    if (subNode != null && cbx.IsChecked != null)
                    {
                        if (cbx.IsChecked.Value)
                        {
                            subNode.IsChecked = true;
                            if (!_selectedOrgs.Contains(subNode))
                            {
                                _selectedOrgs.Add(subNode);
                                CheckChild(subNode, true);
                            }
                        }
                        else
                        {
                            subNode.IsChecked = false;
                            if (_selectedOrgs.Contains(subNode))
                            {
                                _selectedOrgs.Remove(subNode);
                                CheckChild(subNode, false);
                            }
                        }
                        CheckParent(subNode);

                    }
                }
                SetText();
            }
            catch (Exception ex)
            {

            }
        }

        private void cxb_Node_UnChecked(object sender, RoutedEventArgs e) { }

        private void cxb_Node_Checked(object sender, RoutedEventArgs e) { }

        private void cxb_Node_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleButton cbx = sender as ToggleButton;
                var node = cbx.DataContext as HierarchicalTreeItemModel;

                if (node != null && cbx.IsChecked != null)
                {
                    if (cbx.IsChecked.Value)
                    {
                        if (!_selectedOrgs.Contains(node))
                        {
                            _selectedOrgs.Add(node);
                            CheckChild(node, true);
                        }
                    }
                    else
                    {
                        if (_selectedOrgs.Contains(node))
                        {
                            _selectedOrgs.Remove(node);
                            CheckChild(node, false);
                        }
                    }
                    CheckParent(node);
                }

               

                //ToggleButton.Tag = string.Join(",", _selectedOrgs.ToList().ConvertAll(a => a.Info.childTagName));
                SetText();
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckParent(HierarchicalTreeItemModel node)
        {
            if (node.Parent != null)
            {
                bool isCheck = true;
                foreach (HierarchicalTreeItemModel item in node.Parent.Nodes)
                {
                    if (!item.IsChecked)
                    {
                        isCheck = false;
                    }
                }
                if (isCheck)
                {
                    node.Parent.IsChecked = true;
                    if (!_selectedOrgs.Contains(node.Parent))
                    {
                        _selectedOrgs.Insert(0, node.Parent);
                    }
                }
                else
                {
                    node.Parent.IsChecked = false;
                    if (_selectedOrgs.Contains(node.Parent))
                    {
                        _selectedOrgs.Remove(node.Parent);
                    }
                }
                //ToggleButton.Tag = string.Join(",", _selectedOrgs.ToList().ConvertAll(a => a.Info.childTagName));
                if (node.Parent.Parent != null)
                {
                    CheckParent(node.Parent);
                }
            }
        }

        private void CheckChild(HierarchicalTreeItemModel node, bool isCheck)
        {
            if (node.Nodes.Count > 0)
            {
                if (isCheck)
                {
                    foreach (HierarchicalTreeItemModel item in node.Nodes)
                    {
                        item.IsChecked = true;
                        if (!_selectedOrgs.Contains(item))
                        {
                            _selectedOrgs.Add(item);
                        }
                        CheckChild(item, true);
                    }
                }
                else
                {
                    foreach (HierarchicalTreeItemModel item in node.Nodes)
                    {
                        item.IsChecked = false;
                        if (_selectedOrgs.Contains(item))
                        {
                            _selectedOrgs.Remove(item);
                        }
                        CheckChild(item, false);
                    }
                }
            }
        }

        public void ClearSelection()
        {
            SetValue_Iterateor(_collection, null);
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

        public void Select(List<string> orgIdList, ObservableCollection<HierarchicalTreeItemModel> Nodes = null)
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
                        foreach (HierarchicalTreeItemModel item in _collection[0].Nodes)
                        {
                            if (orgIdList.Exists(a => a == item.Info.childTagValue.ToString()))
                            {
                                item.IsChecked = true;
                                if (!_selectedOrgs.Contains(item))
                                {
                                    _selectedOrgs.Add(item);
                                }
                            }
                            else
                            {
                                item.IsChecked = false;
                                if (_selectedOrgs.Contains(item))
                                {
                                    _selectedOrgs.Remove(item);
                                }
                            }
                            Select(orgIdList, item.Nodes);
                        }
                    }
                    else
                    {
                        foreach (HierarchicalTreeItemModel item in Nodes)
                        {
                            if (orgIdList.Exists(a => a == item.Info.childTagValue.ToString()))
                            {
                                item.IsChecked = true;
                                if (!_selectedOrgs.Contains(item))
                                {
                                    _selectedOrgs.Add(item);
                                }
                            }
                            else
                            {
                                item.IsChecked = false;
                                if (_selectedOrgs.Contains(item))
                                {
                                    _selectedOrgs.Remove(item);
                                }
                            }
                            Select(orgIdList, item.Nodes);
                        }


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
                int count = 0;
                StringBuilder sb = new StringBuilder();
                StringBuilder valueSb = new StringBuilder();
                iterateor(_collection, sb, valueSb, ref count);
                this.SelectedCount = count;
                this.SelectedTexts = sb.ToString();//string.Format("共{0}条:{1}", count, sb);
                this.SelectedValues = valueSb.ToString();
            }
            else
            {
                this.SelectedCount = 0;
                this.SelectedTexts = DefaultText;
                this.SelectedValues = string.Empty;
            }

            int _selectedCount = 0;
            foreach (HierarchicalTreeItemModel s in _collection)
            {
                if (s.IsChecked && s.Name != ALL)
                    _selectedCount++;
            }
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

        private void iterateor(ObservableCollection<HierarchicalTreeItemModel> collection, StringBuilder sb, StringBuilder valueSb, ref int count)
        {
            foreach (HierarchicalTreeItemModel item in collection)
            {

                if (item.IsLeaf)
                {
                    if (item.IsChecked)
                    {
                        count++;
                        if (sb != null)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(item.Name);
                        }

                        if (valueSb != null)
                        {
                            if (valueSb.Length > 0)
                            {
                                valueSb.Append(",");
                            }
                            valueSb.Append(item.Info.childTagValue);
                        }
                    }
                }
                else
                {
                    iterateor(item.Nodes, sb, valueSb, ref count);
                }
            }
        }

        private void SetValue_Iterateor(ObservableCollection<HierarchicalTreeItemModel> collection, string[] valuesArr)
        {
            foreach (HierarchicalTreeItemModel item in collection)
            {
                if (item.Name == ALL)
                {
                    item.IsChecked = valuesArr == null ? false : valuesArr.Any(entry => entry == item.Info.childTagValue);
                }

                if (item.IsLeaf)
                {
                    item.IsChecked = valuesArr == null ? false : valuesArr.Any(entry => entry == item.Info.childTagValue);
                    CheckParent(item);
                }
                else
                {
                    SetValue_Iterateor(item.Nodes, valuesArr);
                }
            }
        }

        private void innerCCB_Loaded(object sender, RoutedEventArgs e)
        {
           
            _inited = true;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void count_text_DropDownOpened(object sender, EventArgs e)
        {
            this.count_text.ItemsSource = _selectedOrgs.Where(item => item.IsLeaf == true);
        }

        private void right_part_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (count_text.Visibility == Visibility.Collapsed)
            {
                this.count_text.IsDropDownOpen = false;
            }
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
    }
}
