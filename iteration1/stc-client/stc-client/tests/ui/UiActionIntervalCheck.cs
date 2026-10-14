using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;

namespace SalesTrading.UiTests
{
    internal static class UiActionIntervalCheck
    {
        [STAThread]
        private static int Main(string[] args)
        {
            string directory = Path.Combine(args[0], "action-interval-check-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var app = new Application();
            var panel = new StackPanel();
            var button = new Button { Content = "测试点击", Name = "ClickTarget" };
            var text = new TextBox { Name = "InputTarget" };
            var selection = new ComboBox { Name = "SelectionTarget" };
            selection.Items.Add("first");
            selection.Items.Add("second");
            panel.Children.Add(button);
            panel.Children.Add(text);
            panel.Children.Add(selection);
            var window = new Window { Content = panel, Width = 320, Height = 180 };
            int result = 1;
            window.Loaded += async (sender, eventArgs) =>
            {
                try
                {
                    Type workerType = typeof(CaseDefinition).Assembly.GetType("SalesTrading.UiTests.UiScenarioWorker");
                    workerType.GetField("reportDirectory", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, directory);
                    workerType.GetField("definition", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null,
                        new CaseDefinition { Id = "action-delay", Name = "动作等待验证" });
                    object worker = Activator.CreateInstance(workerType, true);
                    var interval = new OperationInterval(directory);
                    int heartbeat = 0;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
                    timer.Tick += (source, tick) => heartbeat++;
                    timer.Start();
                    int clicks = 0;
                    button.Click += (source, click) => clicks++;
                    interval.Save(200);
                    await CheckDelay(() => Invoke(worker, "ClickAsync", button), () => clicks == 1);
                    var boundSource = new TextBox { Text = "original" };
                    text.SetBinding(TextBox.TextProperty, new Binding("Text")
                    {
                        Source = boundSource, Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
                    await CheckDelay(() => Invoke(worker, "SetTextAsync", text, "changed"), () => text.Text == "changed");
                    Assert(BindingOperations.IsDataBound(text, TextBox.TextProperty) && boundSource.Text == "changed",
                        "自动输入必须保留双向绑定");
                    boundSource.Text = "server-update";
                    text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    Assert(text.Text == "server-update", "后续模型更新必须回填输入框");
                    boundSource.Text = "changed";
                    text.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    await CheckDelay(() => Invoke(worker, "SelectItemAsync", selection, "second"), () => Equals(selection.SelectedItem, "second"));
                    Assert(heartbeat >= 10, "等待不能阻塞 WPF 消息循环");

                    interval.Save(60000);
                    Task pending = Invoke(worker, "ClickAsync", button);
                    await Task.Delay(100);
                    Assert(clicks == 1, "等待结束前不得点击");
                    interval.Save(0);
                    await pending;
                    Assert(clicks == 2, "实时调为0后应立即执行");

                    string sharedDirectory = Path.Combine(directory, "shared");
                    Directory.CreateDirectory(sharedDirectory);
                    Environment.SetEnvironmentVariable("STC_UI_REALTIME_ROOT", sharedDirectory);
                    var sharedInterval = new OperationInterval(sharedDirectory);
                    sharedInterval.Save(200);
                    Stopwatch verificationTime = Stopwatch.StartNew();
                    pending = Invoke(worker, "HoldVerificationAsync", text, "核对结果");
                    await Task.Delay(50);
                    Assert(!pending.IsCompleted, "验证结果必须保留供人工观察");
                    await pending;
                    Assert(verificationTime.ElapsedMilliseconds >= 190, "验证展示未使用共享操作间隔");
                    sharedInterval.Save(60000);
                    pending = Invoke(worker, "HoldVerificationAsync", text, "实时调速");
                    await Task.Delay(100);
                    Assert(!pending.IsCompleted, "验证展示过早完成");
                    sharedInterval.Save(0);
                    await pending;
                    sharedInterval.Save(60000);
                    pending = Invoke(worker, "HoldVerificationAsync", text, "取消展示");
                    await Task.Delay(100);
                    File.WriteAllText(Path.Combine(directory, "cancel"), "stop");
                    bool holdCancelled = false;
                    try
                    {
                        await pending;
                    }
                    catch (OperationCanceledException)
                    {
                        holdCancelled = true;
                    }
                    Assert(holdCancelled, "停止请求必须取消验证展示，不能提前发出通过回执");
                    File.Delete(Path.Combine(directory, "cancel"));
                    var playback = new PlaybackControl(sharedDirectory);
                    playback.SetPaused(true);
                    Task synchronization = Invoke(worker, "WaitSignalAsync", "paused-peer", 1);
                    await Task.Delay(1200);
                    Assert(!synchronization.IsCompleted, "暂停时间不得消耗双账号同步超时");
                    File.WriteAllText(Path.Combine(sharedDirectory, "paused-peer.json"), "{}");
                    await synchronization;
                    pending = Invoke(worker, "BeginVerificationAsync", "单步验证");
                    await Task.Delay(100);
                    Assert(!pending.IsCompleted, "暂停时不得开始下一验证阶段");
                    playback.NextAction();
                    await pending;
                    pending = Invoke(worker, "HoldVerificationAsync", text, "单步验证结束");
                    await Task.Delay(50);
                    Assert(pending.IsCompleted && playback.IsPaused, "一个验证阶段只应领取一次单步许可");
                    await pending;
                    playback.SetPaused(false);
                    Environment.SetEnvironmentVariable("STC_UI_REALTIME_ROOT", null);

                    interval.Save(60000);
                    pending = Invoke(worker, "SetTextAsync", text, "must-not-change");
                    await Task.Delay(100);
                    File.WriteAllText(Path.Combine(directory, "cancel"), "stop");
                    bool cancelled = false;
                    try
                    {
                        await pending;
                    }
                    catch (OperationCanceledException)
                    {
                        cancelled = true;
                    }
                    Assert(cancelled && text.Text == "changed", "停止请求应取消未执行的输入");
                    timer.Stop();
                    result = 0;
                    Console.WriteLine("PASS: each click/input/selection waits before mutation; UI responsive; verification hold uses shared interval; live adjustment and cancellation.");
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                }
                app.Shutdown(result);
            };
            app.Run(window);
            return result;
        }

        private static async Task CheckDelay(Func<Task> action, Func<bool> changed)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            Task pending = action();
            await Task.Delay(50);
            Assert(!changed(), "操作在等待结束前执行了");
            await pending;
            Assert(changed() && elapsed.ElapsedMilliseconds >= 190, "每个操作都必须独立等待");
        }

        private static Task Invoke(object worker, string name, params object[] arguments)
        {
            return (Task)worker.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(worker, arguments);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
