using OxyPlot;
using BDB.Trade.Plugin.ViewModel.Chart;
using BDB.Trade.Plugin.ViewModel.Chat;
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

namespace BDB.Trade.Plugin.View.Chart
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionLadder : UserControl
    {
        private TransactionLadderViewModel _viewModel;
        public TransactionLadder()
        {
            InitializeComponent();
            _viewModel = new ViewModel.Chart.TransactionLadderViewModel();

            this.DataContext = _viewModel;

            //画直线
            this.Plot.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            this.Plot.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            var pc = Plot.ActualController;
            this.Plot.MouseDoubleClick += Plot_MouseDoubleClick;
            // 启用会不是鼠标移不动
            //pc.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
            //    (Plot, controller, args) =>
            //   controller.AddHoverManipulator(Plot, new WpbTrackerManipulator(Plot), args)));
        }

        private void Plot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Plot.Model.ResetAllAxes();
            Plot.InvalidatePlot(false);
        }
    }
}
