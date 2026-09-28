using OxyPlot;
namespace STC.Chart.Plugin.ViewModel
{
    using OxyPlot.Annotations;
    using OxyPlot.Series;
    using System;

    /// <summary>
    /// Provides a plot manipulator for tracker functionality.
    /// </summary>
    public class CandlelineTrackerManipulator : MouseManipulator
    {
        /// <summary>
        /// The current series.
        /// </summary>
        private Series currentSeries;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerManipulator" /> class.
        /// </summary>
        /// <param name="plotView">The plot view.</param>
        public CandlelineTrackerManipulator(IPlotView plotView) : base(plotView)
        {
            this.Snap = true;
            this.PointsOnly = false;
            this.LockToInitialSeries = true;
        }

        private EventHandler<TrackerEventArgs> _trackerChanged;

        /// <summary>
        /// Occurs when the tracker has been changed.
        /// </summary>
        public event EventHandler<TrackerEventArgs> TrackerChanged
        {
            add { _trackerChanged += value; }
            remove { _trackerChanged -= value; }
        }

        public IPlotView Plot2 { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to show tracker on points only (not interpolating).
        /// </summary>
        public bool PointsOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to snap to the nearest point.
        /// </summary>
        public bool Snap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to lock the tracker to the initial series.
        /// </summary>
        /// <value><c>true</c> if the tracker should be locked; otherwise, <c>false</c>.</value>
        public bool LockToInitialSeries { get; set; }

        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            e.Handled = true;

            this.currentSeries = null;
            this.PlotView.HideTracker();
            if (Plot2.ActualModel.Annotations.Count > 0)
            {
                Plot2.ActualModel.Annotations.Clear();
                Plot2.ActualModel.InvalidatePlot(false);
            }
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            e.Handled = true;

            if (this.currentSeries == null || !this.LockToInitialSeries)
            {
                // get the nearest
                this.currentSeries = this.PlotView.ActualModel != null ? this.PlotView.ActualModel.GetSeriesFromPoint(e.Position, 20) : null;
            }

            if (this.currentSeries == null)
            {
                if (!this.LockToInitialSeries)
                {
                    this.PlotView.HideTracker();

                    if (Plot2.ActualModel.Annotations.Count > 0)
                    {
                        Plot2.ActualModel.Annotations.Clear();
                        Plot2.ActualModel.InvalidatePlot(false);
                    }
                    // Plot2.HideTracker();
                }

                return;
            }

            var actualModel = this.PlotView.ActualModel;
            if (actualModel == null)
            {
                return;
            }

            if (!actualModel.PlotArea.Contains(e.Position.X, e.Position.Y))
            {
                return;
            }

            var result = GetNearestHit(this.currentSeries, e.Position, this.Snap, this.PointsOnly);
            if (result != null)
            {
                result.PlotModel = this.PlotView.ActualModel;
                this.PlotView.ShowTracker(result);
                // Plot2.ShowTracker(result);

                var maxY = 10;
                if (Plot2.ActualModel.Annotations.Count > 0)
                {
                    Plot2.ActualModel.Annotations.Clear();
                    Plot2.ActualModel.InvalidatePlot(false);
                }

                var arrowAnnotation_y = new ArrowAnnotation
                {
                    ToolTip = "Bottom",
                    StartPoint = new DataPoint(result.DataPoint.X, 0),
                    EndPoint = new DataPoint(result.DataPoint.X, maxY),
                    HeadWidth = 3,
                    LineStyle = LineStyle.Dot,
                    StrokeThickness = 1.5,
                    HeadLength = 0,
                    Veeness = 0,
                    Color = OxyColor.Parse("#FFEBC8"),
                };

                Plot2.ActualModel.Annotations.Add(arrowAnnotation_y);
                Plot2.ActualModel.InvalidatePlot(true);
            }
        }

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);
            this.currentSeries = this.PlotView.ActualModel != null ? this.PlotView.ActualModel.GetSeriesFromPoint(e.Position) : null;
            this.Delta(e);
        }

        /// <summary>
        /// Gets the nearest tracker hit.
        /// </summary>
        /// <param name="series">The series.</param>
        /// <param name="point">The point.</param>
        /// <param name="snap">Snap to points.</param>
        /// <param name="pointsOnly">Check points only (no interpolation).</param>
        /// <returns>A tracker hit result.</returns>
        private static TrackerHitResult GetNearestHit(Series series, ScreenPoint point, bool snap, bool pointsOnly)
        {
            if (series == null)
            {
                return null;
            }

            // Check data points only
            if (snap || pointsOnly)
            {
                var result = series.GetNearestPoint(point, false);
                if (result != null)
                {
                    if (result.Position.DistanceTo(point) < 20)
                    {
                        return result;
                    }
                }
            }

            // Check between data points (if possible)
            if (!pointsOnly)
            {
                var result = series.GetNearestPoint(point, true);
                return result;
            }

            return null;
        }
    }
}