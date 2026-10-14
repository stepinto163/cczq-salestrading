using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
                    await CheckDelay(() => Invoke(worker, "SetTextAsync", text, "changed"), () => text.Text == "changed");
                    await CheckDelay(() => Invoke(worker, "SelectItemAsync", selection, "second"), () => Equals(selection.SelectedItem, "second"));
                    Assert(heartbeat >= 10, "等待不能阻塞 WPF 消息循环");

                    interval.Save(60000);
                    Task pending = Invoke(worker, "ClickAsync", button);
                    await Task.Delay(100);
                    Assert(clicks == 1, "等待结束前不得点击");
                    interval.Save(0);
                    await pending;
                    Assert(clicks == 2, "实时调为0后应立即执行");

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
                    Console.WriteLine("PASS: each click/input/selection waits before mutation; UI responsive; live adjustment and cancellation.");
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
