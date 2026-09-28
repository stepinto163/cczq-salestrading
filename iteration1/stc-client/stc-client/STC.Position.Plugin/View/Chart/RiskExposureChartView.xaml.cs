using OxyPlot;
using STC.Position.Plugin.ViewModel.Chart;
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

namespace STC.Position.Plugin.View.Chart
{
    /// <summary>
    /// RiskExposureChartView.xaml 的交互逻辑
    /// </summary>
    public partial class RiskExposureChartView : UserControl
    {
        private RiskExposureChartViewModel _viewModel;

        public PlotController CustomPlotController { get; set; }

        public RiskExposureChartView()
        {
            InitializeComponent();

            _viewModel = new RiskExposureChartViewModel();

            //画直线
            this.DataContext = _viewModel;

            //画直线
            this.Plot.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            this.Plot.ActualController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            var pc = Plot.ActualController;
            this.Plot.MouseDoubleClick += Plot_MouseDoubleClick;

            var customPlotController = new PlotController();
            customPlotController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
                (view, controller, args) =>
                controller.AddHoverManipulator(view, new TrackerManipulator(view)
                { LockToInitialSeries = false, Snap = false, PointsOnly = true }, args)));

            CustomPlotController = customPlotController;

            this.Plot.Controller = CustomPlotController;

            //左键按住不放，移动图形
            //this.Plot.Controller.UnbindMouseDown(OxyMouseButton.Right);
            //this.Plot.Controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
        }

        private void Plot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Plot.Model.ResetAllAxes();
            Plot.InvalidatePlot(true);
        }

        public void TryDispose()
        {
            if (Plot != null)
            {
                Plot.Model = null;
                Plot = null;
            }
            if (_viewModel != null)
                _viewModel = null;
        }

    }
}
