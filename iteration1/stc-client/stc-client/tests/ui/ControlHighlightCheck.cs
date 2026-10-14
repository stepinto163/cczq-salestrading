using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SalesTrading.UiTests
{
    internal static class ControlHighlightCheck
    {
        [STAThread]
        public static int Main()
        {
            Application application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                Window window = null;
                ControlHighlight highlight = null;
                try
                {
                    TextBox input = new TextBox { Text = "输入报价文本 / 核对识别结果", Height = 36, Width = 340, Margin = new Thickness(20) };
                    Button button = new Button { Content = "识别报价", Height = 36, Width = 120, Margin = new Thickness(20) };
                    StackPanel panel = new StackPanel { LayoutTransform = new ScaleTransform(1.25, 1.25) };
                    panel.Children.Add(input);
                    panel.Children.Add(button);
                    AdornerDecorator decorator = new AdornerDecorator { Child = panel };
                    window = new Window { Title = "自动化红框回归检查", Width = 640, Height = 360, Content = decorator };
                    window.Show();
                    window.Activate();
                    await Task.Delay(100);
                    highlight = new ControlHighlight();
                    button.Focus();
                    input.Focus();
                    await Task.Delay(80);
                    Require(ReferenceEquals(highlight.Target, input), "Focus did not highlight input");
                    IInputElement originalFocus = Keyboard.FocusedElement;
                    highlight.Show(button);
                    window.UpdateLayout();
                    Require(ReferenceEquals(originalFocus, Keyboard.FocusedElement), "Verification highlight stole focus");
                    Require(input.ActualHeight == 36 && button.ActualHeight == 36, "Highlight changed control layout");
                    Require(AdornerLayer.GetAdornerLayer(input).GetAdorners(input) == null, "Previous outline was not removed");
                    Adorner[] adorners = AdornerLayer.GetAdornerLayer(button).GetAdorners(button);
                    Require(adorners.Length == 1 && !adorners[0].IsHitTestVisible && !adorners[0].Focusable, "Outline intercepts input");
                    Point center = button.TranslatePoint(new Point(button.ActualWidth / 2, button.ActualHeight / 2), window);
                    DependencyObject hit = window.InputHitTest(center) as DependencyObject;
                    while (hit != null && !ReferenceEquals(hit, button))
                    {
                        hit = VisualTreeHelper.GetParent(hit);
                    }
                    Require(ReferenceEquals(hit, button), "Highlighted button cannot receive mouse input");
                    button.Width = 190;
                    window.Left += 40;
                    window.UpdateLayout();
                    await Task.Delay(80);
                    VerifyRedBounds(window, button, "control-highlight.png");
                    highlight.SetSuspended(true);
                    highlight.Show(button);
                    input.Focus();
                    Require(highlight.Target == null, "Latency action suspension failed");
                    highlight.SetSuspended(false);
                    highlight.Show(input);
                    input.Visibility = Visibility.Collapsed;
                    Require(highlight.Target == null, "Hidden control retained highlight");
                    input.Visibility = Visibility.Visible;
                    window.UpdateLayout();
                    highlight.Show(input);
                    panel.Children.Remove(input);
                    await Task.Delay(80);
                    Require(highlight.Target == null, "Unloaded control retained highlight");
                    highlight.Show(button);
                    highlight.Dispose();
                    Require(AdornerLayer.GetAdornerLayer(button).GetAdorners(button) == null, "Dispose left an outline");
                    Console.WriteLine("PASS: focus tracking, explicit verification, focus/layout preservation, mouse hit testing, scaled/resized red bounds, suspension, hidden/unloaded cleanup, disposal; control-highlight.png");
                    window.Close();
                    application.Shutdown(0);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    highlight?.Dispose();
                    window?.Close();
                    application.Shutdown(1);
                }
            }));
            return application.Run();
        }

        private static void VerifyRedBounds(Window window, FrameworkElement target, string filename)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth),
                (int)Math.Ceiling(window.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
            int redCount = 0;
            int minX = bitmap.PixelWidth;
            int maxX = 0;
            int minY = bitmap.PixelHeight;
            int maxY = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    int offset = (y * bitmap.PixelWidth + x) * 4;
                    if (pixels[offset + 2] > 230 && pixels[offset + 1] < 40 && pixels[offset] < 40)
                    {
                        redCount++;
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
            Rect expected = target.TransformToAncestor(window).TransformBounds(new Rect(target.RenderSize));
            Require(redCount > 200, "Red rectangle was not rendered");
            Require(Math.Abs(minX - expected.Left) < 3 && Math.Abs(maxX - expected.Right) < 3
                && Math.Abs(minY - expected.Top) < 3 && Math.Abs(maxY - expected.Bottom) < 3,
                "Red outline did not follow scaled and resized control bounds");
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
