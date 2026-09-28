using STC.Common.Handler.PT;
using STC.Common.Model.PT;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace STC.PT.Plugin.View
{
    /// <summary>
    /// ApprovalHisView.xaml 的交互逻辑
    /// </summary>
    public partial class ApprovalHisPopupView : UserControl
    {
        private ApprovalHisPopupViewModel viewModel = null;
        public ApprovalHisPopupView()
        {
            InitializeComponent();
            viewModel = new ApprovalHisPopupViewModel();
            this.DataContext = viewModel;
        }

        public void initData(string execId)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                {
                    //PtExecOrderQueryReq req = new PtExecOrderQueryReq();
                    //req.execid = execId;
                    List<string> req = new List<string>();
                    req.Add(execId);
                    PtExecordersReplyPo result = PTHandler.Instance.ExecOrderApprovalHistQuery(req);
                    if (result != null && "0000".Equals(result.Code))
                    {
                        viewModel.setData(result.Datas);
                    }
                }));
            });
        }

    }
}
