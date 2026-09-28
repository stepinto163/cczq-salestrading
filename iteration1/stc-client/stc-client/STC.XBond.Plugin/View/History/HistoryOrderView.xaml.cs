using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// HistoryOrderView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryOrderView : UserControl
    {
        private HistoryOrderViewModel _historyOrderViewModel = new HistoryOrderViewModel();

        private String isDouble = "";

        public String IsDouble
        {
            get
            {
                return isDouble;
            }

            set
            {
                isDouble = value;
            }
        }

        public bool IsLoad
        {
            get
            {
                return isLoad;
            }

            set
            {
                isLoad = value;
            }
        }

        private bool isLoad = true;

        public HistoryOrderView()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _historyOrderViewModel;
            _historyOrderViewModel.HistoryOrderView = this;
            _historyOrderViewModel.PageControl = this.pageControl;
            _historyOrderViewModel.EMSPageControl = this.EMSPageControl;
            //_historyOrderViewModel.Init();
        }

        private void ContentPresenter_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void Orders_dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void historyOrderFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.historyOrderFilter.FilterDic.ContainsKey("TransactDate"))
            {
                EMSPageControl.Page = 1;
                if (this.EMSFilterPanel.FilterDic["TransactDate"][0].Equals(this.historyOrderFilter.FilterDic["TransactDate"][0])&& this.EMSFilterPanel.FilterDic["TransactDate"][1].Equals(this.historyOrderFilter.FilterDic["TransactDate"][1]))
                {
                    _historyOrderViewModel.StcCodeStatus = false;
                }
                else
                {
                    _historyOrderViewModel.StcCodeStatus = true;
                }
                //_historyOrderViewModel.SelectSTCCode = "";//查了两次
                stcCodeTextBox.Text = "";
                _historyOrderViewModel.EMSRequest.firmflag = "";
                _historyOrderViewModel.EMSRequest.orderStatus = "";
                _historyOrderViewModel.EMSRequest.venues = "";
                emsOrderTextBox.Text = "";
                emsOrderCombox.SelectedIndex = 0;
                _historyOrderViewModel.EMSRequest.strategyorderid = "";
                _historyOrderViewModel.EMSRequest.securityid = "";
                _historyOrderViewModel.EMSRequest.symbol = "";
                this.EMSFilterPanel.FilterDic.Remove("TransactDate");
                this.EMSFilterPanel.FilterDic.Remove("OrderStatus");
                this.EMSFilterPanel.FilterDic.Remove("FirmFlag");
                this.EMSFilterPanel.FilterDic.Remove("Venues");
                IList dateList = this.historyOrderFilter.FilterDic["TransactDate"];
                IList list = new ArrayList();
                list.Add(dateList[0].ToString());
                list.Add(dateList[1].ToString());
                this.EMSFilterPanel.FilterDic.Add("TransactDate", list);
                _historyOrderViewModel.StcCodeStatus = false;
                EMSFilterPanel.setPro();
            }
            pageControl.Page = 1;
            _historyOrderViewModel.ProcessOrdersFilterCondition(this.historyOrderFilter.FilterDic);
        }

        private void pageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                _historyOrderViewModel.clear();
                _historyOrderViewModel.Index = e.Page-1;

                bool isAllow = Common.Handler.Admin.AdminHandler.Instance.CheckPermission("1003", null);
                if (!isAllow)
                {
                    return;
                }
                bool isNotAllow = AdminHandler.Instance.CheckPermission("4002", "400201");
                if (isNotAllow)
                {
                    return;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    ShowMask(true);
                    _historyOrderViewModel.RequestOrders();
                });
            }

        }

        private void EMSPageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                _historyOrderViewModel.EMSIndex = e.Page - 1;
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    _historyOrderViewModel.clearEMS();
                    ShowEMSMask(true);
                    _historyOrderViewModel.RequestEMSOrders();
                });
            }
        }

        private void EMSFilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.EMSFilterPanel.IsClick)
            {
                EMSPageControl.Page = 1;
                _historyOrderViewModel.ProcessEMSOrdersFilterCondition(this.EMSFilterPanel.FilterDic);
            }
        }

        private void emsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void orderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (orderTextBox != null && !string.IsNullOrEmpty(orderTextBox.Text))
            {
                FilterCondition();
                pageControl.Page = 1;
                _historyOrderViewModel.ProcessOrdersFilterCondition(this.historyOrderFilter.FilterDic);
            }
        }
        private void FilterCondition()
        {
            string text = orderTextBox.Text;
            if (((ComboBoxItem)orderComboBox.SelectedItem).Tag.ToString().Equals("1"))
            {
                _historyOrderViewModel.Request.orderid = text;
                _historyOrderViewModel.Request.securityid = "";
                _historyOrderViewModel.Request.symbol = "";
            }
            if (((ComboBoxItem)orderComboBox.SelectedItem).Tag.ToString().Equals("2"))
            {
                _historyOrderViewModel.Request.orderid = "";
                _historyOrderViewModel.Request.securityid = text;
                _historyOrderViewModel.Request.symbol = "";
            }
            if (((ComboBoxItem)orderComboBox.SelectedItem).Tag.ToString().Equals("3"))
            {
                _historyOrderViewModel.Request.orderid = "";
                _historyOrderViewModel.Request.securityid = "";
                _historyOrderViewModel.Request.symbol = text;
            }
        }
        private bool isFromEnter = false;
        private void orderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                FilterCondition();
                pageControl.Page = 1;
                _historyOrderViewModel.ProcessOrdersFilterCondition(this.historyOrderFilter.FilterDic);
            }
        }
        private bool orderFlag = false;
        private void orderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }
            if (!orderFlag)
                return;
            if (string.IsNullOrEmpty(orderTextBox.Text.Trim()))
            {
                _historyOrderViewModel.Request.orderid = "";
                _historyOrderViewModel.Request.securityid = "";
                _historyOrderViewModel.Request.symbol = "";
            }
            else
            {
                FilterCondition();
            }
        }

        private void orderTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            orderFlag = true;
        }

        private void orderTextBox_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            orderFlag = false;
        }

        private void emsOrderCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (emsOrderTextBox != null && !string.IsNullOrEmpty(emsOrderTextBox.Text))
            {
                EMSFilterCondition();
                EMSPageControl.Page = 1;
                _historyOrderViewModel.ProcessEMSOrdersFilterCondition(this.EMSFilterPanel.FilterDic);
            }
        }

        private void EMSFilterCondition()
        {
            string text = emsOrderTextBox.Text;
            if (((ComboBoxItem)emsOrderCombox.SelectedItem).Tag.ToString().Equals("1"))
            {
                _historyOrderViewModel.EMSRequest.strategyorderid = text;
                _historyOrderViewModel.EMSRequest.securityid = "";
                _historyOrderViewModel.EMSRequest.symbol = "";
            }
            if (((ComboBoxItem)emsOrderCombox.SelectedItem).Tag.ToString().Equals("2"))
            {
                _historyOrderViewModel.EMSRequest.strategyorderid = "";
                _historyOrderViewModel.EMSRequest.securityid = text;
                _historyOrderViewModel.EMSRequest.symbol = "";
            }
            if (((ComboBoxItem)emsOrderCombox.SelectedItem).Tag.ToString().Equals("3"))
            {
                _historyOrderViewModel.EMSRequest.strategyorderid = "";
                _historyOrderViewModel.EMSRequest.securityid = "";
                _historyOrderViewModel.EMSRequest.symbol = text;
            }
        }
        private void emsOrderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            isFromEnter = false;
            AppUtils.IsInnerInput = true;
            if (e.Key == Key.Enter)
            {
                isFromEnter = true;
                EMSFilterCondition();
                EMSPageControl.Page = 1;
                _historyOrderViewModel.ProcessEMSOrdersFilterCondition(this.EMSFilterPanel.FilterDic);
            }
        }
        private bool EMSorderFlag = false;
        private void emsOrderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            if (isFromEnter)
            {
                AppUtils.IsInnerInput = false;
            }
            if (!EMSorderFlag)
                return;
            if (string.IsNullOrEmpty(emsOrderTextBox.Text.Trim()))
            {
                _historyOrderViewModel.EMSRequest.strategyorderid = "";
                _historyOrderViewModel.EMSRequest.securityid = "";
                _historyOrderViewModel.EMSRequest.symbol = "";
            }
            else
            {
                EMSFilterCondition();
            }
        }

        private void emsOrderTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            EMSorderFlag = true;
        }

        private void emsOrderTextBox_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            EMSorderFlag = false;
        }

        private void Orders_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.History_Orders_dataGrid.SelectedIndex >= 0)
            {
                IsDouble = "IsDouble";
                _historyOrderViewModel.selectEMSOrderBySTCOrderID(true);
                _historyOrderViewModel.ClickNum = "2";
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //_historyOrderViewModel.Init();
        }

        public void load()
        {
            pageControl.Page = 1;
            EMSPageControl.Page = 1;
            _historyOrderViewModel.Init();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if ("".Equals(IsDouble) || "false".Equals(isDouble))
            {
                IsDouble = "isExpanded";
                DataGridRow row = FindVisualParent<DataGridRow>(sender as Expander);
                _historyOrderViewModel.selectEMSOrderBySTCOrderID(true);
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if ("".Equals(isDouble) || "false".Equals(isDouble))
            {
                IsDouble = "isCollapsed";
                DataGridRow row = FindVisualParent<DataGridRow>(sender as Expander);
                _historyOrderViewModel.selectEMSOrderBySTCOrderID(true);
            }
        }

        public T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
        private T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        public void ShowMask(bool flag)
        {
            loadMask.Dispatcher.Invoke(() =>
            {
                loadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }
        public void ShowEMSMask(bool flag)
        {
            emsloadMask.Dispatcher.Invoke(() =>
            {
                emsloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void orderTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AppUtils.IsInnerInput = false;
        }
    }
}
