using STC.Common.ViewModel.Drp.Component;
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

namespace STC.Common.View.Drp.Component
{
    /// <summary>
    /// DRPOrderLogView.xaml 的交互逻辑
    /// </summary>
    public partial class DRPOrderLogView : UserControl
    {
        private DRPOrderLogViewModel ddrvm = null;
        public DRPOrderLogView()
        {
            InitializeComponent();
            ddrvm = new DRPOrderLogViewModel();
            this.DataContext = ddrvm;
        }

        /// <summary>
        /// 根据参数查询历史回价订单信息
        /// </summary>
        /// <param name="orderid">订单id</param>
        /// <param name="queryType">查询类型(1-报单方，2-询价、点价方)</param>
        public void QueryLog(string orderid,string queryType)
        {
            if (this.DataContext != null && this.DataContext is DRPOrderLogViewModel)
            {
                (this.DataContext as DRPOrderLogViewModel).Init(orderid, queryType);
            }
        }
    }
}
