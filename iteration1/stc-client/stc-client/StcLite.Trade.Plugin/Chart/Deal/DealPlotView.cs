using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Price.Plugin.Chart.Deal
{
    public class DealPlotView : PlotView
    {
        protected override IRenderContext CreateRenderContext()
        {
            return new DealCanvasRenderContext(this.Canvas);
        }

        protected override void ClearBackground()
        {
            if (this.Canvas != null)
            {
                this.Canvas.Children.Clear();

                if (this.ActualModel != null && this.ActualModel.Background.IsVisible())
                {
                    this.Canvas.Background = this.ActualModel.Background.ToBrush();
                }
                else
                {
                    this.Canvas.Background = null;
                }
            }
        }

        public void Dispose()
        {
            this.ClearBackground();
        }
    }
}
