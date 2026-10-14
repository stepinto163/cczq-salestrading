using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SalesTrading.UiTests
{
    internal static class DemoCaptionCheck
    {
        [STAThread]
        public static int Main()
        {
            Application application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                DemoCaptionWindow caption = null;
                Window window = null;
                try
                {
                    TextBox input = new TextBox { Text = "演示输入框", Height = 35, VerticalAlignment = VerticalAlignment.Top };
                    window = new Window { Title = "字幕控件回归检查", Width = 1000, Height = 600, Content = input };
                    window.Show();
                    window.Activate();
                    input.Focus();
                    await Task.Delay(100);
                    IInputElement focus = Keyboard.FocusedElement;
                    caption = new DemoCaptionWindow();
                    caption.Present(window, "精确报价识别与确认", 2, 8, "正在执行", "输入报价文本并点击识别报价");
                    await Task.Delay(100);
                    Require(caption.IsVisible && !caption.ShowActivated, "Caption must be visible without activation");
                    Require(ReferenceEquals(focus, Keyboard.FocusedElement), "Caption stole input focus");
                    int style = GetWindowLong(new WindowInteropHelper(caption).Handle, -20);
                    Require((style & 0x08000020) == 0x08000020, "Missing native no-activate / mouse-pass-through styles");
                    Require(caption.OperationText == "正在执行：输入报价文本并点击识别报价", "Current action caption incorrect");
                    StackPanel lines = (StackPanel)((Border)caption.Content).Child;
                    Require(lines.Children.Count == 2, "Caption must show only heading and current operation");
                    caption.Present(window, "精确报价识别与确认", 2, 8, "已完成", "输入报价文本并点击识别报价");
                    await Task.Delay(100);
                    Require(caption.OperationText == "已完成：输入报价文本并点击识别报价", "Completion must retain the completed step");
                    Require(((TextBlock)lines.Children[0]).Text.EndsWith("2 / 8"), "Interval changed step number too early");
                    caption.Present(window, "精确报价识别与确认", 3, 8, "正在执行", "核对债券、机构与交易员");
                    caption.UpdateLayout();
                    Require(caption.OperationText.StartsWith("正在执行"), "Current action caption not updated");
                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)Math.Ceiling(caption.ActualWidth),
                        (int)Math.Ceiling(caption.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(caption);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (FileStream file = File.Create("demo-caption.png")) encoder.Save(file);
                    double oldLeft = caption.Left;
                    window.Left += 50;
                    await Task.Delay(350);
                    Require(Math.Abs(caption.Left - oldLeft - 50) < 2, "Caption did not follow host window");
                    caption.Suspend();
                    Require(!caption.IsVisible, "Performance-action suspension failed");
                    caption.Present(window, "精确报价识别与确认", 8, 8, "收尾", "清理本例测试数据");
                    Require(caption.IsVisible && caption.OperationText == "收尾：清理本例测试数据", "Caption did not resume for cleanup");
                    Console.WriteLine("PASS: current-only captions, completed-step caption, step numbering, input focus, click-through styles, host tracking, suspend/resume; screenshot demo-caption.png");
                    caption.Close();
                    window.Close();
                    application.Shutdown(0);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    caption?.Close();
                    window?.Close();
                    application.Shutdown(1);
                }
            }));
            return application.Run();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr handle, int index);
    }
}
