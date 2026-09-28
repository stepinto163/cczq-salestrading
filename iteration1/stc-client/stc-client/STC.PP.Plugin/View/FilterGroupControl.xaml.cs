using STC.Common.ViewModel.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.PP.Plugin.View
{
    /// <summary>
    /// FilterGroupControl.xaml 的交互逻辑
    /// </summary>
    public partial class FilterGroupControl : UserControl
    {
        public FilterGroupControl()
        {
            InitializeComponent();
        }
        #region PPGroupList
        public static readonly DependencyProperty PPGroupListProperty = DependencyProperty.Register("PPGroupList", typeof(ObservableCollection<FilterGroupItem>), typeof(FilterGroupControl), new PropertyMetadata(null));

        public ObservableCollection<FilterGroupItem> PPGroupList
        {
            get { return (ObservableCollection<FilterGroupItem>)GetValue(PPGroupListProperty); }
            set { SetValue(PPGroupListProperty, value); }
        }
        #endregion

        #region SelectedFilterGroupItem
        public static readonly DependencyProperty SelectedFilterGroupItemProperty = DependencyProperty.Register("SelectedFilterGroupItem", typeof(string), typeof(FilterGroupControl),new PropertyMetadata(string.Empty));

        public string SelectedFilterGroupItem
        {
            get { return (string)GetValue(SelectedFilterGroupItemProperty); }
            set { SetValue(SelectedFilterGroupItemProperty, value); }
        }

        #endregion

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(FilterGroupItem), typeof(FilterGroupControl), new PropertyMetadata(null));

        public FilterGroupItem SelectedItem
        {
            get { return (FilterGroupItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region CheckboxEvent
        public event EventHandler<string> CheckboxEvent;

        protected virtual void OnCheckboxEvent(string tag)
        {
            if (CheckboxEvent != null)
                CheckboxEvent(this, tag);
        }
        #endregion

        #region DeleteRequested 事件
        public event EventHandler<FilterGroupItem> DeleteRequested;

        protected virtual void OnDeleteRequested(FilterGroupItem content)
        {
            if (DeleteRequested != null)
                DeleteRequested(this, content);
        }

        #endregion

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var clicked = sender as CheckBox;
            if (clicked == null)
                return;

            // 互斥逻辑
            var panel = clicked.Parent as Panel;
            if (panel != null)
            {
                foreach (var child in panel.Children)
                {
                    var cb = child as CheckBox;
                    if (cb != null && cb != clicked)
                        cb.IsChecked = false;
                }
            }

            // 设置选中状态
            if (clicked.IsChecked == true)
            {
                UncheckOtherBoxes(clicked);

                if (clicked.DataContext is FilterGroupItem)
                {
                    SelectedItem = clicked.DataContext as FilterGroupItem;
                }

                SelectedFilterGroupItem = clicked.Tag != null ? clicked.Tag.ToString() : null;
                OnCheckboxEvent(SelectedFilterGroupItem);
            }
            else
            {
                SelectedFilterGroupItem = null;
                SelectedItem = null;
                OnCheckboxEvent(null);
            }
        }


        private void UncheckOtherBoxes(CheckBox exclude)
        {
            foreach (var item in FindVisualChildren<CheckBox>(this))
            {
                if (item != exclude)
                    item.IsChecked = false;
            }
        }

        /// <summary>
        /// 取消所有选择
        /// </summary>
        public void UnCheckAll()
        {
            foreach (var item in FindVisualChildren<CheckBox>(this))
            {
                item.IsChecked = false;
            }
        }

        private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

                var t = child as T;
                if (t != null)
                    yield return t;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }


        private CheckBox _lastRightClickedCheckBox; // 记录最后右键的CheckBox
        private void CheckBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb == null)
                return;

            _lastRightClickedCheckBox = cb;

            // 动态加载菜单
            var menu = this.Resources["DeleteMenu"] as ContextMenu;
            if (menu != null)
            {
                // 克隆菜单实例（资源设置 x:Shared="False"）
                menu.PlacementTarget = cb;
                menu.IsOpen = true;
            }

            e.Handled = true;
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = null;
            if (_lastRightClickedCheckBox != null)
                cb = _lastRightClickedCheckBox;

            if (cb.DataContext is FilterGroupItem)
            {
                FilterGroupItem fgi = cb.DataContext as FilterGroupItem;
                OnDeleteRequested(fgi);
            }
        }
    }
}
