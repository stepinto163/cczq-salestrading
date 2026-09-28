using STC.Broker.Plugin.Model;
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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// BondShowControl.xaml 的交互逻辑
    /// </summary>
    public partial class BondShowControl : UserControl
    {
        public BondShowControl()
        {
            InitializeComponent();
        }

        private bool _isDoubleClickGrid = true;
        private void tbOrderId_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void tbOrderId_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void tbOrderId_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                _isDoubleClickGrid = false;
                e.Handled = true;
                TextBlock textBlock = sender as TextBlock;
                TransactionOrdermModel orderDetailPo = textBlock.DataContext as TransactionOrdermModel;

                if (orderDetailPo == null)
                    return;

                //if (!recordDetailDomain.Check(orderDetailPo, OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId)))
                //    return;

                orderDetailPo.BondEditVisible = Visibility.Visible;
                orderDetailPo.BondShowVisible = Visibility.Collapsed;
                orderDetailPo.BondShowNoEditVisible = Visibility.Collapsed;
                
                //ContentPresenter cp = dgOrder.CurrentCell.Column.GetCellContent(dgOrder.SelectedItem) as ContentPresenter;
                //SearchEditUI searchEditUI = cp.ContentTemplate.FindName("seGoodscode", cp) as SearchEditUI;
                //if (searchEditUI != null)
                //{
                //    searchEditUI.Data = orderDetailPo;
                //    searchEditUI.SetButtonVisible(Visibility.Collapsed);
                //    searchEditUI.SetSearchTxt(orderDetailPo.GoodsCode);
                //}
            }
        }
    }
}
