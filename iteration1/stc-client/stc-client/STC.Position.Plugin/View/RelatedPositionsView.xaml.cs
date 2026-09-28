using com.gateway.log;
using STC.Common.Handler.Position;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.View.Component;
using STC.Position.Plugin.ViewModel;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// RelatedPositionsView.xaml 的交互逻辑
    /// </summary>
    public partial class RelatedPositionsView : Window
    {
        public RelatedPositionsView(HistoryPositionOrderDetailViewModel viewModel, string execid)
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;
            this.DataContext = viewModel;
            comBookId.ItemsSource = viewModel.LstBookList;
            (this.DataContext as HistoryPositionOrderDetailViewModel).ChangedExecId = execid;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                emsloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ShowEMSMask(true);

                UpdatePositionReq req = new UpdatePositionReq();
                req.operationType = "1";
                req.newBookID = (this.DataContext as HistoryPositionOrderDetailViewModel).NewBookId;
                req.orderID = (this.DataContext as HistoryPositionOrderDetailViewModel).ChangedExecId;

                if ((this.DataContext as HistoryPositionOrderDetailViewModel).UpdatePosition(req, this))
                {
                    bool isOk = (this.DataContext as HistoryPositionOrderDetailViewModel).CommitRelated();
                    if (isOk)
                    {
                        this.Close();
                    }
                }

                ShowEMSMask(false);
            }));
        }

        
    }
}
