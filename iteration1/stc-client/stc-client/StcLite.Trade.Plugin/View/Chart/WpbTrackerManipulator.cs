using OxyPlot;
using OxyPlot.Series;
using System.Linq;
using System.Text;

public class WpbTrackerManipulator : MouseManipulator
{
    /// <summary>
    /// The current series.
    /// </summary>
    private DataPointSeries currentSeries;

    public WpbTrackerManipulator(IPlotView plotView) : base(plotView)
    {
    }

    /// <summary>
    /// Occurs when a manipulation is complete.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Completed(OxyMouseEventArgs e)
    {
        base.Completed(e);
        e.Handled = true;

        currentSeries = null;
        // PlotView.HideTracker();
    }

    /// <summary>
    /// Occurs when the input device changes position during a manipulation.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Delta(OxyMouseEventArgs e)
    {
        base.Delta(e);
        e.Handled = true;

        if (currentSeries == null)
        {
            PlotView.HideTracker();
            return;
        }

        if (XAxis == null)
        {
            return;
        }

        var actualModel = PlotView.ActualModel;
        if (actualModel == null)
        {
            return;
        }

        if (!actualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
        {
            return;
        }

        

            //double sx = candle.InverseTransform(e.Position).X;
            //double sy = candle.InverseTransform(e.Position).Y;
            //TrackerHitResult ts = candle.GetNearestPoint(new ScreenPoint(sx, sy), false);
            //if (ts != null)
            //{
            //    CurrentTrackerOther = ts.Text;
            //}

            var time = currentSeries.InverseTransform(e.Position).X;
            // var points = currentSeries.ItemsSource as ObservableCollection<DataPoint>;
            var points = currentSeries.Points;
            DataPoint dp = points.FirstOrDefault(d => d.X >= time);
            // Exclude default DataPoint.
            // It has insignificant downside and is more performant than using First above
            // and handling exceptions.
            if (dp.X != 0 || dp.Y != 0)
            {
                    int index = points.IndexOf(dp);
                    var s = PlotView.ActualModel.PlotView;
                    object tag = ((System.Windows.FrameworkElement)s).Tag;
                    OxyPlot.PlotModel pm = PlotView.ActualModel as OxyPlot.PlotModel;
                    var jlSeries = (OxyPlot.Series.LineSeries)pm.Series[0];
                    OxyPlotDemo.ViewModel.FullData fulldata = (OxyPlotDemo.ViewModel.FullData)tag;
                    string res = string.Empty;
                    var position = XAxis.Transform(dp.X, dp.Y, currentSeries.YAxis);
                    position = new ScreenPoint(position.X, e.Position.Y);
                    var datapoint = jlSeries.InverseTransform(position);

                    string d = OxyPlot.Axes.DateTimeAxis.ToDateTime(datapoint.X).ToString();
                    var info = fulldata.Points1.FirstOrDefault(o => o.X == datapoint.X);
                    var info2 = fulldata.Points2.FirstOrDefault(o => o.X == datapoint.X);
                    if (info != null)
                    {
                        res = $"{d}\nPoint1:{info.Y}\nPoint2:{info2.Y}";
                    }

                    double[] values = new double[6];
                    var result = new WpbTrackerHitResult(values)
                    {
                        Series = currentSeries,
                        DataPoint = dp,
                        Index = index,
                        Item = res,
                        Position = position,
                        PlotModel = PlotView.ActualModel
                    };

                    PlotView.ShowTracker(result);
            }
    }

    /// <summary>
    /// Occurs when an input device begins a manipulation on the plot.
    /// </summary>
    /// <param name="e">
    /// The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.
    /// </param>
    public override void Started(OxyMouseEventArgs e)
    {
        base.Started(e);
        currentSeries = PlotView?.ActualModel?.Series.FirstOrDefault(s => s.IsVisible) as DataPointSeries;
        Delta(e);
    }
}


public class WpbTrackerHitResult : TrackerHitResult
{

    public double[] Values { get; private set; }

    // can't use the default indexer name (Item) since the base class uses that for something else
    [System.Runtime.CompilerServices.IndexerName("ValueString")]
    public string this[int index]
    {
        get
        {
            return Values[3].ToString();
            //System.Text.StringBuilder sb = new StringBuilder();
            //return string.Format((index == 1 || index == 4) ?
            //  "{0,7:###0   }" : "{0,7:###0.0#}", Values[index]);
        }
    }

    public WpbTrackerHitResult(double[] values)
    {
        Values = values;
    }
}