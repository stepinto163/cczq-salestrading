using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using LogarithmicAxis = OxyPlot.Axes.LogarithmicAxis;
using ScatterSeries = OxyPlot.Series.ScatterSeries;

namespace STC.Chart.Plugin.ViewModel
{
    public class ScatterSeriesDemoViewModel : ViewModelBase
    {
        public PlotModel model { get; set; }

        private object _CurrentTag = string.Empty;
        public object CurrentTag
        {
            get
            {
                return _CurrentTag;
            }

            set
            {
                _CurrentTag = value;
                OnPropertyChanged(nameof(CurrentTag));
            }
        }

        public List<LineData> Data { get; set; }
        public List<LineData> BidScatterData { get; set; }
        public List<LineData> OfrScatterData { get; set; }

        public LinearAxis linearAxis1;

        public LogarithmicAxis logarithmicAxis1;

        public LineSeries lineSeries1;
        public ScatterSeriesDemoViewModel()
        {

            model = new PlotModel
            {
                PlotType = PlotType.XY,
                Title = "国开债曲线",
                TitleFontSize = 16,
                IsLegendVisible = false,
                PlotAreaBackground = OxyColor.Parse("#121A19"),
                PlotAreaBorderColor = OxyColor.Parse("#193D37"),
                PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1),
                //LegendBorderThickness = 0,
                TitleColor = OxyColors.Wheat,
                TitlePadding = 0,
                //LegendItemAlignment = OxyPlot.HorizontalAlignment.Right,
                PlotMargins = new OxyThickness(50, 0, 0, 20), //调节上边距

            };

            model.Background = OxyColor.Parse("#0A0F0E");
            model.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea;
            model.TitlePadding = 4;
            linearAxis1 = new LinearAxis();
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.Title = "X";
            linearAxis1.TextColor = OxyColors.White;
            linearAxis1.MaximumPadding = 0.05;
            linearAxis1.Minimum = 0;
            linearAxis1.StringFormat = "f2";
            linearAxis1.MajorGridlineColor = OxyColor.Parse("#193D37");
            linearAxis1.MajorGridlineThickness = 1;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.MinorGridlineThickness = 0.5;
            model.Axes.Add(linearAxis1);
            
            logarithmicAxis1 = new LogarithmicAxis();
            logarithmicAxis1.MajorGridlineColor = OxyColor.Parse("#193D37");
            logarithmicAxis1.MajorGridlineStyle = LineStyle.Solid;
            logarithmicAxis1.MajorGridlineThickness = 1;
            logarithmicAxis1.MinorGridlineStyle = LineStyle.Dot;
            logarithmicAxis1.MinorGridlineThickness = 0.5;
            logarithmicAxis1.AxislineThickness = 1;
            logarithmicAxis1.Title = "Y";
            logarithmicAxis1.TextColor = OxyColors.White;
            logarithmicAxis1.MaximumPadding = 0.1;
            logarithmicAxis1.MinimumPadding = 0.05;
            logarithmicAxis1.StringFormat = "f4";
            model.Axes.Add(logarithmicAxis1);

            lineSeries1 = new LineSeries();
            lineSeries1.Color = OxyColor.Parse("#FFFFFF");
            lineSeries1.Title = "Data:";
            lineSeries1.BrokenLineThickness = 0.1;
            lineSeries1.LineStyle = LineStyle.Solid;
            lineSeries1.StrokeThickness = 1;
            lineSeries1.Color = OxyColor.Parse("#FB943C");
            lineSeries1.MarkerType = MarkerType.Circle;
            lineSeries1.MarkerSize = 0;
            lineSeries1.MarkerFill = OxyColor.Parse("#FB943C");
            model.Series.Add(lineSeries1);

            //请求数据并画图
            LoadDataAndDraw();
        }

        public async void LoadDataAndDraw()
        {
            CalCurveReq req = new CalCurveReq() { scale = 5, terms = new string[] { "0.0", "1.0", "3.0", "5.0", "7.0", "10.0" }, yields = new string[] { "1.49", "2.05", "2.38", "2.65", "2.82", "2.85" } };
            CalCurveResult reply = await GetData(req);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    Data = new List<LineData>();
                    for (int i = 0; i < reply.Datas.Count - 1; i++)
                    {
                        Double date = Double.Parse(reply.Datas[i][0]);
                        Double open = Double.Parse(reply.Datas[i][1]);

                        Data.Add(new LineData { x = date, y = open, });
                    }
                }

                DrawLine();
            }
        }

        public void DrawLine()
        {
            
            ScatterSeries OfrScatterSeries = new ScatterSeries
            {
                Title = "Ofr",
                MarkerType = MarkerType.Diamond,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.Blue,

            };
            ScatterSeries BidScatterSeries = new ScatterSeries
            {
                Title = "Bid",
                MarkerType = MarkerType.Triangle,
                MarkerStroke = OxyColors.Orange,
                MarkerFill = OxyColors.Orange
            };

            model.Series.Add(OfrScatterSeries);
            model.Series.Add(BidScatterSeries);

            // 数值范围
            if (Data.Count > 0)
            {
                linearAxis1.Minimum = 0;
                linearAxis1.Maximum = Data[Data.Count - 1].x + 0.1;
                linearAxis1.AbsoluteMinimum = 0;
                linearAxis1.AbsoluteMaximum = Data[Data.Count - 1].x + 0.1;
            }

            // 画曲线
            if (Data.Count > 0)
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    lineSeries1.Points.Add(new DataPoint(Data[i].x, Data[i].y));
                }
            }

            model.InvalidatePlot(true);
            // 画散点
        }

        public async Task<CalCurveResult> GetData(CalCurveReq req)
        {
            CalCurveResult reply = null;
            await Task.Run(() =>
            {
                reply = CalHandler.Instance.CalCurve(req);
            });

            return reply;
        }

       
    }

    public class LineData
    {
        public double x { get; set; }
        public double y { get; set; }
    }
}
