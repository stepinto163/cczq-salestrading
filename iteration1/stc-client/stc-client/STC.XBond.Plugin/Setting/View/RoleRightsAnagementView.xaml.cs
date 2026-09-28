using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using STC.XBond.Plugin.Setting.ViewModel;
using STC.Frame.Utils;
using Ats.Foundation.Message;

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// RoleRightsAnagementView.xaml 的交互逻辑
    /// </summary>
    public partial class RoleRightsAnagementView : UserControl
    {
        // public List<Nodes> InitialNodeList { get; set; }//初始化集合数据
        private RoleTreeModel viewModel = new RoleTreeModel();
        public RoleRightsAnagementView()
        {
            InitializeComponent();
            //InitializeNodes();//初始化节点
            this.TreeView_NodeList.ItemsSource = this.viewModel.InitialNodeList;//显示节点
            //this.dgActions.ItemsSource = this.viewModel.InitialNodeList;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            List<Roles> Roles = this.viewModel.RolesSt;
            foreach (Roles role in Roles)
            {
                AddButton(role.id, role.rolename);
            }
            this.viewModel.Init();
        }

        /// <summary>
        /// 初始化节点
        /// </summary>
        /// <returns></returns>
        private void InitializeNodes(TModulesQuery moles)
        {

            //var parent = from m in moles.Msg
            //             where (m.id == m.parentid)
            //             select m;
            //InitialNodeList = new List<Nodes>();
            //foreach (Modules mo in parent)
            //{
            //    var child = from m in moles.Msg
            //                where (mo.id == m.parentid && mo.id != m.id)
            //                select m;
            //    var childn = new List<Nodes>();
            //    foreach (Modules co in child)
            //    {
            //        Nodes leafTwoNode = new Nodes();
            //        leafTwoNode.Id = co.id;
            //        leafTwoNode.Modulename = co.modulename;
            //        childn.Add(leafTwoNode);
            //    }
            //    var node1 = new Nodes();
            //    node1.Modulename = mo.modulename;
            //    node1.Childs = childn;

            //    InitialNodeList.Add(node1);





            //Nodes leafOneNode = new Nodes();
            //leafOneNode.Id = 10000;
            //leafOneNode.Modulename = "DKL-200x500";

            //Nodes leafTwoNode = new Nodes();
            //leafTwoNode.Id = 10001;
            //leafTwoNode.Modulename = "DKL-200x600";

            //Nodes leafThreeNode = new Nodes();
            //leafThreeNode.Id = 10002;
            //leafThreeNode.Modulename = "DKL-250x600";

            //var nodes = new List<Nodes>();
            //nodes.Add(leafOneNode);
            //nodes.Add(leafTwoNode);
            //nodes.Add(leafThreeNode);


            //var node1 = new Nodes();
            //node1.Modulename = CurrentCategoryName;
            //node1.Nodess = nodes;
            //var node2 = new Nodes();
            //node2.Modulename = "ddddd";
            //node2.Nodess.Add( node1);

            //InitialNodeList = new List<Nodes>();
            //InitialNodeList.Add(node1);
            //InitialNodeList.Add(node2);
        }

        private void cbDefault_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tree = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            Nodes tree2 = (Nodes)tree.Header;
            #region parent勾选，child都勾选；child勾掉，parent也勾掉
            if (tree2.Childs.Count > 0)
            {
                foreach (Nodes child in tree2.Childs)
                {
                    child.IsSelected = tree2.IsSelected;
                }
            }
            else if (tree2.Parent != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(tree);
                while (!(parent is TreeViewItem))
                {
                    TreeViewItem tree1 = VisualUpwardSearch<TreeViewItem>(VisualTreeHelper.GetParent(parent) as DependencyObject) as TreeViewItem;
                    Nodes treeroot = (Nodes)tree1.Header;
                    if (treeroot.Childs.Count > 0)
                    {
                        if (tree2.IsSelected.Value)
                        {
                            var li = from m in treeroot.Childs
                                     where (m.IsSelected == false)
                                     select m;
                            if (li.ToList().Count > 0)
                            {
                                treeroot.IsSelected = !tree2.IsSelected;
                            }
                            else
                            {
                                treeroot.IsSelected = tree2.IsSelected;
                            }
                        }
                        else
                        {
                            treeroot.IsSelected = tree2.IsSelected;
                        }
                        break;
                    }
                }
            }
            #endregion

            #region 一级选项勾掉，二级选项也勾掉。
            if (!tree2.IsSelected.Value)
            {
                if (tree2.Actionst != null && tree2.Actionst.Count > 0)
                {
                    this.dgActions.DataContext = null;
                    foreach (ModulesAction m in tree2.Actionst)
                    {
                        m.IsSelected = tree2.IsSelected.Value;
                    }
                    this.dgActions.DataContext = tree2.Actionst;
                }
            }
            #endregion
        }

        #region 二级选项勾掉，一级选项也勾掉；二级选项勾上，一级选项也勾上。
        private void cbRemarkAction_Click(object sender, RoutedEventArgs e)
        {
            Nodes tree2 = TreeView_NodeList.SelectedItem as Nodes;
            //if(tree2.Actionst)
            if (tree2.IsSelected.Value == true)
                return;
            bool IsContain = false;
            foreach (ModulesAction a in tree2.Actionst)
            {
                if (a.IsSelected)
                    IsContain = true;
            }
            if (IsContain)
                tree2.IsSelected = true;
        }
        #endregion

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tree = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            Nodes tree2 = (Nodes)tree.Header;
            if (tree2.Actionst != null)
            {
                this.dgActions.DataContext = tree2.Actionst;
                dgActions.Visibility = Visibility.Visible;
                laRemark.Visibility = Visibility.Hidden;
            }
            else
            {
                this.dgActions.DataContext = null;
                dgActions.Visibility = Visibility.Hidden;
                laRemark.Content = tree2.remark;
                laRemark.Visibility = Visibility.Visible;
            }
        }
        //private void cbDefault_Checked(object sender, RoutedEventArgs e)
        //{
        //    TreeViewItem tree = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
        //    Nodes tree2 = (Nodes)tree.Header;
        //    //if (this.viewModel.dicModulesAction.ContainsKey(tree2.id))
        //    //{
        //    //    this.dgActions.DataContext = this.viewModel.dicModulesAction[tree2.id];
        //    //    dgActions.Visibility = Visibility.Visible;
        //    //}
        //    //else
        //    //{
        //    //    this.dgActions.DataContext = null;
        //    //    dgActions.Visibility = Visibility.Hidden;
        //    //}
        //    if (tree2.Childs.Count > 0)
        //    {
        //        foreach (Nodes child in tree2.Childs)
        //        {
        //            child.IsSelected = tree2.IsSelected;
        //        }
        //    }
        //    else if (tree2.Parent != null)
        //    {
        //        DependencyObject parent = VisualTreeHelper.GetParent(tree);
        //        while (!(parent is TreeViewItem))
        //        {
        //            TreeViewItem tree1 = VisualUpwardSearch<TreeViewItem>(VisualTreeHelper.GetParent(parent) as DependencyObject) as TreeViewItem;
        //            Nodes treeroot = (Nodes)tree1.Header;
        //            if (treeroot.Childs.Count > 0)
        //            {
        //                if (tree2.IsSelected.Value)
        //                {
        //                    var li = from m in treeroot.Childs
        //                             where (m.IsSelected == false)
        //                             select m;
        //                    if (li.ToList().Count > 0)
        //                    {
        //                        treeroot.IsSelected = !tree2.IsSelected;
        //                    }
        //                    else
        //                    {
        //                        treeroot.IsSelected = tree2.IsSelected;
        //                    }
        //                }
        //                else
        //                {
        //                    treeroot.IsSelected = tree2.IsSelected;
        //                }
        //                break;
        //            }
        //        }
        //    }
        //}

        private void cbDefault_Unchecked(object sender, RoutedEventArgs e)
        {
            TreeViewItem tree = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            Nodes tree2 = (Nodes)tree.Header;
            //if (this.viewModel.dicModulesAction.ContainsKey(tree2.id))
            //{
            //    this.dgActions.DataContext = this.viewModel.dicModulesAction[tree2.id];
            //    dgActions.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    this.dgActions.DataContext = null;
            //    dgActions.Visibility = Visibility.Hidden;
            //}
            if (tree2.Childs.Count > 0)
            {
                foreach (Nodes child in tree2.Childs)
                {
                    child.IsSelected = tree2.IsSelected;
                }
            }
            else if (tree2.Parent != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(tree);
                while (!(parent is TreeViewItem))
                {
                    TreeViewItem tree1 = VisualUpwardSearch<TreeViewItem>(VisualTreeHelper.GetParent(parent) as DependencyObject) as TreeViewItem;
                    Nodes treeroot = (Nodes)tree1.Header;
                    if (treeroot.Childs.Count > 0)
                    {
                        if (tree2.IsSelected.Value)
                        {
                            var li = from m in treeroot.Childs
                                     where (m.IsSelected == false)
                                     select m;
                            if (li.ToList().Count > 0)
                            {
                                treeroot.IsSelected = !tree2.IsSelected;
                            }
                            else
                            {
                                treeroot.IsSelected = tree2.IsSelected;
                            }
                        }
                        else
                        {
                            treeroot.IsSelected = tree2.IsSelected;
                        }
                        break;
                    }
                }
            }
        }
       



        private void wrPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        //private void B_Click(object sender, RoutedEventArgs e)
        //{

        //}

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        #region lock/unlock
        private void lockBtn_Click(object sender, RoutedEventArgs e)
        {
            PassworVerificationU ui = new PassworVerificationU();
            bool? b = ui.ShowDialog();
            if (b == true)
            {
                // string memoryPwd = EncrypterHelper.Decrypt(ui.PassWord, LocalEnv.UserInfo.UserName);
                string pwd = Base64Encrypt.EncryptBase(ui.PassWord);
                if (pwd.Equals(LocalEnv.UserInfo.Password2))
                {
                    if (this.lockBtn.Visibility == Visibility.Visible)
                    {
                        this.lockBtn.Visibility = Visibility.Hidden;
                        unlockBtn.Visibility = Visibility.Visible;
                        SetEnabled(viewModel.InitialNodeList.ToList(), true);
                    }
                }
            }
        }

        private void SetEnabled(List<Nodes> list, bool IsEnabled)
        {
            foreach (Nodes s in list)
            {
                s.IsEnabled = IsEnabled;
                if (s.Childs != null && s.Childs.Count > 0)
                {
                    SetEnabled(s.Childs, IsEnabled);
                }
            }
        }

        private void unlockBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.unlockBtn.Visibility == Visibility.Visible)
            {
                this.unlockBtn.Visibility = Visibility.Hidden;
                lockBtn.Visibility = Visibility.Visible;
                SetEnabled(viewModel.InitialNodeList.ToList(), false);
            }
        }
        #endregion

        private void btnAddRole_Click(object sender, RoutedEventArgs e)
        {
            NewRoleUI ui = new NewRoleUI(this.viewModel.RolesSt);
            bool? b = ui.ShowDialog();
            if (b == true)
            {
                TRoleUpdate result = viewModel.AddRole(ui.RoleUpdateReq);
                if ("0000".Equals(result.Code))
                    AddButton(result.Msg.id, ui.RoleUpdateReq.rolename);
            }
        }
        private void btDeleteRole_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem item in wrPanel.SelectedItems)
            {
                wrPanel.Items.Remove(item);
                RoleUpdateReq req = new RoleUpdateReq();
                req.id = item.Tag + "";
                req.rolename = item.Content + "";
                req.action = "3";
                viewModel.DeleteRole(req);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveAction();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton(string id,string rolename)
        {
            ListBoxItem b = new ListBoxItem();
            b.Height = 24;
            b.Width = 85;
            //b.VerticalContentAlignment = VerticalAlignment.Center;
            //b.HorizontalContentAlignment = HorizontalAlignment.Center;
            //b.HorizontalAlignment = HorizontalAlignment.Center;
            //b.VerticalAlignment = VerticalAlignment.Center;
            b.Style = (Style)this.FindResource("ListBoxItemStyle");
            b.Content = rolename;
            b.Tag = id;
            wrPanel.Items.Add(b);
        }

        private void unlockBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}

 