using OxyPlot;
using STC.Chart.Plugin.ViewModel;
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

namespace STC.Chart.Plugin.View.Chart
{
    /// <summary>
    /// PlotPage.xaml 的交互逻辑
    /// </summary>
    public partial class CandleLine : UserControl
    {
        private CandleLineViewModel _viewModel;

        public PlotController CustomPlotController { get; set; }
        public Window myWindow { get; set; }
        [Obsolete]
        public CandleLine()
        {
            InitializeComponent();

            _viewModel = new CandleLineViewModel();
            //画直线
            this.DataContext = _viewModel;

            this.Plot.MouseDoubleClick += Plot_MouseDoubleClick;

            this.Plot2.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            this.Plot2.MouseDoubleClick += Plot_MouseDoubleClick;

            rangeSlider.Minimum = 0;
            rangeSlider.Maximum = 1;
            rangeSlider.LowerValue = rangeSlider.Minimum;
            rangeSlider.UpperValue = rangeSlider.Maximum;
            rangeSlider.RangeMargin =0.1;

            //// 重写 PlotController
            var customPlotController = new PlotController();
            customPlotController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(
                (view, controller, args) =>
                controller.AddHoverManipulator(view, new CandlelineTrackerManipulator(view)
                { LockToInitialSeries = false, Snap = false, PointsOnly = true, Plot2 = this.Plot2 }, args)));
            CustomPlotController = customPlotController;

            this.Plot.Controller = CustomPlotController;
            //// 需要在重写Controller后，再次取消鼠标左键功能
            this.Plot.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            this.Plot.ActualController.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            this.Plot.ActualController.BindKeyDown(OxyKey.Escape, PlotCommands.Reset);
        }

        private void Plot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Plot.Model.ResetAllAxes();
            Plot.InvalidatePlot(true);
            Plot2.Model.ResetAllAxes();
            Plot2.InvalidatePlot(true);
        }


        private void RangeSlider_ValueChanged(double lower, double upper)
        {
            _viewModel.RangeSliderChange(lower, upper);
        }

        public void SetMyWindow(object mywin)
        {
            this.myWindow = (Window)mywin;
        }

        private void btnShutDown_Click(object sender, RoutedEventArgs e)
        {
            if (myWindow != null)
            {
                myWindow.Close();
                GC.Collect();
            }
        }
    }
}
