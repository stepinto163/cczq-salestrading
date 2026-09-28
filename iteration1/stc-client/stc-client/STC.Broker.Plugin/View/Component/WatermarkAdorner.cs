using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace STC.Broker.Plugin.View.Component
{
    public  class WatermarkAdorner: Adorner
    {
        private string _watermarkText;

        public WatermarkAdorner(UIElement adornedElement, string watermarkText) : base(adornedElement)
        {
            if (string.IsNullOrWhiteSpace(watermarkText))
            {
                _watermarkText = LocalEnv.UserInfo.FullName;
            }
            else {
                _watermarkText = watermarkText;
            }
            
            this.IsHitTestVisible = false; //使水印不捕获事件
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect rect = new Rect(this.AdornedElement.RenderSize);
            double centerX = rect.Right / 2.0;
            double centerY = rect.Bottom / 2.0;
            //drawingContext.
            drawingContext.PushOpacity(0.3);
            RotateTransform rotateTransform = new RotateTransform(45, centerX, centerY);
            drawingContext.PushTransform(rotateTransform);

            RotateTransform rt = new RotateTransform(-45, centerX, centerY);
            Point point = default(Point);
            double n = 5.0;
            double margin = 40;
            double halfWidth = GetTextLength(_watermarkText) * 10 / 2.0;


            //第1排3个
            point = RotatePoint(0.5, 0.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(1.5, 0.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(2.5, 0.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(3.5, 0.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(4.5, 0.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);

            //第2排2个
            point = RotatePoint(1, 1.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(2, 1.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(3, 1.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(4, 1.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);

            //第3排3个
            point = RotatePoint(0.5, 2.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(1.5, 2.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(2.5, 2.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(3.5, 2.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(4.5, 2.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);

            //第4排2个
            point = RotatePoint(1, 3.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(2, 3.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(3, 3.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(4, 3.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);

            //第5排3个
            point = RotatePoint(0.5, 4.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(1.5, 4.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(2.5, 4.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(3.5, 4.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);
            point = RotatePoint(4.5, 4.5, n, rt, rect, margin);
            DrawText(point.X, point.Y, halfWidth, drawingContext, _watermarkText);

        }

        private void DrawText(double x, double y, double textHalfWidth, DrawingContext drawingContext, string text)
        {
            int fontSize = 20;
            SolidColorBrush colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
            Point point = new Point(x - textHalfWidth, y - fontSize / 2.0);
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), fontSize, colorBrush);
            drawingContext.DrawText(formattedText, point);
        }

        /// <summary>
        /// 旋转Point
        /// </summary>
        /// <param name="ratioX">文本中心点占区域宽度n等分的比例</param>
        /// <param name="ratioY">文本中心点占区域长度n等分的比例</param>
        /// <param name="n">区域长宽n等分</param>
        /// <param name="rotateTransform">旋转对象</param>
        /// <param name="rect">区域</param>
        /// <param name="margin">Margin</param>
        private Point RotatePoint(double ratioX, double ratioY, double n, RotateTransform rotateTransform, Rect rect, double margin)
        {
            return rotateTransform.Transform(new Point(ratioX / n * rect.Right, ratioY / n * (rect.Bottom - 2 * margin) + margin));
        }

        #region 计算文本长度(汉字计为2 大写字母计为1.5 小写字母计为1)
        /// <summary>
        /// 计算文本长度(汉字计为2 大写字母计为1.5 小写字母计为1)
        /// </summary>
        private double GetTextLength(string text)
        {
            double length = 0;

            Regex reg1 = new Regex("[\u4E00-\u9FFF]|[\uFE30-\uFFA0]");
            Regex reg2 = new Regex("[A-Z]");

            foreach (char c in text)
            {
                if (reg1.IsMatch(c.ToString()))
                {
                    length += 2;
                }
                else if (reg2.IsMatch(c.ToString()))
                {
                    length += 1.5;
                }
                else
                {
                    length += 1;
                }
            }

            return length;
        }
        #endregion

    }
}
