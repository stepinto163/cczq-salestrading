using Newtonsoft.Json;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Common.View.Component
{
    /// <summary>
    /// MultiTreeComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class MultiTreeComboBox : UserControl
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


        public static readonly DependencyProperty IsEnableSelectRootNodeProperty = DependencyProperty.Register("IsEnableSelectRootNode", typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false, OnIsEnableSelectRootNode_Changed));


        private static void OnIsEnableSelectRootNode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiTreeComboBox treeCmb = d as MultiTreeComboBox;
            if (treeCmb != null)
            {
                treeCmb.IsEnableSelectRootNode = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// 是否允许搜索，如果允许，则显示搜索框
        /// </summary>
        public bool IsEnableSearch
        {
            get { return (bool)GetValue(IsEnableSearchProperty); }
            set { SetValue(IsEnableSearchProperty, value); }
        }

        public static readonly DependencyProperty IsEnableSearchProperty = DependencyProperty.Register("IsEnableSearch", typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false));

        /// <summary>
        /// 是否允许使用 IsEnabled
        /// </summary>
        public bool IsCanUse
        {
            get { return (bool)GetValue(IsCanUseProperty); }
            set { SetValue(IsCanUseProperty, value); }
        }


        public static readonly DependencyProperty IsCanUseProperty = DependencyProperty.Register("IsCanUse", typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(true, OnIsCanUse_Changed));


        private static void OnIsCanUse_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiTreeComboBox treeCmb = d as MultiTreeComboBox;
            if (treeCmb != null)
            {
                treeCmb.ComboBox.IsEnabled = (bool)e.NewValue;
                //treeCmb.IsEnabled = (bool)e.NewValue;
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

        public static readonly DependencyProperty IsEnableMultiSelectProperty = DependencyProperty.Register("IsEnableMultiSelect", typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(true));

        #endregion

        #region 默认显示文本 
        /// <summary>
        /// ALL 或 不限，用户未选择时，显示文本
        /// </summary>
        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata(""));


        #endregion

        #region  每页显示父节点数/最大下拉的高度
        /// <summary>
        /// 每页显示父节点数，用于分页
        /// </summary>
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(int), typeof(MultiTreeComboBox), new PropertyMetadata(10));


        /// <summary>
        /// 最大下拉的高度，默认300,当页面高度足够时可以修改
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(
                nameof(MaxDropDownHeight),
                typeof(double),
                typeof(MultiTreeComboBox),
                new PropertyMetadata(300.0)); // 默认 300
        #endregion

        #region 多选文本
        /// <summary>
        /// 选择的子节点列表 ,分割
        /// </summary>
        public string SelectedTexts
        {
            get { return (string)GetValue(SelectedTextsProperty); }
            set { SetValue(SelectedTextsProperty, value); }
        }

        public static readonly DependencyProperty SelectedTextsProperty = DependencyProperty.Register("SelectedTexts", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata(""));


        /// <summary>
        /// 选择的子节点id ,分割
        /// </summary>
        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedIdProperty = DependencyProperty.Register("SelectedId", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata(""));


        /// <summary>
        /// 选择的组合名称列表
        /// </summary>
        public List<string> SelectedParentTexts
        {
            get { return (List<string>)GetValue(SelectedParentTextsProperty); }
            set { SetValue(SelectedParentTextsProperty, value); }
        }

        public static readonly DependencyProperty SelectedParentTextsProperty = DependencyProperty.Register("SelectedParentTexts", typeof(List<string>), typeof(MultiTreeComboBox), new UIPropertyMetadata(new List<string>()));

        /// <summary>
        /// 选择的组合ID列表
        /// </summary>
        public List<string> SelectedParentId
        {
            get { return (List<string>)GetValue(SelectedParentIdProperty); }
            set { SetValue(SelectedParentIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedParentIdProperty = DependencyProperty.Register("SelectedParentId", typeof(List<string>), typeof(MultiTreeComboBox), new UIPropertyMetadata(new List<string>()));


        /// <summary>
        /// 选择的组和列表
        /// </summary>
        public Dictionary<string, Dictionary<string,object>> SelectedParentItems
        {
            get { return (Dictionary<string, Dictionary<string, object>>)GetValue(SelectedParentItemsProperty); }
            set { SetValue(SelectedParentItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedParentItemsProperty = DependencyProperty.Register("SelectedParentItems", typeof(Dictionary<string, Dictionary<string, object>>), typeof(MultiTreeComboBox), new UIPropertyMetadata(new Dictionary<string, Dictionary<string, object>>()));

        /// <summary>
        /// 选择的组和基金账号
        /// </summary>
        public List<string> SelectedGroupProduct
        {
            get { return (List<string>)GetValue(SelectedGroupProductProperty); }
            set { SetValue(SelectedGroupProductProperty, value); }
        }

        public static readonly DependencyProperty SelectedGroupProductProperty = DependencyProperty.Register("SelectedGroupProduct", typeof(List<string>), typeof(MultiTreeComboBox), new UIPropertyMetadata(new List<string>()));

        #endregion

        #region 显示内容
        /// <summary>
        /// 显示内容 默认是选择的子节点名称  SelectedText[资金账号] 可选 SelectedParentTexts[组合名称], SelectedGroupProduct [组合名称1(恒生编号1),...,组合名称n(恒生编号n)], PA[组合1，...,组合n(恒生编号1,...,恒生编号n)]
        /// </summary>
        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }

        public static readonly DependencyProperty DisplayPathProperty = DependencyProperty.Register("DisplayPath", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata("SelectedTexts"));

        #endregion

        #region 显示代码
        /// <summary>
        /// 已废弃
        /// </summary>
        public string DisplayCode
        {
            get { return (string)GetValue(DisplayCodeProperty); }
            set { SetValue(DisplayCodeProperty, value); }
        }
        public static readonly DependencyProperty DisplayCodeProperty = DependencyProperty.Register("DisplayCode", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata("Id"));


        /// <summary>
        /// 暴露搜索文本属性，供外部使用
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(MultiTreeComboBox), new UIPropertyMetadata(""));

        #endregion

        #region Tag

        #endregion

        public MultiTreeComboBox()
        {
            InitializeComponent();
            Uid = Guid.NewGuid().ToString();

            this.Loaded += OnThis_Loaded;

            //注册类级别事件处理程序
            //EventManager.RegisterClassHandler(typeof(MultiTreeComboBox), TreeView.SelectedItemChangedEvent, new RoutedPropertyChangedEventHandler<object>(OnTreeView_SelectedItemChangedEvent));

            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnTreeViewItem_MouseUP));

            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnTreeViewItem_MouseDown));

            EventManager.RegisterClassHandler(typeof(MultiTreeComboBox), TreeView.PreviewMouseUpEvent, new MouseButtonEventHandler(OnTreeViewItem_PreviewMouseUp));

            this.ComboBox.DropDownOpened += OnComboBox_DropDownOpened;
        }

        #region OnTreeViewItem_PreviewMouseUp事件
        private void OnTreeViewItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //判断是否是自己的实例
            if (sender.GetType() == typeof(MultiTreeComboBox))
            {
                if ((sender as MultiTreeComboBox).Uid != this.Uid)
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
                
                //选择的子节点
                if (dr.Nodes.Count == 0)
                {
                    if (!string.IsNullOrEmpty(dr.Parentnode))
                    {
                        //单选
                        if (!IsEnableMultiSelect)
                        {
                            string oldChecked = dr.Checked;
                            SelectedId = "";
                            SelectedTexts = "";
                            SelectedItem = "";
                            SelectedParentId = new List<string>();
                            SelectedProductIds = new List<string>();
                            SelectedProductId = "";
                            SelectedParentTexts = new List<string>();
                            SelectedParentItems = new Dictionary<string, Dictionary<string, object>>();
                            SelectedGroupProduct = new List<string>();
                            SetAllCheckStatusToDefault();
                            if (oldChecked == "0")
                            {
                                dr.Checked = "1";
                                SetParentNodeStatus(dr);
                            }
                            else
                            {
                                oldChecked = "0";
                            }
                        }
                        else
                        {
                            SetCheckStatus(dr.Id);
                            SetGroupCheckStatus(dr, dr.Id, dr.Checked == "0" ? "1" : "0");
                        }

                        //如果允许选择组，则反向选择父级节点
                        if (IsEnableSelectRootNode)
                        {
                            UpdateParentNodeStatus(dr.Id, dr.Checked);
                        }
                    }
                    else
                    {
                        //如果用户选择了组
                        if (IsEnableSelectRootNode)
                        {
                            //单选并且不加载子节点
                            if (!IsEnableMultiSelect)
                            {
                                SelectedId = "";
                                SelectedTexts = "";
                                SelectedItem = "";
                                SelectedParentId = new List<string>();
                                SelectedProductIds = new List<string>();
                                SelectedProductId = "";
                                SelectedParentTexts = new List<string>();
                                SelectedParentItems = new Dictionary<string, Dictionary<string, object>>();
                                SelectedGroupProduct = new List<string>();
                                SetAllCheckStatusToDefault();
                            }

                            SetGroupCheckStatus(dr, dr.Id, dr.Checked == "0" ? "1" : "0");
                            UpdateParentNodeStatus(dr.Id, dr.Checked);
                        }
                        else
                        {
                            //如果单选并且不允许选择Root,并且是用户选择了组
                            if (dr.IsGroup && dr.IsLeaf)
                            {
                                string oldChecked = dr.Checked;
                                SelectedId = "";
                                SelectedTexts = "";
                                SelectedItem = "";
                                SelectedParentId = new List<string>();
                                SelectedProductIds = new List<string>();
                                SelectedProductId = "";
                                SelectedParentTexts = new List<string>();
                                SelectedParentItems = new Dictionary<string, Dictionary<string, object>>();
                                SelectedGroupProduct = new List<string>();
                                SetAllCheckStatusToDefault();
                                if (oldChecked == "0")
                                {
                                    dr.Checked = "1";
                                    SetParentNodeStatus(dr);
                                }
                                else
                                {
                                    oldChecked = "0";
                                }
                            }
                        }
                    }
                   
                }
                else
                {
                    //如果允许选择组，则自动选择下面所有选项
                    if (IsEnableSelectRootNode)
                    {
                        //单选并且不加载子节点
                        if (!IsEnableMultiSelect)
                        {
                            SelectedId = "";
                            SelectedTexts = "";
                            SelectedItem = "";
                            SelectedParentId = new List<string>();
                            SelectedProductIds = new List<string>();
                            SelectedProductId = "";
                            SelectedParentTexts = new List<string>();
                            SelectedParentItems = new Dictionary<string, Dictionary<string, object>>();
                            SelectedGroupProduct = new List<string>();
                            SetAllCheckStatusToDefault();
                        }

                        SetGroupCheckStatus(dr, dr.Id, dr.Checked == "0" ? "1" : "0");
                        UpdateParentNodeStatus(dr.Id, dr.Checked);
                            
                    }
                }
            }

            if (!IsEnableMultiSelect)
            {
                if (this.ComboBox.IsDropDownOpen == true)
                {
                    this.ComboBox.IsDropDownOpen = false;
                }
            }
            //Dictionary<string, object>
            List<TreeNodeModel> templst = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x => x.Nodes?.Count > 0 || x.Nodes != null).ToList();
            foreach (var key in SelectedParentItems.Keys.ToList())
            {
                TreeNodeModel tnm = SelectedParentItems[key].Values.FirstOrDefault() as TreeNodeModel;
                if (tnm != null)
                {
                    if (tnm.Nodes.Count > 0)
                    {
                        int s = tnm.Nodes.Where(o => o.Checked == "0").ToList().Count;
                        if (tnm.Nodes.Count == s)
                        {
                            SelectedParentItems.Remove(key);
                        }
                    }
                    else
                    {
                        if (tnm.Checked == "0")
                        {
                            SelectedParentItems.Remove(key);
                        }
                    }
                }
            }

            List<string> groupidlist = SelectedParentItems.Keys.ToList();
            List<string> groupnamelist = SelectedParentItems.Values.SelectMany(x=>x.Keys).ToList(); 
            List<string> groupgroupidlist = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x=>x.Nodes?.Count == 0 || x.Nodes == null ).Select(x=>x.Id).ToList();

            List<TreeNodeModel> selgrouphavenodeslist = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x => x.Nodes?.Count > 0 || x.Nodes != null).ToList();

            List<string> checkedIds = selgrouphavenodeslist.Where(x => x.Nodes != null).SelectMany(x => x.Nodes).Where(n => n.Checked == "1").Select(n => n.Id).ToList();
            List<string> checkedtexts = selgrouphavenodeslist.Where(x => x.Nodes != null).SelectMany(x => x.Nodes).Where(n => n.Checked == "1").Select(n => n.Name).ToList();

            List<string> newresult = checkedIds.Except(groupidlist).ToList();
            SelectedId = string.Join(",", newresult.ToArray()).TrimStart(',').TrimEnd(',');

            List<string> newtextresult = checkedtexts.Except(groupnamelist).ToList();
            SelectedTexts = string.Join(",", newtextresult.ToArray()).TrimStart(',').TrimEnd(',');

            SelectedItem = string.Join(",", newresult.Union(groupgroupidlist).ToList()).TrimStart(',').TrimEnd(',');

            SelectedProductIds = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x =>!string.IsNullOrEmpty(x.ProductId)).Select(x => x.ProductId).ToList();
            SelectedProductId = string.Join(",", SelectedProductIds.ToArray()).TrimStart(',').TrimEnd(',');

            SelectedGroupProduct = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Select(x=> $"{x.Name}({x.ProductId})").ToList();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
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


        #region 返回根据节点Id找到选择的第一个节点
        /// <summary>
        /// 根据节点Id找到选择的第一个节点信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TreeNodeModel FindTreeNodeById(string id)
        {
            TreeNodeModel dr = null;
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

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

                    SetParentNodeStatus(tnParent);
                    //SetGroupDisplayText(tnParent, status);
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
        /// 设置父节点值
        /// </summary>
        /// <param name="tnm"></param>
        private void SetParentNodeStatus(TreeNodeModel tnm)
        {
            if (tnm == null)
                return;

            List<string> oldlist = SelectedParentId.ToList();
            List<string> prelist = SelectedParentTexts.ToList();
            //List<string> proidlist = SelectedProductIds.ToList();
            Dictionary<string, Dictionary<string, object>> dict = SelectedParentItems.ToDictionary(o => o.Key, o => o.Value);

            //如果有父级节点，则查找父级节点设置返回值
            if (!string.IsNullOrWhiteSpace(tnm.Parentnode??""))
            {
                TreeNodeModel parentNode = FindTreeNodeById(tnm.Parentnode);
                if (parentNode != null)
                {
                    if (parentNode.Nodes.Where(p => p.Checked == "1").ToList().Count > 0)
                    {
                        if (!oldlist.Contains(parentNode.Id))
                        {
                            oldlist.Add(parentNode.Id);
                            prelist.Add(parentNode.Name);

                            Dictionary<string, object> sudict = new Dictionary<string, object>();
                            sudict.Add(parentNode.Name, parentNode);
                            dict.Add(parentNode.Id, sudict);
                        }
                        //if (!proidlist.Contains(parentNode.ProductId))
                        //{
                        //    proidlist.Add(parentNode.ProductId);
                        //}
                    }
                    else
                    {
                        if (oldlist.Contains(parentNode.Id))
                        {
                            oldlist.Remove(parentNode.Id);
                            prelist.Remove(parentNode.Name);
                            dict.Remove(parentNode.Id);
                        }
                        //if (proidlist.Contains(parentNode.ProductId))
                        //{
                        //    proidlist.Remove(parentNode.ProductId);
                        //}
                    }
                }
            }
            else
            {
                //如果没有父级节点，则认为是组合，无需向父节点遍历
                if (tnm.Checked == "1") 
                {
                    if (!oldlist.Contains(tnm.Id))
                    {
                        oldlist.Add(tnm.Id);
                        prelist.Add(tnm.Name);

                        Dictionary<string, object> sudict = new Dictionary<string, object>();
                        sudict.Add(tnm.Name, tnm);

                        dict.Add(tnm.Id, sudict);
                    }
                    //if (!proidlist.Contains(tnm.ProductId))
                    //{
                    //    proidlist.Add(tnm.ProductId);
                    //}
                }
                else
                {
                    if (oldlist.Contains(tnm.Id))
                    {
                        oldlist.Remove(tnm.Id);
                        prelist.Remove(tnm.Name);
                        dict.Remove(tnm.Id);
                    }
                    //if (proidlist.Contains(tnm.ProductId))
                    //{
                    //    proidlist.Remove(tnm.ProductId);
                    //}
                }
            }

            SelectedParentId = new List<string>(oldlist);
            SelectedParentTexts = new List<string>(prelist);
            //SelectedProductIds = new List<string>(proidlist);
            //SelectedProductId = string.Join(",", SelectedProductIds);
            SelectedParentItems = new Dictionary<string, Dictionary<string, object>>(dict);
        }


        private void OnTreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null)
            {
                object commandParameter = TreeViewItemClickCommandParameter;
                if (commandParameter == null)
                    commandParameter = treeViewItem.DataContext;

                TreeView treeParent = FindVisualParent<TreeView>(treeViewItem);
                if (treeParent != null && treeParent == this.DropDownTreeViewer)
                {
                    //判断是否是自己的实例
                    if (treeParent.Uid != this.Uid)
                        return;

                    RoutedCommand command = TreeViewItemClickCommand as RoutedCommand;
                    if (command != null)
                        command.Execute(commandParameter, TreeViewItemClickTarget);

                    else if (TreeViewItemClickCommand != null)
                        this.TreeViewItemClickCommand.Execute(commandParameter);

                    //不允许选择根节点，用于当没有触发 selectedchange时，修改选择
                    if (!IsEnableSelectRootNode)
                    {
                        Type Ts = commandParameter.GetType();
                        object property = Ts.GetProperty("IsLeaf", BindingFlags.Instance | BindingFlags.Public)?.GetValue(commandParameter, null);
                        if (property != null)
                        {
                            bool IsLeaf = (bool)property;
                            if (!IsLeaf)
                            {
                                return;
                            }
                        }

                        // SetDisplayText(commandParameter);
                    }
                }
            }
        }

        #endregion

        #region TreeItem的MouseUP事件
        private void OnTreeViewItem_MouseUP(object sender, MouseButtonEventArgs e)
        {
            //判断是否是自己的实例
            TreeViewItem treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null)
            {
                TreeView treeParent = FindVisualParent<TreeView>(treeViewItem);
                if (treeParent != null && treeParent == this.DropDownTreeViewer)
                {
                    if (treeParent.Uid != this.Uid)
                        return;
                }
                else
                {
                    return;
                }
            }

            if (!IsEnableMultiSelect)
            {
                if (this.ComboBox.IsDropDownOpen == true)
                {
                    this.ComboBox.IsDropDownOpen = false;
                }
            }
        }

        /// <summary>
        /// 利用VisualTreeHelper寻找指定依赖对象的父级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            try
            {

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
            catch { }

            return null;
        }
        #endregion

        #region TreeCombobox下拉时的事件
        private void OnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (DropDownTreeViewer != null && string.IsNullOrEmpty(this.SelectedItem as string))
            {
                SetAllCheckStatusToDefault();
            }

            if (DropDownTreeViewer != null && this.SelectedItem != null)
            {
                //if (!IsEnableSelectRootNode && !IsLoadLeafNodes)
                if (!IsEnableMultiSelect && IsLoadLeafNodes)
                {
                    //单选
                    if (!string.IsNullOrEmpty(this.SelectedItem as string))
                    {
                        SetAllCheckStatusToDefault();
                        TreeNodeModel parentNode = FindTreeNodeById(this.SelectedItem as string);
                        if (parentNode != null)
                        {
                            parentNode.Checked = "1";
                            SetParentNodeStatus(parentNode);
                        }
                    }
                }
                else
                {
                    //多选 或者 单选并且不加载子节点
                    if ((SelectedItem as string) != this.SelectedId)
                    {
                        //设置已选择的状态
                        List<string> lstids = (SelectedItem as string).Split(',').ToList();
                        for (int i = 0; i < lstids.Count; i++)
                        {
                            SetCheckStatus(lstids[i]);
                        }
                    }
                }
            }
        }

        #endregion

        #region 设置节点状态
        public void SetAllCheckStatusToDefault(bool isChecked = true)
        {
            foreach (TreeNodeModel pbd in TreeComboxItemsSource)
            {
                ReplaceNode(pbd, pbd.Id, isChecked);
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


        #region TreeView选中项发生改变
        public void SetDisplayText(object item)
        {
            SetParentNodeStatus(item as TreeNodeModel);
        }
        #endregion



        #region 选中项



        public object SelectedItem2
        {
            get { return (object)GetValue(SelectedItem2Property); }
            set { SetValue(SelectedItem2Property, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItem2Property =
            DependencyProperty.Register("SelectedItem2", typeof(object), typeof(MultiTreeComboBox), new PropertyMetadata("", OnSelectedItem));

        private static void OnSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiTreeComboBox source = d as MultiTreeComboBox;
            if (source != null && e.NewValue != null)
            {
                if ((e.NewValue != null && source.SelectedItem == null)
                   || (e.NewValue == null && source.SelectedItem != null)
                    || (e.NewValue.ToString() != source.SelectedItem.ToString()))
                {
                    source.SetSelection(e.NewValue.ToString());
                }
            }
        }


        /// <summary>
        /// 默认恒生基金编号，对应于ProductId，如果多选 以,隔开
        /// </summary>
        public object SelectedProductId
        {
            get { return (object)GetValue(SelectedProductIdProperty); }
            set { SetValue(SelectedProductIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedProductIdProperty = DependencyProperty.Register("SelectedProductId", typeof(object), typeof(MultiTreeComboBox), new PropertyMetadata(""));


        /// <summary>
        /// 选择的父文本对应id
        /// </summary>
        public List<string> SelectedProductIds
        {
            get { return (List<string>)GetValue(SelectedProductIdsProperty); }
            set { SetValue(SelectedProductIdsProperty, value); }
        }

        public static readonly DependencyProperty SelectedProductIdsProperty = DependencyProperty.Register("SelectedProductIds", typeof(List<string>), typeof(MultiTreeComboBox), new UIPropertyMetadata(new List<string>()));

        /// <summary>
        /// 默认选择 子节点Id列表，如果没有子节点则返回组合的id
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(MultiTreeComboBox), new PropertyMetadata(""));
        #endregion


        #region 设置选中项目
        public void SetTreeViewSelectedItem(object selectedValue)
        {
            if (selectedValue == null) return;
            if (this.DropDownTreeViewer != null)
            {
                if (this.DropDownTreeViewer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    SelectedTheTreeViewItem(selectedValue);
                }
                else
                {
                    EventHandler[] eventHandler = new EventHandler[1];
                    eventHandler[0] = new EventHandler((o1, e1) =>
                    {
                        if (this.DropDownTreeViewer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                        {
                            this.DropDownTreeViewer.ItemContainerGenerator.StatusChanged -= eventHandler[0];
                            SelectedTheTreeViewItem(selectedValue);
                        }
                        ;
                    });
                    this.DropDownTreeViewer.ItemContainerGenerator.StatusChanged += eventHandler[0];
                }
            }
        }
        private void SelectedTheTreeViewItem(object selectedValue)
        {
            for (int i = 0; i < this.DropDownTreeViewer.ItemContainerGenerator.Items.Count; i++)
            {
                TreeViewItem treeViewItem = this.DropDownTreeViewer.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (treeViewItem != null)
                {
                    object item = this.DropDownTreeViewer.ItemContainerGenerator.ItemFromContainer(treeViewItem);
                    if (item == selectedValue)
                    {
                        treeViewItem.IsSelected = true;
                        TunThreadSetIsOpen();
                        return;
                    }
                    else
                    {
                        SetTreeViewItemSelected(treeViewItem, selectedValue);
                    }
                }
            }
        }
        private void TunThreadSetIsOpen()
        {
            //这个事件实在下拉时候发生的，因为选中了之后会触发下拉框自动回收，这就很操蛋，只能延迟后重新打开
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(100);
                this.Dispatcher.Invoke(() =>
                {
                    if (this.ComboBox.IsDropDownOpen == false)
                    {
                        this.ComboBox.IsDropDownOpen = true;
                    }
                });
            }).Start();
        }
        private void SetTreeViewItemSelected(TreeViewItem treeViewItem, object selectedValue)
        {
            if (treeViewItem == null || selectedValue == null) return;
            if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                for (int i = 0; i < treeViewItem.ItemContainerGenerator.Items.Count; i++)
                {
                    TreeViewItem childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                    if (childTreeViewItem != null)
                    {
                        object item = treeViewItem.ItemContainerGenerator.ItemFromContainer(childTreeViewItem);
                        if (item == selectedValue)
                        {
                            childTreeViewItem.IsSelected = true;
                            TunThreadSetIsOpen();
                            return;
                        }
                        else
                        {
                            SetTreeViewItemSelected(childTreeViewItem, selectedValue);
                        }
                    }
                }
            }
            else
            {
                EventHandler[] eventHandler = new EventHandler[1];
                eventHandler[0] = new EventHandler((o1, e1) =>
                {
                    if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        treeViewItem.ItemContainerGenerator.StatusChanged -= eventHandler[0];
                        SetTreeViewItemSelected(treeViewItem, selectedValue);
                    }
                    ;
                });
                treeViewItem.ItemContainerGenerator.StatusChanged += eventHandler[0];
            }
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
        public static readonly DependencyProperty TreeItemsSourceProperty = DependencyProperty.Register("TreeItemsSource", typeof(IEnumerable), typeof(MultiTreeComboBox), new PropertyMetadata(null, OnTreeItemsSource_Changed));

        private static void OnTreeItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiTreeComboBox treeComboBox = d as MultiTreeComboBox;
            ObservableCollection<TreeNodeModel> list = new ObservableCollection<TreeNodeModel>((e.NewValue as ObservableCollection<TreeNodeModel>).ToList());

            if (treeComboBox.TreeComboxItemsSource != list)
            {
                treeComboBox.TreeComboxItemsSource = list;
                // 启动等待逻辑
                treeComboBox.WaitForTreeData(treeComboBox.TreeComboxItemsSource);
            }
        }
        #endregion

        #region TreeItemStyle
        /// <summary>
        /// 已废弃
        /// </summary>
        public Style TreeItemStyle
        {
            get { return (Style)GetValue(TreeItemStyleProperty); }
            set { SetValue(TreeItemStyleProperty, value); }
        }
        public static readonly DependencyProperty TreeItemStyleProperty = DependencyProperty.Register("TreeItemStyle", typeof(Style), typeof(MultiTreeComboBox), new PropertyMetadata(null, OnTreeItemStyle_Changed));

        private static void OnTreeItemStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ////MultiTreeComboBox treeCombo = d as MultiTreeComboBox;
            ////if (treeCombo != null && treeCombo.DropDownTreeViewer != null)
            ////{
            ////    treeCombo.DropDownTreeViewer.ItemContainerStyle = e.NewValue as Style;
            ////}
        }
        #endregion

        #region 树子项显示模板
        public HierarchicalDataTemplate TreeItemItemTemplate
        {
            get { return (HierarchicalDataTemplate)GetValue(TreeItemItemTemplateProperty); }
            set { SetValue(TreeItemItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty TreeItemItemTemplateProperty = DependencyProperty.Register("TreeItemItemTemplate", typeof(HierarchicalDataTemplate), typeof(MultiTreeComboBox), new PropertyMetadata(null, OnTreeItemItemTemplate_Changed));

        private static void OnTreeItemItemTemplate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiTreeComboBox treeComboBox = d as MultiTreeComboBox;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                treeComboBox.DropDownTreeViewer.ItemTemplate = e.NewValue as HierarchicalDataTemplate;
            }
        }

        #endregion

        TreeView DropDownTreeViewer = null;

        private bool isLoad = false;

        /// <summary>
        /// 是否加载完整的树，true完整加载 false只加载跟节点
        /// </summary>
        public bool IsLoadLeafNodes = true;

        #region Load时候获取TreeView控件并设置数据
        private async void OnThis_Loaded(object sender, RoutedEventArgs e)
        {
            if (DropDownTreeViewer == null)
            {

                DropDownTreeViewer = await GetDropDownTreeViewAsync(this.ComboBox);

                if (DropDownTreeViewer != null)
                {
                    DropDownTreeViewer.Uid = this.Uid;

                    if (DropDownTreeViewer.ItemTemplate != this.TreeItemItemTemplate)
                    {
                        DropDownTreeViewer.ItemTemplate = this.TreeItemItemTemplate;
                    }

                    var binding = (DropDownTreeViewer.ItemTemplate as HierarchicalDataTemplate)?.ItemsSource as Binding;
                    if (binding != null)
                    {
                        string path = binding.Path?.Path; // 获取绑定路径
                        if (path?.ToLower() == "null")
                        {
                            IsLoadLeafNodes = false;
                            PageSize = 20;
                        }
                    }

                    //if (DropDownTreeViewer.ItemsSource != this.TreeItemsSource)
                    //{
                        //DropDownTreeViewer.Items.Clear();
                        //TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>((this.TreeItemsSource as ObservableCollection<TreeNodeModel>).ToList());
                        //DropDownTreeViewer.ItemsSource = PagedNodes;
                    //}
                    //if (DropDownTreeViewer.ItemContainerStyle != this.TreeItemStyle)
                    //{
                    //    DropDownTreeViewer.ItemContainerStyle = this.TreeItemStyle;
                    //}
                }
            }

            // 启动等待逻辑
            WaitForTreeData(TreeComboxItemsSource);
        }

        private void treeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            combo.ApplyTemplate(); // 确保模板已经应用

            // 第一步：找到 toggleButton
            var toggleButton = combo.Template.FindName("toggleButton", combo) as ToggleButton;
            if (toggleButton == null)
            {
                return;
            }

            toggleButton.ApplyTemplate();

            // 第二步：在 toggleButton 的模板中查找 txText
            var txText = toggleButton.Template.FindName("txText", toggleButton) as TextBlock;
            if (txText != null)
            {
                //SelectedTexts
                if (!string.IsNullOrEmpty(this.DisplayPath) && this.DisplayPath != "SelectedTexts" && this.DisplayPath!="PA")
                {
                    var newBinding = new Binding
                    {
                        ElementName = "treeComboBox",
                        Path = new PropertyPath(DisplayPath),
                        Converter = (IValueConverter)combo.TryFindResource("ComboBoxDisplayConverter"),
                        ConverterParameter = "Full",
                        Mode = BindingMode.OneWay
                    };

                    // 应用新的绑定
                    txText.SetBinding(TextBlock.TextProperty, newBinding);
                    txText.SetBinding(TextBlock.ToolTipProperty, newBinding);
                }

                //平安养老定制 (组名+恒生在账号) 建议单选时使用，多选时显示格式 group1,group(hs1,hs2)
                if (!string.IsNullOrEmpty(this.DisplayPath) && this.DisplayPath != "SelectedTexts" && this.DisplayPath == "PA")
                {
                    // <TextBlock x: Name = "txText" IsEnabled = "{TemplateBinding IsEnabled}" Foreground = "{DynamicResource ComboBoxItem.MouseOver.Border}"   ToolTip = "{Binding ElementName=treeComboBox, Path=SelectedParentTexts, Converter={StaticResource ComboBoxDisplayConverter}, ConverterParameter=Trimmed}" Padding = "3" TextTrimming = "CharacterEllipsis">
                    //    <Run Text = "{Binding ElementName=treeComboBox, Path=SelectedParentTexts, Converter={StaticResource ComboBoxDisplayConverter}, ConverterParameter=Full}" ></ Run >
                    //    <Run Text = "{Binding  ElementName=treeComboBox, Path=SelectedProductIds, Converter={StaticResource ComboBoxDisplayConverter}, ConverterParameter=PA}" Foreground = "White" />
                    //</TextBlock >

                    // ToolTip 绑定
                    var tooltipBinding = new Binding("SelectedParentTexts")
                    {
                        ElementName = "treeComboBox",
                        Converter = (IValueConverter)combo.TryFindResource("ComboBoxDisplayConverter"),
                        ConverterParameter = "Trimmed"
                    };
                    txText.SetBinding(TextBlock.ToolTipProperty, tooltipBinding);

                    // Run 1 绑定 SelectedParentTexts
                    var run1 = new Run();
                    var run1Binding = new Binding("SelectedParentTexts")
                    {
                        ElementName = "treeComboBox",
                        Converter = (IValueConverter)combo.TryFindResource("ComboBoxDisplayConverter"),
                        ConverterParameter = "Full"
                    };
                    run1.SetBinding(Run.TextProperty, run1Binding);

                    // Run 2 绑定 SelectedProductIds
                    Run run2 = new Run
                    {
                        Foreground = Brushes.White
                    };

                    var run2Binding = new Binding("SelectedProductIds")
                    {
                        ElementName = "treeComboBox",
                        Converter = (IValueConverter)combo.TryFindResource("ComboBoxDisplayConverter"),
                        ConverterParameter = "PA"
                    };
                    run2.SetBinding(Run.TextProperty, run2Binding);

                    // 添加子 Run
                    txText.Inlines.Add(run1);
                    txText.Inlines.Add(run2);
                }

                //如果只加载跟节点
                if (!IsLoadLeafNodes  && (!string.IsNullOrEmpty(this.DisplayPath) && this.DisplayPath != "SelectedTexts" && this.DisplayPath != "SelectedGroupProduct" && this.DisplayPath != "PA"))
                {
                    //Text="{Binding ElementName=treeComboBox, Path=SelectedParentTexts, Converter={StaticResource ComboBoxDisplayConverter}, ConverterParameter=Full}"
                    // 创建新绑定
                    var newBinding = new Binding
                    {
                        ElementName = "treeComboBox",
                        Path = new PropertyPath("SelectedParentTexts"),
                        Converter = (IValueConverter)combo.TryFindResource("ComboBoxDisplayConverter"),
                        ConverterParameter = "Full",
                        Mode = BindingMode.OneWay
                    };

                    // 应用新的绑定
                    txText.SetBinding(TextBlock.TextProperty, newBinding);
                    txText.SetBinding(TextBlock.ToolTipProperty, newBinding);
                }
            }
        }

        private void WaitForTreeData(ObservableCollection<TreeNodeModel> TreeComboxItemsSource, int timeoutMs = 5000)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);

            int waited = 0;

            timer.Tick += (s, e) =>
            {
                waited += 200;

                if (TreeComboxItemsSource.Count > 0)
                {
                    waited = 80000;
                    timer.Stop();
                    // 回到 UI 线程操作界面
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 在这里操作 UI，比如恢复控件颜色
                        CurrentPage = 1;
                        RefreshPage();

                        if (this.SelectedItem != null)
                        {
                            if (!IsEnableMultiSelect && IsLoadLeafNodes)
                            {
                                //单选
                                if (!string.IsNullOrEmpty(this.SelectedItem as string))
                                {
                                    SetAllCheckStatusToDefault();
                                    TreeNodeModel parentNode = FindTreeNodeById(this.SelectedItem as string);
                                    if (parentNode != null)
                                    {
                                        parentNode.Checked = "1";
                                        SetParentNodeStatus(parentNode);
                                    }
                                }
                            }
                            else
                            {
                                //多选 或者 单选并且不加载子节点
                                List<string> lstids = (SelectedItem as string).Split(',').ToList();
                                for (int i = 0; i < lstids.Count; i++)
                                {
                                    TreeNodeModel dr = FindTreeNodeById(lstids[i]);
                                    if (dr != null)
                                    {
                                        SetGroupCheckStatus(dr, dr.Id, dr.Checked == "0" ? "1" : "0");
                                        UpdateParentNodeStatus(dr.Id, dr.Checked);
                                    }
                                }
                            }

                            isLoad = true;
                        }
                        else if (this.SelectedItem == null)
                        {
                            isLoad = true;
                        }
                        
                        //增加筛选值
                        SetDefaultDisplayText();

                    }));

                }
                else if (waited >= timeoutMs)
                {
                    timer.Stop();
                    // 回到 UI 线程操作界面
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 超时处理（可选）
                        isLoad = true;
                    }));
                }
            };

            timer.Start();
        }

        private void SetDefaultDisplayText()
        {
            List<string> groupidlist = SelectedParentItems.Keys.ToList();
            List<string> groupnamelist = SelectedParentItems.Values.SelectMany(x => x.Keys).ToList();
            List<string> groupgroupidlist = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x => x.Nodes?.Count == 0 || x.Nodes == null).Select(x => x.Id).ToList();

            List<TreeNodeModel> selgrouphavenodeslist = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x => x.Nodes?.Count > 0 || x.Nodes != null).ToList();
            List<string> checkedIds = selgrouphavenodeslist.Where(x => x.Nodes != null).SelectMany(x => x.Nodes).Where(n => n.Checked == "1").Select(n => n.Id).ToList();
            List<string> checkedtexts = selgrouphavenodeslist.Where(x => x.Nodes != null).SelectMany(x => x.Nodes).Where(n => n.Checked == "1").Select(n => n.Name).ToList();

            List<string> newresult = checkedIds.Except(groupidlist).ToList();
            SelectedId = string.Join(",", newresult.ToArray()).TrimStart(',').TrimEnd(',');

            List<string> newtextresult = checkedtexts.Except(groupnamelist).ToList();
            SelectedTexts = string.Join(",", newtextresult.ToArray()).TrimStart(',').TrimEnd(',');

            SelectedItem = string.Join(",", newresult.Union(groupgroupidlist).ToList()).TrimStart(',').TrimEnd(',');

            SelectedProductIds = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Where(x => !string.IsNullOrEmpty(x.ProductId)).Select(x => x.ProductId).ToList();
            SelectedProductId = string.Join(",", SelectedProductIds.ToArray()).TrimStart(',').TrimEnd(',');

            SelectedGroupProduct = SelectedParentItems.Values.SelectMany(x => x.Values).ToList().Cast<TreeNodeModel>().Select(x => $"{x.Name}({x.ProductId})").ToList();
            RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
        }


        private async Task<TreeView> GetDropDownTreeViewAsync(ComboBox comboBox, int timeoutMs = 10000)
        {
            TreeView result = null;
            int waited = 0;

                while (result == null && waited < timeoutMs)
                {
                    result = comboBox.Template.FindName("InnerTreeView", comboBox) as TreeView;
                    if (result != null)
                        break;
                    await Task.Delay(500); // 等待 500ms
                    waited += 500;
                }
  
            return result;
        }

        private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        /// <summary>
        /// 真正的数据源
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

        #region 声明选中项发生改变事件
        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(name: "SelectedItemChanged", routingStrategy: RoutingStrategy.Bubble, handlerType: typeof(RoutedEventHandler), ownerType: typeof(MultiTreeComboBox));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }
        #endregion

        #region 点击命令
        /// <summary>
        /// 此命令会将
        /// </summary>
        public ICommand TreeViewItemClickCommand
        {
            get { return (ICommand)GetValue(TreeViewItemClickCommandProperty); }
            set { SetValue(TreeViewItemClickCommandProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickCommandProperty = DependencyProperty.Register("TreeViewItemClickCommand", typeof(ICommand), typeof(MultiTreeComboBox), new PropertyMetadata(null));

        public object TreeViewItemClickCommandParameter
        {
            get { return (object)GetValue(TreeViewItemClickCommandParameterProperty); }
            set { SetValue(TreeViewItemClickCommandParameterProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickCommandParameterProperty = DependencyProperty.Register("TreeViewItemClickCommandParameter", typeof(object), typeof(MultiTreeComboBox), new PropertyMetadata(null));

        public IInputElement TreeViewItemClickTarget
        {
            get { return (IInputElement)GetValue(TreeViewItemClickTargetProperty); }
            set { SetValue(TreeViewItemClickTargetProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickTargetProperty = DependencyProperty.Register("TreeViewItemClickTarget", typeof(IInputElement), typeof(MultiTreeComboBox), new PropertyMetadata(null));


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

        #region 删除所有选择
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearSelection();
            }
            catch
            {

            }
        }

        private TextBox _SearchTextBox;
        private TextBox SearchTextBox
        {
            get
            {
                if (_SearchTextBox == null)
                {
                    if (ComboBox.Template != null)
                    {
                        Popup p = ComboBox.Template.FindName("PART_Popup", ComboBox) as Popup;
                        if (p != null)
                        {
                            _SearchTextBox = p.FindName("txt_SearchTextBox") as TextBox;
                        }
                    }

                }
                return _SearchTextBox;
            }
        }

        /// <summary>
        /// 清空所有选择
        /// </summary>
        public void ClearSelection()
        {
            SetAllCheckStatusToDefault();
            SelectedParentItems = new Dictionary<string, Dictionary<string, object>>();
            SelectedTexts = "";
            SelectedId = "";
            SelectedItem = "";
            SelectedProductId = "";
            SelectedProductIds = new List<string>();
            SelectedParentTexts = new List<string>();
            SelectedParentId = new List<string>();
            SelectedGroupProduct = new List<string>();
            RaiseEvent(new RoutedEventArgs { RoutedEvent = SelectedItemChangedEvent });
        }

        /// <summary>
        /// 设置选中项
        /// </summary>
        /// <param name="selectedId"></param>
        public void SetSelection(string selectedId)
        {
            if (string.IsNullOrWhiteSpace(selectedId))
            {
                ClearSelection();
                return;
            }

            SelectedItem = selectedId;
            // 启动等待逻辑
            WaitForTreeData(TreeComboxItemsSource);
        }
        #endregion

        #region 筛选
        private void MultiSelectCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox != null)
            {
                SearchText = textbox.Text;
                FilterTree();
            }
        }

        /// <summary>
        /// 递归复制匹配的节点及其父节点
        /// </summary>
        private TreeNodeModel FilterNodeRecursive(TreeNodeModel node, string keyword)
        {
            bool isMatch = node.Name.Contains(keyword);

            var matchedChildren = new ObservableCollection<TreeNodeModel>();
            foreach (var child in node.Nodes)
            {
                var result = FilterNodeRecursive(child, keyword);
                if (result != null)
                    matchedChildren.Add(result);
            }

            if (isMatch || matchedChildren.Any())
            {
                string s = STC.Common.Utils.CommonUtils.SerializeObject(node);
                return STC.Common.Utils.CommonUtils.DeserializeObject<TreeNodeModel>(s);
            }

            return null;
        }


        private void cleanBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            DependencyObject parent = button;
            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var textBox = parent as TextBox;
            if (textBox != null)
            {
                textBox.Text = "";
            }
        }

        private void FilterTree()
        {
            var keyword = SearchText.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                _currentRootSource = TreeComboxItemsSource;
                CurrentPage = 1;
                RefreshPage();
                return;
            }
            else
            {
                var resultRoots = new List<TreeNodeModel>();
                foreach (TreeNodeModel root in TreeComboxItemsSource)
                {
                    if (IsLoadLeafNodes)
                    {
                        if (root.Name.Contains(keyword) || root.Id.Contains(keyword) || root.ProductId.Contains(keyword))
                        {
                            resultRoots.Add(root);
                        }
                        else
                        {
                            var matchedChildren = root.Nodes.Where(c => c.Name.Contains(keyword) || c.Id.Contains(keyword) || c.Parentnode.Contains(keyword) || c.ProductId.Contains(keyword)).ToList();
                            if (matchedChildren.Any())
                            {
                                var newRoot = new TreeNodeModel
                                {
                                    Id = root.Id,
                                    Name = root.Name,
                                    ProductId = root.ProductId,
                                    Nodes = new ObservableCollection<TreeNodeModel>(matchedChildren)
                                };

                                resultRoots.Add(newRoot);
                            }
                        }
                    }
                    else
                    {
                        if (root.Name.Contains(keyword) || root.Id.Contains(keyword) || root.ProductId.Contains(keyword))
                        {
                            resultRoots.Add(root);
                        }
                    }
                }

                CurrentPage = 1;
                _currentRootSource = resultRoots;
                RefreshPage();
            }
        }

        #endregion

        #region 分页

        #region 按钮可用性

        public bool CanGoFirst
        {
            get { return (bool)GetValue(CanGoFirstProperty);}
            set { SetValue(CanGoFirstProperty, value); }
        }

        public static readonly DependencyProperty CanGoFirstProperty = DependencyProperty.Register(nameof(CanGoFirst), typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false));

        public bool CanGoPrev
        {
            get { return (bool)GetValue(CanGoPrevProperty); }
            set { SetValue(CanGoPrevProperty, value); }
        }

        public static readonly DependencyProperty CanGoPrevProperty = DependencyProperty.Register(nameof(CanGoPrev), typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false));


        public bool CanGoNext
        {
            get { return (bool)GetValue(CanGoNextProperty); }
            set { SetValue(CanGoNextProperty, value); }
        }

        public static readonly DependencyProperty CanGoNextProperty = DependencyProperty.Register(nameof(CanGoNext), typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false));


        public bool CanGoLast
        {
            get { return (bool)GetValue(CanGoLastProperty); }
            set { SetValue(CanGoLastProperty, value); }
        }

        public static readonly DependencyProperty CanGoLastProperty = DependencyProperty.Register(nameof(CanGoLast), typeof(bool), typeof(MultiTreeComboBox), new PropertyMetadata(false));
        #endregion

        // CurrentPage 依赖属性
        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(MultiTreeComboBox), new PropertyMetadata(1));


        private IEnumerable<TreeNodeModel> _currentRootSource; // 当前数据源

        // PagedNodes 依赖属性
        public ObservableCollection<TreeNodeModel> PagedNodes
        {
            get { return (ObservableCollection<TreeNodeModel>)GetValue(PagedNodesProperty); }
            set { SetValue(PagedNodesProperty, value); }
        }
        public static readonly DependencyProperty PagedNodesProperty =
            DependencyProperty.Register(nameof(PagedNodes), typeof(ObservableCollection<TreeNodeModel>), typeof(MultiTreeComboBox), new PropertyMetadata(null));


        public int TotalPages
        {
            get { return (int)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesPropertyKey, value);}
        }
        private static readonly DependencyPropertyKey TotalPagesPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TotalPages), typeof(int), typeof(MultiTreeComboBox), new PropertyMetadata(0));
        public static readonly DependencyProperty TotalPagesProperty = TotalPagesPropertyKey.DependencyProperty;

        private void RefreshPage()
        {
            isRefreshPageFirst = true;
            var source = _currentRootSource ?? TreeComboxItemsSource;
            var total = source.Count();
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            var data = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            PagedNodes = new ObservableCollection<TreeNodeModel>(data);
            // 更新按钮状态
            CanGoFirst = CurrentPage > 1;
            CanGoPrev = CurrentPage > 1;
            CanGoNext = CurrentPage < TotalPages;
            CanGoLast = CurrentPage < TotalPages;
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                RefreshPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            var total = (_currentRootSource ?? TreeComboxItemsSource).Count();
            if (CurrentPage * PageSize < total)
            {
                CurrentPage++;
                RefreshPage();
            }
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = 1;
            RefreshPage();
        }

        private void EndPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = TotalPages;
            RefreshPage();
        }
        #endregion

        #region CommboxBox 下拉
        private bool isRefreshPageFirst = false;

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.SearchText))
            {
                if (TreeComboxItemsSource != null && TreeComboxItemsSource.Count > 0)
                {
                    if (!isRefreshPageFirst)
                    {
                        RefreshPage();
                    }
                }
            }
            // 回到 UI 线程操作界面
            if (IsEnableSearch)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ComboBox.Template != null)
                    {
                        Popup p = ComboBox.Template.FindName("PART_Popup", ComboBox) as Popup;
                        if (p != null)
                        {
                            _SearchTextBox = p.FindName("txt_SearchTextBox") as TextBox;
                            if (_SearchTextBox != null)
                            {
                                _SearchTextBox.Focus();
                                _SearchTextBox.SelectAll();
                            }
                        }
                    }
                }));
            }
        }
        #endregion

    }
}
