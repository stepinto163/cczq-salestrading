using STC.Common.DataProcesser;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// CrossQtyPopup.xaml 的交互逻辑
    /// </summary>
    public partial class CrossQtyPopup : UserControl
    {
        public CrossQtyPopup()
        {
            InitializeComponent();
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                if (CrossQtyDataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                {
                    if (!cell.IsSelected)
                    {
                        cell.IsSelected = true;
                    }
                }
                else
                {
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    if (row != null && !row.IsSelected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void Edit_NetPrice_TXT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataRowView drv = (sender as NumericUpDown).DataContext as DataRowView;

                ModifyXBondNetPrice(drv);
            }
        }

        private void ModifyXBondNetPrice(DataRowView drv)
        {
            if (this.DataContext != null && this.DataContext is CrossQtyPopupViewModel)
            {
                CrossQtyPopupViewModel viewModel = this.DataContext as CrossQtyPopupViewModel;
                decimal edit_Netprice = CommonUtils.ConvertToDecimal(drv[CrossOrdersProcesser.EDIT_NETPRICE_FIELD].ToString());
                NewOrderSingle newOrderSingle = new NewOrderSingle();
                newOrderSingle.AlgoName = "CROSS";
                newOrderSingle.StrategyOrderId = viewModel.STCCode;
                newOrderSingle.ClOrdID = CommonUtils.ConvertToString(drv[CrossOrdersProcesser.CLORDID_FIELD]);
                newOrderSingle.NetPrice = edit_Netprice.ToString();
                PResult result = TBookHandler.Instance.updateOrder(newOrderSingle);
                if (result.IsSuccess() == false)
                {
                    Common.View.Component.CustomMessageBox.Error($"失败原因: {result.Info1}", "修改净价");
                }
            }
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Row.Item is System.Data.DataRowView)
            {
                DataRowView drv = e.Row.Item as DataRowView;
                if (this.DataContext != null && this.DataContext is CrossQtyPopupViewModel)
                {
                    CrossQtyPopupViewModel viewModel = this.DataContext as CrossQtyPopupViewModel;
                    decimal cal_px = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CrossOrdersProcesser.PRICE_FIELD]));
                    decimal cur_px = CommonUtils.ConvertToDecimal(CommonUtils.ConvertToString(drv[CrossOrdersProcesser.NETPRICE_FIELD]));
                    cal_px = cal_px / 100;
                    decimal cal_netPrice = viewModel.CalBond("4", viewModel.SettleType, cal_px.ToString());
                    decimal up_netPrice = cal_netPrice;
                    decimal down_netPrice = cal_netPrice;
                    decimal up_px = cal_px;
                    decimal down_px = cal_px;

                    // 向上调整
                    do
                    {
                        up_netPrice += 0.0001M;
                        up_px = viewModel.CalBond("2", viewModel.SettleType, up_netPrice.ToString());
                    }
                    while (cal_px == up_px);
                    up_netPrice -= 0.0001M;

                    // 向下调整
                    do
                    {
                        down_netPrice -= 0.0001M;
                        down_px = viewModel.CalBond("2", viewModel.SettleType, down_netPrice.ToString());
                    }
                    while (cal_px == down_px);
                    down_netPrice += 0.0001M;


                    // 设置当前numberUpDown的MaxValue 和 MinValue

                    if (e.EditingElement != null)
                    {
                        NumericUpDown numericUpDown = CommonUtils.FindVisualChild<NumericUpDown>(e.EditingElement) as NumericUpDown;
                        //NumericUpDown numericUpDown = sender as NumericUpDown;
                        if (numericUpDown != null)
                        {
                            numericUpDown.NumericValue = cur_px;
                            numericUpDown.MaxValue = up_netPrice;
                            numericUpDown.MinValue = down_netPrice;
                        }
                    }
                }
            }
        }

        private void EidtBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (sender as Button).DataContext as DataRowView;
            this.CrossQtyDataGrid.BeginEdit();
        }

        private void CrossQtyDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
           if(e.Row.Item != null && e.Row.Item is DataRowView)
            {
                DataRowView drv = e.Row.Item as DataRowView;
                string status = CommonUtils.ConvertToString(drv[CrossOrdersProcesser.STATUS_FIELD]);
                string venues = CommonUtils.ConvertToString(drv[CrossOrdersProcesser.VENUES_FIELD]);
                if (!((status == "正常" || status == "部分成交"|| status == "撤销中") && venues == "ODM"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (sender as Button).DataContext as DataRowView;
            this.CrossQtyDataGrid.CommitEdit();

            ModifyXBondNetPrice(drv);
        }
    }
}
