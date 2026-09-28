using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    /// MultiSelectTreeViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class MultiSelectTreeViewControl : UserControl
    {
        #region 是否允许选择根节点
        /// <summary>
        /// 是否允许选择根节点, 当允许时 CheckBox 显示模板 <Button  Margin="0, 0, 4, 0" VerticalAlignment="Center" ToolTip="{Binding Checked, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Style="{DynamicResource TreeCheckBoxButton}" Background="Transparent" Tag="{Binding Id}"/>
        /// 当不允许选择跟节点时，CheckBox 显示模板为 <Button  Margin="0, 0, 4, 0" VerticalAlignment="Center" ToolTip="{Binding Checked, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Style="{DynamicResource TreeCheckBoxButton}" Background="Transparent" Tag="{Binding Id}" Visibility="{Binding IsLeaf, Converter={StaticResource visibilityConvertor}}"/>
        /// </summary>
        public bool IsEnableSelectRootNode
        {
            get { return (bool)GetValue(IsEnableSelectRootNodeProperty); }
            set { SetValue(IsEnableSelectRootNodeProperty, value); }
        }


        public static readonly DependencyProperty IsEnableSelectRootNodeProperty = DependencyProperty.Register("IsEnableSelectRootNode", typeof(bool), typeof(MultiSelectTreeViewControl), new PropertyMetadata(false, OnIsEnableSelectRootNode_Changed));


        private static void OnIsEnableSelectRootNode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeViewControl treeCmb = d as MultiSelectTreeViewControl;
            if (treeCmb != null)
            {
                treeCmb.IsEnableSelectRootNode = (bool)e.NewValue;
            }
        }
        #endregion

        #region 是否允许多选

        /// <summary>
        /// 是否允许多选,多选时，不自动关闭Popup
        /// </summary>
        public bool IsEnableMultiSelect
        {
            get { return (bool)GetValue(IsEnableMultiSelectProperty); }
            set { SetValue(IsEnableMultiSelectProperty, value); }
        }

        public static readonly DependencyProperty IsEnableMultiSelectProperty = DependencyProperty.Register("IsEnableMultiSelect", typeof(bool), typeof(MultiSelectTreeViewControl), new PropertyMetadata(true));

        #endregion

        #region 多选文本
        /// <summary>
        /// 选择的文本列表 ,分割
        /// </summary>
        public string SelectedTexts
        {
            get { return (string)GetValue(SelectedTextsProperty); }
            set { SetValue(SelectedTextsProperty, value); }
        }

        public static readonly DependencyProperty SelectedTextsProperty = DependencyProperty.Register("SelectedTexts", typeof(string), typeof(MultiSelectTreeViewControl), new UIPropertyMetadata(""));


        /// <summary>
        /// 选择的文本对应id
        /// </summary>
        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedIdProperty = DependencyProperty.Register("SelectedId", typeof(string), typeof(MultiSelectTreeViewControl), new UIPropertyMetadata(""));
        #endregion

        #region 显示内容
        /// <summary>
        /// 显示内容 默认为对象Name
        /// </summary>
        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }
        public static readonly DependencyProperty DisplayPathProperty = DependencyProperty.Register("DisplayPath", typeof(string), typeof(MultiSelectTreeViewControl), new UIPropertyMetadata("Name"));

        #endregion

        #region 显示代码
        /// <summary>
        /// 显示代码 默认为对象的Id
        /// </summary>
        public string DisplayCode
        {
            get { return (string)GetValue(DisplayCodeProperty); }
            set { SetValue(DisplayCodeProperty, value); }
        }
        public static readonly DependencyProperty DisplayCodeProperty = DependencyProperty.Register("DisplayCode", typeof(string), typeof(MultiSelectTreeViewControl), new UIPropertyMetadata("Id"));

        #endregion

        #region SelectedItem 默认选择Id列表，如果多选 以,隔开
        /// <summary>
        /// 默认选择Id列表，如果多选 以,隔开
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(MultiSelectTreeViewControl), new PropertyMetadata(null));

        #endregion


        #region 声明选中项发生改变事件
        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(name: "SelectedItemChanged", routingStrategy: RoutingStrategy.Bubble, handlerType: typeof(RoutedEventHandler), ownerType: typeof(MultiSelectTreeViewControl));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }
        #endregion

        #region 数据源
        private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        /// <summary>
        /// 数据源
        /// </summary>
        public ObservableCollection<TreeNodeModel> TreeComboxItemsSource
        {
            get { return _TreeComboxItemsSource; }
            set
            {
                if (_TreeComboxItemsSource != value)
                {
                    _TreeComboxItemsSource = value;
                }
            }
        }
        #endregion


        public MultiSelectTreeViewControl()
        {
            InitializeComponent();
            Uid = Guid.NewGuid().ToString();

            this.Loaded += OnThis_Loaded;
        }

        TreeView DropDownTreeViewer = null;

        private bool isLoad = false;

        #region Load时候获取TreeView控件并设置数据
        private void OnThis_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoad)
            {
                if (DropDownTreeViewer == null)
                {
                    DropDownTreeViewer = InnerTreeView;
                    if (DropDownTreeViewer != null)
                    {
                        DropDownTreeViewer.Uid = this.Uid;

                        if (DropDownTreeViewer.ItemTemplate != this.TreeItemItemTemplate)
                        {
                            DropDownTreeViewer.ItemTemplate = this.TreeItemItemTemplate;
                        }
                        if (DropDownTreeViewer.ItemsSource != this.TreeItemsSource)
                        {
                            DropDownTreeViewer.Items.Clear();
                            TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>((this.TreeItemsSource as ObservableCollection<TreeNodeModel>).ToList());
                            DropDownTreeViewer.ItemsSource = TreeComboxItemsSource;
                        }
                        if (DropDownTreeViewer.ItemContainerStyle != this.TreeItemStyle)
                        {
                            DropDownTreeViewer.ItemContainerStyle = this.TreeItemStyle;
                        }
                    }
                }

                if (this.SelectedItem != null)
                {
                    if (!string.IsNullOrEmpty((SelectedItem as string)))
                    {
                        List<string> lstids = (SelectedItem as string).Split(',').ToList();
                        for (int i = 0; i < lstids.Count; i++)
                        {
                            SetCheckStatus(lstids[i]);
                        }
                    }
                    else
                    {
                        SetDisplayText(null);
                    }

                    isLoad = true;
                    RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
                }
                else if (this.SelectedItem == null)
                {
                    SetDisplayText(null);
                    RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
                    isLoad = true;
                }
            }
        }
        #endregion


        private void InnerTreeView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //判断是否是自己的实例
            if (sender.GetType() == typeof(TreeView))
            {
                if ((sender as TreeView).Uid != this.Uid)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            //判断是否是点击了"Expander"
            if (e.OriginalSource is ToggleButton)
            {
                return;
            }

            TreeViewItem tree = FindVisualParent<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (tree == null)
                return;

            //判断选中项类别
            if (tree.Header.GetType() == typeof(TreeNodeModel))
            {
                TreeNodeModel dr = tree.Header as TreeNodeModel;

                if (dr.Nodes.Count == 0)
                {
                    SetCheckStatus(dr.Id);
                    //如果允许选择组，则反向选择父级节点
                    if (IsEnableSelectRootNode)
                    {
                        UpdateParentNodeStatus(dr.Id, dr.Checked);
                    }
                }
                else
                {
                    //如果允许选择组，则自动选择下面所有选项
                    if (IsEnableSelectRootNode)
                    {
                        SetGroupCheckStatus(dr, dr.Id, dr.Checked == "0" ? "1" : "0");
                        UpdateParentNodeStatus(dr.Id, dr.Checked);
                    }
                }

                RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
            }
        }

        private string UpdateParentNodeStatus(string id, string status)
        {
            TreeNodeModel currentNode = FindTreeNodeById(id);
            if (currentNode != null)
            {
                TreeNodeModel parentNode = FindTreeNodeById(currentNode.Parentnode);
                if (parentNode != null)
                {
                    if (parentNode.Nodes.Where(p => p.Checked == status).ToList().Count == parentNode.Nodes.Count)
                    {
                        parentNode.Checked = status;
                        SetGroupCheckStatus(parentNode, parentNode.Id, parentNode.Checked = status == "0" ? "0" : "1");
                    }
                    else
                    {
                        parentNode.Checked = status == "1" ? "0" : "0";
                    }

                    UpdateParentNodeStatus(parentNode.Id, status);
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// 如果选择组，则自动选择或取消选择下面所有选项
        /// </summary>
        /// <param name="tnParent"></param>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private TreeNodeModel SetGroupCheckStatus(TreeNodeModel tnParent, string id, string status)
        {
            if (tnParent == null) return null;
            if (tnParent.Id == id)
            {
                //判断是否有 Checked ，如果有则设置选中状态 "0" 未选择 "1" 选择, 如果不允许多选则有用户自己设置
                PropertyInfo propertyChecked = tnParent.GetType().GetProperty("Checked", BindingFlags.Instance | BindingFlags.Public);

                if ((object)propertyChecked != null)
                {
                    propertyChecked.SetValue(tnParent, status);
                }

                if (tnParent.Nodes.Count == 0)
                {
                    SetGroupDisplayText(tnParent, status);
                }
            }

            TreeNodeModel tnRet = null;
            foreach (TreeNodeModel tn in tnParent.Nodes)
            {
                tnRet = SetGroupCheckStatus(tn, tn.Id, status);
                if (tnRet != null) break;
            }

            return tnRet;
        }

        /// <summary>
        /// 设置组全选或取消全选时更新选择内容
        /// </summary>
        /// <param name="item"></param>
        /// <param name="status"></param>
        public void SetGroupDisplayText(object item, string status)
        {
            if (item == null)
            {
                SelectedTexts = "";
                SelectedId = "";
                return;
            }

            if (!IsEnableMultiSelect)
            {
                SelectedTexts = "";
                SelectedId = "";
            }

            //设置显示内容 Name
            if (!string.IsNullOrEmpty(this.DisplayPath))
            {
                string[] propName = this.DisplayPath.Split('-');
                if (propName.Length > 0)
                {
                    object tmpVAlue = item;
                    for (int i = 0; i < propName.Length; i++)
                    {
                        PropertyInfo property = tmpVAlue.GetType().GetProperty(propName[i], BindingFlags.Instance | BindingFlags.Public);
                        if (property != null)
                        {
                            tmpVAlue = property.GetValue(tmpVAlue);
                        }

                        //判断是否有 Checked ，如果有则设置选中状态 "0" 未选择 "1" 选择, 如果不允许多选则有用户自己设置
                        PropertyInfo propertyChecked = item.GetType().GetProperty("Checked", BindingFlags.Instance | BindingFlags.Public);


                        if (tmpVAlue == null || i == propName.Length - 1)
                        {
                            string value = (tmpVAlue ?? "NaN??").ToString();
                            if (status == "1")
                            {
                                if (!SelectedTexts.Split(',').Contains(value))
                                {
                                    List<string> oldlist = SelectedTexts.Split(',').ToList();
                                    oldlist.Add(value);
                                    SelectedTexts = string.Join(",", oldlist.ToArray()).TrimStart(',');

                                    if ((object)propertyChecked != null)
                                    {
                                        propertyChecked.SetValue(item, "1");
                                    }
                                }
                            }
                            else
                            {
                                List<string> oldlist = SelectedTexts.Split(',').ToList();
                                oldlist.Remove(value);
                                SelectedTexts = string.Join(",", oldlist.ToArray()).TrimStart(',');
                                if ((object)propertyChecked != null)
                                {
                                    propertyChecked.SetValue(item, "0");
                                }
                            }
                        }
                    }
                }
            }

            //设置代码Id 
            if (!string.IsNullOrEmpty(this.DisplayCode))
            {
                string[] propName = this.DisplayCode.Split('.');
                if (propName.Length > 0)
                {
                    object tmpVAlue = item;
                    for (int i = 0; i < propName.Length; i++)
                    {
                        PropertyInfo property = tmpVAlue.GetType().GetProperty(propName[i], BindingFlags.Instance | BindingFlags.Public);
                        if (property != null)
                        {
                            tmpVAlue = property.GetValue(tmpVAlue);
                        }
                        if (tmpVAlue == null || i == propName.Length - 1)
                        {
                            string value = (tmpVAlue ?? "NaN??").ToString();
                            if (status == "1")
                            {
                                if (!SelectedId.Split(',').Contains(value))
                                {
                                    List<string> oldlist = SelectedId.Split(',').ToList();
                                    oldlist.Add(value);
                                    SelectedId = string.Join(",", oldlist.ToArray()).TrimStart(',');
                                }
                            }
                            else
                            {
                                List<string> oldlist = SelectedId.Split(',').ToList();
                                oldlist.Remove(value);
                                SelectedId = string.Join(",", oldlist.ToArray()).TrimStart(',');
                            }

                            if (this.isLoad)
                            {
                                this.SelectedItem = SelectedId;
                            }

                            return;
                        }
                    }
                }
            }
        }


        #region 返回根据节点Id找到选择的第一个节点
        /// <summary>
        /// 根据节点Id找到选择的第一个节点信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TreeNodeModel FindTreeNodeById(string id)
        {
            TreeNodeModel dr = null;
            foreach (TreeNodeModel pbd in TreeComboxItemsSource)
            {
                dr = FindNode(pbd, id);
                if (dr != null)
                    break;
            }

            return dr;
        }

        private TreeNodeModel FindNode(TreeNodeModel tnParent, string id)
        {
            if (tnParent == null) return null;
            if (tnParent.Id == id)
            {
                return tnParent;
            }

            TreeNodeModel tnRet = null;
            foreach (TreeNodeModel tn in tnParent.Nodes)
            {
                tnRet = FindNode(tn, id);
                if (tnRet != null) break;
            }

            return tnRet;
        }
        #endregion


        /// <summary>
        /// 利用VisualTreeHelper寻找指定依赖对象的父级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }


        #region 设置  SelectedTexts，SelectedId，SelectedItem
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="item"></param>
        public void SetDisplayText(object item)
        {
            if (item == null)
            {
                SelectedTexts = "";
                SelectedId = "";
                //this.ComboBox.Items.Clear();
                ////this.ComboBox.Text = "";
                return;
            }

            if (!IsEnableMultiSelect)
            {
                SelectedTexts = "";
                SelectedId = "";
            }

            //设置显示内容 Name
            if (!string.IsNullOrEmpty(this.DisplayPath))
            {
                string[] propName = this.DisplayPath.Split('-');
                if (propName.Length > 0)
                {
                    object tmpVAlue = item;
                    for (int i = 0; i < propName.Length; i++)
                    {
                        PropertyInfo property = tmpVAlue.GetType().GetProperty(propName[i], BindingFlags.Instance | BindingFlags.Public);
                        if (property != null)
                        {
                            tmpVAlue = property.GetValue(tmpVAlue);
                        }

                        //判断是否有 Checked ，如果有则设置选中状态 "0" 未选择 "1" 选择, 如果不允许多选则有用户自己设置
                        PropertyInfo propertyChecked = item.GetType().GetProperty("Checked", BindingFlags.Instance | BindingFlags.Public);


                        if (tmpVAlue == null || i == propName.Length - 1)
                        {
                            string value = (tmpVAlue ?? "NaN??").ToString();
                            if (!SelectedTexts.Split(',').Contains(value))
                            {
                                List<string> oldlist = SelectedTexts.Split(',').ToList();
                                oldlist.Add(value);
                                SelectedTexts = string.Join(",", oldlist.ToArray()).TrimStart(',');

                                if ((object)propertyChecked != null)
                                {
                                    propertyChecked.SetValue(item, "1");
                                }
                            }
                            else
                            {
                                List<string> oldlist = SelectedTexts.Split(',').ToList();
                                oldlist.Remove(value);
                                SelectedTexts = string.Join(",", oldlist.ToArray()).TrimStart(',');
                                if ((object)propertyChecked != null)
                                {
                                    propertyChecked.SetValue(item, "0");
                                }
                            }
                        }
                    }
                }
            }

            //设置代码Id 
            if (!string.IsNullOrEmpty(this.DisplayCode))
            {
                string[] propName = this.DisplayCode.Split('.');
                if (propName.Length > 0)
                {
                    object tmpVAlue = item;
                    for (int i = 0; i < propName.Length; i++)
                    {
                        PropertyInfo property = tmpVAlue.GetType().GetProperty(propName[i], BindingFlags.Instance | BindingFlags.Public);
                        if (property != null)
                        {
                            tmpVAlue = property.GetValue(tmpVAlue);
                        }
                        if (tmpVAlue == null || i == propName.Length - 1)
                        {
                            string value = (tmpVAlue ?? "NaN??").ToString();
                            if (!SelectedId.Split(',').Contains(value))
                            {
                                List<string> oldlist = SelectedId.Split(',').ToList();
                                oldlist.Add(value);
                                SelectedId = string.Join(",", oldlist.ToArray()).TrimStart(',');
                            }
                            else
                            {
                                List<string> oldlist = SelectedId.Split(',').ToList();
                                oldlist.Remove(value);
                                SelectedId = string.Join(",", oldlist.ToArray()).TrimStart(',');
                            }

                            if (this.isLoad)
                            {
                                this.SelectedItem = SelectedId;
                            }

                            return;
                        }
                    }
                }
            }

            //SelectedTexts = value;
            //this.ComboBox.Items.Clear();
            //this.ComboBox.Items.Add(this.SelectedItem.ToString());
            //this.ComboBox.Text = this.ComboBox.Items[0].ToString();
        }
        #endregion

        #region 设置节点状态
        public void SetAllCheckStatusToDefault()
        {
            foreach (TreeNodeModel pbd in TreeComboxItemsSource)
            {
                ReplaceNode(pbd, pbd.Id, true);
            }
        }

        /// <summary>
        /// 修改树中节点的状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isSetDefault">是否设置为空</param>
        public void SetCheckStatus(string id, bool isSetDefault = false)
        {
            foreach (TreeNodeModel pbd in TreeComboxItemsSource)
            {
                ReplaceNode(pbd, id, isSetDefault);
            }
        }

        private TreeNodeModel ReplaceNode(TreeNodeModel tnParent, string id, bool isSetDefault)
        {
            if (tnParent == null) return null;
            if (tnParent.Id == id)
            {
                //判断是否有 Checked ，如果有则设置选中状态 "0" 未选择 "1" 选择, 如果不允许多选则有用户自己设置
                PropertyInfo propertyChecked = tnParent.GetType().GetProperty("Checked", BindingFlags.Instance | BindingFlags.Public);
                if (isSetDefault)
                {
                    if ((object)propertyChecked != null)
                    {
                        propertyChecked.SetValue(tnParent, "0");
                    }
                }
                else
                {
                    SetDisplayText(tnParent);
                }
            }

            TreeNodeModel tnRet = null;
            foreach (TreeNodeModel tn in tnParent.Nodes)
            {
                if (isSetDefault)
                    tnRet = ReplaceNode(tn, tn.Id, isSetDefault);
                else
                    tnRet = ReplaceNode(tn, id, isSetDefault);

                if (tnRet != null) break;
            }

            return tnRet;
        }
        #endregion


        #region ItemsSource
        /// <summary>
        /// 数据源
        /// </summary>
        public IEnumerable TreeItemsSource
        {
            get { return (IEnumerable)GetValue(TreeItemsSourceProperty); }
            set { SetValue(TreeItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty TreeItemsSourceProperty = DependencyProperty.Register("TreeItemsSource", typeof(IEnumerable), typeof(MultiSelectTreeViewControl), new PropertyMetadata(null, OnTreeItemsSource_Changed));

        private static void OnTreeItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeViewControl treeComboBox = d as MultiSelectTreeViewControl;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                if (e.NewValue != null)
                {
                    treeComboBox.TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>((e.NewValue as ObservableCollection<TreeNodeModel>).ToList());
                    treeComboBox.DropDownTreeViewer.ItemsSource = treeComboBox.TreeComboxItemsSource;
                }
            }
        }
        #endregion

        #region TreeItemStyle
        public Style TreeItemStyle
        {
            get { return (Style)GetValue(TreeItemStyleProperty); }
            set { SetValue(TreeItemStyleProperty, value); }
        }
        public static readonly DependencyProperty TreeItemStyleProperty = DependencyProperty.Register("TreeItemStyle", typeof(Style), typeof(MultiSelectTreeViewControl), new PropertyMetadata(null, OnTreeItemStyle_Changed));

        private static void OnTreeItemStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeViewControl treeCombo = d as MultiSelectTreeViewControl;
            if (treeCombo != null && treeCombo.DropDownTreeViewer != null)
            {
                treeCombo.DropDownTreeViewer.ItemContainerStyle = e.NewValue as Style;
            }
        }
        #endregion

        #region 树子项显示模板
        public HierarchicalDataTemplate TreeItemItemTemplate
        {
            get { return (HierarchicalDataTemplate)GetValue(TreeItemItemTemplateProperty); }
            set { SetValue(TreeItemItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty TreeItemItemTemplateProperty = DependencyProperty.Register("TreeItemItemTemplate", typeof(HierarchicalDataTemplate), typeof(MultiSelectTreeViewControl), new PropertyMetadata(null, OnTreeItemItemTemplate_Changed));

        private static void OnTreeItemItemTemplate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeViewControl treeComboBox = d as MultiSelectTreeViewControl;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                treeComboBox.DropDownTreeViewer.ItemTemplate = e.NewValue as HierarchicalDataTemplate;
            }
        }

        #endregion

        #region 响应鼠标滚轮事件
        private void DropDownScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 找到父级的 ScrollViewer 并进行滚动
            ScrollViewer scrollViewer = FindParent<ScrollViewer>(sender as DependencyObject);
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineUp();
                }
                else
                {
                    scrollViewer.LineDown();
                }

                e.Handled = true;
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
        #endregion

    }
}
