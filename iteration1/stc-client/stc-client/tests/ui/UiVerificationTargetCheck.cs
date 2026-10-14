using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SalesTrading.UiTests
{
    internal static class UiVerificationTargetCheck
    {
        public sealed class SampleRow
        {
            public string Code { get; set; }
            public string Settlement { get; set; }
            public string TradeDate { get; set; }
        }

        [STAThread]
        private static int Main(string[] args)
        {
            string directory = Path.Combine(args[0], "verification-target-check");
            Directory.CreateDirectory(directory);
            var app = new Application();
            var grid = new DataGrid { AutoGenerateColumns = false, Height = 190,
                EnableRowVirtualization = true, EnableColumnVirtualization = true };
            grid.Columns.Add(new DataGridTextColumn { Header = "代码", Binding = new Binding("Code"), Width = 180 });
            grid.Columns.Add(new DataGridTextColumn { Header = "清算速度", Binding = new Binding("Settlement"), Width = 180 });
            grid.Columns.Add(new DataGridTextColumn { Header = "交易日期", Binding = new Binding("TradeDate"), Width = 180 });
            var rows = Enumerable.Range(1, 40).Select(index => new SampleRow
            {
                Code = "quote-" + index, Settlement = "T+1", TradeDate = "2026-10-15"
            }).ToArray();
            grid.ItemsSource = rows;
            var input = new TextBox { Text = "无关输入框" };
            var panel = new StackPanel();
            panel.Children.Add(grid);
            panel.Children.Add(input);
            var window = new Window { Content = panel, Width = 440, Height = 280, Title = "精确验证目标回归" };
            int result = 1;
            window.Loaded += async (sender, eventArgs) =>
            {
                IDisposable highlighter = null;
                try
                {
                    Type workerType = typeof(CaseDefinition).Assembly.GetType("SalesTrading.UiTests.UiScenarioWorker");
                    Type highlightType = workerType.Assembly.GetType("SalesTrading.UiTests.ControlHighlight");
                    highlighter = (IDisposable)Activator.CreateInstance(highlightType, true);
                    workerType.GetField("highlight", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, highlighter);
                    workerType.GetField("reportDirectory", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, directory);
                    workerType.GetField("definition", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null,
                        new CaseDefinition { Id = "verification-target", Name = "精确验证目标" });
                    object worker = Activator.CreateInstance(workerType, true);
                    var findCell = workerType.GetMethod("FindVerificationCell", BindingFlags.NonPublic | BindingFlags.Static);
                    var verify = workerType.GetMethod("VerifyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                    var targetProperty = highlightType.GetProperty("Target");
                    var pinnedProperty = highlightType.GetProperty("VerificationPinned");
                    var playback = new PlaybackControl(directory);
                    new OperationInterval(directory).Save(0);
                    grid.UpdateLayout();
                    var cell = (DataGridCell)findCell.Invoke(null, new object[] { grid, rows[1], "清算速度" });
                    Assert(ReferenceEquals(cell.DataContext, rows[1]) && cell.Column == grid.Columns[1], "必须选择指定行/列");
                    Assert(cell.ActualWidth < window.ActualWidth / 2, "不得用整个页面或整行代替字段");
                    bool checkedValue = false;
                    playback.SetPaused(true);
                    Task pending = (Task)verify.Invoke(worker, new object[] { cell, "第2行 · 清算速度：预期 T+1", (Action)(() => checkedValue = true) });
                    await Task.Delay(100);
                    Assert(!pending.IsCompleted && !checkedValue, "暂停时断言不得提前执行");
                    Assert(ReferenceEquals(targetProperty.GetValue(highlighter), cell), "暂停前必须显示正确单元格");
                    input.Focus();
                    await Task.Delay(50);
                    Assert(ReferenceEquals(targetProperty.GetValue(highlighter), cell), "焦点变化不得覆盖验证目标");
                    var screenshot = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    screenshot.Render(window);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(screenshot));
                    using (var stream = File.Create(Path.Combine(directory, "paused-cell.png"))) encoder.Save(stream);
                    playback.NextAction();
                    await pending;
                    Assert(checkedValue && playback.IsPaused, "单步应执行当前验证并保持暂停");
                    Assert(!(bool)pinnedProperty.GetValue(highlighter), "验证结束必须解除焦点锁定");
                    playback.SetPaused(false);

                    var lastCell = (DataGridCell)findCell.Invoke(null, new object[] { grid, rows[39], "交易日期" });
                    Assert(ReferenceEquals(lastCell.DataContext, rows[39]) && lastCell.Column == grid.Columns[2], "虚拟化滚动后必须定位正确单元格");
                    Assert(grid.EnableRowVirtualization && grid.EnableColumnVirtualization, "不得关闭虚拟化");
                    await (Task)verify.Invoke(worker, new object[] { lastCell, "末行交易日期", (Action)(() => { }) });
                    window.Width = 380;
                    window.UpdateLayout();
                    Assert(ReferenceEquals(targetProperty.GetValue(highlighter), lastCell), "调整窗口后目标身份不得改变");
                    await (Task)verify.Invoke(worker, new object[] { null, "服务端校验", (Action)(() => { }) });
                    Assert(targetProperty.GetValue(highlighter) == null, "后台断言不得残留界面红框");
                    bool missingRejected = false;
                    try
                    {
                        findCell.Invoke(null, new object[] { grid, rows[0], "不存在的列" });
                    }
                    catch (TargetInvocationException error)
                    {
                        missingRejected = error.InnerException.Message.Contains("验证目标缺失");
                    }
                    Assert(missingRejected, "缺失目标必须明确报错，不得退化为整个页面");
                    result = 0;
                    Console.WriteLine("PASS: exact cell; target before pause; focus pinning; single step; virtualized row/column scrolling; resizing; server-only clear; missing-target failure.");
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
                finally
                {
                    highlighter?.Dispose();
                    app.Shutdown(result);
                }
            };
            app.Run(window);
            return result;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
