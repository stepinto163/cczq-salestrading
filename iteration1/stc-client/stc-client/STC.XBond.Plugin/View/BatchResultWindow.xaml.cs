using STC.Common.Model.TBook;
using STC.Common.View.Component;
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
    /// BatchResultWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatchResultWindow : WindowBase
    {
        private BatchResultWindowViewModel batchResultWindowViewModel = new BatchResultWindowViewModel();

        public BatchResultWindow(
            Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic,
            Dictionary<string, List<TPlaceOrderResult>> replyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            int failCount
            )
        {
            InitializeComponent();
            batchResultWindowViewModel.Init(newOrderSingleDic,
                replyDic, prohibitReplyDic, failCount);
            this.DataContext = batchResultWindowViewModel;
        }
    }
}
