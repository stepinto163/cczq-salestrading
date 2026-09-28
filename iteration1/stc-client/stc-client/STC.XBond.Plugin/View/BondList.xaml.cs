using STC.Common.View.UI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// BondList.xaml 的交互逻辑
    /// </summary>
    public partial class BondList : UserControl, IContentInterface
    {
        private BondListViewModel _bondListViewModel = new BondListViewModel();

        public BondList()
        {
            InitializeComponent();
            this.XBondDataGrid.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.XBondDataGrid_MouseLeftButtonDown), true);

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = this._bondListViewModel;
            this._bondListViewModel.Init();
        }

        private void PopupTBook_Click(object sender, RoutedEventArgs e)
        {
            Window win = new Window();
            win.Background = new SolidColorBrush(Colors.Gray);
            StackPanel stackpanel = new StackPanel();
            stackpanel.Orientation = Orientation.Vertical;
            stackpanel.Children.Add(new QuotationFilterPanel());
            stackpanel.Children.Add(new QuotationList());
            win.Content = stackpanel;
            win.Show();
        }

        //private BitmapImage pauseImage = new BitmapImage(new Uri(@"/STC.XBond.Plugin; component/Images/stc_icon_input_pause_normal.png"));
        //private BitmapImage playImage = new BitmapImage(new Uri(@"/STC.XBond.Plugin; component/Images/stc_icon_input_play_normal.png"));
        private void PauseAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Image pauseImage = (Image)this.FindResource("pauseImage");
            Image playImage = (Image)this.FindResource("playIamge");
            bool isPause = this.PauseAllBtn.Content == pauseImage;
            string message = string.Format("确认要{0}所有债券?", isPause == true ? "暂停" : "启动");
            //MessageBoxResult result = MessageBox.Show(message, "X-bond做市", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            //    if (isPause)
            //    {
            //        this.PauseAllBtn.Content = playImage;
            //    }
            //    else
            //    {
            //        this.PauseAllBtn.Content = pauseImage;
            //    }
            //    if (this._bondListView != null)
            //    {
            //        this._bondListView.PauseAndPlayBonds(isPause);
            //    }
            //}
            MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
            messageBoxGUI.setWarnMsg(message);
            messageBoxGUI.MOK += PauseAll_MessageBoxGUI_MOK;
            messageBoxGUI.Owner = Application.Current.MainWindow;
            messageBoxGUI.ShowDialog();
        }

        private void PauseAll_MessageBoxGUI_MOK()
        {
            Image pauseImage = (Image)this.FindResource("pauseImage");
            Image playImage = (Image)this.FindResource("playIamge");
            bool isPause = this.PauseAllBtn.Content == pauseImage;
            if (isPause)
            {
                this.PauseAllBtn.Content = playImage;
            }
            else
            {
                this.PauseAllBtn.Content = pauseImage;
            }
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.PauseAndPlayBonds(isPause);
            }
        }

        private void StopAllBtn_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result = MessageBox.Show("确认停止所有债券?", "X-bond做市", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            //    if (this._bondListView != null)
            //    {
            //        this._bondListView.StopAll();
            //    }
            //}
            MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
            messageBoxGUI.setWarnMsg("确认停止所有债券?");
            messageBoxGUI.MOK += StopAll_MessageBoxGUI_MOK;
            messageBoxGUI.Owner = Application.Current.MainWindow;
            messageBoxGUI.ShowDialog();
        }

        private void StopAll_MessageBoxGUI_MOK()
        {
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.StopAll();
            }
        }

        private void StartAllBtn_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result = MessageBox.Show("确认启动所有债券?", "X-bond做市", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
            //    if (this._bondListView != null)
            //    {
            //        this._bondListView.StartAll();
            //    }
            //}
            MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
            messageBoxGUI.setWarnMsg("确认启动所有债券?");
            messageBoxGUI.MOK += StartAll_MessageBoxGUI_MOK;
            messageBoxGUI.Owner = Application.Current.MainWindow;
            messageBoxGUI.ShowDialog();
        }

        private void StartAll_MessageBoxGUI_MOK()
        {
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.StartAll();
            }
        }

        volatile bool isCheckSwitch = false;
        private void Switch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (this._bondListView != null)
            //{
            //    this._bondListView.OpenCloseBond();
            //}
           // e.Handled = false;
            isCheckSwitch = true;
        }

        private void XBondDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            
        }

        private void XBondDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.XBondDataGrid.CurrentCell != null && this.XBondDataGrid.CurrentCell.Column != null)
            {
                if (this.XBondDataGrid.CurrentCell.Column.DisplayIndex == 0)
                {
                    if (isCheckSwitch == true)
                    {
                        if (this._bondListViewModel != null)
                        {
                            this._bondListViewModel.OpenCloseBond();
                        }
                    }
                    if (isPopupTBook == true)
                    {
                        if (this._bondListViewModel != null)
                        {
                            this._bondListViewModel.OpenTradeBook();
                        }
                    }
                }
            }
            isCheckSwitch = false;
            isPopupTBook = false;
        }

        private void XBondDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.XBondDataGrid.CurrentCell != null
                && this.XBondDataGrid.CurrentCell.Column != null)
            {
                if (this.XBondDataGrid.CurrentCell.Column.DisplayIndex == 0)
                {
                    if (isCheckSwitch == true)
                    {
                        if (this._bondListViewModel != null)
                        {
                            this._bondListViewModel.OpenCloseBond();
                        }
                    }
                    if (isPopupTBook == true)
                    {
                        if (this._bondListViewModel != null)
                        {
                            this._bondListViewModel.OpenTradeBook();
                        }
                    }
                }
            }
            //this.CaptureMouse();
            
            //if (Mouse.Captured is Button)
            //{
            //    Mouse.Capture(null);
            //}
            //this.ReleaseMouseCapture();
            isCheckSwitch = false;
            isPopupTBook = false;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = true;
        }

        private const string ALL = "全部";

        private void ItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (e.AddedItems.Count > 0)
                {
                    foreach (object obj in e.AddedItems)
                    {
                        ListBoxItem item = obj as ListBoxItem;
                        if (item != null)
                        {
                            if (ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (lbi.IsSelected && !ALL.Equals(lbi.Content))
                                        lbi.IsSelected = false;
                                }
                            }
                            else
                            {
                                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                if (tmp.IsSelected)
                                    tmp.IsSelected = false;

                            }
                        }
                    }
                }

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (listBox.SelectedItems.Count == 0)
                            listBox.SelectedItem = item;
                    }
                }
            }
        }

        private void bondTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.FilterBondType(bondTypeFilterListBox.SelectedItems);
            }
        }

        private void RemaingingTimeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.FilterRemainingTime(RemaingingTimeFilterListBox.SelectedItems);
            }
        }

        private void Rating_CurrentFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.FilterRating(Rating_CurrentFilterListBox.SelectedItems);
            }
        }

        private void SettlTypeFilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemsSelectionChanged(sender, e);
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.FilterSettlType(SettlTypeFilterListBox.SelectedItems);
            }
        }

        private void CloseFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterPopup.IsOpen = false;
        }

        private void bondTypeFilterListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var item8 = this.bondTypeFilterListBox.Items[8];
            if (item8 != null && item8 is ListBoxItem)
            {
                ListBoxItem lbi = item8 as ListBoxItem;
                var reg = lbi.Template.FindName("Sep", lbi);
                if (reg != null && reg is Rectangle)
                {
                    ((Rectangle)reg).Visibility = Visibility.Hidden;
                }
            }
        }

        private void Rating_CurrentFilterListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var item10 = this.Rating_CurrentFilterListBox.Items[10];
            if (item10 != null && item10 is ListBoxItem)
            {
                ListBoxItem lbi = item10 as ListBoxItem;
                var reg = lbi.Template.FindName("Sep", lbi);
                if (reg != null && reg is Rectangle)
                {
                    ((Rectangle)reg).Visibility = Visibility.Hidden;
                }
            }
        }

        public void Clear()
        {
            if (this._bondListViewModel != null)
            {
                this._bondListViewModel.Clear();
                this._bondListViewModel.Dispose();
                this.DataContext = null;
            }
        }

        volatile bool isPopupTBook = false;
        private void popupTBook_Btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPopupTBook = true;
        }
    }
}
