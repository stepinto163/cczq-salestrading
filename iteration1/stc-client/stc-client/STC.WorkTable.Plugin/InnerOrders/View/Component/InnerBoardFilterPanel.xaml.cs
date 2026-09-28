using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.WorkTable.ViewModel.Data;
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

namespace STC.WorkTable.Plugin.InnerOrders.View.Component
{
    /// <summary>
    /// InnerBoardFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class InnerBoardFilterPanel : UserControl
    {
        // 过滤属性
        public static readonly DependencyProperty FilterDicProperty = DependencyProperty.Register("FilterDic", typeof(Dictionary<string, IList>),
            typeof(InnerBoardFilterPanel), new PropertyMetadata(new Dictionary<string, IList>()));

        public Dictionary<string, IList> FilterDic
        {
            get { return (Dictionary<string, IList>)GetValue(FilterDicProperty); }
            set { SetValue(FilterDicProperty, value); }
        }


        /// <summary>
        /// 过滤条件变化事件
        /// </summary>
        public static readonly RoutedEvent FilterDicChangedEvent = EventManager.RegisterRoutedEvent("FilterDicChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InnerBoardFilterPanel));

        public event RoutedEventHandler FilterDicChanged
        {
            add { AddHandler(FilterDicChangedEvent, value); }
            remove { RemoveHandler(FilterDicChangedEvent, value); }
        }

        public InnerBoardFilterPanel()
        {
            InitializeComponent();
            InitRoles();
        }

        private void InitRoles()
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                QueryRolesReplyPo reply = TableHandler.Instance.QueryRoles();
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        //RoleList.Add(new FilterListBoxItemData() { itemId = string.Empty, itemText = "全", itemIsSelected = true });
                        if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                        {
                            foreach (RoleInfo ri in reply.Datas)
                            {
                                //RoleList.Add(new FilterListBoxItemData() { itemId = ri.id, itemText = ri.rolename });
                                dic[ri.id] = ri.rolename;
                            }
                        }
                        this.RoleCbb.ItemsSource = dic;
                    }));
                }
            });
        }

        private void FirmFlagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.FirmTypeListBox != null)
            {
                FilterDic["FirmFlag"] = this.FirmTypeListBox.SelectedItems;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void SettlTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.SettlTypeListBox != null)
            {
                FilterDic["SettlType"] = this.SettlTypeListBox.SelectedItems;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void SideListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.SideListBox != null)
            {
                FilterDic["Side"] = this.SideListBox.SelectedItems;
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private void RoleCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (isCheckShowAll == true)
                return;

            if (this.RoleCbb != null)
            {
                FilterDic["Role"] = this.RoleCbb.SelectedValues.Split(',');
            }

            this.AdjustShowAll_ToggleButtonCheck();

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
        }

        private bool isNonmanualCheckShowAll = false;
        private bool isCheckShowAll = false;
        private Dictionary<string, string> venuesDic = new Dictionary<string, string>();
        private void ShowAll_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }

            isCheckShowAll = true;
            if (this.FirmTypeListBox != null)
            {
                foreach (ListBoxItem item in this.FirmTypeListBox.Items)
                {
                    item.IsSelected = false;
                }
            }

            if (this.SettlTypeListBox != null)
            {
                foreach (ListBoxItem item in this.SettlTypeListBox.Items)
                {
                    item.IsSelected = false;
                }
            }

            if (this.SideListBox != null)
            {
                foreach(ListBoxItem item in this.SideListBox.Items)
                {
                    item.IsSelected = false;
                }
            }

            if (this.RoleCbb != null)
            {
                this.RoleCbb.IsAll = true;
            }

            if (this.FirmTypeListBox != null)
            {
                FilterDic["FirmFlag"] = this.FirmTypeListBox.SelectedItems;
            }

            if (this.SettlTypeListBox != null)
            {
                FilterDic["SettlType"] = this.SettlTypeListBox.SelectedItems;
            }

            if (this.SideListBox != null)
            {
                FilterDic["Side"] = this.SideListBox.SelectedItems;
            }

            if (this.RoleCbb != null)
            {
                FilterDic["Role"] = this.RoleCbb.SelectedValues.Split(',');
            }

            RaiseEvent(new RoutedEventArgs { RoutedEvent = FilterDicChangedEvent });
            isCheckShowAll = false;
        }


        private void ShowAll_ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isNonmanualCheckShowAll == true)
            {
                isNonmanualCheckShowAll = false;
                return;
            }
        }

        private void AdjustShowAll_ToggleButtonCheck()
        {
            bool isChecked = true;
            foreach (KeyValuePair<string, IList> pair in this.FilterDic)
            {
                if (pair.Key == "FirmFlag")
                {
                    int firmFlagCount = 0;
                    foreach (ListBoxItem item in pair.Value)
                    {
                        if (item.IsSelected == true)
                        {
                            firmFlagCount++;
                        }
                    }
                    if (!(firmFlagCount == 0 || firmFlagCount == 2))
                    {
                        isChecked = false;
                        break;
                    }
                }
                else if (pair.Key == "SettlType")
                {
                    int settlTypeCount = 0;
                    foreach (ListBoxItem item in pair.Value)
                    {
                        if (item.IsSelected == true)
                        {
                            settlTypeCount++;
                        }
                    }
                    if (!(settlTypeCount == 0 || settlTypeCount == 3))
                    {
                        isChecked = false;
                        break;
                    }
                }
                else if (pair.Key == "Side")
                {
                    if (!(this.SideListBox.SelectedItems.Count == 0 || this.SideListBox.SelectedItems.Count == this.SideListBox.Items.Count))
                    {
                        isChecked = false;
                        break;
                    }
                }
                else if (pair.Key == "Role")
                {
                    string[] selectedRoleList = this.RoleCbb.SelectedValues.Split(',');
                    if (!(selectedRoleList.Length == 0 || selectedRoleList.Length == this.RoleCbb.ItemsSource.Count))
                    {
                        isChecked = false;
                        break;
                    }
                }
                //if (pair.Value.Count > 0)
                //{
                //    isChecked = false;
                //    break;
                //}
            }
            isNonmanualCheckShowAll = true;
            this.ShowAll_ToggleButton.IsChecked = isChecked;
            isNonmanualCheckShowAll = false;
        }

        private void ShowAll_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.FirmTypeListBox.SelectedIndex = -1;
            this.SettlTypeListBox.SelectedIndex = -1;
            this.SideListBox.SelectedIndex = -1;
            this.RoleCbb.IsAll = true;
        }
    }
}
