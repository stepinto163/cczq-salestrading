using STC.Common.Model.Admin;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// BookHierarchicalComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class BookHierarchicalComboBox : UserControl
    {
        private Timer _interval;

        private const string ALL = "全部";
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(BookHierarchicalComboBox), new PropertyMetadata(false, OnIsEditableChanged));

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BookHierarchicalComboBox control = d as BookHierarchicalComboBox;

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
            DependencyProperty.Register("DefaultText", typeof(string), typeof(BookHierarchicalComboBox), new PropertyMetadata("", OnDefaultChanged));

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                BookHierarchicalComboBox control = (BookHierarchicalComboBox)d;
                control.SelectedTexts = e.NewValue.ToString();
            }
        }

        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BookHierarchicalComboBox));

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
            DependencyProperty.Register("SelectedTexts", typeof(string), typeof(BookHierarchicalComboBox), new UIPropertyMetadata(""));

        /// <summary>
        /// 选择的文本对应id
        /// </summary>
        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedIdProperty =
            DependencyProperty.Register("SelectedId", typeof(string), typeof(BookHierarchicalComboBox), new UIPropertyMetadata(""));

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
         DependencyProperty.Register("SelectedValues", typeof(string), typeof(BookHierarchicalComboBox),
             new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedValuesChanged)));


        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }
            BookHierarchicalComboBox control = (BookHierarchicalComboBox)d;
            StringBuilder valuesb = new StringBuilder();
            int selectedItemsCount = 0;
            //control.iterateor(control._collection, null, valuesb, ref selectedItemsCount);
            if (e.NewValue.ToString() != valuesb.ToString())
            {
                string selectedValues = e.NewValue.ToString();
                string[] valuesArr = selectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
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
            DependencyProperty.Register("SelectedCount", typeof(int), typeof(BookHierarchicalComboBox), new PropertyMetadata(0, OnSelectedCountChanged));

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
        }

        /// <summary>
        /// 当前策略的用户
        /// </summary>
        public string QuantUser
        {
            get { return (string)GetValue(QuantUserProperty); }
            set { SetValue(QuantUserProperty, value); }
        }

        public static readonly DependencyProperty QuantUserProperty =
            DependencyProperty.Register("QuantUser", typeof(string), typeof(BookHierarchicalComboBox), new UIPropertyMetadata(""));

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


        public BookHierarchicalComboBox()
        {
            InitializeComponent();
        }

        void _interval_Elapsed(object sender, ElapsedEventArgs e)
        {
            // pause the timer
            this._interval.Stop();

            // show the drop down when user starts typing then stops
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                this.ViewSource.View?.Refresh();

                if (this.SelectedId == null || string.IsNullOrEmpty(SelectedId))
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

        public void InitData(ObservableCollection<TBookBookTreeItemModel> bookList)
        {
            if (this.OrgTree != null)
            {
                this.OrgTree.ItemsSource = bookList;
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    ExpandInternal(this.OrgTree, false);
                });
            }
        }

        private void orgTree_Loaded(object sender, RoutedEventArgs e)
        {
            //ExpandInternal(this.orgTree);
        }

        private void BuildTree(TBookBookTreeItemModel root, IList<ChildTag> orgs)
        {
            if (orgs != null && orgs.Count > 0)
            {
                foreach (ChildTag item in orgs)
                {
                    TBookBookTreeItemModel model = new TBookBookTreeItemModel();
                    model.Name = item.childTagName;
                    model.Id = item.childTag;
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
                //root.IsLeaf = true;
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
        private void ExpandToSelectedBranch(TreeViewItem selectedItem)
        {
            TreeViewItem parent = selectedItem.Parent as TreeViewItem;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent as TreeViewItem;
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
                    }
                    else
                    {

                    }
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
                this.SelectedCount = _selectedOrgs.Count;//count;
                this.SelectedTexts = _selectedOrgs[0].Name;//sb.ToString();//string.Format("共{0}条:{1}", count, sb);
                this.SelectedValues = valueSb.ToString();
                this.SelectedId = _selectedOrgs[0].Id;
            }
            else
            {
                this.SelectedCount = 0;
                this.SelectedTexts = DefaultText;
                this.SelectedValues = string.Empty;
                this.SelectedId = string.Empty;
            }

            int _selectedCount = 0;
            if (_selectedCount != 0 && _selectedCount == _collection.Count)
            {
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
                CheckBox chk = this.innerCCB.Template.FindName("allChk", innerCCB) as CheckBox;
                if (chk != null)
                {
                    chk.IsChecked = false;
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

        private void right_part_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void MultiSelectCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
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

        private void Arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (!string.IsNullOrEmpty(SelectedId)) 
            //{
            //    this.Dispatcher.BeginInvoke((Action)delegate
            //    {
            //        ExpandSelectedInternal(this.OrgTree, true);
            //    });
            //}
        }
        //private void ExpandSelectedInternal(System.Windows.Controls.ItemsControl targetItemContainer, bool isExpand)
        //{
        //    try
        //    {
        //        foreach (object item in targetItemContainer.Items)
        //        {
        //            if (item is TBookBookTreeItemModel treeViewItem) 
        //            {
        //                if (treeViewItem.Id.Equals(SelectedId))
        //                {
        //                    treeViewItem.IsExpanded = true;
        //                    if (treeViewItem.Parent != null)
        //                    {
        //                        ExpandedViewItem(treeViewItem.Parent);
        //                    }
        //                }
        //                else 
        //                {
        //                    if (treeViewItem.Nodes != null && treeViewItem.Nodes.Count > 0) 
        //                    {
        //                        FindViewItem(treeViewItem);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        //private void FindViewItem(TBookBookTreeItemModel model)
        //{
        //    foreach (TBookBookTreeItemModel item in model.Nodes)
        //    {
        //        if (item.Id.Equals(SelectedId))
        //        {
        //            item.IsExpanded = true;
        //            if (item.Parent != null)
        //            {
        //                ExpandedViewItem(item.Parent);
        //            }
        //        }
        //        else 
        //        {
        //            if (item.Nodes != null && item.Nodes.Count > 0)
        //            {
        //                FindViewItem(item);
        //            }
        //        }
        //    }
        //}
        //private void ExpandedViewItem(TBookBookTreeItemModel parent)
        //{
        //    parent.IsExpanded = true;
        //    if (parent.Parent != null) 
        //    {
        //        ExpandedViewItem(parent.Parent);
        //    }
        //}
    }
}
